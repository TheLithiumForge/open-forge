using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorWorkspaceRouteMappingTests
{
    [Fact(DisplayName = "Doctor maps exact Loader-declared root boundaries with typed evidence")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DeclaredRootsMapWithoutInventingReachability()
    {
        var view = View(
            [
                RouteDeclaredRootObservation.Missing(
                    ".agents/missing/AGENTS.md",
                    ".agents/loader.md"),
                RouteDeclaredRootObservation.Unreachable(
                    ".agents/detached/AGENTS.md",
                    ".agents/loader.md"),
            ],
            []);
        var findings = new List<DoctorFinding>();

        WorkspaceRouteDoctorInspector.Inspect(view, findings);

        Assert.Equal(
            [DoctorFindingKind.WorkspaceRootMissing, DoctorFindingKind.WorkspaceRootUnreachable],
            findings.Select(finding => finding.Kind));
        Assert.All(findings, finding => Assert.NotEmpty(finding.Evidence));
        Assert.All(findings, finding => Assert.Equal(".agents/loader.md", finding.Provenance.Path));
    }

    [Fact(DisplayName = "Doctor maps only producer-owned finite route-shape observations")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void RouteShapeFactsRetainEveryExactSubjectAndEvidence()
    {
        var location = new SourceLocation(
            line: 2,
            column: 4,
            byteOffset: 10,
            byteLength: 18);
        var observations = new[]
        {
            RouteShapeObservation.EntrypointDuplicate(
            [
                ".agents/team/AGENTS.md",
                ".agents/team/_team.md",
            ]),
            RouteShapeObservation.Unreachable(".agents/orphan.md"),
            RouteShapeObservation.Detached(
                ".agents/tree/AGENTS.md",
                [".agents/tree/child.md"]),
            RouteShapeObservation.OverwriteIndependentIndex(
                ".agents/tree/AGENTS.md",
                ".agents/tree/child.overwrite.md",
                "child.overwrite.md",
                location),
        };

        var findings = RouteShapeDoctorInspector.Inspect(observations).ToArray();

        Assert.Equal(5, findings.Length);
        Assert.Equal(2, findings.Count(finding =>
            finding.Kind == DoctorFindingKind.RouteEntrypointDuplicate));
        Assert.Contains(findings, finding => finding.Kind == DoctorFindingKind.RouteUnreachable);
        Assert.Contains(findings, finding => finding.Kind == DoctorFindingKind.RouteDetached);
        Assert.Contains(findings, finding =>
            finding.Kind == DoctorFindingKind.RouteOverwriteIndependentIndex);
        Assert.All(findings, finding => Assert.NotEmpty(finding.Evidence));
    }

    [Fact(DisplayName = "Doctor projects the precise duplicate Loader root fact as a malformed Loader")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DuplicateLoaderRootMapsToLoaderMalformedWithTypedEvidence()
    {
        var workspace = DoctorOperationTestSupport.Workspace();
        var routes = View(
            [],
            [],
            [new SourceRouteIssue(
                SourceRouteIssueCode.LoaderDuplicateRoot,
                ".agents/loader.md",
                [".agents/other-loader.md"],
                occurrence: 1,
                "The same root is declared more than once.")]);
        var entry = new WorkspaceEntryDoctorView(
            new WorkspaceEntryDoctorSummary(
                OperationalViewState.Complete,
                OperationalInstallationState.Uninstalled,
                null,
                null),
            new WorkspacePathObservationSet(
                WorkspacePathObservation.Unresolved(
                    workspace.LexicalRoot,
                    PhysicalPathState.Missing),
                WorkspacePathObservation.Unresolved(
                    ".agents",
                    PhysicalPathState.Missing),
                WorkspacePathObservation.Unresolved(
                    ".agents/AGENTS.md",
                    PhysicalPathState.Missing),
                WorkspacePathObservation.Unresolved(
                    ".agents/loader.md",
                    PhysicalPathState.Missing)));

        var report = WorkspaceEntryDoctorInspector.Inspect(workspace, entry, routes);

        var finding = Assert.Single(report.Findings, candidate =>
            candidate.Kind == DoctorFindingKind.WorkspaceLoaderMalformed);
        var evidence = Assert.IsType<DoctorStateEvidence>(Assert.Single(finding.Evidence));
        Assert.Equal(DoctorObservedState.Malformed, evidence.State);
        Assert.Equal(".agents/loader.md", finding.Provenance.Path);
    }

    [Fact(DisplayName = "Unavailable generated navigation never projects expected-entry drift")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnavailableGeneratedNavigationDoesNotProjectEntryComparisons()
    {
        var observation = DoctorGeneratedNavigationTargetObservation.Unavailable(
            ".agents/AGENTS.md",
            OperationalGeneratedNavigationState.Unavailable,
            new DoctorGeneratedNavigationContent(
                SourceGeneratedEntriesFacts.Absent,
                [],
                [new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Missing,
                    "expected entry",
                    actual: null,
                    location: null)]),
            new DoctorGeneratedNavigationUnavailability(
                GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable,
                "The generated region is unavailable."));

        Assert.Empty(RouteGeneratedEntryDoctorInspector.Inspect([observation]));
    }

    private static RouteDoctorView View(
        IReadOnlyList<RouteDeclaredRootObservation> roots,
        IReadOnlyList<RouteShapeObservation> shape,
        IReadOnlyList<SourceRouteIssue>? issues = null)
    {
        var workspace = DoctorOperationTestSupport.Workspace();
        return new RouteDoctorView
        {
            State = OperationalViewState.Complete,
            SourceInventory = RouteSourceInventoryState.Present,
            Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = new SourceRouteFacts(
                new SourceRouteTopology([], []),
                [],
                issues ?? [],
                areLoaderRootFactsComplete: true,
                isCancelled: false),
            Metadata = [],
            WorkspaceEntry = new RouteSourceLayerObservation(
                "AGENTS.md",
                FileReadState.Missing,
                Text: null),
            Sources = [],
            GeneratedNavigation = [],
            DeclaredRoots = roots,
            Shape = shape,
        };
    }
}
