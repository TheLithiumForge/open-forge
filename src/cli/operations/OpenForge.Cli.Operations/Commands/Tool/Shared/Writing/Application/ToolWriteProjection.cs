using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;

internal static class ToolWriteProjection
{
    internal static ToolWriteResult Result(ToolWritePlan plan, ToolWriteExecution state)
    {
        var findings = plan.Findings.Concat(state.Findings).ToArray();
        var status = Status(findings);
        var tools = plan.Tools.Select(tool => View(tool, plan, state)).ToArray();
        var outcomes = state.Outcomes.ToList();
        foreach (var tool in plan.Tools.Where(tool => !tool.NoOp))
        {
            List<string> stages = [ToolWriteStages.Config];
            if (tool.Channel is not null) stages.Insert(0, ToolWriteStages.Channel);
            if (tool.Native is not null) stages.Insert(stages.Count - 1, ToolWriteStages.Native);
            foreach (var stage in stages)
                if (!outcomes.Any(outcome => outcome.ToolId == tool.Entry.Identity.Id && outcome.Stage == stage))
                    outcomes.Add(new(tool.Entry.Identity.Id, stage, ToolWriteStages.NotStarted, [], []));
        }
        return new()
        {
            Kind = plan.Input.Request.Kind,
            Status = status,
            Workspace = plan.Input.Request.Workspace,
            WorkspacePath = plan.Input.Request.Workspace.LexicalRoot,
            WorkspaceExplicit = plan.Input.Request.Workspace.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace,
            Findings = findings,
            Data = new()
            {
                Preview = plan.Input.Request.Preview,
                Tools = tools,
                Outcomes = outcomes,
                Grants = state.Grants is { } grants ? [grants] : [],
                Plan = new()
                {
                    UsedNetwork = plan.UsedNetwork,
                    BinPath = plan.Input.Facts.Path.BinPath,
                    PathReadiness = Readiness(plan.Input.Facts.Path.Readiness),
                    PathInstructions = [.. state.PathInstructions],
                    Backups = [.. state.Backups.GroupBy(backup => backup.Backup.TargetPath,
                        Core.Framework.Filesystem.PhysicalPaths.PhysicalIdentityTracker.PathComparer)
                        .Select(group => new ToolWriteBackupView(group.Key, group.First().Backup.BackupPath,
                            group.Last().ExpectedAppliedSha256, group.Last().Verified, group.First().Backup.GitObjectId))],
                },
            },
        };
    }

    internal static CliSemanticStatus Status(IEnumerable<ToolWriteFinding> findings)
    {
        var values = findings.Select(finding => finding.Status).ToHashSet();
        foreach (var status in new[]
        {
            CliSemanticStatus.Interrupted, CliSemanticStatus.Invalid, CliSemanticStatus.Blocked,
            CliSemanticStatus.Failed, CliSemanticStatus.Incomplete, CliSemanticStatus.Attention,
        })
            if (values.Contains(status)) return status;
        return CliSemanticStatus.Complete;
    }

    internal static ToolWriteCommandView Command(string purpose, ToolProcessRequest command)
        => new(purpose, command.ExecutablePath, command.Arguments, command.WorkingDirectory, command.Environment,
            command.Timeout.TotalSeconds, command.MaximumOutputBytes);

    internal static ToolWriteEffectView Effect(ToolDeclaredEffect effect)
        => new(effect.Id, effect.Scope.ToString(), effect.Kind.ToString(), effect.Description, effect.PathTemplate);

