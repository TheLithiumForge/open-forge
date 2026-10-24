using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionUpdateProcessTests
{
    private const string CustomRoot = "docs/forge";
    private const string UpdatedBody = "# Updated custom-root toolkit\n";

    [Theory(DisplayName = "Published Extension Update follows an authored custom root and repeats without byte changes")]
    [InlineData(PublishedTokens.Settings.RootFrontmatter), InlineData(PublishedTokens.Settings.ScopedFrontmatter)]
    [Trait("Feature", "extension-update"), Trait("Evidence", "EndToEnd")]
    public async Task CustomRootUpdateAndRepeat(string form)
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create(CustomRoot, form);
        var framework = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath,
            [PublishedTokens.Install.Command, PublishedTokens.Install.Automatic], working.EnvironmentVariables);
        Assert.True(framework.ExitCode == 0, framework.StandardOutput + framework.StandardError);
        var installed = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath,
            [PublishedTokens.Extension.Command, PublishedTokens.ExtensionInstall.Command, PublishedExtensionInstallWorkspace.ToolkitId,
                PublishedTokens.ExtensionInstall.Source, working.CataloguePath, PublishedTokens.ExtensionInstall.Automatic], working.EnvironmentVariables);
        Assert.True(installed.ExitCode == 0, installed.StandardOutput + installed.StandardError);
        var before = File.ReadAllBytes(working.TargetPath);
        var packagePath = Path.Combine(working.CataloguePath, PublishedExtensionInstallWorkspace.ToolkitId,
            PublishedTokens.ExtensionPackage.ContentDirectory, PublishedTokens.Sources.AgentsRoot, PublishedExtensionInstallWorkspace.RelativeTarget);
        File.WriteAllText(packagePath, OpenForgeDocumentSeed.Metadata(PublishedExtensionInstallWorkspace.ToolkitId, [], UpdatedBody));
        var sourceBytes = File.ReadAllBytes(packagePath);
        string[] arguments = [PublishedTokens.Extension.Command, PublishedTokens.ExtensionUpdate.Command, PublishedExtensionInstallWorkspace.ToolkitId,
            PublishedTokens.ExtensionUpdate.Source, working.CataloguePath, PublishedTokens.ExtensionUpdate.Automatic];
        var updated = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath, arguments, working.EnvironmentVariables);
        Assert.True(updated.ExitCode == 0, updated.StandardOutput + updated.StandardError);
        var after = File.ReadAllBytes(working.TargetPath);
        Assert.False(before.AsSpan().SequenceEqual(after));
        var expectedBody = Encoding.UTF8.GetBytes(UpdatedBody);
        Assert.True(after.Length >= expectedBody.Length);
        Assert.Equal(expectedBody, after[^expectedBody.Length..]);
        CommandOutputSnapshot.MatchSnapshot(Encoding.UTF8.GetString(after), form);
        Assert.False(File.Exists(Path.Combine(working.WorkspacePath, PublishedTokens.Sources.AgentsRoot, PublishedExtensionInstallWorkspace.RelativeTarget)));
        var ownershipPath = Path.Combine(working.WorkspacePath, PublishedTokens.Ownership.RelativePath);
        var ownership = File.ReadAllBytes(ownershipPath);
        var repeated = await PublishedProcessTestSupport.RunAsync(target, working.WorkspacePath, arguments, working.EnvironmentVariables);
        Assert.True(repeated.ExitCode == 0, repeated.StandardOutput + repeated.StandardError);
        Assert.Equal(after, File.ReadAllBytes(working.TargetPath));
        Assert.Equal(ownership, File.ReadAllBytes(ownershipPath));
        Assert.Equal(sourceBytes, File.ReadAllBytes(packagePath));
    }

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
            "open-forge extension update [<stable-id>...] [options]",
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
            "--automatic", "--format=json",
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
        Assert.Equal("completed", appliedDocument.RootElement.GetProperty("status").GetString());
        Assert.Empty(appliedDocument.RootElement.GetProperty("findings").EnumerateArray());
        Assert.True(File.Exists(working.TargetPath));
        Assert.Equal(updatedPayload, File.ReadAllText(working.TargetPath));
        Assert.False(beforeUpdate.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SequenceEqual(working.SnapshotWorkspace().OrderBy(pair => pair.Key, StringComparer.Ordinal)));
        Assert.Equal(sourceAfterEdit, working.SnapshotSource());

        var recoveryPath = appliedDocument.RootElement.GetProperty("recovery").GetProperty("path").GetString();
        var recoveryBytes = File.ReadAllBytes(Assert.IsType<string>(recoveryPath));
        var beforeRepeat = working.SnapshotWorkspace();
        var repeated = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);

        Assert.True(repeated.ExitCode == 0, repeated.StandardOutput + repeated.StandardError);
        Assert.Equal(string.Empty, repeated.StandardError);
        using var repeatedDocument = JsonDocument.Parse(repeated.StandardOutput);
        var repeatedResult = repeatedDocument.RootElement;
        Assert.Empty(repeatedResult.GetProperty("effects").EnumerateArray());
        Assert.Equal(
            "The toolkit Extension is up to date. Nothing to do.",
            repeatedResult.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Equal(sourceAfterEdit, working.SnapshotSource());
        Assert.Equal(beforeRepeat, working.SnapshotWorkspace());
        Assert.Equal(recoveryBytes, File.ReadAllBytes(recoveryPath));
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
        Assert.Contains("Cannot update toolkit:", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Status: blocked", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("extension-update.", result.StandardError, StringComparison.Ordinal);
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
            ["install", "--automatic", "--frontmatter", "scoped", "--workspace", working.WorkspacePath],
            working.EnvironmentVariables);
        Assert.Equal(0, frameworkSeeded.ExitCode);
        Assert.Equal(string.Empty, frameworkSeeded.StandardError);

        var seeded = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            [
                "extension", "install", "toolkit",
                "--source", working.CataloguePath,
                "--automatic", "--format=json",
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
