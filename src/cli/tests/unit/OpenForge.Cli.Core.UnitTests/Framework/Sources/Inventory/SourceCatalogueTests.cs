using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Inventory;

public sealed class SourceCatalogueTests
{
    [Fact(DisplayName = "Neutral source catalogue orders candidates, sources, and issues independently by ordinal facts")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CatalogueSnapshotsDeterministicCollections()
    {
        var catalogue = CreateCatalogue();

        Assert.Equal(
            [
                ".agents/root/leaf.md",
                ".agents/root/leaf.overwrite.md",
                ".agents/root/other.md",
            ],
            catalogue.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            ["root/leaf", "root/other"],
            catalogue.Sources.Select(source => source.Identity.AutomaticId));
        Assert.Equal(
            [SourceCatalogueIssueCode.RootMissing, SourceCatalogueIssueCode.IdentityCollision],
            catalogue.Issues.Select(issue => issue.Code));
        Assert.False(catalogue.IsCancelled);
    }

    [Fact(DisplayName = "Neutral source catalogue resolves a candidate by exact canonical path")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CandidatePathLookupRetainsUnsafeAndContainedCandidateAddressability()
    {
        var catalogue = CreateCatalogue();

        var candidate = Assert.IsType<SourceCandidate>(
            catalogue.FindCandidateByPath(".agents/root/leaf.overwrite.md"));

        Assert.Equal(PhysicalPathState.Contained, candidate.PhysicalState);
        Assert.Equal(SourceDocumentForm.OverwriteCompanion, candidate.Form);
    }

    [Fact(DisplayName = "Neutral source catalogue finds every candidate with a requested lexical ID including unsafe candidates")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CandidateIdentityLookupIncludesUnsafeEvidence()
    {
        var catalogue = CreateCatalogue(includeUnsafeCandidate: true);

        var candidates = catalogue.FindAllCandidatesById("root/leaf");

        Assert.Equal(
            [".agents/root/leaf.md", ".agents/root/unsafe.md"],
            candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Contains(candidates, candidate => candidate.PhysicalState == PhysicalPathState.External);
    }

    [Fact(DisplayName = "Neutral source catalogue resolves retained logical sources by ID and adjacent paths")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void LogicalLookupsPreserveAdjacentOverwriteIdentity()
    {
        var catalogue = CreateCatalogue();

        var byId = catalogue.FindAllById("root/leaf");
        var byBase = catalogue.FindByPath(".agents/root/leaf.md");
        var byOverwrite = catalogue.FindByPath(".agents/root/leaf.overwrite.md");

        var source = Assert.Single(byId);
        var overwriteSource = Assert.IsType<SourceLogicalSource>(byOverwrite);
        Assert.Same(source, byBase);
        Assert.Same(source, byOverwrite);
        Assert.Same(source.Overwrite, overwriteSource.Overwrite);
    }

    [Fact(DisplayName = "Neutral source catalogue full selection projects all candidates, sources, and root issues")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SelectAllIsTheCompleteReadAllowlist()
    {
        var catalogue = CreateCatalogue(includeUnsafeCandidate: true);

        var selection = catalogue.SelectAll();

        Assert.Equal(catalogue.Sources, selection.Sources);
        Assert.Equal(catalogue.Candidates, selection.Candidates);
        Assert.Single(selection.RootIssues);
        Assert.Contains(selection.Issues, issue => issue.Code == SourceCatalogueIssueCode.IdentityCollision);
    }

    [Fact(DisplayName = "Neutral source catalogue filtered selection retains included physical scopes and removes excluded support")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SelectProjectsSourcesCandidatesAndScopeDerivedIssues()
    {
        var catalogue = CreateCatalogue(includeUnsafeCandidate: true);
        var source = catalogue.Sources.Single(candidate => candidate.Identity.AutomaticId == "root/leaf");
        var request = new SourceCatalogueSelectionRequest(
            [source],
            [new SourceCatalogueSelectionScope(
                ".agents/root",
                Physical(".agents/root"))],
            [new SourceCatalogueSelectionScope(
                ".agents/root/private",
                Physical(".agents/root/private"))]);

        var selection = catalogue.Select(request);

        Assert.Same(source, Assert.Single(selection.Sources));
        Assert.Equal(
            [".agents/root/leaf.md", ".agents/root/leaf.overwrite.md", ".agents/root/unsafe.md"],
            selection.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.DoesNotContain(selection.Issues, issue => issue.ScopePhysicalPath == Physical(".agents/root/private"));
        Assert.Single(selection.RootIssues);
    }

    [Fact(DisplayName = "Neutral filtered selection projects collision and alias participation only while selected paths remain covered")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SelectProjectsIssueMembershipFromPhysicalScopesAndCandidates()
    {
        var workspace = new CliWorkspace(
            Physical("."),
            Physical("."),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var first = Source(".agents/root/collision.md", "root/collision", SourceDocumentForm.Markdown);
        var second = Source(
            ".agents/root/collision/_collision.md",
            "root/collision",
            SourceDocumentForm.CanonicalEntrypoint);
        var firstCandidate = Candidate(
            ".agents/root/collision.md",
            SourceDocumentForm.Markdown,
            "root/collision",
            PhysicalPathState.Contained);
        var secondCandidate = Candidate(
            ".agents/root/collision/_collision.md",
            SourceDocumentForm.CanonicalEntrypoint,
            "root/collision",
            PhysicalPathState.Contained);
        var aliasCandidate = Candidate(
            ".agents/root/alias.md",
            SourceDocumentForm.Markdown,
            "root/alias",
            PhysicalPathState.Contained);
        var rootIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Root,
            SourceCatalogueIssueCode.RootMissing,
            ".agents/missing",
            [],
            null,
            null);
        var directoryIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Directory,
            SourceCatalogueIssueCode.DirectoryUnavailable,
            ".agents/root/private",
            [],
            Physical(".agents/root/private"),
            new OpenForge.Cli.Core.Framework.Filesystem.FilesystemFailure(
                OpenForge.Cli.Core.Framework.Filesystem.FilesystemFailureKind.AccessDenied,
                "denied"));
        var collisionIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Identity,
            SourceCatalogueIssueCode.IdentityCollision,
            ".agents/root/collision.md",
            [first.Identity.CanonicalBasePath, second.Identity.CanonicalBasePath],
            Physical(".agents/root"),
            null);
        var aliasIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Identity,
            SourceCatalogueIssueCode.PhysicalAlias,
            ".agents/root/alias.md",
            [first.Identity.CanonicalBasePath, ".agents/root/alias.md"],
            Physical(".agents/root"),
            null);
        var catalogue = new SourceCatalogue(
            workspace,
            [firstCandidate, secondCandidate, aliasCandidate],
            [first, second],
            [aliasIssue, collisionIssue, directoryIssue, rootIssue],
            false);
        var request = new SourceCatalogueSelectionRequest(
            [first, second],
            [new SourceCatalogueSelectionScope(
                ".agents/root",
                Physical(".agents/root"))],
            [new SourceCatalogueSelectionScope(
                ".agents/root/private",
                Physical(".agents/root/private"))]);

        var selection = catalogue.Select(request);

        Assert.Equal(2, selection.Sources.Count);
        Assert.Contains(selection.Candidates, candidate =>
            candidate.CanonicalPath == ".agents/root/alias.md");
        Assert.Contains(selection.Issues, issue => issue.Code == SourceCatalogueIssueCode.IdentityCollision);
        Assert.Contains(selection.Issues, issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias);
        Assert.DoesNotContain(selection.Issues, issue =>
            issue.Code == SourceCatalogueIssueCode.DirectoryUnavailable);
        Assert.Single(selection.RootIssues);
    }

    private static SourceCatalogue CreateCatalogue(bool includeUnsafeCandidate = false)
    {
        var workspace = new CliWorkspace(
            Physical("."),
            Physical("."),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var leaf = Source(
            ".agents/root/leaf.md",
            "root/leaf",
            SourceDocumentForm.Markdown,
            withOverwrite: true);
        var other = Source(
            ".agents/root/other.md",
            "root/other",
            SourceDocumentForm.Markdown);
        var candidates = new List<SourceCandidate>
        {
            Candidate(
                ".agents/root/other.md",
                SourceDocumentForm.Markdown,
                "root/other",
                PhysicalPathState.Contained),
            Candidate(
                ".agents/root/leaf.overwrite.md",
                SourceDocumentForm.OverwriteCompanion,
                "root/leaf",
                PhysicalPathState.Contained),
            Candidate(
                ".agents/root/leaf.md",
                SourceDocumentForm.Markdown,
                "root/leaf",
                PhysicalPathState.Contained),
        };
        if (includeUnsafeCandidate)
        {
            candidates.Add(Candidate(
                ".agents/root/unsafe.md",
                SourceDocumentForm.Markdown,
                "root/leaf",
                PhysicalPathState.External));
        }

        var rootIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Root,
            SourceCatalogueIssueCode.RootMissing,
            ".agents/missing",
            [],
            null,
            null);
        var collision = new SourceCatalogueIssue(
            SourceCatalogueIssueStage.Identity,
            SourceCatalogueIssueCode.IdentityCollision,
            ".agents/root/other.md",
            [".agents/root/leaf.md", ".agents/root/other.md"],
            Physical(".agents/root"),
            null);
        return new SourceCatalogue(
            workspace,
            candidates,
            [other, leaf],
            [collision, rootIssue],
            isCancelled: false);
    }

    private static SourceLogicalSource Source(
        string canonicalPath,
        string id,
        SourceDocumentForm form,
        bool withOverwrite = false)
    {
        var baseLayer = new SourceLayer(
            canonicalPath,
            Physical(canonicalPath),
            form,
            SourceLayerKind.Base);
        var overwritePath = canonicalPath[..^".md".Length] + ".overwrite.md";
        var overwrite = withOverwrite
            ? new SourceLayer(
                overwritePath,
                Physical(overwritePath),
                SourceDocumentForm.OverwriteCompanion,
                SourceLayerKind.Overwrite)
            : null;
        return new SourceLogicalSource(
            new SourceLogicalIdentity(id, canonicalPath),
            baseLayer,
            overwrite);
    }

    private static SourceCandidate Candidate(
        string canonicalPath,
        SourceDocumentForm form,
        string id,
        PhysicalPathState state)
    {
        return new SourceCandidate(
            canonicalPath,
            form,
            id,
            state,
            state == PhysicalPathState.Contained ? Physical(canonicalPath) : null,
            Physical(".agents/root"));
    }

    private static string Physical(string relativePath)
    {
        return Path.GetFullPath(Path.Combine(
            Path.GetTempPath(),
            "source-catalogue-unit",
            relativePath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
