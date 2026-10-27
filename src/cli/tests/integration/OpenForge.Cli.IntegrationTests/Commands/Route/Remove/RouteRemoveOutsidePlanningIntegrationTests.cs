using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Documents.Markdown;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

[Trait("Boundary", "Host"), Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
public sealed class RouteRemoveOutsidePlanningIntegrationTests
{
    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot)]
    public async Task CategoryLikeOutsideNamesNeverCreatePrimaryCategoryExclusions(string root)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root);
        const string category = RouteRemoveOutsideWorkspace.DirectoryPath + "/guidance";
        var entrypoint = WorkspaceRoutePath.CanonicalEntrypointPath(category);
        const string row = "- [Independent Guidance](guidance/_guidance.md) - #Authored\r\n";
        var parent = RouteRemoveOutsideWorkspace.Entry(RouteRemoveOutsideWorkspace.SurvivingRows + row);
        fixture.Inner.WriteText(fixture.Anchor, parent);
        fixture.Inner.WriteText(entrypoint, RouteRemoveOutsideWorkspace.Entry(MarkdownEntriesSectionReader.EmptyEntry));
        var result = await fixture.Execute(entrypoint);
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Empty(fixture.Inner.ReadSettings().RemovedCategories);
        Assert.Equal([category], fixture.Inner.ReadSettings().RemovedDirectories);
        Assert.Equal(parent.Replace(row, string.Empty, StringComparison.Ordinal), fixture.Inner.ReadText(fixture.Anchor));
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot, true, false)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, false, false)]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false, true)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot, true, true)]
    public async Task ExactLeafRetainsLayoutAndAuthoredRows(string root, bool listed, bool compatibility)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, listed, compatibility);
        var before = fixture.Inner.SnapshotHashes();
        var plan = await fixture.Plan();
        Assert.Same(plan.LayoutRead, plan.Projection.Subject.LayoutRead);
        Assert.Same(plan.LayoutRead.Settings, plan.Projection.Settings);
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + RouteRemoveOutsideWorkspace.Leaf, plan.Preview.Source.Id);
        Assert.Equal(RouteRemoveOutsideWorkspace.Leaf, plan.Preview.Source.Path);
        Assert.Equal([RouteRemoveOutsideWorkspace.Leaf, RouteRemoveOutsideWorkspace.Overwrite], plan.Projection.RemovalSelection.Files);
        Assert.Empty(plan.Projection.RemovalSelection.Categories);
        Assert.All(plan.Projection.Navigation.GeneratedNavigation.Regions,
            region => Assert.StartsWith(RouteRemoveOutsideWorkspace.DirectoryPath + "/", region.Path, StringComparison.Ordinal));
        Assert.DoesNotContain(plan.Projection.FileChanges, change => change.LogicalPath == fixture.Inner.Combine(fixture.Layout.LoaderPath));
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot)]
    public async Task ApplyPreservesSurvivingRowsAndConverges(string root)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, listed: false);
        var before = fixture.Inner.SnapshotHashes();
        var parent = fixture.Inner.ReadText(fixture.Anchor);
        var loader = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        var keep = fixture.Inner.ReadBytes(RouteRemoveOutsideWorkspace.Survivor);
        var preview = await fixture.Execute(mode: RouteRemoveMode.DryRun);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
        var result = await fixture.Execute();
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.Equal(RouteRemoveVerificationState.Verified, result.Verification);
        Assert.False(File.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Leaf)));
        Assert.False(File.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Overwrite)));
        Assert.Equal(parent.Replace(RouteRemoveOutsideWorkspace.RemovedRow, string.Empty, StringComparison.Ordinal), fixture.Inner.ReadText(fixture.Anchor));
        Assert.Equal(loader, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
        Assert.Equal(keep, fixture.Inner.ReadBytes(RouteRemoveOutsideWorkspace.Survivor));
        Assert.Equal([RouteRemoveOutsideWorkspace.DirectoryPath], fixture.Inner.ReadSettings().OutsideRoutes);
        Assert.Empty(fixture.Inner.ReadSettings().RemovedCategories);
        using var settings = JsonDocument.Parse(fixture.Inner.ReadBytes(WorkspaceSettingsDefinitions.RelativePath));
        Assert.True(settings.RootElement.GetProperty("independent").GetProperty("preserve").GetBoolean());
        var after = fixture.Inner.SnapshotHashes();
        var repeat = await fixture.Execute();
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
        Assert.Empty(repeat.Findings);
        Assert.Equal(after, fixture.Inner.SnapshotHashes());
    }

    [Theory]
    [InlineData(WorkspaceLayout.DefaultMainRoot)]
    [InlineData(RouteRemoveOutsideWorkspace.CustomRoot)]
    public async Task CompleteCategoryUsesLiteralExclusionAndPreservesRegistration(string root)
    {
        using var fixture = new RouteRemoveOutsideWorkspace(root, listed: false, compatibility: true);
        fixture.SeedCategory();
        var loader = fixture.Inner.ReadBytes(fixture.Layout.LoaderPath);
        var parent = fixture.Inner.ReadText(fixture.Anchor);
        var result = await fixture.Execute(WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category));
        Assert.True(result.Status == CliSemanticStatus.Complete, string.Join(Environment.NewLine, result.Findings.Select(finding => finding.Cause)));
        Assert.False(Directory.Exists(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.Category)));
        Assert.Equal(parent.Replace("- [Topics](topics/_topics.md) - #Authored\r\n", string.Empty, StringComparison.Ordinal), fixture.Inner.ReadText(fixture.Anchor));
        Assert.Equal([RouteRemoveOutsideWorkspace.Category], fixture.Inner.ReadSettings().RemovedDirectories);
        Assert.Empty(fixture.Inner.ReadSettings().RemovedCategories);
        Assert.Equal([RouteRemoveOutsideWorkspace.DirectoryPath], fixture.Inner.ReadSettings().OutsideRoutes);
        Assert.Equal(loader, fixture.Inner.ReadBytes(fixture.Layout.LoaderPath));
        var repeat = await fixture.Execute(WorkspaceRoutePath.CanonicalEntrypointPath(RouteRemoveOutsideWorkspace.Category));
        Assert.Equal(CliSemanticStatus.Complete, repeat.Status);
        Assert.Empty(repeat.Effects);
    }

    [Theory]
    [InlineData("foreign-prose")]
    [InlineData("foreign-row")]
    [InlineData("file-only")]
    [InlineData("missing-anchor")]
    [InlineData("ambiguous-anchor")]
    [InlineData("ordinal-alias")]
    [InlineData("unavailable-registration")]
    [InlineData("malformed-row")]
    [InlineData("foreign-independent-row")]
    [InlineData("ordinal-prefix")]
    public async Task RequiredAuthorityOrForeignRewriteRefusesWithoutEffects(string scenario)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var target = RouteRemoveOutsideWorkspace.Leaf;
        switch (scenario)
        {
            case "foreign-prose": fixture.Inner.WriteText("incoming.md", "Before [old](docs/team/old.md) after.\n"); break;
            case "foreign-row": fixture.Inner.WriteText("included.md", RouteRemoveOutsideWorkspace.Entry("- [Old](docs/team/old.md) - #Foreign")); break;
            case "file-only": fixture.Inner.WriteText(WorkspaceSettingsDefinitions.RelativePath, fixture.Settings($",\"{WorkspaceSettingsDefinitions.RootsProperty}\":[\"{target}\"]", [])); break;
            case "missing-anchor": fixture.Inner.DeleteFile(fixture.Anchor); break;
            case "ambiguous-anchor": fixture.Inner.WriteText(RouteRemoveOutsideWorkspace.DirectoryPath + "/index.md", RouteRemoveOutsideWorkspace.Entry(MarkdownEntriesSectionReader.EmptyEntry)); break;
            case "ordinal-alias": target = "Docs/team/old.md"; break;
            case "ordinal-prefix": target = "docs/team-other/old.md"; fixture.Inner.WriteText(target, "Independent content\n"); break;
            case "malformed-row": fixture.Inner.WriteText(fixture.Anchor, fixture.Inner.ReadText(fixture.Anchor).Replace($"#{SourceLoadingClosureResolver.LoadNowTag} #Authored - applies to `old/**`", "malformed policy", StringComparison.Ordinal)); break;
            case "foreign-independent-row": fixture.Inner.WriteText(fixture.Layout.LoaderPath, RouteRemoveOutsideWorkspace.Entry("- [Direct old](../docs/team/old.md) - #Independent")); break;
            case "unavailable-registration": fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Leaf); fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Overwrite); fixture.Inner.DeleteFile(RouteRemoveOutsideWorkspace.Survivor); fixture.Inner.DeleteFile(fixture.Anchor); Directory.Delete(fixture.Inner.Combine(RouteRemoveOutsideWorkspace.DirectoryPath)); break;
        }
        var before = fixture.Inner.SnapshotHashes();
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(fixture.Request(target), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.NotEmpty(build.Formation.Findings);
        Assert.Empty(build.Formation.Effects);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
        fixture.Inner.AssertNoLockInfrastructure();
    }

    [Theory]
    [InlineData("docs")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath)]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/_team.md")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/_team.overwrite.md")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/index.md")]
    [InlineData(RouteRemoveOutsideWorkspace.DirectoryPath + "/_index.md")]
    public async Task RegisteredBoundariesAndAnchorsAreProtectedEvenWhenAbsent(string path)
    {
        using var fixture = new RouteRemoveOutsideWorkspace();
        var before = fixture.Inner.SnapshotHashes();
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(fixture.Request(path), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteRemoveFindingCode.ProtectedTarget);
        Assert.Equal(before, fixture.Inner.SnapshotHashes());
    }
}
