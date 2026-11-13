using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Run.Shared.Reporting;

internal static class ToolRunFailureCodes
{
    internal static string Suffix(ToolRunFailureCode code) => code switch
    {
        ToolRunFailureCode.InvalidInput => "invalid-input",
        ToolRunFailureCode.ToolUndeclared => "tool-undeclared",
        ToolRunFailureCode.DeclaredToolMissing => "declared-tool-missing",
        ToolRunFailureCode.PinMismatch => "pin-mismatch",
        ToolRunFailureCode.WorkspaceUnavailable => "workspace-unavailable",
        ToolRunFailureCode.VerificationUnavailable => "verification-unavailable",
        ToolRunFailureCode.LaunchIntegrityMismatch => "launch-integrity-mismatch",
        ToolRunFailureCode.StartFailed => "start-failed",
        ToolRunFailureCode.Cancelled => "cancelled",
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };

    internal static int ExitCode(ToolRunFailureCode code) => CliStatusDefinitions.Read(Status(code)).Disposition.ExitCode;

    private static CliSemanticStatus Status(ToolRunFailureCode code) => code switch
    {
        ToolRunFailureCode.InvalidInput or ToolRunFailureCode.ToolUndeclared => CliSemanticStatus.Invalid,
        ToolRunFailureCode.DeclaredToolMissing or ToolRunFailureCode.PinMismatch or ToolRunFailureCode.LaunchIntegrityMismatch => CliSemanticStatus.Blocked,
        ToolRunFailureCode.WorkspaceUnavailable or ToolRunFailureCode.VerificationUnavailable => CliSemanticStatus.Incomplete,
        ToolRunFailureCode.StartFailed => CliSemanticStatus.Failed,
        ToolRunFailureCode.Cancelled => CliSemanticStatus.Interrupted,
        _ => throw new ArgumentOutOfRangeException(nameof(code)),
    };
}
