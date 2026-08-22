using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

internal enum RouteInspectSourceKind
{
    Entrypoint,
    Markdown,
    Native,
}

internal enum RouteInspectSourceForm
{
    CanonicalEntrypoint,
    CompatibilityEntrypoint,
    Markdown,
    Native,
}

internal enum RouteInspectRouteState
{
    Routed,
    Detached,
    NotRouted,
    Ambiguous,
    Unresolved,
}

internal sealed class RouteInspectIdentity
{
    internal RouteInspectIdentity(
        string id,
        string canonicalWorkspaceRelativePath,
        RouteInspectSourceKind kind,
        RouteInspectSourceForm form,
        RouteInspectRouteState routeState,
        IEnumerable<RouteInspectPhysicalLayer> physicalLayers)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentException.ThrowIfNullOrWhiteSpace(canonicalWorkspaceRelativePath);
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The route-inspect source kind is not defined.");
        }

        if (!Enum.IsDefined(form))
        {
            throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The route-inspect source form is not defined.");
        }

        if (!Enum.IsDefined(routeState))
        {
            throw new ArgumentOutOfRangeException(
                nameof(routeState),
                routeState,
                "The route-inspect route state is not defined.");
        }

        Id = id;
        CanonicalWorkspaceRelativePath = canonicalWorkspaceRelativePath;
        Kind = kind;
        Form = form;
        RouteState = routeState;
        PhysicalLayers = MaterializePhysicalLayers(physicalLayers);
    }

    internal string Id { get; }

    internal string CanonicalWorkspaceRelativePath { get; }

    internal RouteInspectSourceKind Kind { get; }

    internal RouteInspectSourceForm Form { get; }

    internal RouteInspectRouteState RouteState { get; }

    internal IReadOnlyList<RouteInspectPhysicalLayer> PhysicalLayers { get; }

    private static IReadOnlyList<RouteInspectPhysicalLayer> MaterializePhysicalLayers(
        IEnumerable<RouteInspectPhysicalLayer> physicalLayers)
    {
        ArgumentNullException.ThrowIfNull(physicalLayers);
        var materialized = physicalLayers.ToArray();
        if (materialized.Length == 0)
        {
            throw new ArgumentException("A route-inspect identity requires at least one physical layer.", nameof(physicalLayers));
        }

        if (materialized.Length > 2)
        {
            throw new ArgumentException(
                "A route-inspect identity allows one base and one overwrite physical layer.",
                nameof(physicalLayers));
        }

        var workspacePaths = new HashSet<string>(StringComparer.Ordinal);
        var physicalPaths = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < materialized.Length; index++)
        {
            var layer = materialized[index];
            if (layer is null)
            {
                throw new ArgumentException("Physical layers cannot contain null.", nameof(physicalLayers));
            }

            if (!workspacePaths.Add(layer.WorkspaceRelativePath)
                || !physicalPaths.Add(layer.PhysicalPath))
            {
                throw new ArgumentException("Physical-layer paths must be unique.", nameof(physicalLayers));
            }

            if (index == 0 && layer.Role != RouteInspectLayerRole.Base)
            {
                throw new ArgumentException("The first physical layer must be the base.", nameof(physicalLayers));
            }

            if (index == 1 && layer.Role != RouteInspectLayerRole.Overwrite)
            {
                throw new ArgumentException(
                    "The second physical layer must be the overwrite.",
                    nameof(physicalLayers));
            }
        }

        return new ReadOnlyCollection<RouteInspectPhysicalLayer>(materialized);
    }
}
