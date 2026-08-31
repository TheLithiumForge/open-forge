using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly RouteInitTargetPlanner _targetPlanner = new();
    private readonly RouteInitCurrentStateReader _currentStateReader = new();
    private readonly RouteInitFrameworkAlignmentBuilder _alignmentBuilder = new();
    private readonly RouteInitFrameworkLifecycleBuilder _lifecycleBuilder = new();
    private readonly RouteInitProspectiveTopologyPlanner _topologyPlanner = new();
    private readonly RouteInitMetadataResolver _metadataResolver = new();
    private readonly RouteInitScaffoldComposer _scaffoldComposer = new();
    private readonly RouteInitExactSourceSnapshotReader _snapshotReader = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly FrameworkDocumentMetadataParser _frameworkMetadataParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();
    private readonly RecoveryBundleCatalogue _recoveryCatalogue;

    internal RouteInitPlanBuilder()
        : this(new RecoveryBundleCatalogue(new RecoveryBundleReader()))
    {
    }

    internal RouteInitPlanBuilder(RecoveryBundleCatalogue recoveryCatalogue)
    {
        ArgumentNullException.ThrowIfNull(recoveryCatalogue);
        _recoveryCatalogue = recoveryCatalogue;
    }

    internal async ValueTask<RouteInitPlanBuild> BuildAsync(
        RouteInitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (cancellationToken.IsCancellationRequested)
        {
            return Stop(request, null, RouteInitFindingCode.Interrupted, "Route Init planning was cancelled.", incomplete: true);
        }

        var resolution = _targetPlanner.Resolve(request);
        if (!resolution.IsResolved || resolution.Target is not { } requestedTarget)
        {
            return Stop(
                request,
                null,
                RouteInitFindingCode.InvalidTarget,
                resolution.Cause ?? "The Route Init target is invalid.",
                incomplete: true,
                requested: request.RouteTarget);
        }

        if (!IsValidMetadataInput(request))
        {
            return Stop(
                request,
                requestedTarget,
                RouteInitFindingCode.InvalidMetadata,
                "The Route Init metadata input is invalid for the selected scaffold and target.",
                incomplete: true);
        }

        FrameworkPayload? payload = null;
        RouteInitFrameworkAlignment? alignment = null;
        var target = requestedTarget;
        if (request.Scaffold == RouteInitScaffold.Framework)
        {
            var payloadRead = EmbeddedFrameworkPayloadReader.Read();
            if (payloadRead.Payload is not { } embedded)
            {
                return Stop(
                    request,
                    requestedTarget,
                    payloadRead.State == FrameworkPayloadReadState.Invalid
                        ? RouteInitFindingCode.FrameworkPayloadInvalid
                        : RouteInitFindingCode.FrameworkPayloadUnavailable,
                    payloadRead.Cause ?? "The embedded Framework payload is unavailable.",
                    incomplete: payloadRead.State != FrameworkPayloadReadState.Invalid);
            }

            payload = embedded;
            ImmutableArray<SourceLogicalSource> projected;
            try
            {
                projected = new EmbeddedFrameworkSourceProjector().Project(request.Workspace, payload);
            }
            catch (Exception exception) when (exception is ArgumentException
                or InvalidDataException
                or InvalidOperationException)
            {
                return Stop(
                    request,
                    requestedTarget,
                    RouteInitFindingCode.FrameworkPayloadInvalid,
                    exception.Message,
                    incomplete: false);
            }

            var aligned = _alignmentBuilder.Build(request.Workspace, requestedTarget, projected);
            if (aligned.State != RouteInitFrameworkAlignmentState.Complete
                || aligned.Alignment is not { } completeAlignment)
            {
                var code = aligned.Cause?.Contains("scope label", StringComparison.OrdinalIgnoreCase) == true
                    ? RouteInitFindingCode.InvalidTarget
                    : RouteInitFindingCode.FrameworkAlignmentBlocked;
                return Stop(
                    request,
                    requestedTarget,
                    code,
                    aligned.Cause ?? "The Framework target could not be aligned.",
                    incomplete: code == RouteInitFindingCode.InvalidTarget);
            }

            alignment = completeAlignment;
            target = alignment.Target;
        }

        var catalogue = await _currentStateReader.ReadCatalogueAsync(
                request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        if (catalogue.IsCancelled)
        {
            return Stop(request, target, RouteInitFindingCode.Interrupted, "Route Init inspection was cancelled.", incomplete: true);
        }

        var catalogueBoundary = ReadCatalogueBoundary(request, target, catalogue);
        if (catalogueBoundary is not null)
        {
            return catalogueBoundary;
        }

        var exactCompatibilityBoundary = ReadExactCompatibilityBoundary(
            request,
            target,
            catalogue,
            alignment,
            payload);
        if (exactCompatibilityBoundary is not null)
        {
            return exactCompatibilityBoundary;
        }

        RouteInitFrameworkTrust? trust = null;
        if (payload is not null)
        {
            trust = await _lifecycleBuilder.ReadTrustAsync(
                    request.Workspace,
                    payload,
                    cancellationToken)
                .ConfigureAwait(false);
            if (!trust.IsCurrent)
            {
                return StopForTrust(request, target, trust, alignment, payload);
            }
        }

        var current = await _currentStateReader.ReadChainAsync(catalogue, target, cancellationToken)
            .ConfigureAwait(false);
        var currentBoundary = ReadCurrentBoundary(request, current, alignment, payload);
        if (currentBoundary is not null)
        {
            return currentBoundary;
        }

        if (current.Chain[^1].Existing is not null && HasMetadataInput(request.Metadata))
        {
            return Stop(
                request,
                target,
                RouteInitFindingCode.InvalidMetadata,
                "Metadata flags cannot be applied to an existing final route entrypoint.",
                incomplete: true);
        }

        IntendedChain intended;
        try
        {
            var frameworkBasis = alignment is not null && payload is not null && trust is not null
                ? new FrameworkPlanningBasis(alignment, payload, trust)
                : null;
            intended = await BuildIntendedChainAsync(
                    request,
                    current,
                    frameworkBasis,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RouteInitPlanningException exception)
        {
            return Stop(request, target, exception.Code, exception.Message, exception.Incomplete);
        }

        var topology = _topologyPlanner.Build(catalogue, intended.Entries.Select(entry => entry.Source));
        if (!topology.IsSafeForProjection)
        {
            return Stop(
                request,
                target,
                ReadTopologyFinding(topology),
                "The prospective route topology is ambiguous, colliding, or unsafe.",
                incomplete: false,
                alignment: alignment,
                payload: payload);
        }

        var projectionInputs = BuildProjectionInputs(topology, intended, current);
        if (projectionInputs.Boundary is { } projectionInputBoundary)
        {
            return Stop(
                request,
                target,
                projectionInputBoundary.Code,
                projectionInputBoundary.Cause,
                projectionInputBoundary.Incomplete,
                alignment: alignment,
                payload: payload);
        }

        var projectionBuild = _topologyPlanner.Project(
            topology,
            projectionInputs.Regions,
            projectionInputs.Metadata);
        if (projectionBuild.State != RouteInitProspectiveProjectionState.Complete
            || projectionBuild.Projection is not { } projection)
        {
            var blockedCode = projectionBuild.Projection?.Regions.Any(region =>
                    region.State != GeneratedNavigationRegionState.Available
                    && region.Source.Base.Form == SourceDocumentForm.Loader) == true
                ? RouteInitFindingCode.LoaderUnsafe
                : RouteInitFindingCode.GeneratedRegionUnsafe;
            return Stop(
                request,
                target,
                projectionBuild.State == RouteInitProspectiveProjectionState.Unavailable
                    ? RouteInitFindingCode.ProjectionIncomplete
                    : blockedCode,
                projectionBuild.Cause ?? "The generated navigation projection is unavailable.",
                incomplete: projectionBuild.State == RouteInitProspectiveProjectionState.Unavailable,
                alignment: alignment,
                payload: payload);
        }

        IReadOnlyList<RouteInitProspectiveSourceContent> effectSources;
        try
        {
            effectSources = await BuildEffectSourcesAsync(
                    request,
                    topology,
                    intended,
                    current,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RouteInitPlanningException exception)
        {
            return Stop(
                request,
                target,
                exception.Code,
                exception.Message,
                exception.Incomplete,
                alignment: alignment,
                payload: payload);
        }

        IReadOnlyList<FileStateSnapshot> directoryStates;
        try
        {
            directoryStates = ReadMissingDirectories(request, intended);
        }
        catch (RouteInitPlanningException exception)
        {
            return Stop(request, target, exception.Code, exception.Message, exception.Incomplete, alignment: alignment, payload: payload);
        }

        var initialEffects = _topologyPlanner.AssembleEffects(
            topology,
            projection,
            directoryStates,
            effectSources,
            lifecycleEffect: null);
        if (initialEffects.State != RouteInitProspectiveEffectPlanState.Complete
            || initialEffects.Plan is not { } effectPlan)
        {
            return Stop(
                request,
                target,
                RouteInitFindingCode.GeneratedRegionUnsafe,
                initialEffects.Cause ?? "The Route Init effects are unsafe.",
                incomplete: false,
                alignment: alignment,
                payload: payload);
        }

        FrameworkLifecycleState? intendedLifecycle = null;
        if (payload is not null && trust is not null)
        {
            var lifecycle = _lifecycleBuilder.BuildPlan(trust, payload, effectPlan.FrameworkManagedStates);
            if (lifecycle.State != RouteInitFrameworkLifecyclePlanState.Complete
                || lifecycle.WritePlan is not { } writePlan)
            {
                return Stop(
                    request,
                    target,
                    RouteInitFindingCode.LifecycleBlocked,
                    lifecycle.Cause ?? "The scoped Framework lifecycle update is blocked.",
                    incomplete: false,
                    alignment: alignment,
                    payload: payload);
            }

            if (trust.Read.File is not { } lifecycleBefore)
            {
                return Stop(
                    request,
                    target,
                    RouteInitFindingCode.LifecycleBlocked,
                    "The trusted Framework lifecycle read has no exact file snapshot.",
                    incomplete: false,
                    alignment: alignment,
                    payload: payload);
            }

            intendedLifecycle = writePlan.State == LifecycleWritePlanState.Planned
                ? lifecycle.Intended
                : null;
            effectPlan = _topologyPlanner.AssembleEffects(
                topology,
                projection,
                directoryStates,
                effectSources,
                new RouteInitLifecycleEffectInput(writePlan, lifecycleBefore)).Plan
                ?? throw new InvalidOperationException("A complete scoped lifecycle plan must form complete Route Init effects.");
        }

        if (effectPlan.RecoveryTargets.Count > 0)
        {
            var recovery = await _recoveryCatalogue.ReadAsync(
                    request.Workspace,
                    cancellationToken)
                .ConfigureAwait(false);
            var recoveryBoundary = ReadRecoveryBoundary(
                request,
                target,
                alignment,
                payload,
                recovery);
            if (recoveryBoundary is not null)
            {
                return recoveryBoundary;
            }
        }

        var preflight = await new MutationPreflight(
                new FileExpectationValidator(_physicalPathResolver))
            .ValidateAsync(
                request.Workspace,
                effectPlan.DirectoryCreations,
                effectPlan.FileChanges,
                cancellationToken)
            .ConfigureAwait(false);
        if (preflight.State != MutationValidationState.Valid)
        {
            return StopForPreflight(request, target, alignment, payload, preflight);
        }

        var preview = BuildPreview(
            request,
            target,
            alignment,
            payload,
            intended,
            projection,
            effectPlan,
            intendedLifecycle);
        var plan = new RouteInitPlan(
            request,
            preview,
            effectPlan.DirectoryCreations,
            effectPlan.FileChanges,
            effectPlan.RecoveryTargets,
            intendedLifecycle);
        return new RouteInitPlanBuild(plan, preview);
    }

    private async ValueTask<IntendedChain> BuildIntendedChainAsync(
        RouteInitRequest request,
        RouteInitCurrentStateFacts current,
        FrameworkPlanningBasis? framework,
        CancellationToken cancellationToken)
    {
        var entries = new List<IntendedEntrypoint>(current.Chain.Count);
        for (var index = 0; index < current.Chain.Count; index++)
        {
            var chain = current.Chain[index];
            var aligned = framework?.Alignment.Segments[index];
            if (chain.Existing is { } existing)
            {
                var read = current.Reads.Single(item => ReferenceEquals(item.Source, existing));
                var before = await _snapshotReader.ReadAsync(
                        request,
                        read.Base,
                        cancellationToken)
                    .ConfigureAwait(false);
                var sourceAssetPath = ReadTrustedSourceAssetPath(
                    existing,
                    aligned,
                    framework?.Trust);
                var ownsGeneratedEntries = IsManagedFrameworkSegment(aligned);
                entries.Add(new IntendedEntrypoint(
                    chain,
                    existing,
                    before,
                    before.Bytes.AsSpan(),
                    metadata: null,
                    sourceAssetPath,
                    sourceAssetPath is not null
                        ? RouteInitEntrypointOwnership.Framework
                        : RouteInitEntrypointOwnership.User,
                    ownsGeneratedEntries));
                continue;
            }

            if (aligned is not null
                && aligned.Role is RouteInitFrameworkSegmentRole.InstalledRoot or RouteInitFrameworkSegmentRole.Managed)
            {
                var sourceAssetPath = aligned.SourceAssetPath
                    ?? throw new RouteInitPlanningException(RouteInitFindingCode.FrameworkPayloadInvalid, "A managed Framework segment has no embedded source path.", false);
                var asset = framework?.Payload.Find(sourceAssetPath)
                    ?? throw new RouteInitPlanningException(RouteInitFindingCode.FrameworkPayloadInvalid, "A managed Framework segment has no embedded asset.", false);
                var document = _markdownParser.Parse(StrictUtf8.GetString(asset.Bytes.AsSpan()));
                var facts = _frameworkMetadataParser.Parse(document);
                if (facts.State != FrameworkDocumentMetadataState.Complete || facts.Metadata is not { } authored)
                {
                    throw new RouteInitPlanningException(RouteInitFindingCode.FrameworkPayloadInvalid, "A managed Framework entrypoint has incomplete embedded metadata.", false);
                }

                var embeddedMetadata = new RouteInitMetadata(
                    authored.Description,
                    RouteInitDescriptionSource.Embedded,
                    authored.Responsibility,
                    RouteInitResponsibilitySource.Embedded,
                    authored.Tags,
                    RouteInitTagsSource.Embedded);
                entries.Add(new IntendedEntrypoint(
                    chain,
                    aligned.IntendedSource,
                    MissingSnapshot(request, aligned.IntendedSource.Identity.CanonicalBasePath),
                    asset.Bytes.AsSpan(),
                    embeddedMetadata,
                    sourceAssetPath,
                    RouteInitEntrypointOwnership.Framework,
                    ownsGeneratedEntries: true));
                continue;
            }

            var metadata = _metadataResolver.Resolve(
                chain.Id,
                index == current.Chain.Count - 1,
                request.Metadata);
            var title = chain.Id.Split('/')[^1];
            var scaffold = _scaffoldComposer.Compose(chain.Id, title, metadata);
            var source = aligned?.IntendedSource ?? CreateSource(request, chain.Id, chain.CanonicalMissingPath);
            entries.Add(new IntendedEntrypoint(
                chain,
                source,
                MissingSnapshot(request, source.Identity.CanonicalBasePath),
                scaffold.Bytes.AsSpan(),
                scaffold.Metadata,
                sourceAssetPath: null,
                RouteInitEntrypointOwnership.User,
                ownsGeneratedEntries: false));
        }

        return new IntendedChain(entries);
    }

    private static bool IsManagedFrameworkSegment(RouteInitFrameworkAlignedSegment? aligned)
        => aligned?.Role is RouteInitFrameworkSegmentRole.InstalledRoot
            or RouteInitFrameworkSegmentRole.Managed;

    private static string? ReadTrustedSourceAssetPath(
        SourceLogicalSource existing,
        RouteInitFrameworkAlignedSegment? aligned,
        RouteInitFrameworkTrust? trust)
    {
        if (!IsManagedFrameworkSegment(aligned)
            || aligned?.SourceAssetPath is not { } sourceAssetPath
            || trust?.Read.Framework is not { } lifecycle)
        {
            return null;
        }

        return lifecycle.Targets.Any(target => target.Region is null
                && string.Equals(
                    target.Path,
                    existing.Identity.CanonicalBasePath,
                    StringComparison.Ordinal)
                && string.Equals(
                    target.SourceAssetPath,
                    sourceAssetPath,
                    StringComparison.Ordinal))
            ? sourceAssetPath
            : null;
    }

    private async ValueTask<IReadOnlyList<RouteInitProspectiveSourceContent>> BuildEffectSourcesAsync(
        RouteInitRequest request,
        RouteInitProspectiveTopology topology,
        IntendedChain intended,
        RouteInitCurrentStateFacts current,
        CancellationToken cancellationToken)
    {
        var sources = intended.Entries
            .Select(entry => entry.Content)
            .ToList();
        if (topology.Formation.Loader is not { } loader)
        {
            return sources;
        }

        var currentLoader = current.Reads.Single(read => string.Equals(
            read.Source.Identity.CanonicalBasePath,
            loader.Identity.CanonicalBasePath,
            StringComparison.Ordinal));
        var before = await _snapshotReader.ReadAsync(
                request,
                currentLoader.Base,
                cancellationToken)
            .ConfigureAwait(false);
        sources.Add(new RouteInitProspectiveSourceContent(
            loader,
            before,
            before.Bytes.AsSpan(),
            sourceAssetPath: null,
            ownsGeneratedEntries: false));
        return sources;
    }

    private ProjectionInputs BuildProjectionInputs(
        RouteInitProspectiveTopology topology,
        IntendedChain intended,
        RouteInitCurrentStateFacts current)
    {
        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in intended.Entries)
        {
            try
            {
                documents.Add(entry.Source.Identity.CanonicalBasePath, StrictUtf8.GetString(entry.Content.IntendedBytes.AsSpan()));
            }
            catch (DecoderFallbackException exception)
            {
                return ProjectionInputs.Blocked(RouteInitFindingCode.FrameworkPayloadInvalid, exception.Message, incomplete: false);
            }
        }

        foreach (var read in current.Reads)
        {
            if (documents.ContainsKey(read.Source.Identity.CanonicalBasePath))
            {
                continue;
            }

            if (read.Base.Read is not { State: FileReadState.Complete, Value: { } value })
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.InspectionIncomplete,
                    "A route projection source could not be read completely.",
                    incomplete: true);
            }

            documents.Add(read.Source.Identity.CanonicalBasePath, value);
        }

        var metadata = new List<GeneratedNavigationMetadata>();
        foreach (var source in topology.Formation.Sources.Where(source => source.Base.Form != SourceDocumentForm.Loader))
        {
            if (!documents.TryGetValue(source.Identity.CanonicalBasePath, out var sourceText))
            {
                continue;
            }

            var facts = _metadataParser.Parse(_markdownParser.Parse(sourceText), source.Base.Form);
            if (facts.State is SourceAuthoredMetadataState.Missing)
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.MetadataIncomplete,
                    $"The source '{source.Identity.CanonicalBasePath}' lacks required authored metadata.",
                    incomplete: true);
            }

            if (facts.State is SourceAuthoredMetadataState.Malformed)
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.MetadataUnsafe,
                    $"The source '{source.Identity.CanonicalBasePath}' has malformed authored metadata.",
                    incomplete: false);
            }

            metadata.Add(new GeneratedNavigationMetadata(source, facts));
        }

        var regionPaths = intended.Entries
            .Select(entry => entry.Source.Identity.CanonicalBasePath)
            .ToHashSet(StringComparer.Ordinal);
        if (topology.Formation.Loader is not null)
        {
            regionPaths.Add(topology.Formation.Loader.Identity.CanonicalBasePath);
        }

        var regions = new List<GeneratedNavigationRegionInput>();
        foreach (var path in regionPaths.Order(StringComparer.Ordinal))
        {
            var source = topology.Formation.FindSource(path)
                ?? throw new InvalidOperationException("A prospective region source must belong to the formation.");
            if (!documents.TryGetValue(path, out var sourceText))
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.InspectionIncomplete,
                    $"The projection source '{path}' is unavailable.",
                    incomplete: true);
            }

            regions.Add(new GeneratedNavigationRegionInput(source, _markdownParser.Parse(sourceText)));
        }

        return ProjectionInputs.Complete(regions, metadata);
    }

    private IReadOnlyList<FileStateSnapshot> ReadMissingDirectories(
        RouteInitRequest request,
        IntendedChain intended)
    {
        var canonicalDirectories = new List<string> { SourceLogicalPath.AgentsRoot };
        foreach (var entry in intended.Entries)
        {
            canonicalDirectories.Add(SourceLogicalPath.ReadParent(entry.Source.Identity.CanonicalBasePath));
        }

        var results = new List<FileStateSnapshot>();
        foreach (var canonical in canonicalDirectories.Distinct(StringComparer.Ordinal))
        {
            var logical = SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, canonical);
            var resolution = _physicalPathResolver.ResolveCandidate(
                request.Workspace.LexicalRoot,
                request.Workspace.PhysicalRoot,
                logical);
            if (resolution.State == PhysicalPathState.Missing)
            {
                results.Add(FileStateSnapshot.Missing(logical));
                continue;
            }

            if (resolution.State != PhysicalPathState.Contained)
            {
                throw new RouteInitPlanningException(
                    RouteInitFindingCode.TargetUnsafe,
                    $"The route directory '{canonical}' has an unsafe physical boundary.",
                    incomplete: false);
            }

            try
            {
                var attributes = File.GetAttributes(resolution.GetContainedPhysicalPath());
                if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
                    != FileAttributes.Directory)
                {
                    throw new RouteInitPlanningException(
                        RouteInitFindingCode.IdentityCollision,
                        $"The route directory '{canonical}' is occupied by a non-directory target.",
                        incomplete: false);
                }
            }
            catch (RouteInitPlanningException)
            {
                throw;
            }
            catch (Exception exception) when (exception is UnauthorizedAccessException or IOException)
            {
                throw new RouteInitPlanningException(
                    RouteInitFindingCode.InspectionIncomplete,
                    exception.Message,
                    incomplete: true);
            }
        }

        return results;
    }

    private static RouteInitResultFormation BuildPreview(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload? payload,
        IntendedChain intended,
        GeneratedNavigationProjection projection,
        RouteInitProspectiveEffectPlan effects,
        FrameworkLifecycleState? intendedLifecycle)
    {
        var entrypoints = intended.Entries.Select(entry => new RouteInitEntrypoint(
            entry.Chain.Id,
            entry.Source.Identity.CanonicalBasePath,
            SourceFormClassifier.IsCompatibilityEntrypoint(entry.Source.Base.Form)
                ? RouteInitEntrypointForm.Compatibility
                : RouteInitEntrypointForm.Canonical,
            entry.Chain.IsMissing ? RouteInitEntrypointCurrent.Missing : RouteInitEntrypointCurrent.Existing,
            entry.Ownership,
            entry.Chain.IsMissing ? entry.Metadata : null,
            entry.SourceAssetPath,
            entry.Chain.IsMissing ? RouteInitEntrypointOutcome.Planned : RouteInitEntrypointOutcome.Unchanged)).ToArray();
        var resultEffects = BuildResultEffects(request, intended, projection, effects);
        var unchanged = entrypoints
            .Where(entrypoint => entrypoint.Outcome == RouteInitEntrypointOutcome.Unchanged)
            .Select(entrypoint => entrypoint.Path)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var findings = entrypoints.Any(entrypoint => entrypoint.Current == RouteInitEntrypointCurrent.Missing
                && entrypoint.Metadata?.Tags.Contains("NeedsAuthoring", StringComparer.Ordinal) == true)
            ? new[]
            {
                new RouteInitFinding(
                    RouteInitFindingCode.NeedsAuthoring,
                    "One or more new Route Init entrypoints require authoring.",
                    target.Id),
            }
            : [];
        var lifecycleAction = request.Scaffold == RouteInitScaffold.Framework
            ? intendedLifecycle is null || effects.FileChanges.All(change => !IsLifecyclePath(request, change.LogicalPath))
                ? RouteInitLifecycleAction.Preserve
                : RouteInitLifecycleAction.Publish
            : RouteInitLifecycleAction.None;
        var lifecycleOutcome = lifecycleAction switch
        {
            RouteInitLifecycleAction.None => RouteInitLifecycleOutcome.NotRequested,
            RouteInitLifecycleAction.Preserve => RouteInitLifecycleOutcome.AlreadyCurrent,
            RouteInitLifecycleAction.Publish => RouteInitLifecycleOutcome.Planned,
            _ => throw new ArgumentOutOfRangeException(nameof(lifecycleAction)),
        };
        return new RouteInitResultFormation(
            request.Workspace,
            request.Mode,
            request.Scaffold,
            new RouteInitTarget(target.Requested, target.Id, target.CanonicalPath),
            new RouteInitPlanFacts(RouteInitPlanCompleteness.Complete, RouteInitPlanSafety.Safe),
            alignment is null || payload is null
                ? null
                : new RouteInitFramework(
                    payload.InventoryFingerprint,
                    alignment.Segments.Select(segment => new RouteInitFrameworkSegment(
                        segment.IntendedSource.Identity.CanonicalBasePath,
                        segment.Role,
                        segment.SourceAssetPath))),
            entrypoints,
            resultEffects,
            unchanged,
            new RouteInitLifecycle(lifecycleAction, lifecycleOutcome),
            new RouteInitRecovery(
                effects.RecoveryTargets.Count == 0
                    ? RouteInitRecoveryState.NotCreated
                    : RouteInitRecoveryState.NotCreated,
                ResidualPath: null),
            RouteInitVerificationState.NotRequested,
            findings);
    }

    private static IReadOnlyList<RouteInitEffect> BuildResultEffects(
        RouteInitRequest request,
        IntendedChain intended,
        GeneratedNavigationProjection projection,
        RouteInitProspectiveEffectPlan effects)
    {
        var result = new List<RouteInitEffect>();
        result.AddRange(effects.DirectoryCreations.Select(directory => new RouteInitEffect(
            Relative(request, directory.LogicalPath),
            RouteInitEffectKind.Directory,
            RouteInitEffectAction.Create,
            SourceAssetPath: null,
            Change: null,
            RouteInitEffectOutcome.Planned,
            RouteInitEffectResidual.None)));
        var intendedByLogical = intended.Entries.ToDictionary(
            entry => entry.Content.Before.LogicalPath,
            entry => entry,
            PathComparer());
        var regionsByLogical = projection.Regions.ToDictionary(
            region => region.PhysicalPath,
            region => region,
            PathComparer());
        foreach (var change in effects.FileChanges)
        {
            if (IsLifecyclePath(request, change.LogicalPath))
            {
                continue;
            }

            if (change.Kind == PlannedFileChangeKind.Create
                && intendedByLogical.TryGetValue(change.LogicalPath, out var entry))
            {
                result.Add(new RouteInitEffect(
                    entry.Source.Identity.CanonicalBasePath,
                    RouteInitEffectKind.Entrypoint,
                    RouteInitEffectAction.Create,
                    SourceAssetPath: null,
                    new RouteInitEffectChange(null, StrictUtf8.GetString(change.IntendedBytes.AsSpan())),
                    RouteInitEffectOutcome.Planned,
                    RouteInitEffectResidual.None));
                continue;
            }

            var region = regionsByLogical.Values.FirstOrDefault(candidate => PathComparer().Equals(
                candidate.PhysicalPath,
                change.Expectation.PhysicalPath));
            if (region?.Change is not { } bounded)
            {
                throw new InvalidOperationException("A generated Route Init replacement requires one bounded projected change.");
            }

            result.Add(new RouteInitEffect(
                region.CanonicalPath,
                RouteInitEffectKind.GeneratedRegion,
                RouteInitEffectAction.Replace,
                SourceAssetPath: null,
                new RouteInitEffectChange(bounded.BeforeBody, bounded.ExpectedBody),
                RouteInitEffectOutcome.Planned,
                RouteInitEffectResidual.None));
        }

        return result;
    }

    private RouteInitPlanBuild? ReadCatalogueBoundary(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitCurrentCatalogueFacts current)
    {
        var issue = current.Catalogue.Issues.FirstOrDefault(candidate =>
            candidate.Code != OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.RootMissing
            || request.Scaffold == RouteInitScaffold.Framework);
        if (issue is null)
        {
            return null;
        }

        var (code, incomplete) = issue.Code switch
        {
            OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.IdentityCollision
                or OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.PhysicalAlias
                or OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.OrphanOverwrite => (RouteInitFindingCode.IdentityCollision, false),
            OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.RootUnsafe
                or OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.CandidateUnsafe => (RouteInitFindingCode.TargetUnsafe, false),
            OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogueIssueCode.RootMissing when request.Scaffold == RouteInitScaffold.Framework => (RouteInitFindingCode.FrameworkInstallRequired, false),
            _ => (RouteInitFindingCode.InspectionIncomplete, true),
        };
        return Stop(
            request,
            target,
            code,
            $"The source catalogue boundary '{issue.AttemptedCanonicalPath}' is not safe and complete.",
            incomplete);
    }

    private static RouteInitPlanBuild? ReadCurrentBoundary(
        RouteInitRequest request,
        RouteInitCurrentStateFacts current,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload? payload)
    {
        var ambiguous = current.Chain.FirstOrDefault(entry => entry.IsAmbiguous);
        if (ambiguous is not null)
        {
            return Stop(request, current.Target, RouteInitFindingCode.RouteAmbiguous, "A route folder has more than one recognized entrypoint.", false, alignment: alignment, payload: payload);
        }

        var collision = current.Chain.FirstOrDefault(entry => entry.IsMissing && entry.IdentityOccupants.Count > 0);
        if (collision is not null)
        {
            return Stop(request, current.Target, RouteInitFindingCode.IdentityCollision, "A route identity is occupied by a non-entrypoint source.", false, alignment: alignment, payload: payload);
        }

        foreach (var read in current.Reads.SelectMany(ReadLayers))
        {
            if (read.Verification.State == SourceLayerVerificationState.Cancelled
                || read.Read?.State == FileReadState.Cancelled)
            {
                return Stop(request, current.Target, RouteInitFindingCode.Interrupted, "Route Init source inspection was cancelled.", true, alignment: alignment, payload: payload);
            }

            if (read.Verification.State == SourceLayerVerificationState.Unsafe)
            {
                var code = read.Layer.Form == SourceDocumentForm.Loader
                    ? RouteInitFindingCode.LoaderUnsafe
                    : RouteInitFindingCode.TargetUnsafe;
                return Stop(request, current.Target, code, "A required Route Init source has an unsafe physical boundary.", false, alignment: alignment, payload: payload);
            }

            if (read.Verification.State != SourceLayerVerificationState.Verified
                || read.Read?.State != FileReadState.Complete)
            {
                return Stop(request, current.Target, RouteInitFindingCode.InspectionIncomplete, "A required Route Init source could not be read completely.", true, alignment: alignment, payload: payload);
            }
        }

        return null;
    }

    private static RouteInitPlanBuild? ReadExactCompatibilityBoundary(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitCurrentCatalogueFacts current,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload? payload)
    {
        if (target.Kind != RouteInitTargetKind.ExactPath
            || target.CanonicalPath is not { } requestedPath
            || !SourceFormClassifier.TryClassify(requestedPath, out var requestedForm)
            || !SourceFormClassifier.IsCompatibilityEntrypoint(requestedForm))
        {
            return null;
        }

        var exactExists = target.Id is { } targetId
            && current.Catalogue.FindAllById(targetId).Any(source =>
                SourceFormClassifier.IsEntrypoint(source.Base.Form)
                && string.Equals(
                    source.Identity.CanonicalBasePath,
                    requestedPath,
                    StringComparison.Ordinal));
        if (exactExists)
        {
            return null;
        }

        return Stop(
            request,
            target,
            RouteInitFindingCode.InvalidTarget,
            "An exact compatibility Route Init target must already exist at the requested path.",
            incomplete: true,
            alignment: alignment,
            payload: payload);
    }

    private static RouteInitPlanBuild? ReadRecoveryBoundary(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload? payload,
        RecoveryBundleCatalogueResult recovery)
    {
        return recovery.State switch
        {
            RecoveryBundleCatalogueState.Available when recovery.Candidates.Length == 0 => null,
            RecoveryBundleCatalogueState.Available => Stop(
                request,
                target,
                RouteInitFindingCode.RecoveryConflict,
                "A recognized recovery candidate conflicts with this Route Init plan.",
                incomplete: false,
                alignment: alignment,
                payload: payload),
            RecoveryBundleCatalogueState.Unavailable => Stop(
                request,
                target,
                RouteInitFindingCode.RecoveryUnavailable,
                recovery.Cause ?? "The Route Init recovery catalogue is unavailable.",
                incomplete: true,
                alignment: alignment,
                payload: payload),
            RecoveryBundleCatalogueState.Cancelled => Stop(
                request,
                target,
                RouteInitFindingCode.Interrupted,
                "Route Init recovery inspection was interrupted.",
                incomplete: true,
                alignment: alignment,
                payload: payload),
            _ => throw new ArgumentOutOfRangeException(
                nameof(recovery),
                recovery.State,
                "The recovery catalogue state is not defined."),
        };
    }

    private static IEnumerable<SourceDocumentReadResult> ReadLayers(RouteInitCurrentSourceRead source)
    {
        yield return source.Base;
        if (source.Overwrite is not null)
        {
            yield return source.Overwrite;
        }
    }

    private static RouteInitPlanBuild StopForTrust(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitFrameworkTrust trust,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload payload)
    {
        var (code, incomplete) = trust.State switch
        {
            RouteInitFrameworkTrustState.InstallRequired => (RouteInitFindingCode.FrameworkInstallRequired, false),
            RouteInitFrameworkTrustState.UpdateRequired => (RouteInitFindingCode.FrameworkUpdateRequired, false),
            RouteInitFrameworkTrustState.Incomplete => (RouteInitFindingCode.LifecycleUnavailable, true),
            RouteInitFrameworkTrustState.Cancelled => (RouteInitFindingCode.Interrupted, true),
            RouteInitFrameworkTrustState.Blocked => (RouteInitFindingCode.LifecycleBlocked, false),
            _ => (RouteInitFindingCode.LifecycleBlocked, false),
        };
        return Stop(request, target, code, trust.Cause ?? "The trusted Framework lifecycle boundary is unavailable.", incomplete, alignment: alignment, payload: payload);
    }

    private static RouteInitPlanBuild StopForPreflight(
        RouteInitRequest request,
        RouteInitTargetFacts target,
        RouteInitFrameworkAlignment? alignment,
        FrameworkPayload? payload,
        MutationValidationResult preflight)
    {
        var (code, incomplete) = preflight.State switch
        {
            MutationValidationState.Mismatched => (RouteInitFindingCode.TargetChanged, false),
            MutationValidationState.Blocked => (RouteInitFindingCode.TargetUnsafe, false),
            MutationValidationState.Failed => (RouteInitFindingCode.InspectionIncomplete, true),
            MutationValidationState.Cancelled => (RouteInitFindingCode.Interrupted, true),
            _ => (RouteInitFindingCode.OperationFailed, false),
        };
        return Stop(request, target, code, preflight.Cause ?? "Route Init preflight did not complete.", incomplete, alignment: alignment, payload: payload);
    }

    private static RouteInitPlanBuild Stop(
        RouteInitRequest request,
        RouteInitTargetFacts? target,
        RouteInitFindingCode code,
        string cause,
        bool incomplete,
        string? requested = null,
        RouteInitFrameworkAlignment? alignment = null,
        FrameworkPayload? payload = null)
    {
        var targetFacts = new RouteInitTarget(
            requested ?? target?.Requested ?? request.RouteTarget,
            target?.Id,
            target?.CanonicalPath);
        var formation = new RouteInitResultFormation(
            request.Workspace,
            request.Mode,
            request.Scaffold,
            targetFacts,
            new RouteInitPlanFacts(
                incomplete ? RouteInitPlanCompleteness.Incomplete : RouteInitPlanCompleteness.Complete,
                incomplete ? RouteInitPlanSafety.Safe : RouteInitPlanSafety.Blocked),
            alignment is null || payload is null
                ? null
                : new RouteInitFramework(
                    payload.InventoryFingerprint,
                    alignment.Segments.Select(segment => new RouteInitFrameworkSegment(
                        segment.IntendedSource.Identity.CanonicalBasePath,
                        segment.Role,
                        segment.SourceAssetPath))),
            entrypoints: [],
            effects: [],
            unchangedPaths: [],
            new RouteInitLifecycle(
                request.Scaffold == RouteInitScaffold.Framework
                    ? RouteInitLifecycleAction.Preserve
                    : RouteInitLifecycleAction.None,
                request.Scaffold == RouteInitScaffold.Framework
                    ? RouteInitLifecycleOutcome.NotStarted
                    : RouteInitLifecycleOutcome.NotRequested),
            new RouteInitRecovery(RouteInitRecoveryState.NotRequired, ResidualPath: null),
            RouteInitVerificationState.NotRequested,
            [new RouteInitFinding(code, cause, target?.Id ?? request.RouteTarget)]);
        return new RouteInitPlanBuild(Plan: null, formation);
    }

    private static FileStateSnapshot MissingSnapshot(RouteInitRequest request, string canonicalPath)
        => FileStateSnapshot.Missing(SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, canonicalPath));

    private static SourceLogicalSource CreateSource(RouteInitRequest request, string id, string canonicalPath)
    {
        if (!SourceFormClassifier.TryClassify(canonicalPath, out var form))
        {
            throw new RouteInitPlanningException(RouteInitFindingCode.InvalidTarget, "The intended route source form is invalid.", false);
        }

        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            new SourceLayer(
                canonicalPath,
                Path.GetFullPath(SourceLogicalPath.ToLexicalPath(request.Workspace.PhysicalRoot, canonicalPath)),
                form,
                SourceLayerKind.Base));
    }

    private static bool HasMetadataInput(RouteInitMetadataInput input)
        => input.Description is not null
            || input.ResponsibilitySpecified
            || input.Tags.Count > 0;

    private static bool IsValidMetadataInput(RouteInitRequest request)
    {
        var input = request.Metadata;
        if (request.Scaffold == RouteInitScaffold.Framework && HasMetadataInput(input))
        {
            return false;
        }

        if (input.Description is not null && string.IsNullOrWhiteSpace(input.Description))
        {
            return false;
        }

        if (input.ResponsibilitySpecified
            && input.Responsibility is { Length: > 0 }
            && string.IsNullOrWhiteSpace(input.Responsibility))
        {
            return false;
        }

        return input.Tags.All(FrameworkDocumentMetadataTagGrammar.IsValid)
            && input.Tags.Distinct(StringComparer.Ordinal).Count() == input.Tags.Count;
    }

    private static RouteInitFindingCode ReadTopologyFinding(RouteInitProspectiveTopology topology)
        => topology.Formation.IntendedTargetCollisions.Count > 0
            ? RouteInitFindingCode.IdentityCollision
            : topology.Formation.Ambiguities.Any(ambiguity => ambiguity.Kind == OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation.GeneratedNavigationFormationAmbiguityKind.PhysicalAlias)
                ? RouteInitFindingCode.TargetUnsafe
                : RouteInitFindingCode.RouteAmbiguous;

    private static bool IsLifecyclePath(RouteInitRequest request, string logicalPath)
        => PathComparer().Equals(
            logicalPath,
            SourceLogicalPath.ToLexicalPath(request.Workspace.LexicalRoot, ".agents/open-forge.lifecycle.json"));

    private static string Relative(RouteInitRequest request, string logicalPath)
        => Path.GetRelativePath(request.Workspace.LexicalRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/');

    private static StringComparer PathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private sealed record IntendedEntrypoint
    {
        internal IntendedEntrypoint(
            RouteInitCurrentChainEntry chain,
            SourceLogicalSource source,
            FileStateSnapshot before,
            ReadOnlySpan<byte> intendedBytes,
            RouteInitMetadata? metadata,
            string? sourceAssetPath,
            RouteInitEntrypointOwnership ownership,
            bool ownsGeneratedEntries)
        {
            Chain = chain;
            Source = source;
            Metadata = metadata;
            SourceAssetPath = sourceAssetPath;
            Ownership = ownership;
            Content = new RouteInitProspectiveSourceContent(
                source,
                before,
                intendedBytes,
                sourceAssetPath,
                ownsGeneratedEntries);
        }

        internal RouteInitCurrentChainEntry Chain { get; }

        internal SourceLogicalSource Source { get; }

        internal RouteInitProspectiveSourceContent Content { get; }

        internal RouteInitMetadata? Metadata { get; }

        internal string? SourceAssetPath { get; }

        internal RouteInitEntrypointOwnership Ownership { get; }
    }

    private sealed record IntendedChain(IReadOnlyList<IntendedEntrypoint> Entries);

    private sealed record FrameworkPlanningBasis(
        RouteInitFrameworkAlignment Alignment,
        FrameworkPayload Payload,
        RouteInitFrameworkTrust Trust);

    private sealed record ProjectionBoundary(RouteInitFindingCode Code, string Cause, bool Incomplete);

    private sealed record ProjectionInputs(
        IReadOnlyList<GeneratedNavigationRegionInput> Regions,
        IReadOnlyList<GeneratedNavigationMetadata> Metadata,
        ProjectionBoundary? Boundary)
    {
        internal static ProjectionInputs Complete(
            IReadOnlyList<GeneratedNavigationRegionInput> regions,
            IReadOnlyList<GeneratedNavigationMetadata> metadata)
            => new(regions, metadata, Boundary: null);

        internal static ProjectionInputs Blocked(RouteInitFindingCode code, string cause, bool incomplete)
            => new([], [], new ProjectionBoundary(code, cause, incomplete));
    }

}
