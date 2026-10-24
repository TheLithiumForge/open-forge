using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Repair;

[Trait("Feature", "repair"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class RepairLayoutIntegrationTests
{
    private const string MainRoot = "knowledge/nested";
    private const string Category = "notes";
    private const string Entrypoint = MainRoot + "/" + Category + "/" + SourceFormClassifier.IndexFileName;
    private const string Source = MainRoot + "/" + Category + "/source.md";
    private const string Target = "guide.md";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Repair retains Doctor's layout through catalogue and maintenance planning")]
    public async Task PlanningRetainsTheOriginalObservation()
    {
        using var files = Create();
        var workspace = Workspace(files);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var diagnosis = await components.DiagnosisReader.ReadAsync(new(workspace), Token);
        var observation = Assert.IsType<WorkspaceLayoutRead>(diagnosis.Observation.LayoutRead);
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings("knowledge/replacement"));

        var catalogue = await components.CatalogueReader.ReadAsync(observation, diagnosis.Observation.LocalReferences, Token);
        var plan = RepairPlanner.Build(Request(workspace), catalogue.Proposals);
        plan = await Assert.IsType<RepairMaintenancePlanner>(components.MaintenancePlanner).CompleteAsync(plan, catalogue, Token);

        Assert.Same(observation, catalogue.LayoutRead);
        Assert.Same(observation, plan.LayoutRead);
        Assert.Same(observation.Layout, Assert.IsType<OpenForge.Cli.Core.Framework.Sources.Models.Inventory.SourceCatalogue>(catalogue.Catalogue).Layout);
        Assert.False(plan.IsBlocked);
        Assert.NotEmpty(plan.FileChanges);
    }

    [Fact(DisplayName = "Repair rejects a changed layout before target effects or recovery preparation")]
    public async Task ChangedLayoutPreservesAuthoredBytes()
    {
        using var files = Create();
        var workspace = Workspace(files);
        var components = RepairOperationFactory.CreateDefaultComponents();
        var diagnosis = await components.DiagnosisReader.ReadAsync(new(workspace), Token);
        var observation = Assert.IsType<WorkspaceLayoutRead>(diagnosis.Observation.LayoutRead);
        var catalogue = await components.CatalogueReader.ReadAsync(observation, diagnosis.Observation.LocalReferences, Token);
        var plan = await Assert.IsType<RepairMaintenancePlanner>(components.MaintenancePlanner).CompleteAsync(
            RepairPlanner.Build(Request(workspace), catalogue.Proposals), catalogue, Token);
        var preflight = await components.Application.PreflightAsync(plan, Token);
        Assert.Equal(RepairPreflightState.Ready, preflight.Outcome.Preflight.State);
        var bytes = File.ReadAllBytes(files.Combine(Source));
        files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, Settings("knowledge/replacement"));

        var outcome = await components.Application.ExecuteAsync(plan, preflight.Targets, Token);

        Assert.Equal(RepairPreflightState.Blocked, outcome.Preflight.State);
        Assert.Equal(bytes, File.ReadAllBytes(files.Combine(Source)));
        Assert.Contains(outcome.Findings, finding => finding.Code == RepairFindingCode.TargetChanged);
        Assert.False(Directory.Exists(files.Combine(MainRoot, WorkspaceLayoutNames.SkillsDirectoryName)));
    }

    private static TemporaryWorkspace Create()
    {
        var files = TemporaryWorkspace.Create("repair-layout");
        files.WriteText(SourceLogicalPath.WorkspaceEntryPath, $"# Workspace\n\nRead `{MainRoot}/{WorkspaceLayoutNames.LoaderFileName}`.\n");
        files.WriteText(WorkspaceSettingsDefinitions.RelativePath, Settings(MainRoot));
        files.WriteText($"{MainRoot}/{WorkspaceLayoutNames.LoaderFileName}", OpenForgeDocumentSeed.GeneratedEntries(
            $"- [Notes]({Category}/{Path.GetFileName(Entrypoint)}) - #Docs"));
        files.WriteText(Entrypoint, OpenForgeDocumentSeed.Metadata("Notes", ["Docs"], OpenForgeDocumentSeed.GeneratedEntries(string.Empty)));
        files.WriteText(Source, OpenForgeDocumentSeed.Metadata("Source", ["Docs"], $"# Source\n\n[Guide](./{Target})\n"));
        files.WriteText($"{MainRoot}/{Category}/{Target}", OpenForgeDocumentSeed.Metadata("Guide", ["Docs"], "# Guide\n"));
        return files;
    }

    private static CliWorkspace Workspace(TemporaryWorkspace files) => new(files.Path, files.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    private static RepairRequest Request(CliWorkspace workspace) => new(workspace, RepairMode.Apply, automatic: true, [], allowInteraction: false);
    private static string Settings(string root) => $$"""
        {"{{WorkspaceSettingsDefinitions.SchemaVersionProperty}}":{{WorkspaceSettingsDefinitions.SchemaVersion}},"{{WorkspaceSettingsDefinitions.MainRootProperty}}":"{{root}}"}
        """;
}
