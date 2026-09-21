using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorWorkspaceRouteMappingTests
{
    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor identifies missing generated entries by expected destination")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void GeneratedEntryMappingsUseExpectedIdentityOnlyForMissingComparisons()
    {
        const string cataloguePath = ".agents/skills/use-workflow/references/_references.md";
        var extraLocation = new SourceLocation(
            line: 4,
            column: 2,
            byteOffset: 30,
            byteLength: 12);
        var observation = DoctorGeneratedNavigationTargetObservation.Available(
            cataloguePath,
            OperationalGeneratedNavigationState.Changed,
            new DoctorGeneratedNavigationContent(
                SourceGeneratedEntriesFacts.Complete([]),
                [],
                [
                    new RouteGeneratedEntryComparison(
                        RouteGeneratedEntryComparisonKind.Missing,
                        expected: "beta-probe.md",
                        actual: null,
                        location: null),
                    new RouteGeneratedEntryComparison(
                        RouteGeneratedEntryComparisonKind.Extra,
                        expected: null,
                        actual: "stale.md",
                        location: extraLocation),
                ]));

        var findings = RouteGeneratedEntryDoctorInspector.Inspect([observation]).ToArray();

        var missing = Assert.Single(
            findings,
            finding => finding.Kind == DoctorFindingKind.RouteGeneratedEntryMissing);
        Assert.Equal(DoctorSubjectKind.GeneratedRegion, missing.Subject.Kind);
        Assert.Equal(cataloguePath, missing.Subject.Path);
        Assert.Equal(cataloguePath, missing.Provenance.Path);
        Assert.Equal("beta-probe.md", missing.Subject.Identifier);
        Assert.Collection(
            missing.Evidence,
            evidence =>
            {
                var comparison = Assert.IsType<DoctorComparisonEvidence>(evidence);
                Assert.Equal("beta-probe.md", comparison.Expected);
                Assert.Equal("absent", comparison.Actual);
            },
            evidence =>
            {
                var authored = Assert.IsType<DoctorAuthoredValueEvidence>(evidence);
                Assert.Equal("absent", authored.Value);
                Assert.Null(authored.Location);
            });

        var extra = Assert.Single(
            findings,
            finding => finding.Kind == DoctorFindingKind.RouteGeneratedEntryExtra);
        Assert.Equal(cataloguePath, extra.Subject.Path);
        Assert.Equal("stale.md", extra.Subject.Identifier);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor targets generated-navigation actions at their owning catalogue")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void GeneratedNavigationActionsUseCataloguePathForEveryTargetedObservation()
    {
        const string cataloguePath = ".agents/catalogue/_catalogue.md";
        const string childPath = ".agents/catalogue/child.md";
        const string overwritePath = ".agents/catalogue/child.overwrite.md";
        var location = new SourceLocation(
            line: 7,
            column: 3,
            byteOffset: 50,
            byteLength: 14);
        var generatedObservation = DoctorGeneratedNavigationTargetObservation.Available(
            cataloguePath,
            OperationalGeneratedNavigationState.Changed,
            new DoctorGeneratedNavigationContent(
                SourceGeneratedEntriesFacts.Complete([]),
                [],
                [new RouteGeneratedEntryComparison(
                    RouteGeneratedEntryComparisonKind.Missing,
                    expected: childPath,
                    actual: null,
                    location: null)]));

        var region = Assert.Single(
            RouteDoctorInspector.Inspect(
                View([], [], generatedNavigation: [generatedObservation])).Findings,
            finding => finding.Kind == DoctorFindingKind.RouteGeneratedRegionStale);
        var entry = Assert.Single(
            RouteGeneratedEntryDoctorInspector.Inspect([generatedObservation]),
            finding => finding.Kind == DoctorFindingKind.RouteGeneratedEntryMissing);
        var overwrite = Assert.Single(
            RouteShapeDoctorInspector.Inspect(
                [RouteShapeObservation.OverwriteIndependentIndex(
                    cataloguePath,
                    overwritePath,
                    childPath,
                    location)]),
            finding => finding.Kind == DoctorFindingKind.RouteOverwriteIndependentIndex);

        AssertIndexAction(region, cataloguePath);
        AssertIndexAction(entry, cataloguePath);
        AssertIndexAction(overwrite, cataloguePath);
        Assert.Equal(childPath, entry.Subject.Identifier);
        Assert.Equal(overwritePath, overwrite.Evidence
            .OfType<DoctorComparisonEvidence>()
            .Select(evidence => evidence.Actual)
            .Single());
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Readable malformed metadata retains complete route coverage and reports an error")]
    [InlineData((int)GeneratedNavigationRegionUnavailableReason.MetadataInvalid)]
    [InlineData((int)GeneratedNavigationRegionUnavailableReason.MetadataUnavailable)]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ReadableMalformedMetadataRetainsCompleteCoverageAndReportsError(
        int reasonValue)
    {
        var reason = (GeneratedNavigationRegionUnavailableReason)reasonValue;
        const string path = ".agents/source.md";
        var view = View(
            [],
            [],
            metadata:
            [
                new RouteMetadataObservation(
                    path,
                    FrameworkDocumentMetadataFacts.Malformed(
                        FrameworkDocumentMetadataFailureKind.Malformed)),
            ],
            sources:
            [
                Source(
                    path,
                    FileReadState.Complete,
                    issues:
                    [
                        RouteWorkspaceSourceIssue.WithoutLocation(
                            RouteWorkspaceSourceIssueKind.FrontmatterMalformed,
                            path),
                    ]),
            ],
            generatedNavigation:
            [
                GeneratedNavigationUnavailable(
                    path,
                    reason),
            ]);

        var routeReport = RouteDoctorInspector.Inspect(view);
        Assert.Equal(DoctorCoverageState.Complete, routeReport.Coverage);

        var findings = new List<DoctorFinding>();
        WorkspaceSourceDoctorInspector.Inspect(view, findings);

        var finding = Assert.Single(findings);
        Assert.Equal(DoctorFindingKind.WorkspaceFrontmatterMalformed, finding.Kind);
        Assert.Equal(DoctorFindingSeverity.Error, finding.Severity);
        Assert.Equal(path, finding.Subject.Path);
        Assert.Equal(path, finding.Provenance.Path);
        var evidence = Assert.IsType<DoctorStateEvidence>(Assert.Single(finding.Evidence));
        Assert.Equal(DoctorObservedState.Malformed, evidence.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Unavailable source layers retain incomplete coverage and exact read causes")]
    [InlineData((int)FileReadState.AccessDenied, "The source read was denied.")]
    [InlineData((int)FileReadState.InputOutputFailure, "The source read failed.")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnavailableSourceLayersRetainIncompleteCoverageAndExactReadCauses(
        int stateValue,
        string cause)
    {
        var state = (FileReadState)stateValue;
        const string path = ".agents/source.md";
        var view = View(
            [],
            [],
            sources:
            [
                Source(
                    path,
                    state,
                    cause,
                    [
                        RouteWorkspaceSourceIssue.WithoutLocation(
                            RouteWorkspaceSourceIssueKind.ParseIncomplete,
                            path),
                    ]),
            ],
            generatedNavigation:
            [
                GeneratedNavigationUnavailable(
                    path,
                    GeneratedNavigationRegionUnavailableReason.MetadataUnavailable),
            ]);

        var routeReport = RouteDoctorInspector.Inspect(view);
        Assert.Equal(DoctorCoverageState.Incomplete, routeReport.Coverage);

        var findings = new List<DoctorFinding>();
        WorkspaceSourceDoctorInspector.Inspect(view, findings);

        var finding = Assert.Single(findings);
        Assert.Equal(DoctorFindingKind.WorkspaceParseIncomplete, finding.Kind);
        Assert.Equal(DoctorFindingSeverity.Error, finding.Severity);
        Assert.Equal(path, finding.Subject.Path);
        Assert.Equal(path, finding.Provenance.Path);
        Assert.Equal(cause, finding.Message);
        var evidence = Assert.IsType<DoctorStateEvidence>(Assert.Single(finding.Evidence));
        Assert.Equal(DoctorObservedState.Unavailable, evidence.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Unrelated generated-navigation unavailability retains incomplete coverage")]
    [InlineData((int)GeneratedNavigationRegionUnavailableReason.TopologyUnavailable)]
    [InlineData((int)GeneratedNavigationRegionUnavailableReason.ProjectionUnavailable)]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void UnrelatedGeneratedNavigationUnavailabilityRetainsIncompleteCoverage(
        int reasonValue)
    {
        var reason = (GeneratedNavigationRegionUnavailableReason)reasonValue;
        const string path = ".agents/source.md";
        var report = RouteDoctorInspector.Inspect(
            View(
                [],
                [],
                sources: [Source(path, FileReadState.Complete)],
                generatedNavigation: [GeneratedNavigationUnavailable(path, reason)]));

        Assert.Equal(DoctorCoverageState.Incomplete, report.Coverage);
    }

    private static RouteDoctorView View(
        IReadOnlyList<RouteDeclaredRootObservation> roots,
        IReadOnlyList<RouteShapeObservation> shape,
        IReadOnlyList<SourceRouteIssue>? issues = null,
        IReadOnlyList<RouteMetadataObservation>? metadata = null,
        IReadOnlyList<RouteSourceObservation>? sources = null,
        IReadOnlyList<DoctorGeneratedNavigationTargetObservation>? generatedNavigation = null,
        OperationalViewState state = OperationalViewState.Complete)
    {
        var workspace = DoctorOperationTestSupport.Workspace();
        return new RouteDoctorView
        {
            State = state,
            SourceInventory = RouteSourceInventoryState.Present,
            Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = new SourceRouteFacts(
                new SourceRouteTopology([], []),
                [],
                issues ?? [],
                areLoaderRootFactsComplete: true,
                isCancelled: false),
            Metadata = metadata ?? [],
            WorkspaceEntry = new RouteSourceLayerObservation(
                "AGENTS.md",
                FileReadState.Missing,
                Text: null),
            Sources = sources ?? [],
            GeneratedNavigation = generatedNavigation ?? [],
            DeclaredRoots = roots,
            Shape = shape,
        };
    }

    private static RouteSourceObservation Source(
        string path,
        FileReadState state,
        string? cause = null,
        IReadOnlyList<RouteWorkspaceSourceIssue>? issues = null)
    {
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity("source", path),
            new SourceLayer(
                path,
                Path.GetFullPath(Path.Combine(
                    Path.GetTempPath(),
                    "open-forge-doctor-unit",
                    path.Replace('/', Path.DirectorySeparatorChar))),
                SourceDocumentForm.Markdown,
                SourceLayerKind.Base));
        return new RouteSourceObservation
        {
            Source = source,
            Layers =
            [
                new RouteSourceLayerObservation(
                    path,
                    state,
                    state == FileReadState.Complete ? "# Source\n" : null,
                    cause),
            ],
            Document = null,
            AuthoredMetadata = SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed),
            FrameworkMetadata = FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Malformed),
            GeneratedEntries = SourceGeneratedEntriesFacts.Absent,
            Structure = new RouteSourceStructureObservation(
                RouteTitleObservation.NotApplicable(),
                RouteAxiomsObservation.NotApplicable()),
            WorkspaceIssues = issues ?? [],
        };
    }

    private static DoctorGeneratedNavigationTargetObservation GeneratedNavigationUnavailable(
        string path,
        GeneratedNavigationRegionUnavailableReason reason)
        => DoctorGeneratedNavigationTargetObservation.Unavailable(
            path,
            OperationalGeneratedNavigationState.Unavailable,
            new DoctorGeneratedNavigationContent(
                SourceGeneratedEntriesFacts.Absent,
                [],
                EntryComparisons: []),
            new DoctorGeneratedNavigationUnavailability(
                reason,
                "The generated region is unavailable."));

    private static void AssertIndexAction(DoctorFinding finding, string cataloguePath)
    {
        Assert.Equal(DoctorResolutionLane.TargetedOperation, finding.Resolution);
        var action = Assert.Single(finding.Actions);
        Assert.Equal(DoctorNextActionKind.AcceptedOperation, action.Kind);
        Assert.Equal(DoctorNextOperation.Index, action.Operation);
        Assert.Equal($"open-forge index {cataloguePath}", action.Command);
    }
}
