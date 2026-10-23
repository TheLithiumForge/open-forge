using System.Text.Json;
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
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Update;

public sealed class ExtensionUpdateRestoredCategoryIntegrationTests
{
    [Fact(DisplayName = "Essentials records excluded Core Templates then Update delivers its files after Templates is restored")]
    [Trait("Feature", "extension-update"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task RestoringTemplatesCompletesTheRecordedPackageJourney()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-restored-category");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--preset", "essentials", "--automatic"])).ExitCode);
        var beforeInstall = workspace.Snapshot();
        var installPreview = await workspace.RunAsync(["extension", "install", "core-templates", "--dry-run", "--format", "json"]);
        Assert.Equal(0, installPreview.ExitCode);
        Assert.Equal(beforeInstall, workspace.Snapshot());
        var installed = await workspace.RunAsync(["extension", "install", "core-templates", "--automatic", "--format", "json"]);
        Assert.Equal(0, installed.ExitCode);
        Assert.Empty(installed.StandardError);
        using var installation = JsonDocument.Parse(installed.StandardOutput);
        Assert.Empty(installation.RootElement.GetProperty("effects").EnumerateArray());
        var recorded = Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray());
        Assert.Equal("core-templates", recorded.GetProperty("id").GetString());
        Assert.Empty(recorded.GetProperty("paths").EnumerateArray());
        Assert.False(Directory.Exists(workspace.Combine(".agents/templates")));

        var beforeRestore = workspace.Snapshot();
        var restorePreview = await workspace.RunAsync(["route", "init", "templates", "--framework", "--dry-run"]);
        Assert.Equal(0, restorePreview.ExitCode);
        Assert.Equal(beforeRestore, workspace.Snapshot());
        var restored = await workspace.RunAsync(["route", "init", "templates", "--framework"]);
        Assert.True(restored.ExitCode == 0, restored.StandardOutput + restored.StandardError);
        Assert.True(File.Exists(workspace.Combine(".agents/templates/_templates.md")));
        var beforeDelivery = workspace.Snapshot();
        var capture = new ReadCommandOutputCapture(workspace.Path);
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var preview in new[] { true, false })
        {
            var arguments = new List<string> { "extension", "install", "core-templates", preview ? "--dry-run" : "--automatic" };
            var blocked = await workspace.RunAsync([.. arguments, "--format", "json"]);
            Assert.Equal(5, blocked.ExitCode);
            using var document = JsonDocument.Parse(blocked.StandardOutput);
            Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            Assert.Equal("extension-install.package-contents-changed",
                Assert.Single(document.RootElement.GetProperty("findings").EnumerateArray()).GetProperty("code").GetString());
            Assert.Equal("open-forge extension update core-templates", document.RootElement.GetProperty("next").GetProperty("command").GetString());
            await capture.MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = preview ? "install-restored-preview" : "install-restored-apply",
                Arguments = [.. arguments],
                ExitCode = 5,
            }, snapshotCollector: snapshots);
            Assert.Equal(beforeDelivery, workspace.Snapshot());
        }

        var renderers = CommandOutputRenderers<ExtensionUpdateResult>.From(ExtensionUpdatePresentation.Rendering);
        var prompts = new CliPrompts(ScriptedCliTerminal.Lines([], canPrompt: false).Terminal);
        var operation = ExtensionUpdateOperationFactory.Create(ExtensionInteractionTestFactory.ForUpdate(prompts), workspace.LockStoreRoot);
        foreach (var mode in new[] { ExtensionUpdateMode.DryRun, ExtensionUpdateMode.Apply })
        {
            var result = await operation.ExecuteAsync(new ExtensionUpdateRequest(workspace.Workspace, mode,
                ["core-templates"], all: false, sourcePath: null, force: false, prune: false, automatic: true, allowInteraction: false),
                TestContext.Current.CancellationToken);
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            var package = Assert.Single(result.Packages);
            Assert.Equal(package.FromVersion, package.ToVersion);
            var files = result.Effects.Where(effect => effect.Kind == ExtensionUpdateEffectKind.PackageFile).ToArray();
            Assert.Equal(8, files.Length);
            Assert.All(files, effect => Assert.Equal(mode == ExtensionUpdateMode.DryRun
                ? ExtensionUpdateEffectOutcome.Planned : ExtensionUpdateEffectOutcome.Verified, effect.Outcome));
            if (mode == ExtensionUpdateMode.DryRun) Assert.Equal(beforeDelivery, workspace.Snapshot());
            renderers.MatchDetails(result, mode == ExtensionUpdateMode.DryRun ? "update-restored-preview" : "update-restored-apply",
                result.Recovery.ResidualPath, snapshotCollector: snapshots);
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
        Assert.Equal(8, Assert.Single(workspace.ReadExtensionOwnership().EnumerateArray()).GetProperty("paths").GetArrayLength());
        Assert.True(File.Exists(workspace.Combine(".agents/templates/core/_core.md")));
        var completed = workspace.Snapshot();
        Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "core-templates", "--automatic"])).ExitCode);
        Assert.Equal(completed, workspace.Snapshot());
    }

    [Fact(DisplayName = "Different sources versions dependencies and damaged receipts retain the Install mismatch explanation")]
    [Trait("Feature", "extension-install"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public async Task MismatchedRecordsDoNotClaimUndeliveredFiles()
    {
        using var workspace = ExtensionInstallIntegrationWorkspace.Create("extension-restored-mismatch");
        Assert.Equal(0, (await workspace.RunAsync(["install", "--preset", "essentials", "--automatic"])).ExitCode);
        Assert.Equal(0, (await workspace.RunAsync(["extension", "install", "core-templates", "--automatic"])).ExitCode);
        var restored = await workspace.RunAsync(["route", "init", "templates", "--framework"]);
        Assert.True(restored.ExitCode == 0, restored.StandardOutput + restored.StandardError);
        var recordedOwnership = workspace.ReadText(ExtensionInstallIntegrationWorkspace.OwnershipPath);
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var mismatch in new[] { "source", "version", "dependencies", "paths" })
        {
            var ownership = JsonNode.Parse(recordedOwnership)?.AsObject()
                ?? throw new InvalidOperationException("The fixture requires a readable ownership record.");
            var packages = ownership["extensions"]?.AsArray()
                ?? throw new InvalidOperationException("The fixture requires Extension records.");
            var package = Assert.Single(packages)?.AsObject()
                ?? throw new InvalidOperationException("The fixture requires one Extension record.");
            package[mismatch] = mismatch switch
            {
                "source" => JsonValue.Create("different-source"),
                "version" => JsonValue.Create("different-version"),
                "dependencies" => new JsonArray("workflows"),
                "paths" => new JsonArray(".agents/templates/retired.md"),
                _ => throw new ArgumentOutOfRangeException(nameof(mismatch)),
            };
            workspace.ReplaceText(ExtensionInstallIntegrationWorkspace.OwnershipPath, ownership.ToJsonString());
            var before = workspace.Snapshot();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = "mismatch-" + mismatch,
                Arguments = ["extension", "install", "core-templates", "--dry-run"],
                ExitCode = 5,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.Snapshot());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
