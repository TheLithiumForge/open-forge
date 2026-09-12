using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class RouteDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.RoutesMetadataOverwritesGeneratedNavigation;

    internal static DoctorDomainReport Inspect(RouteDoctorView view)
    {
        var findings = new List<DoctorFinding>();
        findings.AddRange(view.Routes.RouteFacts.Select(CreateRouteFinding).Where(finding => finding is not null).Cast<DoctorFinding>());
        findings.AddRange(view.Routes.Issues.Select(CreateIssueFinding).Where(finding => finding is not null).Cast<DoctorFinding>());
        findings.AddRange(view.Metadata
            .Where(metadata => !string.Equals(metadata.Path, SourceLogicalPath.LoaderPath, StringComparison.Ordinal))
            .Select(CreateMetadataFinding)
            .Where(finding => finding is not null)
            .Cast<DoctorFinding>());
        findings.AddRange(view.GeneratedNavigation.Select(CreateGeneratedFinding).Where(finding => finding is not null).Cast<DoctorFinding>());
        findings.AddRange(RouteGeneratedEntryDoctorInspector.Inspect(view.GeneratedNavigation));
        findings.AddRange(RouteShapeDoctorInspector.Inspect(view.Shape));
        findings.AddRange(RouteSourceDoctorInspector.Inspect(view));
        findings.AddRange(view.Catalogue.Issues
            .Where(issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite)
            .Select(CreateOrphanFinding));

        var coverage = DoctorDomainSupport.Coverage(view.State);
        var unavailable = view.Routes.RouteFacts.Any(fact => fact.State == SourceRouteState.Unavailable)
            || view.Routes.Issues.Any(issue => issue.Code == SourceRouteIssueCode.RouteSupportUnavailable)
            || view.Metadata.Any(item => item.Facts.State == FrameworkDocumentMetadataState.Malformed)
            || view.GeneratedNavigation.Any(item => item.State is OperationalGeneratedNavigationState.Unavailable
                or OperationalGeneratedNavigationState.Blocked)
            || !view.Routes.AreLoaderRootFactsComplete
            || view.Routes.IsCancelled
            || view.Catalogue.IsCancelled;
        if (unavailable)
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        }

        var limitations = unavailable
            ? new[]
            {
                DoctorDomainSupport.Limitation(
                    coverage,
                    "Route facts contain unavailable distinctions; malformed-region and authored-metadata causes were not inferred."),
            }
            : [];
        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.RouteUniverse, Path = null },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static DoctorFinding? CreateRouteFinding(SourceRouteFact fact)
    {
        var descriptor = fact.State switch
        {
            SourceRouteState.Routed => null,
            SourceRouteState.Unrouted => null,
            SourceRouteState.Ambiguous => DoctorDomainSupport.Error(
                DoctorFindingKind.RouteCompatibilityConflict,
                "A source has ambiguous route identity."),
            SourceRouteState.Unavailable => null,
            _ => throw new ArgumentOutOfRangeException(nameof(fact), fact.State, "The route state is not defined."),
        };
        return descriptor is null ? null : Create(descriptor, fact.Identity.CanonicalBasePath, fact.Identity.AutomaticId);
    }

    private static DoctorFinding? CreateIssueFinding(SourceRouteIssue issue)
    {
        var descriptor = issue.Code switch
        {
            SourceRouteIssueCode.RouteAmbiguous => DoctorDomainSupport.Error(
                DoctorFindingKind.RouteCompatibilityConflict,
                issue.Cause),
            SourceRouteIssueCode.LoaderDestinationMissing => DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteEntrypointMissing,
                issue.Cause,
                DoctorResolutionLane.ManualDecision),
            SourceRouteIssueCode.LoaderUnsafe => DoctorDomainSupport.Error(
                DoctorFindingKind.RouteEscape,
                issue.Cause),
            SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderDuplicateRoot
                or SourceRouteIssueCode.LoaderMalformed
                or SourceRouteIssueCode.RouteSupportUnavailable => null,
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The route issue code is not defined."),
        };
        return descriptor is null ? null : Create(descriptor, issue.CanonicalPath, identifier: null);
    }

    private static DoctorFinding? CreateMetadataFinding(RouteMetadataObservation observation)
    {
        var descriptor = observation.Facts.State switch
        {
            FrameworkDocumentMetadataState.Complete => null,
            FrameworkDocumentMetadataState.Missing => DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteMetadataRequiredMissing,
                "Required route metadata is missing.",
                DoctorResolutionLane.ManualDecision),
            FrameworkDocumentMetadataState.Malformed => null,
            _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.Facts.State, "The metadata state is not defined."),
        };
        return descriptor is null ? null : DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Route, observation.Path),
            DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.RouteMetadata, observation.Path),
            [new DoctorStateEvidence(observation.Facts.State == FrameworkDocumentMetadataState.Missing
                ? DoctorObservedState.Missing
                : DoctorObservedState.Malformed)]);
    }

    private static DoctorFinding? CreateGeneratedFinding(
        DoctorGeneratedNavigationTargetObservation observation)
    {
        var action = new DoctorNextAction
        {
            Kind = DoctorNextActionKind.AcceptedOperation,
            Operation = DoctorNextOperation.Index,
            Command = "open-forge index",
            Reason = "Index owns deterministic generated-navigation projection.",
        };
        var descriptor = observation.State switch
        {
            OperationalGeneratedNavigationState.Current or OperationalGeneratedNavigationState.NotApplicable => null,
            OperationalGeneratedNavigationState.Changed => DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteGeneratedRegionStale,
                "Generated navigation differs from current route facts.",
                DoctorResolutionLane.TargetedOperation,
                action),
            OperationalGeneratedNavigationState.Missing => DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteGeneratedRegionMissing,
                "A required generated-navigation region is missing.",
                DoctorResolutionLane.TargetedOperation,
                action),
            OperationalGeneratedNavigationState.Unavailable or OperationalGeneratedNavigationState.Blocked => null,
            _ => throw new ArgumentOutOfRangeException(nameof(observation), observation.State, "The generated-navigation state is not defined."),
        };
        return descriptor is null ? null : DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.GeneratedRegion, observation.Path),
            DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.GeneratedNavigation, observation.Path),
            [new DoctorStateEvidence(observation.State switch
            {
                OperationalGeneratedNavigationState.Changed => DoctorObservedState.Changed,
                OperationalGeneratedNavigationState.Missing => DoctorObservedState.Missing,
                _ => DoctorObservedState.Unavailable,
            })]);
    }

    private static DoctorFinding CreateOrphanFinding(SourceCatalogueIssue issue)
        => Create(
            DoctorDomainSupport.Warning(
                DoctorFindingKind.RouteOverwriteOrphan,
                "An overwrite companion has no valid base source.",
                DoctorResolutionLane.ManualDecision),
            issue.AttemptedCanonicalPath,
            identifier: null);

    private static DoctorFinding Create(DoctorFindingDescriptor descriptor, string path, string? identifier)
        => DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Route, path, identifier),
            DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.RouteInventory, path),
            [new DoctorAuthoredValueEvidence(path, Location: null)]);
}
