using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Commands.Extension.Shared.Permissions;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Install.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Install.Shared.Planning;

internal sealed class ExtensionInstallPlanner
{
    private readonly ExtensionInstallSourceResolver _sourceResolver;
    private readonly ExtensionInstallSelectionResolver _selectionResolver;
    private readonly ExtensionInstallFoundationReader _foundationReader;
    private readonly ExtensionInstallTargetInspector _targetInspector;
    private readonly ExtensionInstallEffectPlanner _effectPlanner;
    private readonly PhysicalPathResolver _physicalPathResolver;
    private readonly WorkspaceOwnershipStore _ownershipStore = new();

    internal ExtensionInstallPlanner(
        CliPrompt<CliMultiSelectQuestion<string>, CliMultiSelection<string>> selectionPrompt,
        string selectionQuestion,
        CliPlanConfirmation<ExtensionInstallResult, CliConfirmQuestion> forceConfirmation,
        Func<IReadOnlyList<string>, CliConfirmQuestion> forceQuestion,
        PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(selectionPrompt);
        ArgumentNullException.ThrowIfNull(forceConfirmation);
        ArgumentNullException.ThrowIfNull(forceQuestion);
        _physicalPathResolver = physicalPathResolver;
        var validator = new FileExpectationValidator(physicalPathResolver);
        _sourceResolver = new ExtensionInstallSourceResolver(physicalPathResolver);
        _selectionResolver = new ExtensionInstallSelectionResolver(
            selectionPrompt,
            selectionQuestion,
            new ExtensionInstallDependencyClosureResolver());
        _foundationReader = new ExtensionInstallFoundationReader(physicalPathResolver);
        _targetInspector = new ExtensionInstallTargetInspector(forceConfirmation, forceQuestion, validator);
        _effectPlanner = new ExtensionInstallEffectPlanner(_ownershipStore, validator);
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
                "Extension install was cancelled. Nothing was changed."));
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
        var mutationPackages = MutationPackages(
            resolution.Packages,
            selection,
            resolution.Ownership);
        var packageFacts = PackageFacts(resolution.Packages, selection);
        var evidence = ExtensionInstallResultFactory.Evidence(
            sourceFact,
            selection,
            packageFacts);

        var libraryBoundary = await ReadLibraryBoundaryAsync(
            request,
            mutationPackages.SelectMany(package => package.Payload)
                .Select(file => file.TargetPath)
                .OfType<string>(),
            resolution.Ownership,
            cancellationToken).ConfigureAwait(false);
        if (libraryBoundary is not null)
        {
            return Stop(ExtensionInstallResultFactory.Boundary(
                request,
                evidence,
                libraryBoundary));
        }

        var foundationObservation = await _foundationReader.ReadAsync(
            request,
            resolution.Packages,
            cancellationToken,
            resolution.Ownership).ConfigureAwait(false);
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
                MutationPackageIds = mutationPackages
                    .Select(package => package.Id)
                    .ToHashSet(StringComparer.Ordinal),
                Ownership = foundation.Ownership,
                FrameworkOwnership = foundation.FrameworkOwnership,
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
        var effectPlan = await _effectPlanner.BuildAsync(
            new ExtensionInstallEffectPlanningInput
            {
                Request = request,
                Packages = mutationPackages,
                Topology = foundation.Topology,
                TargetState = targetState,
                Ownership = foundation.Ownership,
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
                FrameworkOwnership = foundation.FrameworkOwnership,
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
            Packages = mutationPackages,
            ValidationPackages = resolution.Packages,
            FrameworkPayload = foundation.FrameworkPayload,
            FrameworkOwnership = foundation.FrameworkOwnership,
            Ownership = foundation.Ownership,
            IntendedExtensions = targetState.IntendedExtensions,
            Topology = foundation.Topology,
            TopologyFindings = foundation.TopologyFindings,
            ForceEligiblePaths = targetState.EligibleOccupants,
            Facts = facts,
            Effects = effectPlan.Effects,
            OwnershipChange = effectPlan.OwnershipChange,
            OwnershipRecoveryTarget = effectPlan.OwnershipRecoveryTarget,
        });
        return new ExtensionInstallPlanBuild(
            plan,
            ExtensionInstallResultFactory.Result(request, facts, plan.TopologyFindings));
    }

    internal ValueTask<ExtensionInstallFinding?> AuthorizeForceAsync(
        ExtensionInstallRequest request,
        ExtensionInstallPlan plan,
        ExtensionInstallResult preview,
        CancellationToken cancellationToken)
        => _targetInspector.AuthorizeForceAsync(
            request,
            plan.ForceEligiblePaths,
            preview,
            cancellationToken);

    private async ValueTask<ExtensionInstallFinding?> ReadLibraryBoundaryAsync(
        ExtensionInstallRequest request,
        IEnumerable<string> targetPaths,
        WorkspaceOwnershipRead? ownershipObservation,
        CancellationToken cancellationToken)
    {
        var unsafeTarget = targetPaths.FirstOrDefault(path => !ExtensionDestinationPolicy.IsAllowed(request.Workspace, path));
        if (unsafeTarget is not null)
        {
            return new ExtensionInstallFinding(ExtensionInstallFindingCode.TargetUnsafe,
                "The Extension destination is a protected workspace or recovery path.", unsafeTarget);
        }
        var ownership = ownershipObservation ?? await WorkspaceOwnershipReader.ReadAsync(
            _physicalPathResolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        if (ownership.State != WorkspaceOwnershipReadState.Complete)
        {
            return null;
        }
        var record = LibraryRegistrationReader.Read(ownership);
        if (record.State == LibraryRegistrationReadState.Complete)
        {
            var document = record.Record
                ?? throw new InvalidOperationException("A complete Library record observation requires its document.");
            var claimedPaths = document.Libraries
                .SelectMany(library => LibraryPathIdentity.Mappings(library).Select(mapping => mapping.DestinationPath))
                .Select(path => PortableWorkspacePath.CreatePortableKey(path.Value))
                .ToHashSet(StringComparer.Ordinal);
            var conflict = targetPaths.FirstOrDefault(path => claimedPaths.Contains(PortableWorkspacePath.CreatePortableKey(path)));
            if (conflict is not null)
            {
                return new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.OwnershipConflict,
                    "The Extension target is owned by a registered workspace Library.",
                    conflict);
            }

            var sourceRootKeys = document.Libraries
                .Select(library => PortableWorkspacePath.CreatePortableKey(library.SourceRoot.Value))
                .ToArray();
            var sourceConflict = targetPaths.FirstOrDefault(path =>
            {
                var targetKey = PortableWorkspacePath.CreatePortableKey(path);
                return sourceRootKeys.Any(rootKey => IsPortablePathWithinRoot(targetKey, rootKey));
            });
            return sourceConflict is null
                ? null
                : new ExtensionInstallFinding(
                    ExtensionInstallFindingCode.OwnershipConflict,
                    "The Extension target is inside the source root of a registered workspace Library.",
                    sourceConflict);
        }

        return new ExtensionInstallFinding(ExtensionInstallFindingCode.LifecycleObservation,
            record.Cause ?? "Library ownership could not be interpreted; no file effects were inferred.");
    }

    private static bool IsPortablePathWithinRoot(string targetKey, string rootKey)
        => rootKey == "."
            || targetKey == rootKey
            || targetKey.StartsWith($"{rootKey}/", StringComparison.Ordinal);

    private static ExtensionInstallPackage[] PackageFacts(
        IReadOnlyList<ExtensionPackageFact> packages,
        ExtensionInstallSelection? selection)
        => [.. packages.Select(package => new ExtensionInstallPackage(
            package.Id,
            package.Version,
            selection?.RootIds.Contains(package.Id, StringComparer.Ordinal) == true,
            package.Dependencies.Order(StringComparer.Ordinal)))];

    private static IReadOnlyList<ExtensionPackageFact> MutationPackages(
        IReadOnlyList<ExtensionPackageFact> packages,
        ExtensionInstallSelection selection,
        WorkspaceOwnershipRead? ownership)
    {
        ArgumentNullException.ThrowIfNull(packages);
        ArgumentNullException.ThrowIfNull(selection);

        // A missing lock is an established empty inventory. An invalid or
        // unreadable lock has no trustworthy inventory, so preserve the legacy
        // best-effort package set and let the existing lifecycle/write facts
        // report that condition. Only a trusted inventory can satisfy a
        // required dependency without making that dependency a mutation target.
        if (ownership is not { IsTrustworthy: true })
        {
            return packages;
        }

        var roots = selection.RootIds.ToHashSet(StringComparer.Ordinal);
        var installed = ownership.Document.Extensions
            .Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        return packages
            .Where(package => roots.Contains(package.Id) || !installed.Contains(package.Id))
            .ToArray();
    }

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
