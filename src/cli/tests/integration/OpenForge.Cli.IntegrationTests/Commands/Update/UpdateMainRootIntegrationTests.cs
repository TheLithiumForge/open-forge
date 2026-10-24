using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Interaction;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Update;

[Trait("Feature", "update"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class UpdateMainRootIntegrationTests
{
    private const string MainRoot = "handbook/context";
    private const string OtherRoot = "handbook/other";
    private const string Loader = MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    private const string Guide = MainRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;
    private const string DefaultGuide = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;
    private const string OwnerId = "fixture-framework";
    private const string RetiredName = "retired" + SourceFormClassifier.MarkdownExtension;
    private const string RetiredPath = MainRoot + "/" + RetiredName;
    private const string ChangedBody = "# Changed after review\n";

    public enum LayoutChange { Settings, Loader, DefaultLoader, Host }

    [Fact(DisplayName = "Update compares installed custom paths while preserving package source identities")]
    public async Task PlanSeparatesInstalledAndPackageCoordinates()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(PlanSeparatesInstalledAndPackageCoordinates));
        Seed(temporary);
        var before = temporary.SnapshotHashes();
        var execution = await Plan(temporary);
        Assert.Equal(MainRoot, execution.LayoutRead.Layout?.MainRoot);
        var loader = Assert.Single(execution.Observations, value => value.Comparison.RelativePath == Loader);
        Assert.Equal(FrameworkPayloadAsset.LoaderPath, loader.Comparison.SourceAssetPath);
        var guide = Assert.Single(execution.Observations, value => value.Comparison.RelativePath == Guide);
        Assert.Equal(DefaultGuide, guide.Comparison.SourceAssetPath);
        var nativeCatalogue = WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension;
        Assert.Contains(execution.Observations, value => value.Comparison.RelativePath == nativeCatalogue && value.Comparison.SourceAssetPath == nativeCatalogue);
        Assert.DoesNotContain(execution.Effects, value => value.ResultEffect.Path == SourceLogicalPath.LoaderPath);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Update filters package content by its installed removal path")]
    public async Task MappedRemovalKeepsTheCustomGuideAbsent()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(MappedRemovalKeepsTheCustomGuideAbsent));
        Seed(temporary);
        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}",
             "{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}":["{{Guide}}"]}
            """);
        var before = temporary.SnapshotHashes();
        var execution = await Plan(temporary);
        Assert.DoesNotContain(execution.Observations, value => value.Comparison.RelativePath == Guide);
        Assert.DoesNotContain(execution.Effects, value => value.ResultEffect.Path == Guide);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Update keeps explicitly admitted retired paths literal when excluding mapped package content")]
    public async Task RetiredLiteralPathIsIndependentOfMappedRemoval()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(RetiredLiteralPathIsIndependentOfMappedRemoval));
        Seed(temporary, DefaultGuide);
        temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{MainRoot}}",
             "{{WorkspaceSettingsDefinitions.AllowInstallPathsProperty}}":["{{DefaultGuide}}"],
             "{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}":["{{Guide}}"]}
            """);
        var before = temporary.SnapshotHashes();
        var resolution = await UpdatePlanBuilder.Create().BuildExecutionAsync(Request(temporary, prune: true), TestContext.Current.CancellationToken);
        var execution = Assert.IsType<UpdatePlanExecution>(resolution.Execution);
        Assert.Contains(execution.Build.Plan!.Decisions, value => value.Comparison.RelativePath == DefaultGuide
            && value.Disposition == UpdatePlanningDisposition.Delete);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Update detects another owner at the mapped installed destination")]
    public async Task MappedOwnershipCollisionStopsThePlan()
    {
        using var temporary = TemporaryWorkspace.Create(nameof(MappedOwnershipCollisionStopsThePlan));
        Seed(temporary);
        var framework = new FrameworkOwnership(new OwnedSource(OwnerId, null), [Loader], []);
        temporary.ReplaceBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with
        {
            Framework = framework,
            Extensions = [new ExtensionOwnership(OwnerId, null, null, [], [Guide], [])],
        }));
        var before = temporary.SnapshotHashes();
        var resolution = await UpdatePlanBuilder.Create().BuildExecutionAsync(Request(temporary), TestContext.Current.CancellationToken);
        Assert.Null(resolution.Execution);
        var finding = Assert.Single(resolution.Build.Preview.Findings);
        Assert.Equal(UpdateFindingCode.OwnershipConflict, finding.Code);
        Assert.Equal(Guide, finding.Target);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Update retirement authority matches content roots ordinally")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CaseAliasNeedsExplicitAdmission(bool alias)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(CaseAliasNeedsExplicitAdmission));
        var retired = alias ? MainRoot.ToUpperInvariant() + "/" + RetiredName : RetiredPath;
        Seed(temporary, retired);
        var before = temporary.SnapshotHashes();
        var resolution = await UpdatePlanBuilder.Create().BuildExecutionAsync(Request(temporary, prune: true), TestContext.Current.CancellationToken);
        if (alias)
        {
            Assert.Null(resolution.Execution);
            Assert.Equal(UpdateFindingCode.OwnershipObservation, Assert.Single(resolution.Build.Preview.Findings).Code);
        }
        else
        {
            var execution = Assert.IsType<UpdatePlanExecution>(resolution.Execution);
            Assert.Contains(execution.Build.Plan!.Decisions, value => value.Comparison.RelativePath == RetiredPath
                && value.Disposition == UpdatePlanningDisposition.Delete);
        }
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Update rejects changed layout evidence before effects")]
    [InlineData(LayoutChange.Settings)]
    [InlineData(LayoutChange.Loader)]
    [InlineData(LayoutChange.DefaultLoader)]
    [InlineData(LayoutChange.Host)]
    public async Task ChangedLayoutEvidenceStopsApplication(LayoutChange change)
    {
        using var temporary = TemporaryWorkspace.Create(nameof(ChangedLayoutEvidenceStopsApplication));
        Seed(temporary);
        IReadOnlyDictionary<string, string>? afterEdit = null;
        var result = await UpdateOperationFactory.Create(UpdateInteractionTestSupport.Confirmation(observe: (_, _) =>
        {
            switch (change)
            {
                case LayoutChange.Settings: temporary.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings(OtherRoot)); break;
                case LayoutChange.Loader: temporary.ReplaceText(Loader, ChangedBody); break;
                case LayoutChange.DefaultLoader: temporary.WriteText(SourceLogicalPath.LoaderPath, ChangedBody); break;
                case LayoutChange.Host: temporary.WriteText(WorkspaceLayoutNames.AgentHostFileName, ChangedBody); break;
                default: throw new ArgumentOutOfRangeException(nameof(change));
            }
            afterEdit = temporary.SnapshotHashes();
        })).ExecuteAsync(Request(temporary, prompt: true), TestContext.Current.CancellationToken);
        Assert.NotNull(afterEdit);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == UpdateFindingCode.TargetUnsafe);
        Assert.Equal(afterEdit, temporary.SnapshotHashes());
        Assert.False(File.Exists(temporary.Combine(Guide)));
    }

    private static async Task<UpdatePlanExecution> Plan(TemporaryWorkspace temporary)
    {
        var resolution = await UpdatePlanBuilder.Create().BuildExecutionAsync(Request(temporary), TestContext.Current.CancellationToken);
        Assert.True(resolution.Execution is not null, string.Join(Environment.NewLine, resolution.Build.Preview.Findings.Select(finding => finding.Cause)));
        return Assert.IsType<UpdatePlanExecution>(resolution.Execution);
    }

    private static UpdateRequest Request(TemporaryWorkspace temporary, bool prune = false, bool prompt = false)
        => new(new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.CurrentDirectory), UpdateMode.Apply,
            force: false, prune: prune, automatic: !prompt, allowsInteractiveConfirmation: prompt);

    private static void Seed(TemporaryWorkspace temporary, string? retired = null)
    {
        temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        temporary.WriteText(Loader, OpenForgeDocumentSeed.GeneratedEntries(string.Empty));
        if (retired is not null) temporary.WriteText(retired, OpenForgeDocumentSeed.Metadata(OwnerId, [], ChangedBody));
        var framework = new FrameworkOwnership(new OwnedSource(OwnerId, null), retired is null ? [Loader] : [Loader, retired], []);
        temporary.WriteBytes(WorkspaceOwnershipDefinitions.RelativePath, WorkspaceOwnershipCodec.Write(WorkspaceOwnershipDocument.Empty with { Framework = framework }));
    }

    private static string Settings(string mainRoot)
        => $$"""
            {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},
             "{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{mainRoot}}"}
            """;
}
