using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitProspectivePlanBuilder
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private readonly RouteInitProspectiveTopologyPlanner _topologyPlanner = new();
    private readonly SourceDocumentSnapshotReader _snapshotReader = new();
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver = new();

    internal async ValueTask<RouteInitProspectivePlanResult> BuildAsync(
        RouteInitRequest request,
        RouteInitInspectionFacts inspection,
        RouteInitIntendedChain intended,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(inspection);
        ArgumentNullException.ThrowIfNull(intended);

        var topology = _topologyPlanner.Build(
            inspection.Catalogue,
            intended.Entries.Select(entry => entry.Source));
        if (!topology.IsSafeForProjection)
        {
            return Stopped(
                inspection,
                ReadTopologyFinding(topology),
                "The prospective route topology is ambiguous, colliding, or unsafe.",
                incomplete: false);
        }

        var projectionInputs = BuildProjectionInputs(
            topology,
            intended,
            inspection.Current);
        if (projectionInputs.Boundary is { } projectionInputBoundary)
        {
            return Stopped(
                inspection,
                projectionInputBoundary.Code,
                projectionInputBoundary.Cause,
                projectionInputBoundary.Incomplete);
        }

        var projectionBuild = _topologyPlanner.Project(
            topology,
            projectionInputs.Regions,
            projectionInputs.Metadata);
        if (projectionBuild.State != RouteInitProspectiveProjectionState.Complete
            || projectionBuild.Projection is not { } projection)
        {
            var (code, incomplete) = ReadProjectionFinding(projectionBuild);
            return Stopped(
                inspection,
                code,
                projectionBuild.Cause ?? "The generated navigation projection is unavailable.",
                incomplete);
        }

        IReadOnlyList<RouteInitProspectiveSourceContent> effectSources;
        try
        {
            effectSources = await BuildEffectSourcesAsync(
                    request,
                    topology,
                    intended,
                    inspection.Current,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (RouteInitPlanningException exception)
        {
            return Stopped(
                inspection,
                exception.Code,
                exception.Message,
                exception.Incomplete);
        }

        IReadOnlyList<FileStateSnapshot> directoryStates;
        try
        {
            directoryStates = ReadMissingDirectories(request, intended);
        }
        catch (RouteInitPlanningException exception)
        {
            return Stopped(
                inspection,
                exception.Code,
                exception.Message,
                exception.Incomplete);
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
            return Stopped(
                inspection,
                RouteInitFindingCode.GeneratedRegionUnsafe,
                initialEffects.Cause ?? "The Route Init effects are unsafe.",
                incomplete: false);
        }

        return new RouteInitProspectivePlanCompleted(new RouteInitProspectivePlanFacts(
            topology,
            projection,
            directoryStates,
            effectSources,
            effectPlan));
    }

    private async ValueTask<IReadOnlyList<RouteInitProspectiveSourceContent>> BuildEffectSourcesAsync(
        RouteInitRequest request,
        RouteInitProspectiveTopology topology,
        RouteInitIntendedChain intended,
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
        var before = await ReadSourceSnapshotAsync(request, currentLoader.Base, cancellationToken)
            .ConfigureAwait(false);
        sources.Add(new RouteInitProspectiveSourceContent(
            loader,
            before,
            before.Bytes.AsSpan(),
            sourceAssetPath: null,
            ownsGeneratedEntries: false));
        return sources;
    }

    private async ValueTask<FileStateSnapshot> ReadSourceSnapshotAsync(
        RouteInitRequest request,
        SourceDocumentReadResult read,
        CancellationToken cancellationToken)
    {
        if (read.Verification.State != SourceLayerVerificationState.Verified
            || read.Verification.CurrentPhysicalPath is null)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                "The exact current source snapshot is unavailable.",
                incomplete: true);
        }

        try
        {
            return await _snapshotReader.ReadAsync(request.Workspace, read, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.Interrupted,
                "Route Init source snapshotting was cancelled.",
                incomplete: true);
        }
        catch (Exception exception) when (exception is UnauthorizedAccessException
            or IOException
            or DecoderFallbackException)
        {
            throw new RouteInitPlanningException(
                RouteInitFindingCode.InspectionIncomplete,
                exception.Message,
                incomplete: true);
        }
    }

    private ProjectionInputs BuildProjectionInputs(
        RouteInitProspectiveTopology topology,
        RouteInitIntendedChain intended,
        RouteInitCurrentStateFacts current)
    {
        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in intended.Entries)
        {
            try
            {
                documents.Add(
                    entry.Source.Identity.CanonicalBasePath,
                    StrictUtf8.GetString(entry.Content.IntendedBytes.AsSpan()));
            }
            catch (DecoderFallbackException exception)
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.FrameworkPayloadInvalid,
                    exception.Message,
                    incomplete: false);
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
        foreach (var source in topology.Formation.Sources.Where(source =>
                     source.Base.Form != SourceDocumentForm.Loader))
        {
            if (!documents.TryGetValue(source.Identity.CanonicalBasePath, out var sourceText))
            {
                continue;
            }

            var facts = _metadataParser.Parse(
                _markdownParser.Parse(sourceText),
                source.Base.Form);
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
                ?? throw new InvalidOperationException(
                    "A prospective region source must belong to the formation.");
            if (!documents.TryGetValue(path, out var sourceText))
            {
                return ProjectionInputs.Blocked(
                    RouteInitFindingCode.InspectionIncomplete,
                    $"The projection source '{path}' is unavailable.",
                    incomplete: true);
            }

            regions.Add(new GeneratedNavigationRegionInput(
                source,
                _markdownParser.Parse(sourceText)));
        }

        return ProjectionInputs.Complete(regions, metadata);
    }

    private IReadOnlyList<FileStateSnapshot> ReadMissingDirectories(
        RouteInitRequest request,
        RouteInitIntendedChain intended)
    {
        var canonicalDirectories = new List<string> { SourceLogicalPath.AgentsRoot };
        foreach (var entry in intended.Entries)
        {
            canonicalDirectories.Add(
                SourceLogicalPath.ReadParent(entry.Source.Identity.CanonicalBasePath));
        }

        var results = new List<FileStateSnapshot>();
        foreach (var canonical in canonicalDirectories.Distinct(StringComparer.Ordinal))
        {
            var logical = SourceLogicalPath.ToLexicalPath(
                request.Workspace.LexicalRoot,
                canonical);
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
                if ((attributes & (FileAttributes.Directory
                        | FileAttributes.ReparsePoint
                        | FileAttributes.Device)) != FileAttributes.Directory)
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

    private static (RouteInitFindingCode Code, bool Incomplete) ReadProjectionFinding(
        RouteInitProspectiveProjection projection)
    {
        if (projection.State == RouteInitProspectiveProjectionState.Unavailable)
        {
            return (RouteInitFindingCode.ProjectionIncomplete, true);
        }

        var loaderBlocked = projection.Projection?.Regions.Any(region =>
            region.State != GeneratedNavigationRegionState.Available
            && region.Source.Base.Form == SourceDocumentForm.Loader) == true;
        return loaderBlocked
            ? (RouteInitFindingCode.LoaderUnsafe, false)
            : (RouteInitFindingCode.GeneratedRegionUnsafe, false);
    }

    private static RouteInitFindingCode ReadTopologyFinding(
        RouteInitProspectiveTopology topology)
    {
        if (topology.Formation.IntendedTargetCollisions.Count > 0)
        {
            return RouteInitFindingCode.IdentityCollision;
        }

        if (topology.Formation.Ambiguities.Any(ambiguity =>
                ambiguity.Kind == GeneratedNavigationFormationAmbiguityKind.PhysicalAlias))
        {
            return RouteInitFindingCode.TargetUnsafe;
        }

        return RouteInitFindingCode.RouteAmbiguous;
    }

    private static RouteInitProspectivePlanStopped Stopped(
        RouteInitInspectionFacts inspection,
        RouteInitFindingCode code,
        string cause,
        bool incomplete)
        => new(new RouteInitPlanningBoundary(
            code,
            cause,
            incomplete,
            inspection.Target,
            Alignment: inspection.Framework?.Alignment,
            Payload: inspection.Framework?.Payload));

    private sealed record ProjectionBoundary(
        RouteInitFindingCode Code,
        string Cause,
        bool Incomplete);

    private sealed record ProjectionInputs(
        IReadOnlyList<GeneratedNavigationRegionInput> Regions,
        IReadOnlyList<GeneratedNavigationMetadata> Metadata,
        ProjectionBoundary? Boundary)
    {
        internal static ProjectionInputs Complete(
            IReadOnlyList<GeneratedNavigationRegionInput> regions,
            IReadOnlyList<GeneratedNavigationMetadata> metadata)
            => new(regions, metadata, Boundary: null);

        internal static ProjectionInputs Blocked(
            RouteInitFindingCode code,
            string cause,
            bool incomplete)
            => new([], [], new ProjectionBoundary(code, cause, incomplete));
    }
}
