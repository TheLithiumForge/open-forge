using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallPlanner
{
    private readonly ExtensionInstallSourceResolver _sourceResolver;
    private readonly ExtensionInstallSelectionResolver _selectionResolver;
    private readonly ExtensionInstallFoundationReader _foundationReader;
    private readonly ExtensionInstallTargetInspector _targetInspector;
    private readonly ExtensionInstallEffectPlanner _effectPlanner;

    internal ExtensionInstallPlanner(
        CliInteractiveSession interactiveSession,
        PhysicalPathResolver physicalPathResolver,
        LifecycleStore lifecycleStore,
        RecoveryBundleCatalogue recoveryCatalogue)
    {
        var validator = new FileExpectationValidator(physicalPathResolver);
        _sourceResolver = new ExtensionInstallSourceResolver(physicalPathResolver);
        _selectionResolver = new ExtensionInstallSelectionResolver(
            interactiveSession,
            new ExtensionInstallDependencyClosureResolver(),
            new ExtensionInstallPayloadNormalizer());
        _foundationReader = new ExtensionInstallFoundationReader(
            lifecycleStore,
            new FrameworkLifecycleCurrentnessReader(physicalPathResolver),
            recoveryCatalogue);
        _targetInspector = new ExtensionInstallTargetInspector(interactiveSession, validator);
        _effectPlanner = new ExtensionInstallEffectPlanner(lifecycleStore, validator);
    }

    internal async ValueTask<ExtensionInstallPlanBuild> BuildAsync(
        ExtensionInstallRequest request,
        CancellationToken cancellationToken)
    {
        var sourceResolution = await _sourceResolver.ReadAsync(request, cancellationToken)
            .ConfigureAwait(false);
        var source = sourceResolution.Source;
        var sourceBoundary = ExtensionInstallResultFactory.SourceBoundary(request, source);
        if (sourceBoundary is not null)
        {
            return Stop(sourceBoundary);
        }

        var sourceFact = ExtensionInstallResultFactory.Source(source);
        ExtensionInstallSelectionResolution resolution;
        try
        {
            resolution = await _selectionResolver.ResolveAsync(request, sourceResolution, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(Boundary(
                request,
                ExtensionInstallResultFactory.Evidence(sourceFact),
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install selection was interrupted."));
        }

        var selectionEvidence = ExtensionInstallResultFactory.Evidence(
            sourceFact,
            resolution.Selection,
            PackageFacts(resolution.Packages, resolution.Selection));
        if (resolution.Finding is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                selectionEvidence,
                resolution.Finding));
        }

        var selection = resolution.Selection
            ?? throw new InvalidOperationException(
                "A complete Extension selection resolution requires its selection.");
        var packageFacts = PackageFacts(resolution.Packages, selection);
        var evidence = ExtensionInstallResultFactory.Evidence(
            sourceFact,
            selection,
            packageFacts);

        var foundationObservation = await _foundationReader.ReadAsync(
            request,
            resolution.Packages,
            cancellationToken).ConfigureAwait(false);
        if (foundationObservation.Finding is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                evidence,
                foundationObservation.Finding));
        }

        var foundation = foundationObservation.Foundation
            ?? throw new InvalidOperationException(
                "Complete Extension foundation observation requires its facts.");

        var targetInspection = await _targetInspector.InspectAsync(
            new ExtensionInstallTargetInspectionInput
            {
                Request = request,
                Packages = resolution.Packages,
                CurrentExtensions = foundation.CurrentExtensions,
                FrameworkLifecycle = foundation.FrameworkLifecycle,
                ProtectedAuthoredPaths = foundation.Topology.ProtectedPaths,
                InitialForceEligiblePaths = foundation.Topology.InitialForceEligiblePaths,
                Topology = foundation.Topology,
                SourceIdentity = source.Identity,
            },
            cancellationToken).ConfigureAwait(false);
        if (targetInspection.Finding is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                evidence,
                targetInspection.Finding));
        }

        var targetState = targetInspection.State
            ?? throw new InvalidOperationException(
                "Complete Extension target inspection requires its state.");
        ExtensionInstallFinding? forceBoundary;
        try
        {
            forceBoundary = await _targetInspector.AuthorizeForceAsync(
                request,
                targetState,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            forceBoundary = new ExtensionInstallFinding(
                ExtensionInstallFindingCode.Interrupted,
                "Extension Install force interaction was interrupted.");
        }

        if (forceBoundary is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                evidence,
                forceBoundary));
        }

        var effectPlan = await _effectPlanner.BuildAsync(
            new ExtensionInstallEffectPlanningInput
            {
                Request = request,
                Packages = resolution.Packages,
                Topology = foundation.Topology,
                TargetState = targetState,
                LifecycleRead = foundation.ExtensionsRead,
            },
            cancellationToken).ConfigureAwait(false);
        if (effectPlan.Finding is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                evidence,
                effectPlan.Finding));
        }

        var facts = ExtensionInstallResultFactory.PlanFacts(
            new ExtensionInstallPlannedFactsInput
            {
                Request = request,
                Source = sourceFact,
                Selection = selection,
                Packages = packageFacts,
                FrameworkPayload = foundation.FrameworkPayload,
                FrameworkLifecycle = foundation.FrameworkLifecycle,
                Topology = foundation.Topology,
                EffectPlan = effectPlan,
            });
        var plan = ExtensionInstallPlan.Create(new ExtensionInstallPlanInput
        {
            Request = request,
            SourceRead = source,
            SourceSignature = ExtensionInstallFoundationReader.SourceSignature(source),
            InferredRootId = sourceResolution.InferredRootId,
            Selection = selection,
            Packages = resolution.Packages,
            FrameworkPayload = foundation.FrameworkPayload,
            FrameworkLifecycle = foundation.FrameworkLifecycle,
            CurrentLifecycle = foundation.CurrentExtensions,
            IntendedLifecycle = targetState.IntendedLifecycle,
            Topology = foundation.Topology,
            Facts = facts,
            Effects = effectPlan.Effects,
            LifecycleChange = effectPlan.LifecycleChange,
            LifecycleRecoveryTarget = effectPlan.LifecycleRecoveryTarget,
        });
        return new ExtensionInstallPlanBuild(
            plan,
            ExtensionInstallResultFactory.Result(request, facts, []));
    }

    private static IReadOnlyList<ExtensionInstallPackage> PackageFacts(
        IReadOnlyList<ExtensionPackageFact> packages,
        ExtensionInstallSelection? selection)
        => packages.Select(package => new ExtensionInstallPackage(
            package.Id,
            selection?.RootIds.Contains(package.Id, StringComparer.Ordinal) == true,
            package.Dependencies.Order(StringComparer.Ordinal))).ToArray();

    private static ExtensionInstallResult Boundary(
        ExtensionInstallRequest request,
        ExtensionInstallBoundaryEvidence evidence,
        ExtensionInstallFindingCode code,
        string cause,
        string? target = null)
        => ExtensionInstallResultFactory.Boundary(
            request,
            evidence,
            new ExtensionInstallFinding(code, cause, target));

    private static ExtensionInstallPlanBuild Stop(ExtensionInstallResult result)
        => new(Plan: null, result);
}
