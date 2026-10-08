using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Distribution.Shared.Content;

internal static class WorkspacePayloadRenderer
{
    // The Frontmatter Form section of .agents/memory/crystallized/documents/cli/shared-operation-contract.md defines delivery eligibility.
    private const string AgentsPrefix = ".agents/";
    private const string MarkdownExtension = ".md";
    private const string NativeSkillFileName = "SKILL.md";

    internal static FrameworkFrontmatterTransformResult Render(string destinationPath, ReadOnlyMemory<byte> sourceBytes, FrontmatterForm form)
    {
        ArgumentNullException.ThrowIfNull(destinationPath);
        return form switch
        {
            FrontmatterForm.Scoped => FrameworkFrontmatterTransformResult.Unchanged(sourceBytes),
            FrontmatterForm.Root => RenderRoot(destinationPath, sourceBytes),
            _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The frontmatter form is not defined."),
        };
    }

    private static FrameworkFrontmatterTransformResult RenderRoot(string destinationPath, ReadOnlyMemory<byte> sourceBytes)
    {
        if (!PortableWorkspacePath.TryNormalize(destinationPath, out var path)
            || !path.StartsWith(AgentsPrefix, StringComparison.Ordinal)
            || !path.EndsWith(MarkdownExtension, StringComparison.Ordinal)
            || path.AsSpan(path.LastIndexOf('/') + 1).SequenceEqual(NativeSkillFileName))
        {
            return FrameworkFrontmatterTransformResult.Unchanged(sourceBytes);
        }

        return FrameworkFrontmatterDocumentTransformer.Transform(sourceBytes, FrontmatterForm.Root);
    }
}
