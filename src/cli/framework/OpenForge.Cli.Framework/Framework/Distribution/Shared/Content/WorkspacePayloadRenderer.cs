using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

internal static class WorkspacePayloadRenderer
{
    // The Frontmatter Form section of .agents/memory/crystallized/documents/cli/shared-operation-contract.md defines delivery eligibility.
    private const string MarkdownExtension = SourceFormClassifier.MarkdownExtension;
    private const string NativeSkillFileName = SourceFormClassifier.NativeSkillFileName;
    private static readonly WorkspaceLayout DefaultLayout = WorkspaceLayout.Create(
        new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory),
        WorkspaceLayout.DefaultMainRoot);

    internal static FrameworkFrontmatterTransformResult Render(string destinationPath, ReadOnlyMemory<byte> sourceBytes, FrontmatterForm form)
        => Render(DefaultLayout, destinationPath, sourceBytes, form);

    internal static FrameworkFrontmatterTransformResult Render(
        WorkspaceLayout layout, string packagePath, ReadOnlyMemory<byte> sourceBytes, FrontmatterForm form)
    {
        ArgumentNullException.ThrowIfNull(packagePath);
        if (!Enum.IsDefined(form))
            throw new ArgumentOutOfRangeException(nameof(form), form, "The frontmatter form is not defined.");
        var rendered = WorkspaceLayoutPayloadRenderer.Render(layout, packagePath, sourceBytes);
        if (form == FrontmatterForm.Scoped || rendered.Bytes is not { } bytes)
            return rendered;

        var destination = layout.IsDefault ? packagePath : layout.MapPayloadPath(packagePath);
        var frontmatter = RenderRoot(layout, destination, bytes);
        return frontmatter.State == FrameworkFrontmatterTransformState.Unchanged ? rendered : frontmatter;
    }

    private static FrameworkFrontmatterTransformResult RenderRoot(
        WorkspaceLayout layout, string destinationPath, ReadOnlyMemory<byte> sourceBytes)
    {
        if (!PortableWorkspacePath.TryNormalize(destinationPath, out var path)
            || !SourceLogicalPath.IsPrimarySource(layout, path)
            || !path.EndsWith(MarkdownExtension, StringComparison.Ordinal)
            || path.AsSpan(path.LastIndexOf('/') + 1).SequenceEqual(NativeSkillFileName))
        {
            return FrameworkFrontmatterTransformResult.Unchanged(sourceBytes);
        }

        return FrameworkFrontmatterDocumentTransformer.Transform(sourceBytes, FrontmatterForm.Root);
    }
}
