using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Init.Shared.Planning;

[Trait("Feature", "route-init-outside"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteInitOutsidePlanningIntegrationTests
{
    [Theory(DisplayName = "Registered outside index and references anchors retain canonical exact-path initialization")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.ReferencesFileName)]
    public async Task CompatibilityNamedOutsideCanonicalAnchorsRemainUsable(string root, string compatibilityFile)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, listed: false,
            directory: "outside/" + Path.GetFileNameWithoutExtension(compatibilityFile));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var preview = await workspace.Execute(workspace.Request(workspace.Anchor, RouteInitMode.DryRun));
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        var applied = await workspace.Execute(workspace.Request(workspace.Anchor));
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteInitVerificationState.Verified, applied.Verification);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + workspace.Anchor, applied.Target.Id);
        Assert.True(File.Exists(workspace.Files.Combine(workspace.Anchor)));
        workspace.AssertPreserved(before);
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeated = await workspace.Execute(workspace.Request(workspace.Anchor));
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Empty(repeated.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init retains a compatibility anchor while initializing descendants and exact repeat selection")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(RouteInitOutsideWorkspace.CustomRoot)]
    public async Task CompatibilityAnchorBoundsTheDescendantChain(string root)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, listed: false);
        var compatibility = RouteInitOutsideWorkspace.Directory + "/" + SourceFormClassifier.IndexFileName;
        workspace.Files.WriteText(compatibility, RouteInitOutsideWorkspace.Document());
        var before = workspace.Files.SnapshotHashes();
        var target = WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory);
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(RouteInitVerificationState.Verified, result.Verification);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + compatibility, result.Entrypoints[0].Id);
        Assert.False(File.Exists(workspace.Files.Combine(workspace.Anchor)));
        workspace.AssertPreserved(before);
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeat = await workspace.Execute(workspace.Request(compatibility));
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init restores a registered missing anchor or nested chain without relisting")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, true, true)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, false, false)]
    public async Task MissingChainIsBounded(string root, bool listed, bool nested)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, listed);
        var target = nested ? WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory) : workspace.Anchor;
        var plan = await workspace.Plan(target);
        Assert.Equal(nested ? 3 : 1, plan.Preview.Entrypoints.Length);
        Assert.Equal(workspace.Anchor, plan.Preview.Entrypoints[0].Path);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + target, plan.Preview.Target.Id);
        Assert.Equal(workspace.Anchor, plan.LayoutRead.OutsideRoutes.Single().Descriptor.CataloguePath);
        Assert.All(plan.Preview.Effects, effect => Assert.StartsWith(RouteInitOutsideWorkspace.Directory + "/", effect.Path, StringComparison.Ordinal));
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var preview = await workspace.Execute(workspace.Request(target, RouteInitMode.DryRun));
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
        var applied = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(RouteInitVerificationState.Verified, applied.Verification);
        workspace.AssertPreserved(before);
        Assert.False(File.Exists(workspace.Files.Combine(WorkspaceRoutePath.CanonicalEntrypointPath("docs"))));
        var after = workspace.Files.SnapshotHashes(includeRuntime: true);
        var repeat = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Equal(after, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory(DisplayName = "Outside Init retains an existing compatibility entrypoint while filling ancestors")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.IndexFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreIndexFileName)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, SourceFormClassifier.ReferencesFileName)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, SourceFormClassifier.UnderscoreReferencesFileName)]
    public async Task ExistingCompatibilityIsPreserved(string root, string file)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, listed: false);
        var target = RouteInitOutsideWorkspace.NestedDirectory + "/" + file;
        workspace.Files.WriteText(target, RouteInitOutsideWorkspace.Document());
        var bytes = File.ReadAllBytes(workspace.Files.Combine(target));
        var before = workspace.Files.SnapshotHashes();
        var result = await workspace.Execute(workspace.Request(target));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(bytes, File.ReadAllBytes(workspace.Files.Combine(target)));
        Assert.False(File.Exists(workspace.Files.Combine(WorkspaceRoutePath.CanonicalEntrypointPath(RouteInitOutsideWorkspace.NestedDirectory))));
        Assert.Equal(RouteInitEntrypointForm.Compatibility, result.Entrypoints[^1].Form);
        workspace.AssertPreserved(before);
    }

    [Theory(DisplayName = "Outside category-like names keep Init draft and final-only authored metadata")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, WorkspaceCategoryNames.Directives)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, WorkspaceCategoryNames.Memory)]
    [InlineData(RouteInitOutsideWorkspace.CustomRoot, WorkspaceLayoutNames.SkillsDirectoryName)]
    public async Task MetadataHasNoCategorySemantics(string root, string directoryName)
    {
        using var workspace = new RouteInitOutsideWorkspace(root, directory: "outside/" + directoryName);
        if (root == RouteInitOutsideWorkspace.CustomRoot)
            workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, workspace.Settings(["outside/" + directoryName],
                $",\"{WorkspaceSettingsDefinitions.FrontmatterProperty}\":\"{WorkspaceSettingsDefinitions.FrontmatterRootName}\""));
        var target = WorkspaceRoutePath.CanonicalEntrypointPath("outside/" + directoryName + "/child");
        var metadata = new RouteInitMetadataInput(RouteInitOutsideWorkspace.Description, true, "Own the authored child.", [RouteInitOutsideWorkspace.Tag]);
        var result = await workspace.Execute(workspace.Request(target, metadata: metadata));
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteInitDescriptionSource.Draft, result.Entrypoints[0].Metadata?.DescriptionSource);
        Assert.Equal(RouteInitTagsSource.Draft, result.Entrypoints[0].Metadata?.TagsSource);
        Assert.Contains(result.Findings, finding => finding.Code == RouteInitFindingCode.NeedsAuthoring);
        Assert.Equal([RouteInitOutsideWorkspace.Tag], result.Entrypoints[^1].Metadata?.Tags);
        Assert.Equal(RouteInitTagsSource.Explicit, result.Entrypoints[^1].Metadata?.TagsSource);
        var facts = new FrameworkDocumentMetadataParser().Parse(new MarkdownDocumentParser().Parse(
            File.ReadAllText(workspace.Files.Combine(target))), FrameworkMetadataReadScope.RoutedSource);
        Assert.Equal(RouteInitOutsideWorkspace.Description, facts.Metadata?.Description);
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var invalid = await workspace.Execute(workspace.Request(target, metadata: metadata));
        Assert.Equal(CliSemanticStatus.Invalid, invalid.Status);
        Assert.Contains(invalid.Findings, finding => finding.Code == RouteInitFindingCode.InvalidMetadata);
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }
}
