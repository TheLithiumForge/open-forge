using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Result;

internal enum RemoveFindingCode
{
    InvalidInput,
    ProtectedPath,
    EntryPointRequiresRoute,
    TargetUnavailable,
    TargetUnsafe,
    TargetChanged,
    NavigationUnavailable,
    SettingsUnavailable,
    OwnershipUnavailable,
    WorkspaceLockUnavailable,
    ConfirmationRequired,
    PermissionDeclined,
    RecoveryUnavailable,
    WriteFailed,
    Interrupted,
}

internal sealed record RemoveFinding(
    RemoveFindingCode Code,
    CliSemanticStatus Status,
    string Message,
    string? Target);

internal sealed record RemoveEffect(
    string Path,
    string Kind,
    string Action,
    string Outcome);

internal sealed record RemoveResult : ICliCommandResult
{
    internal RemoveResult(
        CliWorkspace? workspace,
        string target,
        string kind,
        CliSemanticStatus status,
        IReadOnlyList<RemoveFinding> findings,
        IReadOnlyList<RemoveEffect> effects,
        IReadOnlyList<string> removed,
        string? recoveryPath,
        string recoveryDisposition,
        bool dryRun,
        CliNextAction? next)
    {
        Workspace = workspace;
        Target = target;
        Kind = kind;
        Status = status;
        Findings = findings;
        Effects = effects;
        Removed = removed;
        RecoveryPath = recoveryPath;
        RecoveryDisposition = recoveryDisposition;
        DryRun = dryRun;
        Next = next;
    }

    public string Command => "remove";

    public CliSemanticStatus Status { get; }

    public CliWorkspace? Workspace { get; }

    public CliNextAction? Next { get; }

    internal string Target { get; }

    internal string? WorkspacePath => Workspace?.LexicalRoot;

    internal bool WorkspaceExplicit
        => Workspace?.SelectedBy == CliWorkspaceSelectionMethod.ExplicitWorkspace;

    internal string Kind { get; }

    internal IReadOnlyList<RemoveFinding> Findings { get; }

    internal IReadOnlyList<RemoveEffect> Effects { get; }

    internal IReadOnlyList<string> Removed { get; }

    internal string? RecoveryPath { get; }

    internal string RecoveryDisposition { get; }

    internal bool DryRun { get; }

    internal static RemoveResult Complete(
        CliWorkspace workspace,
        string target,
        string kind,
        bool dryRun,
        IEnumerable<RemoveEffect> effects,
        IEnumerable<string> removed,
        string? recoveryPath = null,
        string recoveryDisposition = "not-required")
        => new(
            workspace,
            target,
            kind,
            CliSemanticStatus.Complete,
            [],
            [.. effects],
            [.. removed],
            recoveryPath,
            recoveryDisposition,
            dryRun,
            next: null);

    internal static RemoveResult Refused(
        CliWorkspace? workspace,
        string target,
        string kind,
        RemoveFindingCode code,
        CliSemanticStatus status,
        string message,
        string? nextCommand = null,
        string? nextReason = null,
        bool dryRun = false,
        IReadOnlyList<RemoveEffect>? effects = null,
        string? recoveryPath = null,
        string recoveryDisposition = "not-required",
        IReadOnlyList<string>? removed = null)
    {
        var finding = new RemoveFinding(code, status, message, target);
        var next = nextCommand is null
            ? new CliNextAction("open-forge remove --help", "Correct the Remove target and review its safety boundary.")
            : new CliNextAction(nextCommand, nextReason ?? message);
        return new RemoveResult(
            workspace,
            target,
            kind,
            status,
            [finding],
            effects ?? [],
            removed ?? [],
            recoveryPath,
            recoveryDisposition,
            dryRun,
            next);
    }
}
