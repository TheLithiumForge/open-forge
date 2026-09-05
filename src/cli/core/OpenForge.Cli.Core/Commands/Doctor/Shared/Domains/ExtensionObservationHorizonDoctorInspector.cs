using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionObservationHorizonDoctorInspector
{
    internal static void AddLimitations(
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        limitations.Add(DoctorDomainSupport.Limitation(
            DoctorCoverageState.Incomplete,
            "Typed Extension bridge-registration role and observed-state authority is unavailable; no target role was inferred."));
        limitations.Add(DoctorDomainSupport.Limitation(
            DoctorCoverageState.Incomplete,
            "A bounded contained Extension manifest candidate universe is unavailable; no installed manifest scan was inferred."));
    }
}
