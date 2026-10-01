using OpenForge.Cli.Core.Commands.Index;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Models.Selection;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models.Formation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.UnitTests.Framework.GeneratedNavigation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Selection;

public sealed class IndexSelectionResolverTests
{
    private const string RootPath = ".agents/root/_root.md";
    private const string ChildPath = ".agents/root/child/_child.md";
    private const string LeafPath = ".agents/root/leaf.md";
    private const string SkillsRootPath = ".agents/skills/_skills.md";
    private const string WorkflowSkillPath = ".agents/skills/use-workflow/SKILL.md";
    private const string ReferencesCataloguePath = ".agents/skills/use-workflow/references/_references.md";
    private const string NestedCataloguePath = ".agents/skills/use-workflow/references/nested/_nested.md";

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index selection closes automatic and explicit rooted entrypoints through Loader"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootedEntrypointClosureIncludesLoaderAndDescendants()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var formation = Formation(loader, child, root);
        var resolver = Resolver();

        var automatic = resolver.Resolve(Request(formation), formation);
        var explicitRoot = resolver.Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionOrigin.AutomaticLoader, automatic.Selection.Origin);
        Assert.Equal([SourceLogicalPath.LoaderPath], automatic.Selection.Sources.Select(source => source.Path));
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: loader,
                formation: formation),
            Assert.Single(automatic.Selection.Sources));
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            Assert.Single(explicitRoot.Selection.Sources));
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, RootPath, ChildPath],
            automatic.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, RootPath, ChildPath],
            explicitRoot.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.All([automatic, explicitRoot], resolution =>
        {
            Assert.True(resolution.IsComplete);
            Assert.Empty(resolution.Findings);
        });
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index bridges rooted Skills to immediate catalogues for default and entrypoint closures")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootedSkillCataloguesExpandDefaultLoaderAndExplicitEntrypointClosures()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var skillsRoot = Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(WorkflowSkillPath, SourceDocumentForm.Skill);
        var references = Source(ReferencesCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint);
        var nested = Source(NestedCataloguePath, SourceDocumentForm.CanonicalEntrypoint);
        var referencePage = Source(".agents/skills/use-workflow/references/guide.md", SourceDocumentForm.Markdown);
        var nestedPage = Source(".agents/skills/use-workflow/references/nested/guide.md", SourceDocumentForm.Markdown);
        var formation = Formation(loader, skillsRoot, skill, references, nested, referencePage, nestedPage);
        var beforeRoots = formation.Topology.LoaderRootPaths.ToArray();
        var beforeNodes = TopologySnapshot(formation);
        var resolver = Resolver();

        var automatic = resolver.Resolve(Request(formation), formation);
        var explicitLoader = resolver.Resolve(Request(formation, SourceLogicalPath.LoaderPath), formation);
        var explicitSkills = resolver.Resolve(Request(formation, SkillsRootPath), formation);
        var explicitCatalogue = resolver.Resolve(Request(formation, references.Identity.CanonicalBasePath), formation);

        var rootedTargets = new[]
        {
            SourceLogicalPath.LoaderPath,
            SkillsRootPath,
            references.Identity.CanonicalBasePath,
            NestedCataloguePath,
        };
        Assert.Equal([SourceLogicalPath.LoaderPath], automatic.Selection.Sources.Select(source => source.Path));
        Assert.Equal([SourceLogicalPath.LoaderPath], explicitLoader.Selection.Sources.Select(source => source.Path));
        Assert.Equal([SkillsRootPath], explicitSkills.Selection.Sources.Select(source => source.Path));
        Assert.Equal(rootedTargets, automatic.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(rootedTargets, explicitLoader.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(rootedTargets, explicitSkills.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            [references.Identity.CanonicalBasePath, NestedCataloguePath],
            explicitCatalogue.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.All([automatic, explicitLoader, explicitSkills, explicitCatalogue], resolution =>
        {
            Assert.True(resolution.IsComplete);
            Assert.Empty(resolution.Findings);
            Assert.DoesNotContain(resolution.Targets, target => target.Base.Form == SourceDocumentForm.Skill);
        });
        Assert.Equal(beforeRoots, formation.Topology.LoaderRootPaths);
        Assert.Equal(beforeNodes, TopologySnapshot(formation));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index explicit native Skill selection remains exposing-parent only")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ExplicitSkillSelectsExposingParentWithoutInvokingCatalogueClosure()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var skillsRoot = Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(WorkflowSkillPath, SourceDocumentForm.Skill);
        var references = Source(ReferencesCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint);
        var formation = Formation(loader, skillsRoot, skill, references);

        var resolution = Resolver().Resolve(Request(formation, WorkflowSkillPath), formation);

        Assert.Equal([WorkflowSkillPath], resolution.Selection.Sources.Select(source => source.Path));
        Assert.Equal([SkillsRootPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(resolution.Targets, target => target.Base.Form == SourceDocumentForm.Skill);
        Assert.True(resolution.IsComplete);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index Skill catalogue lookup recognizes all entrypoint forms and keeps ordinal target order")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void SkillCatalogueLookupSupportsAllEntrypointFormsInOrdinalOrder()
    {
        var catalogueSources = new[]
        {
            Source(".agents/skills/use-workflow/canonical/_canonical.md", SourceDocumentForm.CanonicalEntrypoint),
            Source(".agents/skills/use-workflow/compat-index/index.md", SourceDocumentForm.IndexEntrypoint),
            Source(".agents/skills/use-workflow/compat-underscore-index/_index.md", SourceDocumentForm.UnderscoreIndexEntrypoint),
            Source(".agents/skills/use-workflow/compat-references/references.md", SourceDocumentForm.ReferencesEntrypoint),
            Source(".agents/skills/use-workflow/compat-underscore-references/_references.md", SourceDocumentForm.UnderscoreReferencesEntrypoint),
        };
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var skillsRoot = Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(WorkflowSkillPath, SourceDocumentForm.Skill);
        var formation = Formation([.. catalogueSources.Reverse(), loader, skillsRoot, skill]);

        var resolution = Resolver().Resolve(Request(formation), formation);

        Assert.True(resolution.IsComplete);
        Assert.Equal(
        [
            SourceLogicalPath.LoaderPath,
            SkillsRootPath,
            ".agents/skills/use-workflow/canonical/_canonical.md",
            ".agents/skills/use-workflow/compat-index/index.md",
            ".agents/skills/use-workflow/compat-references/references.md",
            ".agents/skills/use-workflow/compat-underscore-index/_index.md",
            ".agents/skills/use-workflow/compat-underscore-references/_references.md",
        ],
            resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index bridges direct and scoped rooted Skills and accepts zero or multiple catalogues")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DirectAndScopedSkillsSupportZeroOrSeveralImmediateCatalogues()
    {
        const string TeamEntrypointPath = ".agents/skills/team/_team.md";
        const string ScopedEntrypointPath = ".agents/skills/team/scopes/_scopes.md";
        const string ScopedSkillPath = ".agents/skills/team/scopes/skill/SKILL.md";
        const string ScopedCataloguePath = ".agents/skills/team/scopes/skill/manuals/_manuals.md";
        const string MultipleSkillPath = ".agents/skills/multiple/SKILL.md";
        const string FirstCataloguePath = ".agents/skills/multiple/reference/_reference.md";
        const string SecondCataloguePath = ".agents/skills/multiple/recipes/_recipes.md";
        const string EmptySkillPath = ".agents/skills/empty/SKILL.md";

        var sources = new[]
        {
            Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader),
            Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint),
            Source(".agents/skills/direct/SKILL.md", SourceDocumentForm.Skill),
            Source(TeamEntrypointPath, SourceDocumentForm.CanonicalEntrypoint),
            Source(ScopedEntrypointPath, SourceDocumentForm.CanonicalEntrypoint),
            Source(ScopedSkillPath, SourceDocumentForm.Skill),
            Source(ScopedCataloguePath, SourceDocumentForm.CanonicalEntrypoint),
            Source(MultipleSkillPath, SourceDocumentForm.Skill),
            Source(FirstCataloguePath, SourceDocumentForm.CanonicalEntrypoint),
            Source(SecondCataloguePath, SourceDocumentForm.CanonicalEntrypoint),
            Source(EmptySkillPath, SourceDocumentForm.Skill),
        };
        var formation = Formation(sources);

        var resolution = Resolver().Resolve(Request(formation), formation);

        Assert.True(resolution.IsComplete);
        Assert.Contains(ScopedCataloguePath, resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Contains(FirstCataloguePath, resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Contains(SecondCataloguePath, resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(resolution.Targets, target => target.Base.Form == SourceDocumentForm.Skill);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index excludes unrooted, outside-root, nested Skill bridges, hidden sources, and gaps")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void SkillCatalogueBridgeRequiresExistingSkillsRootAncestryAndDoesNotBridgeGaps()
    {
        const string RootSkillPath = ".agents/skills/rooted/SKILL.md";
        const string RootCataloguePath = ".agents/skills/rooted/references/_references.md";
        const string NestedSkillPath = ".agents/skills/rooted/references/nested/SKILL.md";
        const string NestedCataloguePath = ".agents/skills/rooted/references/nested/resources/_resources.md";
        const string GapCataloguePath = ".agents/skills/rooted/references/gap/deep/_deep.md";
        const string UnrootedSkillPath = ".agents/skills/unrooted/deep/SKILL.md";
        const string UnrootedCataloguePath = ".agents/skills/unrooted/deep/references/_references.md";
        const string OutsideRootPath = ".agents/guidance/_guidance.md";
        const string OutsideSkillPath = ".agents/guidance/foreign/SKILL.md";
        const string OutsideCataloguePath = ".agents/guidance/foreign/references/_references.md";
        const string HiddenSkillPath = ".agents/skills/rooted/hidden/SKILL.md";
        const string HiddenCataloguePath = ".agents/skills/rooted/hidden/references/_references.md";

        var sources = new[]
        {
            Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader),
            Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint),
            Source(RootSkillPath, SourceDocumentForm.Skill),
            Source(RootCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint),
            Source(NestedSkillPath, SourceDocumentForm.Skill),
            Source(NestedCataloguePath, SourceDocumentForm.CanonicalEntrypoint),
            Source(GapCataloguePath, SourceDocumentForm.CanonicalEntrypoint),
            Source(UnrootedSkillPath, SourceDocumentForm.Skill),
            Source(UnrootedCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint),
            Source(OutsideRootPath, SourceDocumentForm.CanonicalEntrypoint),
            Source(OutsideSkillPath, SourceDocumentForm.Skill),
            Source(OutsideCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint),
        };
        var hiddenCandidates = new[]
        {
            GeneratedNavigationTestData.Candidate(
                HiddenSkillPath,
                SourceDocumentForm.Skill,
                Physical(HiddenSkillPath)),
            GeneratedNavigationTestData.Candidate(
                HiddenCataloguePath,
                SourceDocumentForm.UnderscoreReferencesEntrypoint,
                Physical(HiddenCataloguePath)),
        };
        var formation = new GeneratedNavigationFormationBuilder().Build(
            GeneratedNavigationTestData.Catalogue(sources, additionalCandidates: hiddenCandidates));

        var resolution = Resolver().Resolve(Request(formation), formation);

        Assert.True(resolution.IsComplete);
        Assert.Contains(RootCataloguePath, resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(
            resolution.Targets,
            source => source.Identity.CanonicalBasePath is NestedCataloguePath
                or GapCataloguePath
                or UnrootedCataloguePath
                or OutsideCataloguePath
                or HiddenCataloguePath);
        Assert.DoesNotContain(
            resolution.Targets,
            source => source.Identity.CanonicalBasePath is NestedSkillPath or UnrootedSkillPath or OutsideSkillPath or HiddenSkillPath);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index rejects multiple entrypoints for one immediate Skill catalogue directory even when empty")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void MultipleEntrypointsForOneSkillCatalogueDirectoryAreTopologyAmbiguous()
    {
        const string ConflictingCataloguePath = ".agents/skills/use-workflow/references/index.md";
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var skillsRoot = Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(WorkflowSkillPath, SourceDocumentForm.Skill);
        var references = Source(ReferencesCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint);
        var conflicting = Source(ConflictingCataloguePath, SourceDocumentForm.IndexEntrypoint);
        var formation = Formation(loader, skillsRoot, skill, references, conflicting);

        var resolution = Resolver().Resolve(Request(formation), formation);

        Assert.Empty(resolution.Targets);
        var finding = Assert.Single(resolution.Findings);
        Assert.Equal(IndexFindingCode.TopologyAmbiguous, finding.Code);
        Assert.Equal(
            [ReferencesCataloguePath, ConflictingCataloguePath],
            finding.Candidates.Select(candidate => candidate.Path));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index overlapping rooted and catalogue selections process each generated region once")]
    [Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootedAndCatalogueOperandsDeduplicateBridgedTargets()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var skillsRoot = Source(SkillsRootPath, SourceDocumentForm.CanonicalEntrypoint);
        var skill = Source(WorkflowSkillPath, SourceDocumentForm.Skill);
        var references = Source(ReferencesCataloguePath, SourceDocumentForm.UnderscoreReferencesEntrypoint);
        var formation = Formation(loader, skillsRoot, skill, references);

        var resolution = Resolver().Resolve(
            Request(formation, SkillsRootPath, ReferencesCataloguePath, ReferencesCataloguePath),
            formation);

        Assert.True(resolution.IsComplete);
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, SkillsRootPath, ReferencesCataloguePath],
            resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            resolution.Targets.Count,
            resolution.Targets.Select(source => source.Identity.CanonicalBasePath).Distinct(StringComparer.Ordinal).Count());
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index selection keeps a complete detached entrypoint local when Loader is missing"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DetachedEntrypointDoesNotInventLoaderOrParent()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var formation = Formation(child, root);

        var resolution = Resolver().Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionScope.Detached, resolution.Selection.Scope);
        Assert.Equal(
            IndexLogicalSourceProjector.Project(
                source: root,
                formation: formation),
            Assert.Single(resolution.Selection.Sources));
        Assert.Equal([RootPath, ChildPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.DoesNotContain(
            resolution.Targets,
            source => source.Identity.CanonicalBasePath == SourceLogicalPath.LoaderPath);
        Assert.True(resolution.IsComplete);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index leaf and overwrite references normalize to one base selection and direct parent target"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void LeafAndOverwriteReferencesSelectOnlyTheDirectParent()
    {
        var parent = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(LeafPath, SourceDocumentForm.Markdown, withOverwrite: true);
        var formation = Formation(leaf, parent);

        var resolution = Resolver().Resolve(
            Request(formation, LeafPath, ".agents/root/leaf.overwrite.md", LeafPath),
            formation);

        Assert.Equal([LeafPath], resolution.Selection.Sources.Select(source => source.Path));
        Assert.Equal([RootPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.True(resolution.IsComplete);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index overlapping selections union target closure once in canonical order"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void DuplicateAndOverlappingSelectionsDoNotDuplicateTargets()
    {
        var root = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var child = Source(ChildPath, SourceDocumentForm.CanonicalEntrypoint);
        var leaf = Source(LeafPath, SourceDocumentForm.Markdown);
        var formation = Formation(leaf, child, root);

        var resolution = Resolver().Resolve(
            Request(formation, ChildPath, LeafPath, RootPath, RootPath),
            formation);

        Assert.Equal([RootPath, ChildPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            [RootPath, ChildPath, LeafPath],
            resolution.Selection.Sources.Select(source => source.Path));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index uses compatible physical aliases once and blocks relevant incompatible aliases"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void PhysicalAliasesUseFormationCompatibilityAndAuthoritativePathIdentity()
    {
        var parent = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var sharedLeaf = Physical("shared/leaf.md");
        var alpha = Source(".agents/root/alpha.md", SourceDocumentForm.Markdown, sharedLeaf);
        var zeta = Source(".agents/root/zeta.md", SourceDocumentForm.Markdown, sharedLeaf);
        var compatible = Formation(zeta, parent, alpha);

        var normalized = Resolver().Resolve(
            Request(compatible, alpha.Identity.CanonicalBasePath, zeta.Identity.CanonicalBasePath),
            compatible);

        Assert.Equal([alpha.Identity.CanonicalBasePath], normalized.Selection.Sources.Select(source => source.Path));
        Assert.Equal([RootPath], normalized.Targets.Select(source => source.Identity.CanonicalBasePath));

        var sharedRoot = Physical("shared/root.md");
        var firstRoot = Source(".agents/alpha/_alpha.md", SourceDocumentForm.CanonicalEntrypoint, sharedRoot);
        var secondRoot = Source(".agents/zeta/_zeta.md", SourceDocumentForm.CanonicalEntrypoint, sharedRoot);
        var incompatible = Formation(secondRoot, firstRoot);

        var blocked = Resolver().Resolve(Request(incompatible, firstRoot.Identity.CanonicalBasePath), incompatible);

        Assert.Empty(blocked.Targets);
        Assert.Equal(IndexFindingCode.SourceUnsafe, Assert.Single(blocked.Findings).Code);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index source ID collisions block IDs while exact paths remain disambiguated"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ExactPathsDisambiguateSourceIdCollisions()
    {
        var loader = Source(SourceLogicalPath.LoaderPath, SourceDocumentForm.Loader);
        var entrypoint = Source(".agents/memory/_memory.md", SourceDocumentForm.CanonicalEntrypoint);
        var collidingLeaf = Source(".agents/memory.md", SourceDocumentForm.Markdown);
        var formation = Formation(collidingLeaf, entrypoint, loader);

        var ambiguous = Resolver().Resolve(Request(formation, "memory"), formation);
        var exact = Resolver().Resolve(Request(formation, entrypoint.Identity.CanonicalBasePath), formation);

        Assert.Empty(ambiguous.Targets);
        Assert.Equal(IndexFindingCode.SourceAmbiguous, Assert.Single(ambiguous.Findings).Code);
        Assert.True(exact.IsComplete);
        Assert.Equal(
            [SourceLogicalPath.LoaderPath, entrypoint.Identity.CanonicalBasePath],
            exact.Targets.Select(source => source.Identity.CanonicalBasePath));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Index root safety and availability own explicit failure before source resolution"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    [InlineData(SourceCatalogueIssueCode.RootUnsafe, IndexFindingCode.WorkspaceUnsafe)]
    [InlineData(SourceCatalogueIssueCode.RootUnavailable, IndexFindingCode.DiscoveryIncomplete)]
    internal void RootBoundaryFailuresShortCircuitExplicitResolution(
        SourceCatalogueIssueCode issueCode,
        IndexFindingCode expectedCode)
    {
        var formation = Formation([], RootIssue(issueCode));

        var resolution = Resolver().Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexSelectionScope.NotEstablished, resolution.Selection.Scope);
        Assert.Empty(resolution.Targets);
        Assert.Equal(expectedCode, Assert.Single(resolution.Findings).Code);
        Assert.DoesNotContain(resolution.Findings, finding => finding.Code == IndexFindingCode.InvalidSource);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index root missing is an automatic Loader failure but adds no explicit workspace finding"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void RootMissingKeepsAutomaticAndExplicitMeaningsDistinct()
    {
        var formation = Formation([], RootIssue(SourceCatalogueIssueCode.RootMissing));
        var resolver = Resolver();

        var automatic = resolver.Resolve(Request(formation), formation);
        var explicitSource = resolver.Resolve(Request(formation, RootPath), formation);

        Assert.Equal(IndexFindingCode.TargetUnexposed, Assert.Single(automatic.Findings).Code);
        Assert.Equal(IndexFindingCode.InvalidSource, Assert.Single(explicitSource.Findings).Code);
        Assert.DoesNotContain(
            explicitSource.Findings,
            finding => finding.Code is IndexFindingCode.WorkspaceUnavailable or IndexFindingCode.DiscoveryIncomplete);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Index selection resolution enforces complete and incomplete target coherence"), Trait("Feature", "index-command"), Trait("Evidence", "Unit")]
    public void ResolutionRejectsNullFindingsAndImpossibleExecutableStates()
    {
        var target = Source(RootPath, SourceDocumentForm.CanonicalEntrypoint);
        var projected = new IndexLogicalSource(
            target.Identity.AutomaticId,
            target.Identity.CanonicalBasePath,
            IndexLogicalSourceScope.Detached);
        var selection = new IndexSelection(
            IndexSelectionOrigin.ExplicitSources,
            IndexSelectionScope.Detached,
            [projected]);
        var finding = new IndexFinding(
            IndexFindingCode.InvalidSource,
            sourceOccurrence: 1,
            source: null,
            cause: "The source reference is invalid.",
            candidates: []);
        var invalidTarget = Source(LeafPath, SourceDocumentForm.Markdown);
        var physicalAliasTarget = Source(
            ".agents/alias/_alias.md",
            SourceDocumentForm.CanonicalEntrypoint,
            target.Base.PhysicalPath);

        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [target], [finding]));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [], []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(
            IndexSelection.NotEstablished(IndexSelectionOrigin.ExplicitSources),
            [target],
            []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [], new IndexFinding[1]));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [invalidTarget], []));
        Assert.Throws<ArgumentException>(() => new IndexSelectionResolution(selection, [target, physicalAliasTarget], []));

        var complete = new IndexSelectionResolution(selection, [target], []);
        Assert.True(complete.IsComplete);
        Assert.Same(target, Assert.Single(complete.Targets));
    }

    private static IndexSelectionResolver Resolver()
        => new(new SourceReferenceResolver((_, path) =>
            PhysicalPathResolution.Classified(PhysicalPathState.Missing, path)));

    private static IndexRequest Request(
        GeneratedNavigationFormation formation,
        params string[] sourceReferences)
        => new(formation.Catalogue.Workspace, sourceReferences, IndexMode.Apply);

    private static GeneratedNavigationFormation Formation(params SourceLogicalSource[] sources)
        => Formation(sources, []);

    private static GeneratedNavigationFormation Formation(
        IEnumerable<SourceLogicalSource> sources,
        params SourceCatalogueIssue[] issues)
        => new GeneratedNavigationFormationBuilder().Build(
            GeneratedNavigationTestData.Catalogue(sources, issues: issues));

    private static SourceCatalogueIssue RootIssue(SourceCatalogueIssueCode code)
        => new(
            code,
            SourceLogicalPath.AgentsRoot,
            relatedPaths: [],
            scopePhysicalPath: null,
            failure: null);

    private static string[] TopologySnapshot(GeneratedNavigationFormation formation)
        => formation.Topology.Nodes.Select(node =>
            $"{node.Identity.CanonicalBasePath}|{node.ParentState}|{string.Join(',', node.ParentPaths)}|{string.Join(',', node.ChildPaths)}")
            .ToArray();

    private static SourceLogicalSource Source(
        string canonicalPath,
        SourceDocumentForm form,
        string? physicalPath = null,
        bool withOverwrite = false)
    {
        var resolvedPhysicalPath = physicalPath ?? Physical(canonicalPath);
        var identity = new SourceLogicalIdentity(
            SourceIdentity.DeriveId(canonicalPath)
                ?? throw new ArgumentException("An Index selection test source requires a derived ID.", nameof(canonicalPath)),
            canonicalPath);
        var baseLayer = new SourceLayer(
            canonicalPath,
            resolvedPhysicalPath,
            form,
            SourceLayerKind.Base);
        if (!withOverwrite)
        {
            return new SourceLogicalSource(identity, baseLayer);
        }

        var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
        var overwriteLayer = new SourceLayer(
            overwritePath,
            Physical(overwritePath),
            SourceDocumentForm.OverwriteCompanion,
            SourceLayerKind.Overwrite);
        return new SourceLogicalSource(identity, baseLayer, overwriteLayer);
    }

    private static string Physical(string relativePath)
        => Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "open-forge-index-selection-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
}
