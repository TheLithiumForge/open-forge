using System.Text;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed partial class RouteRemoveNavigationPlanner
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);

    private NavigationMetadataRead ReadMetadata(
        RouteRemoveNavigationPlanningRequest request,
        GeneratedNavigationFormation formation,
        IReadOnlyList<SelectedRegion>? selected = null)
    {
        var values = new List<GeneratedNavigationMetadata>();
        var inputs = new List<FileStateSnapshot>();
        var needed = selected?.Select(region => region.Source.Base.CanonicalPath).ToHashSet(StringComparer.Ordinal);
        foreach (var source in formation.Sources.Where(source => request.Subject.OutsideRoute is null
            || needed is null || needed.Contains(source.Base.CanonicalPath)))
        {
            var read = ReadSourceText(request, source);
            if (read is null)
            {
                return NavigationMetadataRead.Stop(StopFormation(
                    request,
                    RouteRemoveFindingCode.ProjectionIncomplete,
                    CliSemanticStatus.Incomplete,
                    source.Identity.CanonicalBasePath,
                    "Complete authored metadata could not be read for intended route topology."));
            }

            values.Add(new GeneratedNavigationMetadata(
                source,
                _metadataParser.Parse(_markdownParser.Parse(read.Text), source.Base.Form)));
            inputs.Add(read.Snapshot);
        }

        return NavigationMetadataRead.Complete(values) with { Inputs = inputs };
    }

    private RegionDocumentRead ReadDocuments(
        RouteRemoveNavigationPlanningRequest request,
        IEnumerable<SelectedRegion> regions,
        bool applyRows = true)
    {
        var values = new List<RegionDocument>();
        foreach (var region in regions)
        {
            var read = ReadSourceText(request, region.Source);
            if (read is null)
            {
                return RegionDocumentRead.Stop(StopFormation(
                    request,
                    RouteRemoveFindingCode.GeneratedRegionUnsafe,
                    CliSemanticStatus.Blocked,
                    region.Source.Identity.CanonicalBasePath,
                    "An applicable generated navigation source could not retain exact UTF-8 bytes."));
            }

            var text = read.Text;
            if (applyRows && request.RowDocuments.SingleOrDefault(document => document.SourcePath == region.Source.Base.CanonicalPath) is { } rows)
            {
                if (rows.Snapshot.Expectation != read.Snapshot.Expectation
                    || !rows.Snapshot.Bytes.AsSpan().SequenceEqual(read.Snapshot.Bytes.AsSpan()))
                    return RegionDocumentRead.Stop(StopFormation(request, RouteRemoveFindingCode.TargetChanged,
                        CliSemanticStatus.Blocked, rows.SourcePath, "The outside row and generated projection inputs changed between reads."));
                text = RouteRemoveAuthoredRowProjector.Apply(text, rows.RowEdits);
            }
            values.Add(new RegionDocument(region, text, read.Snapshot));
        }

        return RegionDocumentRead.Complete(values);
    }

    private static SourceText? ReadSourceText(
        RouteRemoveNavigationPlanningRequest request,
        SourceLogicalSource source)
    {
        var workspace = request.Subject.Request.Workspace;
        try
        {
            if (request.Subject.OutsideRoute is not null
                && RouteRemoveOutsideBoundaryInspector.ValidateEffect(request.Subject, source.Base.CanonicalPath, CancellationToken.None) is not null)
                return null;
            var bytes = File.ReadAllBytes(source.Base.PhysicalPath);
            return new SourceText(
                StrictUtf8.GetString(bytes),
                FileStateSnapshot.File(
                    Path.Combine(
                        workspace.LexicalRoot,
                        source.Identity.CanonicalBasePath.Replace('/', Path.DirectorySeparatorChar)),
                    source.Base.PhysicalPath,
                    bytes));
        }
        catch (Exception exception) when (exception is DecoderFallbackException
            or UnauthorizedAccessException or IOException)
        {
            return null;
        }
    }

    private sealed record SourceText(string Text, FileStateSnapshot Snapshot);

    private sealed record RegionDocumentRead(
        IReadOnlyList<RegionDocument> Values,
        RouteRemoveResultFormation? Boundary)
    {
        internal static RegionDocumentRead Complete(IReadOnlyList<RegionDocument> values)
            => new(values, Boundary: null);

        internal static RegionDocumentRead Stop(RouteRemoveResultFormation boundary)
            => new([], boundary);
    }

    private sealed record NavigationMetadataRead(
        IReadOnlyList<GeneratedNavigationMetadata> Values,
        RouteRemoveResultFormation? Boundary)
    {
        internal IReadOnlyList<FileStateSnapshot> Inputs { get; init; } = [];

        internal static NavigationMetadataRead Complete(IReadOnlyList<GeneratedNavigationMetadata> values)
            => new(values, Boundary: null);

        internal static NavigationMetadataRead Stop(RouteRemoveResultFormation boundary)
            => new([], boundary);
    }
}
