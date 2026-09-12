using OpenForge.Cli.Core.Commands.Route.List.Models.Presentation;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Rendering;

internal static partial class RouteListJsonProjection
{
    internal static RouteListCompactJsonResult CreateCompact(RouteListResult result)
        => new()
        {
            Selection = Selection(result.Selection),
            RequestedDepth = RouteListJsonDepth.From(result.RequestedDepth),
            EffectiveDepth = RouteListJsonDepth.From(result.EffectiveDepth),
            Coverage = Coverage(result.Coverage),
            Findings = result.Findings.Select(Finding).ToArray(),
            Rows = result.Rows.Select(CompactRow).ToArray(),
        };

    private static RouteListCompactJsonRow CompactRow(RouteListRow row)
        => new()
        {
            Id = row.Id,
            Path = row.Path,
            ParentId = row.ParentId,
            ParentPath = row.ParentPath,
            AbsoluteDepth = row.AbsoluteDepth,
            RelativeDepth = row.RelativeDepth,
            Kind = row.Kind switch
            {
                RouteListRowKind.Entrypoint => "entrypoint",
                RouteListRowKind.RoutedLeaf => "routed-leaf",
                _ => throw new ArgumentOutOfRangeException(nameof(row), row.Kind, "The route-list row kind is not defined."),
            },
            Description = row.Description,
            Tags = row.Tags.ToArray(),
            DirectChildCount = row.DirectChildCount,
        };
}
