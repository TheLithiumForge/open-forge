using System.Security.Cryptography;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application;

internal sealed class ToolWriteRemovalScenario : IDisposable
{
    internal const string Id = "synthetic-removal";
    internal const string Version = "1.2.3";
    internal const string ProjectFile = "synthetic-addition.txt";
    internal const string Uninstall = "synthetic-uninstall";
    internal const string Preview = "synthetic-preview";
    internal const string IndependentClient = "independent";
    internal const string UnavailableClient = "unavailable";
    internal static readonly byte[] Before = [1, 2, 3];
    internal static readonly byte[] After = [4, 5, 6];
    internal TemporaryWorkspace Files { get; } = TemporaryWorkspace.Create("tool-deferred-removal");
    internal string Home { get; }
    internal string NativeHome { get; }
    internal string NativeOutput { get; }
    internal ToolCommandTarget Invocation { get; }
    internal ToolCatalogueEntry Entry { get; }
    internal ToolCatalogue Catalogue { get; }
    internal ToolWriteHostFacts Facts { get; }
    internal ToolConfigService Config { get; }
    internal ToolBackupService Backups { get; }
    internal ToolRecordService Records { get; }
    internal ToolWriteToolPlanner Planner { get; }
    internal ToolWriteNativeApplication Native { get; }
    internal Host Processes { get; }
    internal List<string> Events { get; } = [];
    internal List<ToolWriteConsentQuestion> Questions { get; } = [];
    internal bool Terminal { get; set; }
    internal bool AcceptAccount { get; set; }
    internal ToolRecordsRequest Locations => new(Files.Path, Home);

