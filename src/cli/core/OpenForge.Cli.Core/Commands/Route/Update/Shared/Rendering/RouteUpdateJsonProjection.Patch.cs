using OpenForge.Cli.Core.Commands.Route.Update.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Rendering;

internal static partial class RouteUpdateJsonProjection
{
    private static RouteUpdateJsonPatch Patch(RouteUpdatePatch patch)
        => new()
        {
            Description = new RouteUpdateJsonDescriptionPatch
            {
                Requested = patch.Description.Requested,
                Before = patch.Description.Before,
                Expected = patch.Description.Expected,
                State = RouteUpdateDefinitions.ReadMachineName(patch.Description.State),
            },
            Responsibility = new RouteUpdateJsonResponsibilityPatch
            {
                Requested = patch.Responsibility.Requested,
                Operation = RouteUpdateDefinitions.ReadMachineName(
                    patch.Responsibility.Operation),
                Before = patch.Responsibility.Before,
                Expected = patch.Responsibility.Expected,
                State = RouteUpdateDefinitions.ReadMachineName(
                    patch.Responsibility.State),
            },
            Tags = new RouteUpdateJsonTagsPatch
            {
                Requested = patch.Tags.Requested,
                Before = patch.Tags.Before is { } before ? [.. before] : null,
                Expected = patch.Tags.Expected is { } expected ? [.. expected] : null,
                State = RouteUpdateDefinitions.ReadMachineName(patch.Tags.State),
            },
        };

    private static RouteUpdateJsonTemplate Template(RouteUpdateTemplate template)
        => new()
        {
            Requested = template.Requested,
            Id = template.Id,
            Path = template.Path,
            Classification = template.Classification is { } classification
                ? RouteUpdateDefinitions.ReadMachineName(classification)
                : null,
            BodyByteLength = template.BodyByteLength,
            Decision = RouteUpdateDefinitions.ReadMachineName(template.Decision),
        };
}
