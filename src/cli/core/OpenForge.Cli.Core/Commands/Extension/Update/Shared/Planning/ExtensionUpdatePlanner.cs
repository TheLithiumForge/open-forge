using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Selection;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Serialization;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.Core.Commands.Extension.Update.Shared.Planning;

internal sealed class ExtensionUpdatePlanner(
    ExtensionSourceReader sourceReader,
    LifecycleStore lifecycleStore,
    FileExpectationValidator validator,
    FrameworkLifecycleCurrentnessReader frameworkCurrentness,
    PhysicalPathResolver physicalPathResolver)
{
    private readonly ExtensionSourceReader _sourceReader = sourceReader;
    private readonly LifecycleStore _lifecycleStore = lifecycleStore;
    private readonly FileExpectationValidator _validator = validator;
    private readonly FrameworkLifecycleCurrentnessReader _frameworkCurrentness = frameworkCurrentness;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly ExtensionUpdateTopologyBuilder _topologyBuilder = new();
    private readonly ExtensionUpdateReconciler _reconciler = new(validator);

    internal async ValueTask<ExtensionUpdatePlanBuild> BuildAsync(
        ExtensionUpdateRequest request,
        CancellationToken cancellationToken)
        => await BuildAsync(request, allowedRecovery: null, cancellationToken)
            .ConfigureAwait(false);

    internal async ValueTask<ExtensionUpdatePlanBuild> BuildForVerificationAsync(
        ExtensionUpdateRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
        => await BuildAsync(request, allowedRecovery, cancellationToken).ConfigureAwait(false);

    private async ValueTask<ExtensionUpdatePlanBuild> BuildAsync(
        ExtensionUpdateRequest request,
        RecoveryBundlePreparation? allowedRecovery,
        CancellationToken cancellationToken)
    {
        var lifecycle = await _lifecycleStore.ReadAsync(
            request.Workspace,
            LifecycleSection.Extensions,
            cancellationToken).ConfigureAwait(false);
        if (lifecycle.State != LifecycleStoreReadState.Available)
        {
            return Stop(
                request,
                ReadLifecycleFinding(lifecycle.State),
                lifecycle.Cause ?? "The installed Extension lifecycle is unavailable.");
        }

        var currentLifecycle = lifecycle.Extensions
            ?? throw new InvalidOperationException(
                "An available Extension lifecycle read requires lifecycle facts.");

        RecoveryBundleCatalogueResult recovery;
        try
        {
            recovery = await RecoveryBundleCatalogue.ReadAsync(
                request.Workspace,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update recovery inspection was interrupted.");
        }
        catch (Exception)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.RecoveryUnavailable,
                "Extension Update recovery inspection failed unexpectedly.");
        }

        var recoveryIsExpected = recovery.State == RecoveryBundleCatalogueState.Available
            && (allowedRecovery is null
                ? recovery.Candidates.Length == 0
                : recovery.Candidates.Length == 1
                    && Matches(recovery.Candidates[0], allowedRecovery));
        if (!recoveryIsExpected)
        {
            return Stop(
                request,
                recovery.State == RecoveryBundleCatalogueState.Cancelled
                    ? ExtensionUpdateFindingCode.Interrupted
                    : recovery.State == RecoveryBundleCatalogueState.Available
                        ? ExtensionUpdateFindingCode.RecoveryConflict
                        : ExtensionUpdateFindingCode.RecoveryUnavailable,
                recovery.Cause ?? "Recognized recovery residuals block Extension Update.");
        }

        var source = await _sourceReader.ReadAsync(
            request.Workspace,
            request.SourcePath,
            cancellationToken).ConfigureAwait(false);
        if (source.State != ExtensionSourceReadState.Complete)
        {
            return Stop(
                request,
                ReadSourceFinding(source),
                source.Cause ?? "The selected Extension source is unavailable.",
                selection: null,
                source: SourceFact(source));
        }

        var inferred = !request.All
            && request.RequestedIds.Count == 0
            && source.Packages.Count == 1;
        if (!request.All && request.RequestedIds.Count == 0 && !inferred)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.SelectionRequired,
                "A multi-package Extension source requires explicit Extension IDs or --all.",
                source: SourceFact(source));
        }

        string[] selectedIds = request.All
            ? [.. currentLifecycle.Packages.Select(package => package.Id).Order(StringComparer.Ordinal)]
            : inferred
                ? [source.Packages[0].Id]
                : [.. request.RequestedIds.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        var selection = new ExtensionUpdateSelection(
            request.All
                ? ExtensionUpdateSelectionKind.ExplicitAll
                : inferred
                    ? ExtensionUpdateSelectionKind.SinglePackageInference
                    : ExtensionUpdateSelectionKind.ExplicitIds,
            request.All ? [] : selectedIds);
        var installed = currentLifecycle.Packages.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        var absentInstalled = selectedIds.FirstOrDefault(id => !installed.Contains(id));
        if (absentInstalled is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.LifecycleObservation,
                    "The selected Extension is not present in the trusted installed lifecycle.",
                    absentInstalled),
                selection,
                SourceFact(source));
        }

        var closure = ResolveClosure(source.Packages, selectedIds);
        if (closure.Cause is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.SourceUnavailable,
                    closure.Cause,
                    closure.Target),
                selection,
                SourceFact(source));
        }

        var uninstalledDependency = closure.Packages.FirstOrDefault(
            package => !installed.Contains(package.Id));
        if (uninstalledDependency is not null)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.LifecycleObservation,
                    "Extension Update cannot introduce an uninstalled dependency.",
                    uninstalledDependency.Id),
                selection,
                SourceFact(source));
        }

        var selectedSet = closure.Packages.Select(package => package.Id)
            .ToHashSet(StringComparer.Ordinal);
        var libraryTargets = closure.Packages.SelectMany(package => package.Payload)
            .Select(file => file.TargetPath)
            .OfType<string>()
            .Concat(currentLifecycle.Packages
                .Where(package => selectedSet.Contains(package.Id))
                .SelectMany(package => package.Paths));
        var libraryBoundary = await ReadLibraryBoundaryAsync(
            request,
            libraryTargets,
            cancellationToken).ConfigureAwait(false);
        if (libraryBoundary is not null)
        {
            return Stop(request, libraryBoundary, selection, SourceFact(source));
        }

        var topologyBuild = await BuildTopologyAsync(
            new TopologyBuildInput
            {
                Request = request,
                Packages = closure.Packages,
                CurrentLifecycle = currentLifecycle,
                Selection = selection,
                Source = source,
                Admission = new ExtensionUpdateTopologyAdmission(
                    new Dictionary<string, byte[]>(StringComparer.Ordinal),
                    new HashSet<string>(StringComparer.Ordinal)),
            },
            cancellationToken).ConfigureAwait(false);
        if (topologyBuild.Boundary is not null)
        {
            return topologyBuild.Boundary;
        }

        var topology = topologyBuild.Topology
            ?? throw new InvalidOperationException("A complete topology build requires topology facts.");
        var payloadRead = EmbeddedFrameworkPayloadReader.Read();
        var frameworkRead = await _lifecycleStore.ReadAsync(
            request.Workspace,
            LifecycleSection.Framework,
            cancellationToken).ConfigureAwait(false);
        if (payloadRead.Payload is not { } frameworkPayload
            || frameworkRead.State != LifecycleStoreReadState.Available
            || frameworkRead.Framework is not { } frameworkLifecycle)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.FrameworkUnavailable,
                frameworkRead.Cause ?? payloadRead.Cause ?? "The installed Framework anchor is unavailable.",
                selection,
                SourceFact(source));
        }

        var reconciliation = await _reconciler.BuildAsync(
            new ExtensionUpdateReconciliationInput
            {
                Request = request,
                Packages = closure.Packages,
                Current = currentLifecycle,
                Topology = topology,
                SourceIdentity = source.Identity,
            },
            cancellationToken).ConfigureAwait(false);
        if (reconciliation.Finding is null
            && (reconciliation.AdmittedOverrides.Count > 0
                || reconciliation.AdmittedExclusions.Count > 0))
        {
            var admittedTopology = await BuildTopologyAsync(
                new TopologyBuildInput
                {
                    Request = request,
                    Packages = closure.Packages,
                    CurrentLifecycle = currentLifecycle,
                    Selection = selection,
                    Source = source,
                    Admission = new ExtensionUpdateTopologyAdmission(
                        reconciliation.AdmittedOverrides,
                        reconciliation.AdmittedExclusions),
                },
                cancellationToken).ConfigureAwait(false);
            if (admittedTopology.Boundary is not null)
            {
                return admittedTopology.Boundary;
            }

            topology = admittedTopology.Topology
                ?? throw new InvalidOperationException(
                    "A complete admitted topology build requires topology facts.");
        }

        if (reconciliation.Finding is null)
        {
            reconciliation = await _reconciler.ReprojectGeneratedAsync(
                request,
                topology,
                reconciliation,
                cancellationToken).ConfigureAwait(false);
        }
        var packageFacts = closure.Packages.Select(package => new ExtensionUpdatePackage(
            package.Id,
            selectedIds.Contains(package.Id, StringComparer.Ordinal),
            package.Dependencies.Order(StringComparer.Ordinal))).ToArray();
        if (reconciliation.Finding is not null)
        {
            return Boundary(
                request,
                ExtensionUpdateResultFormationFactory.Facts(new ExtensionUpdateResultFactsInput
                {
                    Selection = selection,
                    Source = SourceFact(source),
                    Packages = packageFacts,
                    Comparisons = reconciliation.Comparisons,
                    Topology = topology,
                    Effects = [],
                    LifecycleAction = ExtensionUpdateLifecycleAction.None,
                    LifecycleOutcome = ExtensionUpdateLifecycleOutcome.NotRequested,
                    RecoveryState = ExtensionUpdateRecoveryState.NotRequired,
                    Verification = ExtensionUpdateVerificationState.NotRequested,
                }),
                reconciliation.Finding);
        }

        var currentness = await _frameworkCurrentness.ReadAsync(
            request.Workspace,
            frameworkLifecycle,
            frameworkPayload,
            cancellationToken).ConfigureAwait(false);
        if (currentness.State == FrameworkLifecycleCurrentnessState.Changed
            && frameworkLifecycle.GeneratedRegions.Any(region =>
                string.Equals(region.Path, currentness.Path, StringComparison.Ordinal)))
        {
            var currentBaseline = await BuildCurrentFrameworkBaselineAsync(
                request,
                frameworkLifecycle,
                topology,
                reconciliation.Comparisons,
                cancellationToken).ConfigureAwait(false);
            if (currentBaseline.Finding is { } finding)
            {
                return Stop(
                    request,
                    finding,
                    selection,
                    SourceFact(source));
            }

            currentness = await _frameworkCurrentness.ReadAsync(
                request.Workspace,
                currentBaseline.Lifecycle
                    ?? throw new InvalidOperationException(
                        "A complete current Framework baseline requires lifecycle facts."),
                frameworkPayload,
                cancellationToken).ConfigureAwait(false);
        }

        if (currentness.State != FrameworkLifecycleCurrentnessState.Current)
        {
            return Stop(
                request,
                new ExtensionUpdateFinding(
                    currentness.State == FrameworkLifecycleCurrentnessState.Cancelled
                        ? ExtensionUpdateFindingCode.Interrupted
                        : currentness.State is FrameworkLifecycleCurrentnessState.Unavailable
                            or FrameworkLifecycleCurrentnessState.Missing
                            ? ExtensionUpdateFindingCode.FrameworkUnavailable
                            : ExtensionUpdateFindingCode.FrameworkUnsafe,
                    currentness.Cause ?? "The installed Framework anchor is not current.",
                    currentness.Path),
                selection,
                SourceFact(source));
        }

        var lifecyclePlan = _lifecycleStore.PlanExtensionUpdate(
            lifecycle,
            reconciliation.IntendedLifecycle);
        if (lifecyclePlan.State == LifecycleWritePlanState.Blocked)
        {
            return Stop(
                request,
                ExtensionUpdateFindingCode.LifecycleBlocked,
                lifecyclePlan.Cause ?? "The intended Extension lifecycle is blocked.",
                selection,
                SourceFact(source));
        }

        RecoveryBundleTarget? lifecycleRecovery = null;
        if (lifecyclePlan.Change is { } lifecycleChange)
        {
            if (lifecycle.File is not { } lifecycleBefore)
            {
                return Stop(
                    request,
                    ExtensionUpdateFindingCode.LifecycleUnavailable,
                    "Lifecycle publication requires its exact prior snapshot.",
                    selection,
                    SourceFact(source));
            }

            lifecycleRecovery = RecoveryBundleTarget.Create(lifecycleChange, lifecycleBefore);
        }

        var hasChanges = reconciliation.Effects.Count > 0 || lifecyclePlan.Change is not null;
        var facts = ExtensionUpdateResultFormationFactory.Facts(new ExtensionUpdateResultFactsInput
        {
            Selection = selection,
            Source = SourceFact(source),
            Packages = packageFacts,
            Comparisons = reconciliation.Comparisons,
            Topology = topology,
            Effects = [.. reconciliation.Effects.Select(effect => effect.Result)],
            LifecycleAction = lifecyclePlan.State == LifecycleWritePlanState.Unchanged
                ? ExtensionUpdateLifecycleAction.Preserve
                : ExtensionUpdateLifecycleAction.Publish,
            LifecycleOutcome = lifecyclePlan.State == LifecycleWritePlanState.Unchanged
                ? ExtensionUpdateLifecycleOutcome.AlreadyCurrent
                : ExtensionUpdateLifecycleOutcome.Planned,
            RecoveryState = hasChanges
                ? ExtensionUpdateRecoveryState.NotCreated
                : ExtensionUpdateRecoveryState.NotRequired,
            Verification = hasChanges
                ? ExtensionUpdateVerificationState.Planned
                : ExtensionUpdateVerificationState.Verified,
        });
        var plan = ExtensionUpdatePlan.Create(new ExtensionUpdatePlanInput
        {
            Request = request,
            SourceRead = source,
            SourceSignature = ReadSourceSignature(source),
            Selection = selection,
            Packages = closure.Packages,
            FrameworkPayload = frameworkPayload,
            FrameworkLifecycle = frameworkLifecycle,
            LifecycleRead = lifecycle,
            CurrentLifecycle = currentLifecycle,
            IntendedLifecycle = reconciliation.IntendedLifecycle,
            Topology = topology,
            Facts = facts,
            Findings = reconciliation.Findings,
            Effects = reconciliation.Effects,
            DirectoryCreations = reconciliation.DirectoryCreations,
            LifecycleChange = lifecyclePlan.Change,
            LifecycleRecoveryTarget = lifecycleRecovery,
        });
        return new ExtensionUpdatePlanBuild { Plan = plan, Result = plan.Result };
    }

    private async ValueTask<ExtensionUpdateFinding?> ReadLibraryBoundaryAsync(
        ExtensionUpdateRequest request,
        IEnumerable<string> targetPaths,
        CancellationToken cancellationToken)
    {
        var record = await LibrariesRecordReader.ReadAsync(
            _physicalPathResolver,
            request.Workspace,
            cancellationToken).ConfigureAwait(false);
        if (record.State == LibrariesRecordReadState.Complete)
        {
            var claimedPaths = record.Record!.Libraries
                .SelectMany(library => library.Paths)
                .Select(path => path.Value)
                .ToHashSet(StringComparer.Ordinal);
            var conflict = targetPaths.FirstOrDefault(claimedPaths.Contains);
            return conflict is null
                ? null
                : new ExtensionUpdateFinding(
                    ExtensionUpdateFindingCode.OwnershipConflict,
                    "The Extension target is owned by a registered workspace Library.",
                    conflict);
        }

        return record.State switch
        {
            LibrariesRecordReadState.Missing => null,
            LibrariesRecordReadState.Unavailable => new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.ProjectionUnavailable,
                record.Cause ?? "Library ownership could not be observed for Extension Update."),
            LibrariesRecordReadState.Malformed or LibrariesRecordReadState.Blocked => new ExtensionUpdateFinding(
                ExtensionUpdateFindingCode.OwnershipConflict,
                record.Cause ?? "Library ownership is unsafe or ambiguous for Extension Update."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(request),
                record.State,
                "The Library record state is not defined."),
        };
    }

    private async ValueTask<TopologyBuild> BuildTopologyAsync(
        TopologyBuildInput input,
        CancellationToken cancellationToken)
    {
        var request = input.Request;
        try
        {
            var selectedIds = input.Packages.Select(package => package.Id)
                .ToHashSet(StringComparer.Ordinal);
            var intendedPaths = input.Packages.SelectMany(package => package.Payload)
                .Select(file => file.TargetPath)
                .OfType<string>()
                .ToHashSet(StringComparer.Ordinal);
            var retiredPaths = request.Prune
                ? input.CurrentLifecycle.Packages
                .Where(package => selectedIds.Contains(package.Id))
                .SelectMany(package => package.Paths)
                .Where(path => !intendedPaths.Contains(path))
                .ToHashSet(StringComparer.Ordinal)
                : new HashSet<string>(StringComparer.Ordinal);
            var topology = await _topologyBuilder.BuildAsync(
                new ExtensionUpdateTopologyInput(
                    request,
                    input.Packages,
                    retiredPaths,
                    input.Admission),
                cancellationToken)
                .ConfigureAwait(false);
            return new TopologyBuild(topology, Boundary: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.Interrupted,
                "Extension Update topology planning was interrupted.",
                input.Selection,
                SourceFact(input.Source)));
        }
        catch (ExtensionUpdateTopologyAliasException exception)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.SourceIdentityConflict,
                exception.Message,
                input.Selection,
                SourceFact(input.Source)));
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or IOException)
        {
            return new TopologyBuild(null, Stop(
                request,
                ExtensionUpdateFindingCode.ProjectionUnavailable,
                exception.Message,
                input.Selection,
                SourceFact(input.Source)));
        }
    }

    private static Closure ResolveClosure(
        IReadOnlyList<ExtensionPackageFact> universe,
        IReadOnlyList<string> roots)
    {
        var byId = universe.ToDictionary(package => package.Id, StringComparer.Ordinal);
        var visiting = new HashSet<string>(StringComparer.Ordinal);
        var visited = new HashSet<string>(StringComparer.Ordinal);
        var ordered = new List<ExtensionPackageFact>();
        foreach (var root in roots.Order(StringComparer.Ordinal))
        {
            if (!Visit(root, out var cause, out var target))
            {
                return new Closure([], cause, target);
            }
        }

        return new Closure(ordered, Cause: null, Target: null);

        bool Visit(string id, out string? cause, out string? target)
        {
            cause = null;
            target = null;
            if (!byId.TryGetValue(id, out var package))
            {
                cause = "The selected installed Extension has no exact package in the selected source.";
                target = id;
                return false;
            }

            if (visited.Contains(id))
            {
                return true;
            }

            if (!visiting.Add(id))
            {
                cause = "The selected Extension dependency closure contains a cycle.";
                target = id;
                return false;
            }

            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                if (!Visit(dependency, out cause, out target))
                {
                    return false;
                }
            }

            _ = visiting.Remove(id);
            _ = visited.Add(id);
            ordered.Add(package);
            return true;
        }
    }

    private static ExtensionUpdateFindingCode ReadSourceFinding(ExtensionSourceReadResult source)
        => source.State == ExtensionSourceReadState.Cancelled
            ? ExtensionUpdateFindingCode.Interrupted
            : source.FailureKind == ExtensionSourceFailureKind.Overlap
                ? ExtensionUpdateFindingCode.SourceOverlap
                : source.State == ExtensionSourceReadState.Invalid
                    ? ExtensionUpdateFindingCode.SourceInvalid
                    : ExtensionUpdateFindingCode.SourceUnavailable;

    private static ExtensionUpdateFindingCode ReadLifecycleFinding(
        LifecycleStoreReadState state)
        => state switch
        {
            LifecycleStoreReadState.Invalid
                or LifecycleStoreReadState.Blocked => ExtensionUpdateFindingCode.LifecycleBlocked,
            LifecycleStoreReadState.DocumentMissing
                or LifecycleStoreReadState.SectionMissing
                or LifecycleStoreReadState.Unavailable => ExtensionUpdateFindingCode.LifecycleUnavailable,
            LifecycleStoreReadState.Cancelled => ExtensionUpdateFindingCode.Interrupted,
            LifecycleStoreReadState.Available => throw new ArgumentException(
                "An available lifecycle read does not require failure mapping.",
                nameof(state)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle read state is not defined."),
        };

    private static ExtensionUpdateSource SourceFact(ExtensionSourceReadResult source)
        => new(
            source.Kind switch
            {
                ExtensionSourceKind.EmbeddedCatalogue => ExtensionUpdateSourceKind.Embedded,
                ExtensionSourceKind.Package => ExtensionUpdateSourceKind.Package,
                ExtensionSourceKind.Catalogue => ExtensionUpdateSourceKind.Catalogue,
                null => ExtensionUpdateSourceKind.Catalogue,
                _ => throw new ArgumentOutOfRangeException(nameof(source), source.Kind, "The source kind is not defined."),
            },
            source.Kind == ExtensionSourceKind.EmbeddedCatalogue ? null : source.Identity,
            source.Identity,
            source.Packages.Count);

    internal static string ReadSourceSignature(ExtensionSourceReadResult source)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        hash.AppendData(Encoding.UTF8.GetBytes(source.Identity));
        foreach (var package in source.Packages.OrderBy(package => package.Id, StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(
                $"\n{package.Id}\n{package.Name}\n{package.Description}\n{package.Version}\n{package.ManifestPath}"));
            foreach (var dependency in package.Dependencies.Order(StringComparer.Ordinal))
            {
                hash.AppendData(Encoding.UTF8.GetBytes($"\ndependency:{dependency}"));
            }

            foreach (var file in package.Payload.OrderBy(file => file.TargetPath, StringComparer.Ordinal))
            {
                hash.AppendData(Encoding.UTF8.GetBytes(
                    $"\nfile:{file.Path}\n{file.TargetPath}\n{file.State}\n{file.Sha256}"));
            }
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }

    private static ExtensionUpdatePlanBuild Stop(
        ExtensionUpdateRequest request,
        ExtensionUpdateFindingCode code,
        string cause,
        ExtensionUpdateSelection? selection = null,
        ExtensionUpdateSource? source = null)
        => Stop(request, new ExtensionUpdateFinding(code, cause), selection, source);

    private static ExtensionUpdatePlanBuild Stop(
        ExtensionUpdateRequest request,
        ExtensionUpdateFinding finding,
        ExtensionUpdateSelection? selection = null,
        ExtensionUpdateSource? source = null)
        => Boundary(
            request,
            ExtensionUpdateResultFormationFactory.EmptyFacts(selection, source),
            finding);

    private static ExtensionUpdatePlanBuild Boundary(
        ExtensionUpdateRequest request,
        ExtensionUpdateResultFacts facts,
        ExtensionUpdateFinding finding)
        => new()
        {
            Plan = null,
            Result = ExtensionUpdateResultFormationFactory.Create(request, facts, [finding]),
        };

    private async ValueTask<CurrentFrameworkBaselineBuild> BuildCurrentFrameworkBaselineAsync(
        ExtensionUpdateRequest request,
        FrameworkLifecycleState lifecycle,
        ExtensionUpdateTopology intendedTopology,
        IReadOnlyList<ExtensionUpdateComparison> comparisons,
        CancellationToken cancellationToken)
    {
        ExtensionUpdateTopology currentTopology;
        try
        {
            currentTopology = await _topologyBuilder.BuildAsync(
                new ExtensionUpdateTopologyInput(
                    request,
                    Packages: [],
                    new HashSet<string>(StringComparer.Ordinal),
                    new ExtensionUpdateTopologyAdmission(
                        new Dictionary<string, byte[]>(StringComparer.Ordinal),
                        new HashSet<string>(StringComparer.Ordinal))),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return CurrentFrameworkBaselineBuild.Stop(
                ExtensionUpdateFindingCode.Interrupted,
                "Current authored topology inspection was interrupted.");
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or IOException)
        {
            return CurrentFrameworkBaselineBuild.Stop(
                ExtensionUpdateFindingCode.FrameworkUnsafe,
                $"The current authored topology is unsafe: {exception.Message}");
        }

        var affectedPaths = comparisons
            .Where(comparison => comparison.Kind == ExtensionUpdateComparisonTargetKind.PackageFile
                && comparison.CurrentState is ExtensionUpdateComparisonCurrentState.Changed
                    or ExtensionUpdateComparisonCurrentState.Missing)
            .Select(comparison => comparison.Path)
            .ToHashSet(StringComparer.Ordinal);
        var generatedBytes = new Dictionary<string, byte[]>(StringComparer.Ordinal);
        foreach (var generated in lifecycle.GeneratedRegions)
        {
            if (!TryReadGeneratedBytes(currentTopology, generated.Path, out var expectedBytes)
                || !currentTopology.GeneratedEntries.TryGetValue(generated.Path, out var currentEntries)
                || !intendedTopology.GeneratedEntries.TryGetValue(generated.Path, out var intendedEntries))
            {
                return CurrentFrameworkBaselineBuild.Stop(
                    ExtensionUpdateFindingCode.FrameworkUnsafe,
                    "The current authored topology does not project every lifecycle generated target.",
                    generated.Path);
            }

            var logicalPath = Path.GetFullPath(Path.Combine(
                request.Workspace.LexicalRoot,
                generated.Path.Replace('/', Path.DirectorySeparatorChar)));
            var validation = await _validator.ValidateAsync(
                request.Workspace,
                FileExpectation.Missing(logicalPath),
                cancellationToken).ConfigureAwait(false);
            if (validation.State == FileExpectationValidationState.Cancelled)
            {
                return CurrentFrameworkBaselineBuild.Stop(
                    ExtensionUpdateFindingCode.Interrupted,
                    "Current generated navigation inspection was interrupted.",
                    generated.Path);
            }

            if (validation.State is FileExpectationValidationState.Blocked)
            {
                return CurrentFrameworkBaselineBuild.Stop(
                    ExtensionUpdateFindingCode.FrameworkUnsafe,
                    validation.Cause ?? "A current generated navigation target is unsafe.",
                    generated.Path);
            }

            if (validation.State is FileExpectationValidationState.Failed
                || validation.Actual is not { Kind: FileExpectationKind.File, HasBytes: true } actual)
            {
                return CurrentFrameworkBaselineBuild.Stop(
                    ExtensionUpdateFindingCode.FrameworkUnavailable,
                    validation.Cause ?? "A current generated navigation target is unavailable.",
                    generated.Path);
            }

            var bytes = actual.Bytes.ToArray();
            if (!bytes.AsSpan().SequenceEqual(expectedBytes)
                && !IsBoundedGeneratedDifference(
                    bytes,
                    expectedBytes,
                    currentEntries,
                    intendedEntries,
                    affectedPaths))
            {
                return CurrentFrameworkBaselineBuild.Stop(
                    ExtensionUpdateFindingCode.FrameworkUnsafe,
                    "Current generated navigation differs from the independently projected authored topology.",
                    generated.Path);
            }

            generatedBytes.Add(generated.Path, bytes);
        }

        return CurrentFrameworkBaselineBuild.Complete(
            WithGeneratedBytes(lifecycle, generatedBytes));
    }

    private bool IsBoundedGeneratedDifference(
        byte[] actualBytes,
        byte[] expectedBytes,
        IReadOnlyList<Framework.GeneratedNavigation.Models.GeneratedNavigationEntry> currentEntries,
        IReadOnlyList<Framework.GeneratedNavigation.Models.GeneratedNavigationEntry> intendedEntries,
        HashSet<string> affectedPaths)
    {
        string actualSource;
        string expectedSource;
        try
        {
            actualSource = new UTF8Encoding(false, true).GetString(actualBytes);
            expectedSource = new UTF8Encoding(false, true).GetString(expectedBytes);
        }
        catch (DecoderFallbackException)
        {
            return false;
        }

        var actual = _markdownParser.Parse(actualSource);
        var expected = _markdownParser.Parse(expectedSource);
        if (actual.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || actual.GeneratedRegion.ContentSpan is not { } actualContent
            || expected.GeneratedRegion.State != MarkdownGeneratedRegionState.Complete
            || expected.GeneratedRegion.ContentSpan is not { } expectedContent
            || !string.Equals(
                actualSource[..actualContent.Start],
                expectedSource[..expectedContent.Start],
                StringComparison.Ordinal)
            || !string.Equals(
                actualSource[actualContent.End..],
                expectedSource[expectedContent.End..],
                StringComparison.Ordinal))
        {
            return false;
        }

        var actualLines = ReadGeneratedLines(actualSource, actualContent);
        var expectedLines = ReadGeneratedLines(expectedSource, expectedContent);
        if (actualLines.Length != actualLines.Distinct(StringComparer.Ordinal).Count())
        {
            return false;
        }

        var expectedByLine = currentEntries.ToDictionary(entry => entry.Line, StringComparer.Ordinal);
        var allowedEntries = currentEntries.Concat(intendedEntries)
            .Where(entry => affectedPaths.Contains(entry.CanonicalPath))
            .GroupBy(entry => entry.Destination, StringComparer.Ordinal)
            .Select(group => group.First())
            .ToArray();
        var actualAttributions = new HashSet<string>(StringComparer.Ordinal);
        foreach (var line in actualLines.Where(line => !expectedLines.Contains(line, StringComparer.Ordinal)))
        {
            if (IsEmptyGeneratedLine(line))
            {
                if (expectedByLine.Values.Any(entry => !affectedPaths.Contains(entry.CanonicalPath)))
                {
                    return false;
                }

                continue;
            }

            var attribution = allowedEntries.FirstOrDefault(entry =>
                line.StartsWith("- [", StringComparison.Ordinal)
                && line.Contains($"]({entry.Destination}) - ", StringComparison.Ordinal));
            if (attribution is null || !actualAttributions.Add(attribution.CanonicalPath))
            {
                return false;
            }
        }

        foreach (var line in expectedLines.Where(line => !actualLines.Contains(line, StringComparer.Ordinal)))
        {
            if (IsEmptyGeneratedLine(line))
            {
                if (actualLines.Any(value => !allowedEntries.Any(entry =>
                    value.StartsWith("- [", StringComparison.Ordinal)
                    && value.Contains($"]({entry.Destination}) - ", StringComparison.Ordinal))))
                {
                    return false;
                }

                continue;
            }

            if (!expectedByLine.TryGetValue(line, out var entry)
                || !affectedPaths.Contains(entry.CanonicalPath))
            {
                return false;
            }
        }

        var unaffectedLines = currentEntries
            .Where(entry => !affectedPaths.Contains(entry.CanonicalPath))
            .Select(entry => entry.Line)
            .ToArray();
        return actualLines.Where(unaffectedLines.Contains).SequenceEqual(unaffectedLines);
    }

    private static string[] ReadGeneratedLines(
        string source,
        MarkdownTextSpan content)
    {
        var body = source[content.Start..content.End];
        if (body.Contains('\r') && !body.Contains("\r\n", StringComparison.Ordinal))
        {
            return [body];
        }

        return body.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Split('\n', StringSplitOptions.RemoveEmptyEntries);
    }

    private static bool IsEmptyGeneratedLine(string line)
        => string.Equals(line, "- none - No entries - #Empty", StringComparison.Ordinal);

    private static bool TryReadGeneratedBytes(
        ExtensionUpdateTopology topology,
        string path,
        out byte[] bytes)
    {
        bytes = topology.IntendedTargetBytes.GetValueOrDefault(path)
            ?? topology.GeneratedTargetBytes.GetValueOrDefault(path)
            ?? [];
        return bytes.Length > 0;
    }

    private static bool Matches(
        RecoveryBundleCandidateSnapshot candidate,
        RecoveryBundlePreparation preparation)
    {
        if (candidate.Kind != RecoveryBundleCandidateKind.Final
            || candidate.Integrity != RecoveryBundleIntegrity.Verified
            || candidate.Verified is not { } verified)
        {
            return false;
        }

        return PhysicalIdentityTracker.PathComparer.Equals(candidate.Path, preparation.BundlePath)
            && PhysicalIdentityTracker.PathComparer.Equals(
                verified.WorkspacePhysicalPath,
                preparation.WorkspacePhysicalPath)
            && string.Equals(verified.WorkspaceKey, preparation.WorkspaceKey, StringComparison.Ordinal)
            && string.Equals(verified.Command, preparation.Command, StringComparison.Ordinal)
            && verified.Attribution == preparation.Attribution
            && verified.OperationId == preparation.OperationId
            && verified.Entries.SequenceEqual(preparation.Entries);
    }

    private static FrameworkLifecycleState WithGeneratedBytes(
        FrameworkLifecycleState lifecycle,
        IReadOnlyDictionary<string, byte[]> generatedBytes)
    {
        var contentIdentity = new FrameworkContentIdentity();
        var generated = lifecycle.GeneratedRegions
            .Select(region => (region.Path, region.Region))
            .ToHashSet();
        return new FrameworkLifecycleState
        {
            Coverage = lifecycle.Coverage,
            Source = new FrameworkLifecycleSource
            {
                Id = lifecycle.Source.Id,
                Version = lifecycle.Source.Version,
                InventoryFingerprint = lifecycle.Source.InventoryFingerprint,
            },
            Targets = [.. lifecycle.Targets.Select(target =>
            {
                if (target.Region is not { } region
                    || !generated.Contains((target.Path, region)))
                {
                    return new FrameworkLifecycleTarget
                    {
                        Path = target.Path,
                        SourceAssetPath = target.SourceAssetPath,
                        Region = target.Region,
                        BaselineFingerprint = target.BaselineFingerprint,
                        FingerprintKind = target.FingerprintKind,
                    };
                }

                var bytes = generatedBytes.GetValueOrDefault(target.Path)
                    ?? throw new InvalidDataException(
                        $"Generated Framework target '{target.Path}' has no independently validated bytes.");
                return new FrameworkLifecycleTarget
                {
                    Path = target.Path,
                    SourceAssetPath = target.SourceAssetPath,
                    Region = target.Region,
                    BaselineFingerprint = contentIdentity.ReadGeneratedEntriesFingerprint(
                        bytes,
                        target.FingerprintKind),
                    FingerprintKind = target.FingerprintKind,
                };
            })],
            GeneratedRegions = [.. lifecycle.GeneratedRegions.Select(region => new FrameworkGeneratedRegion
            {
                Path = region.Path,
                Region = region.Region,
            })],
        };
    }

    private sealed record Closure(
        IReadOnlyList<ExtensionPackageFact> Packages,
        string? Cause,
        string? Target);

    private sealed record TopologyBuild(
        ExtensionUpdateTopology? Topology,
        ExtensionUpdatePlanBuild? Boundary);

    private sealed class TopologyBuildInput
    {
        internal required ExtensionUpdateRequest Request { get; init; }

        internal required IReadOnlyList<ExtensionPackageFact> Packages { get; init; }

        internal required ExtensionLifecycleState CurrentLifecycle { get; init; }

        internal required ExtensionUpdateSelection Selection { get; init; }

        internal required ExtensionSourceReadResult Source { get; init; }

        internal required ExtensionUpdateTopologyAdmission Admission { get; init; }
    }

    private sealed record CurrentFrameworkBaselineBuild(
        FrameworkLifecycleState? Lifecycle,
        ExtensionUpdateFinding? Finding)
    {
        internal static CurrentFrameworkBaselineBuild Complete(FrameworkLifecycleState lifecycle)
            => new(lifecycle, Finding: null);

        internal static CurrentFrameworkBaselineBuild Stop(
            ExtensionUpdateFindingCode code,
            string cause,
            string? target = null)
            => new(Lifecycle: null, new ExtensionUpdateFinding(code, cause, target));
    }
}
