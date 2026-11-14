using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

public sealed class ToolRecordServiceTests
{
    [Fact(DisplayName = "Absent Tool records produce absence expectations without creating state"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task MissingRecordsAreReadOnly()
    {
        using var fixture = new ToolRecordFixture();
        var read = await fixture.Service.ReadAsync(fixture.Locations, TestContext.Current.CancellationToken);
        Assert.Equal(ToolRecordReadState.Missing, read.PortableState);
        Assert.Equal(ToolRecordReadState.Missing, read.MachineState);
        Assert.Equal(ToolRecordReadState.Missing, read.WorkspaceMachineState);
        Assert.Null(read.ExpectedFileSha256[fixture.Paths.Tool(ToolRecordFixture.Id)]);
        Assert.False(Directory.Exists(fixture.Paths.StatePath));
        Assert.False(Directory.Exists(fixture.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
    }

    [Fact(DisplayName = "Tool record publication separates exact portable facts from machine paths and native previous settings"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task PortableAndMachineRoundTrip()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var read = await fixture.Service.ReadAsync(fixture.Locations, token);
        var workspace = new ToolWorkspaceMachineState
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            WorkspaceKey = fixture.Paths.WorkspaceKey,
            WorkspacePath = fixture.PhysicalWorkspace,
            NativeSettings = [new() { ToolId = ToolRecordFixture.Id, ChangeId = "synthetic-native", NativeHomePath = fixture.Paths.HomePath,
                Previous = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "previous-only" },
                Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "applied-only" }, ManualRestoreArguments = ["native-restore"] }],
        };
        var result = await fixture.Service.WriteAsync(fixture.Write(read) with
        {
            WorkspaceMachine = workspace,
            Path = new() { SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion, BinPath = fixture.Home.Combine(ToolRecordDefinitions.HomeDirectoryName, ToolRecordDefinitions.BinDirectoryName), Changes = [] },
        }, token);
        Assert.Null(result.Failure);
        Assert.Equal(5, result.WrittenPaths.Count);
        Assert.Equal(5, result.PendingBackups.Count);
        Assert.All(result.PendingBackups, item => { Assert.True(item.Verified); Assert.NotNull(item.ExpectedAppliedSha256); });
        Assert.False(Directory.Exists(Path.Combine(fixture.Paths.HomePath, ToolRecordDefinitions.HomeDirectoryName)));
        var after = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Equal(ToolRecordFixture.InitialVersion, after.Portable?.Lock[ToolRecordFixture.Id].Version);
        Assert.Equal(ToolRecordFixture.Channel, after.Portable?.Lock[ToolRecordFixture.Id].Channel);
        Assert.Equal(fixture.CommandPath, after.Machine[ToolRecordFixture.Id].CommandPath);
        Assert.Equal(workspace.NativeSettings[0].Previous, after.WorkspaceMachine?.NativeSettings[0].Previous);
        var lockRoot = ToolRecordJson.Read(await File.ReadAllBytesAsync(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath), token));
        var entry = Assert.IsType<JsonObject>(lockRoot[ToolRecordDefinitions.ToolsProperty]?[ToolRecordFixture.Id]);
        Assert.Equal(new[] { ToolRecordDefinitions.VersionProperty, ToolRecordDefinitions.ChannelProperty, ToolRecordDefinitions.ChangesProperty }.Order(StringComparer.Ordinal), entry.Select(pair => pair.Key).Order(StringComparer.Ordinal));
        Assert.False(Directory.Exists(fixture.Workspace.Combine(TemporaryWorkspace.RuntimeDirectoryName)));
    }

    [Fact(DisplayName = "Tool publication keeps unknown settings, ownership, machine and workspace members"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task UnknownMembersSurvivePublication()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, new JsonObject { [WorkspaceSettingsDefinitions.SchemaVersionProperty] = ToolRecordDefinitions.ReleasedSettingsSchemaVersion, ["future"] = 9 }.ToJsonString());
        fixture.Workspace.WriteText(WorkspaceOwnershipDefinitions.RelativePath, new JsonObject { [WorkspaceOwnershipDefinitions.SchemaVersionProperty] = ToolRecordDefinitions.ReleasedLockSchemaVersion, ["future"] = 11 }.ToJsonString());
        var read = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Null((await fixture.Service.WriteAsync(fixture.Write(read), token)).Failure);
        var machinePath = fixture.Paths.Tool(ToolRecordFixture.Id);
        var root = ToolRecordJson.Read(await File.ReadAllBytesAsync(machinePath, token));
        root["future-machine"] = new JsonObject { ["value"] = 17 };
        await File.WriteAllBytesAsync(machinePath, ToolRecordJson.Write(root), token);
        var next = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Null((await fixture.Service.WriteAsync(fixture.Write(next), token)).Failure);
        var machine = ToolRecordJson.Read(await File.ReadAllBytesAsync(machinePath, token));
        Assert.True(JsonNode.DeepEquals(root["future-machine"], machine["future-machine"]));
        foreach (var relative in new[] { WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath })
            Assert.True(ToolRecordJson.Read(await File.ReadAllBytesAsync(fixture.Workspace.Combine(relative), token)).ContainsKey("future"));
    }

    [Fact(DisplayName = "A changed record hash stops publication before effects"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task ExpectedStateMismatchWritesNothing()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var read = await fixture.Service.ReadAsync(fixture.Locations, token);
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{}");
        var result = await fixture.Service.WriteAsync(fixture.Write(read), token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(File.Exists(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
    }

    [Fact(DisplayName = "A record write failure reports actual completed paths and unfinished publication"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task FailedWriteKeepsCompletedPrefix()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var service = new ToolRecordService(fixture.Catalogue, new ToolRecordFixture.Versions(), new ToolRecordFixture.BackupHost { FailOnCall = 2 });
        var read = await service.ReadAsync(fixture.Locations, token);
        var result = await service.WriteAsync(fixture.Write(read), token);
        Assert.NotNull(result.Failure);
        Assert.Equal(2, result.PendingBackups.Count);
        Assert.True(result.PendingBackups[0].Verified);
        Assert.False(result.PendingBackups[1].Verified);
        Assert.Equal(fixture.PortablePath(WorkspaceSettingsDefinitions.RelativePath), Assert.Single(result.WrittenPaths));
        Assert.Contains(fixture.PortablePath(WorkspaceOwnershipDefinitions.RelativePath), result.Unfinished);
        Assert.False(File.Exists(fixture.Workspace.Combine(WorkspaceOwnershipDefinitions.RelativePath)));
        Assert.True(File.Exists(fixture.Workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
    }

    [Theory(DisplayName = "Lower, higher and absent schema versions keep understood Tool declarations"), InlineData(null), InlineData(1), InlineData(99), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task VersionDifferencesAreFacts(int? version)
    {
        using var fixture = new ToolRecordFixture();
        var root = new JsonObject { [ToolRecordDefinitions.ToolsProperty] = new JsonObject { [ToolRecordFixture.Id] = new JsonObject { [ToolRecordDefinitions.VersionProperty] = "3.2.x" } } };
        if (version is { } number) root[WorkspaceSettingsDefinitions.SchemaVersionProperty] = number;
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, root.ToJsonString());
        var result = await fixture.Service.ReadAsync(fixture.Locations, TestContext.Current.CancellationToken);
        Assert.Equal(ToolRecordReadState.Available, result.PortableState);
        Assert.NotEmpty(result.Problems);
        Assert.Equal(new(3, 2), result.Portable?.Settings[ToolRecordFixture.Id].Version);
    }

    [Fact(DisplayName = "Wrong Tool record shapes keep malformed state separate from missing state"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task MalformedIsNotMissing()
    {
        using var fixture = new ToolRecordFixture();
        fixture.Workspace.WriteText(WorkspaceSettingsDefinitions.RelativePath, new JsonObject { [ToolRecordDefinitions.ToolsProperty] = new JsonArray() }.ToJsonString());
        var result = await fixture.Service.ReadAsync(fixture.Locations, TestContext.Current.CancellationToken);
        Assert.Equal(ToolRecordReadState.Invalid, result.PortableState);
        Assert.Equal(ToolRecordReadState.Missing, result.MachineState);
        Assert.NotEmpty(result.Problems);
    }

    [Fact(DisplayName = "Workspace machine facts reject a mismatched physical workspace binding"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task WorkspaceBindingIsChecked()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var read = await fixture.Service.ReadAsync(fixture.Locations, token);
        var result = await fixture.Service.WriteAsync(fixture.Write(read) with
        {
            WorkspaceMachine = new() { SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion, WorkspaceKey = fixture.Paths.WorkspaceKey, WorkspacePath = fixture.Home.Path, NativeSettings = [] },
        }, token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
    }

    [Fact(DisplayName = "Private runtime records use the accepted version filename and keep unknown members"), Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task RuntimeRecordsRoundTrip()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var runtime = new ToolNodeRuntimeRecord
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            Version = ToolRecordFixture.InitialVersion,
            RuntimePath = fixture.Home.Combine(ToolRecordDefinitions.HomeDirectoryName, ToolRecordDefinitions.RuntimesDirectoryName, ToolRecordDefinitions.NodeDirectoryName, ToolRecordFixture.InitialVersion),
            Integrity = new() { Kind = ToolVerificationKind.SignedNodeChecksums, Source = "synthetic-source", Sha256 = new('b', 64) },
        };
        var runtimeRoot = ToolRecordJson.Read(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(runtime, ToolMachineJsonContext.Default.ToolNodeRuntimeRecord));
        runtimeRoot["future-runtime"] = 12;
        var path = fixture.Paths.Runtime(runtime.Version);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        await File.WriteAllBytesAsync(path, ToolRecordJson.Write(runtimeRoot), token);
        var before = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Equal(ToolRecordReadState.Available, before.RuntimeRecordsState);
        Assert.Null((await fixture.Service.WriteAsync(fixture.Write(before) with { Runtimes = [runtime] }, token)).Failure);
        var after = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Equal(runtime, after.Runtimes[runtime.Version]);
        Assert.True(ToolRecordJson.Read(await File.ReadAllBytesAsync(path, token)).ContainsKey("future-runtime"));
    }
    [Theory(DisplayName = "Native-setting records reject missing or relative home bindings without changing stored bytes"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task NativeHomeBindingIsRequired(bool missing)
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var state = new ToolWorkspaceMachineState
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            WorkspaceKey = fixture.Paths.WorkspaceKey,
            WorkspacePath = fixture.PhysicalWorkspace,
            NativeSettings = [new()
            {
                ToolId = ToolRecordFixture.Id, ChangeId = "synthetic-native", NativeHomePath = fixture.Paths.HomePath,
                Previous = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "old" },
                Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "new" }, ManualRestoreArguments = [],
            }],
        };
        var root = ToolRecordJson.Read(System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(state, ToolMachineJsonContext.Default.ToolWorkspaceMachineState));
        var setting = Assert.IsType<JsonObject>(root[Property(nameof(ToolWorkspaceMachineState.NativeSettings))]?[0]);
        var homeKey = Property(nameof(ToolNativeSettingRecord.NativeHomePath));
        if (missing) setting.Remove(homeKey);
        else setting[homeKey] = "relative-home";
        var path = fixture.Paths.WorkspaceRecordPath;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        var bytes = ToolRecordJson.Write(root);
        await File.WriteAllBytesAsync(path, bytes, token);
        var found = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Equal(ToolRecordReadState.Invalid, found.WorkspaceMachineState);
        Assert.Null(found.WorkspaceMachine);
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path, token));
    }

    [Theory(DisplayName = "Null machine collection elements read as invalid facts and incomplete Tool status"), InlineData(0), InlineData(1), InlineData(2),
     Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task NullMachineElementsAreInvalid(int shape)
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var (path, root) = NullElement(fixture, shape);
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? throw new InvalidOperationException());
        var bytes = ToolRecordJson.Write(root);
        await File.WriteAllBytesAsync(path, bytes, token);
        var found = await fixture.Service.ReadAsync(fixture.Locations, token);
        var state = shape switch
        {
            0 => found.MachineState,
            1 => found.WorkspaceMachineState,
            2 => found.PathRecordState,
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        Assert.Equal(ToolRecordReadState.Invalid, state);
        Assert.NotEmpty(found.Problems);
        Assert.Equal(CliSemanticStatus.Incomplete, ToolReadObserver.Status(found, new([])));
        Assert.Equal(bytes, await File.ReadAllBytesAsync(path, token));
    }

    [Theory(DisplayName = "Null machine collection elements stop publication before effects and return a failure result"), InlineData(0), InlineData(1), InlineData(2),
     Trait("Feature", "tool-records"), Trait("Evidence", "Integration")]
    public async Task NullMachineElementsStopWrites(int shape)
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var request = fixture.Write(await fixture.Service.ReadAsync(fixture.Locations, token));
        var (_, root) = NullElement(fixture, shape);
        request = shape switch
        {
            0 => request with { Machine = [root.Deserialize(ToolMachineJsonContext.Default.ToolMachineState) ?? throw new InvalidOperationException()] },
            1 => request with { WorkspaceMachine = root.Deserialize(ToolMachineJsonContext.Default.ToolWorkspaceMachineState) ?? throw new InvalidOperationException() },
            2 => request with { Path = root.Deserialize(ToolMachineJsonContext.Default.ToolPathState) ?? throw new InvalidOperationException() },
            _ => throw new ArgumentOutOfRangeException(nameof(shape)),
        };
        var result = await fixture.Service.WriteAsync(request, token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.WrittenPaths);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(0, fixture.Backups.Calls);
        Assert.False(Directory.Exists(fixture.OpenForgeHome));
        Assert.False(Directory.Exists(fixture.Workspace.Combine(WorkspaceSettingsDefinitions.DirectoryName)));
    }

    private static (string Path, JsonObject Root) NullElement(ToolRecordFixture fixture, int shape)
    {
        JsonObject root;
        string path;
        string collection;
        switch (shape)
        {
            case 0:
                root = ToolRecordJson.Read(JsonSerializer.SerializeToUtf8Bytes(fixture.Machine(), ToolMachineJsonContext.Default.ToolMachineState));
                path = fixture.Paths.Tool(ToolRecordFixture.Id); collection = nameof(ToolMachineState.Launchers);
                break;
            case 1:
                root = ToolRecordJson.Read(JsonSerializer.SerializeToUtf8Bytes(new ToolWorkspaceMachineState
                {
                    SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                    WorkspaceKey = fixture.Paths.WorkspaceKey,
                    WorkspacePath = fixture.PhysicalWorkspace,
                    NativeSettings = [],
                }, ToolMachineJsonContext.Default.ToolWorkspaceMachineState));
                path = fixture.Paths.WorkspaceRecordPath; collection = nameof(ToolWorkspaceMachineState.NativeSettings);
                break;
            case 2:
                root = ToolRecordJson.Read(JsonSerializer.SerializeToUtf8Bytes(new ToolPathState
                {
                    SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                    BinPath = Path.Combine(fixture.OpenForgeHome, ToolRecordDefinitions.BinDirectoryName),
                    Changes = [],
                }, ToolMachineJsonContext.Default.ToolPathState));
                path = fixture.Paths.PathRecordPath; collection = nameof(ToolPathState.Changes);
                break;
            default: throw new ArgumentOutOfRangeException(nameof(shape));
        }
        var values = new JsonArray();
        ((IList<JsonNode?>)values).Add(null);
        root[Property(collection)] = values;
        return (path, root);
    }

    private static string Property(string name) => System.Text.Json.JsonNamingPolicy.CamelCase.ConvertName(name);
}
