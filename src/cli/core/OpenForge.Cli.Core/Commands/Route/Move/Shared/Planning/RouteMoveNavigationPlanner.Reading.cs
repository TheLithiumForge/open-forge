using System.Text;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal sealed partial class RouteMoveNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private NavigationMetadataRead ReadMetadata(
        RouteMoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation)
    {
        var values = new List<GeneratedNavigationMetadata>();
        foreach (var source in formation.Sources)
        {
            var read = ReadSourceText(request, source);
            if (read is null)
            {
                return NavigationMetadataRead.Stop(StopFormation(
                    request,
                    RouteMoveFindingCode.ProjectionIncomplete,
                    CliSemanticStatus.Incomplete,
                    source.Identity.CanonicalBasePath,
                    "Complete authored metadata could not be read for intended route topology."));
            }

            var facts = _metadataParser.Parse(_markdownParser.Parse(read.Text), source.Base.Form);
            values.Add(new GeneratedNavigationMetadata(source, facts));
        }

        return NavigationMetadataRead.Complete(values);
    }

    private RegionDocumentRead ReadDocuments(
        RouteMoveNavigationPlanningRequest request,
        IEnumerable<SelectedRegion> regions)
    {
        var values = new List<RegionDocument>();
        foreach (var region in regions)
        {
            var read = ReadSourceText(request, region.Source);
            if (read is null)
            {
                return RegionDocumentRead.Stop(StopFormation(
                    request,
                    RouteMoveFindingCode.GeneratedRegionUnsafe,
                    CliSemanticStatus.Blocked,
                    region.Source.Identity.CanonicalBasePath,
                    "An applicable generated navigation source could not retain exact UTF-8 bytes."));
            }

            values.Add(new RegionDocument(region, read.Text, read.Snapshot));
        }

        return RegionDocumentRead.Complete(values);
    }

    private static SourceText? ReadSourceText(
        RouteMoveNavigationPlanningRequest request,
        SourceLogicalSource source)
    {
        var destination = request.Destination;
        var workspace = destination.Inventory.Subject.Request.Workspace;
        var moved = RouteMoveReferenceChangeProjector.BuildMovedPathMap(destination);
        var originalPath = moved.FirstOrDefault(pair => string.Equals(
            pair.Value,
            source.Identity.CanonicalBasePath,
            StringComparison.Ordinal)).Key;
        var snapshot = ReadMovedSnapshot(destination, workspace.LexicalRoot, originalPath);
        try
        {
            snapshot ??= FileStateSnapshot.File(
                Path.Combine(
                    workspace.LexicalRoot,
                    source.Identity.CanonicalBasePath.Replace('/', Path.DirectorySeparatorChar)),
                source.Base.PhysicalPath,
                File.ReadAllBytes(source.Base.PhysicalPath));
            return new SourceText(StrictUtf8.GetString(snapshot.Bytes.AsSpan()), snapshot);
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or UnauthorizedAccessException
            or IOException)
        {
            return null;
        }
    }

    private static FileStateSnapshot? ReadMovedSnapshot(
        RouteMoveResolvedDestination destination,
        string workspaceRoot,
        string? originalPath)
        => originalPath is null
            ? null
            : destination.Inventory.Items.FirstOrDefault(item => string.Equals(
                Canonical(workspaceRoot, item.SourcePath),
                originalPath,
                StringComparison.Ordinal))?.Snapshot;

    private static string Canonical(string workspaceRoot, string logicalPath)
        => Path.GetRelativePath(workspaceRoot, logicalPath)
            .Replace(Path.DirectorySeparatorChar, '/');

    private sealed record SourceText(string Text, FileStateSnapshot Snapshot);

    private sealed record RegionDocumentRead(
        IReadOnlyList<RegionDocument> Values,
        RouteMoveResultFormation? Boundary)
    {
        internal static RegionDocumentRead Complete(IReadOnlyList<RegionDocument> values)
            => new(values, Boundary: null);

        internal static RegionDocumentRead Stop(RouteMoveResultFormation boundary)
            => new([], boundary);
    }

    private sealed record NavigationMetadataRead(
        IReadOnlyList<GeneratedNavigationMetadata> Values,
        RouteMoveResultFormation? Boundary)
    {
        internal static NavigationMetadataRead Complete(IReadOnlyList<GeneratedNavigationMetadata> values)
            => new(values, Boundary: null);

        internal static NavigationMetadataRead Stop(RouteMoveResultFormation boundary)
            => new([], boundary);
    }
}
