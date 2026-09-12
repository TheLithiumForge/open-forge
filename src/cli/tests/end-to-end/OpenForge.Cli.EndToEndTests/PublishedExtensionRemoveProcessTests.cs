using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionRemoveProcessTests
{
    [Fact(DisplayName = "Published Extension Remove help is reachable without workspace inspection or writes"), Trait("Feature", "extension-remove"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpIsReachableWithoutWorkspaceInspectionOrWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-extension-remove-help");
        using var lockStore = PublishedWorkspaceLockStore.Create("e2e-extension-remove-help-locks");
        _ = lockStore.Track(working.Path);

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["extension", "remove", "--help"],
            lockStore.EnvironmentVariables);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(
            "open-forge extension remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global options]",
            string.Join(" ", result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Contains("Selection and dependencies", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Ownership and changed content", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.StandardOutput, StringComparison.Ordinal);
        lockStore.AssertNoInfrastructure();
    }

    [Fact(DisplayName = "Published Extension Remove releases one package without its source and converges a trusted repeat"), Trait("Feature", "extension-remove"), Trait("Evidence", "EndToEnd")]
    public async Task DefaultRemovalPreservesUnownedContentAndRepeatsAsNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        await SeedExtensionAsync(target, working);

        var sourceBefore = working.SnapshotSource();
        var unownedPath = Path.Combine(working.WorkspacePath, "workspace-note.md");
        var unownedBytes = File.ReadAllBytes(unownedPath);
        var arguments = new[]
        {
            "extension", "remove", "toolkit",
            "--automatic",
            "--workspace", working.WorkspacePath,
        };

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains("Status: complete", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("IDs: toolkit", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Prune: false", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Package source unchanged: true", applied.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(working.TargetPath));
        Assert.Equal(unownedBytes, File.ReadAllBytes(unownedPath));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertPersistentLock();
        var afterRemoval = SnapshotWorkspaceAndSource(working);

        var repeated = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.WorkspacePath,
            () => SnapshotWorkspaceAndSource(working),
            arguments,
            working.EnvironmentVariables);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        Assert.Contains("Status: complete", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Effects: 0", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("already-current", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(afterRemoval, SnapshotWorkspaceAndSource(working));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertPersistentLock();
    }

    [Fact(DisplayName = "Published Extension Remove uses one prune plan for JSON preview and apply while preserving source"), Trait("Feature", "extension-remove"), Trait("Evidence", "EndToEnd")]
    public async Task JsonPrunePreviewAndApplySharePlanAndDeleteChangedFinalOwner()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        await SeedExtensionAsync(target, working);

        var sourceBefore = working.SnapshotSource();
        var unownedPath = Path.Combine(working.WorkspacePath, "workspace-note.md");
        var unownedBytes = File.ReadAllBytes(unownedPath);
        var changedPayload = OpenForgeDocumentSeed.Metadata(
            "toolkit changed",
            ["Extension"],
            "# toolkit changed\n");
        File.WriteAllText(working.TargetPath, changedPayload);
        var beforePreview = SnapshotWorkspaceAndSource(working);
        var applyArguments = new[]
        {
            "extension", "remove", "toolkit",
            "--prune", "--automatic", "--json",
            "--workspace", working.WorkspacePath,
        };
        string[] previewArguments = [.. applyArguments, "--dry-run"];

        var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.WorkspacePath,
            () => SnapshotWorkspaceAndSource(working),
            previewArguments,
            working.EnvironmentVariables);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        using var previewDocument = JsonDocument.Parse(preview.StandardOutput);
        var previewRoot = previewDocument.RootElement;
        AssertEnvelope(previewRoot, "dry-run");
        var previewResult = previewRoot.GetProperty("result");
        Assert.Equal("dry-run", previewResult.GetProperty("mode").GetString());
        Assert.True(previewResult.GetProperty("prune").GetBoolean());
        Assert.True(previewResult.GetProperty("automatic").GetBoolean());
        Assert.Equal("explicit-ids", previewResult.GetProperty("selection").GetProperty("selectedBy").GetString());
        Assert.Equal(["toolkit"], previewResult.GetProperty("selection").GetProperty("ids").EnumerateArray().Select(id => id.GetString()));
        AssertChangedFinalOwnerDeletePlan(previewResult);
        Assert.All(
            previewResult.GetProperty("effects").EnumerateArray(),
            effect => Assert.Equal("planned", effect.GetProperty("outcome").GetString()));
        Assert.Equal("not-created", previewResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.Equal(JsonValueKind.Null, previewResult.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.True(previewResult.GetProperty("packageSourceUnchanged").GetBoolean());
        Assert.Empty(previewResult.GetProperty("findings").EnumerateArray());
        Assert.Equal(beforePreview, SnapshotWorkspaceAndSource(working));
        Assert.Equal(changedPayload, File.ReadAllText(working.TargetPath));
        Assert.Equal(unownedBytes, File.ReadAllBytes(unownedPath));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertNoRecoveryArtifacts();

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            applyArguments,
            working.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        var appliedRoot = appliedDocument.RootElement;
        AssertEnvelope(appliedRoot, "apply");
        var appliedResult = appliedRoot.GetProperty("result");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.True(appliedResult.GetProperty("prune").GetBoolean());
        Assert.True(appliedResult.GetProperty("automatic").GetBoolean());
        Assert.Equal(
            previewResult.GetProperty("selection").GetRawText(),
            appliedResult.GetProperty("selection").GetRawText());
        Assert.Equal(
            previewResult.GetProperty("dependencies").GetRawText(),
            appliedResult.GetProperty("dependencies").GetRawText());
        Assert.Equal(
            previewResult.GetProperty("paths").GetRawText(),
            appliedResult.GetProperty("paths").GetRawText());
        Assert.Equal(
            previewResult.GetProperty("generatedNavigation").GetRawText(),
            appliedResult.GetProperty("generatedNavigation").GetRawText());
        Assert.Equal(EffectPlans(previewResult), EffectPlans(appliedResult));

        var targetEffect = Assert.Single(
            appliedResult.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal("delete", targetEffect.GetProperty("action").GetString());
        Assert.Equal("verified", targetEffect.GetProperty("outcome").GetString());
        Assert.Equal("none", targetEffect.GetProperty("residual").GetString());
        Assert.Equal("publish", appliedResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("verified", appliedResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal("removed", appliedResult.GetProperty("recovery").GetProperty("state").GetString());
        Assert.NotEmpty(appliedResult.GetProperty("recovery").GetProperty("protectedPaths").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, appliedResult.GetProperty("recovery").GetProperty("residualPath").ValueKind);
        Assert.All(
            appliedResult.GetProperty("verification").EnumerateObject(),
            verification => Assert.Equal("verified", verification.Value.GetString()));
        Assert.True(appliedResult.GetProperty("packageSourceUnchanged").GetBoolean());
        Assert.Empty(appliedResult.GetProperty("findings").EnumerateArray());
        Assert.Equal(JsonValueKind.Null, appliedRoot.GetProperty("next").ValueKind);
        Assert.False(File.Exists(working.TargetPath));
        Assert.Equal(unownedBytes, File.ReadAllBytes(unownedPath));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertPersistentLock();
        working.AssertNoRecoveryArtifacts();
    }

    private static async Task SeedExtensionAsync(
        PublishedExecutableTarget target,
        PublishedExtensionInstallWorkspace working)
    {
        var framework = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            ["install", "--automatic", "--workspace", working.WorkspacePath],
            working.EnvironmentVariables);
        Assert.Equal(0, framework.ExitCode);
        Assert.Equal(string.Empty, framework.StandardError);

        var extension = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            [
                "extension", "install", "toolkit",
                "--source", working.CataloguePath,
                "--automatic", "--json",
                "--workspace", working.WorkspacePath,
            ],
            working.EnvironmentVariables);
        Assert.Equal(0, extension.ExitCode);
        Assert.Equal(string.Empty, extension.StandardError);
    }

    private static SortedDictionary<string, string> SnapshotWorkspaceAndSource(
        PublishedExtensionInstallWorkspace working)
    {
        var snapshot = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var item in working.SnapshotWorkspace())
        {
            snapshot[$"workspace/{item.Key}"] = item.Value;
        }

        foreach (var item in working.SnapshotSource())
        {
            snapshot[$"source/{item.Key}"] = item.Value;
        }

        return snapshot;
    }

    private static void AssertEnvelope(JsonElement root, string mode)
    {
        AssertOrder(root, "schemaVersion", "command", "status", "workspace", "result", "next");
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        AssertOrder(
            root.GetProperty("result"),
            "mode", "prune", "automatic", "selection", "dependencies", "paths",
            "generatedNavigation", "effects", "permissions", "lifecycle", "recovery", "verification",
            "packageSourceUnchanged", "findings");
        Assert.Equal(mode, root.GetProperty("result").GetProperty("mode").GetString());
    }

    private static void AssertChangedFinalOwnerDeletePlan(JsonElement result)
    {
        var path = Assert.Single(
            result.GetProperty("paths").EnumerateArray(),
            item => item.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal("changed-final-owner", path.GetProperty("classification").GetString());
        Assert.Equal("delete", path.GetProperty("action").GetString());

        var effect = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            item => item.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal("toolkit", effect.GetProperty("packageId").GetString());
        Assert.Equal("package-file", effect.GetProperty("kind").GetString());
        Assert.Equal("delete", effect.GetProperty("action").GetString());
        Assert.Equal("none", effect.GetProperty("residual").GetString());
    }

    private static string[] EffectPlans(JsonElement result)
        => [.. result.GetProperty("effects")
            .EnumerateArray()
            .Select(effect => string.Join(
                "\u001f",
                effect.GetProperty("path").GetString() ?? string.Empty,
                effect.GetProperty("packageId").GetString() ?? string.Empty,
                effect.GetProperty("kind").GetString() ?? string.Empty,
                effect.GetProperty("action").GetString() ?? string.Empty,
                effect.GetProperty("residual").GetString() ?? string.Empty))];

    private static void AssertOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
