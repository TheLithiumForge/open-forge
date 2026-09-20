using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Update;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Update.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Update;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionUpdateBeforeOutputSnapshotTests
{
    private static readonly CommandOutputRenderers<ExtensionUpdateResult> Renderers = CommandOutputRenderers<ExtensionUpdateResult>.From(ExtensionUpdatePresentation.Rendering);

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Extension update selects an installed package and applies after final confirmation")]
    public async Task AttemptedInteractiveSelection()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-output-selection");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-output-selection-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", "# Toolkit\n"));
        source.AddPackage("base", [], (".agents/base.md", "# Base\n"));
        var installed = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        source.ReplacePayload("toolkit", ".agents/toolkit.md", "# Toolkit v2\n");
        var scripted = ScriptedCliTerminal.Lines(["2", "yes"], canPrompt: true);
        var prompts = new CliPrompts(scripted.Terminal);
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        var result = await ExtensionUpdateOperationFactory.Create(
                ExtensionInteractionTestFactory.ForUpdate(prompts), workspace.LockStoreRoot)
            .ExecuteAsync(new ExtensionUpdateRequest(workspace.Workspace, ExtensionUpdateMode.Apply, [], false, source.Path,
                force: false, prune: false, automatic: false, allowInteraction: true), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Findings);
        Assert.NotEmpty(scripted.Output.ToString());
        Assert.Equal("# Toolkit v2\n", workspace.ReadText(".agents/toolkit.md"));
        Assert.NotEqual(before, workspace.Snapshot());
        Assert.Equal(sourceBefore, source.Snapshot());
        Assert.NotNull(result.Recovery.ResidualPath);
        Assert.True(File.Exists(result.Recovery.ResidualPath));
        Renderers.MatchDetails(result, "select-prompt", result.Recovery.ResidualPath, source.Path,
            testName: $"{nameof(AttemptedInteractiveSelection)}_select-prompt");
    }

    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "Extension update output preserves package changes, retirement and explicit selection")]
    [InlineData("up-to-date", (int)CliSemanticStatus.Complete)]
    [InlineData("files-replaced", (int)CliSemanticStatus.Complete)]
    [InlineData("new-version-with-new-files", (int)CliSemanticStatus.Complete)]
    [InlineData("retired-kept", (int)CliSemanticStatus.Attention)]
    [InlineData("retired-pruned", (int)CliSemanticStatus.Complete)]
    [InlineData("all-packages", (int)CliSemanticStatus.Complete)]
    [InlineData("no-selection-non-interactive", (int)CliSemanticStatus.Invalid)]
    [InlineData("ownership-unknown", (int)CliSemanticStatus.Attention)]
    [InlineData("permission-required", (int)CliSemanticStatus.Blocked)]
    [InlineData("dry-run", (int)CliSemanticStatus.Complete)]
    [InlineData("source-unreadable", (int)CliSemanticStatus.Incomplete)]
    [InlineData("lock-held", (int)CliSemanticStatus.Blocked)]
    [InlineData("cancelled", (int)CliSemanticStatus.Interrupted)]
    [InlineData("write-failed-partial", (int)CliSemanticStatus.Failed)]
    public async Task PackageUpdate(string situation, int status)
    {
        if (situation == "write-failed-partial" && !OperatingSystem.IsWindows())
            Assert.Skip("This deterministic replacement failure requires Windows file sharing.");
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-update-output");
        await workspace.SeedFrameworkAsync();
        using var source = ExtensionInstallCatalogue.Create("extension-update-output-source");
        source.AddPackage("toolkit", [], (".agents/toolkit.md", "# Toolkit v1\n"));
        if (situation == "write-failed-partial") source.AddPayload("toolkit", ".agents/aaa.md", "# Earlier v1\n");
        if (situation is "all-packages" or "no-selection-non-interactive")
            source.AddPackage("base", [], (".agents/base.md", "# Base v1\n"));
        var installed = await workspace.RunAsync(["extension", "install", "--all", "--source", source.Path, "--automatic"]);
        Assert.Equal(0, installed.ExitCode);
        if (situation is "retired-kept" or "retired-pruned") source.RemovePayload("toolkit", ".agents/toolkit.md");
        if (situation is "files-replaced" or "dry-run" or "all-packages" or "lock-held" or "write-failed-partial") source.ReplacePayload("toolkit", ".agents/toolkit.md", "# Toolkit v2\n");
        if (situation == "write-failed-partial") source.ReplacePayload("toolkit", ".agents/aaa.md", "# Earlier v2\n");
        if (situation == "all-packages") source.ReplacePayload("base", ".agents/base.md", "# Base v2\n");
        if (situation == "new-version-with-new-files")
        {
            source.SetVersion("toolkit", "2.0.0");
            source.AddPayload("toolkit", ".agents/new.md", "# New guide\n");
        }
        if (situation == "ownership-unknown") workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, "{ unavailable ownership");
        if (situation == "permission-required")
        {
            var ownership = JsonNode.Parse(workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath))!;
            ownership["extensions"]![0]!["paths"]!.AsArray().Add((JsonNode?)JsonValue.Create("workspace-note.md"));
            workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
        }
        var scripted = ScriptedCliTerminal.Lines([], canPrompt: false);
        var prompts = new CliPrompts(scripted.Terminal);
        using var cancellation = new CancellationTokenSource();
        if (situation == "cancelled") cancellation.Cancel();
        var before = workspace.Snapshot();
        var sourceBefore = source.Snapshot();
        ExtensionUpdateResult result;
        using (var held = situation == "lock-held" ? workspace.HoldLock() : null)
        using (var denied = situation == "write-failed-partial"
                   ? File.Open(workspace.Combine(".agents/toolkit.md"), FileMode.Open, FileAccess.Read, FileShare.Read) : null)
        using (var unreadable = situation == "source-unreadable"
                   ? File.Open(Path.Combine(source.PackagePath("toolkit"), "extension.json"), FileMode.Open, FileAccess.Read, FileShare.None) : null)
        {
            result = await ExtensionUpdateOperationFactory.Create(
                    ExtensionInteractionTestFactory.ForUpdate(prompts), workspace.LockStoreRoot)
                .ExecuteAsync(new ExtensionUpdateRequest(workspace.Workspace,
                    situation == "dry-run" ? ExtensionUpdateMode.DryRun : ExtensionUpdateMode.Apply,
                    situation is "all-packages" or "no-selection-non-interactive" ? [] : ["toolkit"],
                    all: situation == "all-packages", source.Path, force: false, prune: situation is "retired-pruned" or "permission-required",
                    automatic: true, allowInteraction: false), cancellation.Token);
        }
        Assert.Equal((CliSemanticStatus)status, result.Status);
        if (situation is "up-to-date" or "dry-run" or "source-unreadable" or "lock-held" or "cancelled" or "no-selection-non-interactive" or "ownership-unknown" or "permission-required")
            Assert.Equal(before, workspace.Snapshot());
        if (situation is "files-replaced" or "all-packages") Assert.Equal("# Toolkit v2\n", workspace.ReadText(".agents/toolkit.md"));
        if (situation == "retired-pruned") Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        if (situation == "retired-kept") Assert.True(File.Exists(workspace.Combine(".agents/toolkit.md")));
        if (situation == "write-failed-partial")
        {
            Assert.Equal("# Earlier v2\n", workspace.ReadText(".agents/aaa.md"));
            Assert.Equal("# Toolkit v1\n", workspace.ReadText(".agents/toolkit.md"));
            Assert.NotNull(result.Recovery.ResidualPath);
        }
        Assert.Equal(sourceBefore, source.Snapshot());
        if (result.Recovery.ResidualPath is { } path) Assert.True(File.Exists(path));
        Renderers.MatchDetails(result, situation, result.Recovery.ResidualPath, source.Path,
            testName: $"{nameof(PackageUpdate)}_{situation}");
    }
}
