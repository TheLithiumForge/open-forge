using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionRemoveProcessTests
{
    [Fact(DisplayName = "Published Extension Remove rejects the removed prune option before any writes"), Trait("Feature", "extension-remove"), Trait("Evidence", "EndToEnd")]
    public async Task RemovedPruneIsAnUnknownOption()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        await SeedExtensionAsync(target, working);
        var run = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.WorkspacePath, () => SnapshotWorkspaceAndSource(working),
            ["extension", "remove", "toolkit", "--prune", "--automatic", "--format=json", "--workspace", working.WorkspacePath],
            working.EnvironmentVariables);

        Assert.Equal(4, run.ExitCode);
        Assert.Equal(string.Empty, run.StandardOutput);
        Assert.Contains("Unrecognized command or argument '--prune'.", run.StandardError, StringComparison.Ordinal);
        Assert.True(File.Exists(working.TargetPath));
    }

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
            "open-forge extension remove [<stable-id>...] [--automatic] [--dry-run] [--allow-path <path>] [global options]",
            string.Join(" ", result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        Assert.Contains("Selection and dependencies", result.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Ownership and recovery", result.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("Removed the toolkit Extension.", applied.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("deleted", applied.StandardOutput, StringComparison.Ordinal);
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
        Assert.Contains("No files are recorded for toolkit, so there is nothing to remove.", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(afterRemoval, SnapshotWorkspaceAndSource(working));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertPersistentLock();
    }

    [Fact(DisplayName = "Published Extension Remove uses one deletion plan for JSON preview and apply while preserving source"), Trait("Feature", "extension-remove"), Trait("Evidence", "EndToEnd")]
    public async Task JsonPreviewAndApplySharePlanAndDeleteChangedFinalOwner()
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
            "--automatic", "--format=json",
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
        var previewResult = previewRoot.GetProperty("data");
        Assert.Equal("dry-run", previewResult.GetProperty("mode").GetString());
        Assert.False(previewResult.TryGetProperty("prune", out _));
        Assert.True(previewResult.GetProperty("automatic").GetBoolean());
        Assert.Equal(["toolkit"], previewResult.GetProperty("packages").EnumerateArray()
            .Select(package => package.GetProperty("id").GetString()));
        AssertChangedFinalOwnerDeletePlan(previewResult);
        Assert.Equal("planned", Assert.Single(
            previewResult.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md")
            .GetProperty("outcome").GetString());
        Assert.Empty(previewRoot.GetProperty("findings").EnumerateArray());
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
        var appliedResult = appliedRoot.GetProperty("data");
        Assert.Equal("apply", appliedResult.GetProperty("mode").GetString());
        Assert.False(appliedResult.TryGetProperty("prune", out _));
        Assert.True(appliedResult.GetProperty("automatic").GetBoolean());
        Assert.Equal(
            previewResult.GetProperty("packages").GetRawText(),
            appliedResult.GetProperty("packages").GetRawText());
        Assert.Equal(EffectPlans(previewResult), EffectPlans(appliedResult));

        var targetEffect = Assert.Single(
            appliedResult.GetProperty("effects").EnumerateArray(),
            effect => effect.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal("delete", targetEffect.GetProperty("action").GetString());
        Assert.Equal("verified", targetEffect.GetProperty("outcome").GetString());
        Assert.Equal("toolkit", targetEffect.GetProperty("owner").GetString());
        Assert.Equal("retained", appliedRoot.GetProperty("recovery").GetProperty("disposition").GetString());
        Assert.True(File.Exists(appliedRoot.GetProperty("recovery").GetProperty("path").GetString()));
        Assert.Empty(appliedRoot.GetProperty("findings").EnumerateArray());
        Assert.Equal("open-forge cleanup  (after reviewing the bundle)", appliedRoot.GetProperty("next").GetProperty("command").GetString());
        Assert.False(File.Exists(working.TargetPath));
        Assert.Equal(unownedBytes, File.ReadAllBytes(unownedPath));
        Assert.Equal(sourceBefore, working.SnapshotSource());
        working.AssertPersistentLock();
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
                "--automatic", "--format=json",
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
        AssertOrder(root, "schemaVersion", "command", "status", "detail", "filter", "workspace", "summary", "findings", "effects", "counts", "limitations", "data", "recovery", "next");
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        if (mode == "dry-run")
        {
            Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        }
        else
        {
            Assert.Equal("open-forge cleanup  (after reviewing the bundle)", root.GetProperty("next").GetProperty("command").GetString());
        }
        var dataNames = new List<string>
        {
            "mode", "automatic", "packages", "orphaned", "permissions", "effects",
        };
        if (root.GetProperty("data").TryGetProperty("removalOrder", out _))
        {
            dataNames.Add("removalOrder");
        }
        AssertOrder(root.GetProperty("data"), [.. dataNames]);
        Assert.Equal(mode, root.GetProperty("data").GetProperty("mode").GetString());
    }

    private static void AssertChangedFinalOwnerDeletePlan(JsonElement result)
    {
        var effect = Assert.Single(
            result.GetProperty("effects").EnumerateArray(),
            item => item.GetProperty("path").GetString() == ".agents/toolkit/_toolkit.md");
        Assert.Equal("toolkit", effect.GetProperty("owner").GetString());
        Assert.Equal("delete", effect.GetProperty("action").GetString());
        Assert.Equal([], effect.GetProperty("keptFor").EnumerateArray().Select(value => value.GetString()));
    }

    private static string[] EffectPlans(JsonElement result)
        => [.. result.GetProperty("effects")
            .EnumerateArray()
            .Select(effect => string.Join(
                "\u001f",
                 effect.GetProperty("path").GetString() ?? string.Empty,
                 effect.GetProperty("owner").GetString() ?? string.Empty,
                 effect.GetProperty("action").GetString() ?? string.Empty,
                 effect.GetProperty("keptFor").GetRawText()))];

    private static void AssertOrder(JsonElement element, params string[] names)
        => Assert.Equal(names, element.EnumerateObject().Select(property => property.Name));
}
