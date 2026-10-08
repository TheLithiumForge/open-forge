using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Content;

public sealed class WorkspacePayloadRendererTests
{
    [Fact(DisplayName = "Scoped delivery preserves canonical bytes without parsing"), Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void ScopedDeliveryPreservesCanonicalBytes()
    {
        AssertUnchanged(".agents/directives/example.md", Encoding.UTF8.GetBytes("\uFEFF---\r\nopen-forge:\n  tags: [Core]\n---\r\nBody\n"), FrontmatterForm.Scoped);
        AssertUnchanged(".agents/directives/example.md", new byte[] { 0xFF }, FrontmatterForm.Scoped);
    }

    [Theory(DisplayName = "Eligible root delivery preserves the exact body and fenced examples")]
    [InlineData(".agents/example.md"), InlineData(".agents/directives/scope/example.md")]
    [Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void RootDeliveryPreservesBodyAndFencedExamples(string path)
    {
        const string body = "\r\n# Body 🐱\n```yaml\nopen-forge:\n  tags: [Example]\n```\r\n";
        var source = Encoding.UTF8.GetBytes("---\r\nopen-forge:\r\n  description: Purpose\n  tags: [Core]\r\n---\r\n" + body);
        var result = WorkspacePayloadRenderer.Render(path, source, FrontmatterForm.Root);
        Assert.Equal(FrameworkFrontmatterTransformState.Changed, result.State);
        Assert.Null(result.Cause);
        Assert.NotNull(result.Bytes);
        Assert.Equal(Encoding.UTF8.GetBytes("---\r\ndescription: Purpose\ntags: [Core]\r\n---\r\n" + body), result.Bytes.Value.ToArray());
    }

    [Theory(DisplayName = "Native Skill and opaque delivery preserve bytes without parsing")]
    [InlineData(".agents/skills/example/SKILL.md"), InlineData(".agents/SKILL.md")]
    [InlineData(".agents/example.json"), InlineData(".agents/example.md.txt")]
    [Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void NativeAndOpaqueDeliveryPreservesBytes(string path)
    {
        AssertUnchanged(path, Encoding.UTF8.GetBytes("---\nopen-forge: [\n---\n"), FrontmatterForm.Root);
        AssertUnchanged(path, new byte[] { 0xFF }, FrontmatterForm.Root);
    }

    [Theory(DisplayName = "Destinations outside canonical agents Markdown are never rendered")]
    [InlineData("AGENTS.md"), InlineData("CLAUDE.md"), InlineData("docs/example.md")]
    [InlineData(".agents-other/example.md"), InlineData("nested/.agents/example.md")]
    [InlineData(".agents/../example.md"), InlineData("/.agents/example.md"), InlineData(".agents//example.md")]
    [Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void PathsOutsideAgentsAreNeverRendered(string path)
    {
        AssertUnchanged(path, Encoding.UTF8.GetBytes("---\nopen-forge:\n  tags: [Core]\n---\n"), FrontmatterForm.Root);
        AssertUnchanged(path, new byte[] { 0xFF }, FrontmatterForm.Root);
    }

    [Fact(DisplayName = "Eligible root delivery returns the document transformation failure"), Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void InvalidEligiblePayloadRetainsCause()
    {
        var result = WorkspacePayloadRenderer.Render(".agents/example.md", Encoding.UTF8.GetBytes("---\nopen-forge: [Core]\n---\n"), FrontmatterForm.Root);
        Assert.Equal(FrameworkFrontmatterTransformState.Invalid, result.State);
        Assert.Null(result.Bytes);
        Assert.NotNull(result.Cause);
    }

    [Fact(DisplayName = "Workspace payload rendering rejects unnamed form values"), Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit")]
    public void UndefinedFormIsRejected()
        => Assert.Throws<ArgumentOutOfRangeException>(() => WorkspacePayloadRenderer.Render("AGENTS.md", ReadOnlyMemory<byte>.Empty, (FrontmatterForm)99));

    private static void AssertUnchanged(string path, ReadOnlyMemory<byte> source, FrontmatterForm form)
    {
        var result = WorkspacePayloadRenderer.Render(path, source, form);
        Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
        Assert.Null(result.Cause);
        Assert.Equal(source, result.Bytes);
    }
}
