using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionObservationHorizonDoctorInspector
{
    internal static void AddBridgeRegistrationObservations(
        ExtensionBridgeRegistrationFacts facts,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(findings);
        ArgumentNullException.ThrowIfNull(limitations);

        if (facts.Coverage == ExtensionBridgeRegistrationCoverage.Complete)
        {
            foreach (var observation in facts.Observations)
            {
                ExtensionBridgeRegistrationDoctorInspector.Inspect(
                    observation,
                    findings);
            }

            return;
        }

        var state = facts.Coverage == ExtensionBridgeRegistrationCoverage.Blocked
            ? DoctorCoverageState.Blocked
            : DoctorCoverageState.Incomplete;
        coverage = DoctorDomainSupport.Combine(coverage, state);
        limitations.Add(DoctorDomainSupport.Limitation(
            state,
            facts.Coverage == ExtensionBridgeRegistrationCoverage.Blocked
                ? facts.Cause ?? "Typed Extension bridge-registration observations are blocked."
                : "Typed Extension bridge-registration role and observed-state authority is unavailable; no target role was inferred."));
    }

    internal static void AddLimitations(
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        limitations.Add(DoctorDomainSupport.Limitation(
            DoctorCoverageState.Incomplete,
            "A bounded contained Extension manifest candidate universe is unavailable; no installed manifest scan was inferred."));
    }
}
