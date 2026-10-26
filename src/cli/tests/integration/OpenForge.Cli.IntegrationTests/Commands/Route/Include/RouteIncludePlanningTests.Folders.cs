using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Documents.Shared.Entries;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed partial class RouteIncludePlanningTests
{
    [Theory(DisplayName = "Folder preview preserves existing bytes and plans only the canonical anchor when absent"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task FolderPreview(bool custom, bool anchor)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, anchor);
        var before = workspace.Files.SnapshotHashes();
        var build = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace) with { DryRun = true }, RouteInclusionFolderFixture.Token);
        var plan = Assert.IsType<RouteInclusionPlan>(build.Plan);
        Assert.Equal(CliSemanticStatus.Complete, build.Outcome.Status);
        Assert.Equal(RouteInclusionTargetKind.Folder, plan.Target.Kind);
        Assert.Equal(RouteInclusionFolderFixture.Folder, build.Outcome.Path);
        Assert.Equal(RouteInclusionFolderFixture.Catalogue, build.Outcome.Catalogue);
        Assert.Empty(plan.LayoutRead.Layout!.OutsideRoutes);
        Assert.Equal(RouteInclusionFolderFixture.Folder, Assert.Single(plan.IntendedLayout.OutsideRoutes).DirectoryPath);
        Assert.Contains(plan.Target.Expectations, item => item.Kind == FileExpectationKind.Directory);
        Assert.Equal(anchor ? 2 : 3, plan.Changes.Length);
        Assert.Equal(plan.Changes.Length, plan.RecoveryTargets.Length);
        Assert.All(plan.RecoveryTargets, item => Assert.True(item.RequiresRecovery));
        var scaffold = RouteInclusionFolderPlanner.Document(plan.Target, plan.LayoutRead);
        MarkdownEntryRowParser.TryParse(build.Outcome.After!, out var row, out _);
        Assert.Equal(scaffold.Metadata.Description, row!.Description);
        Assert.Empty(row.Tags);
        Assert.Empty(row.ApplyTo);
        if (!anchor)
        {
            Assert.Equal(RouteInclusionEffectKind.Catalogue, build.Outcome.Effects[0].Kind);
            var created = plan.Changes[0];
            Assert.Equal(PlannedFileChangeKind.Create, created.Kind);
            Assert.Equal(RoutedMetadataDefaults.Tags(RoutedMetadataCategory.Workspace), scaffold.Metadata.Tags);
            Assert.Equal(new RouteScaffoldComposer().Compose(scaffold), created.IntendedBytes);
        }
        Assert.False(build.Outcome.RegisteredBefore);
        Assert.True(build.Outcome.RegisteredAfter);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Folder Loader metadata merges by field without reading policy from catalogue frontmatter"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderAuthoredRow(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom, true);
        var row = $"- [Handwritten]({RouteInclusionFolderFixture.Destination(workspace)}) - #Docs #LoadNow - applies to `src/**`";
        RouteInclusionFolderFixture.Rows(workspace, row + "\n- [Unrelated](../elsewhere.md) - #Other");
        var loader = workspace.Bytes(RouteInclusionFolderFixture.Loader(workspace));
        var anchor = workspace.Bytes(RouteInclusionFolderFixture.Catalogue);
        var request = RouteInclusionFolderFixture.Request(workspace);
        var preserved = await new RouteInclusionPlanBuilder().BuildAsync(request, RouteInclusionFolderFixture.Token);
        Assert.Equal(row, preserved.Outcome.After);
        Assert.Equal(RouteInclusionEffectKind.Settings, Assert.Single(preserved.Outcome.Effects).Kind);
        foreach (var replacement in new[]
        {
            request with { Description = "Chosen row" }, request with { Tags = ["Review"] },
            request with { ApplyTo = ApplyToPatternExpressionParser.Parse("docs/**").Patterns },
        })
        {
            var changed = await new RouteInclusionPlanBuilder().BuildAsync(replacement, RouteInclusionFolderFixture.Token);
            Assert.NotNull(changed.Plan);
            MarkdownEntryRowParser.TryParse(changed.Outcome.After!, out var merged, out _);
            Assert.Equal(replacement.Description ?? "Handwritten", merged!.Description);
            Assert.Equal(replacement.Tags ?? ["Docs", "LoadNow"], merged.Tags);
            Assert.Equal((replacement.ApplyTo ?? ApplyToPatternExpressionParser.Parse("src/**").Patterns).Select(pattern => pattern.Text), merged.ApplyTo.Select(pattern => pattern.Text));
        }
        Assert.Equal(loader, workspace.Bytes(RouteInclusionFolderFixture.Loader(workspace)));
        Assert.Equal(anchor, workspace.Bytes(RouteInclusionFolderFixture.Catalogue));
    }

    [Theory(DisplayName = "Folder Include refuses reserved overlaps and lexical aliases without infrastructure"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(false), InlineData(true)]
    public async Task FolderRefusalMatrix(bool custom)
    {
        using var workspace = RouteInclusionFolderFixture.Create(custom);
        workspace.Files.CreateDirectory(WorkspaceRuntimePaths.DirectoryName);
        workspace.Files.CreateDirectory(WorkspaceLayoutNames.GitMetadataDirectoryName);
        var root = RouteInclusionFolderFixture.Settings(workspace).MainRoot;
        foreach (var path in new[] { ".", root, WorkspaceLayout.DefaultMainRoot, WorkspaceRuntimePaths.DirectoryName,
            WorkspaceLayoutNames.GitMetadataDirectoryName, "project/../" + RouteInclusionFolderFixture.Folder })
        {
            var before = workspace.Files.SnapshotHashes();
            var result = await new RouteInclusionPlanBuilder().BuildAsync(RouteInclusionFolderFixture.Request(workspace) with { File = path }, RouteInclusionFolderFixture.Token);
            Assert.Equal(CliSemanticStatus.Invalid, result.Outcome.Status);
            Assert.Null(result.Plan);
            Assert.Equal(before, workspace.Files.SnapshotHashes());
        }
        Assert.False(workspace.Locks.InfrastructureExists);
    }
}
