using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ExtensionBridgeRegistrationDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.ExtensionLifecycle;

    internal static void Inspect(
        ExtensionBridgeRegistrationObservation observation,
        ICollection<DoctorFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(observation);
        ArgumentNullException.ThrowIfNull(findings);

        if (observation.State == ExtensionBridgeRegistrationState.Current)
        {
            return;
        }

        var evidence = observation.State switch
        {
            ExtensionBridgeRegistrationState.Missing =>
                (IReadOnlyList<DoctorEvidence>)[new DoctorStateEvidence(DoctorObservedState.Missing)],
            ExtensionBridgeRegistrationState.Unreadable =>
                [new DoctorStateEvidence(DoctorObservedState.Unavailable)],
            ExtensionBridgeRegistrationState.Inconsistent when observation.ActualEntry is { } actual =>
                [
                    new DoctorComparisonEvidence(observation.ExpectedEntry, actual),
                    new DoctorAuthoredValueEvidence(observation.ExpectedEntry, Location: null),
                ],
            _ => throw new ArgumentOutOfRangeException(
                nameof(observation),
                observation.State,
                "The Extension bridge-registration state is not defined."),
        };

        findings.Add(DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(
                DoctorFindingKind.ExtensionBridgeRegistration,
                observation.Cause
                    ?? "An Extension generated-navigation Entries registration is not current.",
                DoctorResolutionLane.ManualDecision),
            DoctorDomainSupport.Subject(
                DoctorSubjectKind.Extension,
                observation.TargetPath,
                observation.PackageId),
            DoctorDomainSupport.Provenance(
                Domain,
                DoctorProvenanceSource.GeneratedNavigation,
                observation.ParentPath),
            evidence));
    }
}
