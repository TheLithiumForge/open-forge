using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Shared.Routing;

internal static class SourceRoutingTestData
{
    internal static SourceLogicalSource Source(
        string canonicalPath,
        string id,
        SourceDocumentForm form = SourceDocumentForm.Markdown)
    {
        var physicalPath = Physical(canonicalPath);
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            new SourceLayer(canonicalPath, physicalPath, form, SourceLayerKind.Base));
    }

    internal static SourceRouteTopology Topology()
    {
        var root = Source(".agents/root/_root.md", "root", SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(".agents/root/child.md", "root/child");
        var rootNode = new SourceRouteNode(
            root.Identity,
            SourceRouteParentState.None,
            [],
            [child.Identity.CanonicalBasePath]);
        var childNode = new SourceRouteNode(
            child.Identity,
            SourceRouteParentState.Resolved,
            [root.Identity.CanonicalBasePath],
            []);
        return new SourceRouteTopology(
            [childNode, rootNode],
            [root.Identity.CanonicalBasePath]);
    }

    internal static string Physical(string canonicalPath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "source-routing-model-unit",
            canonicalPath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
