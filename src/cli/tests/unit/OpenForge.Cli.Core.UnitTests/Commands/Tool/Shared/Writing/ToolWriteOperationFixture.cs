using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

internal enum ToolWriteCall { Read, Grant, ChannelPlan, ChannelInstall, ConfigPlan, ConfigApply, ConfigRemovePlan, ConfigRemove, Native, Write, BackupComplete, OptionalPrompt, PlanPrompt, ManagerPrompt, SharedRemove, PathAdd, PathRemove, PathPrompt, NativePreparation }
internal sealed record ToolWriteCallFact(ToolWriteCall Call, string? Id = null);

// The other lanes implement these frozen services independently. These doubles exercise
// D's sequence and complete-result-set consumption without installing tools or claiming B/C evidence.
internal sealed class ToolWriteOperationFixture
{
    internal const string Id = "synthetic-tool";
    internal const string OtherId = "synthetic-z";
    internal const string ChannelId = "synthetic-channel";
    internal const string FirstChange = "synthetic-first";
    internal const string SecondChange = "synthetic-second";
    internal const string NativeChange = "synthetic-native";
    internal const string OldVersion = "1.2.3";
    internal const string LatestVersion = "1.2.4";
    internal static readonly ToolVersionRange Range = new(1, 2);
    internal static readonly string Digest = new('a', 64);
    internal List<ToolWriteCallFact> Calls { get; } = [];
    internal string Root { get; } = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "synthetic-tool-operation", Guid.NewGuid().ToString("N")));
    internal ToolCatalogue Catalogue { get; private set; }
    internal ToolWriteHostFacts Facts { get; set; }
    internal Action? OnPlanConfirmed { get; set; }
    internal bool PlacedByOpenForge { get; set; }
    internal ToolRecordsResult Records { get; set; }
    internal Dictionary<string, string?> DetectedVersions { get; } = new(StringComparer.Ordinal);
    internal List<ToolChannelRequest> ChannelRequests { get; } = [];
    internal List<ToolChannelResolution> ExecutedPlans { get; } = [];
    internal List<ToolChannelResolution> ResolvedPlans { get; } = [];
    internal List<ToolBackupCompletionRequest> IssuedBackups { get; } = [];
    internal List<ToolConfigPlan> ConfigCalls { get; } = [];
    internal List<ToolRecordWriteRequest> Writes { get; } = [];
    internal List<ToolRecordGrantRequest> GrantRequests { get; } = [];
    internal List<string> SavedGrants { get; } = [];
    internal List<ToolSetupRequest> SetupRequests { get; } = [];
    internal List<ToolConfigRequest> ConfigRequests { get; } = [];
    internal List<ToolBackupCompletionRequest> PathBackups { get; } = [];
    internal bool FailGrants { get; set; }
    internal bool CancelPlanning { get; set; }
    internal bool FailGrantRead { get; set; }
    private bool _incidentalTelemetry;
    private bool _accountUninstall;
    internal bool IncidentalTelemetry
    {
        get => _incidentalTelemetry;
        set { _incidentalTelemetry = value; SetUninstallEffects(); }
    }
    internal bool FailPath { get; set; }
    internal bool FailPathPublication { get; set; }
    internal bool FailBackupCompletion { get; set; }
    internal ToolConfigObservation? Observation { get; set; }
    internal List<ToolBackupCompletionRequest> CompletedBackups { get; } = [];
    internal List<ToolProcessRequest> Processes { get; } = [];
    internal List<ToolWriteConsentQuestion> OptionalQuestions { get; } = [];
    internal bool AccountUninstall
    {
        get => _accountUninstall;
        set { _accountUninstall = value; SetUninstallEffects(); }
    }
    internal bool FailNativePreparation { get; set; }
    internal bool FailPreflight { get; set; }
    internal bool FailChannel { get; set; }
    internal bool FailNative { get; set; }
    internal bool FailWrite { get; set; }
    internal int FailWriteAt { get; set; }
    internal bool AddPathChoice { get; set; }
    internal List<ToolPathSetupRequest> PathRequests { get; } = [];
    internal string? FailConfig { get; set; }
    internal bool ConflictRemoval { get; set; }
    internal bool MultipleManagers { get; set; }
    internal bool AcceptOptional { get; set; }
    internal bool AcceptPlan { get; set; } = true;
    internal bool Terminal { get; set; }
    internal VersionService Versions { get; } = new();

    internal ToolWriteOperationFixture(bool declared = true, bool locked = true, bool secondTool = false, string minimumOpenForgeVersion = "0.0.0")
    {
        Catalogue = new()
        {
            Entries = new Dictionary<string, ToolCatalogueEntry> { [Id] = Entry(Id, minimumOpenForgeVersion), [OtherId] = Entry(OtherId, minimumOpenForgeVersion) },
            NodeRuntimes = [],
        };
        var settings = new Dictionary<string, ToolSetting>();
        var locks = new Dictionary<string, ToolLockRecord>();
        if (declared) settings[Id] = new(Range);
        if (locked) locks[Id] = new() { Version = OldVersion, Channel = ChannelId, Changes = [] };
        if (secondTool)
        {
            settings[OtherId] = new(Range);
            locks[OtherId] = new() { Version = OldVersion, Channel = ChannelId, Changes = [] };
        }
        Records = new()
        {
            PortableState = ToolRecordReadState.Available,
            Portable = new() { Settings = settings, Lock = locks },
            MachineState = ToolRecordReadState.Missing,
            Machine = new Dictionary<string, ToolMachineState>(),
            WorkspaceMachineState = ToolRecordReadState.Missing,
            WorkspaceMachine = null,
            RuntimeRecordsState = ToolRecordReadState.Missing,
            Runtimes = new Dictionary<string, ToolNodeRuntimeRecord>(),
            PathRecordState = ToolRecordReadState.Missing,
            Path = null,
            Problems = [],
            ExpectedFileSha256 = new Dictionary<string, string?>(),
        };
        Facts = new()
        {
            HomePath = Path.Combine(Root, ToolRecordDefinitions.HomeDirectoryName),
            MainRoot = "synthetic-context",
            OpenForgeVersion = "1.0.0",
            NativeEnvironment = new Dictionary<string, string>(),
            NativeHomes = new Dictionary<ToolNativeHomePolicy, string>
            {
                [ToolNativeHomePolicy.Node] = Path.Combine(Root, "native-home"),
                [ToolNativeHomePolicy.HomeThenUserProfile] = Path.Combine(Root, "native-home"),
            },
            ClientRoots = new Dictionary<string, string>(),
            ProjectManagers = [],
            Target = new() { OperatingSystem = ToolOperatingSystem.Windows, Cpu = ToolCpu.X64 },
            Path = new() { BinPath = Path.Combine(Root, ToolRecordDefinitions.BinDirectoryName), Readiness = ToolPathReadiness.Ready, DetectedShellFiles = [] },
        };
    }

    internal ToolWriteRequest Request(ToolWriteKind kind = ToolWriteKind.Install, bool yes = true, bool preview = false, ToolSpec? spec = null, bool json = false)
        => new()
        {
            Kind = kind,
            Workspace = new(Root, Root, CliWorkspaceSelectionMethod.CurrentDirectory),
            Spec = spec ?? (kind == ToolWriteKind.Remove ? new(Id, new() { Kind = ToolVersionSelectionKind.LatestSupported }) : null),
            Yes = yes,
            CanPrompt = Terminal,
            Format = json ? CliFormat.Json : CliFormat.Text,
            Preview = preview,
        };

    internal ToolWriteOperation Create()
    {
        var channel = new ChannelService(this);
        var config = new ConfigService(this);
        var setup = new SetupService(this);
        var publisher = new ToolWriteRecordPublisher(new RecordService(this));
        var native = new ToolWriteNativeApplication(new ProcessHost(this), new BackupService(this), setup);
        return new(new(Catalogue, new(Versions, channel, config, setup)),
            new(new(channel, Versions, publisher), native, new(config, publisher), new(channel, new PathHost(this), publisher)),
            publisher, Facts, new()
            {
                CanPrompt = Terminal,
                ConfirmPlan = (_, _, _, _) =>
                {
                    Calls.Add(new(ToolWriteCall.PlanPrompt));
                    OnPlanConfirmed?.Invoke();
                    return ValueTask.FromResult(CliPromptReply<bool>.Answered(AcceptPlan));
                },
                ConfirmOptional = (question, _, _) =>
                {
                    Calls.Add(new(ToolWriteCall.OptionalPrompt));
                    OptionalQuestions.Add(question);
                    return ValueTask.FromResult(CliPromptReply<bool>.Answered(AcceptOptional));
                },
                ChooseManager = (_, _, _) =>
                {
                    Calls.Add(new(ToolWriteCall.ManagerPrompt));
                    return ValueTask.FromResult(CliPromptReply<int>.Answered((int)ToolNpmClient.Npm));
                },
                ChoosePath = (_, _, _) =>
                {
                    Calls.Add(new(ToolWriteCall.PathPrompt));
                    return ValueTask.FromResult(CliPromptReply<ToolWritePathAnswer>.Answered(new(AddPathChoice, [])));
                },
            });
    }

    internal void SeedChanges(params ToolAppliedChange[] changes)
    {
        var portable = Records.Portable ?? throw new InvalidOperationException();
        Records = Records with
        {
            Portable = portable with { Lock = new Dictionary<string, ToolLockRecord>(portable.Lock) { [Id] = portable.Lock[Id] with { Changes = changes } } },
        };
    }

    internal void SetUninstall(ToolSetupSpec uninstall)
    {
        var entry = Catalogue.Entries[Id];
        Catalogue = Catalogue with
        {
            Entries = new Dictionary<string, ToolCatalogueEntry>(Catalogue.Entries)
            {
                [Id] = entry with { Profiles = [.. entry.Profiles.Select(profile => profile with { Uninstall = uninstall })] },
            },
        };
    }

    private void SetUninstallEffects()
    {
        IReadOnlyList<ToolDeclaredEffect> effects = [];
        if (IncidentalTelemetry)
            effects = [new() { Id = "synthetic-telemetry", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Telemetry, Description = "Synthetic incidental telemetry." }];
        if (AccountUninstall)
            effects = [new() { Id = "synthetic-account", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.SharedFileDeletion, Description = "Synthetic account binary and all agent entries." }];
        Catalogue = Catalogue with
        {
            Entries = Catalogue.Entries.ToDictionary(pair => pair.Key, pair => pair.Value with
            {
                Profiles = [.. pair.Value.Profiles.Select(profile => profile with { Uninstall = profile.Uninstall with { Effects = effects } })],
            }, StringComparer.Ordinal),
        };
    }

    internal ToolBackupCompletionRequest PendingBackup(string name = "synthetic-record.json")
    {
        var backup = new ToolBackupCompletionRequest(new()
        {
            TargetPath = Path.Combine(Root, name),
            Kind = ToolBackupKind.Sibling,
            BackupPath = Path.Combine(Root, name + ".bak"),
        }, Calls.Count.ToString(System.Globalization.CultureInfo.InvariantCulture), true);
        IssuedBackups.Add(backup);
        return backup;
    }

    internal sealed class VersionService : IToolVersionService
    {
        public ToolSpec? ReadSpec(string value) => value switch
        {
            Id => new(Id, new() { Kind = ToolVersionSelectionKind.LatestSupported }),
            Id + "@1.2" => new(Id, new() { Kind = ToolVersionSelectionKind.MinorRange, Range = ToolWriteOperationFixture.Range }),
            Id + "@1.2.3" => new(Id, new() { Kind = ToolVersionSelectionKind.Exact, ExactVersion = OldVersion }),
            OtherId => new(OtherId, new() { Kind = ToolVersionSelectionKind.LatestSupported }),
            _ => null,
        };
        public ToolVersionRange? ReadRange(string value) => value == "1.2.x" ? ToolWriteOperationFixture.Range : null;
        public bool Satisfies(string version, ToolVersionSelection selection)
            => selection.Kind == ToolVersionSelectionKind.Exact ? version == selection.ExactVersion
                : (version == OldVersion || version == LatestVersion) && (selection.Range is null || selection.Range == ToolWriteOperationFixture.Range);
    }

    private sealed class ChannelService(ToolWriteOperationFixture fixture) : IToolChannelService
    {
        public Task<ToolDetectionResult> DetectAsync(ToolDetectionRequest request, CancellationToken cancellationToken)
        {
            var version = fixture.DetectedVersions.GetValueOrDefault(request.Entry.Identity.Id);
            return Task.FromResult(new ToolDetectionResult
            {
                ToolId = request.Entry.Identity.Id,
                Version = version,
                Invocation = version is null ? null : Target(fixture.Root),
                ChannelId = version is null ? null : ChannelId,
                CommandPath = version is null ? null : Target(fixture.Root).ExecutablePath,
                RecordedState = request.RecordedState,
            });
        }
        public Task<IReadOnlyList<ToolPackageManagerFacts>> DetectManagersAsync(string workspacePath, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<ToolPackageManagerFacts>>([]);
        public Task<ToolChannelPlanResult> PlanAsync(ToolChannelRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ChannelPlan, request.Entry.Identity.Id));
            if (fixture.CancelPlanning) throw new OperationCanceledException(cancellationToken);
            fixture.ChannelRequests.Add(request);
            if (fixture.FailPreflight && request.Entry.Identity.Id == OtherId)
                return Task.FromResult(new ToolChannelPlanResult(null, [], null, ["synthetic-blocker"], false));
            if (fixture.MultipleManagers && request.ManagerChoice == ToolManagerChoiceMode.Ask && request.ChosenManager is null)
                return Task.FromResult(new ToolChannelPlanResult(null, [Manager(ToolNpmClient.Npm), Manager(ToolNpmClient.Pnpm)], null, [], false));
            var version = request.Selection.ExactVersion ?? LatestVersion;
            var resolution = new ToolChannelResolution
            {
                ToolId = request.Entry.Identity.Id,
                Version = version,
                ChannelId = ChannelId,
                ReuseExisting = request.Existing.Version == version,
                Existing = request.Existing,
                Effects = [],
                Execution = new()
                {
                    HomePath = request.HomePath,
                    WorkspacePath = request.WorkspacePath,
                    BinPath = fixture.Facts.Path.BinPath,
                    NativeInstallDirectory = fixture.Facts.Path.BinPath,
                    CommandPath = Target(fixture.Root).ExecutablePath,
                    Invocation = Target(fixture.Root),
                    Downloads = [],
                    Extractions = [],
                    Commands = [],
                    Launchers = [],
                },
            };
            fixture.ResolvedPlans.Add(resolution);
            return Task.FromResult(new ToolChannelPlanResult(resolution, [], null, [], request.Selection.Kind != ToolVersionSelectionKind.Exact));
        }
        public Task<ToolChannelInstallResult> InstallAsync(ToolChannelInstallRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ChannelInstall, request.Plan.ToolId));
            fixture.ExecutedPlans.Add(request.Plan);
            if (fixture.FailChannel) return Task.FromResult(new ToolChannelInstallResult(null, [], "synthetic-failure") { PendingBackups = [], Runtimes = [] });
            fixture.DetectedVersions[request.Plan.ToolId] = request.Plan.Version;
            var state = new ToolMachineState
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                ToolId = request.Plan.ToolId,
                Version = request.Plan.Version,
                ChannelId = ChannelId,
                CommandPath = request.Plan.Execution.CommandPath,
                Invocation = request.Plan.Execution.Invocation,
                PlacedByOpenForge = fixture.PlacedByOpenForge,
                Launchers = [],
                Integrity = [],
            };
            return Task.FromResult(new ToolChannelInstallResult(state, [request.Plan.ToolId], null) { PendingBackups = [fixture.PendingBackup()], Runtimes = [] });
        }
        public Task<ToolManagedFilesRemoveResult> RemoveAsync(ToolManagedFilesRemoveRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.SharedRemove));
            return Task.FromResult(new ToolManagedFilesRemoveResult(request.ApprovedToolPaths, [], null));
        }
        private ToolPackageManagerFacts Manager(ToolNpmClient client)
            => new() { Client = client, Version = OldVersion, Invocation = Target(fixture.Root), GlobalBinReadiness = ToolPathReadiness.Ready, SupportsGlobalInstall = true };
    }

    private sealed class SetupService(ToolWriteOperationFixture fixture) : IToolSetupService
    {
        public Task<ToolSetupPlan> PlanAsync(ToolSetupRequest request, CancellationToken cancellationToken)
        {
            if (request.InstalledInvocation is not null) fixture.Calls.Add(new(ToolWriteCall.NativePreparation));
            fixture.SetupRequests.Add(request);
            return Task.FromResult(new ToolSetupPlan
            {
                Request = request,
                Command = new()
                {
                    ExecutablePath = request.Invocation.ExecutablePath,
                    Arguments = [.. request.Invocation.PrefixArguments, .. request.Setup.Arguments],
                    WorkingDirectory = request.Context.WorkspacePath,
                    Environment = request.Setup.Environment,
                    Timeout = request.Setup.Timeout,
                    MaximumOutputBytes = request.Setup.MaximumOutputBytes,
                },
                PreviewKind = request.InstalledInvocation is null ? ToolNativePreviewKind.Description : ToolNativePreviewKind.Plan,
                PossibleOutputs = [],
                ExpectedFileSha256 = new Dictionary<string, string?>(),
                Problems = fixture.FailNativePreparation && request.InstalledInvocation is not null ? ["synthetic-preparation-failure"] : [],
            });
        }
    }

    private sealed class ConfigService(ToolWriteOperationFixture fixture) : IToolConfigService
    {
        public string ComputeProfileDigest(ToolVersionProfile profile) => Digest;
        public Task<ToolConfigPlan> PlanAsync(ToolConfigRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ConfigPlan, request.ToolId));
            fixture.ConfigRequests.Add(request);
            IReadOnlyList<ToolConfigObservation> changes = [.. request.Changes.Select(change => new ToolConfigObservation
            {
                Change = change, Current = Value(false), Desired = Value(true),
                Disposition = request.AppliedChanges.Any(receipt => receipt.Id == change.Id) ? ToolChangeDisposition.NoChange : ToolChangeDisposition.Add,
            })];
            return Task.FromResult(new ToolConfigPlan(request, fixture.Observation is { } observation ? [observation] : changes, []));
        }
        public Task<ToolConfigApplyResult> ApplyAsync(ToolConfigPlan plan, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ConfigApply, plan.Changes[0].Change.Id));
            fixture.ConfigCalls.Add(plan);
            var id = plan.Changes[0].Change.Id;
            var failed = fixture.FailConfig == id;
            var additions = plan.Request.AppliedChanges.ToList();
            if (!failed && !additions.Any(receipt => receipt.Id == id)) additions.Add(new ToolManagedFileAddition { Id = id, Path = id, Sha256 = Digest });
            return Task.FromResult(new ToolConfigApplyResult
            {
                RemainingAdditions = additions,
                CompletedChanges = failed ? [] : [id],
                RemovedChanges = [],
                PendingBackups = [fixture.PendingBackup()],
                NativeSettings = plan.Request.NativeSettings,
                Unfinished = failed ? [id] : [],
                Failure = failed ? "synthetic-config-failure" : null,
            });
        }
        public Task<ToolConfigRemovePlan> PlanRemoveAsync(ToolConfigRemoveRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ConfigRemovePlan, request.ToolId));
            return Task.FromResult(new ToolConfigRemovePlan(request, [.. request.Lock.Changes.Select(addition => new ToolConfigRemovalObservation
            {
                Addition = addition, Current = Value(true), Desired = Value(false),
                Disposition = fixture.ConflictRemoval && addition.Id == FirstChange ? ToolChangeDisposition.Conflict : ToolChangeDisposition.Remove,
            })]));
        }
        public Task<ToolConfigRemoveResult> RemoveAsync(ToolConfigRemovePlan plan, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.ConfigRemove, plan.Changes[0].Addition.Id));
            var observation = plan.Changes[0];
            var conflict = observation.Disposition == ToolChangeDisposition.Conflict;
            return Task.FromResult(new ToolConfigRemoveResult
            {
                RemovedChanges = conflict ? [] : [observation.Addition.Id],
                RemainingAdditions = [.. plan.Request.Lock.Changes.Where(change => conflict || change.Id != observation.Addition.Id)],
                PendingBackups = [fixture.PendingBackup()],
                Unfinished = conflict ? [observation.Addition.Id] : [],
                KeptNativeSettings = plan.Request.NativeSettings,
            });
        }
    }

    private sealed class RecordService(ToolWriteOperationFixture fixture) : IToolRecordService
    {
        public Task<ToolRecordWriteResult> AddAllowInstallPathsAsync(ToolRecordGrantRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.Grant));
            fixture.GrantRequests.Add(request);
            Assert.Same(fixture.Records, request.Expected);
            if (!fixture.FailGrants)
            {
                fixture.SavedGrants.AddRange(request.AllowInstallPaths);
                fixture.Records = fixture.Records with
                {
                    ExpectedFileSha256 = new Dictionary<string, string?> { [Path.Combine(fixture.Root, "synthetic-settings.json")] = Digest },
                };
            }
            return Task.FromResult(new ToolRecordWriteResult(fixture.FailGrants ? [] : ["synthetic-settings.json"],
                fixture.FailGrants ? ["synthetic-settings.json"] : [], fixture.FailGrants ? "synthetic-grant-failure" : null)
            { PendingBackups = [fixture.PendingBackup()] });
        }

        public Task<ToolRecordsResult> ReadAsync(ToolRecordsRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.Read));
            if (fixture.FailGrantRead && fixture.GrantRequests.Count != 0) throw new IOException("synthetic-read-failure");
            return Task.FromResult(fixture.Records);
        }
        public Task<ToolRecordWriteResult> WriteAsync(ToolRecordWriteRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.Write));
            Assert.Same(fixture.Records, request.Expected);
            fixture.Writes.Add(request);
            if (fixture.FailWrite || fixture.FailWriteAt > 0 && fixture.Writes.Count >= fixture.FailWriteAt)
                return Task.FromResult(new ToolRecordWriteResult([], ["synthetic-write"], "synthetic-record-failure") { PendingBackups = [fixture.PendingBackup()] });
            fixture.Records = fixture.Records with
            {
                Portable = request.Portable,
                Machine = request.Machine.ToDictionary(value => value.ToolId),
                WorkspaceMachine = request.WorkspaceMachine,
                WorkspaceMachineState = request.WorkspaceMachine is null ? ToolRecordReadState.Missing : ToolRecordReadState.Available,
                Runtimes = request.Runtimes.ToDictionary(value => value.Version),
                Path = request.Path,
            };
            return Task.FromResult(new ToolRecordWriteResult(["synthetic-write"], [], null) { PendingBackups = [fixture.PendingBackup()] });
        }
    }

    private sealed class ProcessHost(ToolWriteOperationFixture fixture) : IToolProcessHost
    {
        public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
            => Task.FromResult<ToolResolvedCommand?>(new(Target(fixture.Root).ExecutablePath, Target(fixture.Root)));
        public Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.Native));
            fixture.Processes.Add(request);
            return Task.FromResult(new ToolProcessResult
            {
                ExitCode = fixture.FailNative ? 1 : 0,
                TimedOut = false,
                Cancelled = false,
                StandardOutput = string.Empty,
                StandardError = string.Empty,
            });
        }
    }

    private sealed class BackupService(ToolWriteOperationFixture fixture) : IToolBackupService
    {
        public Task<ToolBackupResult> PrepareAsync(ToolBackupRequest request, CancellationToken cancellationToken)
            => Task.FromResult(new ToolBackupResult { TargetPath = request.TargetPath, Kind = ToolBackupKind.PriorAbsence });
        public Task<ToolBackupCompletionResult> CompleteAsync(ToolBackupCompletionRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.BackupComplete));
            fixture.CompletedBackups.Add(request);
            return Task.FromResult(new ToolBackupCompletionResult(!fixture.FailBackupCompletion,
                fixture.FailBackupCompletion ? request.Backup.BackupPath : null, fixture.FailBackupCompletion ? "synthetic-backup-failure" : null));
        }
    }

    private sealed class PathHost(ToolWriteOperationFixture fixture) : IToolPathHost
    {
        public Task<ToolPathFacts> ObserveAsync(string homePath, CancellationToken cancellationToken) => Task.FromResult(fixture.Facts.Path);
        public Task<ToolPathSetupResult> AddAsync(ToolPathSetupRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.PathAdd));
            fixture.PathRequests.Add(request);
            if (request.Choice == ToolPathChoice.Manual)
                return Task.FromResult(new ToolPathSetupResult(null, ["synthetic manual path instruction"], false, null) { PendingBackups = [] });
            var backup = fixture.PendingBackup("synthetic-profile");
            fixture.PathBackups.Add(backup);
            if (fixture.FailPathPublication) fixture.FailWrite = true;
            return Task.FromResult(new ToolPathSetupResult(new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                BinPath = request.Facts.BinPath,
                Changes = [new()
                {
                    Kind = ToolPathChangeKind.ShellBlock, Target = Path.Combine(fixture.Root, "synthetic-profile"),
                    Prior = Value(false), Applied = Value(true), StartMarker = "synthetic-start", EndMarker = "synthetic-end",
                }],
            }, [], true, fixture.FailPath ? "synthetic-path-failure" : null)
            { PendingBackups = [backup] });
        }
        public Task<ToolPathRemoveResult> RemoveAsync(ToolPathRemoveRequest request, CancellationToken cancellationToken)
        {
            fixture.Calls.Add(new(ToolWriteCall.PathRemove));
            var backup = fixture.PendingBackup("synthetic-profile");
            fixture.PathBackups.Add(backup);
            if (fixture.FailPathPublication) fixture.FailWrite = true;
            return Task.FromResult(new ToolPathRemoveResult([.. request.Expected.Changes.Select(change => change.Target)], [], true,
                fixture.FailPath ? "synthetic-path-failure" : null)
            { PendingBackups = [backup] });
        }
    }

    private static ToolRecordedValue Value(bool present) => new() { Present = present, Kind = ToolRecordedValueKind.Bytes, Bytes = [] };
    private static ToolCommandTarget Target(string root) => new() { ExecutablePath = Path.Combine(root, "synthetic-executable"), PrefixArguments = ["synthetic-prefix"] };
    private static ToolCatalogueEntry Entry(string id, string minimumOpenForgeVersion)
        => new()
        {
            Identity = new() { Id = id, Name = id, Origin = ToolOrigin.ThirdParty, Upstream = new("https://example.invalid/synthetic"), Licence = "synthetic" },
            Channels = [new ToolNpmChannel
            {
                Id = ChannelId, Kind = ToolChannelKind.Npm, Package = id, EntryRelativePath = "synthetic-entry.js", Requirements = new() { Targets = [] }, MinimumClientVersions = new Dictionary<ToolNpmClient, string>(),
            }],
            Detection = new()
            {
                Command = id,
                Arguments = [],
                Timeout = TimeSpan.FromSeconds(1),
                MaximumOutputBytes = 1024,
                Environment = new Dictionary<string, string?>(),
                Parser = ToolVersionParser.StableSemVer,
            },
            Profiles = [new()
            {
                Id = "synthetic-profile", Range = Range, MinimumToolVersion = OldVersion, MinimumOpenForgeVersion = minimumOpenForgeVersion,
                TestedVersions = [OldVersion, LatestVersion], NativeHomePolicy = ToolNativeHomePolicy.Node,
                Setup = Setup("install"), Uninstall = Setup("uninstall"),
                Changes = [Change(FirstChange), Change(SecondChange)],
            }],
        };
    private static ToolSetupSpec Setup(string verb)
        => new()
        {
            Arguments = [verb],
            Environment = new Dictionary<string, string?>(),
            Effects = [],
            PossibleOutputs = [],
            Timeout = TimeSpan.FromSeconds(1),
            MaximumOutputBytes = 1024,
        };
    private static ToolManagedFileChange Change(string id)
        => new()
        {
            Id = id,
            Description = id,
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            PathTemplate = id,
            TemplateResource = id,
        };
}
