using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.Rendering;

internal static class RouteInspectCompactIdentity
{
    internal static void Add(
        ICollection<string> lines,
        RouteInspectIdentity? identity)
    {
        if (identity is null)
        {
            lines.Add("ID: not established");
            lines.Add("Path: not established");
            lines.Add("Source state: not established");
            lines.Add("Route state: unresolved");
            lines.Add("Overwrite: not established");
            return;
        }

        lines.Add($"ID: {RouteInspectHumanValues.Text(identity.Id)}");
        lines.Add($"Path: {RouteInspectHumanValues.Text(identity.CanonicalWorkspaceRelativePath)}");
        lines.Add($"Source state: {RouteInspectHumanValues.SourceState(identity)}");
        lines.Add($"Route state: {RouteInspectHumanValues.RouteState(identity.RouteState)}");
        lines.Add($"Entrypoint form: {Form(identity.Form)}");
        lines.Add($"Physical layers: {string.Join(", ", identity.PhysicalLayers.Select(Layer))}");
        lines.Add($"Overwrite: {(identity.PhysicalLayers.Count > 1 ? "present (base plus overwrite)" : "none")}");
    }

    private static string Form(RouteInspectSourceForm form)
    {
        return form switch
        {
            RouteInspectSourceForm.CanonicalEntrypoint => "canonical",
            RouteInspectSourceForm.CompatibilityEntrypoint => "compatibility",
            RouteInspectSourceForm.Markdown => "Markdown",
            RouteInspectSourceForm.Native => "native",
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The source form is not defined."),
        };
    }

    private static string Layer(RouteInspectPhysicalLayer layer)
    {
        var role = layer.Role switch
        {
            RouteInspectLayerRole.Base => "base",
            RouteInspectLayerRole.Overwrite => "overwrite",
            _ => throw new ArgumentOutOfRangeException(nameof(layer), layer.Role, "The layer role is not defined."),
        };
        return $"{role} {RouteInspectHumanValues.Text(layer.WorkspaceRelativePath)}";
    }
}
