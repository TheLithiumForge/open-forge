using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class RouteGeneratedEntryDoctorInspector
{
    internal static IEnumerable<DoctorFinding> Inspect(
        IReadOnlyList<DoctorGeneratedNavigationTargetObservation> observations)
    {
        foreach (var observation in observations)
        {
            if (observation.State is not (OperationalGeneratedNavigationState.Current
                or OperationalGeneratedNavigationState.Changed))
            {
                continue;
            }

            foreach (var comparison in observation.Content.EntryComparisons)
            {
                yield return Create(
                    observation.Path,
                    ReadKind(comparison.Kind),
                    comparison.Expected ?? "absent",
                    comparison.Actual ?? "absent",
                    comparison.Location);
            }
        }
    }

    private static DoctorFindingKind ReadKind(RouteGeneratedEntryComparisonKind kind)
        => kind switch
        {
            RouteGeneratedEntryComparisonKind.Missing => DoctorFindingKind.RouteGeneratedEntryMissing,
            RouteGeneratedEntryComparisonKind.Extra => DoctorFindingKind.RouteGeneratedEntryExtra,
            RouteGeneratedEntryComparisonKind.Order => DoctorFindingKind.RouteGeneratedEntryOrder,
            RouteGeneratedEntryComparisonKind.Path => DoctorFindingKind.RouteGeneratedEntryPath,
            RouteGeneratedEntryComparisonKind.Description => DoctorFindingKind.RouteGeneratedEntryDescription,
            RouteGeneratedEntryComparisonKind.Tags => DoctorFindingKind.RouteGeneratedEntryTags,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The generated-entry comparison kind is not defined."),
        };

    private static DoctorFinding Create(
        string path,
        DoctorFindingKind kind,
        string expected,
        string actual,
        Framework.Sources.Models.Locations.SourceLocation? location)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(
                kind,
                "A generated entry differs from the exact current route projection.",
                DoctorResolutionLane.TargetedOperation,
                new DoctorNextAction
                {
                    Kind = DoctorNextActionKind.AcceptedOperation,
                    Operation = DoctorNextOperation.Index,
                    Command = CommandLines.Index,
                    Reason = "Index owns deterministic generated-navigation projection.",
                }),
            new DoctorSubject
            {
                Kind = DoctorSubjectKind.GeneratedRegion,
                Path = path,
                Identifier = actual,
                Location = location,
            },
            new DoctorProvenance
            {
                Domain = DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                Source = DoctorProvenanceSource.GeneratedNavigation,
                Path = path,
                Location = location,
            },
            [
                new DoctorComparisonEvidence(expected, actual),
                new DoctorAuthoredValueEvidence(actual, location),
            ]);
}
