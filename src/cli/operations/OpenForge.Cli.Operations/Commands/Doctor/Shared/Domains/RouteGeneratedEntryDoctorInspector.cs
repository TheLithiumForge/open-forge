using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Actions;
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
                yield return Create(observation.Path, comparison);
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

    private static DoctorFinding Create(string path, RouteGeneratedEntryComparison comparison)
    {
        var expected = comparison.Expected ?? "absent";
        var actual = comparison.Actual ?? "absent";
        var identifier = comparison.Kind is RouteGeneratedEntryComparisonKind.Missing
            ? comparison.Expected ?? throw new InvalidOperationException(
                "A missing generated-entry comparison requires an expected destination.")
            : actual;

        return DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(
                ReadKind(comparison.Kind),
                "A generated entry differs from the exact current route projection.",
                DoctorResolutionLane.TargetedOperation,
                DoctorIndexAction.ForPath(path)),
            new DoctorSubject
            {
                Kind = DoctorSubjectKind.GeneratedRegion,
                Path = path,
                Identifier = identifier,
                Location = comparison.Location,
            },
            new DoctorProvenance
            {
                Domain = DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                Source = DoctorProvenanceSource.GeneratedNavigation,
                Path = path,
                Location = comparison.Location,
            },
            [
                new DoctorComparisonEvidence(expected, actual),
                new DoctorAuthoredValueEvidence(actual, comparison.Location),
            ]);
    }
}
