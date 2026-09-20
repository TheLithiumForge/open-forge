using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport.Filesystem;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F23RecoveryCleanupJourneyTests
{
    private const string Feature = "f23-recovery-cleanup-journey";
    private const string WorkspaceNotePath = "workspace-note.md";

    [Fact(DisplayName = "F23 C07-03 C07-02 C07-01 cleanup carries one real workspace through preview, removal, and no-op")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F23")]
    public async Task C07MainJourneyCarriesRealRecoveryStateThroughPreviewRemovalAndNoOp()
    {
        using var workspace = await CreateInstalledWorkspaceAsync("f23-main");
        using var fixture = RecoveryFixture.Create(workspace);
        var sourceBefore = CaptureSourceInventory(workspace);
        var previewBefore = CaptureFullInventory(workspace, fixture);

        var preview = await workspace.RunAsync("cleanup", "--dry-run");

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(string.Empty, preview.StandardError);
        Assert.Contains(
            "Would remove 2 recovery bundles and 1 unfinished draft.",
            preview.StandardOutput,
            StringComparison.Ordinal);
        Assert.Equal(previewBefore, CaptureFullInventory(workspace, fixture));
        Assert.Equal(sourceBefore, CaptureSourceInventory(workspace));
        Assert.True(File.Exists(fixture.FirstBundlePath));
        Assert.True(File.Exists(fixture.SecondBundlePath));
        Assert.True(File.Exists(fixture.DraftPath));

        var applied = await workspace.RunAsync("cleanup");

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(string.Empty, applied.StandardError);
        Assert.Contains(
            "Removed 2 recovery bundles and 1 unfinished draft.",
            applied.StandardOutput,
            StringComparison.Ordinal);
        Assert.False(File.Exists(fixture.FirstBundlePath));
        Assert.False(File.Exists(fixture.SecondBundlePath));
        Assert.False(File.Exists(fixture.DraftPath));
        Assert.Equal(0, CountFiles(fixture.SelectedRecoveryDirectory, ".zip"));
        Assert.Equal(0, CountFiles(fixture.SelectedRecoveryDirectory, ".draft"));
        Assert.True(File.Exists(fixture.UnknownPath));
        Assert.True(File.Exists(fixture.ForeignPath));
        Assert.Equal(sourceBefore, CaptureSourceInventory(workspace));

        var repeatBefore = CaptureFullInventory(workspace, fixture);
        var repeated = await workspace.RunAsync("cleanup");

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(string.Empty, repeated.StandardError);
        Assert.Contains("No recovery data to remove.", repeated.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(repeatBefore, CaptureFullInventory(workspace, fixture));
        Assert.Equal(sourceBefore, CaptureSourceInventory(workspace));
        Assert.True(File.Exists(fixture.UnknownPath));
        Assert.True(File.Exists(fixture.ForeignPath));
    }

    [Fact(DisplayName = "F23 C07-04 damaged selected-workspace candidate is retained while independent recovery items are removed")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F23")]
    public async Task C07DamagedCandidateDoesNotBlockIndependentRemoval()
    {
        using var workspace = await CreateInstalledWorkspaceAsync("f23-damaged");
        using var fixture = RecoveryFixture.Create(workspace, includeDamagedCandidate: true);
        var damagedBytes = File.ReadAllBytes(fixture.DamagedPath);

        var result = await workspace.RunAsync("cleanup");

        Assert.Equal(2, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("removed", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("left in place", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(Path.GetFileName(fixture.DamagedPath), result.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(fixture.FirstBundlePath));
        Assert.False(File.Exists(fixture.SecondBundlePath));
        Assert.False(File.Exists(fixture.DraftPath));
        Assert.True(File.Exists(fixture.DamagedPath));
        Assert.Equal(damagedBytes, File.ReadAllBytes(fixture.DamagedPath));
        Assert.True(File.Exists(fixture.UnknownPath));
        Assert.True(File.Exists(fixture.ForeignPath));
    }

    [Fact(DisplayName = "F23 C07-06 unreadable selected recovery store reports incomplete and deletes nothing")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F23")]
    public async Task C07UnreadableSelectedStorePreservesEveryRecoveryArtifact()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("C07-06 requires the Windows directory-enumeration ACL capability.");
            return;
        }

        using var workspace = await CreateInstalledWorkspaceAsync("f23-unreadable");
        using var fixture = RecoveryFixture.Create(workspace);
        var before = CaptureFullInventory(workspace, fixture);
        var ownedDataHomeRoot = workspace.LockStore.LocalApplicationDataDirectory;
        var selectedRecoveryDirectory = fixture.SelectedRecoveryDirectory;
        Assert.True(Path.IsPathFullyQualified(ownedDataHomeRoot));
        Assert.True(Path.IsPathFullyQualified(selectedRecoveryDirectory));
        var recoveryRelativePath = Path.GetRelativePath(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(ownedDataHomeRoot)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(selectedRecoveryDirectory)));
        Assert.False(
            Path.IsPathRooted(recoveryRelativePath)
            || recoveryRelativePath == ".."
            || recoveryRelativePath.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal));

        ProcessRunResult result;
        using (WindowsDirectoryEnumerationDenial.Create(
                   ownedDataHomeRoot,
                   selectedRecoveryDirectory))
        {
            result = await workspace.RunAsync("cleanup");
        }

        Assert.Equal(3, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.Contains("could not be read completely", result.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("Nothing was removed", result.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, CaptureFullInventory(workspace, fixture));
        Assert.True(File.Exists(fixture.FirstBundlePath));
        Assert.True(File.Exists(fixture.SecondBundlePath));
        Assert.True(File.Exists(fixture.DraftPath));
        Assert.True(File.Exists(fixture.UnknownPath));
        Assert.True(File.Exists(fixture.ForeignPath));
    }

    [Fact(DisplayName = "F23 C07-07 partial deletion reports progress, retains the held item, and retries from actual state")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F23")]
    public async Task C07PartialDeletionReportsProgressThenRetriesActualState()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("C07-07 requires Windows FileShare.Read deletion-denial semantics.");
        }

        using var workspace = await CreateInstalledWorkspaceAsync("f23-partial");
        using var fixture = RecoveryFixture.Create(workspace);
        var sourceBefore = CaptureSourceInventory(workspace);

        using (var held = new FileStream(
                   fixture.SecondBundlePath,
                   FileMode.Open,
                   FileAccess.Read,
                   FileShare.Read))
        {
            Assert.True(held.CanRead);
            Assert.NotEqual(-1, held.ReadByte());
            var partial = await workspace.RunAsync("cleanup");

            Assert.Equal(1, partial.ExitCode);
            Assert.Equal(string.Empty, partial.StandardOutput);
            Assert.Contains("Cleanup stopped after removing", partial.StandardError, StringComparison.Ordinal);
            Assert.False(File.Exists(fixture.FirstBundlePath));
            Assert.True(File.Exists(fixture.SecondBundlePath));
            Assert.True(File.Exists(fixture.DraftPath));
            Assert.True(File.Exists(fixture.UnknownPath));
            Assert.True(File.Exists(fixture.ForeignPath));
        }

        var retry = await workspace.RunAsync("cleanup");

        Assert.Equal(0, retry.ExitCode);
        Assert.Equal(string.Empty, retry.StandardError);
        Assert.Contains("Removed", retry.StandardOutput, StringComparison.Ordinal);
        Assert.False(File.Exists(fixture.SecondBundlePath));
        Assert.False(File.Exists(fixture.DraftPath));
        Assert.True(File.Exists(fixture.UnknownPath));
        Assert.True(File.Exists(fixture.ForeignPath));
        Assert.Equal(sourceBefore, CaptureSourceInventory(workspace));
        Assert.Equal(0, CountFiles(fixture.SelectedRecoveryDirectory, ".zip"));
        Assert.Equal(0, CountFiles(fixture.SelectedRecoveryDirectory, ".draft"));
    }

    private static async Task<PublishedJourneyWorkspace> CreateInstalledWorkspaceAsync(string purpose)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(WorkspaceNotePath);
            workspace.WriteText(WorkspaceNotePath, "Preserve this authored workspace file.\n");

            var install = await workspace.RunAsync("install", "--automatic");
            Assert.Equal(0, install.ExitCode);
            Assert.Equal(string.Empty, install.StandardError);
            Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
            workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static int CountFiles(string directory, string suffix)
        => Directory.Exists(directory)
            ? Directory.EnumerateFiles(directory, $"*{suffix}", SearchOption.TopDirectoryOnly).Count()
            : 0;

    private static IReadOnlyDictionary<string, string> CaptureSourceInventory(
        PublishedJourneyWorkspace workspace)
    {
        var inventory = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in workspace.SnapshotState())
        {
            inventory[$"workspace/{pair.Key}"] = pair.Value;
        }

        AddTree(
            inventory,
            "catalogue",
            Path.Combine(
                workspace.LockStore.LocalApplicationDataDirectory,
                "OpenForge",
                "locks"));
        return inventory;
    }

    private static IReadOnlyDictionary<string, string> CaptureFullInventory(
        PublishedJourneyWorkspace workspace,
        RecoveryFixture fixture)
    {
        var inventory = new SortedDictionary<string, string>(
            CaptureSourceInventory(workspace).ToDictionary(pair => pair.Key, pair => pair.Value),
            StringComparer.Ordinal);
        AddTree(inventory, "recovery-selected", fixture.SelectedRecoveryDirectory);
        AddTree(inventory, "recovery-foreign", fixture.ForeignRecoveryDirectory);
        return inventory;
    }

    private static void AddTree(IDictionary<string, string> inventory, string prefix, string root)
    {
        if (!Directory.Exists(root))
        {
            inventory[$"{prefix}/<root>"] = "absent";
            return;
        }

        foreach (var pair in PublishedWorkspaceTreeSnapshot.Capture(root))
        {
            inventory[$"{prefix}/{pair.Key}"] = pair.Value;
        }
    }

    private sealed class RecoveryFixture : IDisposable
    {
        private const string FirstBundleName = "operation-11111111111111111111111111111111.zip";
        private const string SecondBundleName = "operation-22222222222222222222222222222222.zip";
        private const string DraftName = "operation-33333333333333333333333333333333.draft";
        private const string DamagedName = "operation-44444444444444444444444444444444.zip";
        private const string UnknownName = "unknown-support.txt";
        private const string ForeignName = "operation-55555555555555555555555555555555.zip";
        private const string NestedDirectoryName = "unknown-support";
        private const string NestedSentinelName = "sentinel.txt";

        private readonly string _nestedDirectory;
        private bool _disposed;

        private RecoveryFixture(
            string selectedRecoveryDirectory,
            string foreignRecoveryDirectory)
        {
            SelectedRecoveryDirectory = selectedRecoveryDirectory;
            ForeignRecoveryDirectory = foreignRecoveryDirectory;
            FirstBundlePath = Path.Combine(selectedRecoveryDirectory, FirstBundleName);
            SecondBundlePath = Path.Combine(selectedRecoveryDirectory, SecondBundleName);
            DraftPath = Path.Combine(selectedRecoveryDirectory, DraftName);
            DamagedPath = Path.Combine(selectedRecoveryDirectory, DamagedName);
            UnknownPath = Path.Combine(selectedRecoveryDirectory, UnknownName);
            ForeignPath = Path.Combine(foreignRecoveryDirectory, ForeignName);
            _nestedDirectory = Path.Combine(selectedRecoveryDirectory, NestedDirectoryName);
        }

        internal string SelectedRecoveryDirectory { get; }

        internal string ForeignRecoveryDirectory { get; }

        internal string FirstBundlePath { get; }

        internal string SecondBundlePath { get; }

        internal string DraftPath { get; }

        internal string DamagedPath { get; }

        internal string UnknownPath { get; }

        internal string ForeignPath { get; }

        internal static RecoveryFixture Create(
            PublishedJourneyWorkspace workspace,
            bool includeDamagedCandidate = false)
        {
            var selected = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
            var foreign = Path.Combine(
                workspace.LockStore.RecoveryStoreRoot,
                Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes("foreign-workspace"))));
            var fixture = new RecoveryFixture(selected, foreign);
            try
            {
                Directory.CreateDirectory(selected);
                Directory.CreateDirectory(foreign);
                Directory.CreateDirectory(fixture._nestedDirectory);
                WriteFinal(fixture.FirstBundlePath, workspace.Path, Guid.ParseExact("11111111111111111111111111111111", "N"), "cleanup-target-one.md");
                WriteFinal(fixture.SecondBundlePath, workspace.Path, Guid.ParseExact("22222222222222222222222222222222", "N"), "cleanup-target-two.md");
                File.WriteAllBytes(fixture.DraftPath, Encoding.UTF8.GetBytes("incomplete cleanup draft\n"));
                File.WriteAllBytes(fixture.UnknownPath, Encoding.UTF8.GetBytes("preserve unknown support\n"));
                File.WriteAllBytes(
                    Path.Combine(fixture._nestedDirectory, NestedSentinelName),
                    Encoding.UTF8.GetBytes("nested support sentinel\n"));
                File.WriteAllBytes(fixture.ForeignPath, Encoding.UTF8.GetBytes("foreign malformed archive\n"));
                if (includeDamagedCandidate)
                {
                    File.WriteAllBytes(fixture.DamagedPath, Encoding.UTF8.GetBytes("not a zip archive\n"));
                }

                return fixture;
            }
            catch
            {
                fixture.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            DeleteFileIfPresent(FirstBundlePath);
            DeleteFileIfPresent(SecondBundlePath);
            DeleteFileIfPresent(DraftPath);
            DeleteFileIfPresent(DamagedPath);
            DeleteFileIfPresent(UnknownPath);
            DeleteFileIfPresent(ForeignPath);
            DeleteFileIfPresent(Path.Combine(_nestedDirectory, NestedSentinelName));
            DeleteEmptyDirectoryIfPresent(_nestedDirectory);
            DeleteEmptyDirectoryIfPresent(SelectedRecoveryDirectory);
            DeleteEmptyDirectoryIfPresent(ForeignRecoveryDirectory);
            _disposed = true;
        }

        private static void WriteFinal(
            string path,
            string workspacePath,
            Guid operationId,
            string logicalPath)
        {
            var priorBytes = Encoding.UTF8.GetBytes($"previous {logicalPath} bytes\n");
            var workspaceKey = PublishedWorkspaceLockStore.RecoveryWorkspaceKey(workspacePath);
            var manifest = $$"""
                {
                  "schemaVersion": 1,
                  "command": "route move",
                  "operationId": "{{operationId:N}}",
                  "workspacePath": "{{JsonEncodedText.Encode(workspacePath)}}",
                  "workspaceKey": "{{workspaceKey}}",
                  "attribution": {
                    "producer": "route",
                    "operation": "move",
                    "subject": {
                      "kind": "workspace",
                      "identity": "{{workspaceKey}}"
                    }
                  },
                  "entries": [{
                    "ordinal": 0,
                    "logicalPath": "{{logicalPath}}",
                    "kind": "ordinary-delete",
                    "prior": {
                      "kind": "ordinary-file",
                      "length": {{priorBytes.Length}},
                      "sha256": "{{Hash(priorBytes)}}",
                      "linkKind": null,
                      "rawRelativeTarget": null
                    },
                    "intended": {
                      "kind": "missing",
                      "length": null,
                      "sha256": null,
                      "linkKind": null,
                      "rawRelativeTarget": null
                    },
                    "priorPayload": "payloads/00000000.bin"
                  }]
                }
                """;
            using var stream = new FileStream(path, FileMode.CreateNew, FileAccess.Write, FileShare.None);
            using var archive = new ZipArchive(stream, ZipArchiveMode.Create, leaveOpen: false);
            var manifestEntry = archive.CreateEntry("manifest.json", CompressionLevel.NoCompression);
            using (var manifestStream = manifestEntry.Open())
            {
                var manifestBytes = Encoding.UTF8.GetBytes(manifest);
                manifestStream.Write(manifestBytes);
            }

            var payloadEntry = archive.CreateEntry("payloads/00000000.bin", CompressionLevel.NoCompression);
            using var payloadStream = payloadEntry.Open();
            payloadStream.Write(priorBytes);
        }

        private static string Hash(byte[] bytes)
            => Convert.ToHexStringLower(SHA256.HashData(bytes));

        private static void DeleteFileIfPresent(string path)
        {
            if (!File.Exists(path))
            {
                return;
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                throw new InvalidOperationException($"The F23 recovery fixture file is not ordinary: {path}");
            }

            File.Delete(path);
        }

        private static void DeleteEmptyDirectoryIfPresent(string path)
        {
            if (!Directory.Exists(path))
            {
                return;
            }

            var attributes = File.GetAttributes(path);
            if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device))
                != FileAttributes.Directory)
            {
                throw new InvalidOperationException($"The F23 recovery fixture directory is not ordinary: {path}");
            }

            if (!Directory.EnumerateFileSystemEntries(path).Any())
            {
                Directory.Delete(path);
            }
        }
    }
}
