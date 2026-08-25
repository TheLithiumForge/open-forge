using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectSourceResolutionInput
{
    internal RouteInspectSourceResolutionInput(
        RouteInspectResolutionInput resolution,
        RouteInspectSelection selection,
        RouteSourceProjection projection,
        string requestedPath)
    {
        ArgumentNullException.ThrowIfNull(resolution);
        ArgumentNullException.ThrowIfNull(selection);
        ArgumentNullException.ThrowIfNull(projection);
        ArgumentException.ThrowIfNullOrWhiteSpace(requestedPath);
        if (projection.Source is null)
        {
            throw new ArgumentException("A selected Route source projection must be available.", nameof(projection));
        }

        Resolution = resolution;
        Selection = selection;
        Projection = projection;
        RequestedPath = requestedPath;
    }

    internal RouteInspectResolutionInput Resolution { get; }

    internal RouteInspectSelection Selection { get; }

    internal RouteSourceProjection Projection { get; }

    internal SourceLogicalSource LogicalSource => Projection.LogicalSource;

    internal string RequestedPath { get; }

}
