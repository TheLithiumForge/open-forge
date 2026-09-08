using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Attach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Attach;

internal static class LibraryAttachOperation
{
    internal static async ValueTask<LibraryAttachResult> ExecuteAsync(
        LibraryAttachRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            return LibraryAttachCompletion.Complete(
                await CollectExecutionAsync(request, cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LibraryAttachCompletion.Complete(new LibraryAttachCompletionInput
            {
                Request = request,
                Plan = null,
                Observations = null,
                Execution = LibraryMutationOperationSupport.Empty(
                    new LibraryCancellationFact(LibraryExecutionStage.Preflight)),
            });
        }
        catch (Exception exception) when (exception is IOException
            or InvalidOperationException
            or UnauthorizedAccessException)
        {
            return LibraryAttachCompletion.Complete(new LibraryAttachCompletionInput
            {
                Request = request,
                Plan = null,
                Observations = null,
                Execution = LibraryMutationOperationSupport.Empty(
                    unexpected: new LibraryUnexpectedFailureFact(
                        LibraryExecutionStage.Preflight,
                        exception.Message)),
            });
        }
    }

    private static async ValueTask<LibraryAttachCompletionInput> CollectExecutionAsync(
        LibraryAttachRequest request,
        CancellationToken cancellationToken)
    {
        var resolver = new PhysicalPathResolver();
        var observations = await ObservePreflightAsync(resolver, request, cancellationToken).ConfigureAwait(false);
        observations = observations with
        {
            ConsumerBoundary = await LibraryConsumerBoundaryReader.ReadAsync(
                resolver,
                observations.ConsumerBoundary.Request,
                cancellationToken).ConfigureAwait(false),
        };
        var plan = LibraryAttachPlanner.Plan(observations, cancellationToken);
        if (request.Mode == LibraryMode.DryRun || plan.State != LibraryPlanState.Complete)
        {
            return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
        }

        return await ApplyAsync(request, plan, observations, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<LibraryAttachPlanningInput> ObservePreflightAsync(
        PhysicalPathResolver resolver,
        LibraryAttachRequest request,
        CancellationToken cancellationToken)
    {
        var record = await LibrariesRecordReader.ReadAsync(
            resolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var sourceObservation = LibrarySourceRootReader.Read(
            resolver,
            new LibrarySourceRootRequest
            {
                Workspace = request.Workspace,
                SourceRoot = request.SourceRoot,
            },
            cancellationToken);
        var source = sourceObservation.State == LibrarySourceRootState.Available
            ? await LibraryInventoryReader.ReadAsync(resolver, sourceObservation, cancellationToken).ConfigureAwait(false)
            : new LibraryInventoryRead
            {
                Source = sourceObservation,
                Inventory = null,
                ExcludedPaths = [],
                UnavailablePaths = [],
            };
        var entries = source.Inventory?.Entries ?? [];
        var mappings = LibraryMutationOperationSupport.ObserveMappings(
            resolver,
            request.Workspace,
            request.SourceRoot,
            entries.Select(entry => entry.SourcePath),
            cancellationToken);
        var navigation = record.State is (LibrariesRecordReadState.Missing or LibrariesRecordReadState.Complete)
            && source.Inventory?.State == LibraryInventoryState.Complete
            ? await LibraryGeneratedNavigationReader.ReadAsync(
                request.Workspace,
                request.LibraryId,
                record.Record,
                entries,
                cancellationToken).ConfigureAwait(false)
            : new LibraryGeneratedNavigationRead([], Issue: null);
        var ownership = await new LifecycleOwnershipReader(resolver).ReadAsync(
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var ancestors = LibraryMutationOperationSupport.ReadAncestors(
            request.Workspace,
            entries.Select(entry => entry.SourcePath.Value),
            navigation.Changes);
        return new LibraryAttachPlanningInput
        {
            Request = request,
            ConsumerBoundary = Unobserved(request.Workspace, ancestors),
            Record = record,
            Source = source,
            Mappings = mappings,
            Ownership = ownership,
            GeneratedRegionChanges = navigation.Changes,
            GeneratedNavigationIssue = navigation.Issue,
        };
    }

    private static async ValueTask<LibraryAttachCompletionInput> ApplyAsync(
        LibraryAttachRequest request,
        LibraryAttachPlan plan,
        LibraryAttachPlanningInput observations,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        var lockResult = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(request.Workspace, LibraryAttachDefinitions.CommandIdentity, operationId),
            cancellationToken).ConfigureAwait(false);
        if (lockResult.State != WorkspaceLockState.Acquired || lockResult.Lease is not { } lease)
        {
            var evidence = lockResult.State == WorkspaceLockState.Cancelled
                ? LibraryMutationOperationSupport.Empty(new LibraryCancellationFact(LibraryExecutionStage.Preflight))
                : LibraryMutationOperationSupport.Empty(unexpected: new LibraryUnexpectedFailureFact(
                    LibraryExecutionStage.Preflight,
                    lockResult.Cause ?? "The Library workspace lock is unavailable."));
            return Complete(request, plan, observations, evidence);
        }

        await using (lease.ConfigureAwait(false))
        {
            var freshResolver = new PhysicalPathResolver();
            var fresh = await ObservePreflightAsync(freshResolver, request, cancellationToken).ConfigureAwait(false);
            fresh = fresh with
            {
                ConsumerBoundary = await LibraryConsumerBoundaryReader.ReadAsync(
                    freshResolver,
                    fresh.ConsumerBoundary.Request,
                    cancellationToken).ConfigureAwait(false),
            };
            var freshPlan = LibraryAttachPlanner.Plan(fresh, cancellationToken);
            if (freshPlan.State != LibraryPlanState.Complete || !Matches(plan, freshPlan))
            {
                return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty(
                    unexpected: new LibraryUnexpectedFailureFact(
                        LibraryExecutionStage.Preflight,
                        "The complete Library attach plan changed under the held workspace lease.")));
            }

            var preparation = await LibraryMutationOperationSupport.PrepareRecoveryAsync(
                lease,
                LibraryAttachDefinitions.CommandIdentity,
                RecoveryBundleOperation.Attach,
                plan.Links,
                plan.GeneratedRegions,
                plan.RecordChange,
                fresh.Mappings,
                fresh.Record,
                cancellationToken).ConfigureAwait(false);
            if (preparation.State is not (RecoveryBundlePreparationState.Prepared
                or RecoveryBundlePreparationState.NotNeeded))
            {
                var evidence = preparation.State == RecoveryBundlePreparationState.Cancelled
                    ? LibraryMutationOperationSupport.Empty(new LibraryCancellationFact(LibraryExecutionStage.RecoveryPreparation))
                    : LibraryMutationOperationSupport.Empty(unexpected: new LibraryUnexpectedFailureFact(
                        LibraryExecutionStage.RecoveryPreparation,
                        preparation.Cause ?? "Library recovery preparation is unavailable."));
                return Complete(request, plan, observations, evidence);
            }

            var outcome = await LibraryAttachApplication.ApplyAsync(
                new LibraryAttachApplicationInput
                {
                    Lease = lease,
                    Plan = plan,
                    RecoveryPreparation = preparation.Preparation,
                },
                cancellationToken).ConfigureAwait(false);
            var execution = outcome.Execution;
            if (execution.Cancellation is null && execution.UnexpectedFailure is null)
            {
                execution = LibraryMutationOperationSupport.WithCleanup(
                    execution,
                    await LibraryMutationOperationSupport.CleanupRecoveryAsync(
                        lease,
                        preparation.Preparation,
                        cancellationToken).ConfigureAwait(false));
            }

            return Complete(request, plan, observations, execution);
        }
    }

    private static LibraryConsumerBoundaryFacts Unobserved(
        CliWorkspace workspace,
        ImmutableArray<CanonicalRelativePath> ancestors)
        => new()
        {
            Request = new LibraryConsumerBoundaryRequest
            {
                Workspace = workspace,
                RequiredAncestorPaths = ancestors,
            },
            ConsumerRoot = null,
            Ancestors = [],
            IsComplete = false,
        };

    private static bool Matches(LibraryAttachPlan expected, LibraryAttachPlan actual)
        => LibraryMutationOperationSupport.PlansMatch(
            expected.Directories,
            expected.Links,
            expected.GeneratedRegions,
            expected.RecordChange,
            actual.Directories,
            actual.Links,
            actual.GeneratedRegions,
            actual.RecordChange);

    private static LibraryAttachCompletionInput Complete(
        LibraryAttachRequest request,
        LibraryAttachPlan plan,
        LibraryAttachPlanningInput observations,
        LibraryExecutionEvidence execution)
        => new()
        {
            Request = request,
            Plan = plan,
            Observations = observations,
            Execution = execution,
        };
}
