using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;

internal sealed record RouteTemplateResolutionRequest
{
    public required CliWorkspace Workspace { get; init; }

    public required string Reference { get; init; }

    public required SourceCatalogue Catalogue { get; init; }
}
