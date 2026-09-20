using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Commands.Repair.Models.Result;

internal sealed record RepairResultInput
{
    internal RepairLibraryExecution? LibraryExecution { get; init; }

    internal required RepairRequest Request { get; init; }

    internal required RepairDiagnosisCoverage Diagnosis { get; init; }

    internal required RepairPlan Plan { get; init; }

    internal required IReadOnlyList<RepairFinding> Findings { get; init; }

    internal required IReadOnlyList<RepairFinding> InitialFindings { get; init; }

    internal required RepairPreflight Preflight { get; init; }

    internal required RepairApplication Application { get; init; }

    internal required RepairVerification Verification { get; init; }

    internal required RepairRecovery Recovery { get; init; }

    internal required RepairPostDiagnosis PostDiagnosis { get; init; }
}
