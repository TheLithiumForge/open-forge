using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-channels"), Trait("Evidence", "Integration")]
public sealed class ToolChannelServiceTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Recorded tool detection keeps the exact invocation and catalogue-only probe environment")]
    public async Task DetectsRecordedInvocation()
    {
        using var fixture = new ToolChannelFixture();
        var command = System.IO.Path.Combine(fixture.Native, "sample.exe");
        await File.WriteAllTextAsync(command, "synthetic", Token);
        var state = fixture.State(command);
        var result = await fixture.Service.DetectAsync(new(fixture.Entry(fixture.Npm()), fixture.Workspace) { RecordedState = state }, Token);
        Assert.Equal(ToolChannelFixture.Version, result.Version);
        Assert.Equal(state.Invocation, result.Invocation);
        Assert.Same(state, result.RecordedState);
        Assert.Equal("1", Assert.Single(fixture.Processes.Requests).Environment[ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable]);
    }

    [Theory(DisplayName = "An unknown existing source blocks replacement even with a named channel override")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task KeepsUnknownExistingSource(bool channelOverride)
    {
        using var fixture = new ToolChannelFixture();
        var npm = fixture.Npm();
        var entry = fixture.Entry(npm, GitHub(fixture.Archive("sample.exe")));
        var state = fixture.State(System.IO.Path.Combine(fixture.Native, "sample.exe"));
        var result = await fixture.Service.PlanAsync(fixture.Request(entry) with
        {
            Existing = new() { ToolId = state.ToolId, Version = state.Version, CommandPath = state.CommandPath, Invocation = state.Invocation },
            Selection = new() { Kind = ToolVersionSelectionKind.LatestSupported },
            ChannelOverride = channelOverride ? npm.Id : null,
            AvailableManagers = [fixture.Manager()],
            AllowLatestLookup = true,
        }, Token);
        Assert.Null(result.Resolution);
        Assert.NotEmpty(result.Problems);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(fixture.Downloads.MetadataRequests);
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    [Theory(DisplayName = "Static native Linux assets accept both compatible libc families without inventing numeric floors")]
    [InlineData(ToolLinuxLibc.Glibc)]
    [InlineData(ToolLinuxLibc.Musl)]
    public async Task PlansStaticLinuxAsset(object value)
    {
        using var fixture = new ToolChannelFixture();
        var libc = Assert.IsType<ToolLinuxLibc>(value);
        var target = new ToolTarget { OperatingSystem = ToolOperatingSystem.Linux, Cpu = ToolCpu.X64, LinuxLibc = libc };
        var asset = fixture.Archive("sample.exe") with { Target = target with { LinuxLibc = ToolLinuxLibc.Static } };
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(asset))) with { Target = target }, Token);
        Assert.NotNull(result.Resolution);
        Assert.Same(asset, result.Resolution.Asset);
        Assert.False(result.UsedNetwork);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "An unrecorded declared bundle is identified before choosing another published channel")]
    public async Task DetectsPublishedBundle()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("node.exe", "lib/app.js") with
        {
            BundledRuntime = new()
            {
                Runtime = "node",
                Version = ToolChannelFixture.RuntimeVersion,
                RuntimeRelativePath = "node.exe",
                EntryRelativePath = "lib/app.js",
                Arguments = ["--liftoff-only"]
            }
        };
        var entry = fixture.Entry(fixture.Npm(), GitHub(asset));
        var runtime = System.IO.Path.Combine(fixture.Native, "node.exe");
        var invocation = new ToolCommandTarget { ExecutablePath = runtime, PrefixArguments = ["--liftoff-only", System.IO.Path.Combine(fixture.Native, "lib", "app.js")] };
        fixture.Processes.Commands[entry.Detection.Command] = new(System.IO.Path.Combine(fixture.Native, "sample.cmd"), invocation);
        var detected = await fixture.Service.DetectAsync(new(entry, fixture.Workspace), Token);
        Assert.Equal(GitHub(asset).Id, detected.ChannelId);
        Assert.Equal(runtime, detected.RuntimePath);
        var planned = await fixture.Service.PlanAsync(fixture.Request(entry) with { Existing = detected }, Token);
        Assert.NotNull(planned.Resolution);
        Assert.True(planned.Resolution.ReuseExisting);
        Assert.Empty(planned.Resolution.Execution.Downloads);
    }

    [Fact(DisplayName = "Exact supported reuse wins before another published channel and performs no install")]
    public async Task ReusesSupportedInstallation()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("sample.exe");
        var entry = fixture.Entry(GitHub(asset), fixture.Npm());
        var command = System.IO.Path.Combine(fixture.Native, "sample.exe");
        await File.WriteAllTextAsync(command, "synthetic", Token);
        var state = fixture.State(command) with { ChannelId = fixture.Npm().Id, PackageManager = fixture.Manager(), PlacedByOpenForge = false };
        var request = fixture.Request(entry) with
        {
            Existing = new()
            {
                ToolId = entry.Identity.Id,
                CommandPath = command,
                Invocation = state.Invocation,
                Version = state.Version,
                ChannelId = state.ChannelId,
                RecordedState = state,
                PackageManager = state.PackageManager
            }
        };
        var planned = await fixture.Service.PlanAsync(request, Token);
        Assert.NotNull(planned.Resolution);
        Assert.True(planned.Resolution.ReuseExisting);
        Assert.Equal(state.ChannelId, planned.Resolution.ChannelId);
        Assert.False(planned.UsedNetwork);
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.Null(result.Failure);
        Assert.False(result.Cancelled);
        Assert.NotNull(result.State);
        Assert.False(result.State.PlacedByOpenForge);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Equal(0, fixture.Backups.Prepared);
        Assert.All(fixture.Processes.Requests, command => Assert.Equal(entry.Detection.Arguments, command.Arguments));
    }

    [Theory(DisplayName = "Invalid existing versions and unsupported selectors block before effects")]
    [InlineData("3.0.0")]
    [InlineData("2.4.2")]
    [InlineData("2.4.3-rc.1")]
    public async Task BlocksUnsupportedExisting(string version)
    {
        using var fixture = new ToolChannelFixture();
        var request = fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            Existing = new()
            {
                ToolId = ToolChannelFixture.Id,
                CommandPath = System.IO.Path.Combine(fixture.Native, "sample.exe"),
                Version = version
            }
        };
        var result = await fixture.Service.PlanAsync(request, Token);
        Assert.Null(result.Resolution);
        Assert.NotEmpty(result.Problems);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "Exact GitHub preview keeps explicit paths and makes no network or mutation calls")]
    public async Task PlansPinnedRelease()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("sample.exe");
        var before = fixture.Temporary.SnapshotHashes();
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(asset))), Token);
        Assert.NotNull(result.Resolution);
        Assert.False(result.UsedNetwork);
        Assert.Equal(asset, Assert.Single(result.Resolution.Execution.Downloads).Asset);
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "Pinned release installation retains launcher backups until the caller publishes records")]
    public async Task InstallsPinnedRelease()
    {
        using var fixture = new ToolChannelFixture();
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(fixture.Archive("sample.exe")))), Token);
        Assert.NotNull(planned.Resolution);
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.Null(result.Failure);
        Assert.False(result.Cancelled);
        Assert.NotNull(result.State);
        Assert.Equal(ToolChannelFixture.Version, result.State.Version);
        Assert.True(File.Exists(result.State.CommandPath));
        Assert.True(Assert.Single(result.PendingBackups).Verified);
        Assert.Equal(0, fixture.Backups.Completed);
        Assert.Equal(1, fixture.Downloads.Calls);
        Assert.True(result.State.PlacedByOpenForge);
    }

    [Fact(DisplayName = "A failed chosen download doesn't invoke another channel or manager")]
    public async Task StopsAfterTransferFailure()
    {
        using var fixture = new ToolChannelFixture();
        fixture.Downloads.Fail = true;
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(fixture.Archive("sample.exe")), fixture.Npm())), Token);
        Assert.NotNull(planned.Resolution);
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.NotNull(result.Failure);
        Assert.Null(result.State);
        Assert.Equal(1, fixture.Downloads.Calls);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "Bundled runtime plans keep declared flags before the declared application entry")]
    public async Task PlansDeclaredBundle()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("node.exe", "lib/app.js", "bin/sample.cmd") with
        {
            EntryRelativePath = "bin/sample.cmd",
            BundledRuntime = new() { Runtime = "node", Version = "24.16.0", RuntimeRelativePath = "node.exe", EntryRelativePath = "lib/app.js", Arguments = ["--liftoff-only", "--disable-warning=ExperimentalWarning"] },
        };
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(asset))), Token);
        Assert.NotNull(planned.Resolution);
        var execution = planned.Resolution.Execution;
        Assert.Equal(asset.BundledRuntime.Arguments, execution.Invocation.PrefixArguments.Take(2));
        Assert.Equal(System.IO.Path.Combine(execution.ToolPath ?? throw new InvalidOperationException(), "lib", "app.js"), execution.Invocation.PrefixArguments[2]);
        Assert.Equal(execution.Invocation.PrefixArguments.Concat(new[] { "--version" }), Assert.Single(execution.Commands).Arguments);
    }

    [Fact(DisplayName = "An attestation-only asset stays blocked without claiming verified bytes")]
    public async Task BlocksUnverifiedAttestation()
    {
        using var fixture = new ToolChannelFixture();
        var asset = fixture.Archive("sample.exe") with { Sha256 = null, Attestation = new() { Repository = "example/sample", Workflow = "release.yml", SourceCommit = "synthetic" } };
        var result = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(asset))), Token);
        Assert.Null(result.Resolution);
        Assert.NotEmpty(result.Problems);
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    [Fact(DisplayName = "An existing installation that changes after planning blocks apply")]
    public async Task BlocksStaleInstalledVersion()
    {
        using var fixture = new ToolChannelFixture();
        var request = fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            Existing = new()
            {
                ToolId = ToolChannelFixture.Id,
                Version = ToolChannelFixture.Version,
                ChannelId = fixture.Npm().Id,
                CommandPath = System.IO.Path.Combine(fixture.Native, "sample.exe"),
                Invocation = new() { ExecutablePath = System.IO.Path.Combine(fixture.Native, "node.exe"), PrefixArguments = [] }
            }
        };
        var planned = await fixture.Service.PlanAsync(request, Token);
        Assert.NotNull(planned.Resolution);
        fixture.Processes.Run = _ => ToolChannelFixture.Success("2.4.9");
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.NotNull(result.Failure);
        Assert.Null(result.State);
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    [Fact(DisplayName = "An already cancelled channel request performs no effects")]
    public async Task CancelsBeforeEffects()
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReleasePlan(fixture);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        await cancellation.CancelAsync();
        var result = await fixture.Service.InstallAsync(new(plan), cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Null(result.State);
        Assert.Empty(result.CompletedEffects);
        Assert.Empty(result.Runtimes);
        Assert.Empty(result.PendingBackups);
        Assert.Empty(fixture.Processes.Requests);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Empty(Directory.EnumerateFileSystemEntries(fixture.Home));
    }

    [Theory(DisplayName = "Download cancellation stays typed whether the host throws or returns a failure after caller cancellation")]
    [InlineData(false), InlineData(true)]
    public async Task ReportsDownloadCancellation(bool returnedFailure)
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReleasePlan(fixture);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        fixture.Downloads.Downloading = _ =>
        {
            if (!returnedFailure) throw new OperationCanceledException();
            cancellation.Cancel();
            fixture.Downloads.Fail = true;
        };
        var result = await fixture.Service.InstallAsync(new(plan), cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Null(result.State);
        Assert.Empty(result.CompletedEffects);
        Assert.Empty(result.PendingBackups);
        Assert.Empty(result.Runtimes);
        Assert.Equal(1, fixture.Downloads.Calls);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Fact(DisplayName = "A verified download stays reported when the caller cancels before extraction")]
    public async Task KeepsVerifiedDownloadOnCancellation()
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReleasePlan(fixture);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        fixture.Downloads.Downloaded = _ => cancellation.Cancel();
        var result = await fixture.Service.InstallAsync(new(plan), cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Null(result.State);
        var download = Assert.Single(plan.Execution.Downloads);
        Assert.Equal(download.DestinationPath, Assert.Single(result.CompletedEffects));
        Assert.True(File.Exists(download.DestinationPath));
        Assert.False(File.Exists(Assert.Single(plan.Execution.Launchers).LauncherPath));
        Assert.Empty(result.PendingBackups);
        Assert.Empty(fixture.Processes.Requests);
    }

    [Theory(DisplayName = "Package command cancellation is distinct from timeout and stops before launchers or verification")]
    [InlineData("cancelled"), InlineData("exception"), InlineData("timeout")]
    public async Task StopsAtPackageCommand(string cause)
    {
        using var fixture = new ToolChannelFixture();
        fixture.Processes.Run = _ => ToolChannelFixture.Success(System.IO.Path.Combine(fixture.Native, ToolNpmPackageReader.ModulesDirectory));
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with { AvailableManagers = [fixture.Manager()] }, Token);
        Assert.NotNull(planned.Resolution);
        fixture.Processes.Requests.Clear();
        fixture.Processes.Run = _ => StoppedProcess(cause);
        var result = await fixture.Service.InstallAsync(new(planned.Resolution), Token);
        Assert.Equal(cause != "timeout", result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Null(result.State);
        Assert.Empty(result.CompletedEffects);
        Assert.Empty(result.PendingBackups);
        Assert.Empty(result.Runtimes);
        Assert.Same(planned.Resolution.Execution.Commands[0], Assert.Single(fixture.Processes.Requests));
        Assert.Equal(0, fixture.Backups.Prepared);
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    [Theory(DisplayName = "Interrupted final verification keeps placement and backup evidence without claiming a verified tool state")]
    [InlineData("cancelled"), InlineData("exception"), InlineData("timeout")]
    public async Task KeepsPlacementWhenVerificationStops(string cause)
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReleasePlan(fixture);
        fixture.Processes.Run = _ => StoppedProcess(cause);
        var result = await fixture.Service.InstallAsync(new(plan), Token);
        Assert.Equal(cause != "timeout", result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Null(result.State);
        string[] completed =
        [
            Assert.Single(plan.Execution.Downloads).DestinationPath,
            Assert.Single(plan.Execution.Extractions).DestinationPath,
            Assert.Single(plan.Execution.Launchers).LauncherPath,
        ];
        Assert.Equal(completed, result.CompletedEffects);
        var backup = Assert.Single(result.PendingBackups);
        Assert.True(backup.Verified);
        Assert.Same(Assert.Single(fixture.Backups.Results), backup.Backup);
        Assert.NotNull(backup.ExpectedAppliedSha256);
        Assert.True(File.Exists(backup.Backup.TargetPath));
        Assert.Same(plan.Execution.Commands[^1], Assert.Single(fixture.Processes.Requests));
        Assert.Empty(result.Runtimes);
        Assert.Equal(0, fixture.Backups.Completed);
    }

    [Theory(DisplayName = "Launcher cancellation keeps the verified prefix and every prepared backup handle")]
    [InlineData(false), InlineData(true)]
    public async Task KeepsLauncherPrefixOnCancellation(bool cancelCaller)
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReleasePlan(fixture);
        var first = Assert.Single(plan.Execution.Launchers);
        var second = first with { LauncherPath = System.IO.Path.Combine(plan.Execution.BinPath, "second-sample.exe") };
        plan = plan with { Execution = plan.Execution with { Launchers = [first, second] } };
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        fixture.Backups.Preparing = _ =>
        {
            if (fixture.Backups.Prepared != 2) return;
            if (!cancelCaller) throw new OperationCanceledException();
            cancellation.Cancel();
        };
        var result = await fixture.Service.InstallAsync(new(plan), cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Null(result.State);
        string[] completed =
        [
            Assert.Single(plan.Execution.Downloads).DestinationPath,
            Assert.Single(plan.Execution.Extractions).DestinationPath,
            first.LauncherPath,
        ];
        Assert.Equal(completed, result.CompletedEffects);
        Assert.Equal(cancelCaller ? 2 : 1, result.PendingBackups.Count);
        Assert.True(result.PendingBackups[0].Verified);
        Assert.Same(fixture.Backups.Results[0], result.PendingBackups[0].Backup);
        if (cancelCaller)
        {
            Assert.False(result.PendingBackups[1].Verified);
            Assert.Same(fixture.Backups.Results[1], result.PendingBackups[1].Backup);
        }
        Assert.True(File.Exists(first.LauncherPath));
        Assert.False(File.Exists(second.LauncherPath));
        Assert.Empty(fixture.Processes.Requests);
        Assert.Empty(result.Runtimes);
        Assert.Equal(0, fixture.Backups.Completed);
    }

    [Theory(DisplayName = "Existing-install checks and reuse verification both carry returned process cancellation")]
    [InlineData(1), InlineData(2)]
    public async Task CancelsExistingVerification(int cancelledCall)
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReusePlan(fixture);
        fixture.Processes.Run = _ => fixture.Processes.Requests.Count == cancelledCall ? StoppedProcess("cancelled") : ToolChannelFixture.Success();
        var result = await fixture.Service.InstallAsync(new(plan), Token);
        Assert.True(result.Cancelled);
        Assert.Null(result.State);
        Assert.Empty(result.CompletedEffects);
        Assert.Empty(result.PendingBackups);
        Assert.Equal(cancelledCall, fixture.Processes.Requests.Count);
        Assert.Equal(0, fixture.Downloads.Calls);
        Assert.Equal(0, fixture.Backups.Prepared);
    }

    [Fact(DisplayName = "A verified reused state is kept when cancellation arrives with the successful final probe")]
    public async Task KeepsVerifiedReuseWhenCallerCancels()
    {
        using var fixture = new ToolChannelFixture();
        var plan = await ReusePlan(fixture);
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(Token);
        fixture.Processes.Run = _ =>
        {
            if (fixture.Processes.Requests.Count == 2) cancellation.Cancel();
            return ToolChannelFixture.Success();
        };
        var result = await fixture.Service.InstallAsync(new(plan), cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.NotNull(result.State);
        Assert.Equal(plan.Version, result.State.Version);
        Assert.False(result.State.PlacedByOpenForge);
        Assert.Single(result.CompletedEffects);
        Assert.Empty(result.PendingBackups);
        Assert.Empty(result.Runtimes);
        Assert.Equal(2, fixture.Processes.Requests.Count);
        Assert.Equal(0, fixture.Downloads.Calls);
    }

    private static ToolProcessResult StoppedProcess(string cause) => cause switch
    {
        "cancelled" => ToolChannelFixture.Success() with { ExitCode = null, Cancelled = true },
        "timeout" => ToolChannelFixture.Success() with { ExitCode = null, TimedOut = true },
        "exception" => throw new OperationCanceledException(),
        _ => throw new ArgumentOutOfRangeException(nameof(cause)),
    };

    private static async Task<ToolChannelResolution> ReleasePlan(ToolChannelFixture fixture)
    {
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(GitHub(fixture.Archive("sample.exe")))), Token);
        Assert.NotNull(planned.Resolution);
        return planned.Resolution;
    }

    private static async Task<ToolChannelResolution> ReusePlan(ToolChannelFixture fixture)
    {
        var command = System.IO.Path.Combine(fixture.Native, "sample.exe");
        await File.WriteAllTextAsync(command, "synthetic", Token);
        var state = fixture.State(command) with
        { ChannelId = fixture.Npm().Id, PackageManager = fixture.Manager(), PlacedByOpenForge = false };
        var planned = await fixture.Service.PlanAsync(fixture.Request(fixture.Entry(fixture.Npm())) with
        {
            Existing = new()
            {
                ToolId = state.ToolId,
                Version = state.Version,
                ChannelId = state.ChannelId,
                CommandPath = state.CommandPath,
                Invocation = state.Invocation,
                RecordedState = state,
                PackageManager = state.PackageManager,
            },
        }, Token);
        Assert.NotNull(planned.Resolution);
        Assert.True(planned.Resolution.ReuseExisting);
        fixture.Processes.Requests.Clear();
        return planned.Resolution;
    }

    private static ToolGitHubReleaseChannel GitHub(ToolReleaseAsset asset) => new()
    {
        Id = "github-release",
        Kind = ToolChannelKind.GitHubRelease,
        Repository = "example/sample",
        Requirements = new() { Targets = [] },
        Assets = [asset]
    };
}
