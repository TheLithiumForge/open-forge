using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedExtensionInstallProcessTests
{
    [Fact(DisplayName = "Published Extension Install help is read-only"), Trait("Feature", "extension-install"), Trait("Evidence", "EndToEnd")]
    public static async Task PublishedHelpIsTruthfulAndReadOnly()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = TemporaryWorkspace.Create("e2e-extension-install-help");
        var result = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target, working.Path, working.SnapshotHashes, ["extension", "install", "--help"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("open-forge extension install", result.StandardOutput, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Published Extension Install requires explicit unattended selection"), Trait("Feature", "extension-install"), Trait("Evidence", "EndToEnd")]
    public static async Task PublishedUnattendedSelectionIsRequired()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        var invalid = await PublishedProcessTestSupport.RunWithoutWritesAsync(
            target,
            working.WorkspacePath,
            working.SnapshotWorkspace,
        [
            "extension", "install",
            "--source", working.CataloguePath,
            "--workspace", working.WorkspacePath,
        ],
            working.EnvironmentVariables);
        Assert.Equal(4, invalid.ExitCode);
        Assert.Equal(string.Empty, invalid.StandardOutput);
        Assert.Contains("Status: invalid", invalid.StandardError, StringComparison.Ordinal);
        Assert.Contains("extension-install.selection-required", invalid.StandardError, StringComparison.Ordinal);

    }

    [Fact(DisplayName = "Published Extension Install applies and repeats without changes"),
     Trait("Feature", "extension-install"), Trait("Evidence", "EndToEnd")]
    public async Task PublishedApplyAndNoOpUseExactStreamsExitsAndJson()
    {
        var target = PublishedExecutableTarget.Discover();
        using var working = PublishedExtensionInstallWorkspace.Create();
        var seeded = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            ["install", "--automatic", "--workspace", working.WorkspacePath],
            working.EnvironmentVariables);
        Assert.Equal(0, seeded.ExitCode);
        Assert.Equal(string.Empty, seeded.StandardError);
        var afterFramework = working.SnapshotWorkspace();

        var arguments = new[]
        {
            "extension", "install", "toolkit",
            "--source", working.CataloguePath,
            "--workspace", working.WorkspacePath,
            "--automatic", "--json",
        };
        var applied = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        using var document = JsonDocument.Parse(applied.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("extension install", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal("explicit-workspace", root.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(JsonValueKind.Null, root.GetProperty("next").ValueKind);
        var commandResult = root.GetProperty("result");
        Assert.Equal(
        [
            "mode", "force", "automatic", "selection", "source", "packages", "framework",
            "footprint", "effects", "generatedNavigation", "permissions", "lifecycle", "recovery", "verification", "findings",
        ],
            commandResult.EnumerateObject().Select(property => property.Name));
        Assert.Equal("explicit-ids", commandResult.GetProperty("selection").GetProperty("selectedBy").GetString());
        Assert.Equal("verified", commandResult.GetProperty("verification").GetProperty("targets").GetString());
        Assert.Equal("verified", commandResult.GetProperty("verification").GetProperty("topology").GetString());
        Assert.Equal("verified", commandResult.GetProperty("verification").GetProperty("extensionsLifecycle").GetString());
        Assert.Equal("verified", commandResult.GetProperty("verification").GetProperty("frameworkLifecycle").GetString());
        Assert.Empty(commandResult.GetProperty("findings").EnumerateArray());
        Assert.True(File.Exists(working.TargetPath));
        Assert.Equal(working.SourcePayloadBytes(), File.ReadAllBytes(working.TargetPath));
        Assert.Equal(working.SourceSnapshot, working.SnapshotSource());

        var afterApply = working.SnapshotWorkspace();
        Assert.False(afterFramework.OrderBy(pair => pair.Key, StringComparer.Ordinal)
            .SequenceEqual(afterApply.OrderBy(pair => pair.Key, StringComparer.Ordinal)));
        var noOp = await PublishedProcessTestSupport.RunAsync(
            target,
            working.WorkspacePath,
            arguments,
            working.EnvironmentVariables);
        Assert.Equal(0, noOp.ExitCode);
        Assert.Equal(string.Empty, noOp.StandardError);
        using var noOpDocument = JsonDocument.Parse(noOp.StandardOutput);
        var noOpResult = noOpDocument.RootElement.GetProperty("result");
        Assert.Empty(noOpResult.GetProperty("effects").EnumerateArray());
        Assert.Equal("preserve", noOpResult.GetProperty("lifecycle").GetProperty("action").GetString());
        Assert.Equal("already-current", noOpResult.GetProperty("lifecycle").GetProperty("outcome").GetString());
        Assert.Equal(afterApply, working.SnapshotWorkspace());
        Assert.Equal(working.SourceSnapshot, working.SnapshotSource());
        working.AssertPersistentLock();
    }


}

internal sealed class PublishedExtensionInstallWorkspace : IDisposable
{
    private readonly TemporaryWorkspace _workspace;
    private readonly TemporaryWorkspace _source;
    private readonly PublishedWorkspaceLockStore _lockStore;

    private PublishedExtensionInstallWorkspace(
        TemporaryWorkspace workspace,
        TemporaryWorkspace source,
        PublishedWorkspaceLockStore lockStore)
    {
        _workspace = workspace;
        _source = source;
        _lockStore = lockStore;
        _ = _lockStore.Track(workspace.Path);
        SourceSnapshot = source.SnapshotHashes();
    }

    internal string WorkspacePath => _workspace.Path;

    internal string CataloguePath => _source.Path;

    internal string TargetPath => _workspace.Combine(".agents/toolkit/_toolkit.md");

    internal IReadOnlyDictionary<string, string> EnvironmentVariables => _lockStore.EnvironmentVariables;

    internal IReadOnlyDictionary<string, string> SourceSnapshot { get; }

    internal static PublishedExtensionInstallWorkspace Create()
    {
        var workspace = TemporaryWorkspace.Create("e2e-extension-install-workspace");
        var source = TemporaryWorkspace.Create("e2e-extension-install-source");
        var lockStore = PublishedWorkspaceLockStore.Create("e2e-extension-install-locks");
        try
        {
            workspace.CreateFile("workspace-note.md", "preserve workspace content\n");
            WritePackage(source, "alpha", ".agents/alpha/_alpha.md");
            WritePackage(source, "toolkit", ".agents/toolkit/_toolkit.md");
            return new PublishedExtensionInstallWorkspace(workspace, source, lockStore);
        }
        catch
        {
            lockStore.Dispose();
            source.Dispose();
            workspace.Dispose();
            throw;
        }
    }

    internal IReadOnlyDictionary<string, string> SnapshotWorkspace() => _workspace.SnapshotHashes();

    internal IReadOnlyDictionary<string, string> SnapshotSource() => _source.SnapshotHashes();

    internal byte[] SourcePayloadBytes()
        => File.ReadAllBytes(_source.Combine("toolkit/content/.agents/toolkit/_toolkit.md"));

    internal void AssertPersistentLock() => _lockStore.AssertPersistentZeroByteLock(WorkspacePath);

    internal void AssertNoRecoveryArtifacts() => _lockStore.AssertNoRecoveryArtifacts(WorkspacePath);

    public void Dispose()
    {
        _ = _lockStore.RemoveRecoveryArtifacts(WorkspacePath);
        DeleteOrdinaryFile(_workspace.Combine("AGENTS.md"));
        DeleteOrdinaryFile(_workspace.Combine("CLAUDE.md"));
        DeleteOrdinaryTree(_workspace.Combine(".agents"));
        _lockStore.Dispose();
        _source.Dispose();
        _workspace.Dispose();
    }

    private static void WritePackage(TemporaryWorkspace source, string id, string target)
    {
        source.WriteText(
            $"{id}/extension.json",
            $$"""
              {
                "id": "{{id}}",
                "name": "{{id}}",
                "description": "Extension package {{id}}.",
                "version": "1.0.0",
                "dependencies": []
              }
              """);
        source.WriteText(
            $"{id}/content/{target}",
            OpenForgeDocumentSeed.Metadata(id, ["Extension"], $"# {id}\n"));
    }

    private static void DeleteOrdinaryTree(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var entry in Directory.EnumerateFileSystemEntries(path, "*", SearchOption.AllDirectories))
        {
            var attributes = File.GetAttributes(entry);
            if ((attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException("The published Extension Install cleanup tree contains a non-ordinary entry.");
            }
        }

        Directory.Delete(path, recursive: true);
    }

    private static void DeleteOrdinaryFile(string path)
    {
        if (!File.Exists(path))
        {
            return;
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The published Extension Install cleanup target is not an ordinary file.");
        }

        File.Delete(path);
    }
}
