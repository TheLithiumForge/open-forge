using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

public sealed class ToolRecordGrantTests
{
    [Fact(DisplayName = "An absent settings grant creates only authored settings and returns a verified pending backup"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task CreatesOnlySettings()
    {
        using var fixture = new ToolRecordFixture();
        var request = await Request(fixture, "notes", "team/config.json");
        var result = await fixture.Service.AddAllowInstallPathsAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.Equal(Settings(fixture), Assert.Single(result.WrittenPaths));
        var pending = Assert.Single(result.PendingBackups);
        Assert.True(pending.Verified);
        Assert.NotNull(pending.ExpectedAppliedSha256);
        Assert.Empty(result.Unfinished);
        var bytes = await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken);
        var document = WorkspaceSettingsCodec.Read(bytes).Document;
        Assert.NotNull(document);
        Assert.Equal(request.AllowInstallPaths, document.AllowInstallPaths);
        Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, document.SchemaVersion);
        NoToolState(fixture);
    }

    [Fact(DisplayName = "Explicit grants keep unknown keys and order without upgrading settings, inspecting Tool home or rewriting an invalid lock"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task KeepsAuthoredDataAndIgnoresOtherStores()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var original = new JsonObject
        {
            ["future-first"] = new JsonObject { ["nested"] = 17 },
            [WorkspaceSettingsDefinitions.SchemaVersionProperty] = ToolRecordDefinitions.ReleasedSettingsSchemaVersion,
            [WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = new JsonArray("existing"),
            ["future-last"] = new JsonArray(23),
        };
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, original.ToJsonString());
        fixture.Workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath, "invalid synthetic lock");
        var homeFile = fixture.Workspace.CreateFile("unused-home", "unrelated");
        var request = await Request(fixture, "notes", "team");
        Assert.Equal(ToolRecordReadState.Invalid, request.Expected.PortableState);
        request = request with { Locations = request.Locations with { HomePath = homeFile } };
        var lockBefore = await File.ReadAllBytesAsync(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath), token);
        var result = await fixture.Service.AddAllowInstallPathsAsync(request, token);
        Assert.Null(result.Failure);
        Assert.Equal(Settings(fixture), Assert.Single(result.WrittenPaths));
        var bytes = await File.ReadAllBytesAsync(Settings(fixture), token);
        var after = ToolRecordJson.Read(bytes);
        Assert.Equal(original.Select(pair => pair.Key), after.Select(pair => pair.Key));
        Assert.True(JsonNode.DeepEquals(original["future-first"], after["future-first"]));
        Assert.True(JsonNode.DeepEquals(original["future-last"], after["future-last"]));
        Assert.Equal(ToolRecordDefinitions.ReleasedSettingsSchemaVersion, after[WorkspaceSettingsDefinitions.SchemaVersionProperty]?.GetValue<int>());
        var expected = WorkspaceSettingsCodec.AddAllowInstallPath(Encoding.UTF8.GetBytes(original.ToJsonString()), request.AllowInstallPaths[0]) ?? throw new InvalidOperationException();
        expected = WorkspaceSettingsCodec.AddAllowInstallPath(expected, request.AllowInstallPaths[1]) ?? throw new InvalidOperationException();
        Assert.Equal(expected, bytes);
        Assert.Equal(lockBefore, await File.ReadAllBytesAsync(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath), token));
        Assert.False(Directory.Exists(fixture.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Workspace.Combine(OpenForge.Cli.TestSupport.TemporaryWorkspace.RuntimeDirectoryName)));
    }

    [Fact(DisplayName = "Already admitted grants use the existing portable comparison and leave bytes and backups unchanged"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task AlreadyAdmittedIsNoOp()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, new JsonObject
        {
            [WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = new JsonArray("Notes"),
        }.ToJsonString());
        var token = TestContext.Current.CancellationToken;
        var before = await File.ReadAllBytesAsync(Settings(fixture), token);
        var result = await fixture.Service.AddAllowInstallPathsAsync(await Request(fixture, "notes/child", "NOTES", "notes/child"), token);
        Assert.Null(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.Equal(before, await File.ReadAllBytesAsync(Settings(fixture), token));
        NoToolState(fixture);
    }

    [Theory(DisplayName = "Every explicit grant is checked with the portable grammar before any mutation"), InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5),
     Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task InvalidBatchWritesNothing(int shape)
    {
        using var fixture = new ToolRecordFixture();
        var value = shape switch
        {
            0 => string.Empty,
            1 => "../outside",
            2 => fixture.Workspace.Path,
            3 => "notes\\child",
            4 => "notes/*",
            5 => "notes/",
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        var result = await fixture.Service.AddAllowInstallPathsAsync(await Request(fixture, "valid-first", value), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(Directory.Exists(fixture.Workspace.Combine(WorkspaceSettingsDefinitions.DirectoryName)));
        NoToolState(fixture);
    }

    [Theory(DisplayName = "Stale settings bytes, stale absence and missing expectations refuse explicit grants"), InlineData(0), InlineData(1), InlineData(2),
     Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task StaleExpectations(int shape)
    {
        using var fixture = new ToolRecordFixture();
        if (shape == 1) fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        var request = await Request(fixture, "notes");
        if (shape == 0) fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        if (shape == 1) fixture.Workspace.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, new JsonObject { ["changed"] = true }.ToJsonString());
        if (shape == 2) request = request with { Expected = request.Expected with { ExpectedFileSha256 = new Dictionary<string, string?>() } };
        var before = File.Exists(Settings(fixture)) ? await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken) : null;
        var result = await fixture.Service.AddAllowInstallPathsAsync(request, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        if (before is not null) Assert.Equal(before, await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken));
        else Assert.False(File.Exists(Settings(fixture)));
        NoToolState(fixture);
    }

    [Theory(DisplayName = "Malformed or empty existing settings aren't replaced by a grant"), InlineData(0), InlineData(1), InlineData(2),
     Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task MalformedSettings(int shape)
    {
        using var fixture = new ToolRecordFixture();
        var value = shape switch
        {
            0 => string.Empty,
            1 => "[]",
            2 => new JsonObject { [WorkspaceSettingsDefinitions.AllowInstallPathsProperty] = 17 }.ToJsonString(),
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, value);
        var before = await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken);
        var result = await fixture.Service.AddAllowInstallPathsAsync(await Request(fixture, "notes"), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.Equal(before, await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken));
        NoToolState(fixture);
    }

    [Theory(DisplayName = "A nonordinary settings destination or parent refuses explicit grants"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task NonordinarySettings(bool parent)
    {
        using var fixture = new ToolRecordFixture();
        if (parent) fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.DirectoryName, "occupied");
        else fixture.Workspace.CreateDirectory(WorkspaceSettingsDefinitions.RelativePath);
        var result = await fixture.Service.AddAllowInstallPathsAsync(await Request(fixture, "notes"), TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        NoToolState(fixture);
    }

    [Fact(DisplayName = "A failed grant backup returns its handle and keeps settings and saved backup bytes"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task BackupFailureKeepsHandle()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        var token = TestContext.Current.CancellationToken;
        var before = await File.ReadAllBytesAsync(Settings(fixture), token);
        var backups = new GrantBackups { Fail = true };
        var service = new ToolRecordService(fixture.Catalogue, new ToolRecordFixture.Versions(), backups);
        var result = await service.AddAllowInstallPathsAsync(await Request(fixture, "notes"), token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.NotNull(pending.ExpectedAppliedSha256);
        Assert.NotNull(pending.Backup.BackupPath);
        Assert.Contains(pending.Backup.BackupPath, result.Unfinished);
        Assert.Equal(before, await File.ReadAllBytesAsync(pending.Backup.BackupPath, token));
        Assert.Equal(before, await File.ReadAllBytesAsync(Settings(fixture), token));
        NoToolState(fixture);
    }

    [Theory(DisplayName = "Changed settings or cancellation after backup keep the handle and prevent replacement"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task FailureAfterBackup(bool cancelled)
    {
        using var fixture = new ToolRecordFixture();
        using var cancellation = new CancellationTokenSource();
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        var request = await Request(fixture, "notes");
        var changed = Encoding.UTF8.GetBytes(new JsonObject { ["external-change"] = true }.ToJsonString());
        var backups = new GrantBackups
        {
            AfterPrepare = () => { if (cancelled) cancellation.Cancel(); else File.WriteAllBytes(Settings(fixture), changed); },
        };
        var service = new ToolRecordService(fixture.Catalogue, new ToolRecordFixture.Versions(), backups);
        var result = await service.AddAllowInstallPathsAsync(request, cancellation.Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        var pending = Assert.Single(result.PendingBackups);
        Assert.False(pending.Verified);
        Assert.NotNull(pending.Backup.BackupPath);
        var current = await File.ReadAllBytesAsync(Settings(fixture), TestContext.Current.CancellationToken);
        if (cancelled) Assert.Equal(await File.ReadAllBytesAsync(pending.Backup.BackupPath, TestContext.Current.CancellationToken), current);
        else Assert.Equal(changed, current);
        NoToolState(fixture);
    }

    [Fact(DisplayName = "Cancellation before grant publication returns no effects or backup handles"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task CancelledBeforeStart()
    {
        using var fixture = new ToolRecordFixture();
        var request = await Request(fixture, "notes");
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var result = await fixture.Service.AddAllowInstallPathsAsync(request, cancellation.Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(File.Exists(Settings(fixture)));
        NoToolState(fixture);
    }

    [Fact(DisplayName = "An empty explicit grant request creates no settings or tool state"), Trait("Feature", "tool-record-grants"), Trait("Evidence", "Integration")]
    public async Task EmptyRequest()
    {
        using var fixture = new ToolRecordFixture();
        var result = await fixture.Service.AddAllowInstallPathsAsync(await Request(fixture), TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.False(File.Exists(Settings(fixture)));
        NoToolState(fixture);
    }

    private static async Task<ToolRecordGrantRequest> Request(ToolRecordFixture fixture, params string[] paths)
        => new(fixture.Locations, await fixture.Service.ReadAsync(fixture.Locations, TestContext.Current.CancellationToken), paths);
    private static string Settings(ToolRecordFixture fixture) => fixture.PortablePath(WorkspaceSettingsDefinitions.RelativePath);
    private static void NoToolState(ToolRecordFixture fixture)
    {
        Assert.False(File.Exists(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.False(Directory.Exists(fixture.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Workspace.Combine(OpenForge.Cli.TestSupport.TemporaryWorkspace.RuntimeDirectoryName)));
    }

    private sealed class GrantBackups : IToolBackupService
    {
        internal bool Fail { get; init; }
        internal Action? AfterPrepare { get; init; }
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        {
            var sibling = Path.Combine(Path.GetDirectoryName(request.TargetPath) ?? throw new InvalidOperationException(), Path.GetRandomFileName());
            File.Copy(request.TargetPath, sibling);
            AfterPrepare?.Invoke();
            return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.Sibling, BackupPath = sibling, Failure = Fail ? "synthetic backup failure" : null });
        }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
            => throw new InvalidOperationException("Only the command completes grant backup handles.");
    }
}
