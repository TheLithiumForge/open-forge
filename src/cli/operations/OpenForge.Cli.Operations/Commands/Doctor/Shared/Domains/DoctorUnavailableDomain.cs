using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class DoctorUnavailableDomain
{
    internal static DoctorDomainReport Create(DoctorDomainKind domain, DoctorBoundaryKind boundary, string? cause,
        IReadOnlyList<DoctorFinding>? findings = null)
    {
        findings ??= [];
        return new()
        {
            Domain = domain,
            Boundary = new DoctorBoundary { Kind = boundary, Path = null },
            Coverage = DoctorCoverageState.Incomplete,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = [DoctorDomainSupport.Limitation(DoctorCoverageState.Incomplete, cause ?? "The workspace layout is unavailable.")],
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }
}
