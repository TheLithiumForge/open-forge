using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Sync.Shared.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Source;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Library.Sync;

internal sealed class LibrarySyncOperation
{
    private readonly LibraryPermissionOperation _permissions;

    internal LibrarySyncOperation(LibraryPermissionOperation permissions)
    {
        _permissions = permissions;
    }

    internal async ValueTask<LibrarySyncResult> ExecuteAsync(
        LibrarySyncRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            return LibrarySyncCompletion.Complete(
                await CollectExecutionAsync(request, cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LibrarySyncCompletion.Complete(new LibrarySyncCompletionInput
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
            return LibrarySyncCompletion.Complete(new LibrarySyncCompletionInput
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

    private async ValueTask<LibrarySyncCompletionInput> CollectExecutionAsync(
        LibrarySyncRequest request,
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
        var plan = LibrarySyncPlanner.Plan(observations, cancellationToken);
        if (plan.State != LibraryPlanState.Complete)
        {
            return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
        }
        var selected = observations.Record.Record?.Libraries.Single(library => library.Id == request.LibraryId)
            ?? throw new InvalidOperationException("A complete Library sync plan requires its selected Library.");
        var livePaths = (observations.Source.Inventory?.Entries ?? []).Select(entry => entry.SourcePath).ToHashSet();
        var targets = observations.Mappings.Select(observation => new LibraryPermissionTarget(
            observation.Mapping.DestinationPath.Value,
            livePaths.Contains(observation.Mapping.SourcePath) ? LibraryPermissionTargetUse.Live : LibraryPermissionTargetUse.Retired)
        {
            Effect = LibraryPermissionPresentation.ReadEffect(plan.Links.SingleOrDefault(link => link.DestinationPath.Value == observation.Mapping.DestinationPath.Value)?.Kind),
        }).ToImmutableArray();
        var permissions = await _permissions.DetermineAsync(new LibraryPermissionRequest
        {
            Workspace = request.Workspace,
            Library = selected,
            Targets = targets,
            AllowPrompt = request.AllowPrompt && request.Mode != LibraryMode.DryRun,
        }, cancellationToken).ConfigureAwait(false);
        plan = plan with
        {
            Permissions = permissions,
            State = permissions.Failure is null ? plan.State : LibraryPlanState.Blocked,
        };
        if (request.Mode == LibraryMode.DryRun || permissions.Failure is not null || !HasEffects(plan))
        {
            return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
        }

        return await ApplyAsync(request, plan, observations, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<LibrarySyncPlanningInput> ObservePreflightAsync(
        PhysicalPathResolver resolver,
        LibrarySyncRequest request,
        CancellationToken cancellationToken)
    {
        var record = await LibrariesRecordReader.ReadAsync(
            resolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var selected = record.Record?.Libraries.FirstOrDefault(library => library.Id == request.LibraryId);
        var source = selected is null
            ? NotObservedSource(request.Workspace)
            : await ReadSourceAsync(resolver, request.Workspace, selected.SourceRoot, cancellationToken).ConfigureAwait(false);
        var entries = source.Inventory?.Entries ?? [];
        var paths = (selected?.Paths ?? [])
            .Concat(entries.Select(entry => entry.SourcePath))
            .DistinctBy(path => path.Value, StringComparer.Ordinal)
            .OrderBy(path => path.Value, StringComparer.Ordinal)
            .ToArray();
        var mappings = selected is null
            ? []
            : LibraryMutationOperationSupport.ObserveMappings(
                resolver,
                new LibraryMappingSetRequest
                {
                    Workspace = request.Workspace,
                    SourceRoot = selected.SourceRoot,
                    DestinationRoot = selected.DestinationRoot,
                    Paths = [.. paths],
                },
                cancellationToken);
        var navigation = record.State == LibrariesRecordReadState.Complete
            && selected is not null
            && source.Inventory?.State == LibraryInventoryState.Complete
            ? await LibraryGeneratedNavigationReader.ReadAsync(
                new LibraryGeneratedNavigationRequest
                {
                    Workspace = request.Workspace,
                    SelectedLibrary = selected,
                    CurrentRecord = record.Record,
                    IntendedEntries = entries,
                },
                cancellationToken).ConfigureAwait(false)
            : new LibraryGeneratedNavigationRead([], Issue: null);
        var ownership = await new LifecycleOwnershipReader(resolver).ReadAsync(
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        return new LibrarySyncPlanningInput
        {
            Request = request,
            ConsumerBoundary = Unobserved(
                request.Workspace,
                LibraryMutationOperationSupport.ReadAncestors(
                    request.Workspace,
                    mappings.Select(observation => observation.Mapping.DestinationPath.Value),
                    navigation.Changes)),
            Record = record,
            Source = source,
            Mappings = mappings,
            Ownership = ownership,
            GeneratedRegionChanges = navigation.Changes,
            GeneratedNavigationIssue = navigation.Issue,
        };
    }

    private static async ValueTask<LibrarySyncCompletionInput> ApplyAsync(
        LibrarySyncRequest request,
        LibrarySyncPlan plan,
        LibrarySyncPlanningInput observations,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        var lockResult = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(request.Workspace, LibrarySyncDefinitions.CommandIdentity, operationId),
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
            var freshPlan = LibrarySyncPlanner.Plan(fresh, cancellationToken);
            var permissions = plan.Permissions
                ?? throw new InvalidOperationException("An admitted Library plan requires its permission observation.");
            if (freshPlan.State != LibraryPlanState.Complete || !Matches(plan, freshPlan)
                || !await LibraryPermissionOperation.RevalidateAsync(lease, permissions, cancellationToken).ConfigureAwait(false))
            {
                return Complete(request, plan with
                {
                    State = LibraryPlanState.Blocked,
                    Permissions = permissions with { Failure = LibraryPermissionFailure.Changed },
                }, observations, LibraryMutationOperationSupport.Empty());
            }

            var preparation = await LibraryMutationOperationSupport.PrepareRecoveryAsync(
                new LibraryRecoveryPreparationRequest
                {
                    Lease = lease,
                    Command = LibrarySyncDefinitions.CommandIdentity,
                    Operation = RecoveryBundleOperation.Sync,
                    Permissions = plan.Permissions,
                    Links = plan.Links,
                    GeneratedRegions = plan.GeneratedRegions,
                    RecordChange = plan.RecordChange,
                    Mappings = fresh.Mappings,
                    Record = fresh.Record,
                },
                cancellationToken).ConfigureAwait(false);
            if (preparation.State is not (RecoveryBundlePreparationState.Prepared
                or RecoveryBundlePreparationState.NotNeeded))
            {
                var evidence = LibraryMutationOperationSupport.Empty() with { RecoveryPreparationOutcome = preparation };
                return Complete(request, plan, observations, evidence);
            }

            var applied = await LibrarySyncApplication.ApplyAsync(
                new LibrarySyncApplicationInput
                {
                    Lease = lease,
                    Plan = plan,
                    RecoveryPreparation = preparation.Preparation,
                },
                cancellationToken).ConfigureAwait(false);
            var execution = applied with { RecoveryPreparationOutcome = preparation };
            if (execution.Cancellation is null && execution.UnexpectedFailure is null && execution.Permission?.Failure is null)
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

    private static async ValueTask<LibraryInventoryRead> ReadSourceAsync(
        PhysicalPathResolver resolver,
        CliWorkspace workspace,
        WorkspaceRelativeDirectory sourceRoot,
        CancellationToken cancellationToken)
    {
        var source = LibrarySourceRootReader.Read(
            resolver,
            new LibrarySourceRootRequest { Workspace = workspace, SourceRoot = sourceRoot },
            cancellationToken);
        return source.State == LibrarySourceRootState.Available
            ? await LibraryInventoryReader.ReadAsync(resolver, source, cancellationToken).ConfigureAwait(false)
            : new LibraryInventoryRead
            {
                Source = source,
                Inventory = null,
                ExcludedPaths = [],
                UnavailablePaths = [],
            };
    }

    private static LibraryInventoryRead NotObservedSource(CliWorkspace workspace)
        => new()
        {
            Source = new LibrarySourceRootObservation
            {
                Request = new LibrarySourceRootRequest
                {
                    Workspace = workspace,
                    SourceRoot = WorkspaceRelativeDirectory.Create("unselected-library"),
                },
                State = LibrarySourceRootState.Unavailable,
                LexicalSourceRoot = null,
                PhysicalSourceRoot = null,
                LexicallyContained = null,
                PhysicallyContained = null,
                Cause = "No registered Library was selected.",
            },
            Inventory = null,
            ExcludedPaths = [],
            UnavailablePaths = [],
        };

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

    private static bool Matches(LibrarySyncPlan expected, LibrarySyncPlan actual)
        => LibraryMutationOperationSupport.PlansMatch(expected.Effects, actual.Effects);

    private static bool HasEffects(LibrarySyncPlan plan)
        => plan.Directories.Length > 0
            || plan.Links.Length > 0
            || plan.GeneratedRegions.Length > 0
            || plan.RecordChange is not null
            || plan.Permissions?.Change is not null;

    private static LibrarySyncCompletionInput Complete(
        LibrarySyncRequest request,
        LibrarySyncPlan plan,
        LibrarySyncPlanningInput observations,
        LibraryExecutionEvidence execution)
        => new()
        {
            Request = request,
            Plan = plan,
            Observations = observations,
            Execution = execution,
        };
}
