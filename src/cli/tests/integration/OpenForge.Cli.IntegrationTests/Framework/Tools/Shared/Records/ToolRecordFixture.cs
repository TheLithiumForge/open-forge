using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

internal sealed class ToolRecordFixture : IDisposable
{
    internal const string Id = "synthetic";
    internal const string InitialVersion = "3.2.1";
    internal const string ChangedVersion = "3.2.2";
    internal const string UnsupportedVersion = "4.0.0";
    internal const string Channel = "synthetic-channel";
    internal TemporaryWorkspace Workspace { get; } = TemporaryWorkspace.Create("tool-records");
    internal TemporaryWorkspace Home { get; } = TemporaryWorkspace.Create("tool-home");
    internal string PhysicalWorkspace => new PhysicalPathResolver().ResolveRoot(Workspace.Path).GetContainedPhysicalPath();
    internal string OpenForgeHome => Home.Combine(ToolRecordDefinitions.HomeDirectoryName);
    internal ToolRecordsRequest Locations => new(Workspace.Path, OpenForgeHome);
    internal ToolRecordPaths Paths => new(Workspace.Path, OpenForgeHome);
    internal BackupHost Backups { get; } = new();
    internal ToolCatalogue Catalogue { get; }
    internal ToolRecordService Service { get; }
    internal string CommandPath => Home.Combine("synthetic-command");
    internal string PortablePath(string relative) => Path.GetFullPath(Path.Combine(PhysicalWorkspace, relative));

    internal ToolRecordFixture()
    {
        var profile = new ToolVersionProfile
        {
            Id = "synthetic-profile",
            Range = new(3, 2),
            TestedVersions = [InitialVersion],
            MinimumOpenForgeVersion = "0.1.0",
            MinimumToolVersion = InitialVersion,
            NativeHomePolicy = ToolNativeHomePolicy.Node,
            Setup = Setup(),
            Uninstall = Setup(),
            Changes = [],
        };
        var entry = new ToolCatalogueEntry
        {
            Identity = new() { Id = Id, Name = "Synthetic tool", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/tool"), Licence = "synthetic" },
            Channels = [],
            Profiles = [profile],
            Detection = new()
            {
                Command = Id,
                Arguments = ["version"],
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 128,
                Environment = new Dictionary<string, string?> { ["SYNTHETIC_NO_DOWNLOAD"] = "1" },
                Parser = ToolVersionParser.StableSemVer
            },
        };
        Catalogue = new() { Entries = new Dictionary<string, ToolCatalogueEntry> { [Id] = entry }, NodeRuntimes = [] };
        Service = new(Catalogue, new Versions(), Backups);
    }

    internal ToolMachineState Machine() => new()
    {
        SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
        ToolId = Id,
        Version = InitialVersion,
        ChannelId = Channel,
        CommandPath = CommandPath,
        Invocation = new() { ExecutablePath = CommandPath, PrefixArguments = [] },
        PlacedByOpenForge = true,
        Launchers = [],
        Integrity = [],
    };

    internal ToolRecordWriteRequest Write(ToolRecordsResult expected) => new()
    {
        Locations = Locations,
        Expected = expected,
        Portable = new()
        {
            Settings = new Dictionary<string, ToolSetting> { [Id] = new(new(3, 2)) },
            Lock = new Dictionary<string, ToolLockRecord> { [Id] = new() { Version = InitialVersion, Channel = Channel, Changes = [] } }
        },
        Machine = [Machine()],
        Runtimes = [],
    };

    internal static ToolSetupSpec Setup() => new() { Arguments = ["synthetic-setup"], Environment = new Dictionary<string, string?>(), Effects = [], Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 128, PossibleOutputs = [] };

    public void Dispose()
    {
        ClearProductFiles(Home, ToolRecordDefinitions.HomeDirectoryName);
        ClearProductFiles(Workspace, WorkspaceSettingsDefinitions.DirectoryName);
        Home.Dispose();
        Workspace.Dispose();
    }

    private static void ClearProductFiles(TemporaryWorkspace owned, string relative)
    {
        var root = new PhysicalPathResolver().ResolveRoot(owned.Path).GetContainedPhysicalPath();
        var path = Path.Combine(root, relative);
        ToolRecordPaths.Check(root, path);
        if (!Directory.Exists(path)) return;
        Clear(root, path);
    }

    private static void Clear(string root, string directory)
    {
        ToolRecordPaths.Check(root, directory);
        foreach (var entry in Directory.EnumerateFileSystemEntries(directory))
        {
            ToolRecordPaths.Check(root, entry);
            if (Directory.Exists(entry)) Clear(root, entry);
            else File.Delete(entry);
        }
        Directory.Delete(directory, recursive: false);
    }

    internal sealed class Versions : IToolVersionService
    {
        public ToolSpec? ReadSpec(string value)
        {
            var version = value.Split('@').Last();
            return version is InitialVersion or ChangedVersion or UnsupportedVersion ? new("record", new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = version }) : null;
        }
        public ToolVersionRange? ReadRange(string value) => value == "3.2.x" ? new(3, 2) : null;
        public bool Satisfies(string version, ToolVersionSelection selection) => version is InitialVersion or ChangedVersion;
    }

    internal sealed class BackupHost : IToolBackupService
    {
        internal int FailOnCall { get; init; }
        internal int Calls { get; private set; }
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
        {
            Calls++;
            if (Calls == FailOnCall) return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence, Failure = "synthetic backup failure" });
            return Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence });
        }
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException("Record writes leave completion to the command.");
    }
}
