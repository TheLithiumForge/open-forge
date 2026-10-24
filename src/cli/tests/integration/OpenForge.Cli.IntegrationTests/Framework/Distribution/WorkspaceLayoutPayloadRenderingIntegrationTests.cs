using System.Text;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Transformation.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.References.Shared.Resolution;
using OpenForge.Cli.Core.Framework.Sources.Shared.Destinations;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Framework.Distribution;

[Trait("Feature", "workspace-payload-rendering"), Trait("Evidence", "Integration"), Trait("Boundary", "Input")]
public sealed class WorkspaceLayoutPayloadRenderingIntegrationTests
{
    private const string CustomRoot = "docs/context";
    private const string SnapshotName = "custom-guidance.content";
    private const string AuthoredText = "# Authored\r\nKeep `" + SourceLogicalPath.HumanGuidePath + "` as an example.\n";
    private static readonly char[] CodeTokenSeparators = [' ', '\t', '\r', '\n', '`', '\'', '"', '[', ']', '(', ')', '{', '}', '<', '>', ',', '|', '=', '@', ':', '\\'];
    private static readonly HashSet<string> FixedPaths =
    [
        WorkspaceSettingsDefinitions.DirectoryName,
        WorkspaceSettingsDefinitions.RelativePath,
        WorkspaceOwnershipDefinitions.DirectoryName,
        WorkspaceOwnershipDefinitions.RelativePath,
        WorkspaceLayout.NativeSkillsPath,
    ];
    private static readonly string[] GuidancePaths =
    [
        FrameworkPayloadAsset.RootAgentPath,
        FrameworkPayloadAsset.RootClaudePath,
        SourceLogicalPath.HumanGuidePath,
        WorkspaceLayoutPayloadGuidance.CliSkillDirectory + "/" + SourceFormClassifier.NativeSkillFileName,
        Reference(WorkspaceLayoutPayloadGuidance.CommonReferenceName),
        Reference(WorkspaceLayoutPayloadGuidance.DiscoveryReferenceName),
        Reference(WorkspaceLayoutPayloadGuidance.RoutesReferenceName),
        Reference(WorkspaceLayoutPayloadGuidance.PackagesReferenceName),
    ];

    [Fact(DisplayName = "Every embedded asset retains exact bytes for default layout rendering")]
    public void DefaultPayloadBytesRemainUnchanged()
    {
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var layout = Layout(WorkspaceLayout.DefaultMainRoot);
        Assert.NotEmpty(payload.Assets);
        foreach (var asset in payload.Assets)
        {
            var result = WorkspaceLayoutPayloadRenderer.Render(layout, asset.Path, asset.Bytes.AsMemory());
            Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
            Assert.Equal(asset.Bytes.ToArray(), result.Bytes?.ToArray());
            var composed = WorkspacePayloadRenderer.Render(layout, asset.Path, asset.Bytes.AsMemory(), FrontmatterForm.Scoped);
            Assert.Equal(asset.Bytes.ToArray(), composed.Bytes?.ToArray());
        }
    }

    [Fact(DisplayName = "Embedded host and guidance templates render reviewed selected paths and exact operand examples")]
    public void CustomGuidanceMatchesReviewedTemplates()
    {
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var snapshot = new StringBuilder();
        foreach (var path in GuidancePaths)
        {
            var asset = Assert.IsType<FrameworkPayloadAsset>(payload.Find(path));
            var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), path, asset.Bytes.AsMemory());
            Assert.NotNull(result.Bytes);
            snapshot.AppendLine(path);
            snapshot.AppendLine(Encoding.UTF8.GetString(result.Bytes.Value.Span));
        }
        CommandOutputSnapshot.MatchDetailSnapshot(new Dictionary<string, string>
        {
            [SnapshotName] = snapshot.ToString(),
        });
    }

    [Fact(DisplayName = "Custom guidance code and links retain only fixed settings ownership and Skill paths under the default root")]
    public void CustomGuidanceReferencesOnlyFixedDefaultPaths()
    {
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        var layout = Layout();
        var referencedPaths = new List<string>();
        foreach (var packagePath in GuidancePaths)
        {
            var asset = Assert.IsType<FrameworkPayloadAsset>(payload.Find(packagePath));
            var result = WorkspaceLayoutPayloadRenderer.Render(layout, packagePath, asset.Bytes.AsMemory());
            Assert.NotNull(result.Bytes);
            var document = new MarkdownDocumentParser().Parse(Encoding.UTF8.GetString(result.Bytes.Value.Span));
            var codePaths = document.OpaqueSpans.Where(span => span.IsCode)
                .SelectMany(span => document.Source[span.Span.Start..span.Span.End]
                    .Split(CodeTokenSeparators, StringSplitOptions.RemoveEmptyEntries)).ToArray();
            referencedPaths.AddRange(codePaths);

            foreach (var link in document.Links.Concat(document.Images))
            {
                var path = link.RawDestination.Split('#', 2)[0];
                if (Uri.TryCreate(path, UriKind.Absolute, out _) || !SourceDestinationDecoder.TryDecode(path, out var decoded, out _)) continue;
                var resolved = SourceLinkLexicalPathResolver.Resolve(layout.Workspace.LexicalRoot, layout.MapPayloadPath(packagePath), decoded);
                if (resolved.CanonicalPath is { } destination) referencedPaths.Add(destination);
            }
        }
        Assert.NotEmpty(referencedPaths);
        Assert.All(referencedPaths, AssertFixedDefaultPath);
    }

    [Fact(DisplayName = "User-authored guide bytes do not acquire shipped template substitutions")]
    public void AuthoredGuidanceIsPreserved()
    {
        var bytes = Encoding.UTF8.GetBytes(AuthoredText);
        foreach (var path in GuidancePaths)
        {
            var result = WorkspaceLayoutPayloadRenderer.Render(Layout(), path, bytes);
            Assert.Equal(FrameworkFrontmatterTransformState.Unchanged, result.State);
            Assert.Equal(bytes, result.Bytes?.ToArray());
        }
    }

    [Theory(DisplayName = "All embedded assets render successfully with nested layouts in either frontmatter form")]
    [InlineData(false), InlineData(true)]
    public void EveryEmbeddedAssetRendersForCustomLayout(bool rootForm)
    {
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);
        foreach (var asset in payload.Assets)
        {
            var result = WorkspacePayloadRenderer.Render(Layout(), asset.Path, asset.Bytes.AsMemory(), rootForm ? FrontmatterForm.Root : FrontmatterForm.Scoped);
            Assert.NotEqual(FrameworkFrontmatterTransformState.Invalid, result.State);
            Assert.NotNull(result.Bytes);
        }
    }

    private static string Reference(string name)
        => WorkspaceLayoutPayloadGuidance.CliSkillDirectory + "/" + WorkspaceLayoutPayloadGuidance.ReferencesDirectoryName + "/" + name + SourceFormClassifier.MarkdownExtension;

    private static void AssertFixedDefaultPath(string value)
    {
        var path = value.StartsWith(SourceWorkspaceRelativePath.LiteralReferencePrefix, StringComparison.Ordinal)
            ? value[SourceWorkspaceRelativePath.LiteralReferencePrefix.Length..] : value;
        path = path.TrimEnd('/');
        if (path.Split('/')[0] != WorkspaceLayout.DefaultMainRoot) return;
        Assert.True(FixedPaths.Contains(path) || path.StartsWith(WorkspaceLayout.NativeSkillsPath + "/", StringComparison.Ordinal), path);
    }

    private static WorkspaceLayout Layout(string root = CustomRoot)
        => WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), root);
}
