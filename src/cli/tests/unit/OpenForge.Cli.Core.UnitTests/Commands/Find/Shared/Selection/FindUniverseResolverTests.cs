using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Find.Shared.Selection;

public sealed class FindUniverseResolverTests
{
    public static TheoryData<SelectorScenario> SelectorScenarios => new()
    {
        new SelectorScenario(
            "resolved",
            [
                new SelectorExpectation(
                    "docs/with-overwrite",
                    SourceReferenceKind.SourceId,
                    FindSelectorResolution.Resolved,
                    FindSourceKind.Ordinary,
                    FindSelectorExpansion.Source,
                    "docs/with-overwrite",
                    ".agents/docs/with-overwrite.md",
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    ".agents/docs/guide.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Resolved,
                    FindSourceKind.Ordinary,
                    FindSelectorExpansion.Source,
                    "docs/guide",
                    ".agents/docs/guide.md",
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    ".agents/docs/_docs.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Resolved,
                    FindSourceKind.Entrypoint,
                    FindSelectorExpansion.Folder,
                    "docs",
                    ".agents/docs/_docs.md",
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    ".agents/skills/search/SKILL.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Resolved,
                    FindSourceKind.Skill,
                    FindSelectorExpansion.Folder,
                    "skills/search",
                    ".agents/skills/search/SKILL.md",
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    ".agents/loader.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Resolved,
                    FindSourceKind.Loader,
                    FindSelectorExpansion.Folder,
                    "loader",
                    ".agents/loader.md",
                    false,
                    [],
                    []),
            ]),
        new SelectorScenario(
            "unresolved",
            [
                new SelectorExpectation(
                    "docs//bad",
                    SourceReferenceKind.SourceId,
                    FindSelectorResolution.Invalid,
                    null,
                    null,
                    null,
                    null,
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    "docs/guide",
                    SourceReferenceKind.SourceId,
                    FindSelectorResolution.Ambiguous,
                    null,
                    null,
                    null,
                    null,
                    false,
                    ["docs/guide", "docs/guide"],
                    [".agents/docs/guide.md", ".agents/docs/guide/_guide.md"]),
                new SelectorExpectation(
                    "docs/missing",
                    SourceReferenceKind.SourceId,
                    FindSelectorResolution.Unknown,
                    null,
                    null,
                    null,
                    null,
                    false,
                    [],
                    []),
                new SelectorExpectation(
                    ".agents/docs/hidden.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Unsupported,
                    null,
                    null,
                    null,
                    null,
                    true,
                    [],
                    []),
            ]),
        new SelectorScenario(
            "unsafe",
            [
                new SelectorExpectation(
                    ".agents/docs/unsafe.md",
                    SourceReferenceKind.SourcePath,
                    FindSelectorResolution.Unsafe,
                    null,
                    null,
                    null,
                    null,
                    true,
                    [],
                    []),
            ]),
    };

    public static TheoryData<AlgebraScenario> AlgebraScenarios => new()
    {
        new AlgebraScenario(
            "union-order",
            [
                new AlgebraExpectation(
                    [
                        ".agents/docs/_docs.md",
                        ".agents/docs/_docs.md",
                        ".agents/skills/search/SKILL.md",
                    ],
                    [],
                    [
                        ".agents/docs/_docs.md",
                        ".agents/docs/guide.md",
                        ".agents/docs/guide/_guide.md",
                        ".agents/docs/with-overwrite.md",
                        ".agents/skills/search/SKILL.md",
                    ]),
                new AlgebraExpectation(
                    [
                        ".agents/skills/search/SKILL.md",
                        ".agents/docs/_docs.md",
                    ],
                    [],
                    [
                        ".agents/docs/_docs.md",
                        ".agents/docs/guide.md",
                        ".agents/docs/guide/_guide.md",
                        ".agents/docs/with-overwrite.md",
                        ".agents/skills/search/SKILL.md",
                    ]),
            ]),
        new AlgebraScenario(
            "exclusion-wins",
            [
                new AlgebraExpectation(
                    [".agents/docs/_docs.md"],
                    [".agents/docs/guide.md"],
                    [
                        ".agents/docs/_docs.md",
                        ".agents/docs/guide/_guide.md",
                        ".agents/docs/with-overwrite.md",
                    ]),
                new AlgebraExpectation(
                    [".agents/docs/with-overwrite.md"],
                    [".agents/docs/with-overwrite.overwrite.md"],
                    []),
            ]),
    };

