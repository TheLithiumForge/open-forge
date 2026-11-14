using OpenForge.Cli.Core.Commands.Tool.Inspect;
using OpenForge.Cli.Core.Commands.Tool.Inspect.Models;
using OpenForge.Cli.Core.Commands.Tool.List;
using OpenForge.Cli.Core.Commands.Tool.List.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Observation;
using OpenForge.Cli.Core.Commands.Tool.Status;
using OpenForge.Cli.Core.Commands.Tool.Status.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Observation;

public sealed class ToolReadCommandTests
{
    [Fact(DisplayName = "Tool List retains catalogue rows in an uninstalled workspace without probing"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task CatalogueWithoutInstallation()
    {
        using var fixture = new ToolRecordFixture();
        var channels = new Channels(fixture);
        var result = await new ToolListOperation(Observer(fixture, channels)).ExecuteAsync(new(Workspace(fixture)), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ToolRecordFixture.Id, Assert.Single(result.Data.Tools).Id);
        Assert.Equal(0, channels.Probes);
        Assert.False(Directory.Exists(fixture.Paths.StatePath));
    }

    [Theory(DisplayName = "Tool Status distinguishes supported self updates from unsupported versions"), InlineData(ToolRecordFixture.ChangedVersion, (int)CliSemanticStatus.Attention, "changed"),
     InlineData(ToolRecordFixture.UnsupportedVersion, (int)CliSemanticStatus.Attention, "unsupported"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task SelfUpdateFacts(string version, int expected, string state)
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        Assert.Null((await fixture.Service.WriteAsync(fixture.Write(await fixture.Service.ReadAsync(fixture.Locations, token)), token)).Failure);
        var channels = new Channels(fixture) { Version = version };
        var result = await new ToolStatusOperation(Observer(fixture, channels)).ExecuteAsync(new(Workspace(fixture), null), token);
        Assert.Equal((CliSemanticStatus)expected, result.Status);
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(state, tool.State);
        Assert.Equal(ToolRecordFixture.InitialVersion, tool.LockedVersion);
        Assert.Equal(version, tool.DetectedVersion);
        Assert.Equal(fixture.Catalogue.Entries[ToolRecordFixture.Id].Detection.Environment, channels.LastRequest?.Entry.Detection.Environment);
        Assert.Equal(1, channels.Probes);
        Assert.Equal(fixture.CommandPath, channels.LastRequest?.RecordedState?.CommandPath);
    }

    [Theory(DisplayName = "Tool Inspect separates missing commands from unavailable native probes"), InlineData(false, (int)CliSemanticStatus.Attention, "missing"),
     InlineData(true, (int)CliSemanticStatus.Incomplete, "unavailable"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task MissingAndUnavailable(bool failure, int status, string state)
    {
        using var fixture = new ToolRecordFixture();
        var channels = new Channels(fixture) { Version = null, Failure = failure ? "synthetic probe failure" : null };
        var result = await new ToolInspectOperation(Observer(fixture, channels)).ExecuteAsync(new(Workspace(fixture), ToolRecordFixture.Id), TestContext.Current.CancellationToken);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(state, tool.State);
        Assert.NotNull(tool.BinPath);
        Assert.Equal(channels.Failure, tool.DetectionFailure);
        Assert.NotNull(tool.ManagerGlobalBinReadiness);
        Assert.False(Directory.Exists(fixture.Paths.StatePath));
    }

    [Fact(DisplayName = "Tool reads report additive drift and missing native machine facts without writing"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task MissingNativeFactsAndConfigurationDrift()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var write = fixture.Write(await fixture.Service.ReadAsync(fixture.Locations, token));
        var locked = write.Portable.Lock[ToolRecordFixture.Id] with { Changes = [new ToolNativeCommandAddition { Id = "synthetic-native", AllowListId = "synthetic-allow" }] };
        Assert.Null((await fixture.Service.WriteAsync(write with { Portable = write.Portable with { Lock = new Dictionary<string, ToolLockRecord> { [ToolRecordFixture.Id] = locked } } }, token)).Failure);
        var observer = Observer(fixture, new(fixture), new Config { Drift = true });
        var before = await fixture.Service.ReadAsync(fixture.Locations, token);
        var result = await new ToolStatusOperation(observer).ExecuteAsync(new(Workspace(fixture), null), token);
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.NotEmpty(tool.Issues);
        Assert.Single(tool.Changes);
        var after = await fixture.Service.ReadAsync(fixture.Locations, token);
        Assert.Equal(before.ExpectedFileSha256, after.ExpectedFileSha256);
    }

    [Fact(DisplayName = "Tool Status with no declarations is a probe-free no-op"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task EmptyDeclarations()
    {
        using var fixture = new ToolRecordFixture();
        var channels = new Channels(fixture);
        var result = await new ToolStatusOperation(Observer(fixture, channels)).ExecuteAsync(new(Workspace(fixture), null), TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Data.Tools);
        Assert.Equal(0, channels.Probes);
    }

    [Fact(DisplayName = "Tool observations carry complete native context, keep other homes and report retired additions"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task CompleteContextAndRetiredChanges()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        var config = new Config { RetiredDrift = true };
        var request = fixture.Write(await fixture.Service.ReadAsync(fixture.Locations, token));
        var native = new ToolNativeSettingRecord
        {
            ToolId = ToolRecordFixture.Id,
            ChangeId = "synthetic-native",
            NativeHomePath = fixture.Paths.HomePath,
            Previous = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "old" },
            Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "new" },
            ManualRestoreArguments = [],
        };
        var locked = request.Portable.Lock[ToolRecordFixture.Id] with { Changes = [new ToolNativeCommandAddition { Id = native.ChangeId, AllowListId = "synthetic" }] };
        Assert.Null((await fixture.Service.WriteAsync(request with
        {
            Portable = request.Portable with { Lock = new Dictionary<string, ToolLockRecord> { [ToolRecordFixture.Id] = locked } },
            WorkspaceMachine = new() { SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion, WorkspaceKey = fixture.Paths.WorkspaceKey, WorkspacePath = fixture.PhysicalWorkspace, NativeSettings = [native] },
        }, token)).Failure);
        var result = await new ToolStatusOperation(Observer(fixture, new(fixture), config)).ExecuteAsync(new(Workspace(fixture), null), token);
        var planned = Assert.IsType<ToolConfigRequest>(config.LastRequest);
        Assert.Equal(fixture.PhysicalWorkspace, planned.Context.WorkspacePath);
        Assert.Equal(fixture.OpenForgeHome, planned.Context.HomePath);
        Assert.Equal(new PhysicalPathResolver().ResolveRoot(fixture.Home.Path).GetContainedPhysicalPath(), planned.Context.NativeHomePath);
        Assert.Equal(WorkspaceSettingsDefinitions.DirectoryName, planned.Context.MainRoot);
        Assert.NotNull(planned.Context.ClientRoots);
        Assert.NotNull(planned.Context.NativeEnvironment);
        Assert.NotNull(planned.InstalledInvocation);
        Assert.Equal(native.NativeHomePath, Assert.Single(planned.NativeSettings).NativeHomePath);
        var tool = Assert.Single(result.Data.Tools);
        Assert.Single(tool.Removals);
        Assert.Contains(ToolReadProjection.Name(ToolObservationIssueKind.ConfigurationDrift), tool.Issues);
        Assert.Contains(ToolReadProjection.Name(ToolObservationIssueKind.NativeRecordMissing), tool.Issues);
    }

    [Fact(DisplayName = "Workspace Status and Doctor construct Tool observations at execution and report missing tools on owned fixtures"), Trait("Feature", "tool-read"), Trait("Evidence", "Integration")]
    public async Task WorkspaceContributorsAreDeferred()
    {
        using var fixture = new ToolRecordFixture();
        var token = TestContext.Current.CancellationToken;
        Assert.Null((await fixture.Service.WriteAsync(fixture.Write(await fixture.Service.ReadAsync(fixture.Locations, token)), token)).Failure);
        var calls = 0;
        ToolWorkspaceObservationReader Create()
        {
            calls++;
            return new(fixture.Catalogue, fixture.Service, Observations(fixture, new(fixture) { Version = null }, new()), fixture.OpenForgeHome);
        }
        var paths = new PhysicalPathResolver();
        var contributors = RepairOperationalContributorFactory.Create(paths);
        var status = new StatusOperation(contributors, paths, Create);
        var doctor = new DoctorOperation(contributors, tools: Create);
        Assert.Equal(0, calls);
        var checkedStatus = await status.ExecuteAsync(new(Workspace(fixture)), token);
        Assert.Contains(checkedStatus.Findings, finding => finding.Code == StatusFindingCode.ToolDeclaredMissing);
        var checkedDoctor = await doctor.ExecuteAsync(new(Workspace(fixture)), token);
        Assert.Contains(checkedDoctor.Diagnosis.Domains.SelectMany(domain => domain.Findings), finding => finding.Kind == DoctorFindingKind.ToolDeclaredMissing);
        Assert.Equal(2, calls);
    }

    private static CliWorkspace Workspace(ToolRecordFixture fixture) => new(fixture.Workspace.Path, fixture.PhysicalWorkspace, CliWorkspaceSelectionMethod.CurrentDirectory);
    private static ToolReadObserver Observer(ToolRecordFixture fixture, Channels channels, Config? config = null)
        => new(fixture.Catalogue, fixture.Service, Observations(fixture, channels, config ?? new()), fixture.OpenForgeHome);

    private static ToolObservationReader Observations(ToolRecordFixture fixture, Channels channels, Config config)
        => new(channels, config, new ToolRecordFixture.Versions(), new Paths(fixture), (request, profile, detection) => new()
        {
            WorkspacePath = request.Locations.WorkspacePath,
            HomePath = fixture.OpenForgeHome,
            BinPath = Path.Combine(fixture.OpenForgeHome, ToolRecordDefinitions.BinDirectoryName),
            NativeHomePath = fixture.Home.Path,
            NativeInstallDirectory = fixture.Home.Path,
            MainRoot = request.MainRoot,
            ClientRoots = new Dictionary<string, string>(),
            NativeEnvironment = new Dictionary<string, string>(),
        });

    private sealed class Channels(ToolRecordFixture fixture) : IToolChannelService
    {
        internal string? Version { get; init; } = ToolRecordFixture.InitialVersion;
        internal string? Failure { get; init; }
        internal int Probes { get; private set; }
        internal ToolDetectionRequest? LastRequest { get; private set; }
        public Task<ToolDetectionResult> DetectAsync(ToolDetectionRequest request, CancellationToken cancellationToken)
        {
            Probes++; LastRequest = request;
            return Task.FromResult(new ToolDetectionResult
            {
                ToolId = ToolRecordFixture.Id,
                Version = Version,
                Failure = Failure,
                CommandPath = Version is null ? null : fixture.CommandPath,
                Invocation = Version is null ? null : new() { ExecutablePath = fixture.CommandPath, PrefixArguments = [] },
            });
        }
        public Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspacePath, CancellationToken cancellationToken) => Task.FromResult<IReadOnlyList<ToolPackageManagerFacts>>([]);
        public Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ToolChannelInstallResult> InstallAsync(ToolChannelInstallRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class Config : IToolConfigService
    {
        internal bool Drift { get; init; }
        internal bool RetiredDrift { get; init; }
        internal ToolConfigRequest? LastRequest { get; private set; }
        public string ComputeProfileDigest(ToolVersionProfile profile) => new('a', 64);
        public Task<ToolConfigPlan> PlanAsync(ToolConfigRequest request, CancellationToken cancellationToken)
        {
            LastRequest = request;
            return Task.FromResult(new ToolConfigPlan(request, Drift ? [new ToolConfigObservation
            {
                Change = new ToolManagedFileChange { Id = "synthetic-file", Scope = ToolEffectScope.Workspace, InitialState = ToolInitialConfigState.AbsentFile, Description = "Synthetic addition", PathTemplate = "synthetic.txt", TemplateResource = "synthetic" },
                Current = new() { Present = false, Kind = ToolRecordedValueKind.Bytes }, Desired = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = [1] }, Disposition = ToolChangeDisposition.Add,
            }] : [], RetiredDrift ? [new ToolConfigRemovalObservation
            {
                Addition = new ToolManagedFileAddition { Id = "synthetic-retired", Path = "synthetic-retired.txt", Sha256 = new('b', 64) },
                Current = new() { Present = true, Kind = ToolRecordedValueKind.Bytes, Bytes = [1] }, Desired = new() { Present = false, Kind = ToolRecordedValueKind.Bytes },
                Disposition = ToolChangeDisposition.Remove,
            }] : []));
        }
        public Task<ToolConfigApplyResult> ApplyAsync(ToolConfigPlan plan, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ToolConfigRemovePlan> PlanRemoveAsync(ToolConfigRemoveRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigRemovePlan plan, CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class Paths(ToolRecordFixture fixture) : IToolPathHost
    {
        public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken) => Task.FromResult(new ToolPathFacts
        { BinPath = fixture.Home.Combine(ToolRecordDefinitions.HomeDirectoryName, ToolRecordDefinitions.BinDirectoryName), Readiness = ToolPathReadiness.Missing, DetectedShellFiles = [] });
        public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
        public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken) => throw new NotSupportedException();
    }
}