    private static ToolWriteToolView View(ToolWriteToolPlan tool, ToolWritePlan plan, ToolWriteExecution state)
    {
        var id = tool.Entry.Identity.Id;
        var commands = new List<ToolWriteCommandView>();
        var execution = tool.Channel?.Execution;
        var recorded = plan.Input.Records.Machine.GetValueOrDefault(id);
        foreach (var command in execution?.Commands ?? []) commands.Add(Command(ToolWriteStages.Channel, command));
        if (tool.Native is { } native) commands.Add(Command(ToolWriteStages.Native, native.Command));
        commands.AddRange(state.ManualCommands.Where(command => command.ToolId == id).Select(command => command.Command));
        var kept = plan.Input.Request.Kind == ToolWriteKind.Remove
            ? (state.Records.WorkspaceMachine?.NativeSettings ?? []).Where(setting => setting.ToolId == id).ToArray()
            : [];
        foreach (var setting in kept)
        {
            if (tool.Detection.Invocation is { } invocation)
                commands.Add(new("manual-native-setting", invocation.ExecutablePath,
                    [.. invocation.PrefixArguments, .. setting.ManualRestoreArguments], plan.Input.Request.Workspace.LexicalRoot,
                    new Dictionary<string, string?>(), 0, 0));
        }
        if (plan.Input.Request.Kind == ToolWriteKind.Remove) AddPackageUninstall(tool, plan, commands);
        return new()
        {
            Id = id,
            OldVersion = tool.Detection.Version,
            Version = tool.Channel?.Version ?? plan.Input.Records.Portable?.Lock.GetValueOrDefault(id)?.Version,
            Channel = tool.Channel?.ChannelId ?? tool.Detection.ChannelId,
            Manager = tool.Channel?.PackageManager?.Client.ToString(),
            ManagerGlobalBin = tool.Channel?.PackageManager?.GlobalBinPath,
            ManagerPathReadiness = tool.Channel?.PackageManager is { } manager ? Readiness(manager.GlobalBinReadiness) : null,
            ReuseExisting = tool.Channel?.ReuseExisting == true,
            NoOp = tool.NoOp,
            Profile = tool.Profile?.Id,
            ProfileDigest = tool.Digest,
            AccountReplacement = tool.Detection.Version is not null && tool.Channel is { } target && target.Version != tool.Detection.Version,
            NativeHome = tool.Context?.NativeHomePath,
            NativePreview = tool.Native?.PreviewKind.ToString(),
            Commands = commands,
            Effects = [.. (tool.Channel?.Effects ?? []).Concat(tool.Native?.Request.Setup.Effects ?? []).Select(effect =>
                new ToolWriteEffectView(effect.Id, effect.Scope.ToString(), effect.Kind.ToString(), effect.Description, effect.PathTemplate))],
            Changes = [.. (tool.Config?.Changes ?? []).Select(change => Change(change)),
                .. (tool.Config?.Removals ?? tool.Removal?.Changes ?? []).Select(change =>
                    new ToolWriteChangeView(change.Addition.Id, AdditionKind(change.Addition), AdditionPath(change.Addition),
                        null, change.Disposition.ToString(), change.Conflict))],
            Downloads = [.. (execution?.Downloads ?? []).Select(download =>
                new ToolWriteDownloadView(download.Asset.Url.AbsoluteUri, download.DestinationPath, download.Asset.Sha256))],
            Paths = [.. new[] { execution?.CommandPath ?? recorded?.CommandPath, execution?.ToolPath ?? recorded?.ToolPath,
                    execution?.RuntimePath ?? recorded?.RuntimePath, execution?.NativeInstallDirectory }
                .OfType<string>().Concat((execution?.Launchers ?? recorded?.Launchers ?? []).Select(launcher => launcher.LauncherPath))],
            Backups = [.. (tool.Native?.ExpectedFileSha256.Keys ?? []).Concat(state.Backups.Select(backup => backup.Backup.BackupPath).OfType<string>()).Distinct(StringComparer.Ordinal)],
            Requirements = [.. tool.Entry.Channels.Select(channel => channel.Requirements.Runtime)
                .OfType<ToolRuntimeRequirement>().Select(runtime => string.Concat(runtime.Runtime, " ", runtime.VersionRange))],
            KeptNativeSettings = [.. kept.Select(setting => new ToolWriteNativeSettingView(
                setting.ChangeId, setting.NativeHomePath, setting.Applied.Native, setting.ManualRestoreArguments))],
            MinimumOpenForgeVersion = tool.Profile?.MinimumOpenForgeVersion,
            MinimumToolVersion = tool.Profile?.MinimumToolVersion,
            Targets = [.. tool.Entry.Channels.Where(channel => channel.Id == (tool.Channel?.ChannelId ?? tool.Detection.ChannelId))
                .SelectMany(channel => channel.Requirements.Targets).Select(target => new ToolWriteTargetView(
                    target.OperatingSystem.ToString(), target.Cpu.ToString(), target.LinuxLibc?.ToString(),
                    target.MinimumOperatingSystemVersion, target.MinimumLibcVersion, target.MinimumLinuxKernelVersion, target.MinimumLibStdCppVersion))],
            PossibleOutputs = [.. (tool.Native?.PossibleOutputs ?? []).Select(output => new ToolWriteNativeOutputView(
                output.PathPattern, output.Scope.ToString(), output.Match.ToString(), output.Purpose.ToString(),
                tool.Native?.DetectedClients is null))],
        };
    }