    public static TheoryData<ScopeScenario> ScopeScenarios => new()
    {
        new ScopeScenario(
            "entrypoint-scope",
            [
                new ScopeExpectation(
                    ".agents/docs/_docs.md",
                    [
                        ".agents/docs/_docs.md",
                        ".agents/docs/guide.md",
                        ".agents/docs/guide/_guide.md",
                        ".agents/docs/unreadable.md",
                        ".agents/docs/with-overwrite.md",
                        ".agents/docs/with-overwrite.overwrite.md",
                    ],
                    [".agents/docs/unreadable.md"]),
            ]),
        new ScopeScenario(
            "narrow-sources",
            [
                new ScopeExpectation(
                    ".agents/skills/search/SKILL.md",
                    [".agents/skills/search/SKILL.md"],
                    []),
                new ScopeExpectation(
                    ".agents/docs/guide.md",
                    [".agents/docs/guide.md"],
                    []),
            ]),
    };

    public static TheoryData<CatalogueIssueScenario> CatalogueIssueScenarios => new()
    {
        new CatalogueIssueScenario(
            "root-boundaries",
            [
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.RootMissing,
                    FindFindingCode.WorkspaceUnavailable,
                    CliSemanticStatus.Blocked,
                    null),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.RootUnsafe,
                    FindFindingCode.WorkspaceUnsafe,
                    CliSemanticStatus.Blocked,
                    null),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.RootUnavailable,
                    FindFindingCode.WorkspaceUnavailable,
                    CliSemanticStatus.Blocked,
                    null),
            ]),
        new CatalogueIssueScenario(
            "candidate-boundaries",
            [
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.DirectoryUnavailable,
                    FindFindingCode.InspectionUnavailable,
                    CliSemanticStatus.Incomplete,
                    null),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.CandidateUnsafe,
                    FindFindingCode.CandidateUnsafe,
                    CliSemanticStatus.Incomplete,
                    1),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.CandidateUnavailable,
                    FindFindingCode.InspectionUnavailable,
                    CliSemanticStatus.Incomplete,
                    1),
            ]),
        new CatalogueIssueScenario(
            "identity-and-pairing",
            [
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.IdentityUnavailable,
                    FindFindingCode.LayerUnresolved,
                    CliSemanticStatus.Incomplete,
                    1),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.IdentityCollision,
                    FindFindingCode.IdentityCollision,
                    CliSemanticStatus.Attention,
                    2),
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.OrphanOverwrite,
                    FindFindingCode.LayerUnresolved,
                    CliSemanticStatus.Incomplete,
                    1),
            ]),
        new CatalogueIssueScenario(
            "aliases",
            [
                new CatalogueIssueExpectation(
                    SourceCatalogueIssueCode.PhysicalAlias,
                    null,
                    null,
                    2),
            ]),
    };

    [Theory(DisplayName = "Find selectors retain source-reference form, expansion kind, and typed unresolved outcomes")]
    [MemberData(nameof(SelectorScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void SelectorsPreserveFormsKindsExpansionsAndTypedOutcomes(SelectorScenario scenario)
    {
        foreach (var expectation in scenario.Expectations)
        {
            var fixture = CreateCatalogueFixture();
            var physicalCalls = 0;
            var resolver = new FindUniverseResolver((workspace, path) =>
            {
                physicalCalls++;
                if (expectation.Resolution == FindSelectorResolution.Unsafe)
                {
                    var outside = Path.GetFullPath(Path.Combine(
                        workspace.PhysicalRoot,
                        "..",
                        "open-forge-find-universe-red-outside",
                        "unsafe.md"));
                    return PhysicalPathResolution.Classified(
                        PhysicalPathState.External,
                        path,
                        outside);
                }

                return PhysicalPathResolution.Contained(
                    path,
                    Physical(workspace.PhysicalRoot, path));
            });

            var resolution = resolver.Resolve(CreateInput(fixture, [expectation.Value], []));
            var selector = Assert.Single(resolution.Universe.Include);

            Assert.Equal(expectation.Value, selector.Value);
            Assert.Equal(expectation.Form, selector.Form);
            Assert.Equal(expectation.Resolution, selector.Resolution);
            Assert.Equal(expectation.SourceKind, selector.SourceKind);
            Assert.Equal(expectation.Expansion, selector.Expansion);
            Assert.Equal(expectation.CandidateIds, selector.Candidates.Select(candidate => candidate.Id));
            Assert.Equal(expectation.CandidatePaths, selector.Candidates.Select(candidate => candidate.Path));

            if (expectation.ExpectedId is null)
            {
                Assert.Null(selector.Identity);
            }
            else
            {
                var identity = Assert.IsType<FindSourceIdentity>(selector.Identity);
                Assert.Equal(expectation.ExpectedId, identity.Id);
                Assert.Equal(expectation.ExpectedPath, identity.Path);
            }

            Assert.Equal(expectation.ProbesPhysical ? 1 : 0, physicalCalls);
        }
    }

    [Theory(DisplayName = "Find include and exclude algebra deduplicates logical sources, is order independent, and gives exclusion priority")]
    [MemberData(nameof(AlgebraScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void IncludeExcludeAlgebraIsDeduplicatedOrderIndependentAndExclusionFirst(AlgebraScenario scenario)
    {
        foreach (var expectation in scenario.Expectations)
        {
            var fixture = CreateCatalogueFixture();
            var resolution = new FindUniverseResolver(ContainedPath).Resolve(
                CreateInput(fixture, expectation.Include, expectation.Exclude));

            Assert.Equal(FindUniverseMode.Filtered, resolution.Universe.Mode);
            Assert.Equal(
                expectation.Include,
                resolution.Universe.Include.Select(selector => selector.Value));
            Assert.Equal(
                expectation.Exclude,
                resolution.Universe.Exclude.Select(selector => selector.Value));
            Assert.Equal(
                expectation.ExpectedPaths,
                resolution.Selection.Sources.Select(source => source.Identity.CanonicalBasePath));
            Assert.Equal(expectation.ExpectedPaths.Length, resolution.Universe.CandidateCount);
            Assert.Equal(
                resolution.Selection.Sources.Select(source => source.Identity.CanonicalBasePath).Distinct(StringComparer.Ordinal).Count(),
                resolution.Selection.Sources.Count);
        }
    }

    [Theory(DisplayName = "Find effective selection projects only in-scope candidates and catalogue issues")]
    [MemberData(nameof(ScopeScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void EffectiveSelectionProjectsOnlyInScopeCandidatesAndIssues(ScopeScenario scenario)
    {
        foreach (var expectation in scenario.Expectations)
        {
            var fixture = CreateCatalogueFixture(includeScopeIssue: true);
            var resolution = new FindUniverseResolver(ContainedPath).Resolve(
                CreateInput(fixture, [expectation.Selector], []));

            Assert.Equal(
                expectation.ExpectedCandidatePaths,
                resolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath));
            Assert.Equal(
                expectation.ExpectedIssuePaths,
                resolution.Selection.Issues.Select(issue => issue.AttemptedCanonicalPath));
            Assert.DoesNotContain(
                ".agents/docs2/outside.md",
                resolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath));
        }
    }

    [Theory(DisplayName = "Find catalogue issues map to exact findings and candidate-count knowledge")]
    [MemberData(nameof(CatalogueIssueScenarios), DisableDiscoveryEnumeration = true)]
    [Trait("Feature", "find-query"), Trait("Evidence", "Unit")]
    public void CatalogueIssuesMapToExactFindFindingsAndCandidateCounts(CatalogueIssueScenario scenario)
    {
        foreach (var expectation in scenario.Expectations)
        {
            var fixture = CreateIssueFixture(expectation.IssueCode);
            var resolution = new FindUniverseResolver(ContainedPath).Resolve(
                CreateInput(fixture, [], []));

            Assert.Equal(expectation.ExpectedCandidateCount, resolution.Universe.CandidateCount);
            if (expectation.ExpectedFindingCode is null)
            {
                Assert.Empty(resolution.Findings);
                continue;
            }

            var finding = Assert.Single(resolution.Findings);
            Assert.Equal(expectation.ExpectedFindingCode, finding.Code);
            Assert.Equal(expectation.ExpectedStatus, finding.Status);
        }
    }

    private static FindUniverseInput CreateInput(
        CatalogueFixture fixture,
        IEnumerable<string> include,
        IEnumerable<string> exclude)
    {
        var query = new FindQuery(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, "frontmatter")],
                [new FindRegion(FindRegionKind.Body, null, "body")]));
        var presentation = new FindPresentationSelection(
            null,
            CliView.Expanded,
            new FindContentSelection([], []));
        var request = new FindRequest(
            fixture.Workspace,
            new FindUniverseFilter(include, exclude),
            query,
            presentation);
        var sourceSession = new SourceReadSession(
            fixture.Catalogue,
            new SourceDocumentReader(fixture.Workspace),
            fixture.DefaultScope);
        return new FindUniverseInput(request, sourceSession);
    }

    private static CatalogueFixture CreateCatalogueFixture(bool includeScopeIssue = false)
    {
        var workspace = CreateWorkspace();
        var specs = new[]
        {
            new SourceSpec(".agents/loader.md", "loader", SourceDocumentForm.Loader, false),
            new SourceSpec(".agents/docs/guide.md", "docs/guide", SourceDocumentForm.Markdown, false),
            new SourceSpec(".agents/docs/_docs.md", "docs", SourceDocumentForm.CanonicalEntrypoint, false),
            new SourceSpec(".agents/docs/guide/_guide.md", "docs/guide", SourceDocumentForm.CanonicalEntrypoint, false),
            new SourceSpec(".agents/docs/with-overwrite.md", "docs/with-overwrite", SourceDocumentForm.Markdown, true),
            new SourceSpec(".agents/skills/search/SKILL.md", "skills/search", SourceDocumentForm.Skill, false),
        };
        var sources = specs.Select(spec => CreateSource(workspace, spec)).ToArray();
        var candidates = sources
            .SelectMany(source => CreateCandidates(workspace, source))
            .ToList();
        candidates.Add(CreateCandidate(
            workspace,
            ".agents/docs2/outside.md",
            SourceDocumentForm.Markdown,
            "docs2/outside",
            PhysicalPathState.Contained));

        var issues = new List<SourceCatalogueIssue>();
        if (includeScopeIssue)
        {
            var path = ".agents/docs/unreadable.md";
            candidates.Add(CreateCandidate(
                workspace,
                path,
                SourceDocumentForm.Markdown,
                "docs/unreadable",
                PhysicalPathState.External));
            issues.Add(new SourceCatalogueIssue(
                SourceCatalogueIssueCode.CandidateUnsafe,
                path,
                [],
                Physical(workspace.PhysicalRoot, ".agents/docs"),
                null));
        }

        return new CatalogueFixture(
            workspace,
            new SourceCatalogue(workspace, candidates, sources, issues, false),
            new SourceCatalogueSelectionScope(
                ".agents",
                Physical(workspace.PhysicalRoot, ".agents")));
    }

    private static CatalogueFixture CreateIssueFixture(SourceCatalogueIssueCode code)
    {
        var workspace = CreateWorkspace();
        var sources = new List<SourceLogicalSource>();
        var candidates = new List<SourceCandidate>();
        var issues = new List<SourceCatalogueIssue>();
        var rootIssue = code is SourceCatalogueIssueCode.RootMissing
            or SourceCatalogueIssueCode.RootUnsafe
            or SourceCatalogueIssueCode.RootUnavailable;

        if (code is SourceCatalogueIssueCode.CandidateUnsafe
            or SourceCatalogueIssueCode.CandidateUnavailable
            or SourceCatalogueIssueCode.IdentityUnavailable)
        {
            var path = code == SourceCatalogueIssueCode.IdentityUnavailable
                ? ".agents/docs/unidentified.md"
                : ".agents/docs/unsafe.md";
            var form = SourceDocumentForm.Markdown;
            var id = code == SourceCatalogueIssueCode.IdentityUnavailable
                ? null
                : "docs/unsafe";
            candidates.Add(CreateCandidate(
                workspace,
                path,
                form,
                id,
                code == SourceCatalogueIssueCode.CandidateUnsafe
                    ? PhysicalPathState.External
                    : code == SourceCatalogueIssueCode.CandidateUnavailable
                        ? PhysicalPathState.Inaccessible
                        : PhysicalPathState.Contained));
            issues.Add(new SourceCatalogueIssue(
                code,
                path,
                [],
                Physical(workspace.PhysicalRoot, ".agents/docs"),
                null));
        }
        else if (code == SourceCatalogueIssueCode.DirectoryUnavailable)
        {
            issues.Add(new SourceCatalogueIssue(
                code,
                ".agents/docs/private",
                [],
                Physical(workspace.PhysicalRoot, ".agents/docs/private"),
                null));
        }
        else if (code == SourceCatalogueIssueCode.IdentityCollision)
        {
            var firstSpec = new SourceSpec(
                ".agents/docs/collision.md",
                "docs/collision",
                SourceDocumentForm.Markdown,
                false);
            var secondSpec = new SourceSpec(
                ".agents/docs/collision/_collision.md",
                "docs/collision",
                SourceDocumentForm.CanonicalEntrypoint,
                false);
            sources.Add(CreateSource(workspace, firstSpec));
            sources.Add(CreateSource(workspace, secondSpec));
            candidates.AddRange(sources.SelectMany(source => CreateCandidates(workspace, source)));
            issues.Add(new SourceCatalogueIssue(
                code,
                firstSpec.Path,
                [firstSpec.Path, secondSpec.Path],
                Physical(workspace.PhysicalRoot, ".agents/docs/collision"),
                null));
        }
        else if (code == SourceCatalogueIssueCode.OrphanOverwrite)
        {
            var path = ".agents/docs/orphan.overwrite.md";
            candidates.Add(CreateCandidate(
                workspace,
                path,
                SourceDocumentForm.OverwriteCompanion,
                "docs/orphan",
                PhysicalPathState.Contained));
            issues.Add(new SourceCatalogueIssue(
                code,
                path,
                [],
                Physical(workspace.PhysicalRoot, ".agents/docs"),
                null));
        }
        else if (code == SourceCatalogueIssueCode.PhysicalAlias)
        {
            var firstSpec = new SourceSpec(
                ".agents/docs/alias.md",
                "docs/alias",
                SourceDocumentForm.Markdown,
                false);
            var secondSpec = new SourceSpec(
                ".agents/docs/alias-copy.md",
                "docs/alias-copy",
                SourceDocumentForm.Markdown,
                false);
            sources.Add(CreateSource(workspace, firstSpec));
            sources.Add(CreateSource(workspace, secondSpec));
            candidates.AddRange(sources.SelectMany(source => CreateCandidates(workspace, source)));
            issues.Add(new SourceCatalogueIssue(
                code,
                firstSpec.Path,
                [firstSpec.Path, secondSpec.Path],
                Physical(workspace.PhysicalRoot, ".agents/docs"),
                null));
        }

        if (rootIssue)
        {
            issues.Add(new SourceCatalogueIssue(
                code,
                ".agents",
                [],
                null,
                null));
        }

        return new CatalogueFixture(
            workspace,
            new SourceCatalogue(workspace, candidates, sources, issues, false),
            new SourceCatalogueSelectionScope(
                ".agents",
                Physical(workspace.PhysicalRoot, ".agents")));
    }

    private static SourceLogicalSource CreateSource(CliWorkspace workspace, SourceSpec spec)
    {
        var baseLayer = new SourceLayer(
            spec.Path,
            Physical(workspace.PhysicalRoot, spec.Path),
            spec.Form,
            SourceLayerKind.Base);
        SourceLayer? overwrite = null;
        if (spec.HasOverwrite)
        {
            var overwritePath = spec.Path[..^".md".Length] + ".overwrite.md";
            overwrite = new SourceLayer(
                overwritePath,
                Physical(workspace.PhysicalRoot, overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite);
        }

        return new SourceLogicalSource(
            new SourceLogicalIdentity(spec.Id, spec.Path),
            baseLayer,
            overwrite);
    }

    private static IEnumerable<SourceCandidate> CreateCandidates(
        CliWorkspace workspace,
        SourceLogicalSource source)
    {
        yield return CreateCandidate(
            workspace,
            source.Base.CanonicalPath,
            source.Base.Form,
            source.Identity.AutomaticId,
            PhysicalPathState.Contained);
        if (source.Overwrite is { } overwrite)
        {
            yield return CreateCandidate(
                workspace,
                overwrite.CanonicalPath,
                overwrite.Form,
                source.Identity.AutomaticId,
                PhysicalPathState.Contained);
        }
    }

    private static SourceCandidate CreateCandidate(
        CliWorkspace workspace,
        string path,
        SourceDocumentForm form,
        string? id,
        PhysicalPathState state)
    {
        var physicalPath = Physical(workspace.PhysicalRoot, path);
        return new SourceCandidate(
            path,
            form,
            id,
            state,
            state == PhysicalPathState.Contained ? physicalPath : null,
            Path.GetDirectoryName(physicalPath));
    }

    private static CliWorkspace CreateWorkspace()
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-universe-red"));
        return new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static PhysicalPathResolution ContainedPath(CliWorkspace workspace, string path)
        => PhysicalPathResolution.Contained(path, Physical(workspace.PhysicalRoot, path));

    private static string Physical(string root, string logicalPath)
        => Path.GetFullPath(Path.Combine(
            root,
            logicalPath.Replace('/', Path.DirectorySeparatorChar)));

    private sealed record CatalogueFixture(
        CliWorkspace Workspace,
        SourceCatalogue Catalogue,
        SourceCatalogueSelectionScope DefaultScope);

    private sealed record SourceSpec(
        string Path,
        string Id,
        SourceDocumentForm Form,
        bool HasOverwrite);

    public sealed class SelectorScenario
    {
        internal SelectorScenario(string name, IReadOnlyList<SelectorExpectation> expectations)
        {
            Name = name;
            Expectations = expectations;
        }

        private string Name { get; }

        public override string ToString() => Name;

        internal IReadOnlyList<SelectorExpectation> Expectations { get; }
    }

    public sealed class AlgebraScenario
    {
        internal AlgebraScenario(string name, IReadOnlyList<AlgebraExpectation> expectations)
        {
            Name = name;
            Expectations = expectations;
        }

        private string Name { get; }

        public override string ToString() => Name;

        internal IReadOnlyList<AlgebraExpectation> Expectations { get; }
    }

    public sealed class ScopeScenario
    {
        internal ScopeScenario(string name, IReadOnlyList<ScopeExpectation> expectations)
        {
            Name = name;
            Expectations = expectations;
        }

        private string Name { get; }

        public override string ToString() => Name;

        internal IReadOnlyList<ScopeExpectation> Expectations { get; }
    }

    public sealed class CatalogueIssueScenario
    {
        internal CatalogueIssueScenario(string name, IReadOnlyList<CatalogueIssueExpectation> expectations)
        {
            Name = name;
            Expectations = expectations;
        }

        private string Name { get; }

        public override string ToString() => Name;

        internal IReadOnlyList<CatalogueIssueExpectation> Expectations { get; }
    }

    internal sealed record SelectorExpectation(
        string Value,
        SourceReferenceKind Form,
        FindSelectorResolution Resolution,
        FindSourceKind? SourceKind,
        FindSelectorExpansion? Expansion,
        string? ExpectedId,
        string? ExpectedPath,
        bool ProbesPhysical,
        string[] CandidateIds,
        string[] CandidatePaths);

    internal sealed record AlgebraExpectation(
        string[] Include,
        string[] Exclude,
        string[] ExpectedPaths);

    internal sealed record ScopeExpectation(
        string Selector,
        string[] ExpectedCandidatePaths,
        string[] ExpectedIssuePaths);

    internal sealed record CatalogueIssueExpectation(
        SourceCatalogueIssueCode IssueCode,
        FindFindingCode? ExpectedFindingCode,
        CliSemanticStatus? ExpectedStatus,
        int? ExpectedCandidateCount);
}
