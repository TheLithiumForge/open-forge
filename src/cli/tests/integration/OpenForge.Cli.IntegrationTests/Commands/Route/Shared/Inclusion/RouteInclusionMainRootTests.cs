using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Commands.Route.Exclude;
using OpenForge.Cli.Core.Commands.Route.Exclude.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Application;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;

public sealed class RouteInclusionMainRootTests
{
    private const string MainRoot = "docs/context";
    private const string OtherRoot = "docs/changed";
    private const string Category = "topics";
    private const string CatalogueName = "_" + Category + SourceFormClassifier.MarkdownExtension;
    private const string CataloguePath = MainRoot + "/" + Category + "/" + CatalogueName;
    private const string SkillsCatalogue = WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension;
    private const string ForeignPath = "team/guide.md";
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Catalogue = "---\ndescription: Topics\ntags: [LoadNow]\n---\n\n# Topics\n\n## Entries\n\n- none - No entries - #Empty\n";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Custom Include and Exclude keep native catalogues distinct from literal foreign rows"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    [InlineData(CataloguePath, false)]
    [InlineData(SkillsCatalogue, false)]
    [InlineData(ForeignPath, true)]
    public async Task MembershipAndRegistration(string path, bool foreign)
    {
        using var workspace = Create();
        var bytes = workspace.Bytes(path);
        var include = new RouteIncludeOperation(workspace.Locks.StoreRoot);
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = path, Description = foreign ? "Team guide" : null };
        var included = await include.ExecuteAsync(request, Token);
        Assert.Equal(CliSemanticStatus.Complete, included.Status);
        Assert.True(included.Outcome.Verified);
        Assert.Equal(foreign, included.Outcome.RegisteredAfter);
        Assert.Equal(RouteInclusionEffectKind.LoaderEntries, included.Outcome.Effects[0].Kind);
        Assert.Equal(LoaderPath, included.Outcome.Effects[0].Path);
        Assert.Equal(foreign ? 2 : 1, included.Outcome.Effects.Length);
        if (foreign) Assert.Equal(RouteInclusionEffectKind.Settings, included.Outcome.Effects[1].Kind);
        var layout = WorkspaceLayout.Create(workspace.Workspace, MainRoot);
        Assert.Contains(path, SourceLoaderEntriesParser.Parse(layout, workspace.Read(LoaderPath)).Destinations.Select(row => row.CanonicalPath));
        var settings = await WorkspaceSettingsReader.ReadAsync(new PhysicalPathResolver(), workspace.Workspace, Token);
        Assert.Equal(foreign ? new[] { path } : [], settings.Document.Roots);
        Assert.Empty(settings.Document.AllowInstallPaths);
        if (foreign)
        {
            var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadIncludedAsync(layout, Token);
            var source = Assert.Single(session.Catalogue.Sources, item => item.Base.CanonicalPath == path);
            Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + path, source.Identity.AutomaticId);
        }
        Assert.True((await include.ExecuteAsync(request, Token)).Outcome.NoOp);
        var excluded = await new RouteExcludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(
            new RouteExcludeRequest { Workspace = workspace.Workspace, File = path }, Token);
        Assert.Equal(CliSemanticStatus.Complete, excluded.Status);
        Assert.True(excluded.Outcome.Verified);
        Assert.Empty(SourceLoaderEntriesParser.Parse(layout, workspace.Read(LoaderPath)).Destinations);
        Assert.Equal(bytes, workspace.Bytes(path));
        Assert.False(File.Exists(workspace.Files.Combine(SourceLogicalPath.LoaderPath)));
    }

    [Theory(DisplayName = "A changed layout observation blocks Include and Exclude before effects"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangedLayoutStopsApplication(bool exclude)
    {
        using var workspace = Create();
        if (exclude) workspace.Files.ReplaceText(LoaderPath, GeneratedLoaderDocumentBuilder.Build($"- [Guide](../../{ForeignPath})"));
        var request = new RouteInclusionRequest
        {
            Workspace = workspace.Workspace,
            File = ForeignPath,
            Description = "Guide",
            Action = exclude ? RouteInclusionAction.Exclude : RouteInclusionAction.Include,
        };
        var planned = await new RouteInclusionPlanBuilder().BuildAsync(request, Token);
        var plan = Assert.IsType<RouteInclusionPlan>(planned.Plan);
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot));
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteInclusionApplication(workspace.Locks.StoreRoot).ApplyAsync(plan, Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInclusionFindingCode.TargetChanged, result.Code);
        Assert.All(result.Effects, effect => Assert.Equal(RouteInclusionEffectState.NotStarted, effect.State));
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "A conflicting default loader blocks custom Include without writes"), Trait("Feature", "route-inclusion"), Trait("Evidence", "Integration")]
    public async Task ConflictingLoader()
    {
        using var workspace = Create();
        File.WriteAllText(workspace.Files.Combine(SourceLogicalPath.LoaderPath), GeneratedLoaderDocumentBuilder.Build(MarkdownEntriesSectionReader.EmptyEntry));
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation(workspace.Locks.StoreRoot).ExecuteAsync(
            new RouteIncludeRequest { Workspace = workspace.Workspace, File = ForeignPath, Description = "Guide" }, Token);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(RouteInclusionFindingCode.WorkspaceUnsafe, result.Outcome.Code);
        Assert.Empty(result.Outcome.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    private static RouteInclusionWorkspace Create()
    {
        var workspace = new RouteInclusionWorkspace();
        File.Delete(workspace.Files.Combine(SourceLogicalPath.LoaderPath));
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        workspace.Files.WriteText(LoaderPath, GeneratedLoaderDocumentBuilder.Build(MarkdownEntriesSectionReader.EmptyEntry));
        workspace.Files.WriteText(CataloguePath, Catalogue);
        workspace.Files.WriteText(SkillsCatalogue, Catalogue);
        workspace.Files.WriteText(ForeignPath, "# Foreign\r\n\r\nKeep exact bytes.\r\n");
        return workspace;
    }

    private static string Settings(string root)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}""";
}
