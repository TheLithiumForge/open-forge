using OpenForge.Cli.Core.Commands.Route.Remove;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.TestSupport;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveMainRootIntegrationTests
{
    private const string MainRoot = "manual/context";
    private const string OtherRoot = "manual/other";
    private const string Category = "notes";
    private const string Description = "Removal fixture";
    private const string Tag = "Guide";
    private const string Child = "child";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string CategoryRoot = MainRoot + "/" + Category;
    private const string Entrypoint = CategoryRoot + "/" + SourceFormClassifier.IndexFileName;
    private const string ChildPath = CategoryRoot + "/" + Child + SourceFormClassifier.MarkdownExtension;
    private const string SkillsEntrypoint = WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.IndexFileName;

    [Theory(DisplayName = "Route Remove maps root category exclusions and retains the initial layout observation")]
    [InlineData(Category, CategoryRoot)]
    [InlineData(WorkspaceLayoutNames.SkillsDirectoryName, WorkspaceLayout.NativeSkillsPath)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task CategoryPlansRetainMappedExclusions(string category, string directory)
    {
        using var temporary = Seed();
        var before = SnapshotBytes(temporary);
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            Request(temporary, category), TestContext.Current.CancellationToken);
        Assert.True(build.Plan is not null, string.Join(Environment.NewLine, build.Formation.Findings.Select(finding => finding.Cause)));
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);
        Assert.Equal(MainRoot, plan.LayoutRead.Layout?.MainRoot);
        Assert.Equal(WorkspaceLayoutReadState.Ready, plan.LayoutRead.State);
        Assert.Same(plan.LayoutRead.Settings, plan.Projection.Settings);
        Assert.Same(plan.LayoutRead.Ownership, plan.Projection.Ownership);
        Assert.Equal([category], plan.Projection.RemovalSelection.Categories);
        Assert.Equal([directory], plan.Projection.RemovalSelection.Directories);
        Assert.Equal([directory], plan.Preview.Persistence.Settings.CategoryPaths);
        AssertBytes(before, SnapshotBytes(temporary));
    }

    [Theory(DisplayName = "Route Remove proves absent custom and native categories from mapped persisted exclusions")]
    [InlineData(Category, CategoryRoot)]
    [InlineData(WorkspaceLayoutNames.SkillsDirectoryName, WorkspaceLayout.NativeSkillsPath)]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task AbsentCategoriesUseTheSelectedLayout(string category, string directory)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(AbsentCategoriesUseTheSelectedLayout));
        WriteSettings(temporary, MainRoot, category);
        temporary.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var before = SnapshotBytes(temporary);
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            Request(temporary, category), TestContext.Current.CancellationToken);
        Assert.Empty(build.Formation.Findings);
        Assert.Null(build.Plan);
        Assert.Equal(RouteRemoveVerificationState.Verified, build.Formation.Verification);
        Assert.Equal(RouteRemovePersistenceOutcome.Unchanged, build.Formation.Persistence.Settings.Outcome);
        Assert.Equal([category], build.Formation.Persistence.Settings.Categories);
        Assert.Equal([directory], build.Formation.Persistence.Settings.CategoryPaths);
        AssertBytes(before, SnapshotBytes(temporary));
    }

    [Fact(DisplayName = "Route Remove rejects a changed main root under the lease before target effects")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ChangedMainRootInvalidatesThePlan()
    {
        using var temporary = Seed();
        using var locks = WorkspaceLockTestStore.Create(nameof(ChangedMainRootInvalidatesThePlan));
        var request = Request(temporary, Category);
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        var plan = Assert.IsType<RouteRemovePlan>(build.Plan);
        var acquired = await locks.AcquireAsync(new WorkspaceLockRequest(request.Workspace,
            RouteRemoveDefinitions.CommandIdentity, Guid.NewGuid()), TestContext.Current.CancellationToken);
        await using var lease = Assert.IsType<WorkspaceLockLease>(acquired.Lease);
        WriteSettings(temporary, OtherRoot);
        var before = SnapshotBytes(temporary);
        var result = await RouteRemoveOperationFactory.CreatePlanRevalidator().RevalidateAsync(
            plan, lease, TestContext.Current.CancellationToken);
        Assert.Equal(RouteRemovePlanRevalidationState.Changed, result.State);
        AssertBytes(before, SnapshotBytes(temporary));
        Assert.True(File.Exists(temporary.Combine(ChildPath)));
    }

    [Fact(DisplayName = "Route Remove refuses conflicting layout evidence without planning effects")]
    [Trait("Feature", "route-remove"), Trait("Evidence", "Integration")]
    public async Task ConflictingLoaderIsWriteFree()
    {
        using var temporary = Seed();
        temporary.WriteText(SourceLogicalPath.LoaderPath, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var before = SnapshotBytes(temporary);
        var build = await RouteRemoveOperationFactory.CreatePlanBuilder().BuildAsync(
            Request(temporary, Category), TestContext.Current.CancellationToken);
        Assert.Null(build.Plan);
        Assert.Contains(build.Formation.Findings, finding => finding.Code == RouteRemoveFindingCode.WorkspaceUnsafe);
        Assert.Empty(build.Formation.Effects);
        AssertBytes(before, SnapshotBytes(temporary));
    }

    private static IReadOnlyDictionary<string, byte[]> SnapshotBytes(TemporaryWorkspace temporary)
        => Directory.EnumerateFiles(temporary.Path, "*", SearchOption.AllDirectories)
            .Where(path => !Path.GetRelativePath(temporary.Path, path).StartsWith(
                TemporaryWorkspace.RuntimeDirectoryName + Path.DirectorySeparatorChar, StringComparison.Ordinal))
            .ToDictionary(path => Path.GetRelativePath(temporary.Path, path), File.ReadAllBytes, StringComparer.Ordinal);

    private static void AssertBytes(IReadOnlyDictionary<string, byte[]> expected, IReadOnlyDictionary<string, byte[]> actual)
    {
        Assert.Equal(expected.Keys.Order(StringComparer.Ordinal), actual.Keys.Order(StringComparer.Ordinal));
        foreach (var (path, bytes) in expected)
            Assert.Equal(bytes, actual[path]);
    }

    private static TemporaryWorkspace Seed()
    {
        var temporary = TemporaryWorkspace.Create(nameof(RouteRemoveMainRootIntegrationTests));
        WriteSettings(temporary, MainRoot);
        temporary.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(
            $"- [{Category}]({Category}/{SourceFormClassifier.IndexFileName}) - #{Tag}\n" +
            $"- [{WorkspaceLayoutNames.SkillsDirectoryName}](../../{SkillsEntrypoint}) - #{Tag}"));
        temporary.WriteText(Entrypoint, OpenForgeDocumentSeed.Metadata(Description, [Tag],
            OpenForgeDocumentSeed.GeneratedEntries($"- [{Child}]({Child}{SourceFormClassifier.MarkdownExtension}) - #{Tag}")));
        temporary.WriteText(ChildPath, OpenForgeDocumentSeed.Metadata(Description, [Tag], Description));
        temporary.WriteText(SkillsEntrypoint, OpenForgeDocumentSeed.Metadata(Description, [Tag], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        return temporary;
    }

    private static RouteRemoveRequest Request(TemporaryWorkspace temporary, string source)
        => new(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace), source, RouteRemoveMode.Apply);

    private static void WriteSettings(TemporaryWorkspace temporary, string mainRoot, string? removedCategory = null)
    {
        var removals = removedCategory is null ? string.Empty : $",\"{WorkspaceSettingsDefinitions.RemovedCategoriesProperty}\":[\"{removedCategory}\"]";
        var text = $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}"{{removals}}}
            """;
        if (File.Exists(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath)))
            temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, text);
        else
            temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, text);
    }
}
