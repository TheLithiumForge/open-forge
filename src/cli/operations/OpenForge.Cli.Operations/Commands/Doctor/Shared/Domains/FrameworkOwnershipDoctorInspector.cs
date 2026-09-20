using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class FrameworkOwnershipDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.FrameworkLifecycle;

    internal static void Inspect(
        WorkspaceOwnershipRead ownership,
        ICollection<DoctorFinding> findings,
        ICollection<DoctorLimitation> limitations,
        ref DoctorCoverageState coverage)
    {
        var frameworkPaths = ownership.Document.Framework?.Paths.ToHashSet(StringComparer.Ordinal) ?? [];
        foreach (var path in ownership.Document.Extensions.SelectMany(package => package.Paths)
            .Distinct(StringComparer.Ordinal).Where(frameworkPaths.Contains))
        {
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Warning(
                    DoctorFindingKind.FrameworkOwnershipConflict,
                    "The lock records Framework and Extension whole-file ownership for the same path.",
                    DoctorResolutionLane.ManualDecision),
                DoctorDomainSupport.Subject(DoctorSubjectKind.ManagedFile, path),
                DoctorDomainSupport.Provenance(
                    Domain,
                    DoctorProvenanceSource.LifecycleOwnership,
                    path),
                [new DoctorStateEvidence(DoctorObservedState.Invalid)]));
        }

    }
}
