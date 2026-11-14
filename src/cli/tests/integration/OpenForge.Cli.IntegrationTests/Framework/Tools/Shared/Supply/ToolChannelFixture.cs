using System.IO.Compression;
using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.PackageManagers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

internal sealed class ToolChannelFixture : IDisposable
{
    internal const string Id = "sample";
    internal const string Version = "2.4.3";
    internal const string RuntimeVersion = "24.1.0";
    internal TemporaryWorkspace Temporary { get; } = TemporaryWorkspace.Create("tool-channel");
    internal string Home { get; }
    internal string Workspace { get; }
    internal string Native { get; }
    internal FakeProcesses Processes { get; } = new();
    internal FakeRecords Records { get; } = new();
    internal FakeBackups Backups { get; } = new();
    internal FakeDownloads Downloads { get; } = new();
    internal ToolChannelService Service => new(Processes, Downloads, Backups, Records, new Dictionary<string, string?>());
    internal ToolTarget Target { get; } = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64, MinimumOperatingSystemVersion = "99.0" };

    internal ToolChannelFixture()
    {
        Home = Temporary.CreateDirectory("home");
        Workspace = Temporary.CreateDirectory("workspace");
        Native = Temporary.CreateDirectory("native");
    }

    internal ToolChannelRequest Request(ToolCatalogueEntry entry) => new()
    {
        Entry = entry,
        WorkspacePath = Workspace,
        HomePath = Home,
        Selection = new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = Version },
        Existing = new() { ToolId = Id },
        Target = Target,
        ProjectManagers = [],
        ManagerChoice = ToolManagerChoiceMode.Default,
        AllowLatestLookup = false,
        AvailableManagers = [],
        AvailableNodeRuntimes = [],
    };

    internal ToolCatalogueEntry Entry(params ToolChannel[] channels)
    {
        var setup = new ToolSetupSpec { Arguments = [], Environment = new Dictionary<string, string?>(), Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 4096, PossibleOutputs = [], Effects = [] };
        return new()
        {
            Identity = new() { Id = Id, Name = "Sample", Origin = ToolOrigin.ThirdParty, Licence = "MIT", Upstream = new("https://github.com/example/sample") },
            Channels = channels,
            Detection = new()
            {
                Command = Id,
                Arguments = ["--version"],
                Parser = ToolVersionParser.StableSemVer,
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 4096,
                Environment = new Dictionary<string, string?> { [ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable] = "1" }
            },
            Profiles = [new() { Id = "reviewed", Range = new(2, 4), TestedVersions = [Version], MinimumOpenForgeVersion = "0.0.0", MinimumToolVersion = Version,
                NativeHomePolicy = ToolNativeHomePolicy.Node, Setup = setup, Uninstall = setup, Changes = [] }],
        };
    }

    internal ToolReleaseAsset Archive(params string[] entries)
    {
        using var bytes = new MemoryStream();
        using (var zip = new ZipArchive(bytes, ZipArchiveMode.Create, leaveOpen: true))
            foreach (var name in entries)
            {
                using var member = zip.CreateEntry(name).Open();
                member.Write(new byte[] { 1, 2, 3 });
            }
        Downloads.Bytes = bytes.ToArray();
        return new()
        {
            Version = Version,
            Target = Target,
            Url = new($"https://{ToolArtifactSource.GitHubHost}/example/sample/releases/download/v{Version}/sample.zip"),
            Sha256 = Convert.ToHexStringLower(SHA256.HashData(Downloads.Bytes)),
            Archive = ToolArchiveKind.Zip,
            StripComponents = 0,
            EntryRelativePath = "sample.exe",
            MaximumBytes = 1024 * 1024
        };
    }

    internal ToolNpmChannel Npm() => new()
    {
        Id = "npm",
        Kind = ToolChannelKind.Npm,
        Package = "@example/sample",
        EntryRelativePath = "entry.js",
        MinimumClientVersions = new Dictionary<ToolNpmClient, string>(),
        Requirements = new() { Targets = [], Runtime = new(ToolNpmPackageReader.NodeRuntime, ">=20 <25") }
    };

    internal ToolPackageManagerFacts Manager(ToolNpmClient client = ToolNpmClient.Npm) => new()
    {
        Client = client,
        Version = "10.0.0",
        SupportsGlobalInstall = true,
        GlobalBinReadiness = ToolPathReadiness.Ready,
        GlobalBinPath = Native,
        RuntimePath = System.IO.Path.Combine(Native, "node.exe"),
        RuntimeVersion = RuntimeVersion,
        Invocation = new() { ExecutablePath = System.IO.Path.Combine(Native, "manager.exe"), PrefixArguments = [] },
    };

    internal static ToolProcessResult Success(string output = Version) => new() { ExitCode = 0, TimedOut = false, Cancelled = false, StandardOutput = output, StandardError = "" };
    internal ToolMachineState State(string command, string? runtime = null) => new()
    {
        SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
        ToolId = Id,
        Version = Version,
        ChannelId = "github-release",
        CommandPath = command,
        Invocation = new() { ExecutablePath = command, PrefixArguments = [] },
        RuntimePath = runtime,
        PlacedByOpenForge = true,
        Launchers = [],
        Integrity = [],
    };

    public void Dispose()
    {
        foreach (var owned in new[] { Home, Workspace, Native })
        {
            var root = new PhysicalPathResolver().ResolveRoot(Temporary.Path).GetContainedPhysicalPath();
            var path = new PhysicalPathResolver().ResolveCandidate(Temporary.Path, root, owned).GetContainedPhysicalPath();
            if (!PhysicalContainment.Contains(root, path) || root == path) throw new IOException("The test home escaped its temporary workspace.");
            Directory.Delete(path, recursive: true);
        }
        Temporary.Dispose();
    }

    internal sealed class FakeProcesses : IToolProcessHost
    {
        internal Dictionary<string, ToolResolvedCommand> Commands { get; } = new(StringComparer.Ordinal);
        internal List<ToolProcessRequest> Requests { get; } = [];
        internal Func<ToolProcessRequest, ToolProcessResult> Run { get; set; } = _ => Success();
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
            => Task.FromResult(Commands.GetValueOrDefault(command));
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken) { Requests.Add(request); return Task.FromResult(Run(request)); }
    }

    internal sealed class FakeDownloads : IToolDownloadHost
    {
        internal byte[] Bytes { get; set; } = [];
        internal int Calls { get; private set; }
        internal bool Fail { get; set; }
        internal List<ToolNpmMetadataRequest> MetadataRequests { get; } = [];
        internal Func<ToolNpmMetadataRequest, ToolNpmMetadataResult> Metadata { get; set; } = _ => throw new InvalidOperationException("Metadata wasn't expected.");
        public Task<ToolNpmMetadataResult> ReadNpmMetadataAsync(ToolNpmMetadataRequest request, CancellationToken cancellationToken)
        { MetadataRequests.Add(request); return Task.FromResult(Metadata(request)); }
        public async Task<ToolDownloadResult> DownloadAsync(ToolDownloadRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            if (Fail) return new(null, null, "synthetic transfer failure");
            await File.WriteAllBytesAsync(request.DestinationPath, Bytes, cancellationToken);
            return new(request.DestinationPath, new() { Kind = ToolVerificationKind.PinnedSha256, Source = request.Asset.Url.AbsoluteUri, Sha256 = request.Asset.Sha256 }, null);
        }
    }

    internal sealed class FakeBackups : IToolBackupService
    {
        internal int Prepared { get; private set; }
        internal int Completed { get; private set; }
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        { Prepared++; return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence }); }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
        { Completed++; return Task.FromResult(new ToolBackupCompletionResult(true, null, null)); }
    }

    internal sealed class FakeRecords : IToolRecordService
    {
        internal ToolRecordsResult Value { get; set; } = new()
        {
            PortableState = ToolRecordReadState.Missing,
            MachineState = ToolRecordReadState.Available,
            Machine = new Dictionary<string, ToolMachineState>(),
            WorkspaceMachineState = ToolRecordReadState.Missing,
            RuntimeRecordsState = ToolRecordReadState.Available,
            Runtimes = new Dictionary<string, ToolNodeRuntimeRecord>(),
            PathRecordState = ToolRecordReadState.Missing,
            Problems = [],
            ExpectedFileSha256 = new Dictionary<string, string?>(),
        };
        internal int Reads { get; private set; }
        public Task<ToolRecordsResult> ReadAsync(ToolRecordsRequest request, CancellationToken cancellationToken) { Reads++; return Task.FromResult(Value); }
        public Task<ToolRecordWriteResult> WriteAsync(ToolRecordWriteRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolRecordWriteResult> AddAllowInstallPathsAsync(ToolRecordGrantRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }
}
