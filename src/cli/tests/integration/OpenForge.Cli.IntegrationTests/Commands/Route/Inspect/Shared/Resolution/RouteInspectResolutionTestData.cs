using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Resolution;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Route.Inspect.Shared.Resolution;

internal static class RouteInspectResolutionTestData
{
    internal static RouteInspectSelection Selection(
        RouteSource source,
        RouteInspectSelectionMethod selectionMethod)
    {
        return selectionMethod switch
        {
            RouteInspectSelectionMethod.AutomaticId => new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                selectionMethod,
                source.Id,
                []),
            RouteInspectSelectionMethod.ExactPath => new RouteInspectSelection(
                RouteInspectReferenceKind.SourcePath,
                selectionMethod,
                source.CanonicalPath,
                []),
            RouteInspectSelectionMethod.Interactive => new RouteInspectSelection(
                RouteInspectReferenceKind.SourceId,
                selectionMethod,
                source.Id,
                []),
            _ => throw new ArgumentOutOfRangeException(
                nameof(selectionMethod),
                selectionMethod,
                "The fixed selection method is not supported."),
        };
    }

    internal static RouteInspectIdentity Identity(
        RouteSource source,
        RouteInspectRouteState routeState)
    {
        return new RouteInspectIdentity(
            source.Id,
            source.CanonicalPath,
            ReadInspectKind(source.Kind),
            ReadInspectForm(source.Base.Form),
            routeState,
            ReadPhysicalLayers(source));
    }

    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "route-inspect-profile-workspace"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.CurrentDirectory);
    }

    private static RouteInspectSourceKind ReadInspectKind(RouteSourceKind kind)
    {
        return kind switch
        {
            RouteSourceKind.Entrypoint => RouteInspectSourceKind.Entrypoint,
            RouteSourceKind.Markdown => RouteInspectSourceKind.Markdown,
            RouteSourceKind.Native => RouteInspectSourceKind.Native,
            _ => throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The fixed source kind is not supported."),
        };
    }

    private static RouteInspectSourceForm ReadInspectForm(SourceDocumentForm form)
    {
        return form switch
        {
            SourceDocumentForm.CanonicalEntrypoint => RouteInspectSourceForm.CanonicalEntrypoint,
            SourceDocumentForm.IndexEntrypoint
                or SourceDocumentForm.UnderscoreIndexEntrypoint
                or SourceDocumentForm.ReferencesEntrypoint
                or SourceDocumentForm.UnderscoreReferencesEntrypoint => RouteInspectSourceForm.CompatibilityEntrypoint,
            SourceDocumentForm.Markdown => RouteInspectSourceForm.Markdown,
            SourceDocumentForm.Skill => RouteInspectSourceForm.Native,
            _ => throw new ArgumentOutOfRangeException(
                nameof(form),
                form,
                "The fixed source form is not supported."),
        };
    }

    private static IReadOnlyList<RouteInspectPhysicalLayer> ReadPhysicalLayers(RouteSource source)
    {
        var layers = new List<RouteInspectPhysicalLayer>
        {
            new(source.CanonicalPath, source.PhysicalPath, RouteInspectLayerRole.Base),
        };
        if (source.Overwrite is not null)
        {
            layers.Add(new RouteInspectPhysicalLayer(
                source.Overwrite.CanonicalLogicalPath,
                source.Overwrite.PhysicalPath,
                RouteInspectLayerRole.Overwrite));
        }

        return layers;
    }
}
