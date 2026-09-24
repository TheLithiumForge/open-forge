using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Recovery;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Recovery;

namespace OpenForge.Cli.IntegrationTests.Commands.Remove;

public sealed class RemoveRootPathIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove ordinary file dry-run, apply, and repeat preserve the exact exclusion contract")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task OrdinaryFileDryRunApplyAndRepeat()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-file-golden");
        var original = new byte[] { 0, 255, 13, 10, 1, 128, 0 };
        workspace.WriteBytes("scratch.bin", original);
        var before = workspace.SnapshotHashes();

        var previewOutput = new StringWriter();
        var previewError = new StringWriter();
        var preview = await workspace.RunAsync(
            ["remove", "./scratch.bin", "--dry-run", "--format", "json"],
            previewOutput,
            previewError);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(string.Empty, previewError.ToString());
        using (var document = JsonDocument.Parse(previewOutput.ToString()))
        {
            Assert.Equal("remove", document.RootElement.GetProperty("command").GetString());
            Assert.Equal("dry-run", document.RootElement.GetProperty("data").GetProperty("mode").GetString());
            Assert.Equal("scratch.bin", document.RootElement.GetProperty("data").GetProperty("target").GetString());
            var effects = document.RootElement.GetProperty("effects").EnumerateArray();
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents"
                && effect.GetProperty("kind").GetString() == "directory"
                && effect.GetProperty("action").GetString() == "created"
                && effect.GetProperty("outcome").GetString() == "planned");
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == WorkspaceSettingsDefinitions.RelativePath
                && effect.GetProperty("kind").GetString() == "setting"
                && effect.GetProperty("action").GetString() == "created"
                && effect.GetProperty("outcome").GetString() == "planned");
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == "scratch.bin"
                && effect.GetProperty("kind").GetString() == "file"
                && effect.GetProperty("action").GetString() == "deleted"
                && effect.GetProperty("outcome").GetString() == "planned");
        }
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));

        var applyOutput = new StringWriter();
        var applyError = new StringWriter();
        var applied = await workspace.RunAsync(
            ["remove", "scratch.bin", "--automatic", "--format", "json"],
            applyOutput,
            applyError);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(string.Empty, applyError.ToString());
        Assert.False(File.Exists(workspace.Combine("scratch.bin")));
        using (var report = JsonDocument.Parse(applyOutput.ToString()))
        {
            var effects = report.RootElement.GetProperty("effects").EnumerateArray();
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == ".agents"
                && effect.GetProperty("kind").GetString() == "directory"
                && effect.GetProperty("action").GetString() == "created"
                && effect.GetProperty("outcome").GetString() == "done");
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == WorkspaceSettingsDefinitions.RelativePath
                && effect.GetProperty("kind").GetString() == "setting"
                && effect.GetProperty("action").GetString() == "created"
                && effect.GetProperty("outcome").GetString() == "done");
            Assert.Equal(1, report.RootElement.GetProperty("counts").GetProperty("removed").GetInt32());
        }
        var recoveryPath = ReadRecoveryPath(applyOutput.ToString());
        Assert.True(File.Exists(recoveryPath));
        var finalRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            recoveryPath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, finalRead.State);
        var recovery = Assert.IsType<RecoveryBundleVerifiedRead>(finalRead.Verified);
        var targetEntry = Assert.Single(recovery.Entries, entry => entry.TargetPath == "scratch.bin");
        Assert.Equal(RecoveryEntryKind.OrdinaryDelete, targetEntry.Kind);
        Assert.Equal(Convert.ToHexString(SHA256.HashData(original)).ToLowerInvariant(),
            targetEntry.Prior.OrdinaryFile?.Sha256);
        using (var archive = ZipFile.OpenRead(recoveryPath))
        {
            var payload = Assert.Single(archive.Entries, entry => entry.FullName == targetEntry.PriorPayload);
            using var stream = payload.Open();
            using var bytes = new MemoryStream();
            await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
            Assert.Equal(original, bytes.ToArray());
        }

        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            Assert.Equal(["scratch.bin"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
                .Select(path => path.GetString()).ToArray());
        }

        var afterApply = workspace.SnapshotHashes();
        var repeatOutput = new StringWriter();
        var repeatError = new StringWriter();
        var repeated = await workspace.RunAsync(
            ["remove", "scratch.bin", "--automatic", "--format", "json"],
            repeatOutput,
            repeatError);
        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Equal(string.Empty, repeatError.ToString());
        using (var document = JsonDocument.Parse(repeatOutput.ToString()))
        {
            Assert.Equal("remove", document.RootElement.GetProperty("command").GetString());
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            Assert.Empty(document.RootElement.GetProperty("counts").EnumerateObject());
        }
        Assert.Equal(afterApply, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove reports settings replacement in preview and apply when settings already exist")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task ExistingSettingsRemovalReportsReplacement()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-existing-settings");
        workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{\"schemaVersion\":1,\"removedFiles\":[]}");
        workspace.WriteText("old.txt", "Existing file.\n");

        var previewOutput = new StringWriter();
        var previewError = new StringWriter();
        var preview = await workspace.RunAsync(
            ["remove", "old.txt", "--dry-run", "--automatic", "--format", "json"],
            previewOutput,
            previewError);

        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, preview.Status);
        Assert.Equal(string.Empty, previewError.ToString());
        using (var report = JsonDocument.Parse(previewOutput.ToString()))
        {
            var effects = report.RootElement.GetProperty("effects").EnumerateArray();
            Assert.Contains(effects, effect => effect.GetProperty("path").GetString() == WorkspaceSettingsDefinitions.RelativePath
                && effect.GetProperty("kind").GetString() == "setting"
                && effect.GetProperty("action").GetString() == "replaced"
                && effect.GetProperty("outcome").GetString() == "planned");
            Assert.DoesNotContain(effects, effect => effect.GetProperty("path").GetString() == ".agents"
                && effect.GetProperty("kind").GetString() == "directory");
        }

        var applyOutput = new StringWriter();
        var applyError = new StringWriter();
        var applied = await workspace.RunAsync(
            ["remove", "old.txt", "--automatic", "--format", "json"],
            applyOutput,
            applyError);

        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.Equal(string.Empty, applyError.ToString());
        Assert.False(File.Exists(workspace.Combine("old.txt")));
        using var appliedReport = JsonDocument.Parse(applyOutput.ToString());
        Assert.Contains(appliedReport.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == WorkspaceSettingsDefinitions.RelativePath
            && effect.GetProperty("kind").GetString() == "setting"
            && effect.GetProperty("action").GetString() == "replaced"
            && effect.GetProperty("outcome").GetString() == "done");
        Assert.Equal(1, appliedReport.RootElement.GetProperty("counts").GetProperty("removed").GetInt32());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove records a future directory exclusion and removes every binary child")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task DirectoryRemovalRecordsFutureExclusionAndDeletesBinaryAssets()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-directory");
        var binary = new byte[] { 255, 0, 1, 13, 10, 128 };
        workspace.WriteBytes("scratch/assets/data.bin", binary);
        workspace.WriteText("scratch/readme.md", "Keep exact file inventory.\n");

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", "scratch", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.False(Directory.Exists(workspace.Combine("scratch")));
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            Assert.Equal(["scratch"], settings.RootElement.GetProperty("removedDirectories").EnumerateArray()
                .Select(path => path.GetString()).ToArray());
            Assert.False(settings.RootElement.TryGetProperty("removedFiles", out _));
        }

        Directory.CreateDirectory(workspace.Combine("scratch/future"));
        File.WriteAllText(workspace.Combine("scratch/future/child.md"), "Created after removal.\n");
        Assert.True(File.Exists(workspace.Combine("scratch/future/child.md")));
        File.Delete(workspace.Combine("scratch/future/child.md"));
        Directory.Delete(workspace.Combine("scratch/future"));
        Directory.Delete(workspace.Combine("scratch"));
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal(2, report.RootElement.GetProperty("counts").GetProperty("removed").GetInt32());
        Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == "scratch/assets/data.bin"
            && effect.GetProperty("kind").GetString() == "file");
        var recoveryPath = ReadRecoveryPath(output.ToString());
        var finalRead = await RecoveryBundleReader.ReadFinalAsync(
            workspace.Workspace,
            recoveryPath,
            TestContext.Current.CancellationToken);
        Assert.Equal(RecoveryBundleReadState.Valid, finalRead.State);
        var recovery = Assert.IsType<RecoveryBundleVerifiedRead>(finalRead.Verified);
        var binaryEntry = Assert.Single(recovery.Entries, entry => entry.TargetPath == "scratch/assets/data.bin");
        Assert.Equal(RecoveryEntryKind.OrdinaryDelete, binaryEntry.Kind);
        using (var archive = ZipFile.OpenRead(recoveryPath))
        {
            var payload = Assert.Single(archive.Entries, entry => entry.FullName == binaryEntry.PriorPayload);
            using var stream = payload.Open();
            using var bytes = new MemoryStream();
            await stream.CopyToAsync(bytes, TestContext.Current.CancellationToken);
            Assert.Equal(binary, bytes.ToArray());
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove dry-run does not initialize an uninitialized workspace")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task UninitializedWorkspaceDryRunIsWriteFree()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-uninitialized");
        workspace.WriteText("notes.txt", "Plain workspace.\n");
        var before = workspace.SnapshotHashes();
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", "notes.txt", "--dry-run", "--automatic"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.False(Directory.Exists(workspace.Combine(".agents")));
        Assert.False(Directory.Exists(workspace.RecoveryDirectory()));

        var applyOutput = new StringWriter();
        var applyError = new StringWriter();
        var applied = await workspace.RunAsync(
            ["remove", "notes.txt", "--automatic", "--format", "json"],
            applyOutput,
            applyError);
        Assert.Equal(0, applied.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, applied.Status);
        Assert.False(File.Exists(workspace.Combine("notes.txt")));
        Assert.True(Directory.Exists(workspace.Combine(".agents")));
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(["notes.txt"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
            .Select(path => path.GetString()).ToArray());
        Assert.Equal(string.Empty, applyError.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove can record one exact missing file exclusion")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingFileExclusionIsExact()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-file");
        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", "missing/one.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        using var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.Equal(["missing/one.md"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
            .Select(path => path.GetString()).ToArray());
        Assert.False(settings.RootElement.TryGetProperty("removedDirectories", out _));
        Assert.False(Directory.Exists(workspace.Combine("missing")));
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove releases a stale claim for an exact missing file even without prior exclusion")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingOwnedFileRecordsExclusionAndReleasesItsClaim()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-owned-file");
        workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), [".agents/loader.md"], []),
            }));
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", ".agents/loader.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("remove", report.RootElement.GetProperty("command").GetString());
        Assert.Equal("Reconciled removal of .agents/loader.md", report.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Empty(report.RootElement.GetProperty("findings").EnumerateArray());
        Assert.Empty(report.RootElement.GetProperty("data").GetProperty("removed").EnumerateArray());
        Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == ".agents/open-forge.lock.json"
            && effect.GetProperty("action").GetString() == "released"
            && effect.GetProperty("outcome").GetString() == "done");

        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            Assert.Equal([".agents/loader.md"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
                .Select(path => path.GetString()).ToArray());
        }
        using (var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json"))))
        {
            Assert.Empty(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray());
        }
        var recoveryPath = ReadRecoveryPath(output.ToString());
        Assert.True(File.Exists(recoveryPath));

        var repeatOutput = new StringWriter();
        var repeatError = new StringWriter();
        var repeated = await workspace.RunAsync(
            ["remove", ".agents/loader.md", "--automatic", "--format", "json"],
            repeatOutput,
            repeatError);

        Assert.Equal(0, repeated.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, repeated.Status);
        Assert.Equal(string.Empty, repeatError.ToString());
        using var repeatReport = JsonDocument.Parse(repeatOutput.ToString());
        Assert.Equal("No changes for .agents/loader.md", repeatReport.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Empty(repeatReport.RootElement.GetProperty("effects").EnumerateArray());
        Assert.True(File.Exists(recoveryPath));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove repairs an interrupted missing-and-excluded file before reporting no changes")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingExcludedOwnedFileReleasesItsStaleClaim()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-excluded-owned-file");
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"schemaVersion\":1,\"removedFiles\":[\".agents/loader.md\"]}");
        workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), [".agents/loader.md"], []),
            }));

        var output = new StringWriter();
        var error = new StringWriter();
        var completion = await workspace.RunAsync(
            ["remove", ".agents/loader.md", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        Assert.Equal(string.Empty, error.ToString());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("Reconciled removal of .agents/loader.md", report.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Contains(report.RootElement.GetProperty("effects").EnumerateArray(), effect =>
            effect.GetProperty("path").GetString() == ".agents/open-forge.lock.json"
            && effect.GetProperty("action").GetString() == "released"
            && effect.GetProperty("outcome").GetString() == "done");
        using var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json")));
        Assert.Empty(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove can retry a missing directory using its recorded directory intent")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingRecordedDirectoryReleasesDescendantClaims()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-directory-intent");
        workspace.WriteText(
            ".agents/open-forge.json",
            "{\"schemaVersion\":1,\"removedDirectories\":[\"gone\"]}");
        workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), ["gone/owned.md"], []),
            }));
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", "gone", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        Assert.Equal(CliSemanticStatus.Complete, completion.Status);
        using var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json")));
        Assert.Empty(ownership.RootElement.GetProperty("framework").GetProperty("paths").EnumerateArray());
        using var report = JsonDocument.Parse(output.ToString());
        Assert.Equal("Reconciled removal of gone", report.RootElement.GetProperty("summary").GetProperty("headline").GetString());
        Assert.Empty(report.RootElement.GetProperty("data").GetProperty("removed").EnumerateArray());
        Assert.Equal(string.Empty, error.ToString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Root Remove does not infer directory scope from an arbitrary missing file target")]
    [Trait("Feature", "remove-root"), Trait("Evidence", "Integration")]
    public async Task MissingFileTargetDoesNotReleaseDescendantClaims()
    {
        using var workspace = RemoveRootIntegrationWorkspace.Create("remove-root-missing-file-scope");
        workspace.WriteBytes(".agents/open-forge.lock.json", WorkspaceOwnershipCodec.Write(
            WorkspaceOwnershipDocument.Empty with
            {
                Framework = new(new("framework", null), ["missing/dir/owned.md"], []),
            }));
        var output = new StringWriter();
        var error = new StringWriter();

        var completion = await workspace.RunAsync(
            ["remove", "missing/dir", "--automatic", "--format", "json"],
            output,
            error);

        Assert.Equal(0, completion.ExitCode);
        using (var settings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))))
        {
            Assert.Equal(["missing/dir"], settings.RootElement.GetProperty("removedFiles").EnumerateArray()
                .Select(path => path.GetString()).ToArray());
            Assert.False(settings.RootElement.TryGetProperty("removedDirectories", out _));
        }
        using (var ownership = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.lock.json"))))
        {
            Assert.Equal(["missing/dir/owned.md"], ownership.RootElement.GetProperty("framework").GetProperty("paths")
                .EnumerateArray().Select(path => path.GetString()).ToArray());
        }
        Assert.Equal(string.Empty, error.ToString());
    }

    private static string ReadRecoveryPath(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.GetProperty("data").GetProperty("recoveryPath").GetString()
            ?? throw new InvalidOperationException("A successful deletion must identify its retained recovery bundle.");
    }
}
