using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Application;

internal sealed record RepairPostVerification(
    RepairVerification Verification,
    RepairPostDiagnosis Diagnosis,
    IReadOnlyList<RepairFinding> Findings);
