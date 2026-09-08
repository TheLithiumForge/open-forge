using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionCreateProcessTests
{
    [Fact(DisplayName = "Published Extension Create help is read-only"), Trait("Feature", "extension-create"), Trait("Evidence", "EndToEnd")]
    public static async Task PublishedHelpIsTruthfulAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-extension-create-help");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotHashes, ["extension", "create", "--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge extension create", result.StandardOutput, StringComparison.Ordinal);
    }



    [Fact(DisplayName = "Published automatic Extension Create applies once and verifies an unchanged no-op"), Trait("Feature", "extension-create"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedAutomaticApplyConvergesWithoutWorkspaceLifecycleOrRecoveryEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionCreateWorkspace.Create();
        using var lockStore = PublishedWorkspaceLockStore.Create("e2e-extension-create-lock-store");
        _ = lockStore.Track(working.WorkspacePath);
        var beforeWorkspace = working.SnapshotWorkspace();
        var arguments = new[]
        {
            "extension", "create", PublishedExtensionCreateWorkspace.StableId,
            "--path", working.CataloguePath,
            "--workspace", working.WorkspacePath,
            "--automatic",
            "--view=compact",
        };

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            lockStore.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("intended=2; applied=2", applied.StandardOutput, StringComparison.Ordinal);
        Assert.True(File.Exists(working.ManifestPath));
        Assert.True(Directory.Exists(working.PayloadAgentsPath));
        using (var manifest = JsonDocument.Parse(await File.ReadAllTextAsync(working.ManifestPath, TestContext.Current.CancellationToken)))
        {
            Assert.Equal(PublishedExtensionCreateWorkspace.StableId, manifest.RootElement.GetProperty("id").GetString());
            Assert.Equal("Development Toolkit", manifest.RootElement.GetProperty("name").GetString());
            Assert.Equal("Open Forge Extension package development-toolkit.", manifest.RootElement.GetProperty("description").GetString());
            Assert.Equal("0.1.0", manifest.RootElement.GetProperty("version").GetString());
            Assert.Empty(manifest.RootElement.GetProperty("dependencies").EnumerateArray());
        }

        var afterApply = working.SnapshotCatalogue();
        var noOp = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            lockStore.EnvironmentVariables);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("intended=2; applied=0", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(afterApply, working.SnapshotCatalogue());
        Assert.Equal(beforeWorkspace, working.SnapshotWorkspace());
        lockStore.AssertNoInfrastructure();
        Assert.Equal("preserve lifecycle", await File.ReadAllTextAsync(
            Path.Combine(working.WorkspacePath, ".agents", "open-forge.lifecycle.json"),
            TestContext.Current.CancellationToken));
        Assert.Equal("preserve recovery evidence", await File.ReadAllTextAsync(
            Path.Combine(working.WorkspacePath, ".agents", "recovery-sentinel.zip"),
            TestContext.Current.CancellationToken));
        Assert.DoesNotContain(
            Directory.EnumerateFiles(working.CataloguePath, "*", SearchOption.AllDirectories),
            path => path.EndsWith(".zip", StringComparison.Ordinal) || path.EndsWith(".draft", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Published Extension Create JSON is exact ordered prompt-free stdout with no writes"), Trait("Feature", "extension-create"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedJsonPreservesExactResultOrderAndStdoutIsolation()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionCreateWorkspace.Create();
        var beforeCatalogue = working.SnapshotCatalogue();
        var beforeWorkspace = working.SnapshotWorkspace();

        var result = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            [
                "extension", "create", PublishedExtensionCreateWorkspace.StableId,
                "--path", working.CataloguePath,
                "--name", "Development Toolkit",
                "--description", "Adds development workflows",
                "--package-version", "0.2.0",
                "--dependency", "zeta",
                "--dependency", "alpha",
                "--workspace", working.WorkspacePath,
                "--dry-run",
                "--json",
            ]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("Stable ID (", result.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Catalogue path (", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("extension create", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("workspace").ValueKind);
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var commandResult = root.GetProperty("result");
        Assert.Equal(
            ["catalogue", "destination", "id", "manifest", "mode", "intendedEffects", "appliedEffects", "verification", "workspaceLifecycleChanged"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal(working.CataloguePath, commandResult.GetProperty("catalogue").GetString());
        Assert.Equal(working.DestinationPath, commandResult.GetProperty("destination").GetString());
        Assert.Equal(PublishedExtensionCreateWorkspace.StableId, commandResult.GetProperty("id").GetString());
        var manifest = commandResult.GetProperty("manifest");
        Assert.Equal(
            ["name", "description", "version", "dependencies"],
            manifest.EnumerateObject().Select(property => property.Name));
        Assert.Equal(["alpha", "zeta"], manifest.GetProperty("dependencies").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal("dry-run", commandResult.GetProperty("mode").GetString());
        Assert.Equal(2, commandResult.GetProperty("intendedEffects").GetArrayLength());
        Assert.Empty(commandResult.GetProperty("appliedEffects").EnumerateArray());
        Assert.False(commandResult.GetProperty("workspaceLifecycleChanged").GetBoolean());
        Assert.Equal(beforeCatalogue, working.SnapshotCatalogue());
        Assert.Equal(beforeWorkspace, working.SnapshotWorkspace());
    }


}
