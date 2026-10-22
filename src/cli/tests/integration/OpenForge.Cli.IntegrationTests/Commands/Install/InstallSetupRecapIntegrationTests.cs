using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models.Structure;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Fixtures;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

public sealed class InstallSetupRecapIntegrationTests
{
    [Theory(DisplayName = "Install review and receipt wrap prose at word boundaries at 80 columns"), InlineData(true), InlineData(false)]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task ProseFitsEightyColumns(bool preview)
    {
        using var workspace = InstallOperationWorkspace.Create("install-eighty-columns-long-workspace-name");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        var request = workspace.Request(preview ? InstallMode.DryRun : InstallMode.Apply) with { Setup = new(false, InstallPreset.FullCore, []) };
        var result = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var rendering = preview ? InstallPresentation.PlanReviewRendering : InstallPresentation.Rendering;
        var selected = CliReportSelection.Select(result, new(CliDetail.Minimal), rendering);
        var text = CliTextRenderer.Render(selected, CliTextStyle.Plain.WithWidth(80), rendering.DataTextRenderer).Content;
        Assert.All(text.Split('\n').Where(line => !line.StartsWith("Workspace:", StringComparison.Ordinal)), line => Assert.True(line.Length <= 80, line));
        Assert.Contains(workspace.PhysicalPath, text, StringComparison.Ordinal);
        Assert.DoesNotContain(workspace.PhysicalPath, selected.TextHeadline.Sentence, StringComparison.Ordinal);
        var renderers = new CommandOutputRenderers<InstallResult>(request => CliRenderingStage.Render(
            request with { Presentation = request.Presentation with { Width = 80 } }, InstallPresentation.Rendering));
        renderers.MatchDetails(result, "eighty-columns",
            testName: nameof(ProseFitsEightyColumns) + (preview ? ".preview" : ".receipt"));
    }

