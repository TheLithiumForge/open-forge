using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionUpdateProcessTests
{
    [Fact(DisplayName = "Published Extension Update help is reachable without workspace inspection or writes"), Trait("Feature", "extension-update"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedHelpIsReachableWithoutWorkspaceInspectionOrWrites()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-extension-update-help");
        using var lockStore = PublishedWorkspaceLockStore.Create("e2e-extension-update-help-locks");
        _ = lockStore.Track(working.Path);

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.Path,
            working.SnapshotHashes,
            ["extension", "update", "--help"],
            lockStore.EnvironmentVariables);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains(
            "open-forge extension update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune] [--automatic] [--dry-run] [global options]",
            string.Join(" ", result.StandardOutput.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)),
            StringComparison.Ordinal);
        lockStore.AssertNoInfrastructure();
    }

    [Fact(DisplayName = "Published Extension Update applies reviewed bytes and repeats as an exact no-op"), Trait("Feature", "extension-update"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedUpdateAppliesReviewedBytesAndRepeatsAsNoOp()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        await SeedExtensionAsync(target, working);
        var beforeUpdate = working.SnapshotWorkspace();

        var sourcePayloadPath = Path.Combine(
            working.CataloguePath,
            "toolkit",
            "content",
            ".agents",
            "toolkit",
            "_toolkit.md");
        var updatedPayload = OpenForgeDocumentSeed.Metadata(
            "toolkit",
            ["Extension"],
            "# toolkit updated\n");
        File.WriteAllText(sourcePayloadPath, updatedPayload);
        var sourceAfterEdit = working.SnapshotSource();
        var arguments = new[]
        {
            "extension", "update", "toolkit",
            "--source", working.CataloguePath,
            "--automatic", "--json",
            "--workspace", working.WorkspacePath,
        };

        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var appliedDocument = JsonDocument.Parse(applied.StandardOutput);
        Assert.Equal("extension update", appliedDocument.RootElement.GetProperty("command").GetString());
        Assert.Equal("complete", appliedDocument.RootElement.GetProperty("status").GetString());
        Assert.Empty(appliedDocument.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray());
        Assert.True(File.Exists(working.TargetPath));
        Assert.Equal(updatedPayload, File.ReadAllText(working.TargetPath));
        Assert.False(beforeUpdate.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SequenceEqual(working.SnapshotWorkspace().OrderBy(pair => pair.Key, StringComparer.Ordinal)));
        Assert.Equal(sourceAfterEdit, working.SnapshotSource());

        var repeated = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var repeatedDocument = JsonDocument.Parse(repeated.StandardOutput);
        var repeatedResult = repeatedDocument.RootElement.GetProperty("result");
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", repeatedResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("already-current", repeatedResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal(sourceAfterEdit, working.SnapshotSource());
        working.AssertPersistentLock();
    }

    [Fact(DisplayName = "Published Extension Update blocks a workspace-overlapping source without effects"), Trait("Feature", "extension-update"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedUpdateBlocksWorkspaceOverlappingSourceWithoutEffects()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        await SeedExtensionAsync(target, working);
        var beforeWorkspace = working.SnapshotWorkspace();
        var beforeSource = working.SnapshotSource();

        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.WorkspacePath,
            () => SnapshotWorkspaceAndSource(working),
            [
                "extension", "update", "toolkit",
                "--source", working.WorkspacePath,
                "--automatic",
                "--workspace", working.WorkspacePath,
            ],
            working.EnvironmentVariables);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("extension-update.", result.StandardError, StringComparison.Ordinal);
        Assert.Equal(beforeWorkspace, working.SnapshotWorkspace());
        Assert.Equal(beforeSource, working.SnapshotSource());
        working.AssertPersistentLock();
        working.AssertNoRecoveryArtifacts();
    }

    private static async Task SeedExtensionAsync(
        PublishedExecutableTarget target,
        PublishedExtensionInstallWorkspace working)
    {
        var frameworkSeeded = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            ["install", "--automatic", "--workspace", working.WorkspacePath],
            working.EnvironmentVariables);
        Assert.Equal(0, frameworkSeeded.ExitCode);
        Assert.Equal(string.Empty, frameworkSeeded.StandardError);

        var seeded = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            [
                "extension", "install", "toolkit",
                "--source", working.CataloguePath,
                "--automatic", "--json",
                "--workspace", working.WorkspacePath,
            ],
            working.EnvironmentVariables);

        Assert.Equal(0, seeded.ExitCode);
        Assert.Equal(string.Empty, seeded.StandardError);
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
}
