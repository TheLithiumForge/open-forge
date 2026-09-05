using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

internal static class DoctorResultPolicy
{
    internal static CliSemanticStatus ReadStatus(DoctorCoverageState coverage, IReadOnlyList<DoctorDomainReport> domains)
    {
        if (coverage == DoctorCoverageState.Blocked)
        {
            return CliSemanticStatus.Blocked;
        }

        if (coverage == DoctorCoverageState.Incomplete)
        {
            return CliSemanticStatus.Incomplete;
        }

        return domains.SelectMany(domain => domain.Findings)
            .Any(finding => finding.Severity is DoctorFindingSeverity.Warning or DoctorFindingSeverity.Error)
            ? CliSemanticStatus.Attention
            : CliSemanticStatus.Complete;
    }

}
