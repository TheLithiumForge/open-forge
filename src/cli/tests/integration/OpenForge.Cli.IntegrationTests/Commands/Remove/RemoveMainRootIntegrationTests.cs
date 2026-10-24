using OpenForge.Cli.Core.Commands.Remove;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Remove.Models.Selection;
using OpenForge.Cli.Core.Commands.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Remove.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveMainRootIntegrationTests
{
    private const string Parent = "handbook";
    private const string MainRoot = Parent + "/context";
    private const string OtherRoot = "other/context";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Category = "notes";
    private const string Note = Category + "/note" + SourceFormClassifier.MarkdownExtension;
    private const string NotePath = MainRoot + "/" + Note;
    private const string Entrypoint = MainRoot + "/" + Category + "/" + SourceFormClassifier.IndexFileName;
    private const string SkillName = "workshop";
    private const string SkillDirectory = WorkspaceLayout.NativeSkillsPath + "/" + SkillName;
    private const string SkillPath = SkillDirectory + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string Target = MainRoot + "/sample.bin";
    private const string Description = "Workshop instructions";
    private const string Owner = "remove-fixture";
    private const string FixtureTag = "Docs";
    private static readonly byte[] OriginalBytes = [0, 255, 13, 10, 128];
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Remove plans native Skill deletion with custom loader navigation and fixed settings")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task NativeSkillRemovalCarriesLayoutAndFixedSettings()
    {
        using var workspace = Create();
        workspace.WriteText(SkillPath, OpenForgeDocumentSeed.Skill(SkillName, Description, string.Empty));
        File.WriteAllText(workspace.Combine(Loader), OpenForgeDocumentSeed.GeneratedEntries($"- [{SkillName}](../../{SkillPath}) - #{FixtureTag}"));
        workspace.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with { Framework = new(new(Owner, null), [Loader, SkillPath, Target], []) }));
        var planner = new RemovePathPlanner(new PhysicalPathResolver(), new WorkspaceOwnershipStore());
        var request = new RemovePathRequest(workspace.Workspace, SkillDirectory, RemoveMode.Apply, automatic: true);
        var plan = Assert.IsType<RemovePathPlanningOutcome.Planned>(await planner.PlanAsync(request, Token)).Plan;

        Assert.Equal(MainRoot, plan.LayoutRead.Layout?.MainRoot);
        Assert.Equal(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath), plan.SettingsChange?.Change.LogicalPath);
        Assert.Equal(workspace.Combine(Loader), Assert.Single(plan.Navigation.Changes).LogicalPath);
        Assert.Null(plan.AgentsCreation);
        var preserved = File.ReadAllBytes(workspace.Combine(Target));
        var result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot).ExecuteAsync(request, Token);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.False(Directory.Exists(workspace.Combine(SkillDirectory)));
        Assert.Equal(preserved, File.ReadAllBytes(workspace.Combine(Target)));
        var settings = WorkspaceSettingsCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))).Document;
        Assert.NotNull(settings);
        Assert.Equal(MainRoot, settings.MainRoot);
        Assert.Contains(SkillDirectory, settings.RemovedDirectories);
        var ownership = WorkspaceOwnershipCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath))).Document;
        Assert.NotNull(ownership);
        Assert.NotNull(ownership.Framework);
        Assert.Equal([Loader, Target], ownership.Framework.Paths);
        Assert.False(File.Exists(workspace.Combine(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName)));
    }

    [Fact(DisplayName = "Remove classifies selected content without treating the former root as content")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task SelectionUsesTheObservedContentLocations()
    {
        using var workspace = Create();
        workspace.WriteText(NotePath, OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], string.Empty));
        workspace.WriteText(Entrypoint, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var oldPath = WorkspaceLayout.DefaultMainRoot + "/" + Note;
        workspace.WriteText(oldPath, OpenForgeDocumentSeed.Metadata(Description, [FixtureTag], string.Empty));
        workspace.WriteText(SkillPath, OpenForgeDocumentSeed.Skill(SkillName, Description, string.Empty));

        Assert.Equal(RemoveKind.Route, (await Resolve(NotePath)).Kind);
        Assert.True((await Resolve(Entrypoint)).IsEntrypointFile);
        Assert.Equal(RemoveKind.Path, (await Resolve(oldPath)).Kind);
        Assert.Equal(RemoveKind.Path, (await Resolve(MainRoot.ToUpperInvariant() + "/" + Note)).Kind);
        Assert.Equal(RemoveKind.Path, (await Resolve(SkillDirectory)).Kind);
        workspace.WriteText(WorkspaceLayout.NativeSkillsPath + "/" + SourceFormClassifier.IndexFileName,
            OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        Assert.Equal(RemoveKind.Route, (await Resolve(WorkspaceLayout.NativeSkillsPath)).Kind);

        ValueTask<RemoveTargetResolution> Resolve(string path)
            => RemoveTargetResolver.ResolveAsync(workspace.Workspace, RemoveSelection.Selected(RemoveKind.Path, path), Token);
    }

    [Theory(DisplayName = "Remove revalidates the selected root and loader before recovery or deletion")]
    [InlineData(true)]
    [InlineData(false)]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ChangedLayoutObservationBlocksEffects(bool changeSettings)
    {
        using var workspace = Create();
        var operation = RemovePathOperationFactory.Create(workspace.LockStoreRoot, (_, _, _, _) =>
        {
            if (changeSettings) File.WriteAllText(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath), SettingsText(OtherRoot));
            else File.WriteAllText(workspace.Combine(Loader), OpenForgeDocumentSeed.GeneratedEntries(string.Empty) + "\n");
            return ValueTask.FromResult(CliPromptReply<bool>.Answered(true));
        });
        var result = await operation.ExecuteAsync(new RemovePathRequest(workspace.Workspace, Target, RemoveMode.Apply,
            automatic: false, allowInteractiveConfirmation: true), Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.TargetChanged);
        Assert.Equal(OriginalBytes, File.ReadAllBytes(workspace.Combine(Target)));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));
    }

    [Theory(DisplayName = "Remove protects the custom root, its ancestors and fixed controls")]
    [InlineData(MainRoot)]
    [InlineData(Parent)]
    [InlineData(WorkspaceSettingsDefinitions.RelativePath)]
    [InlineData(WorkspaceOwnershipDefinitions.RelativePath)]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ProtectedBoundariesRemainUnchanged(string path)
    {
        using var workspace = Create();
        var before = workspace.SnapshotHashes();
        var result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(new RemovePathRequest(workspace.Workspace, path, RemoveMode.Apply, automatic: true), Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.ProtectedPath);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Remove refuses a conflicting default loader without deleting custom content")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ConflictingLoaderBlocksRemoval()
    {
        using var workspace = Create();
        workspace.WriteText(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
            OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        var before = workspace.SnapshotHashes();
        var result = await RemovePathOperationFactory.Create(workspace.LockStoreRoot)
            .ExecuteAsync(new RemovePathRequest(workspace.Workspace, Target, RemoveMode.Apply, automatic: true), Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == RemoveFindingCode.TargetUnsafe);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static RemoveRootIntegrationWorkspace Create()
    {
        var workspace = RemoveRootIntegrationWorkspace.Create(nameof(RemoveMainRootIntegrationTests));
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, SettingsText(MainRoot));
        workspace.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        workspace.WriteBytes(Target, OriginalBytes);
        return workspace;
    }

    private static string SettingsText(string root)
        => $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
            """;
}
