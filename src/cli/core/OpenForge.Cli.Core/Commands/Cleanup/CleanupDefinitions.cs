using OpenForge.Cli.Core.Commands.Cleanup.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Definitions.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Cleanup;

internal enum CleanupMode
{
    NotEstablished,
    Apply,
    DryRun,
}

internal enum CleanupCatalogueCoverage
{
    NotEstablished,
    Complete,
    Incomplete,
    Interrupted,
}

internal enum CleanupCandidateEligibility
{
    NotEstablished,
    Eligible,
    Blocked,
}

internal enum CleanupPlanAction
{
    NotEstablished,
    Delete,
    Preserve,
}

internal enum CleanupPlanSafety
{
    NotEstablished,
    Safe,
    Blocked,
}

internal static class CleanupDefinitions
{
    internal const int SchemaVersion = 1;
    internal const string CommandIdentity = "cleanup";
    internal const string CleanupCommandName = "cleanup";
    internal const string CleanupCommandLine = "open-forge cleanup";
    internal const string CleanupHelpCommand = "open-forge cleanup --help";
    internal const string VerboseCleanupCommand = "open-forge cleanup --verbose";
    internal const string DoctorCommand = "open-forge doctor";

    internal static readonly CliSyntaxDefinition CleanupCommand = new(
        CleanupCommandName,
        "Remove recognized recovery bundles and drafts.");

    internal static readonly CliOptionDefinition<bool> DryRun = new(
        "--dry-run",
        "Preview cleanup without locking the workspace or writing files.",
        CliOptionArity.None,
        false);

    internal static CliSemanticStatus ReadStatus(CleanupFindingCode value)
        => value switch
        {
            CleanupFindingCode.InvalidInput => CliSemanticStatus.Invalid,
            CleanupFindingCode.WorkspaceUnavailable
                or CleanupFindingCode.WorkspaceNotDirectory
                or CleanupFindingCode.WorkspaceUnsafe
                or CleanupFindingCode.RecoveryFinalMalformed
                or CleanupFindingCode.RecoveryFinalUnsupported
                or CleanupFindingCode.RecoveryFinalUnavailable
                or CleanupFindingCode.RecoveryDraftUnsafe
                or CleanupFindingCode.WorkspaceLockUnavailable
                or CleanupFindingCode.CatalogueChangedDuringApply
                or CleanupFindingCode.CandidateChangedDuringApply => CliSemanticStatus.Blocked,
            CleanupFindingCode.CatalogueIncomplete => CliSemanticStatus.Incomplete,
            CleanupFindingCode.DeletionFailed
                or CleanupFindingCode.VerificationFailed
                or CleanupFindingCode.OperationFailed => CliSemanticStatus.Failed,
            CleanupFindingCode.Interrupted => CliSemanticStatus.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(value),
                value,
                "The Cleanup finding code is not defined."),
        };

    internal static CliNextAction? ReadNextAction(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete => null,
            CliSemanticStatus.Attention => null,
            CliSemanticStatus.Invalid => new CliNextAction(
                CleanupHelpCommand,
                "Correct the Cleanup input, then rerun the request."),
            CliSemanticStatus.Blocked => new CliNextAction(
                CleanupCommandLine,
                "Resolve the blocked cleanup boundary, then rerun Cleanup from a fresh catalogue."),
            CliSemanticStatus.Incomplete => new CliNextAction(
                DoctorCommand,
                "Inspect the unavailable workspace or recovery facts before relying on this Cleanup result."),
            CliSemanticStatus.Failed => new CliNextAction(
                VerboseCleanupCommand,
                "Report the failure and retry the same Cleanup request with bounded diagnostics."),
            CliSemanticStatus.Interrupted => new CliNextAction(
                CleanupCommandLine,
                "Rerun the same Cleanup request."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(status),
                status,
                "The Cleanup status is not defined."),
        };
}
