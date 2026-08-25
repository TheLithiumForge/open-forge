using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

internal static class SourceInventoryTestData
{
    internal static CliWorkspace Workspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "source-inventory-model-unit"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    internal static string Physical(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "source-inventory-model-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }

    internal static SourceLayer BaseLayer(
        string canonicalPath,
        SourceDocumentForm form = SourceDocumentForm.Markdown)
    {
        return new SourceLayer(
            canonicalPath,
            Physical(canonicalPath),
            form,
            SourceLayerKind.Base);
    }

    internal static SourceLayer OverwriteLayer(string canonicalPath)
    {
        return new SourceLayer(
            canonicalPath,
            Physical(canonicalPath),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
    }

    internal static SourceLogicalSource Source(
        string canonicalBasePath,
        string? automaticId = null,
        SourceDocumentForm form = SourceDocumentForm.Markdown,
        bool withOverwrite = false)
    {
        var id = automaticId ?? canonicalBasePath[".agents/".Length..^".md".Length];
        var identity = new SourceLogicalIdentity(id, canonicalBasePath);
        var baseLayer = BaseLayer(canonicalBasePath, form);
        var overwritePath = canonicalBasePath[..^".md".Length] + ".overwrite.md";
        return new SourceLogicalSource(
            identity,
            baseLayer,
            withOverwrite ? OverwriteLayer(overwritePath) : null);
    }

    internal static SourceCandidate Candidate(
        string canonicalPath,
        SourceDocumentForm? form,
        string? automaticId,
        PhysicalPathState state,
        string? physicalPath = null,
        string? physicalParentPath = null)
    {
        return new SourceCandidate(
            canonicalPath,
            form,
            automaticId,
            state,
            physicalPath,
            physicalParentPath ?? Physical(".agents"));
    }
}
