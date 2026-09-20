using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Extension.Remove;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Result;
using OpenForge.Cli.Core.Presentation.Extension.Remove;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Install;
using OpenForge.Cli.IntegrationTests.Commands.Extension.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Remove;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ExtensionRemoveBeforeOutputSnapshotTests
{
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
        if (situation is "dependent-blocks" or "no-selection-non-interactive" or "not-installed" or "dry-run" or "lock-held" or "cancelled" or "permission-required")
            Assert.Equal(before, workspace.Snapshot());
        else if (situation is "shared-file-kept" or "write-failed-partial") Assert.True(File.Exists(workspace.Combine(".agents/toolkit.md")));
        else Assert.False(File.Exists(workspace.Combine(".agents/toolkit.md")));
        if (situation == "orphaned-dependency") Assert.True(File.Exists(workspace.Combine(".agents/base.md")));
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
            testName: $"{nameof(PackageRemoval)}_{situation}");
    }
}
