using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
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
using OpenForge.Cli.Core.Framework.Sources.Sharing;
using OpenForge.Cli.Core.Commands.Shared.ReferenceMaintenance;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

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
            intended.Entries.Select(entry => entry.Source).Concat(
                inspection.Restoration?.Files.Where(file => file.Before.Kind == FileExpectationKind.Missing && file.Source is not null)
                    .Select(file => file.Source ?? throw new InvalidOperationException("A routed payload file requires its source.")) ?? []));
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
            inspection.Current,
            inspection.Restoration);
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
            projectionInputs.Metadata,
            new SourceSharing(inspection.SharingOwnership?.Document.Framework?.GitIgnoredRoutes ?? []));
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

        if (ReadOutsideEffectBoundary(inspection, intended, projection) is { } protection)
            return Stopped(inspection, RouteInitFindingCode.TargetUnsafe, protection, incomplete: false);

        IReadOnlyList<RouteInitProspectiveSourceContent> effectSources;
        var readonlySnapshots = ImmutableArray<FileStateSnapshot>.Empty;
        try
        {
            effectSources = await BuildEffectSourcesAsync(
                    request,
                    topology,
                    intended,
                    inspection.Current,
                    cancellationToken,
                    inspection.Restoration)
                .ConfigureAwait(false);
            if (inspection.Target.CanonicalPath is { } path
                && inspection.Catalogue.Catalogue.Layout.TryGetMaintenanceBoundary(path, out var boundary)
                && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside)
            {
                var snapshots = ImmutableArray.CreateBuilder<FileStateSnapshot>();
                foreach (var read in inspection.Current.Reads)
                {
                    snapshots.Add(await ReadSourceSnapshotAsync(request, read.Base, cancellationToken).ConfigureAwait(false));
                    if (read.Overwrite is { } overwrite)
                        snapshots.Add(await ReadSourceSnapshotAsync(request, overwrite, cancellationToken).ConfigureAwait(false));
                }
                readonlySnapshots = snapshots.ToImmutable();
            }
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
            directoryStates = ReadMissingDirectories(inspection.Catalogue.Catalogue.Layout, intended, inspection.Restoration);
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
            ownershipEffect: null);
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
            effectPlan)
        { ReadonlySnapshots = readonlySnapshots });
    }

    private static string? ReadOutsideEffectBoundary(RouteInitInspectionFacts inspection,
        RouteInitIntendedChain intended, GeneratedNavigationProjection projection)
    {
        var layout = inspection.Catalogue.Catalogue.Layout;
        if (inspection.Target.CanonicalPath is not { } path || !layout.TryGetMaintenanceBoundary(path, out var boundary)
            || boundary.Kind != WorkspaceMaintenanceBoundaryKind.Outside) return null;
        var policy = ReferenceProtectionReader.Read(inspection.LayoutRead).Policy;
        if (policy is null) return "The outside Route Init protection policy is unavailable.";
        foreach (var entry in intended.Entries)
        {
            var entryPath = entry.Source.Identity.CanonicalBasePath;
            if (!entry.Chain.IsMissing && !projection.Regions.Any(region => region.CanonicalPath == entryPath
                && region.Change?.RequiresUpdate == true)) continue;
            if (WorkspaceRemovals.IsPathRemoved(layout, entryPath, policy.Settings)
                || !new SourceSharing(policy.Ownership.Framework?.GitIgnoredRoutes ?? []).Includes(entryPath))
                return "The outside Route Init effect is excluded by workspace policy.";
            var protection = new RoutedMetadataCompletionPolicy().Protection(entryPath, policy);
            var boundedEntries = !entry.Chain.IsMissing
                && protection is RoutedMetadataEligibility.ManagedContent or RoutedMetadataEligibility.GeneratedContent;
            if (protection != RoutedMetadataEligibility.Eligible && !boundedEntries
                || policy.Ownership.Libraries.Any(library => library.Paths.Contains(entryPath, StringComparer.Ordinal)))
                return "The outside Route Init effect is protected by workspace policy.";
        }
        return null;
    }

    private async ValueTask<IReadOnlyList<RouteInitProspectiveSourceContent>> BuildEffectSourcesAsync(
        RouteInitRequest request,
        RouteInitProspectiveTopology topology,
        RouteInitIntendedChain intended,
        RouteInitCurrentStateFacts current,
        CancellationToken cancellationToken,
        RouteInitRestoration? restoration)
    {
        var sources = intended.Entries
            .Select(entry => entry.Content)
            .ToList();
        if (restoration is not null)
        {
            sources.AddRange(restoration.Files.Where(file => file.Before.Kind == FileExpectationKind.Missing && file.Source is not null)
                .Select(file => new RouteInitProspectiveSourceContent(
                    file.Source ?? throw new InvalidOperationException("A routed payload file requires its source."),
                    file.Before, file.IntendedBytes.Span, file.Asset.Path, ownsGeneratedEntries: false)));
        }
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
        RouteInitCurrentStateFacts current,
        RouteInitRestoration? restoration)
    {
        var documents = new Dictionary<string, string>(StringComparer.Ordinal);
        if (restoration is not null)
        {
            foreach (var file in restoration.Files.Where(file => file.Before.Kind == FileExpectationKind.Missing && file.Source is not null))
            {
                documents.Add(file.TargetPath, StrictUtf8.GetString(file.IntendedBytes.Span));
            }
        }
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
        WorkspaceLayout layout,
        RouteInitIntendedChain intended,
        RouteInitRestoration? restoration)
    {
        var requestWorkspace = layout.Workspace;
        var canonicalDirectories = new List<string>();
        foreach (var entry in intended.Entries)
        {
            canonicalDirectories.Add(
                SourceLogicalPath.ReadParent(entry.Source.Identity.CanonicalBasePath));
            if (layout.TryGetMaintenanceBoundary(entry.Source.Identity.CanonicalBasePath, out var boundary)
                && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside)
                canonicalDirectories.Add(boundary.DirectoryPath);
            else
                canonicalDirectories.Add(layout.SourceRoots.Single(root =>
                    entry.Source.Identity.CanonicalBasePath.StartsWith(root + "/", StringComparison.Ordinal)));
        }
        if (restoration is not null)
        {
            foreach (var file in restoration.Files)
            {
                var parent = SourceLogicalPath.ReadParent(file.TargetPath);
                while (layout.IsImplicitContentDestination(parent))
                {
                    canonicalDirectories.Add(parent);
                    parent = SourceLogicalPath.ReadParent(parent);
                }
            }
        }

        foreach (var directory in canonicalDirectories.ToArray())
        {
            var parent = SourceLogicalPath.ReadParent(directory);
            while (layout.IsImplicitContentDestination(parent)
                || layout.TryGetMaintenanceBoundary(parent, out var boundary)
                    && boundary.Kind == WorkspaceMaintenanceBoundaryKind.Outside)
            {
                canonicalDirectories.Add(parent);
                parent = SourceLogicalPath.ReadParent(parent);
            }
        }

        var results = new List<FileStateSnapshot>();
        foreach (var canonical in canonicalDirectories.Distinct(StringComparer.Ordinal)
            .OrderBy(path => path.Count(character => character == '/')).ThenBy(path => path, StringComparer.Ordinal))
        {
            var logical = SourceLogicalPath.ToLexicalPath(
                requestWorkspace.LexicalRoot,
                canonical);
            var resolution = _physicalPathResolver.ResolveCandidate(
                requestWorkspace.LexicalRoot,
                requestWorkspace.PhysicalRoot,
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
