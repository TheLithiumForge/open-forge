using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;

internal enum ToolWriteFindingCode
{
    InvalidInput, RecordsUnavailable, UnknownTool, ChannelInvalid, UpdateRequiresRecord,
    ChannelBlocked, ProfileUnavailable, NativePlanBlocked, ConfigConflict, MissingConsent,
    ManagerRequired, NativeFailed, VerificationFailed, ConfigFailed, RecordFailed,
    BackupFailed, ExpectedStateChanged, PermissionRequired, Interrupted, UnexpectedFailure, OpenForgeVersionUnsupported,
}

internal sealed record ToolWriteFinding(
    ToolWriteFindingCode Code, CliSemanticStatus Status, string? ToolId = null, string? Path = null, string? Cause = null);

internal sealed record ToolWriteResult : ICliCommandResult
{
    public required ToolWriteKind Kind { get; init; }
    public string Command => Kind switch
    {
        ToolWriteKind.Install => "tool install",
        ToolWriteKind.Update => "tool update",
        ToolWriteKind.Remove => "tool remove",
        _ => throw new ArgumentOutOfRangeException(nameof(Kind)),
    };
    public required CliSemanticStatus Status { get; init; }
    public CliWorkspace? Workspace { get; init; }
    public required string? WorkspacePath { get; init; }
    public required bool WorkspaceExplicit { get; init; }
    public required ToolWriteData Data { get; init; }
    public required IReadOnlyList<ToolWriteFinding> Findings { get; init; }
    public CliNextAction? Next { get; init; }
}

internal sealed record ToolWriteData
{
    public required IReadOnlyList<ToolWriteToolView> Tools { get; init; }
    public required bool Preview { get; init; }
    public required ToolWritePlanView Plan { get; init; }
    public required IReadOnlyList<ToolWriteOutcome> Outcomes { get; init; }
    public IReadOnlyList<ToolWriteGrantView> Grants { get; init; } = [];
}

internal sealed record ToolWritePlanView
{
    public required bool UsedNetwork { get; init; }
    public required string BinPath { get; init; }
    public required string PathReadiness { get; init; }
    public required IReadOnlyList<string> PathInstructions { get; init; }
    public IReadOnlyList<ToolWriteBackupView> Backups { get; init; } = [];
}

internal sealed record ToolWriteToolView
{
    public required string Id { get; init; }
    public string? OldVersion { get; init; }
    public string? Version { get; init; }
    public string? Channel { get; init; }
    public string? Manager { get; init; }
    public string? ManagerGlobalBin { get; init; }
    public string? ManagerPathReadiness { get; init; }
    public bool ReuseExisting { get; init; }
    public bool NoOp { get; init; }
    public bool AccountReplacement { get; init; }
    public string? Profile { get; init; }
    public string? ProfileDigest { get; init; }
    public string? NativeHome { get; init; }
    public string? NativePreview { get; init; }
    public required IReadOnlyList<ToolWriteCommandView> Commands { get; init; }
    public required IReadOnlyList<ToolWriteEffectView> Effects { get; init; }
    public required IReadOnlyList<ToolWriteChangeView> Changes { get; init; }
    public required IReadOnlyList<ToolWriteDownloadView> Downloads { get; init; }
    public required IReadOnlyList<string> Paths { get; init; }
    public required IReadOnlyList<string> Backups { get; init; }
    public required IReadOnlyList<string> Requirements { get; init; }
    public required IReadOnlyList<ToolWriteNativeSettingView> KeptNativeSettings { get; init; }
    public IReadOnlyList<ToolWriteTargetView> Targets { get; init; } = [];
    public string? MinimumOpenForgeVersion { get; init; }
    public string? MinimumToolVersion { get; init; }
    public IReadOnlyList<ToolWriteNativeOutputView> PossibleOutputs { get; init; } = [];
}

internal sealed record ToolWriteCommandView(
    string Purpose, string ExecutablePath, IReadOnlyList<string> Arguments,
    string WorkingDirectory, IReadOnlyDictionary<string, string?> Environment,
    double TimeoutSeconds, int MaximumOutputBytes);
internal sealed record ToolWriteEffectView(string Id, string Scope, string Kind, string Description, string? Path);
internal sealed record ToolWriteChangeView(string Id, string Kind, string? Path, string? Pointer, string Disposition, string? Description)
{
    public string? StoredNativeValue { get; init; }
    public IReadOnlyDictionary<string, string>? NativeEnvironmentOverrides { get; init; }
}
internal sealed record ToolWriteGrantView(
    IReadOnlyList<string> Requested, string State, IReadOnlyList<string> WrittenPaths, IReadOnlyList<string> Unfinished);
internal sealed record ToolWriteDownloadView(string Url, string Destination, string? Sha256);
internal sealed record ToolWriteNativeSettingView(string ChangeId, string NativeHome, string? Value, IReadOnlyList<string> ManualArguments);
internal sealed record ToolWriteBackupView(string Target, string? Backup, string? ExpectedSha256, bool Verified, string? GitObjectId = null);
internal sealed record ToolWriteTargetView(
    string OperatingSystem, string Cpu, string? Libc, string? MinimumOperatingSystemVersion, string? MinimumLibcVersion,
    string? MinimumLinuxKernelVersion = null, string? MinimumLibStdCppVersion = null);
internal sealed record ToolWriteNativeOutputView(string Path, string Scope, string Match, string Purpose, bool Conditional);
internal sealed record ToolWriteOutcome(string ToolId, string Stage, string State, IReadOnlyList<string> Completed, IReadOnlyList<string> Unfinished)
{
    public ToolWriteNativeOutput? Output { get; init; }
}
internal sealed record ToolWriteNativeOutput(int? ExitCode, bool TimedOut, bool Cancelled, string StandardOutput, string StandardError);
