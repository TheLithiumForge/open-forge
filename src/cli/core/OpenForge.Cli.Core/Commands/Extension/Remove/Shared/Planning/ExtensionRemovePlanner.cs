using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Application;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Ownership;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal sealed class ExtensionRemovePlanner(
    CliInteractiveSession interactiveSession,
    LifecycleStore lifecycleStore,
    LifecycleOwnershipReader ownershipReader,
    PhysicalPathResolver physicalPathResolver,
    RecoveryBundleCatalogue recoveryCatalogue)
{
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly LifecycleOwnershipReader _ownershipReader = ownershipReader;
    private readonly RecoveryBundleCatalogue _recoveryCatalogue = recoveryCatalogue;
    private readonly ExtensionRemoveSelectionResolver _selectionResolver = new(interactiveSession);
    private readonly ExtensionRemovePathInspector _pathInspector = new(physicalPathResolver);
    private readonly ExtensionRemoveTopologyBuilder _topologyBuilder = new();

    internal ValueTask<ExtensionRemovePlanBuild> BuildAsync(
        ExtensionRemoveRequest request,
        CancellationToken cancellationToken)
        => BuildAsync(request, allowedRecovery: null, cancellationToken);

    internal ValueTask<ExtensionRemovePlanBuild> BuildForVerificationAsync(
        ExtensionRemoveRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => BuildAsync(request, allowedRecovery, cancellationToken);

    private async ValueTask<ExtensionRemovePlanBuild> BuildAsync(
        ExtensionRemoveRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var lifecycle = await _lifecycleStore.ReadAsync(
            request.Workspace,
            LifecycleSection.Extensions,
            cancellationToken).ConfigureAwait(false);
        if (lifecycle.State != LifecycleStoreReadState.Available
            || lifecycle.Extensions is not { } current)
        {
            return Stop(
                request,
                ReadLifecycleFindingCode(lifecycle.State),
                lifecycle.Cause ?? "The Extension lifecycle is unavailable.");
        }

        var ownership = await _ownershipReader.ReadAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        if (ownership.Framework.State != LifecycleOwnershipReadState.Trusted
            || ownership.Extensions.State != LifecycleOwnershipReadState.Trusted)
        {
            var finding = ownership.Findings.FirstOrDefault();
            return Stop(
                request,
                ReadOwnershipFindingCode(finding?.Code),
                finding?.Cause
                    ?? ownership.Framework.Cause
                    ?? ownership.Extensions.Cause
                    ?? "Complete lifecycle ownership is unavailable.",
                target: finding?.Path);
        }

        if (ownership.LifecycleFileExpectation != lifecycle.File?.Expectation)
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.TargetChanged,
                "The lifecycle document changed between Extension and complete ownership inspection.");
        }

        var recovery = await ReadRecoveryAsync(request, allowedRecovery, cancellationToken)
            .ConfigureAwait(false);
        if (recovery.Boundary is not null)
        {
            return recovery.Boundary;
        }

        var selectionResult = await _selectionResolver.SelectAsync(request, current, cancellationToken)
            .ConfigureAwait(false);
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

        var dependencies = ExtensionRemoveDependencyPlanner.Build(current, selection.Ids);
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

        var installedIds = current.Packages.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        if (selection.Ids.All(id => !installedIds.Contains(id)))
        {
            return new ExtensionRemovePlanBuild
            {
                Plan = null,
                Result = ExtensionRemoveResultFactory.Create(
                    request,
                    selection,
                    dependencies,
                    [],
                    new ExtensionRemoveGeneratedNavigation([]),
                    [],
                    Lifecycle(
                        ExtensionRemoveLifecycleAction.Preserve,
                        ExtensionRemoveLifecycleOutcome.AlreadyCurrent),
                    new ExtensionRemoveRecovery(
                        ExtensionRemoveRecoveryState.NotRequired,
                        [],
                        residualPath: null),
                    Verified(),
                    []),
            };
        }

        var selected = selection.Ids.ToHashSet(StringComparer.Ordinal);
        var observations = new List<ExtensionRemovePathObservation>();
        foreach (var path in current.Paths.Where(path => path.Owners.Any(selected.Contains)))
        {
            var observation = await _pathInspector.ReadAsync(request, path, selected, cancellationToken)
                .ConfigureAwait(false);
            if (observation.Boundary is not null)
            {
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

        var policyResult = await _selectionResolver.ResolvePolicyAsync(
            request,
            observations.Any(value => value.Classification == ExtensionRemovePathClassification.ChangedFinalOwner),
            cancellationToken).ConfigureAwait(false);
        if (policyResult.Boundary is not null)
        {
            return policyResult.Boundary;
        }

        var policy = policyResult.Policy;
        var pathPlans = observations
            .Select(value => value.ToPathPlan(policy))
            .OrderBy(value => value.Path, StringComparer.Ordinal)
            .ToArray();
        var findings = new List<ExtensionRemoveFinding>();
        findings.AddRange(pathPlans
            .Where(path => path.Action == ExtensionRemovePathAction.KeepAsUnmanaged)
            .Select(path => new ExtensionRemoveFinding(
                ExtensionRemoveFindingCode.ManagedDivergence,
                "Changed final-owner content will remain as unmanaged workspace content.",
                path.Path)));
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
        var intended = ExtensionRemovePlanAssembler.CreateIntendedLifecycle(current, selected);
        var lifecyclePlan = _lifecycleStore.PlanExtensionUpdate(lifecycle, intended);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return Stop(
                request,
                ExtensionRemoveFindingCode.LifecycleBlocked,
                lifecyclePlan.Cause ?? "The intended Extension lifecycle cannot be published.",
                selection,
                dependencies);
        }

        var lifecycleChange = lifecyclePlan.Change;
        var lifecycleRecovery = lifecycleChange is null
            ? null
            : RecoveryBundleTarget.Create(
                lifecycleChange,
                lifecycle.File
                    ?? throw new InvalidOperationException(
                        "A lifecycle replacement requires the exact current lifecycle snapshot."));
        var resultEffects = effects.Select(effect => effect.Result).ToArray();
        var navigation = new ExtensionRemoveGeneratedNavigation(topologyBuild.Regions);
        var plan = ExtensionRemovePlan.Create(new ExtensionRemovePlanInput
        {
            Request = request,
            Selection = selection,
            Dependencies = dependencies,
            Planning = new ExtensionRemovePlanningPlan
            {
                Authority = new ExtensionRemovePlanningAuthority(policy),
                Decisions = pathPlans.Select(path => new ExtensionRemovePlanningDecision
                {
                    Path = path,
                    Disposition = ExtensionRemovePlanAssembler.ReadDisposition(path.Action),
                }).ToArray(),
            },
            Topology = topologyBuild.Topology,
            Effects = effects,
            LifecycleChange = lifecycleChange,
            LifecycleRecoveryTarget = lifecycleRecovery,
        });
        var hasRecovery = effects.Any(effect => effect.RecoveryTarget is not null)
            || lifecycleRecovery is not null;
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
                    lifecycleChange is null
                        ? ExtensionRemoveLifecycleAction.Preserve
                        : ExtensionRemoveLifecycleAction.Publish,
                    lifecycleChange is null
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

    private async ValueTask<ExtensionRemoveRecoveryRead> ReadRecoveryAsync(
        ExtensionRemoveRequest request,
        RecoveryBundlePreparation? allowed,
        CancellationToken cancellationToken)
    {
        RecoveryBundleCatalogueResult catalogue;
        try
        {
            catalogue = await _recoveryCatalogue.ReadAsync(request.Workspace, cancellationToken)
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

        if (allowed is null)
        {
            return catalogue.Candidates.Length == 0;
        }

        return catalogue.Candidates.Length == 1
            && ExtensionRemoveRecoveryApplication.Matches(catalogue.Candidates[0], allowed);
    }

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

    private static ExtensionRemoveFindingCode ReadLifecycleFindingCode(
        LifecycleStoreReadState state)
        => state switch
        {
            LifecycleStoreReadState.Cancelled => ExtensionRemoveFindingCode.Interrupted,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing
                or LifecycleStoreReadState.Unavailable => ExtensionRemoveFindingCode.LifecycleUnavailable,
            LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => ExtensionRemoveFindingCode.LifecycleBlocked,
            LifecycleStoreReadState.Available => throw new InvalidOperationException(
                "An available Extension lifecycle does not require a failure finding."),
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The lifecycle read state is not defined."),
        };

    private static ExtensionRemoveFindingCode ReadOwnershipFindingCode(
        LifecycleOwnershipFindingCode? code)
        => code switch
        {
            LifecycleOwnershipFindingCode.Interrupted => ExtensionRemoveFindingCode.Interrupted,
            LifecycleOwnershipFindingCode.LifecycleMissing
                or LifecycleOwnershipFindingCode.LifecycleUnavailable => ExtensionRemoveFindingCode.LifecycleUnavailable,
            LifecycleOwnershipFindingCode.CrossSectionCollision => ExtensionRemoveFindingCode.OwnershipConflict,
            LifecycleOwnershipFindingCode.FrameworkBlocked => ExtensionRemoveFindingCode.FrameworkUnsafe,
            LifecycleOwnershipFindingCode.LifecycleInvalid
                or LifecycleOwnershipFindingCode.ExtensionsBlocked
                or null => ExtensionRemoveFindingCode.LifecycleBlocked,
            _ => throw new ArgumentOutOfRangeException(nameof(code), code, "The lifecycle ownership finding is not defined."),
        };

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

internal sealed record ExtensionRemoveRecoveryRead(ExtensionRemovePlanBuild? Boundary);
