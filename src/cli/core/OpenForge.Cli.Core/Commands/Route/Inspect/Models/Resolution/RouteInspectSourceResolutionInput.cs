using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectSourceResolutionInput
{
    internal RouteInspectSourceResolutionInput(
        RouteInspectRequest request,
        RouteInspectSelection selection,
        RouteSource source,
        string requestedPath,
        RouteSourceCatalogue catalogue)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(source);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedPath);
        ArgumentNullException.ThrowIfNull(catalogue);
        Request = request;
        Selection = selection;
        Source = source;
        RequestedPath = requestedPath;
        Catalogue = catalogue;
    }

    internal RouteInspectRequest Request { get; }

    internal RouteInspectSelection Selection { get; }

    internal RouteSource Source { get; }

    internal string RequestedPath { get; }

    internal RouteSourceCatalogue Catalogue { get; }
}
