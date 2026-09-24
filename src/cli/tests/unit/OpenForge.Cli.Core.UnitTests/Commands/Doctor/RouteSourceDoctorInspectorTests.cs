using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;
using OpenForge.Cli.Core.Framework.Workspace.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class RouteSourceDoctorInspectorTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Doctor accepts compatible local Axioms, including when the title is Axioms")]
    [InlineData("# Scope\n## Entries\n")]
    [InlineData("# Scope\n## Axioms\n\n## Entries\n")]
    [InlineData("# Scope\n## Axioms\n\n- inherited - No local axioms; loaded ancestor axioms remain active.\n")]
    [InlineData("# Axioms\n## Entries\n")]
    [InlineData("# Axioms\n## Axioms\n\n- A local rule.\n")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void LocalEntrypointsAcceptCompatibleAxioms(string body)
    {
        var findings = Inspect(SourceDocumentForm.CanonicalEntrypoint, body);

        Assert.DoesNotContain(findings, finding => finding.Kind == DoctorFindingKind.RouteAxiomsInvalid);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Doctor keeps Loader Axioms required when missing or empty")]
    [InlineData("# Open Forge Loader\n")]
    [InlineData("# Open Forge Loader\n## Axioms\n\n## Routing\n")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void LoaderMustKeepLocalAxioms(string body)
    {
        var findings = Inspect(SourceDocumentForm.Loader, body);

        AssertRouteAxiomsFinding(findings);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Doctor reports duplicate and malformed Axioms headings")]
    [InlineData("# Scope\n## Axioms\n- One.\n## Axioms\n- Two.\n")]
    [InlineData("# Scope\n### Axioms\n- Rule.\n")]
    [InlineData("# Scope\n## axioms\n- Rule.\n")]
    [InlineData("# Scope\n# Axioms\n")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DuplicateAndMalformedHeadingsAreReported(string body)
    {
        var findings = Inspect(SourceDocumentForm.CanonicalEntrypoint, body);

        AssertRouteAxiomsFinding(findings);
    }

    private static DoctorFinding[] Inspect(SourceDocumentForm form, string body)
    {
        var path = form == SourceDocumentForm.Loader
            ? SourceLogicalPath.LoaderPath
            : ".agents/scope/_scope.md";
        var document = new MarkdownDocumentParser().Parse(body);
        var structure = RouteSourceStructureReader.ReadStructure(
            document,
            form,
            new Utf8SourceMap(body));
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity("scope", path),
            new SourceLayer(
                path,
                PhysicalPath(path),
                form,
                SourceLayerKind.Base));
        var observation = new RouteSourceObservation
        {
            Source = source,
            Layers = [new RouteSourceLayerObservation(path, FileReadState.Complete, body)],
            Document = null,
            AuthoredMetadata = SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.NotApplicable),
            FrameworkMetadata = FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing),
            GeneratedEntries = SourceGeneratedEntriesFacts.Absent,
            Structure = structure,
            WorkspaceIssues = [],
        };
        var workspace = DoctorOperationTestSupport.Workspace();
        var view = new RouteDoctorView
        {
            State = OperationalViewState.Complete,
            SourceInventory = RouteSourceInventoryState.Present,
            Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = new SourceRouteFacts(
                new SourceRouteTopology([], []),
                [],
                [],
                areLoaderRootFactsComplete: true,
                isCancelled: false),
            Metadata = [],
            WorkspaceEntry = new RouteSourceLayerObservation("AGENTS.md", FileReadState.Missing, Text: null),
            Sources = [observation],
            GeneratedNavigation = [],
            DeclaredRoots = [],
            Shape = [],
        };

        return RouteSourceDoctorInspector.Inspect(view).ToArray();
    }

    private static void AssertRouteAxiomsFinding(IReadOnlyList<DoctorFinding> findings)
    {
        var finding = Assert.Single(
            findings,
            candidate => candidate.Kind == DoctorFindingKind.RouteAxiomsInvalid);
        var evidence = Assert.IsType<DoctorStateEvidence>(Assert.Single(finding.Evidence));
        Assert.Equal(DoctorObservedState.Invalid, evidence.State);
    }

    private static string PhysicalPath(string path)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-source-doctor-unit",
            path.Replace('/', Path.DirectorySeparatorChar)));
}
