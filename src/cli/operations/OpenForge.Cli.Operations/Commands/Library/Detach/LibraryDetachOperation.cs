using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Completion;
using OpenForge.Cli.Core.Commands.Library.Detach.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Permissions;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Shared.Application;
using OpenForge.Cli.Core.Commands.Library.Shared.Permissions;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning;
using OpenForge.Cli.Core.Commands.Library.Shared.Planning.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Locking;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Library.Detach;

internal sealed class LibraryDetachOperation
{
    private readonly LibraryPermissionOperation _permissions;
    private readonly CliPlanConfirmation<LibraryDetachResult, LibraryDetachPlan> _confirmation;

    internal LibraryDetachOperation(
        LibraryPermissionOperation permissions,
        CliPlanConfirmation<LibraryDetachResult, LibraryDetachPlan>? confirmation = null)
    {
        _permissions = permissions;
        _confirmation = confirmation ?? MissingConfirmation;
    }

    internal async ValueTask<LibraryDetachResult> ExecuteAsync(
        LibraryDetachRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        try
        {
            return LibraryDetachCompletion.Complete(
                await CollectExecutionAsync(request, cancellationToken).ConfigureAwait(false));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return LibraryDetachCompletion.Complete(new LibraryDetachCompletionInput
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
            return LibraryDetachCompletion.Complete(new LibraryDetachCompletionInput
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

    private async ValueTask<LibraryDetachCompletionInput> CollectExecutionAsync(
        LibraryDetachRequest request,
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
        var plan = LibraryDetachPlanner.Plan(observations, cancellationToken);
        if (plan.State != LibraryPlanState.Complete || observations.Record.OwnershipObservation is not null)
        {
            return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
        }
        var selected = observations.Record.Record?.Libraries.Single(library => library.Id == request.LibraryId)
            ?? throw new InvalidOperationException("A complete Library detach plan requires its selected Library.");
        var targets = LibraryPathIdentity.Mappings(selected).Select(mapping =>
            new LibraryPermissionTarget(mapping.DestinationPath.Value, LibraryPermissionTargetUse.Retired) { Effect = LibraryPermissionEffect.RemoveLink }).ToImmutableArray();
        var permissions = await _permissions.DetermineAsync(new LibraryPermissionRequest
        {
            Workspace = request.Workspace,
            Library = selected,
            Targets = targets,
            ExplicitGrantPaths = request.Allow,
            AllowPrompt = request.AllowPrompt && !request.Automatic && request.Mode != LibraryMode.DryRun,
        }, cancellationToken).ConfigureAwait(false);
        plan = plan with
        {
            Permissions = permissions,
            State = permissions.Failure is null ? plan.State : LibraryPlanState.Blocked,
        };
        if (request.Mode == LibraryMode.DryRun || permissions.Failure is not null)
        {
            return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
        }

        if (!request.Automatic)
        {
            var preview = LibraryDetachCompletion.Complete(
                Complete(request, plan, observations, LibraryMutationOperationSupport.Empty()),
                preview: true);
            var confirmation = await _confirmation(
                preview,
                plan,
                new CliPromptPolicy(request.AllowPrompt),
                cancellationToken).ConfigureAwait(false);
            if (confirmation.State == CliPromptState.Unavailable)
            {
                plan = plan with
                {
                    Findings = plan.Findings.Add(new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.ConfirmationRequired,
                        Status = CliSemanticStatus.Invalid,
                        LibraryId = request.LibraryId.Value,
                        Path = null,
                        Cause = "Library detach needs confirmation, and this session cannot ask.",
                        OccupantKind = null,
                    }),
                };
                return Complete(request, plan, observations, LibraryMutationOperationSupport.Empty());
            }
            if (confirmation.State == CliPromptState.Cancelled || !confirmation.Value)
            {
                return Complete(
                    request,
                    plan,
                    observations,
                    LibraryMutationOperationSupport.Empty(new LibraryCancellationFact(LibraryExecutionStage.Preflight)));
            }
        }

        return await ApplyAsync(request, plan, observations, cancellationToken).ConfigureAwait(false);
    }

    private static async ValueTask<LibraryDetachPlanningInput> ObservePreflightAsync(
        PhysicalPathResolver resolver,
        LibraryDetachRequest request,
        CancellationToken cancellationToken)
    {
        var ownership = await WorkspaceOwnershipReader.ReadAsync(
            resolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        var record = LibraryRegistrationReader.Read(ownership);
        var selected = record.Record?.Libraries.FirstOrDefault(library => library.Id == request.LibraryId);
        var mappings = selected is null
            ? []
            : LibraryMutationOperationSupport.ObserveMappings(
                resolver,
                new LibraryMappingSetRequest
                {
                    Workspace = request.Workspace,
                    SourceRoot = selected.SourceRoot,
                    DestinationRoot = selected.DestinationRoot,
                    Paths = [.. selected.Paths],
                },
                cancellationToken);
        var navigation = record.State == LibraryRegistrationReadState.Complete && selected is not null
            ? await LibraryGeneratedNavigationReader.ReadAsync(
                new LibraryGeneratedNavigationRequest
                {
                    Workspace = request.Workspace,
                    SelectedLibrary = selected,
                    CurrentRecord = record.Record,
                    IntendedEntries = [],
                },
                cancellationToken).ConfigureAwait(false)
            : new LibraryGeneratedNavigationRead([], Issue: null);

        var paths = mappings.Select(observation => observation.Mapping.DestinationPath.Value);
        return new LibraryDetachPlanningInput
        {
            Request = request,
            ConsumerBoundary = Unobserved(
                request.Workspace,
                LibraryMutationOperationSupport.ReadAncestors(
                    request.Workspace,
                    paths,
                    navigation.Changes)),
            Record = record,
            Mappings = mappings,
            Ownership = ownership,
            GeneratedRegionChanges = navigation.Changes,
            GeneratedNavigationIssue = navigation.Issue,
        };
    }

    private static async ValueTask<LibraryDetachCompletionInput> ApplyAsync(
        LibraryDetachRequest request,
        LibraryDetachPlan plan,
        LibraryDetachPlanningInput observations,
        CancellationToken cancellationToken)
    {
        var operationId = Guid.NewGuid();
        var lockResult = await WorkspaceLockManager.CreateForCurrentUser().AcquireAsync(
            new WorkspaceLockRequest(request.Workspace, LibraryDetachDefinitions.CommandIdentity, operationId),
            cancellationToken).ConfigureAwait(false);
        if (lockResult.State != WorkspaceLockState.Acquired || lockResult.Lease is not { } lease)
        {
            var evidence = lockResult.State == WorkspaceLockState.Cancelled
                ? LibraryMutationOperationSupport.Empty(new LibraryCancellationFact(LibraryExecutionStage.Preflight))
                : LibraryMutationOperationSupport.Empty();
            if (lockResult.State != WorkspaceLockState.Cancelled)
            {
                plan = plan with
                {
                    State = LibraryPlanState.Blocked,
                    Findings = plan.Findings.Add(new LibraryDetachFinding
                    {
                        Code = LibraryDetachFindingCode.LockUnavailable,
                        Status = CliSemanticStatus.Blocked,
                        LibraryId = request.LibraryId.Value,
                        Path = null,
                        Cause = lockResult.Cause ?? "The Library workspace lock is unavailable.",
                        OccupantKind = null,
                    }),
                };
            }
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
            var freshPlan = LibraryDetachPlanner.Plan(fresh, cancellationToken);
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
                    Command = LibraryDetachDefinitions.CommandIdentity,
                    Operation = RecoveryBundleOperation.Detach,
                    Permissions = plan.Permissions,
                    Links = plan.Links,
                    GeneratedRegions = plan.GeneratedRegions,
                    Ownership = fresh.Ownership,
                    OwnershipChange = plan.OwnershipChange,
                    Mappings = fresh.Mappings,
                },
                cancellationToken).ConfigureAwait(false);
            if (preparation.State is not (RecoveryBundlePreparationState.Prepared
                or RecoveryBundlePreparationState.NotNeeded))
            {
                var evidence = LibraryMutationOperationSupport.Empty() with { RecoveryPreparationOutcome = preparation };
                return Complete(request, plan, observations, evidence);
            }

            var applied = await LibraryDetachApplication.ApplyAsync(
                new LibraryDetachApplicationInput
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

    private static bool Matches(LibraryDetachPlan expected, LibraryDetachPlan actual)
        => LibraryMutationOperationSupport.PlansMatch(expected.Effects, actual.Effects);

    private static LibraryDetachCompletionInput Complete(
        LibraryDetachRequest request,
        LibraryDetachPlan plan,
        LibraryDetachPlanningInput observations,
        LibraryExecutionEvidence execution)
        => new()
        {
            Request = request,
            Plan = plan,
            Observations = observations,
            Execution = execution,
        };

    private static ValueTask<CliPromptReply<bool>> MissingConfirmation(
        LibraryDetachResult _,
        LibraryDetachPlan __,
        CliPromptPolicy _policy,
        CancellationToken ____)
        => ValueTask.FromResult(CliPromptReply<bool>.Unavailable());
}
