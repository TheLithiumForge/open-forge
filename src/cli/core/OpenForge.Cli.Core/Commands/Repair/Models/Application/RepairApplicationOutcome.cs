using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairApplicationOutcome(
    RepairPreflight Preflight,
    RepairApplication Application,
    RepairVerification Verification,
    RepairRecovery Recovery,
    RepairPostDiagnosis PostDiagnosis,
    IReadOnlyList<RepairFinding> Findings);
