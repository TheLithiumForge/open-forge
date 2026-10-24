using System.Text;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Shared.ManagedHosts;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Shared.Content;

[Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceLayoutPayloadRendererTests
{
    private const string CustomRoot = "docs/context";
    private const string Category = "notes";
    private const string Note = "guide.md";
    private const string Skill = "example";
    private const string Fragment = "#section";
    private const string LocalLink = "[Local](sibling.md)";
    private const string ExternalLink = "[External](https://example.org/guide)";
    private const string PreservedExample = "`" + FrameworkPayloadAsset.LoaderPath + "`\r\n```md\n[Example](../" + WorkspaceLayoutNames.LoaderFileName + ")\n```\r\n";
    private const string PackageNote = WorkspaceLayout.DefaultMainRoot + "/" + Category + "/" + Note;
    private const string SkillPath = WorkspaceLayout.NativeSkillsPath + "/" + Skill + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string ReferencePath = WorkspaceLayout.NativeSkillsPath + "/" + Skill + "/" + Note;
    private const string SnapshotName = "hosts";
    private const string ResourceName = "asset.bin";
    private const string MetadataFixture = "---\nopen-forge:\n  description: Fixture\n---\n";
    private const string SpacedRoot = "docs/team context";
    private const string EncodedSpacedRoot = "docs/team%20context";
    private const string SpacedSkill = "team skill";
    private const string EncodedSpacedSkill = "team%20skill";
    private static readonly byte[] MixedBytes = Encoding.UTF8.GetBytes("\uFEFF# Payload\r\nA\nB\r\n");

    [Fact(DisplayName = "Default layout returns the original bytes without decoding or normalizing line endings")]
    public void DefaultReturnsOriginalBytes()
    {
        foreach (var bytes in new[] { MixedBytes, new byte[] { 0xFF, 0xFE } })
        {
            var result = WorkspaceLayoutPayloadRenderer.Render(Layout(WorkspaceLayout.DefaultMainRoot), PackageNote, bytes);
            Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
            Assert.NotNull(result.Bytes);
            Assert.Equal(bytes, result.Bytes.Value.ToArray());
        }
    }

    [Fact(DisplayName = "Default frontmatter delivery retains canonical package-root casing eligibility")]
    public void DefaultRootCasingRemainsLiteral()
    {
        var bytes = Encoding.UTF8.GetBytes(MetadataFixture);
        var path = WorkspaceLayout.DefaultMainRoot.ToUpperInvariant() + "/" + Category + "/" + Note;
        var result = WorkspacePayloadRenderer.Render(Layout(WorkspaceLayout.DefaultMainRoot), path, bytes, FrontmatterForm.Root);
        Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
        Assert.Equal(bytes, result.Bytes?.ToArray());
    }

    [Fact(DisplayName = "Nested payload links rebase fixed Skills and outside files while retaining same-root spelling")]
    public void RebaseParsedLinksOnly()
    {
        var body = $"\uFEFF[Skill](../{WorkspaceLayoutNames.SkillsDirectoryName}/{Skill}/{SourceFormClassifier.NativeSkillFileName}{Fragment})\r\n"
            + $"[Host](../../{FrameworkPayloadAsset.RootAgentPath})\n{LocalLink}\r\n{ExternalLink}\n{PreservedExample}";
        var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), PackageNote, Encoding.UTF8.GetBytes(body));
        var document = Parse(result);
        Assert.Equal([
            $"../../../{WorkspaceLayout.NativeSkillsPath}/{Skill}/{SourceFormClassifier.NativeSkillFileName}{Fragment}",
            $"../../../{FrameworkPayloadAsset.RootAgentPath}",
            new MarkdownDocumentParser().Parse(LocalLink).Links.Single().RawDestination,
            new MarkdownDocumentParser().Parse(ExternalLink).Links.Single().RawDestination,
        ], document.Links.Select(link => link.RawDestination));
        var expected = body.Replace($"../{WorkspaceLayoutNames.SkillsDirectoryName}/", $"../../../{WorkspaceLayout.NativeSkillsPath}/", StringComparison.Ordinal)
            .Replace($"../../{FrameworkPayloadAsset.RootAgentPath}", $"../../../{FrameworkPayloadAsset.RootAgentPath}", StringComparison.Ordinal);
        Assert.Equal(Encoding.UTF8.GetBytes(expected), result.Bytes?.ToArray());
    }

    [Fact(DisplayName = "Reference definitions and images in fixed Skills point back to the selected root")]
    public void RebaseSharedReferenceDefinitionAndImage()
    {
        var original = $"[Guide][g]\n[Again][g]\n![Image][g]\n\n[g]: ../../{WorkspaceLayoutNames.LoaderFileName}{Fragment}\n";
        var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), SkillPath, Encoding.UTF8.GetBytes(original));
        var parsed = Parse(result);
        var expected = $"../../../{CustomRoot}/{WorkspaceLayoutNames.LoaderFileName}{Fragment}";
        Assert.Equal([expected, expected], parsed.Links.Select(link => link.RawDestination));
        Assert.Equal(expected, Assert.Single(parsed.Images).RawDestination);
    }

    [Fact(DisplayName = "Mapped link paths encode selected root spaces while keeping fragments")]
    public void EncodesSelectedRoot()
    {
        var bytes = Encoding.UTF8.GetBytes($"[Loader](../../{WorkspaceLayoutNames.LoaderFileName}{Fragment})");
        var result = WorkspaceLayoutPayloadRenderer.Render(Layout(SpacedRoot), SkillPath, bytes);
        Assert.Equal($"../../../{EncodedSpacedRoot}/{WorkspaceLayoutNames.LoaderFileName}{Fragment}", Assert.Single(Parse(result).Links).RawDestination);
    }

    [Fact(DisplayName = "Parsed angle destinations and workspace folder links rebase from their resolved package targets")]
    public void RebaseLiteralSpacesAndWorkspaceFolder()
    {
        var source = $"[Skill](<../{WorkspaceLayoutNames.SkillsDirectoryName}/{SpacedSkill}/{SourceFormClassifier.NativeSkillFileName}>)\n[Workspace](../../)";
        var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), PackageNote, Encoding.UTF8.GetBytes(source));
        Assert.Equal([
            $"../../../{WorkspaceLayout.NativeSkillsPath}/{EncodedSpacedSkill}/{SourceFormClassifier.NativeSkillFileName}",
            "../../../",
        ], Parse(result).Links.Select(link => link.RawDestination));
    }

    [Fact(DisplayName = "Managed hosts rebase their recognized loader references and preserve the Claude bridge")]
    public void RenderRecognizedHostReferences()
    {
        var agent = $"# {MarkdownManagedHostReader.HeadingText}\r\n\r\nRead `{FrameworkPayloadAsset.LoaderPath}`.\r\n\r\n**{MarkdownManagedHostReader.FooterText}**\r\n";
        var claude = $"# {MarkdownManagedHostReader.HeadingText}\n\n@{FrameworkPayloadAsset.RootAgentPath}\n@{FrameworkPayloadAsset.LoaderPath}\n\n**{MarkdownManagedHostReader.FooterText}**\n";
        var renderedAgent = Parse(WorkspaceLayoutPayloadRenderer.Render(Layout(), FrameworkPayloadAsset.RootAgentPath, Encoding.UTF8.GetBytes(agent)));
        var renderedClaude = Parse(WorkspaceLayoutPayloadRenderer.Render(Layout(), FrameworkPayloadAsset.RootClaudePath, Encoding.UTF8.GetBytes(claude)));
        CommandOutputSnapshot.MatchSnapshot(renderedAgent.Source + renderedClaude.Source, SnapshotName);
    }

    [Fact(DisplayName = "Authored host prose and code outside a recognized managed region retain their bytes")]
    public void PreserveAuthoredReferences()
    {
        var bytes = Encoding.UTF8.GetBytes($"@{FrameworkPayloadAsset.LoaderPath}\r\n{PreservedExample}");
        foreach (var path in new[] { FrameworkPayloadAsset.RootAgentPath, FrameworkPayloadAsset.RootClaudePath, SkillPath })
        {
            var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), path, bytes);
            Assert.Equal(bytes, result.Bytes?.ToArray());
        }
    }

    [Theory(DisplayName = "Layout rendering composes with both frontmatter forms and preserves native Skill metadata")]
    [InlineData(false), InlineData(true)]
    public void ComposeFrontmatter(bool rootForm)
    {
        var form = rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped;
        var source = Encoding.UTF8.GetBytes(MetadataFixture + $"[Loader](../../{WorkspaceLayoutNames.LoaderFileName})\n");
        var result = WorkspacePayloadRenderer.Render(Layout(), ReferencePath, source, form);
        var parsed = Parse(result);
        Assert.Equal($"../../../{CustomRoot}/{WorkspaceLayoutNames.LoaderFileName}", Assert.Single(parsed.Links).RawDestination);
        var metadata = new FrameworkDocumentMetadataParser().Parse(parsed, FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(form, metadata.Syntax.AuthoredForm);
        var skill = WorkspacePayloadRenderer.Render(Layout(), SkillPath, source, form);
        Assert.Equal(FrontmatterForm.Scoped,
            new FrameworkDocumentMetadataParser().Parse(Parse(skill), FrameworkMetadataReadScope.RoutedSource).Syntax.AuthoredForm);
    }

    [Fact(DisplayName = "Custom layout preserves opaque resources and reports invalid Markdown encoding")]
    public void BinaryAndInvalidEncoding()
    {
        var bytes = new byte[] { 0xFF };
        var binary = WorkspaceLayoutPayloadRenderer.Render(Layout(), WorkspaceLayout.NativeSkillsPath + "/" + ResourceName, bytes);
        Assert.Equal(bytes, binary.Bytes?.ToArray());
        var markdown = WorkspaceLayoutPayloadRenderer.Render(Layout(), PackageNote, bytes);
        Assert.Equal(FrameworkFrontmatterTransformState.Invalid, markdown.State);
        Assert.Null(markdown.Bytes);
    }

    private static MarkdownDocumentFacts Parse(FrameworkFrontmatterTransformResult result)
    {
        Assert.NotNull(result.Bytes);
        Assert.NotEqual(FrameworkFrontmatterTransformState.Invalid, result.State);
        return new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(result.Bytes.Value.Span));
    }

    private static WorkspaceLayout Layout(string root = CustomRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), root);
}
