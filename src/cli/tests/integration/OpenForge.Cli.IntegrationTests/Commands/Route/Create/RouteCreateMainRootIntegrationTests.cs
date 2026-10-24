using OpenForge.Cli.Core.Commands.Route.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Create;

[Trait("Feature", "route-create"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RouteCreateMainRootIntegrationTests
{
    private const string MainRoot = "handbook/context";
    private const string OtherRoot = "handbook/other";
    private const string Route = "notes";
    private const string Nested = "team";
    private const string Leaf = "checklist";
    private const string Description = "Release notes";
    private const string Tag = "Docs";
    private const string ParentName = "_notes" + SourceFormClassifier.MarkdownExtension;
    private const string NestedName = "_team" + SourceFormClassifier.MarkdownExtension;
    private const string ParentPath = MainRoot + "/" + Route + "/" + ParentName;
    private const string LoaderPath = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string TargetId = Route + "/" + Nested + "/" + Leaf;
    private const string TargetPath = MainRoot + "/" + TargetId + SourceFormClassifier.MarkdownExtension;
    private const string ChangedBody = "# Changed after preview\n";

    public enum Change { Settings, Loader, DefaultLoader, Host }

    [Fact(DisplayName = "Route Create maps nested IDs and literal paths through the observed custom layout")]
    public async Task NestedPlanUsesStableIdsAndStopsAtMainRoot()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(NestedPlanUsesStableIdsAndStopsAtMainRoot));
        Seed(temporary);
        var before = temporary.SnapshotHashes();
        var plan = await Plan(temporary, TargetId);
        var literal = await Plan(temporary, SourceWorkspaceRelativePath.LiteralReferencePrefix + TargetPath);
        Assert.Equal(MainRoot, plan.LayoutRead.Layout?.MainRoot);
        Assert.Equal(TargetId, plan.TargetSource.Identity.AutomaticId);
        Assert.Equal(TargetPath, plan.TargetSource.Identity.CanonicalBasePath);
        Assert.Equal(Route + "/" + Nested, plan.ParentSource.Identity.AutomaticId);
        Assert.Equal(MainRoot + "/" + Route + "/" + Nested + "/" + NestedName, plan.ParentSource.Identity.CanonicalBasePath);
        Assert.Equal(plan.Preview.Effects.Select(effect => effect.Path), literal.Preview.Effects.Select(effect => effect.Path));
        Assert.Single(plan.DirectoryCreations);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(new[] { MainRoot + "/" + Route + "/" + Nested,
            MainRoot + "/" + Route + "/" + Nested + "/" + NestedName, TargetPath, ParentPath },
            plan.Preview.Effects.Select(effect => effect.Path));
    }

    [Fact(DisplayName = "Route Create retains the native Skills catalogue as a custom-layout parent")]
    public async Task NativeSkillsCatalogueRemainsAParent()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(NativeSkillsCatalogueRemainsAParent));
        Seed(temporary);
        var skillsCatalogue = WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.IndexFileName;
        temporary.ReplaceText(LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Description}](../../{skillsCatalogue}) - #{Tag}"));
        temporary.WriteText(skillsCatalogue, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        var targetId = WorkspaceLayoutNames.SkillsDirectoryName + "/" + Leaf;
        var plan = await Plan(temporary, targetId);
        Assert.Equal(skillsCatalogue, plan.ParentSource.Identity.CanonicalBasePath);
        Assert.Equal(WorkspaceLayoutNames.SkillsDirectoryName, plan.ParentSource.Identity.AutomaticId);
        Assert.Equal(WorkspaceLayout.NativeSkillsPath + "/" + Leaf + SourceFormClassifier.MarkdownExtension,
            plan.TargetSource.Identity.CanonicalBasePath);
        Assert.Empty(plan.DirectoryCreations);
    }

    [Theory(DisplayName = "Route Create refuses changed layout evidence under the lease before content effects")]
    [InlineData(Change.Settings)]
    [InlineData(Change.Loader)]
    [InlineData(Change.DefaultLoader)]
    [InlineData(Change.Host)]
    public async Task ChangedLayoutEvidencePreventsEffects(Change change)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(ChangedLayoutEvidencePreventsEffects));
        using var dataHome = TestDataHome.Create(nameof(ChangedLayoutEvidencePreventsEffects));
        Seed(temporary);
        var plan = await Plan(temporary, TargetId);
        switch (change)
        {
            case Change.Settings: temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot)); break;
            case Change.Loader: temporary.ReplaceText(LoaderPath, ChangedBody); break;
            case Change.DefaultLoader: temporary.WriteText(SourceLogicalPath.LoaderPath, ChangedBody); break;
            case Change.Host: temporary.WriteText(WorkspaceLayoutNames.AgentHostFileName, ChangedBody); break;
            default: throw new ArgumentOutOfRangeException(nameof(change));
        }
        var preserved = new[] { ParentPath, LoaderPath, WorkspaceSettingsDefinitions.RelativePath }
            .ToDictionary(path => path, path => File.ReadAllBytes(temporary.Combine(path)));
        var before = temporary.SnapshotHashes();
        var result = await new RouteCreateApplicationOperation(WorkspaceLockStoreRoot.FromAbsolutePath(dataHome.LockStoreRoot))
            .ExecuteAsync(plan, TestContext.Current.CancellationToken);
        Assert.Equal(RouteCreateFindingCode.TargetChanged, Assert.Single(result.Findings).Code);
        Assert.False(File.Exists(temporary.Combine(TargetPath)));
        Assert.Equal(before, temporary.SnapshotHashes());
        foreach (var (path, bytes) in preserved) Assert.Equal(bytes, File.ReadAllBytes(temporary.Combine(path)));
    }

    [Fact(DisplayName = "Route Create refuses a conflicting loader without falling back to the default root")]
    public async Task ConflictingLoaderPreventsPlanning()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(ConflictingLoaderPreventsPlanning));
        Seed(temporary);
        temporary.WriteText(SourceLogicalPath.LoaderPath, ChangedBody);
        var before = temporary.SnapshotHashes();
        var build = await new RouteCreatePlanBuilder().BuildAsync(Request(temporary, TargetId), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Equal(RouteCreateFindingCode.WorkspaceUnsafe, Assert.Single(build.Formation.Findings).Code);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static async Task<RouteCreatePlan> Plan(TemporaryWorkspace temporary, string target)
        => Assert.IsType<RouteCreatePlan>((await new RouteCreatePlanBuilder()
            .BuildAsync(Request(temporary, target), TestContext.Current.CancellationToken)).Plan);

    private static RouteCreateRequest Request(TemporaryWorkspace temporary, string target)
        => new(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace),
            target, new RouteCreateMetadataInput(Description, [Tag], null), null, RouteCreateMode.Apply);

    private static void Seed(TemporaryWorkspace temporary)
    {
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        temporary.WriteText(LoaderPath, OpenForgeDocumentSeed.GeneratedEntries($"- [{Description}]({Route}/{ParentName}) - #{Tag}"));
        temporary.WriteText(ParentPath, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
    }

    private static string Settings(string root)
        => $$"""{"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}""";
}