    private static ToolWriteChangeView Change(ToolConfigObservation observation)
        => observation.Change switch
        {
            ToolJsonValueChange change => new(change.Id, change.Operation.ToString(), change.PathTemplate, change.Pointer, observation.Disposition.ToString(), change.Description),
            ToolMarkedBlockChange change => new(change.Id, ToolRecordDefinitions.MarkedBlockKind, change.PathTemplate, null, observation.Disposition.ToString(), change.Description),
            ToolManagedFileChange change => new(change.Id, ToolRecordDefinitions.ManagedFileKind, change.PathTemplate, null, observation.Disposition.ToString(), change.Description),
            ToolNativeConfigChange change => new(change.Id, ToolRecordDefinitions.NativeCommandKind, null, null, observation.Disposition.ToString(), change.Description)
            {
                StoredNativeValue = observation.Current.Native,
                NativeEnvironmentOverrides = observation.NativeEnvironmentOverrides,
            },
            _ => throw new ArgumentOutOfRangeException(nameof(observation)),
        };

    private static string? AdditionPath(ToolAppliedChange change) => change switch
    {
        ToolJsonUnionAddition addition => addition.Path,
        ToolJsonValueAddition addition => addition.Path,
        ToolManagedFileAddition addition => addition.Path,
        ToolMarkedBlockAddition addition => addition.Path,
        ToolNativeCommandAddition => null,
        _ => throw new ArgumentOutOfRangeException(nameof(change)),
    };

    private static string AdditionKind(ToolAppliedChange change) => change switch
    {
        ToolJsonUnionAddition => ToolRecordDefinitions.JsonUnionKind,
        ToolJsonValueAddition => ToolRecordDefinitions.JsonValueKind,
        ToolManagedFileAddition => ToolRecordDefinitions.ManagedFileKind,
        ToolMarkedBlockAddition => ToolRecordDefinitions.MarkedBlockKind,
        ToolNativeCommandAddition => ToolRecordDefinitions.NativeCommandKind,
        _ => throw new ArgumentOutOfRangeException(nameof(change)),
    };

    private static void AddPackageUninstall(ToolWriteToolPlan tool, ToolWritePlan plan, List<ToolWriteCommandView> commands)
    {
        var manager = tool.Detection.PackageManager ?? tool.Detection.RecordedState?.PackageManager;
        var channelId = tool.Detection.ChannelId ?? plan.Input.Records.Portable?.Lock.GetValueOrDefault(tool.Entry.Identity.Id)?.Channel;
        if (manager is null || tool.Entry.Channels.OfType<ToolNpmChannel>().FirstOrDefault(channel => channel.Id == channelId) is not { } npm) return;
        IReadOnlyList<string> arguments = manager.Client switch
        {
            ToolNpmClient.Npm => ["uninstall", "--global", npm.Package],
            ToolNpmClient.Pnpm or ToolNpmClient.Bun => ["remove", "--global", npm.Package],
            ToolNpmClient.YarnClassic => ["global", "remove", npm.Package],
            ToolNpmClient.YarnModern => [],
            _ => throw new ArgumentOutOfRangeException(nameof(manager)),
        };
        if (arguments.Count == 0) return;
        commands.Add(new("manual-package-uninstall", manager.Invocation.ExecutablePath,
            [.. manager.Invocation.PrefixArguments, .. arguments], plan.Input.Request.Workspace.LexicalRoot,
            new Dictionary<string, string?>(), 0, 0));
    }

    private static string Readiness(ToolPathReadiness readiness) => readiness switch
    {
        ToolPathReadiness.Ready => "ready",
        ToolPathReadiness.Missing => "missing",
        ToolPathReadiness.Unavailable => "unavailable",
        _ => throw new ArgumentOutOfRangeException(nameof(readiness)),
    };
}
