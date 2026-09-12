using OpenForge.Cli.Core.Commands.Context.Models.Presentation;
using OpenForge.Cli.Core.Commands.Context.Models.Result;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static partial class ContextJsonProjector
{
    internal static ContextCompactJsonResult CreateCompact(ContextResult result)
        => new()
        {
            Selection = Selection(result.Selection),
            Presentation = Presentation(result.Presentation),
            Coverage = Coverage(result.Coverage),
            Paths = result.Paths.Select(CompactPathProjection).ToArray(),
            Links = result.Links.Select(Link).ToArray(),
            Sources = result.Sources.Select(CompactSource).ToArray(),
            Findings = result.Findings.Select(Finding).ToArray(),
        };

    private static ContextCompactJsonPathProjection CompactPathProjection(ContextPathProjection path)
        => new()
        {
            Position = path.Position,
            SourcePosition = path.SourcePosition,
            Id = path.Id,
            Path = path.Path,
            Layer = Layer(path.Layer),
        };

    private static ContextCompactJsonSource CompactSource(ContextSource source)
        => new()
        {
            Position = source.Position,
            Id = source.Id,
            Path = source.Path,
            RouteState = RouteState(source.RouteState),
            Route = source.Route,
            Scope = source.Scope,
            Layers = source.Layers.Select(CompactSourceLayer).ToArray(),
        };

    private static ContextCompactJsonLayer CompactSourceLayer(ContextLayer layer)
        => new()
        {
            PathPosition = layer.PathPosition,
            Kind = Layer(layer.Kind),
            Path = layer.Path,
            Projections = layer.Projections.Select(Projection).ToArray(),
        };
}