    [Fact(DisplayName = "An occupied Install after the Essentials wizard retains its resolved setup and leaves the files unchanged")]
    [Trait("Feature", "install-command"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
    public async Task ResolvedEssentialsWizardRetryPreservesSelection()
    {
        using var workspace = InstallOperationWorkspace.Create("install-wizard-retry");
        var occupiedHost = InstallMarkdownFixture.OccupiedManagedHost;
        Assert.NotEqual(InstallMarkdownFixture.ReadEmbeddedText("AGENTS.md"), occupiedHost);
        Assert.Equal(MarkdownManagedHostState.Present, new MarkdownDocumentParser().Parse(occupiedHost).ManagedHost.State);
        workspace.WriteText("AGENTS.md", occupiedHost);
        var before = workspace.SnapshotHashes();
        var prompts = new InstallSetupInteraction(
            (_, _, _) => ValueTask.FromResult(CliPromptReply<InstallPreset>.Answered(InstallPreset.Essentials)),
            (_, _, _) => throw new InvalidOperationException("Essentials does not ask for Custom routes."),
            (_, _, _) => ValueTask.FromResult(CliPromptReply<string>.Answered("root")));
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot, prompts);
        var result = await operation.ExecuteAsync(workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == InstallFindingCode.TargetOccupied);
        Assert.Equal(InstallPreset.Essentials, result.Input.Configuration?.Preset);
        Assert.Equal(FrontmatterForm.Root, result.Input.ResolvedFrontmatter);
        Assert.Equal($"open-forge install --workspace {CliCommandText.Quote(workspace.PhysicalPath)} --preset essentials --frontmatter root --force --dry-run", result.Next?.Command);
        Assert.Equal(result.Next?.Command, InstallReportSelector.Select(result, new(CliDetail.Minimal)).Next?.Command);
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Empty(result.Facts.Effects);
        Assert.Equal(occupiedHost, File.ReadAllText(workspace.Combine("AGENTS.md")));
    }

    private static readonly CommandOutputRenderers<InstallResult> Renderers = CommandOutputRenderers<InstallResult>.From(InstallPresentation.Rendering);

    [Fact(DisplayName = "First installation recaps its resolved setup and supplies text-only first-task guidance")]
    [Trait("Feature", "install-presentation"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task FirstInstallationRecapsResolvedSelection()
    {
        var captures = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var essentials in new[] { true, false })
        {
            using var workspace = InstallOperationWorkspace.Create("install-setup-recap");
            var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
            var request = workspace.Request() with { Setup = essentials ? new(false, InstallPreset.Essentials, []) : null };
            var before = workspace.SnapshotHashes();
            var installed = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, installed.Status);
            var selected = CliReportSelection.Select(installed, new(CliDetail.Minimal), InstallPresentation.Rendering);
            Assert.True(selected.Report.Data.ShowFirstTask);
            Assert.Equal(4, selected.Report.Data.TextSetupLines.Count);
            Assert.Equal(essentials ? InstallPreset.Essentials : InstallPreset.FullCore, installed.Input.SetupRecap?.Preset);
            var text = CliTextRenderer.Render(selected, CliTextStyle.Plain, InstallPresentation.Rendering.DataTextRenderer).Content;
            Assert.Contains("Next: review", text, StringComparison.Ordinal);
            Renderers.MatchDetails(installed, essentials ? "essentials-receipt" : "default-full-core-receipt", snapshotCollector: captures);
            using var json = System.Text.Json.JsonDocument.Parse(CliJsonRenderer.Render(selected, InstallPresentation.Rendering.DataJsonTypeInfo));
            Assert.Equal(System.Text.Json.JsonValueKind.Null, json.RootElement.GetProperty("next").ValueKind);
            Assert.False(json.RootElement.GetProperty("data").TryGetProperty("textSetupLines", out _));
            Assert.NotEqual(before, workspace.SnapshotHashes());
            var noOp = await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken);
            var noOpData = InstallReportSelector.Select(noOp, new(CliDetail.Minimal)).Data;
            Assert.True(noOpData.IsNoOp);
            Assert.False(noOpData.ShowFirstTask);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(captures);
    }

    [Fact(DisplayName = "Configure recap shows only changed selections and preserves files when leaving a route out")]
    [Trait("Feature", "install-configuration"), Trait("Evidence", "Integration"), Trait("Boundary", "Output")]
    public async Task ConfigureRecapsOnlyChangedRoutes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-configure-recap");
        var operation = InstallOperationFactory.Create(InstallInteractionTestSupport.Confirmation(), workspace.LockStoreRoot);
        Assert.Equal(CliSemanticStatus.Complete, (await operation.ExecuteAsync(workspace.Request(), TestContext.Current.CancellationToken)).Status);
        var before = workspace.SnapshotHashes();
        var request = workspace.Request(InstallMode.DryRun) with
        { Setup = new(true, InstallPreset.Custom, [new("guidance", InstallRouteAction.Remove)]) };
        var preview = await operation.ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(before, workspace.SnapshotHashes());
        var plan = InstallReportSelector.Select(preview, new(CliDetail.Minimal)).Data;
        Assert.False(plan.ShowFrontmatter);
        Assert.False(plan.ShowFirstTask);
        Assert.Equal(2, plan.TextSetupLines.Count);
        var selection = Assert.IsType<InstallSetupRecap>(preview.Input.SetupRecap);
        Assert.Equal(InstallRouteAction.Remove, selection.Routes.Single(row => row.Id == "guidance").Action);
        Assert.Equal(InstallRouteAction.Add, selection.PreviousRoutes.Single(row => row.Id == "guidance").Action);
        Renderers.MatchDetails(preview, "configure-leave-out");
        var receipt = await operation.ExecuteAsync(workspace.Request() with { Setup = request.Setup }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, receipt.Status);
        var applied = InstallReportSelector.Select(receipt, new(CliDetail.Minimal)).Data;
        Assert.Equal(plan.TextSetupLines, applied.TextSetupLines);
        Assert.False(applied.ShowFirstTask);
        Assert.True(File.Exists(Path.Combine(workspace.PhysicalPath, ".agents", "guidance", "_guidance.md")));
    }
}
