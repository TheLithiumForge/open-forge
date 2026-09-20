using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemovePlanner
{
    private readonly ExtensionRemoveSelectionResolver _selectionResolver;
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly ExtensionRemovePathInspector _pathInspector;
    private readonly ExtensionRemoveTopologyBuilder _topologyBuilder = new();
    private readonly WorkspaceOwnershipStore _ownershipStore = new();

    internal ExtensionRemovePlanner(
        CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> selectionPrompt,
        string selectionQuestion,
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(selectionPrompt);
        _physicalPathResolver = physicalPathResolver;
        _selectionResolver = new ExtensionRemoveSelectionResolver(selectionPrompt, selectionQuestion);
        _pathInspector = new ExtensionRemovePathInspector(physicalPathResolver);
    }

    internal ValueTask<ExtensionRemovePlanBuild> BuildAsync(
        ExtensionRemoveRequest request,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection: null, allowedRecovery: null, cancellationToken);

    internal ValueTask<ExtensionRemovePlanBuild> BuildAsync(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection frozenSelection,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection, allowedRecovery: null, cancellationToken);

    internal ValueTask<ExtensionRemovePlanBuild> BuildForVerificationAsync(
        ExtensionRemoveRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection: null, allowedRecovery, cancellationToken);

    internal ValueTask<ExtensionRemovePlanBuild> BuildForVerificationAsync(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection frozenSelection,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => BuildAsync(request, frozenSelection, allowedRecovery, cancellationToken);

    private async ValueTask<ExtensionRemovePlanBuild> BuildAsync(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection? frozenSelection,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var workspaceOwnership = await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);

        var current = workspaceOwnership.Document.Extensions;

        var selectionResult = frozenSelection is { } frozen
            ? new ExtensionRemoveSelectionRead(frozen, Boundary: null)
            : await _selectionResolver.SelectAsync(request, current, cancellationToken).ConfigureAwait(false);
        if (selectionResult.Boundary is not null)
        {
            return selectionResult.Boundary;
        }

        var selection = selectionResult.Selection
            ?? throw new InvalidOperationException("A complete Extension Remove selection is required.");
        if (selection.Ids.Any(id => !ExtensionIdentity.IsValidStableId(id)))
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.InvalidInput,
                "Extension Remove stable IDs must use the canonical Extension identity grammar.",
                selection);
        }

        if (selection.Ids.Count > 0
            && (workspaceOwnership.State == WorkspaceOwnershipReadState.Invalid
                || workspaceOwnership.State == WorkspaceOwnershipReadState.Unavailable))
        {
            return UnknownOwnershipBoundary(
                request,
                selection,
                workspaceOwnership.Cause
                    ?? "The Extension ownership record is unavailable; no files were deleted.");
        }

        ExtensionRemoveDependencyPlan dependencies;
        try
        {
            dependencies = ExtensionRemoveDependencyPlanner.Build(current, selection.Ids);
        }
        catch (Exception exception) when (exception is ArgumentException or InvalidOperationException)
        {
            return UnknownOwnershipBoundary(
                request,
                selection,
                "The recorded Extension dependencies could not be interpreted; no files were deleted.");
        }
        if (dependencies.RetainedDependentBlockers.Count > 0)
        {
            var blocker = dependencies.RetainedDependentBlockers[0];
            return Stop(
                request,
                ExtensionRemoveFindingCode.DependencyBlocked,
                $"Retained Extension '{blocker.RetainedDependentIds[0]}' depends on selected package '{blocker.DependencyId}'.",
                selection,
                dependencies,
                blocker.DependencyId);
        }

        var installedIds = current.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        if (selection.Ids.All(id => !installedIds.Contains(id)))
        {
            IReadOnlyList<ExtensionRemoveFinding> lockFindings = workspaceOwnership.IsTrustworthy
                ? Array.Empty<ExtensionRemoveFinding>()
                : [new ExtensionRemoveFinding(
                    ExtensionRemoveFindingCode.OwnershipObservation,
                    workspaceOwnership.Cause ?? "No Extension ownership is recorded; no files were deleted.")];
            var result = ExtensionRemoveResultFactory.Create(
                request,
                selection,
                dependencies,
                [],
                new ExtensionRemoveGeneratedNavigation([]),
                [],
                workspaceOwnership.IsTrustworthy
                    ? Lifecycle(
                        ExtensionRemoveLifecycleAction.Preserve,
                        ExtensionRemoveLifecycleOutcome.AlreadyCurrent)
                    : new ExtensionRemoveLifecycle(
                        ExtensionRemoveLifecycleTrust.Unavailable,
                        ExtensionRemoveLifecycleCoverage.Incomplete,
                        ExtensionRemoveLifecycleAction.Preserve,
                        ExtensionRemoveLifecycleOutcome.NotRequested),
                new ExtensionRemoveRecovery(
                    ExtensionRemoveRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Verified(),
                lockFindings);
            if (request.AllowPath.IsEmpty)
            {
                return new ExtensionRemovePlanBuild
                {
                    Plan = null,
                    Result = result,
                };
            }

            var permissionOnlyPlan = ExtensionRemovePlan.Create(new ExtensionRemovePlanInput
            {
                Request = request,
                Selection = selection,
                Dependencies = dependencies,
                Planning = new ExtensionRemovePlanningPlan
                {
                    Decisions = [],
                },
                Topology = new ExtensionRemoveTopology
                {
                    IntendedTargetBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal),
                    GeneratedEntries = new Dictionary<string, IReadOnlyList<GeneratedNavigationEntry>>(StringComparer.Ordinal),
                    ProtectedPaths = new HashSet<string>(StringComparer.Ordinal),
                },
                Effects = [],
                OwnershipChange = null,
                OwnershipRecoveryTarget = null,
            });
            return new ExtensionRemovePlanBuild
            {
                Plan = permissionOnlyPlan,
                Result = result,
            };
        }

        var recovery = await ReadRecoveryAsync(request, allowedRecovery, cancellationToken)
            .ConfigureAwait(false);
        if (recovery.Boundary is not null)
        {
            return recovery.Boundary;
        }

        var selected = selection.Ids.ToHashSet(StringComparer.Ordinal);
        var observations = new List<ExtensionRemovePathObservation>();
        foreach (var path in current.Where(extension => selected.Contains(extension.Id))
            .SelectMany(extension => extension.Paths).Distinct(StringComparer.Ordinal))
        {
            var observation = await _pathInspector.ReadAsync(request, path, workspaceOwnership.Document, selected, cancellationToken)
                .ConfigureAwait(false);
            if (observation.Boundary is not null)
            {
                if (observation.Boundary.Code == ExtensionRemoveFindingCode.OwnershipObservation
                    && observation.LibraryBoundary is null)
                {
                    return UnknownOwnershipBoundary(
                        request,
                        selection,
                        observation.Boundary.Cause);
                }

                return Stop(
                    request,
                    observation.Boundary.Code,
                    observation.Boundary.Cause,
                    selection,
                    dependencies,
                    observation.Boundary.Target);
            }

            observations.Add(observation);
        }

        var pathPlans = observations
            .Select(value => value.ToPathPlan())
            .OrderBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
        var findings = new List<ExtensionRemoveFinding>();
        findings.AddRange(dependencies.RetainedOrphanDependencyIds.Select(id =>
            new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.LifecycleObservation,
                "A dependency becomes unreferenced but remains installed.",
                id)));

        ExtensionRemoveTopologyBuild topologyBuild;
        try
        {
            topologyBuild = await _topologyBuilder.BuildAsync(
                request.Workspace,
                pathPlans
                    .Where(path => path.Action is ExtensionRemovePathAction.Delete
                        or ExtensionRemovePathAction.ReleaseOwnership)
                    .Select(path => path.Path)
                    .ToHashSet(StringComparer.Ordinal),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove topology planning was interrupted.",
                selection,
                dependencies);
        }
        catch (ExtensionRemoveTopologyBlockedException exception)
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.GeneratedRegionUnsafe,
                exception.Message,
                selection,
                dependencies);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or IOException)
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.ProjectionUnavailable,
                exception.Message,
                selection,
                dependencies);
        }

        var effects = ExtensionRemovePlanAssembler.BuildEffects(observations, pathPlans, topologyBuild);
        var ownershipPlan = _ownershipStore.PlanExtensionOwnership(
            workspaceOwnership,
            [.. current.Where(extension => !selected.Contains(extension.Id))]);
        RecoveryBundleTarget? ownershipRecovery = null;
        if (ownershipPlan.Change is { } ownershipChange)
        {
            ownershipRecovery = RecoveryBundleTarget.Create(
                ownershipChange,
                workspaceOwnership.Snapshot
                    ?? throw new InvalidOperationException(
                        "An Extension ownership write plan requires exact prior file facts."));
        }
        var resultEffects = effects.Select(effect => effect.Result).ToArray();
        var navigation = new ExtensionRemoveGeneratedNavigation(topologyBuild.Regions);
        var plan = ExtensionRemovePlan.Create(new ExtensionRemovePlanInput
        {
            Request = request,
            Selection = selection,
            Dependencies = dependencies,
            Planning = new ExtensionRemovePlanningPlan
            {
                Decisions = [.. pathPlans.Select(path => new ExtensionRemovePlanningDecision
                {
                    Path = path,
                    Disposition = ExtensionRemovePlanAssembler.ReadDisposition(path.Action),
                })],
            },
            Topology = topologyBuild.Topology,
            Effects = effects,
            OwnershipChange = ownershipPlan.Change,
            OwnershipRecoveryTarget = ownershipRecovery,
        });
        var hasRecovery = effects.Any(effect => effect.RecoveryTarget is not null)
            || ownershipRecovery is not null;
        return new ExtensionRemovePlanBuild
        {
            Plan = plan,
            Result = ExtensionRemoveResultFactory.Create(
                request,
                selection,
                dependencies,
                pathPlans,
                navigation,
                resultEffects,
                Lifecycle(
                    ownershipPlan.Change is null
                        ? ExtensionRemoveLifecycleAction.Preserve
                        : ExtensionRemoveLifecycleAction.Publish,
                    ownershipPlan.Change is null
                        ? ExtensionRemoveLifecycleOutcome.AlreadyCurrent
                        : ExtensionRemoveLifecycleOutcome.Planned),
                new ExtensionRemoveRecovery(
                    hasRecovery
                        ? ExtensionRemoveRecoveryState.NotCreated
                        : ExtensionRemoveRecoveryState.NotRequired,
                    [],
                    residualPath: null),
                Planned(),
                findings),
        };
    }

    private static ExtensionRemovePlanBuild UnknownOwnershipBoundary(
        ExtensionRemoveRequest request,
        ExtensionRemoveSelection selection,
        string cause)
    {
        var result = ExtensionRemoveResultFactory.Create(
            request,
            selection,
            dependencies: null,
            [],
            new ExtensionRemoveGeneratedNavigation([]),
            [],
            new ExtensionRemoveLifecycle(
                ExtensionRemoveLifecycleTrust.Unavailable,
                ExtensionRemoveLifecycleCoverage.Incomplete,
                ExtensionRemoveLifecycleAction.Preserve,
                ExtensionRemoveLifecycleOutcome.NotRequested),
            new ExtensionRemoveRecovery(
                ExtensionRemoveRecoveryState.NotRequired,
                [],
                residualPath: null),
            Verified(),
            [new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.LifecycleUnavailable,
                cause)]);
        return new ExtensionRemovePlanBuild
        {
            Plan = null,
            Result = result,
        };
    }

    private static async ValueTask<ExtensionRemoveRecoveryRead> ReadRecoveryAsync(
        ExtensionRemoveRequest request,
        RecoveryBundlePreparation? allowed,
        CancellationToken cancellationToken)
    {
        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await RecoveryBundleCatalogue.ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new ExtensionRemoveRecoveryRead(Stop(
                request,
                ExtensionRemoveFindingCode.Interrupted,
                "Extension Remove recovery inspection was interrupted."));
        }
        catch (Exception)
        {
            return new ExtensionRemoveRecoveryRead(Stop(
                request,
                ExtensionRemoveFindingCode.RecoveryUnavailable,
                "Extension Remove recovery inspection failed unexpectedly."));
        }

        if (IsExpectedRecovery(catalogue, allowed))
        {
            return new ExtensionRemoveRecoveryRead(Boundary: null);
        }

        return new ExtensionRemoveRecoveryRead(Stop(
            request,
            ReadRecoveryFindingCode(catalogue.State),
            catalogue.Cause ?? "Recognized recovery residuals block Extension Remove."));
    }

    private static bool IsExpectedRecovery(
        RecoveryBundleCatalogueResult catalogue,
        RecoveryBundlePreparation? allowed)
    {
        if (catalogue.State != RecoveryBundleCatalogueState.Available)
        {
            return false;
        }

        if (!catalogue.Candidates.All(IsVerifiedFinal))
        {
            return false;
        }

        if (allowed is null)
        {
            return true;
        }

        return catalogue.Candidates.Count(candidate => RecoveryBundleIdentity.Matches(candidate, allowed)) == 1;
    }

    private static bool IsVerifiedFinal(RecoveryBundleCandidateSnapshot candidate)
        => candidate.Kind == RecoveryBundleCandidateKind.Final
            && candidate.Integrity == RecoveryBundleIntegrity.Verified
            && candidate.Verified is not null;

    private static ExtensionRemoveFindingCode ReadRecoveryFindingCode(
        RecoveryBundleCatalogueState state)
        => state switch
        {
            RecoveryBundleCatalogueState.Cancelled => ExtensionRemoveFindingCode.Interrupted,
            RecoveryBundleCatalogueState.Available => ExtensionRemoveFindingCode.RecoveryConflict,
            RecoveryBundleCatalogueState.Unavailable => ExtensionRemoveFindingCode.RecoveryUnavailable,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The recovery catalogue state is not defined."),
        };

    private static ExtensionRemoveLifecycle Lifecycle(
        ExtensionRemoveLifecycleAction action,
        ExtensionRemoveLifecycleOutcome outcome)
        => new(
            ExtensionRemoveLifecycleTrust.Trusted,
            ExtensionRemoveLifecycleCoverage.Complete,
            action,
            outcome);

    private static ExtensionRemoveVerification Planned()
        => new(
            ExtensionRemoveVerificationState.Planned,
            ExtensionRemoveVerificationState.Planned,
            ExtensionRemoveVerificationState.Planned);

    private static ExtensionRemoveVerification Verified()
        => new(
            ExtensionRemoveVerificationState.Verified,
            ExtensionRemoveVerificationState.Verified,
            ExtensionRemoveVerificationState.Verified);

    internal static ExtensionRemovePlanBuild Stop(
        ExtensionRemoveRequest request,
        ExtensionRemoveFindingCode code,
        string cause,
        ExtensionRemoveSelection? selection = null,
        ExtensionRemoveDependencyPlan? dependencies = null,
        string? target = null)
        => new()
        {
            Plan = null,
            Result = ExtensionRemoveResultFactory.CreateBoundary(
                request,
                selection,
                dependencies,
                new ExtensionRemoveFinding(code, cause, target)),
        };
}
