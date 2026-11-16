using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

[Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
public sealed class ToolRecordDeletionTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Explicit final record deletion disappears from fresh production reads while account and PATH facts survive")]
    public async Task FinalRemovalFreshRead()
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var workspaceBytes = await File.ReadAllBytesAsync(fixture.Records.Paths.WorkspaceRecordPath, Token);
        var pathBytes = await File.ReadAllBytesAsync(fixture.Records.Paths.PathRecordPath, Token);
        var request = fixture.Publication(before) with
        {
            Portable = new() { Settings = new Dictionary<string, ToolSetting>(), Lock = new Dictionary<string, ToolLockRecord>() },
            RemovedToolIds = [ToolRecordFixture.Id],
            RemovedRuntimeVersions = [ToolRecordFixture.InitialVersion],
        };
        var result = await fixture.Service.WriteAsync(request, Token);
        Assert.Null(result.Failure);
        Assert.False(result.Cancelled);
        Assert.Empty(result.Unfinished);
        Assert.Contains(fixture.SelectedPath(runtime: false), result.WrittenPaths);
        Assert.Contains(fixture.SelectedPath(runtime: true), result.WrittenPaths);
        var deletions = result.PendingBackups.Where(item => item.ExpectedAppliedSha256 is null).ToArray();
        Assert.Equal(2, deletions.Length);
        Assert.All(deletions, item => { Assert.True(item.Verified); Assert.Equal(ToolBackupKind.Sibling, item.Backup.Kind); Assert.True(File.Exists(item.Backup.BackupPath)); });
        var after = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        Assert.Empty(after.Machine);
        Assert.Empty(after.Runtimes);
        var portable = Assert.IsType<ToolPortableRecords>(after.Portable);
        Assert.Empty(portable.Settings);
        Assert.Empty(portable.Lock);
        Assert.Equal(ToolRecordReadState.Missing, after.MachineState);
        Assert.Equal(ToolRecordReadState.Missing, after.RuntimeRecordsState);
        Assert.Equal(before.WorkspaceMachine?.NativeSettings[0].Previous, after.WorkspaceMachine?.NativeSettings[0].Previous);
        Assert.Equal(workspaceBytes, await File.ReadAllBytesAsync(fixture.Records.Paths.WorkspaceRecordPath, Token));
        Assert.Equal(pathBytes, await File.ReadAllBytesAsync(fixture.Records.Paths.PathRecordPath, Token));
        Assert.True(Directory.Exists(fixture.Records.Paths.ToolsDirectory));
        Assert.True(Directory.Exists(fixture.Records.Paths.RuntimesDirectory));
        Assert.False(Directory.Exists(fixture.Records.Workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
    }

    [Fact(DisplayName = "Partial machine and runtime upserts keep omitted records and all their unknown fields")]
    public async Task OmittedRowsSurvive()
    {
        using var fixture = new ToolRecordPublicationFixture();
        await fixture.SeedAsync(additional: true);
        var paths = new[] { fixture.Records.Paths.Tool(ToolRecordPublicationFixture.OtherToolId), fixture.Records.Paths.Runtime(ToolRecordFixture.ChangedVersion) };
        var saved = new Dictionary<string, byte[]>();
        foreach (var path in paths)
        {
            var root = ToolRecordJson.Read(await File.ReadAllBytesAsync(path, Token));
            root["future-record"] = new JsonObject { ["kept"] = 41 };
            saved.Add(path, ToolRecordJson.Write(root));
            await File.WriteAllBytesAsync(path, saved[path], Token);
        }
        var before = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        var result = await fixture.Service.WriteAsync(fixture.Publication(before) with
        {
            Machine = [fixture.Records.Machine() with { Version = ToolRecordFixture.ChangedVersion }],
            Runtimes = [fixture.Runtime()],
        }, Token);
        Assert.Null(result.Failure);
        var after = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        Assert.Equal(2, after.Machine.Count);
        Assert.Equal(2, after.Runtimes.Count);
        Assert.Equal(ToolRecordFixture.ChangedVersion, after.Machine[ToolRecordFixture.Id].Version);
        foreach (var (path, bytes) in saved) Assert.Equal(bytes, await File.ReadAllBytesAsync(path, Token));
        var removed = await fixture.Service.WriteAsync(fixture.Publication(after) with
        {
            RemovedToolIds = [ToolRecordFixture.Id],
            RemovedRuntimeVersions = [ToolRecordFixture.InitialVersion],
        }, Token);
        Assert.Null(removed.Failure);
        var surviving = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        Assert.Equal(ToolRecordPublicationFixture.OtherToolId, Assert.Single(surviving.Machine).Key);
        Assert.Equal(ToolRecordFixture.ChangedVersion, Assert.Single(surviving.Runtimes).Key);
        foreach (var (path, bytes) in saved) Assert.Equal(bytes, await File.ReadAllBytesAsync(path, Token));
    }

    [Fact(DisplayName = "Observed records outside the current catalogue can be explicitly deleted without touching other records")]
    public async Task ObservedUnknownToolCanBeDeleted()
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync(additional: true);
        Assert.DoesNotContain(ToolRecordPublicationFixture.OtherToolId, fixture.Records.Catalogue.Entries.Keys);
        var surviving = await File.ReadAllBytesAsync(fixture.SelectedPath(runtime: false), Token);
        var result = await fixture.Service.WriteAsync(fixture.Publication(before) with { RemovedToolIds = [ToolRecordPublicationFixture.OtherToolId] }, Token);
        Assert.Null(result.Failure);
        var after = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        Assert.Equal(ToolRecordFixture.Id, Assert.Single(after.Machine).Key);
        Assert.Equal(2, after.Runtimes.Count);
        Assert.Equal(surviving, await File.ReadAllBytesAsync(fixture.SelectedPath(runtime: false), Token));
    }

    [Theory(DisplayName = "Changed or missing selected records stop the whole publication before any backup or write")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task StaleSelectedFile(bool runtime, bool missing)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var path = fixture.SelectedPath(runtime);
        byte[]? changed = null;
        if (missing) File.Delete(path);
        else
        {
            var root = ToolRecordJson.Read(await File.ReadAllBytesAsync(path, Token));
            root["concurrent-edit"] = true;
            changed = ToolRecordJson.Write(root);
            await File.WriteAllBytesAsync(path, changed, Token);
        }
        var settings = await File.ReadAllBytesAsync(fixture.Records.PortablePath(WorkspaceSettingsDefinitions.RelativePath), Token);
        var locked = await File.ReadAllBytesAsync(fixture.Records.PortablePath(WorkspaceOwnershipDefinitions.RelativePath), Token);
        var request = fixture.Deletion(before, runtime) with { Portable = new() { Settings = new Dictionary<string, ToolSetting>(), Lock = new Dictionary<string, ToolLockRecord>() } };
        var result = await fixture.Service.WriteAsync(request, Token);
        Assert.NotNull(result.Failure);
        Assert.False(result.Cancelled);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.Equal(settings, await File.ReadAllBytesAsync(fixture.Records.PortablePath(WorkspaceSettingsDefinitions.RelativePath), Token));
        Assert.Equal(locked, await File.ReadAllBytesAsync(fixture.Records.PortablePath(WorkspaceOwnershipDefinitions.RelativePath), Token));
        if (changed is null) Assert.False(File.Exists(path));
        else Assert.Equal(changed, await File.ReadAllBytesAsync(path, Token));
    }

    [Theory(DisplayName = "Record deletion requires both a file expectation and the matching observed typed identity")]
    [InlineData(false, 0), InlineData(false, 1), InlineData(false, 2), InlineData(false, 3), InlineData(false, 4)]
    [InlineData(true, 0), InlineData(true, 1), InlineData(true, 2), InlineData(true, 3), InlineData(true, 4)]
    public async Task UnobservedOrDifferentIdentity(bool runtime, int shape)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var expected = await fixture.SeedAsync();
        var path = fixture.SelectedPath(runtime);
        var bytes = await File.ReadAllBytesAsync(path, Token);
        if (shape is 0 or 1)
        {
            var hashes = expected.ExpectedFileSha256.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            if (shape == 0) hashes.Remove(path);
            else hashes[path] = null;
            expected = expected with { ExpectedFileSha256 = hashes };
        }
        else if (runtime)
        {
            var rows = expected.Runtimes.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            if (shape == 2) rows.Clear();
            else if (shape == 3) rows[ToolRecordFixture.InitialVersion] = rows[ToolRecordFixture.InitialVersion] with { Version = ToolRecordFixture.ChangedVersion };
            else rows[ToolRecordFixture.InitialVersion] = rows[ToolRecordFixture.InitialVersion] with { RuntimePath = fixture.Records.Home.Path };
            expected = expected with { Runtimes = rows };
        }
        else
        {
            var rows = expected.Machine.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
            if (shape == 2) rows.Clear();
            else if (shape == 3) rows[ToolRecordFixture.Id] = rows[ToolRecordFixture.Id] with { ToolId = ToolRecordPublicationFixture.OtherToolId };
            else rows[ToolRecordFixture.Id] = rows[ToolRecordFixture.Id] with { Version = ToolRecordFixture.ChangedVersion };
            expected = expected with { Machine = rows };
        }
        var result = await fixture.Service.WriteAsync(fixture.Deletion(expected, runtime), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path, Token));
    }

    [Theory(DisplayName = "Conflicting write/delete selections and repeated deletions fail before effects")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public async Task ConflictingSelections(bool runtime, bool duplicate)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var request = fixture.Deletion(before, runtime);
        if (runtime)
        {
            request = duplicate
                ? request with { RemovedRuntimeVersions = [ToolRecordFixture.InitialVersion, ToolRecordFixture.InitialVersion] }
                : request with { Runtimes = [fixture.Runtime()] };
        }
        else
        {
            request = duplicate
                ? request with { RemovedToolIds = [ToolRecordFixture.Id, ToolRecordFixture.Id] }
                : request with { Machine = [fixture.Records.Machine()] };
        }
        var result = await fixture.Service.WriteAsync(request, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.True(File.Exists(fixture.SelectedPath(runtime)));
    }

    [Theory(DisplayName = "Unsafe IDs, nonexact runtime versions and unknown selections never authorize record deletion")]
    [InlineData(false, "../outside"), InlineData(false, "unsafe/name"), InlineData(false, "unobserved")]
    [InlineData(true, "../outside"), InlineData(true, "3.2"), InlineData(true, "3.2.1-preview"), InlineData(true, "9.8.7")]
    public async Task InvalidSelections(bool runtime, string value)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var request = fixture.Publication(before) with { RemovedToolIds = runtime ? [] : [value], RemovedRuntimeVersions = runtime ? [value] : [] };
        var result = await fixture.Service.WriteAsync(request, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.True(File.Exists(fixture.SelectedPath(runtime: false)));
        Assert.True(File.Exists(fixture.SelectedPath(runtime: true)));
    }

    [Theory(DisplayName = "Fresh missing records cannot be selected for deletion and create no publication state")]
    [InlineData(false), InlineData(true)]
    public async Task MissingObservedRecord(bool runtime)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.Service.ReadAsync(fixture.Records.Locations, Token);
        var result = await fixture.Service.WriteAsync(fixture.Deletion(before, runtime), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(Directory.Exists(fixture.Records.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Records.Workspace.Combine(WorkspaceSettingsDefinitions.DirectoryName)));
    }

    [Theory(DisplayName = "A directory replacing an observed record is kept and blocks deletion without recursive cleanup")]
    [InlineData(false), InlineData(true)]
    public async Task NonordinarySelectedRecord(bool runtime)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var path = fixture.SelectedPath(runtime);
        File.Delete(path);
        Directory.CreateDirectory(path);
        var result = await fixture.Service.WriteAsync(fixture.Deletion(before, runtime), Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.True(Directory.Exists(path));
    }

    [Theory(DisplayName = "Changed, missing or nonordinary records after backup preparation block deletion and keep recovery evidence")]
    [InlineData(false, 0), InlineData(false, 1), InlineData(false, 2), InlineData(true, 0), InlineData(true, 1), InlineData(true, 2)]
    public async Task ConcurrentEditAfterBackup(bool runtime, int change)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var path = fixture.SelectedPath(runtime);
        var original = await File.ReadAllBytesAsync(path, Token);
        var root = ToolRecordJson.Read(original);
        root["concurrent-edit"] = true;
        var changed = ToolRecordJson.Write(root);
        fixture.Backups.AfterPrepare = (backup, _) =>
        {
            if (change == 0) File.WriteAllBytes(path, changed);
            else
            {
                File.Delete(path);
                if (change == 2) Directory.CreateDirectory(path);
            }
            return backup;
        };
        var result = await fixture.Service.WriteAsync(fixture.Deletion(before, runtime), Token);
        Assert.NotNull(result.Failure);
        Assert.False(result.Cancelled);
        Assert.Empty(result.WrittenPaths);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.Null(pending.ExpectedAppliedSha256);
        var sibling = Assert.IsType<string>(pending.Backup.BackupPath);
        Assert.Contains(path, result.Unfinished);
        Assert.Contains(sibling, result.Unfinished);
        Assert.Equal(original, await File.ReadAllBytesAsync(sibling, Token));
        if (change == 0) Assert.Equal(changed, await File.ReadAllBytesAsync(path, Token));
        else Assert.Equal(change == 2, Directory.Exists(path));
    }

    [Theory(DisplayName = "An occupied sibling backup blocks record deletion and keeps the target and unrelated backup")]
    [InlineData(false), InlineData(true)]
    public async Task OccupiedDeletionBackup(bool runtime)
    {
        using var fixture = new ToolRecordPublicationFixture();
        var before = await fixture.SeedAsync();
        var path = fixture.SelectedPath(runtime);
        var original = await File.ReadAllBytesAsync(path, Token);
        var sibling = ToolBackupPaths.Beside(path);
        byte[] occupied = [1, 7, 13];
        await File.WriteAllBytesAsync(sibling, occupied, Token);
        var result = await fixture.Service.WriteAsync(fixture.Deletion(before, runtime), Token);
        Assert.NotNull(result.Failure);
        Assert.False(result.Cancelled);
        Assert.Empty(result.WrittenPaths);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.Null(pending.Backup.BackupPath);
        Assert.Null(pending.ExpectedAppliedSha256);
        Assert.Contains(path, result.Unfinished);
        Assert.Equal(original, await File.ReadAllBytesAsync(path, Token));
        Assert.Equal(occupied, await File.ReadAllBytesAsync(sibling, Token));
    }
}
