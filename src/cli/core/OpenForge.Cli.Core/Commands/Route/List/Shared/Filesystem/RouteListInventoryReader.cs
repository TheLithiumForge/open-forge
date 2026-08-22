using OpenForge.Cli.Core.Commands.Route.Shared.Source;

namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;

internal sealed class RouteListInventoryReader
{
    private readonly RouteListInventoryTraversal _traversal = new();
    private readonly RouteMetadataParser _metadataParser = new();

    internal async ValueTask<RouteListInventoryFacts> ReadAsync(RouteListInventoryRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var inventory = new RouteListInventoryAccumulator();
        var interruption = await _traversal
            .ReadAsync(request, inventory)
            .ConfigureAwait(false);
        return interruption is null
            ? FormFacts(inventory)
            : FormInterrupted(inventory, interruption);
    }

    private RouteListInventoryFacts FormFacts(RouteListInventoryAccumulator inventory)
    {
        var sources = FormSources(inventory);
        return RouteListInventoryFacts.Create(
            sources,
            inventory.Findings,
            inventory.Aliases,
            inventory.OverwriteFacts);
    }

    private RouteListInventoryFacts FormInterrupted(
        RouteListInventoryAccumulator inventory,
        RouteListFilesystemFinding interruption)
    {
        var sources = FormSources(inventory);
        return RouteListInventoryFacts.Interrupted(
            sources,
            inventory.Findings,
            inventory.Aliases,
            interruption,
            inventory.OverwriteFacts);
    }

    private IReadOnlyList<RouteListInventorySource> FormSources(RouteListInventoryAccumulator inventory)
    {
        var sources = new RouteListInventorySourceBuilder(inventory.Files)
            .FormSources(_metadataParser, inventory.Findings, inventory.OverwriteFacts);
        inventory.Files.Clear();
        return sources;
    }
}
