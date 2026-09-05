using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;

internal sealed class RouteInitialContextReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(false, true);
    private readonly MarkdownDocumentParser _markdownParser = new();
    private readonly EmbeddedFrameworkSourceProjector _sourceProjector = new();
    private readonly SourceAuthoredMetadataParser _metadataParser = new();
    private readonly SourceRouteTopologyBuilder _topologyBuilder = new();

    internal RouteContextSet? Read(
        CliWorkspace workspace,
        FrameworkPayloadReadResult read)
    {
        if (read.State != FrameworkPayloadReadState.Available
            || read.Payload is not { } payload)
        {
            return null;
        }

        try
        {
            var entryAsset = payload.Find(FrameworkPayloadAsset.RootAgentPath);
            if (entryAsset is null)
            {
                return null;
            }

            var projected = _sourceProjector.Project(workspace, payload);
            var preliminary = projected
                .Select(source => CreateSource(source, payload))
                .ToArray();
            var roots = RouteContextRootProjector.Read(preliminary);
            var topologySources = projected
                .Where(source => source.Base.Form != SourceDocumentForm.Loader)
                .ToArray();
            var topology = _topologyBuilder.Build(topologySources, roots.Paths);
            var projectedByPath = projected.ToDictionary(
                source => source.Identity.CanonicalBasePath,
                StringComparer.Ordinal);
            var sources = preliminary.Select(source => ProjectRoute(
                source,
                projectedByPath[source.Path],
                topology)).ToArray();
            return new RouteContextSet
            {
                WorkspaceEntry = Entry(entryAsset),
                Sources = sources,
                RootPaths = roots.Paths,
                RootCategories = roots.Categories,
                IsComplete = sources.All(source =>
                    source.RouteState is SourceRouteState.Routed or SourceRouteState.Unrouted),
            };
        }
        catch (Exception exception) when (exception is ArgumentException
            or DecoderFallbackException
            or InvalidDataException
            or InvalidOperationException)
        {
            return null;
        }
    }

    private RouteContextSource CreateSource(
        SourceLogicalSource source,
        FrameworkPayload payload)
    {
        var asset = payload.Find(source.Base.CanonicalPath)
            ?? throw new InvalidDataException(
                $"The projected embedded source '{source.Base.CanonicalPath}' is unavailable.");
        var text = Decode(asset);
        var document = _markdownParser.Parse(text);
        var layers = new List<RouteContextLayer> { new(asset.Path, text) };
        if (source.Overwrite is { } overwrite)
        {
            var overwriteAsset = payload.Find(overwrite.CanonicalPath)
                ?? throw new InvalidDataException(
                    $"The projected embedded overwrite '{overwrite.CanonicalPath}' is unavailable.");
            layers.Add(new RouteContextLayer(overwriteAsset.Path, Decode(overwriteAsset)));
        }

        return new RouteContextSource
        {
            Id = source.Identity.AutomaticId,
            Path = source.Identity.CanonicalBasePath,
            Form = source.Base.Form,
            RouteState = SourceRouteState.Unrouted,
            Layers = layers.ToArray(),
            Metadata = _metadataParser.Parse(document, source.Base.Form),
            GeneratedEntries = SourceGeneratedEntriesParser.Parse(document),
            ParentPath = null,
        };
    }

    private static RouteContextSource ProjectRoute(
        RouteContextSource context,
        SourceLogicalSource source,
        SourceRouteTopology topology)
    {
        if (source.Base.Form == SourceDocumentForm.Loader)
        {
            return context with { RouteState = SourceRouteState.Routed };
        }

        var node = topology.FindByPath(source.Identity.CanonicalBasePath);
        return context with
        {
            RouteState = SourceRouteStateResolver.Read(
                source,
                topology,
                areLoaderRootFactsComplete: true),
            ParentPath = node?.ParentState == SourceRouteParentState.Resolved
                ? node.ParentPaths[0]
                : null,
        };
    }

    private static RouteContextSource Entry(FrameworkPayloadAsset asset)
        => new()
        {
            Id = null,
            Path = asset.Path,
            Form = SourceDocumentForm.Markdown,
            RouteState = SourceRouteState.Unrouted,
            Layers = [new RouteContextLayer(asset.Path, Decode(asset))],
            Metadata = SourceAuthoredMetadataFacts.WithoutValues(
                SourceAuthoredMetadataState.NotApplicable),
            GeneratedEntries = SourceGeneratedEntriesFacts.Absent,
            ParentPath = null,
        };

    private static string Decode(FrameworkPayloadAsset asset)
        => StrictUtf8.GetString(asset.Bytes.AsSpan());
}
