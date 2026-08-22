namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal sealed class RouteInspectInventory
{
    internal RouteInspectInventory(
        IReadOnlyList<RouteInspectInventoryFile> files,
        IReadOnlySet<string> unsafePaths,
        IReadOnlyList<RouteInspectResolutionIssue> boundaryIssues,
        bool interrupted)
    {
        ArgumentNullException.ThrowIfNull(files);
        ArgumentNullException.ThrowIfNull(unsafePaths);
        ArgumentNullException.ThrowIfNull(boundaryIssues);
        Files = files;
        UnsafePaths = unsafePaths;
        BoundaryIssues = boundaryIssues;
        Interrupted = interrupted;
    }

    internal IReadOnlyList<RouteInspectInventoryFile> Files { get; }

    internal IReadOnlySet<string> UnsafePaths { get; }

    internal IReadOnlyList<RouteInspectResolutionIssue> BoundaryIssues { get; }

    internal bool Interrupted { get; }
}
