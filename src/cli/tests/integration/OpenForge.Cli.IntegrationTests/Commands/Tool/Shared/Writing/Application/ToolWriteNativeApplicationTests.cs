using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application;

[Trait("Feature", "tool"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolWriteNativeApplicationTests
{
    private const string FileName = "synthetic-native.json";
    private static readonly byte[] Before = [1, 2, 3];
    private static readonly byte[] After = [4, 5, 6];

    [Fact(DisplayName = "Tool native application checks owned file expectations and retains backups until publication completes")]
    public async Task VerifiesOwnedNativeOutput()
    {
        using var workspace = TemporaryWorkspace.Create("tool-native-apply");
        var path = workspace.CreateFile(FileName, Before);
        var host = new Host(path);
        var backups = new Backups();
        var application = new ToolWriteNativeApplication(host, backups, new Setup());
        var state = new ToolWriteExecution(EmptyRecords());
        var plan = Plan(workspace.Path, path, Convert.ToHexStringLower(SHA256.HashData(Before)));

        Assert.True(await application.ApplyAsync(plan, state, CancellationToken.None));
        Assert.Equal(After, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        Assert.Equal(1, backups.Prepared);
        Assert.Equal(0, backups.Completed);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(After)), state.Backups[^1].ExpectedAppliedSha256);
        Assert.True(state.Backups[^1].Verified);

        await application.CompleteBackupsAsync(state, CancellationToken.None);
        Assert.Equal(1, backups.Completed);
        Assert.Equal(Convert.ToHexStringLower(SHA256.HashData(After)), Assert.Single(backups.Completions).ExpectedAppliedSha256);
    }

    [Fact(DisplayName = "Tool native stale-file preflight prevents backup and process effects")]
    public async Task StaleFilePreventsEffects()
    {
        using var workspace = TemporaryWorkspace.Create("tool-native-stale");
        var path = workspace.CreateFile(FileName, Before);
        var host = new Host(path);
        var backups = new Backups();
        var state = new ToolWriteExecution(EmptyRecords());
        var plan = Plan(workspace.Path, path, Convert.ToHexStringLower(SHA256.HashData(After)));

        Assert.False(await new ToolWriteNativeApplication(host, backups, new Setup()).ApplyAsync(plan, state, CancellationToken.None));
        Assert.Equal(0, host.Calls);
        Assert.Equal(0, backups.Prepared);
        Assert.Equal(Before, await File.ReadAllBytesAsync(path, TestContext.Current.CancellationToken));
        Assert.Equal(ToolWriteFindingCode.ExpectedStateChanged, Assert.Single(state.Findings).Code);
    }

    [Fact(DisplayName = "Tool native process failure retains original backup and reports failed output")]
    public async Task NativeFailureKeepsBackup()
    {
        using var workspace = TemporaryWorkspace.Create("tool-native-failure");
        var path = workspace.CreateFile(FileName, Before);
        var host = new Host(path) { Fail = true };
        var backups = new Backups();
        var state = new ToolWriteExecution(EmptyRecords());
        var plan = Plan(workspace.Path, path, Convert.ToHexStringLower(SHA256.HashData(Before)));

        Assert.False(await new ToolWriteNativeApplication(host, backups, new Setup()).ApplyAsync(plan, state, CancellationToken.None));
        Assert.True(state.Stopped);
        Assert.Equal(0, backups.Completed);
        Assert.False(Assert.Single(state.Backups).Verified);
        Assert.Equal(1, Assert.Single(state.Outcomes).Output?.ExitCode);
    }

    private static ToolWriteToolPlan Plan(string root, string path, string hash)
    {
        var invocation = new Core.Framework.Tools.Models.Launchers.ToolCommandTarget { ExecutablePath = Path.Combine(root, "synthetic-executable"), PrefixArguments = [] };
        var spec = new ToolSetupSpec
        {
            Arguments = ["synthetic-setup"],
            Environment = new Dictionary<string, string?>(),
            Effects = [],
            PossibleOutputs = [],
            Timeout = TimeSpan.FromSeconds(1),
            MaximumOutputBytes = 1024,
        };
        var request = new ToolSetupRequest
        {
            Setup = spec,
            Invocation = invocation,
            InstalledInvocation = invocation,
            Context = new()
            {
                WorkspacePath = root,
                HomePath = root,
                BinPath = root,
                NativeHomePath = root,
                NativeInstallDirectory = root,
                MainRoot = "synthetic-context",
                ClientRoots = new Dictionary<string, string>(),
            },
        };
        return new()
        {
            Entry = new()
            {
                Identity = new() { Id = "synthetic-tool", Name = "synthetic", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/synthetic"), Licence = "synthetic" },
                Channels = [],
                Profiles = [],
                Detection = new() { Command = "synthetic", Arguments = [], Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 1024, Environment = new Dictionary<string, string?>(), Parser = ToolVersionParser.StableSemVer },
            },
            Detection = new() { ToolId = "synthetic-tool" },
            Native = new()
            {
                Request = request,
                Command = new()
                {
                    ExecutablePath = invocation.ExecutablePath,
                    Arguments = spec.Arguments,
                    WorkingDirectory = root,
                    Environment = spec.Environment,
                    Timeout = spec.Timeout,
                    MaximumOutputBytes = spec.MaximumOutputBytes,
                },
                PreviewKind = ToolNativePreviewKind.Description,
                PossibleOutputs = [],
                ExpectedFileSha256 = new Dictionary<string, string?> { [path] = hash },
                Problems = [],
            },
        };
    }

    private static ToolRecordsResult EmptyRecords() => new()
    {
        PortableState = ToolRecordReadState.Missing,
        MachineState = ToolRecordReadState.Missing,
        WorkspaceMachineState = ToolRecordReadState.Missing,
        RuntimeRecordsState = ToolRecordReadState.Missing,
        PathRecordState = ToolRecordReadState.Missing,
        Machine = new Dictionary<string, ToolMachineState>(),
        Runtimes = new Dictionary<string, ToolNodeRuntimeRecord>(),
        Problems = [],
        ExpectedFileSha256 = new Dictionary<string, string?>(),
    };

    private sealed class Host(string path) : IToolProcessHost
    {
        internal int Calls { get; private set; }
        internal bool Fail { get; init; }
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken) => Task.FromResult<ToolResolvedCommand?>(null);
        public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            if (!Fail) await File.WriteAllBytesAsync(path, After, cancellationToken);
            return new() { ExitCode = Fail ? 1 : 0, TimedOut = false, Cancelled = false, StandardOutput = string.Empty, StandardError = string.Empty };
        }
    }

    private sealed class Backups : IToolBackupService
    {
        internal int Prepared { get; private set; }
        internal int Completed => Completions.Count;
        internal List<ToolBackupCompletionRequest> Completions { get; } = [];
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        {
            Prepared++;
            return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.Git });
        }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
        {
            Completions.Add(request);
            return Task.FromResult(new ToolBackupCompletionResult(true, null, null));
        }
    }

    private sealed class Setup : IToolSetupService
    {
        public Task<ToolSetupPlan> PlanAsync(ToolSetupRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }
}
