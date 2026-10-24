using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Models.Result;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Boundary", "OS"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
public sealed class RouteListMainRootIntegrationTests
{
    private const string MainRoot = "knowledge/team/context";
    private const string Route = "policies";
    private const string CatalogueName = "_" + Route + SourceFormClassifier.MarkdownExtension;
    private const string CataloguePath = MainRoot + "/" + Route + "/" + CatalogueName;
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Leaf = "conduct";
    private const string LeafPath = MainRoot + "/" + Route + "/" + Leaf + SourceFormClassifier.MarkdownExtension;
    private const string MissingRoute = "missing";
    private const string MissingDestination = MissingRoute + "/_" + MissingRoute + SourceFormClassifier.MarkdownExtension;
    private const string NativeCatalogue = WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension;
    private const string UnlistedPath = "handbook/entry" + SourceFormClassifier.MarkdownExtension;
    private const string IncludedPath = "handbook/overview" + SourceFormClassifier.MarkdownExtension;
    private const string Tag = "Policy";

    [Fact]
    public async Task CustomInventoryRetainsObservationAndIgnoresAbsentOptionalSkills()
    {
        using var workspace = Create();
        var request = Request(workspace);
        var resolver = new RouteListSourceResolver(new RouteListInventoryReader(),
            new RouteListSelectionResolver(new PhysicalPathResolver()), new SourceRouteFactsResolver());

        var source = await resolver.ResolveAsync(request, TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceLayoutReadState.Ready, source.LayoutRead.State);
        Assert.Same(source.LayoutRead.Layout, source.Inventory.SourceCatalogue.Layout);
        Assert.Equal(new[] { CataloguePath, LeafPath, LoaderPath }.Order(StringComparer.Ordinal),
            source.Inventory.SourceCatalogue.Sources.Select(item => item.Base.CanonicalPath).Order(StringComparer.Ordinal));
        Assert.Empty(source.Inventory.Findings);
        Assert.False(Directory.Exists(workspace.Combine(WorkspaceLayout.NativeSkillsPath)));
    }

    [Fact]
    public async Task CustomLoaderMissingDestinationKeepsAuthoredSubject()
    {
        using var workspace = Create();
        workspace.ReplaceText(LoaderPath, Loader($"- [Missing]({MissingDestination}) - #{Tag}"));

        var result = await Run(workspace);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.LoaderUnavailable
            && finding.Subject == MissingDestination);
        Assert.Equal(LoaderPath, result.PresentationFacts.CustomLoaderPath);
    }

    [Fact]
    public async Task InvalidSettingsDoNotReadTheDefaultLoader()
    {
        using var workspace = Create();
        workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":false}""");
        workspace.WriteText(SourceLogicalPath.LoaderPath, Loader(string.Empty));

        var result = await Run(workspace);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(result.Rows);
        Assert.Contains(result.Findings, finding => finding.Code == RouteListFindingCode.WorkspaceUnavailable
            && finding.Subject == WorkspaceSettingsDefinitions.RelativePath);
    }

    [Fact]
    public async Task ConflictingDefaultLoaderKeepsTheConfiguredReadOnlyLayout()
    {
        using var workspace = Create();
        workspace.WriteText(SourceLogicalPath.LoaderPath, Loader(string.Empty));

        var result = await Run(workspace);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(new[] { CataloguePath, LeafPath }, result.Rows.Select(row => row.Path));
    }

    [Fact]
    public async Task IncludedAndUnlistedRootsUseTheConfiguredLoader()
    {
        using var workspace = Create();
        workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}","{{WorkspaceSettingsDefinitions.RootsProperty}}":["{{IncludedPath}}","{{UnlistedPath}}"]}""");
        workspace.ReplaceText(LoaderPath, Loader($"- [Overview](../../../{IncludedPath}) - #{Tag}"));
        workspace.WriteText(IncludedPath, "# Overview\n");
        workspace.WriteText(UnlistedPath, "# Unlisted\n");

        var result = await Run(workspace);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var included = Assert.Single(result.Rows, row => row.Path == IncludedPath);
        Assert.Equal(0, included.AbsoluteDepth);
        Assert.Null(included.ParentPath);
        Assert.Equal(RouteListSelectionProvenance.LoaderRoot, included.Provenance.Selection);
        Assert.True(Assert.Single(result.Rows, row => row.Path == UnlistedPath).IsUnlisted);
        Assert.DoesNotContain(result.Rows, row => row.Path == CataloguePath);
    }

    [Fact]
    public async Task MissingNativeCatalogueRetainsItsLoaderRelativeDestination()
    {
        using var workspace = Create();
        var destination = "../../../" + NativeCatalogue;
        workspace.ReplaceText(LoaderPath, Loader($"- [Skills]({destination}) - #{Tag}"));

        var result = await Run(workspace);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Contains(result.Findings, finding => finding.Subject == destination);
    }

    private static TemporaryWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create(nameof(RouteListMainRootIntegrationTests));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath,
            $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}"}""");
        workspace.WriteText(LoaderPath, Loader($"- [Policies]({Route}/{CatalogueName}) - #{Tag}"));
        workspace.WriteText(CataloguePath, OpenForgeDocumentSeed.Metadata(Route, [Tag], "# Policies\n"));
        workspace.WriteText(LeafPath, OpenForgeDocumentSeed.Metadata(Leaf, [Tag], "# Conduct\n"));
        return workspace;
    }

    private static string Loader(string entries) => $"# Loader\n\n## {MarkdownEntriesSectionReader.EntriesHeadingText}\n\n{entries}\n";

    private static RouteListRequest Request(TemporaryWorkspace workspace) => new(
        new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory), null, RouteListDepth.All);

    private static ValueTask<RouteListResult> Run(TemporaryWorkspace workspace)
        => RouteListOperationFactory.Create()(Request(workspace), TestContext.Current.CancellationToken);
}
