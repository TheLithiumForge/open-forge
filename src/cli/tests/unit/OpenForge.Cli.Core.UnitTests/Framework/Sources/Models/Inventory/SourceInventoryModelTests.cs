using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

public sealed class SourceInventoryModelTests
{
    [Fact(DisplayName = "Neutral catalogue requests retain sorted duplicate and overlapping canonical roots")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void RequestsKeepEveryRootWithoutApplyingTraversalPolicy()
    {
        var workspace = SourceInventoryTestData.Workspace();
        var request = new SourceCatalogueRequest(
            workspace,
            [".agents/z", ".agents", ".agents/z", ".agents/a"]);

        Assert.Equal(
            [".agents", ".agents/a", ".agents/z", ".agents/z"],
            request.LogicalRoots);
        Assert.Same(workspace, request.Workspace);
    }

    [Fact(DisplayName = "Neutral candidates retain recognized unsafe evidence and contained physical provenance")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void CandidatesSeparateLogicalFormFromPhysicalState()
    {
        var unsafeCandidate = SourceInventoryTestData.Candidate(
            ".agents/root/leaf.md",
            SourceDocumentForm.Markdown,
            "root/leaf",
            PhysicalPathState.External);
        var containedCandidate = SourceInventoryTestData.Candidate(
            ".agents/root/other.md",
            SourceDocumentForm.Markdown,
            "root/other",
            PhysicalPathState.Contained,
            SourceInventoryTestData.Physical(".agents/root/other.md"),
            SourceInventoryTestData.Physical(".agents/root"));
        var unrecognized = SourceInventoryTestData.Candidate(
            ".agents/root/notes.txt",
            null,
            null,
            PhysicalPathState.Missing);

        Assert.Equal(SourceDocumentForm.Markdown, unsafeCandidate.Form);
        Assert.Equal("root/leaf", unsafeCandidate.AutomaticId);
        Assert.Equal(PhysicalPathState.External, unsafeCandidate.PhysicalState);
        Assert.Null(unsafeCandidate.PhysicalPath);
        Assert.NotNull(unsafeCandidate.PhysicalParentPath);
        Assert.Equal(SourceInventoryTestData.Physical(".agents/root/other.md"), containedCandidate.PhysicalPath);
        Assert.Equal(SourceInventoryTestData.Physical(".agents/root"), containedCandidate.PhysicalParentPath);
        Assert.Null(unrecognized.Form);
        Assert.Null(unrecognized.AutomaticId);
    }

    [Fact(DisplayName = "Neutral layers and logical sources enforce exact base and adjacent overwrite identity")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void LayersEnforcePairShape()
    {
        var source = SourceInventoryTestData.Source(
            ".agents/guidance/style.md",
            withOverwrite: true);

        Assert.Equal(".agents/guidance/style.md", source.Identity.CanonicalBasePath);
        Assert.Equal(SourceLayerKind.Base, source.Base.Kind);
        Assert.Equal(SourceDocumentForm.Markdown, source.Base.Form);
        var overwrite = Assert.IsType<SourceLayer>(source.Overwrite);
        Assert.Equal(
            ".agents/guidance/style.overwrite.md",
            overwrite.CanonicalPath);
        Assert.Equal(SourceLayerKind.Overwrite, overwrite.Kind);

        var nonAdjacent = SourceInventoryTestData.OverwriteLayer(
            ".agents/guidance/other.overwrite.md");
        Assert.Throws<ArgumentException>(() => new SourceLogicalSource(
            source.Identity,
            source.Base,
            nonAdjacent));
    }

    [Fact(DisplayName = "Neutral catalogue issues retain typed stage, scope, ordered related paths, and bounded failure")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void IssuesRemainFactsRatherThanCommandFindings()
    {
        var failure = new FilesystemFailure(
            FilesystemFailureKind.InputOutput,
            "directory inspection failed");
        var issue = new SourceCatalogueIssue(
            SourceCatalogueIssueCode.IdentityCollision,
            ".agents/root/collision.md",
            [".agents/root/z.md", ".agents/root/a.md"],
            SourceInventoryTestData.Physical(".agents/root"),
            failure);

        Assert.Equal(SourceCatalogueIssueStage.Identity, issue.Stage);
        Assert.Equal(SourceCatalogueIssueCode.IdentityCollision, issue.Code);
        Assert.Equal(
            [".agents/root/a.md", ".agents/root/z.md"],
            issue.RelatedPaths);
        Assert.Equal(SourceInventoryTestData.Physical(".agents/root"), issue.ScopePhysicalPath);
        Assert.Same(failure, issue.Failure);
    }

    [Fact(DisplayName = "Neutral selection requests order unique physical include and exclude scopes")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SelectionRequestRetainsItsEffectiveInputs()
    {
        var first = SourceInventoryTestData.Source(".agents/a.md");
        var second = SourceInventoryTestData.Source(".agents/b.md");
        var request = new SourceCatalogueSelectionRequest(
            [second, first],
            [
                new SourceCatalogueSelectionScope(
                    ".agents/z",
                    SourceInventoryTestData.Physical(".agents/z")),
                new SourceCatalogueSelectionScope(
                    ".agents/a",
                    SourceInventoryTestData.Physical(".agents/a")),
            ],
            [
                new SourceCatalogueSelectionScope(
                    ".agents/a/private",
                    SourceInventoryTestData.Physical(".agents/a/private")),
            ]);

        Assert.Equal([first, second], request.Sources);
        Assert.Equal([".agents/a", ".agents/z"], request.IncludedScopes.Select(scope => scope.CanonicalDirectoryPath));
        Assert.Equal([".agents/a/private"], request.ExcludedScopes.Select(scope => scope.CanonicalDirectoryPath));
    }

    [Fact(DisplayName = "Neutral selection separates root issues from projected non-root issues")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Unit")]
    public void SelectionModelKeepsIssueCollectionsTyped()
    {
        var rootIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueCode.RootMissing,
            ".agents/missing",
            [],
            null,
            null);
        var directoryIssue = new SourceCatalogueIssue(
            SourceCatalogueIssueCode.DirectoryUnavailable,
            ".agents/root",
            [],
            SourceInventoryTestData.Physical(".agents/root"),
            new FilesystemFailure(FilesystemFailureKind.AccessDenied, "denied"));
        var selection = new SourceCatalogueSelection(
            [],
            [],
            [directoryIssue],
            [rootIssue]);

        Assert.Empty(selection.Sources);
        Assert.Empty(selection.Candidates);
        Assert.Same(directoryIssue, Assert.Single(selection.Issues));
        Assert.Same(rootIssue, Assert.Single(selection.RootIssues));
    }
}
