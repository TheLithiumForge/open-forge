using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceRouteDoctorInspector
{
    internal static void Inspect(
        RouteDoctorView routes,
        ICollection<DoctorFinding> findings)
    {
        foreach (var root in routes.DeclaredRoots)
        {
            findings.Add(CreateRoot(root));
        }

        foreach (var detached in routes.Shape.Where(shape =>
                     shape.Kind == RouteShapeObservationKind.Detached))
        {
            findings.Add(DoctorDomainSupport.Create(
                DoctorDomainSupport.Information(
                    DoctorFindingKind.WorkspaceDetached,
                    "A complete route tree is detached from established Loader roots."),
                DoctorDomainSupport.Subject(DoctorSubjectKind.Path, detached.Path),
                DoctorDomainSupport.Provenance(
                    DoctorDomainKind.WorkspaceEntry,
                    DoctorProvenanceSource.WorkspaceEntry,
                    detached.Path),
                [new DoctorStateEvidence(DoctorObservedState.Present)]));
        }
    }

    private static DoctorFinding CreateRoot(RouteDeclaredRootObservation root)
    {
        var descriptor = root.State switch
        {
            RouteDeclaredRootState.Missing => DoctorDomainSupport.Warning(
                DoctorFindingKind.WorkspaceRootMissing,
                "A Loader-declared root is missing.",
                DoctorResolutionLane.ManualDecision),
            RouteDeclaredRootState.Unreachable => DoctorDomainSupport.Warning(
                DoctorFindingKind.WorkspaceRootUnreachable,
                "A Loader-declared root is not reachable through established route facts.",
                DoctorResolutionLane.ManualDecision),
            _ => throw new ArgumentOutOfRangeException(
                nameof(root),
                root.State,
                "The declared-root state is not defined."),
        };
        return DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, root.Path),
            DoctorDomainSupport.Provenance(
                DoctorDomainKind.WorkspaceEntry,
                DoctorProvenanceSource.WorkspaceEntry,
                root.LoaderPath),
            [
                new DoctorStateEvidence(root.State == RouteDeclaredRootState.Missing
                    ? DoctorObservedState.Missing
                    : DoctorObservedState.Unavailable),
                new DoctorAuthoredValueEvidence(root.Path, Location: null),
            ]);
    }
}
