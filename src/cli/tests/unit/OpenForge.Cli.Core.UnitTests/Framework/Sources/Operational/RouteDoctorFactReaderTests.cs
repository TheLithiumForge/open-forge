using OpenForge.Cli.Core.UnitTests.Commands.Doctor;
using OpenForge.Cli.Core.Framework.Documents.Markdown;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.TypedReads.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational;

public sealed class RouteDoctorFactReaderTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Doctor facts retain the exact Loader-declared missing root")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void DeclaredRootReaderRetainsMissingDestinationAndLoader()
    {
        var routes = new SourceRouteFacts(
            new SourceRouteTopology([], []),
            [],
            [new SourceRouteIssue(
                SourceRouteIssueCode.LoaderDestinationMissing,
                ".agents/missing/AGENTS.md",
                [".agents/loader.md"],
                occurrence: 0,
                "The Loader destination is missing.")],
            areLoaderRootFactsComplete: false,
            isCancelled: false);

        var root = Assert.Single(RouteDoctorFactReader.ReadDeclaredRoots(routes));

        Assert.Equal(RouteDeclaredRootState.Missing, root.State);
        Assert.Equal(".agents/missing/AGENTS.md", root.Path);
        Assert.Equal(".agents/loader.md", root.LoaderPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Doctor facts distinguish unreachable leaves from detached trees")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ShapeReaderClassifiesOnlyMaterializedRouteRelationships()
    {
        var tree = Identity("tree", ".agents/tree/AGENTS.md");
        var child = Identity("tree/child", ".agents/tree/child.md");
        var orphan = Identity("orphan", ".agents/orphan.md");
        var topology = new SourceRouteTopology(
        [
            new SourceRouteNode(
                tree,
                SourceRouteParentState.None,
                [],
                [child.CanonicalBasePath]),
            new SourceRouteNode(
                child,
                SourceRouteParentState.Resolved,
                [tree.CanonicalBasePath],
                []),
            new SourceRouteNode(
                orphan,
                SourceRouteParentState.None,
                [],
                []),
        ], []);
        var routes = new SourceRouteFacts(
            topology,
            [
                new SourceRouteFact(tree, SourceRouteState.Unrouted, isIdentityUnique: true),
                new SourceRouteFact(child, SourceRouteState.Unrouted, isIdentityUnique: true),
                new SourceRouteFact(orphan, SourceRouteState.Unrouted, isIdentityUnique: true),
            ],
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: false);
        var workspace = OpenForge.Cli.Core.UnitTests.Commands.Doctor.DoctorOperationTestSupport.Workspace();
        var inspection = new RouteSourceInspection
        {
            Catalogue = new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = routes,
            WorkspaceEntry = new RouteSourceLayerObservation(
                "AGENTS.md",
                FileReadState.Missing,
                Text: null),
            Sources = [],
            State = OperationalViewState.Complete,
        };

        var shape = RouteDoctorFactReader.ReadShape(inspection);

        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Detached
            && observation.Path == tree.CanonicalBasePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == orphan.CanonicalBasePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == child.CanonicalBasePath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Doctor omits only complete readable native Skill metadata")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void MetadataReaderOmitsOnlyCompleteReadableNativeSkill()
    {
        const string skillPath = ".agents/skills/use-workflow/SKILL.md";
        const string ordinaryPath = ".agents/guidance/notes.md";
        var skill = Observation(
            skillPath,
            SourceDocumentForm.Skill,
            SourceAuthoredMetadataFacts.Complete("Use workflow", []),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var ordinary = Observation(
            ordinaryPath,
            SourceDocumentForm.Markdown,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));

        var metadata = RouteDoctorFactReader.ReadMetadata(Inspection([skill, ordinary]));

        var observation = Assert.Single(metadata);
        Assert.Equal(ordinaryPath, observation.Path);
        Assert.Equal(FrameworkDocumentMetadataState.Missing, observation.Facts.State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Route Doctor retains native Skill metadata for incomplete or unreadable native facts")]
    [InlineData((int)SourceAuthoredMetadataState.Missing, (int)FileReadState.Complete)]
    [InlineData((int)SourceAuthoredMetadataState.Malformed, (int)FileReadState.Complete)]
    [InlineData((int)SourceAuthoredMetadataState.Complete, (int)FileReadState.AccessDenied)]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void MetadataReaderRetainsIncompleteOrUnreadableNativeSkill(
        int authoredStateValue,
        int readStateValue)
    {
        const string skillPath = ".agents/skills/use-workflow/SKILL.md";
        var authoredState = (SourceAuthoredMetadataState)authoredStateValue;
        var authoredMetadata = authoredState == SourceAuthoredMetadataState.Complete
            ? SourceAuthoredMetadataFacts.Complete("Use workflow", [])
            : SourceAuthoredMetadataFacts.WithoutValues(authoredState);
        var skill = Observation(
            skillPath,
            SourceDocumentForm.Skill,
            authoredMetadata,
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing),
            (FileReadState)readStateValue,
            Document("# Use workflow\n"));

        var observation = Assert.Single(RouteDoctorFactReader.ReadMetadata(Inspection([skill])));

        Assert.Equal(skillPath, observation.Path);
        Assert.Equal(FrameworkDocumentMetadataState.Missing, observation.Facts.State);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Doctor excludes only healthy native Skill resource descendants from route shape findings")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ShapeReaderExcludesOnlyHealthyNativeSkillResourceDescendants()
    {
        const string skillPath = ".agents/skills/use-workflow/SKILL.md";
        const string resourcePath = ".agents/skills/use-workflow/references/_references.md";
        const string unreadableResourcePath = ".agents/skills/use-workflow/unreadable.md";
        const string malformedResourcePath = ".agents/skills/use-workflow/malformed.md";
        const string lookalikePath = ".agents/skills-like/use-workflow/references/_references.md";
        const string prefixSiblingPath = ".agents/skills/use-workflow-extra/references/_references.md";
        const string ordinaryPath = ".agents/guidance/notes.md";
        var skill = Observation(
            skillPath,
            SourceDocumentForm.Skill,
            SourceAuthoredMetadataFacts.Complete("Use workflow", []),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var resource = Observation(
            resourcePath,
            SourceDocumentForm.UnderscoreReferencesEntrypoint,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var unreadableResource = Observation(
            unreadableResourcePath,
            SourceDocumentForm.Markdown,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing),
            FileReadState.AccessDenied,
            Document("# Unreadable\n"));
        var malformedResource = Observation(
            malformedResourcePath,
            SourceDocumentForm.Markdown,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Malformed),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Malformed));
        var lookalike = Observation(
            lookalikePath,
            SourceDocumentForm.UnderscoreReferencesEntrypoint,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var prefixSibling = Observation(
            prefixSiblingPath,
            SourceDocumentForm.UnderscoreReferencesEntrypoint,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var ordinary = Observation(
            ordinaryPath,
            SourceDocumentForm.Markdown,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing));
        var sources = new[]
        {
            skill,
            resource,
            unreadableResource,
            malformedResource,
            lookalike,
            prefixSibling,
            ordinary,
        };
        var topology = new SourceRouteTopology(
        [
            new SourceRouteNode(
                skill.Source.Identity,
                SourceRouteParentState.None,
                [],
                [resourcePath]),
            new SourceRouteNode(
                resource.Source.Identity,
                SourceRouteParentState.Resolved,
                [skillPath],
                []),
            new SourceRouteNode(
                unreadableResource.Source.Identity,
                SourceRouteParentState.None,
                [],
                []),
            new SourceRouteNode(
                malformedResource.Source.Identity,
                SourceRouteParentState.None,
                [],
                []),
            new SourceRouteNode(
                lookalike.Source.Identity,
                SourceRouteParentState.None,
                [],
                []),
            new SourceRouteNode(
                prefixSibling.Source.Identity,
                SourceRouteParentState.None,
                [],
                []),
            new SourceRouteNode(
                ordinary.Source.Identity,
                SourceRouteParentState.None,
                [],
                []),
        ], []);
        var routes = new SourceRouteFacts(
            topology,
            sources.Select(source => new SourceRouteFact(
                source.Source.Identity,
                SourceRouteState.Unrouted,
                isIdentityUnique: true)),
            [],
            areLoaderRootFactsComplete: true,
            isCancelled: false);

        var shape = RouteDoctorFactReader.ReadShape(Inspection(sources, routes));

        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Detached
            && observation.Path == skillPath);
        Assert.DoesNotContain(shape, observation => observation.Path == resourcePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == unreadableResourcePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == malformedResourcePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == lookalikePath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == prefixSiblingPath);
        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.Unreachable
            && observation.Path == ordinaryPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Route Doctor retains duplicate and generated overwrite shape observations")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ShapeReaderRetainsOtherShapeObservations()
    {
        const string sourcePath = ".agents/ordinary.md";
        const string overwritePath = ".agents/ordinary.overwrite.md";
        var source = Observation(
            sourcePath,
            SourceDocumentForm.Markdown,
            SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.Missing),
            FrameworkDocumentMetadataFacts.WithoutValues(FrameworkDocumentMetadataState.Missing),
            generatedEntries: SourceGeneratedEntriesFacts.Complete(
            [
                new SourceGeneratedEntry(
                    "Ordinary overwrite",
                    "ordinary.overwrite.md",
                    [],
                    new MarkdownTextSpan(0, 1)),
            ]));
        var physicalRoot = Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-doctor-fact-unit"));
        var catalogue = new SourceCatalogue(
            DoctorOperationTestSupport.Workspace(),
            [new SourceCandidate(
                overwritePath,
                SourceDocumentForm.OverwriteCompanion,
                null,
                PhysicalPathState.Contained,
                Path.Combine(physicalRoot, "ordinary.overwrite.md"),
                physicalRoot)],
            [],
            [new SourceCatalogueIssue(
                SourceCatalogueIssueCode.EntrypointAmbiguous,
                sourcePath,
                [sourcePath, ".agents/other.md"],
                physicalRoot,
                failure: null)],
            isCancelled: false);

        var shape = RouteDoctorFactReader.ReadShape(
            Inspection([source], catalogue: catalogue));

        Assert.Contains(shape, observation =>
            observation.Kind == RouteShapeObservationKind.EntrypointDuplicate);
        var overwrite = Assert.Single(shape, observation =>
            observation.Kind == RouteShapeObservationKind.OverwriteIndependentIndex);
        Assert.Equal(sourcePath, overwrite.Path);
        Assert.Equal(overwritePath, Assert.Single(overwrite.RelatedPaths));
    }

    private static RouteSourceInspection Inspection(
        IReadOnlyList<RouteSourceObservation> sources,
        SourceRouteFacts? routes = null,
        SourceCatalogue? catalogue = null)
    {
        var workspace = catalogue?.Workspace
            ?? DoctorOperationTestSupport.Workspace();
        return new RouteSourceInspection
        {
            Catalogue = catalogue ?? new SourceCatalogue(workspace, [], [], [], isCancelled: false),
            Routes = routes ?? new SourceRouteFacts(
                new SourceRouteTopology([], []),
                [],
                [],
                areLoaderRootFactsComplete: true,
                isCancelled: false),
            WorkspaceEntry = new RouteSourceLayerObservation(
                "AGENTS.md",
                FileReadState.Missing,
                Text: null),
            Sources = sources,
            State = OperationalViewState.Complete,
        };
    }

    private static RouteSourceObservation Observation(
        string path,
        SourceDocumentForm form,
        SourceAuthoredMetadataFacts authoredMetadata,
        FrameworkDocumentMetadataFacts frameworkMetadata,
        FileReadState readState = FileReadState.Complete,
        MarkdownDocumentFacts? document = null,
        SourceGeneratedEntriesFacts? generatedEntries = null)
    {
        var source = new SourceLogicalSource(
            new SourceLogicalIdentity(path, path),
            new SourceLayer(
                path,
                PhysicalPath(path),
                form,
                SourceLayerKind.Base));
        var retainedDocument = document ?? Document("# Source\n");
        return new RouteSourceObservation
        {
            Source = source,
            Layers =
            [
                new RouteSourceLayerObservation(
                    path,
                    readState,
                    readState == FileReadState.Complete ? retainedDocument.Source : null),
            ],
            Document = retainedDocument,
            AuthoredMetadata = authoredMetadata,
            FrameworkMetadata = frameworkMetadata,
            GeneratedEntries = generatedEntries ?? SourceGeneratedEntriesFacts.Absent,
            Structure = new RouteSourceStructureObservation(
                RouteTitleObservation.NotApplicable(),
                RouteAxiomsObservation.NotApplicable()),
            WorkspaceIssues = [],
        };
    }

    private static MarkdownDocumentFacts Document(string source)
        => new MarkdownDocumentParser().Parse(source);

    private static string PhysicalPath(string path)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "route-doctor-fact-unit",
            path.Replace('/', Path.DirectorySeparatorChar)));

    private static SourceLogicalIdentity Identity(string id, string path)
        => new(id, path);
}
