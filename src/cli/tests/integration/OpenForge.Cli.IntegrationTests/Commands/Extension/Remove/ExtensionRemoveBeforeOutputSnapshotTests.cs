using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Effects;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionRemoveBeforeOutputSnapshotTests
{
    [Theory(DisplayName = "Root and Extension removal previews offer the selected apply request and dependency advice once")]
    [InlineData(false, "text"), InlineData(false, "json"), InlineData(true, "text"), InlineData(true, "json")]
    [Trait("Feature", "extension-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task PreviewNextAppliesSelectedRemoval(bool root, string format)
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("remove retry's workspace");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("remove-retry-source");
        source.AddPackage("toolkit", ["base"], (".agents/toolkit.md", "# Toolkit\n"));
        source.AddPackage("base", [], (".agents/base.md", "# Base\n"));
        Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic"])).ExitCode);
        var before = workspace.Snapshot();
        string[] selection = root ? ["remove", "--kind", "extension", "toolkit"] : ["extension", "remove", "toolkit"];
        string[] apply = [.. selection, "--workspace", workspace.Path,
            "--allow-path", "docs/probe.md", "--automatic", "--format", format, "--detail", "standard", "--detail-filter", "warning"];
        var preview = await workspace.RunAsync([.. apply, "--dry-run"]);
        Assert.Equal(2, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        var command = root ? "remove --kind extension toolkit" : "extension remove toolkit";
        var expected = $"open-forge {command} --workspace {CliCommandText.Quote(workspace.Path)} --allow-path docs/probe.md --automatic --format {format} --detail standard --detail-filter warning";
        string next;
        if (format == "json")
        {
            using var json = JsonDocument.Parse(preview.StandardOutput);
            next = Assert.IsType<string>(json.RootElement.GetProperty("next").GetProperty("command").GetString());
            var finding = Assert.Single(json.RootElement.GetProperty("findings").EnumerateArray());
            Assert.Equal("sentence", Assert.Single(finding.GetProperty("actions").EnumerateArray()).GetProperty("kind").GetString());
        }
        else
        {
            next = Assert.Single(preview.StandardOutput.Split('\n'), line => line.StartsWith("Next: ", StringComparison.Ordinal))[6..].TrimEnd('\r');
            const string advice = "After applying this removal, you can preview removing base too.";
            Assert.Equal(1, preview.StandardOutput.Split(advice, StringSplitOptions.None).Length - 1);
        }
        Assert.Equal(expected, next);
        Assert.Equal(before, workspace.Snapshot());
        var applied = await workspace.RunAsync(apply);
        Assert.Equal(2, applied.ExitCode);
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/base.md")));
        if (format == "json")
        {
            using var receipt = JsonDocument.Parse(applied.StandardOutput);
            Assert.StartsWith("open-forge extension remove base", receipt.RootElement.GetProperty("next").GetProperty("command").GetString(), StringComparison.Ordinal);
        }
    }

    private static readonly CommandOutputRenderers<ExtensionRemoveResult> Renderers = CommandOutputRenderers<ExtensionRemoveResult>.From(ExtensionRemovePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension remove output preserves ownership release, dependencies and selected package effects")]
    [InlineData("single-package", (int)CliSemanticStatus.Complete)]
    [InlineData("shared-file-kept", (int)CliSemanticStatus.Complete)]
    [InlineData("missing-file-released", (int)CliSemanticStatus.Complete)]
    [InlineData("orphaned-dependency", (int)CliSemanticStatus.Attention)]
    [InlineData("dependent-blocks", (int)CliSemanticStatus.Blocked)]
    [InlineData("permission-required", (int)CliSemanticStatus.Blocked)]
    [InlineData("select-prompt", (int)CliSemanticStatus.Complete)]
    [InlineData("no-selection-non-interactive", (int)CliSemanticStatus.Invalid)]
    [InlineData("not-installed", (int)CliSemanticStatus.Complete)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    [InlineData("write-failed-partial", (int)CliSemanticStatus.Failed)]
    public async Task PackageRemoval(string situation, int status)
    {
        if (situation == "write-failed-partial" && !OperatingSystem.IsWindows())
            Assert.Skip("This deterministic deletion failure requires Windows file sharing.");
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-remove-output");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-remove-output-source");
        var dependency = situation is "orphaned-dependency" or "dependent-blocks";
        source.AddPackage("toolkit", dependency ? ["base"] : [], (".agents/toolkit.md", "# Toolkit\n"));
        if (situation == "write-failed-partial") source.AddPayload("toolkit", ".agents/aaa.md", "# Earlier\n");
        if (dependency || situation is "shared-file-kept" or "select-prompt")
        {
            var target = situation == "shared-file-kept" ? ".agents/toolkit.md" : ".agents/base.md";
            var content = situation == "shared-file-kept" ? "# Toolkit\n" : "# Base\n";
            source.AddPackage("base", [], (target, content));
        }

        if (situation != "not-installed")
        {
            var installed = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic"]);
            Assert.Equal(0, installed.ExitCode);
        }
        if (situation == "missing-file-released") File.Delete(workspace.Combine(".agents/toolkit.md"));
        if (situation == "permission-required")
        {
            var ownership = JsonNode.Parse(workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath))!;
            ownership["extensions"]![0]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create("workspace-note.md"));
            workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
        }
        var prompted = situation == "select-prompt";
        var scripted = ScriptedCliTerminal.Lines(
            prompted ? ["2", "yes"] : [],
            canPrompt: prompted);
        var prompts = new CliPrompts(scripted.Terminal);
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        string[] ids = situation switch
        {
            "no-selection-non-interactive" or "select-prompt" => [],
            "dependent-blocks" => ["base"],
            _ => ["toolkit"],
        };
        ExtensionRemoveResult result;
        using (var held = situation == "lock-held" ? workspace.HoldLock() : null)
        using (var denied = situation == "write-failed-partial"
                   ? File.Open(workspace.Combine(".agents/toolkit.md"), FileMode.Open, FileAccess.Read, FileShare.Read) : null)
        {
            result = await ExtensionRemoveOperationFactory.Create(
                    ExtensionInteractionTestFactory.ForRemove(prompts), workspace.LockStoreRoot)
                .ExecuteAsync(new ExtensionRemoveRequest(workspace.Workspace,
                    situation == "dry-run" ? ExtensionRemoveMode.DryRun : ExtensionRemoveMode.Apply,
                    ids, automatic: !prompted, allowInteraction: prompted), cancellation.Token);
        }
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "lock-held" or "permission-required")
        {
            Assert.All(result.Effects, effect => Assert.Equal(ExtensionRemoveEffectOutcome.NotStarted, effect.Outcome));
            Assert.Equal(ExtensionRemoveLifecycleOutcome.NotStarted, result.Lifecycle.Outcome);
        }

        if (situation is "dependent-blocks" or "no-selection-non-interactive" or "dry-run" or "lock-held" or "cancelled" or "permission-required")
            Assert.Equal(before, workspace.Snapshot());
        else if (situation is "shared-file-kept" or "write-failed-partial") Assert.True(File.Exists(workspace.Combine(".agents/toolkit.md")));
        else Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        if (situation == "orphaned-dependency") Assert.True(File.Exists(workspace.Combine(".agents/base.md")));
        if (situation == "not-installed")
        {
            using var settings = JsonDocument.Parse(workspace.ReadText(".agents/open-forge.json"));
            Assert.Equal(["toolkit"], settings.RootElement.GetProperty("removedExtensions")
                .EnumerateArray().Select(value => value.GetString()));
        }
        if (situation == "write-failed-partial")
        {
            Assert.False(File.Exists(workspace.Combine(".agents/aaa.md")));
            Assert.Equal("# Toolkit\n", workspace.ReadText(".agents/toolkit.md"));
            Assert.NotNull(result.Recovery.ResidualPath);
        }
        Assert.Equal(sourceBefore, source.Snapshot());
        if (result.Recovery.ResidualPath is { } path) Assert.True(File.Exists(path));
        // Each theory case needs its own snapshot identity. They all share one test method, and a
        // shared identity makes every case see the other cases' files as missing.
        Renderers.MatchDetails(
            result,
            situation,
            result.Recovery.ResidualPath,
            source.Path,
            testName: $"{nameof(PackageRemoval)}_{situation}",
            lockFailurePath: result.Findings.FirstOrDefault(finding => finding.LockFailure is not null)?.LockFailure?.Path);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension remove dry-run warnings keep the preview headline")]
    public async Task DryRunOrphanedDependencyKeepsPreviewHeadline()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create(
            "extension-remove-dry-run-orphaned-dependency");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create(
            "extension-remove-dry-run-orphaned-dependency-source");
        source.AddPackage(
            "toolkit",
            ["base"],
            (".agents/toolkit.md", "# Toolkit\n"));
        source.AddPackage("base", [], (".agents/base.md", "# Base\n"));

        var installed = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic"]);
        Assert.Equal(0, installed.ExitCode);

        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        var result = await ExtensionRemoveOperationFactory.Create(
                ExtensionInteractionTestFactory.ForRemove(prompts), workspace.LockStoreRoot)
            .ExecuteAsync(
                new ExtensionRemoveRequest(
                    workspace.Workspace,
                    ExtensionRemoveMode.DryRun,
                    ["toolkit"],
                    automatic: true,
                    allowInteraction: false),
                TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        var retainedDependency = Assert.Single(
            result.Findings,
            finding => finding.Code == ExtensionRemoveFindingCode.LifecycleObservation);
        Assert.Equal("base", retainedDependency.Target);
        Assert.Equal(CliSemanticStatus.Attention, retainedDependency.Status);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());

        const string noFilesWereChanged = "No files were changed.";
        foreach (var detail in CommandOutputDetailVocabulary.All)
        {
            var selected = CliReportSelection.Select(
                result,
                new CliSelection(detail),
                ExtensionRemovePresentation.Rendering);
            Assert.Equal(CliHeadlineKind.Warnings, selected.Report.Headline.Kind);
            Assert.Equal("Would remove the toolkit Extension.", selected.Report.Headline.Sentence);
            Assert.Equal($"open-forge extension remove toolkit --automatic --workspace {CliCommandText.Quote(workspace.Path)}", selected.Report.Next?.Command);
            var finding = Assert.Single(selected.Report.Findings, finding => finding.Code == "extension-remove.lifecycle-observation");
            Assert.Equal(CliNextActionKind.Sentence, Assert.Single(finding.Actions).Kind);

            var rendered = CommandOutputRenderers<ExtensionRemoveResult>.Render(
                new CliPresentationRequest<ExtensionRemoveResult>(
                    result,
                    new(CliFormat.Text, detail, null)),
                ExtensionRemovePresentation.Rendering);
            Assert.Contains("Would remove the toolkit Extension.", rendered, StringComparison.Ordinal);
            Assert.DoesNotContain("Removed the toolkit Extension.", rendered, StringComparison.Ordinal);
            Assert.Equal(1, rendered.Split(noFilesWereChanged, StringSplitOptions.None).Length - 1);
        }
        Renderers.MatchDetails(result, "preview", extensionSourcePath: source.Path, testName: nameof(DryRunOrphanedDependencyKeepsPreviewHeadline) + ".preview");

        var applied = await ExtensionRemoveOperationFactory.Create(
                ExtensionInteractionTestFactory.ForRemove(prompts), workspace.LockStoreRoot)
            .ExecuteAsync(new ExtensionRemoveRequest(workspace.Workspace, ExtensionRemoveMode.Apply, ["toolkit"],
                automatic: true, allowInteraction: false), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Attention, applied.Status);
        Assert.Equal(ExtensionRemoveLifecycleOutcome.Verified, applied.Lifecycle.Outcome);
        Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/base.md")));
        var receipt = CliReportSelection.Select(applied, new CliSelection(CliDetail.Standard), ExtensionRemovePresentation.Rendering).Report;
        Assert.Equal(CliNextActionKind.Command, receipt.Next?.Kind);
        var appliedFinding = Assert.Single(receipt.Findings, finding => finding.Code == "extension-remove.lifecycle-observation");
        Assert.Equal(CliNextActionKind.Command, Assert.Single(appliedFinding.Actions).Kind);
        Renderers.MatchDetails(applied, "applied", applied.Recovery.ResidualPath, source.Path,
            testName: nameof(DryRunOrphanedDependencyKeepsPreviewHeadline) + ".applied");
    }
}
