namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal enum RouteInspectLayerRole
{
    Base,
    Overwrite,
}

internal sealed record RouteInspectPhysicalLayer
{
    internal RouteInspectPhysicalLayer(
        string workspaceRelativePath,
        string physicalPath,
        RouteInspectLayerRole role)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspaceRelativePath);
        ArgumentException.ThrowIfNullOrWhiteSpace(physicalPath);
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "The route-inspect physical-layer role is not defined.");
        }

        WorkspaceRelativePath = workspaceRelativePath;
        PhysicalPath = physicalPath;
        Role = role;
    }

    internal string WorkspaceRelativePath { get; }

    internal string PhysicalPath { get; }

    internal RouteInspectLayerRole Role { get; }
}