    internal ToolWriteRemovalScenario(bool missingClient = false)
    {
        Home = Files.Combine(ToolRecordDefinitions.HomeDirectoryName);
        NativeHome = Files.Combine("native-home");
        NativeOutput = Files.CreateFile(Path.Combine("native-home", "account.json"), Before);
        var independent = Files.Combine("independent-client");
        Directory.CreateDirectory(independent);
        Invocation = new() { ExecutablePath = Path.Combine(NativeHome, "synthetic-command"), PrefixArguments = [] };
        var uninstall = new ToolSetupSpec
        {
            Arguments = [Uninstall, ToolRecipeDefinitions.NativeInstallDirectoryToken],
            DryRunArguments = [Uninstall, Preview, ToolRecipeDefinitions.NativeInstallDirectoryToken],
            Environment = new Dictionary<string, string?>(),
            Timeout = TimeSpan.FromSeconds(1),
            MaximumOutputBytes = 1024,
            Effects = [
                new() { Id = "synthetic-clients", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Configuration, Description = "Synthetic account entries." },
                new() { Id = "synthetic-binary", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.SharedFileDeletion, Description = "Synthetic account binary and all agent entries." },
                new() { Id = "synthetic-path", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.PathChange, Description = "Synthetic native persistent user PATH removal." }],
            PossibleOutputs = [
                Output(ToolRecipeDefinitions.NativeHomeToken + "/account.json"),
                Output(ToolRecipeDefinitions.ClientTokenPrefix + IndependentClient + "}/settings.json") with { ClientId = IndependentClient },
                .. missingClient ? new[] { Output(ToolRecipeDefinitions.ClientTokenPrefix + UnavailableClient + "}/settings.json") with { ClientId = UnavailableClient } } : []],
        };
        Entry = new()
        {
            Identity = new() { Id = Id, Name = "Synthetic removal", Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/removal"), Licence = "synthetic" },
            Channels = [],
            Profiles = [new()
            {
                Id = "synthetic-profile", Range = new(1, 2), MinimumToolVersion = Version, MinimumOpenForgeVersion = "0.0.0",
                TestedVersions = [Version], NativeHomePolicy = ToolNativeHomePolicy.Node, Setup = uninstall, Uninstall = uninstall, Changes = [],
            }],
            Detection = new() { Command = Id, Arguments = [], Environment = new Dictionary<string, string?>(), Timeout = TimeSpan.FromSeconds(1), MaximumOutputBytes = 1024, Parser = ToolVersionParser.StableSemVer },
        };
        Catalogue = new() { Entries = new Dictionary<string, ToolCatalogueEntry> { [Id] = Entry }, NodeRuntimes = [] };
        Facts = new()
        {
            HomePath = Home,
            MainRoot = "synthetic-context",
            OpenForgeVersion = "1.0.0",
            NativeEnvironment = new Dictionary<string, string>(),
            NativeHomes = new Dictionary<ToolNativeHomePolicy, string> { [ToolNativeHomePolicy.Node] = NativeHome },
            ClientRoots = new Dictionary<string, string> { [IndependentClient] = independent },
            ProjectManagers = [],
            Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
            Path = new() { BinPath = Files.Combine(ToolRecordDefinitions.BinDirectoryName), Readiness = ToolPathReadiness.Ready, DetectedShellFiles = [] },
        };
        Processes = new(this);
        Backups = new(Processes);
        Config = new(Processes, Backups, new ToolNativePlanReader());
        var versions = new ToolVersionService();
        Records = new(Catalogue, versions, Backups);
        Planner = new(versions, new Channels(this), Config, Config);
        Native = new(Processes, Backups, Config);
    }

    internal async Task SeedAsync()
    {
        Files.CreateFile(ProjectFile, Before);
        var expected = await Records.ReadAsync(Locations, TestContext.Current.CancellationToken);
        var written = await Records.WriteAsync(new()
        {
            Locations = Locations,
            Expected = expected,
            Machine = [],
            Runtimes = [],
            Portable = new()
            {
                Settings = new Dictionary<string, ToolSetting> { [Id] = new(new(1, 2)) },
                Lock = new Dictionary<string, ToolLockRecord>
                {
                    [Id] = new() { Version = Version, Channel = "synthetic-channel", Changes = [new ToolManagedFileAddition { Id = ProjectFile, Path = ProjectFile, Sha256 = Hash(Before) }] },
                },
            },
        }, TestContext.Current.CancellationToken);
        Assert.Null(written.Failure);
        foreach (var backup in written.PendingBackups)
            Assert.Null((await Backups.CompleteAsync(backup, TestContext.Current.CancellationToken)).Failure);
    }

    internal ToolWriteRequest Request(bool yes = true, bool preview = false, bool json = false) => new()
    {
        Kind = ToolWriteKind.Remove,
        Workspace = new(Files.Path, Files.Path, CliWorkspaceSelectionMethod.CurrentDirectory),
        Spec = new(Id, new() { Kind = ToolVersionSelectionKind.LatestSupported }),
        Yes = yes,
        CanPrompt = Terminal,
        Preview = preview,
        Format = json ? CliFormat.Json : CliFormat.Text,
    };

    internal ToolWriteOperation Writer()
    {
        var channels = new Channels(this);
        var publisher = new ToolWriteRecordPublisher(Records);
        return new(new(Catalogue, Planner), new(new(channels, new ToolVersionService(), publisher), Native,
            new(Config, publisher), new(channels, new PathHost(), publisher)), publisher, Facts, new()
            {
                CanPrompt = Terminal,
                ConfirmPlan = (_, _, _, _) => ValueTask.FromResult(CliPromptReply<bool>.Answered(true)),
                ConfirmOptional = (question, _, _) =>
                {
                    Events.Add("account-choice");
                    Questions.Add(question);
                    Processes.AllowNative = AcceptAccount;
                    return ValueTask.FromResult(CliPromptReply<bool>.Answered(AcceptAccount));
                },
                ChooseManager = (_, _, _) => throw new InvalidOperationException(),
                ChoosePath = (_, _, _) => throw new InvalidOperationException(),
            });
    }

    internal async Task<ToolWriteToolPlan> PlanAsync()
        => await Planner.PlanRemoveAsync(Entry, new(Request(), await Records.ReadAsync(Locations, TestContext.Current.CancellationToken), Facts,
            new Dictionary<string, ToolNpmClient>()), TestContext.Current.CancellationToken);

    internal static string Hash(byte[] bytes) => Convert.ToHexStringLower(SHA256.HashData(bytes));
    private static ToolNativeOutputSpec Output(string path) => new()
    {
        PathTemplate = path,
        Scope = ToolEffectScope.Account,
        Match = ToolNativeOutputMatch.Exact,
        Purpose = ToolNativeOutputPurpose.Backup,
    };
    public void Dispose()
    {
        // Product-created records aren't registered fixture files. Verify the owned root before removing that tree.
        _ = Files.SnapshotHashes(includeRuntime: true);
        Directory.Delete(Files.Path, recursive: true);
    }

    internal sealed class Host(ToolWriteRemovalScenario scenario) : IToolProcessHost
    {
        internal bool AllowNative { get; set; }
        internal bool FailUninstall { get; set; }
        internal List<ToolProcessRequest> Previews { get; } = [];
        internal List<ToolProcessRequest> Uninstalls { get; } = [];
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
            => Task.FromResult<ToolResolvedCommand?>(null);
        public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        {
            if (!AllowNative) throw new InvalidOperationException("An unchosen native process was requested.");
            Assert.Equal(scenario.Invocation.ExecutablePath, request.ExecutablePath);
            Assert.Equal(Uninstall, request.Arguments[0]);
            if (request.Arguments.Contains(Preview, StringComparer.Ordinal))
            {
                scenario.Events.Add("preview");
                Previews.Add(request);
            }
            else
            {
                scenario.Events.Add("native");
                Uninstalls.Add(request);
                Assert.True(File.Exists(scenario.Files.Combine(ProjectFile)));
                if (!FailUninstall) await File.WriteAllBytesAsync(scenario.NativeOutput, After, cancellationToken);
            }
            return new()
            {
                ExitCode = FailUninstall && !request.Arguments.Contains(Preview, StringComparer.Ordinal) ? 1 : 0,
                TimedOut = false,
                Cancelled = false,
                StandardOutput = string.Empty,
                StandardError = string.Empty
            };
        }
    }

    private sealed class Channels(ToolWriteRemovalScenario scenario) : IToolChannelService
    {
        public Task<ToolDetectionResult> DetectAsync(ToolDetectionRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new ToolDetectionResult { ToolId = Id, Version = Version, Invocation = scenario.Invocation, CommandPath = scenario.Invocation.ExecutablePath });
        public Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspacePath, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolChannelInstallResult> InstallAsync(ToolChannelInstallRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }

    private sealed class PathHost : IToolPathHost
    {
        public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
        public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken) => throw new InvalidOperationException();
    }
}
