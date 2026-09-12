using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class RouteShapeDoctorInspector
{
    private const DoctorDomainKind Domain =
        DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation;

    internal static IEnumerable<DoctorFinding> Inspect(
        IReadOnlyList<RouteShapeObservation> observations)
    {
        foreach (var observation in observations)
        {
            if (observation.Kind == RouteShapeObservationKind.EntrypointDuplicate)
            {
                foreach (var path in observation.RelatedPaths)
                {
                    yield return Create(
                        DoctorDomainSupport.Error(
                            DoctorFindingKind.RouteEntrypointDuplicate,
                            "A routed folder has duplicate recognized entrypoints."),
                        path,
                        [new DoctorStateEvidence(DoctorObservedState.Invalid)]);
                }

                continue;
            }

            yield return observation.Kind switch
            {
                RouteShapeObservationKind.Unreachable => Create(
                    DoctorDomainSupport.Warning(
                        DoctorFindingKind.RouteUnreachable,
                        "A source is not reachable through established route facts.",
                        DoctorResolutionLane.ManualDecision),
                    observation.Path,
                    [new DoctorStateEvidence(DoctorObservedState.Unavailable)]),
                RouteShapeObservationKind.Detached => Create(
                    DoctorDomainSupport.Warning(
                        DoctorFindingKind.RouteDetached,
                        "A complete route tree is detached from established Loader roots.",
                        DoctorResolutionLane.ManualDecision),
                    observation.Path,
                    [
                        new DoctorStateEvidence(DoctorObservedState.Present),
                        new DoctorComparisonEvidence(
                            "reachable route tree",
                            string.Join("|", observation.RelatedPaths)),
                    ]),
                RouteShapeObservationKind.OverwriteIndependentIndex => Create(
                    IndexWarning(),
                    observation.Path,
                    [
                        new DoctorComparisonEvidence(
                            "logical base source",
                            observation.RelatedPaths[0]),
                        new DoctorAuthoredValueEvidence(
                            observation.AuthoredValue
                                ?? throw new InvalidOperationException(
                                    "An independent overwrite observation requires its authored value."),
                            observation.Location),
                    ],
                    observation.Location),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(observation),
                    observation.Kind,
                    "The route-shape observation kind is not defined."),
            };
        }
    }

    private static DoctorFindingDescriptor IndexWarning()
        => DoctorDomainSupport.Warning(
            DoctorFindingKind.RouteOverwriteIndependentIndex,
            "An overwrite companion appears as an independent generated entry.",
            DoctorResolutionLane.TargetedOperation,
            new DoctorNextAction
            {
                Kind = DoctorNextActionKind.AcceptedOperation,
                Operation = DoctorNextOperation.Index,
                Command = "open-forge index",
                Reason = "Index owns deterministic generated-navigation projection.",
            });

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string path,
        IReadOnlyList<DoctorEvidence> evidence,
        Framework.Sources.Models.Locations.SourceLocation? location = null)
        => DoctorDomainSupport.Create(
            descriptor,
            new DoctorSubject
            {
                Kind = DoctorSubjectKind.Route,
                Path = path,
                Identifier = null,
                Location = location,
            },
            new DoctorProvenance
            {
                Domain = Domain,
                Source = DoctorProvenanceSource.RouteInventory,
                Path = path,
                Location = location,
            },
            evidence);
}
