using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceEntryDoctorInspector
{
    private const DoctorDomainKind Domain = DoctorDomainKind.WorkspaceEntry;

    internal static DoctorDomainReport Inspect(
        CliWorkspace workspace,
        WorkspaceEntryDoctorView view,
        RouteDoctorView routes)
    {
        var findings = routes.Catalogue.Issues
            .Where(issue => issue.Code != SourceCatalogueIssueCode.OrphanOverwrite)
            .SelectMany(CreateCatalogueFindings)
            .ToList();
        WorkspacePathDoctorInspector.Inspect(view, findings);
        WorkspaceSourceDoctorInspector.Inspect(routes, findings);
        WorkspaceRouteDoctorInspector.Inspect(routes, findings);
        findings.AddRange(routes.Routes.Issues
            .Where(issue => issue.Code is SourceRouteIssueCode.LoaderMalformed
                or SourceRouteIssueCode.LoaderDuplicateRoot
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.LoaderUnsafe)
            .Select(CreateLoaderFinding));

        var coverage = DoctorDomainSupport.Coverage(view.State);
        if (routes.Catalogue.IsCancelled || routes.Routes.IsCancelled)
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
        }

        var limitations = new List<DoctorLimitation>();
        AddEntryLimitations(view, limitations, ref coverage);
        if (coverage != DoctorCoverageState.Complete)
        {
            limitations.Add(DoctorDomainSupport.Limitation(
                coverage,
                "Workspace entry evidence did not establish every required path and identity distinction."));
        }

        if (routes.Catalogue.Issues.Any(issue => issue.Code is SourceCatalogueIssueCode.RootUnavailable
                or SourceCatalogueIssueCode.DirectoryUnavailable
                or SourceCatalogueIssueCode.CandidateUnavailable
                or SourceCatalogueIssueCode.IdentityUnavailable))
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
            limitations.Add(DoctorDomainSupport.Limitation(
                DoctorCoverageState.Incomplete,
                "One or more source catalogue paths were unavailable; no physical failure kind was inferred."));
        }

        if (routes.Routes.Issues.Any(issue => issue.Code is SourceRouteIssueCode.LoaderUnavailable
                or SourceRouteIssueCode.LoaderUnreadable
                or SourceRouteIssueCode.RouteSupportUnavailable))
        {
            coverage = DoctorDomainSupport.Combine(coverage, DoctorCoverageState.Incomplete);
            limitations.Add(DoctorDomainSupport.Limitation(
                DoctorCoverageState.Incomplete,
                "Loader or route-support evidence is unavailable; unreadable and unsupported causes were not conflated."));
        }

        return new DoctorDomainReport
        {
            Domain = Domain,
            Boundary = new DoctorBoundary { Kind = DoctorBoundaryKind.Workspace, Path = workspace.LexicalRoot },
            Coverage = coverage,
            Lifecycle = null,
            SourceAvailability = null,
            Limitations = limitations,
            Counts = DoctorFindingAggregation.Count(findings),
            Findings = DoctorFindingAggregation.Order(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
        };
    }

    private static void AddEntryLimitations(
        WorkspaceEntryDoctorView view,
        ICollection<DoctorLimitation>? limitations,
        ref DoctorCoverageState coverage)
    {
        if (view.Installation is OperationalInstallationState.Installed or OperationalInstallationState.Uninstalled)
        {
            return;
        }

        coverage = DoctorDomainSupport.Combine(
            coverage,
            view.Installation == OperationalInstallationState.Blocked
                ? DoctorCoverageState.Blocked
                : DoctorCoverageState.Incomplete);
        limitations?.Add(DoctorDomainSupport.Limitation(
            coverage,
            $"Workspace entry evidence is incomplete (entry: {view.EntryPath
                ?? "unavailable"}; loader: {view.LoaderPath
                ?? "unavailable"}); missing, unreadable, and unsafe path causes cannot be distinguished by this view."));
    }

    private static IEnumerable<DoctorFinding> CreateCatalogueFindings(
        SourceCatalogueIssue issue)
    {
        var descriptor = issue.Code switch
        {
            SourceCatalogueIssueCode.RootUnsafe => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspacePathContainment,
                "A source path cannot establish safe workspace containment."),
            SourceCatalogueIssueCode.EntrypointAmbiguous => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceEntryAmbiguous,
                "More than one recognized entrypoint claims one folder."),
            SourceCatalogueIssueCode.EntrypointCompatibilityCollision => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceEntryCompatibilityCollision,
                "Canonical and compatibility entrypoint forms cannot establish one identity."),
            SourceCatalogueIssueCode.IdentityCollision => DoctorDomainSupport.Warning(
                DoctorFindingKind.WorkspaceSourceIdCollision,
                "Several current sources derive the same automatic identity.",
                DoctorResolutionLane.ManualDecision),
            SourceCatalogueIssueCode.PhysicalAlias => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspacePhysicalAlias,
                "Distinct source paths resolve to an unsafe physical alias."),
            SourceCatalogueIssueCode.UnsupportedSource => DoctorDomainSupport.Information(
                DoctorFindingKind.WorkspaceUnsupportedSource,
                "A contained source candidate has an unsupported source kind."),
            _ => null,
        };
        if (descriptor is null)
        {
            yield break;
        }

        var paths = issue.RelatedPaths.Count == 0
            ? [issue.AttemptedCanonicalPath]
            : issue.RelatedPaths;
        foreach (var path in paths)
        {
            yield return DoctorDomainSupport.Create(
                descriptor,
                DoctorDomainSupport.Subject(DoctorSubjectKind.Path, path),
                DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.WorkspaceEntry, path),
                [new DoctorStateEvidence(ReadCatalogueState(issue.Code))]);
        }
    }

    private static DoctorObservedState ReadCatalogueState(SourceCatalogueIssueCode code)
        => code switch
        {
            SourceCatalogueIssueCode.RootUnsafe
                or SourceCatalogueIssueCode.EntrypointAmbiguous
                or SourceCatalogueIssueCode.EntrypointCompatibilityCollision
                or SourceCatalogueIssueCode.IdentityCollision
                or SourceCatalogueIssueCode.PhysicalAlias => DoctorObservedState.Blocked,
            SourceCatalogueIssueCode.UnsupportedSource => DoctorObservedState.Unsupported,
            _ => DoctorObservedState.Unavailable,
        };

    private static DoctorFinding CreateLoaderFinding(SourceRouteIssue issue)
    {
        var descriptor = issue.Code switch
        {
            SourceRouteIssueCode.LoaderMalformed => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceLoaderMalformed,
                issue.Cause),
            SourceRouteIssueCode.LoaderDuplicateRoot => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceLoaderMalformed,
                issue.Cause),
            SourceRouteIssueCode.LoaderUnreadable => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspaceLoaderUnreadable,
                issue.Cause),
            SourceRouteIssueCode.LoaderUnsafe => DoctorDomainSupport.Error(
                DoctorFindingKind.WorkspacePathContainment,
                issue.Cause),
            _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The Loader issue code is not defined."),
        };
        return DoctorDomainSupport.Create(
            descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, issue.CanonicalPath),
            DoctorDomainSupport.Provenance(Domain, DoctorProvenanceSource.WorkspaceEntry, issue.CanonicalPath),
            [new DoctorStateEvidence(issue.Code switch
            {
                SourceRouteIssueCode.LoaderMalformed
                    or SourceRouteIssueCode.LoaderDuplicateRoot => DoctorObservedState.Malformed,
                SourceRouteIssueCode.LoaderUnreadable => DoctorObservedState.Unavailable,
                SourceRouteIssueCode.LoaderUnsafe => DoctorObservedState.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(issue), issue.Code, "The Loader issue code is not defined."),
            })]);
    }
}
