using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning.Metadata;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning.Metadata;

internal sealed class RouteUpdateMetadataEditDraft(RouteUpdateMetadataLayout layout)
{
    internal RouteUpdateMetadataLayout Layout { get; } = layout;

    internal ImmutableArray<RouteUpdateMetadataEdit>.Builder Edits { get; } =
        ImmutableArray.CreateBuilder<RouteUpdateMetadataEdit>();

    internal ImmutableArray<RouteUpdatePreviewHunk>.Builder Preview { get; } =
        ImmutableArray.CreateBuilder<RouteUpdatePreviewHunk>();

    internal RouteUpdateMetadataEditPlan Build()
        => new()
        {
            Edits = Edits.Where(edit => edit.DocumentLength != 0)
                .Concat(Edits.Where(edit => edit.DocumentLength == 0)
                    .GroupBy(edit => edit.DocumentStart)
                    .Select(group => new RouteUpdateMetadataEdit
                    {
                        DocumentStart = group.Key,
                        DocumentLength = 0,
                        Replacement = string.Concat(group.OrderBy(edit => edit.InsertionOrder)
                            .Select(edit => edit.Replacement)),
                    }))
                .OrderBy(edit => edit.DocumentStart)
                .ThenBy(edit => edit.DocumentLength)
                .ToImmutableArray(),
            Preview = Preview.ToImmutable(),
        };
}
