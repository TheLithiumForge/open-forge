using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class RouteSourceDoctorInspector
{
    internal static IEnumerable<DoctorFinding> Inspect(RouteDoctorView view)
    {
        foreach (var source in view.Sources)
        {
            if (source.Structure.Title.State is RouteTitleState.Missing or RouteTitleState.Invalid)
            {
                yield return Create(
                    DoctorDomainSupport.Warning(
                        DoctorFindingKind.RouteTitleInvalid,
                        "The route entrypoint does not contain one valid primary title.",
                        DoctorResolutionLane.ManualDecision),
                    source.Source.Identity.CanonicalBasePath,
                    DoctorObservedState.Invalid,
                    source.Structure.Title.Location);
            }

            if (source.Structure.Axioms.State is RouteAxiomsState.Missing or RouteAxiomsState.Invalid)
            {
                yield return Create(
                    DoctorDomainSupport.Warning(
                        DoctorFindingKind.RouteAxiomsInvalid,
                        "The route source has missing, duplicate, or empty required Axioms structure.",
                        DoctorResolutionLane.ManualDecision),
                    source.Source.Identity.CanonicalBasePath,
                    DoctorObservedState.Invalid,
                    source.Structure.Axioms.Location);
            }

            if (source.Document?.GeneratedRegion.InvalidKind is { } invalidKind)
            {
                yield return Create(
                    ReadRegionDescriptor(invalidKind),
                    source.Source.Identity.CanonicalBasePath,
                    DoctorObservedState.Malformed);
            }

        }
    }

    private static DoctorFindingDescriptor ReadRegionDescriptor(MarkdownGeneratedRegionInvalidKind kind)
        => kind switch
        {
            MarkdownGeneratedRegionInvalidKind.Malformed => DoctorDomainSupport.Error(
                DoctorFindingKind.RouteGeneratedRegionMalformed,
                "The generated region marker structure is malformed."),
            MarkdownGeneratedRegionInvalidKind.Misplaced => DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteGeneratedRegionMisplaced,
                "The generated region is outside its accepted final location.",
                DoctorResolutionLane.ManualDecision),
            MarkdownGeneratedRegionInvalidKind.Duplicate => DoctorDomainSupport.Error(
                DoctorFindingKind.RouteGeneratedRegionDuplicate,
                "More than one generated region or Entries section claims the source."),
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "The generated-region invalid kind is not defined."),
        };

    private static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        string path,
        DoctorObservedState state,
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
                Domain = DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation,
                Source = DoctorProvenanceSource.RouteInventory,
                Path = path,
                Location = location,
            },
            [new DoctorStateEvidence(state)]);
}
