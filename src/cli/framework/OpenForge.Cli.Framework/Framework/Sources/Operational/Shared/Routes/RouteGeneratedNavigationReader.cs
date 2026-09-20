using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteGeneratedNavigationReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly PhysicalPathResolver _physicalPathResolver;

    internal RouteGeneratedNavigationReader(PhysicalPathResolver physicalPathResolver)
    {
        _physicalPathResolver = physicalPathResolver;
    }

    internal IReadOnlyList<GeneratedNavigationTargetObservation> Read(
        CliWorkspace workspace,
        FrameworkPayloadReadResult payload,
        RouteSourceInspection inspection)
    {
        var paths = ReadExpectedPaths(payload, inspection);
        var formation = new GeneratedNavigationFormationBuilder().Build(inspection.Catalogue);
        var observations = RouteGeneratedNavigationProjection.IndexSources(inspection.Sources);
        var sources = paths.Select(formation.FindSource).OfType<SourceLogicalSource>();
        var projection = RouteGeneratedNavigationProjection.Project(formation, observations, sources);
        var projected = projection.Regions.ToDictionary(
            region => region.CanonicalPath,
            StringComparer.Ordinal);
        return paths.Select(path =>
        {
            if (!projected.TryGetValue(path, out var region))
            {
                return new GeneratedNavigationTargetObservation(path, ReadAbsent(workspace, path));
            }

            return new GeneratedNavigationTargetObservation(path, Project(region))
            {
                MetadataIssues = RouteStatusMetadataIssueReader.Read(formation, observations, region),
            };
        })
            .ToArray();
    }

    private IReadOnlyList<string> ReadExpectedPaths(
        FrameworkPayloadReadResult payload,
        RouteSourceInspection inspection)
    {
        var paths = new HashSet<string>(StringComparer.Ordinal);
        if (payload.State == FrameworkPayloadReadState.Available
            && payload.Payload is { } value)
        {
            try
            {
                foreach (var asset in value.Assets.Where(asset =>
                             SourceFormClassifier.TryClassify(asset.Path, out var form)
                             && form != SourceDocumentForm.OverwriteCompanion))
                {
                    var document = _markdownParser.Parse(
                        StrictUtf8.GetString(asset.Bytes.AsSpan()));
                    if (document.GeneratedRegion.State != MarkdownGeneratedRegionState.Absent)
                    {
                        paths.Add(asset.Path);
                    }
                }
            }
            catch (DecoderFallbackException)
            {
                // The embedded payload failure is retained by the initial measurement.
            }
        }

        foreach (var source in inspection.Sources.Where(source =>
                     source.Document is { } document
                     && document.GeneratedRegion.State != MarkdownGeneratedRegionState.Absent))
        {
            paths.Add(source.Source.Identity.CanonicalBasePath);
        }

        return paths.OrderBy(path => path, StringComparer.Ordinal).ToArray();
    }

    private OperationalGeneratedNavigationState ReadAbsent(
        CliWorkspace workspace,
        string path)
    {
        var resolution = _physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path));
        return resolution.State switch
        {
            PhysicalPathState.Missing => OperationalGeneratedNavigationState.Missing,
            PhysicalPathState.External
                or PhysicalPathState.Dangling
                or PhysicalPathState.Cycle
                or PhysicalPathState.Invalid
                or PhysicalPathState.Unsupported => OperationalGeneratedNavigationState.Blocked,
            PhysicalPathState.Contained
                or PhysicalPathState.Inaccessible
                or PhysicalPathState.InputOutputFailure => OperationalGeneratedNavigationState.Unavailable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(resolution),
                resolution.State,
                "The physical path state is not defined."),
        };
    }

    private static OperationalGeneratedNavigationState Project(
        GeneratedNavigationRegion region)
    {
        if (region.State == GeneratedNavigationRegionState.Available)
        {
            return region.Change?.IsUnchanged == true
                ? OperationalGeneratedNavigationState.Current
                : OperationalGeneratedNavigationState.Changed;
        }

        if (region.State != GeneratedNavigationRegionState.Unavailable
            || region.UnavailableReason is not { } reason)
        {
            throw new InvalidOperationException(
                "An unavailable generated-navigation region requires one finite reason.");
        }

        return RouteGeneratedNavigationProjection.ReadUnavailableState(reason, nameof(region));
    }
}
