using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedInstallProcessTests
{
    [Fact(DisplayName = "Published root and Install leaf help expose one implemented direct command"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpIsTruthfulAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var root = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["--help"],
            workspace.ProcessEnvironment);
        var leaf = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["install", "--help", "--workspace", workspace.Combine("missing")],
            workspace.ProcessEnvironment);

        Assert.Equal(0, root.ExitCode);
        Assert.Equal(string.Empty, root.StandardError);
        Assert.Single(
            root.StandardOutput.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries),
            line => line.TrimStart().StartsWith("install", StringComparison.Ordinal));
        Assert.Contains("Lifecycle", root.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(0, leaf.ExitCode);
        Assert.Equal(string.Empty, leaf.StandardError);
        Assert.Contains(
            "open-forge install [--force] [--automatic] [--dry-run] [global flags]",
            leaf.StandardOutput,
            StringComparison.Ordinal);
        Assert.Contains("Confirmation", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", leaf.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(workspace.Combine("missing")));
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published redirected human Install requires automatic without prompting or writing"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedRedirectedHumanWriteIsInvalidAndSilent()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            workspace.Path,
            workspace.SnapshotState,
            ["install"],
            workspace.ProcessEnvironment);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.DoesNotContain("Apply this Install plan?", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Status: invalid", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("open-forge install --automatic", result.StandardError, StringComparison.Ordinal);
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Relocated published Install reads its embedded payload and emits exact ordered dry-run JSON"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task RelocatedPublishedJsonDryRunUsesEmbeddedPayloadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var relocated = RelocatedPublishedInstallLayout.Create(target);
        using var workspace = PublishedInstallWorkspace.Create();
        var before = workspace.SnapshotState();

        var result = await relocated.RunAsync(
            workspace.Path,
            ["install", "--automatic", "--dry-run", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("Apply this Install plan?", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(workspace.Path, root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var commandResult = root.GetProperty("result");
        Assert.Equal(
            ["mode", "force", "automatic", "source", "classification", "footprint", "effects", "lifecycle", "recovery", "verification", "findings"],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("dry-run", commandResult.GetProperty("mode").GetString());
        Assert.False(commandResult.GetProperty("force").GetBoolean());
        Assert.True(commandResult.GetProperty("automatic").GetBoolean());
        var source = commandResult.GetProperty("source");
        Assert.Equal(
            ["inventoryFingerprint", "assetCount"],
            source.EnumerateObject().Select(property => property.Name));
        Assert.False(string.IsNullOrWhiteSpace(source.GetProperty("inventoryFingerprint").GetString()));
        Assert.Equal(PublishedInstallWorkspace.EmbeddedPayloadPaths.Count + 2, source.GetProperty("assetCount").GetInt32());
        Assert.Equal("safe-absence", commandResult.GetProperty("classification").GetString());
        var footprint = commandResult.GetProperty("footprint");
        Assert.Equal(
            ["payloadFiles", "managedRegions", "generatedRegions"],
            footprint.EnumerateObject().Select(property => property.Name));
        Assert.True(footprint.GetProperty("payloadFiles").GetInt32() > 0);
        Assert.True(footprint.GetProperty("managedRegions").GetInt32() > 0);
        Assert.True(footprint.GetProperty("generatedRegions").GetInt32() > 0);
        var firstEffect = commandResult.GetProperty("effects")[0];
        Assert.Equal(
            ["path", "kind", "action", "sourceAssetPath", "outcome", "residual"],
            firstEffect.EnumerateObject().Select(property => property.Name));
        Assert.Equal(".agents", firstEffect.GetProperty("path").GetString());
        Assert.Equal("directory", firstEffect.GetProperty("kind").GetString());
        Assert.Equal("create", firstEffect.GetProperty("action").GetString());
        Assert.Equal(JsonValueKind.Null, firstEffect.GetProperty("sourceAssetPath").ValueKind);
        Assert.Equal("planned", firstEffect.GetProperty("outcome").GetString());
        Assert.Equal("none", firstEffect.GetProperty("residual").GetString());
        Assert.Equal("publish", commandResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("planned", commandResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("not-required", commandResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal("not-requested", commandResult.GetProperty("verification").GetString());
        Assert.Empty(commandResult.GetProperty("findings").EnumerateArray());
        Assert.Equal(before, workspace.SnapshotState());
        workspace.AssertNoLockInfrastructure();
    }

    [Fact(DisplayName = "Published automatic Install applies, verifies a no-op, and preserves divergence for Update"), Trait("Feature", "install-command"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedApplyConvergesAndManagedDivergenceRemainsUpdateOwned()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedInstallWorkspace.Create();
        var arguments = new[] { "install", "--automatic" };

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Classification: safe-absence", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Verification: verified", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(PublishedInstallWorkspace.EmbeddedPayloadPaths, workspace.InstalledPayloadPaths());
        Assert.Contains(
            "Before starting a task, read `.agents/loader.md`.",
            await workspace.ReadTextAsync("AGENTS.md", TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
        Assert.Contains(
            "@.agents/loader.md",
            await workspace.ReadTextAsync("CLAUDE.md", TestContext.Current.CancellationToken),
            StringComparison.Ordinal);
        workspace.AssertPersistentExternalLock();
        using (var lifecycle = JsonDocument.Parse(await workspace.ReadTextAsync(
                   ".agents/open-forge.lifecycle.json",
                   TestContext.Current.CancellationToken)))
        {
            var framework = lifecycle.RootElement.GetProperty("framework");
            Assert.Equal("embedded-framework", framework.GetProperty("source").GetProperty("id").GetString());
            Assert.False(string.IsNullOrWhiteSpace(
                framework.GetProperty("source").GetProperty("inventoryFingerprint").GetString()));
            Assert.Contains(framework.GetProperty("targets").EnumerateArray(), targetValue =>
                targetValue.GetProperty("region").ValueKind == JsonValueKind.String
                && targetValue.GetProperty("sourceAssetPath").ValueKind == JsonValueKind.Null);
            Assert.All(
                framework.GetProperty("targets").EnumerateArray().Where(targetValue =>
                    targetValue.GetProperty("region").ValueKind == JsonValueKind.Null),
                targetValue => Assert.Equal(
                    JsonValueKind.String,
                    targetValue.GetProperty("sourceAssetPath").ValueKind));
        }

        var afterApply = workspace.SnapshotState();
        var noOp = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            arguments,
            workspace.ProcessEnvironment);

        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        Assert.Contains("Classification: trusted-exact", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Effects: 0", noOp.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(afterApply, workspace.SnapshotState());

        workspace.ReplaceInstalledText(".agents/loader.md", "# Managed divergence\n");
        var beforeBlocked = workspace.SnapshotState();
        var blocked = await PublishedProcessTestSupport.RunAsync(
            target,
            workspace.Path,
            ["install", "--force", "--automatic", "--json"],
            workspace.ProcessEnvironment);

        Assert.Equal(5, blocked.ExitCode);
        Assert.Equal(string.Empty, blocked.StandardError);
        using var blockedDocument = JsonDocument.Parse(blocked.StandardOutput);
        var blockedRoot = blockedDocument.RootElement;
        Assert.Equal("blocked", blockedRoot.GetProperty("status").GetString());
        var blockedResult = blockedRoot.GetProperty("result");
        Assert.Equal("managed-divergence", blockedResult.GetProperty("classification").GetString());
        Assert.Empty(blockedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", blockedResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("not-requested", blockedResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal(
            "install.managed-divergence",
            blockedResult.GetProperty("findings")[0].GetProperty("code").GetString());
        Assert.Equal("open-forge update", blockedRoot.GetProperty("next").GetProperty("command").GetString());
        Assert.Equal(beforeBlocked, workspace.SnapshotState());
    }
}
