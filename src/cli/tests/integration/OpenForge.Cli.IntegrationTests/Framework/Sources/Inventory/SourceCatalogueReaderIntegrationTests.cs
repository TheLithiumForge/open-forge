using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Inventory;

public sealed class SourceCatalogueReaderIntegrationTests
{
    [Fact(DisplayName = "Neutral catalogue reads every recognized source form without reading document bodies")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task CatalogueIsBodyFreeAndClassifiesAllSourceForms()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        workspace.Write(".agents/loader.md", [0xFF, 0xFE]);
        workspace.Write(".agents/root/_root.md", [0xFF, 0xFE]);
        workspace.Write(".agents/index/index.md", [0xFF, 0xFE]);
        workspace.Write(".agents/underscore-index/_index.md", [0xFF, 0xFE]);
        workspace.Write(".agents/references/references.md", [0xFF, 0xFE]);
        workspace.Write(".agents/underscore-references/_references.md", [0xFF, 0xFE]);
        workspace.Write(".agents/skills/tool/SKILL.md", [0xFF, 0xFE]);
        workspace.Write(".agents/root/leaf.md", [0xFF, 0xFE]);
        workspace.Write(".agents/root/leaf.overwrite.md", [0xFF, 0xFE]);
        workspace.Write(".agents/root/ignored.txt", [0xFF, 0xFE]);
        var before = workspace.SnapshotHashes();

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.False(catalogue.IsCancelled);
        Assert.Equal(
            [
                ".agents/index/index.md",
                ".agents/loader.md",
                ".agents/references/references.md",
                ".agents/root/_root.md",
                ".agents/root/leaf.md",
                ".agents/root/leaf.overwrite.md",
                ".agents/skills/tool/SKILL.md",
                ".agents/underscore-index/_index.md",
                ".agents/underscore-references/_references.md",
            ],
            catalogue.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            [
                SourceDocumentForm.IndexEntrypoint,
                SourceDocumentForm.Loader,
                SourceDocumentForm.ReferencesEntrypoint,
                SourceDocumentForm.CanonicalEntrypoint,
                SourceDocumentForm.Markdown,
                SourceDocumentForm.OverwriteCompanion,
                SourceDocumentForm.Skill,
                SourceDocumentForm.UnderscoreIndexEntrypoint,
                SourceDocumentForm.UnderscoreReferencesEntrypoint,
            ],
            catalogue.Candidates.Select(candidate => candidate.Form));
        Assert.DoesNotContain(catalogue.Candidates, candidate =>
            candidate.CanonicalPath.EndsWith("ignored.txt", StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral catalogue deduplicates duplicate and overlapping roots while retaining deterministic candidate ordering")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task DuplicateAndOverlappingRootsDoNotDuplicateCandidates()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        workspace.Write(".agents/root/_root.md", "root");
        workspace.Write(".agents/root/child.md", "child");
        workspace.Write(".agents/other.md", "other");
        var before = workspace.SnapshotHashes();
        var request = workspace.Request(
            ".agents/root",
            ".agents",
            ".agents/root",
            ".agents/root/child");

        var first = await new SourceCatalogueReader().ReadAsync(
            request,
            TestContext.Current.CancellationToken);
        var second = await new SourceCatalogueReader().ReadAsync(
            request,
            TestContext.Current.CancellationToken);

        Assert.Equal(
            first.Candidates.Select(candidate => candidate.CanonicalPath),
            second.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            first.Candidates.Select(candidate => candidate.CanonicalPath).Distinct(StringComparer.Ordinal),
            first.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            [".agents/other.md", ".agents/root/_root.md", ".agents/root/child.md"],
            first.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral catalogue keeps separate root outcomes for missing contained-file and unsafe roots")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task RootOutcomesRemainTypedAndIndependent()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        workspace.Write(".agents/file-root", "not a directory");
        workspace.Write(".agents/safe.md", "safe");
        using var outside = OpenForge.Cli.TestSupport.TemporaryWorkspace.Create("source-catalogue-root-outside");
        outside.WriteText("outside.md", "outside");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/external-root", outside.Path, out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/dangling-root", "missing-root", out _),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            workspace.Request(
                ".agents/missing-root",
                ".agents/file-root",
                ".agents/external-root",
                ".agents/dangling-root",
                ".agents"),
            TestContext.Current.CancellationToken);

        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootMissing);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootUnsafe);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.RootUnavailable);
        Assert.Contains(catalogue.Sources, source => source.Identity.CanonicalBasePath == ".agents/safe.md");
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral catalogue blocks external dangling cyclic and re-entry candidates while retaining safe sources")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task CandidateBoundariesStopAtTheFirstUnsafeTransition()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        using var outside = OpenForge.Cli.TestSupport.TemporaryWorkspace.Create("source-catalogue-candidate-outside");
        workspace.Write(".agents/safe.md", "safe");
        workspace.Write(".agents/reentry-target.md", "safe reentry target");
        var outsideFile = outside.CreateFile("outside.md", "outside");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/external.md", outsideFile, out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(".agents/dangling.md", "missing-file", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/cycle-a", "cycle-b", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/cycle-b", "cycle-a", out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            outside.TryCreateDirectorySymbolicLink("back", workspace.Absolute(".agents"), out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateDirectorySymbolicLink(".agents/reentry", outside.Path, out _),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        var unsafePaths = catalogue.Issues
            .Where(issue => issue.Code == SourceCatalogueIssueCode.CandidateUnsafe)
            .Select(issue => issue.AttemptedCanonicalPath)
            .ToArray();
        Assert.Contains(".agents/external.md", unsafePaths);
        Assert.Contains(".agents/dangling.md", unsafePaths);
        Assert.Contains(".agents/cycle-a", unsafePaths);
        Assert.Contains(".agents/cycle-b", unsafePaths);
        Assert.Contains(".agents/reentry", unsafePaths);
        Assert.Contains(catalogue.Sources, source => source.Identity.CanonicalBasePath == ".agents/safe.md");
        Assert.DoesNotContain(catalogue.Sources, source =>
            source.Identity.CanonicalBasePath == ".agents/external.md");
        Assert.DoesNotContain(catalogue.Candidates, candidate =>
            candidate.CanonicalPath.StartsWith(".agents/reentry/", StringComparison.Ordinal));
        Assert.DoesNotContain(catalogue.Sources, source =>
            source.Identity.CanonicalBasePath.StartsWith(".agents/reentry/", StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral catalogue retains aliases collisions unavailable IDs and exact overwrite pairing facts")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task IdentityAndPairingFactsRemainDistinct()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        workspace.Write(".agents/physical/target.md", "target");
        workspace.Write(".agents/collision.md", "leaf");
        workspace.Write(".agents/collision/_collision.md", "entrypoint");
        workspace.Write(".agents/.md", "unavailable identity");
        workspace.Write(".agents/paired.md", "base");
        workspace.Write(".agents/paired.overwrite.md", "overwrite");
        workspace.Write(".agents/orphan.overwrite.md", "orphan");
        workspace.Write(".agents/nonadjacent/_nonadjacent.md", "entrypoint");
        workspace.Write(".agents/nonadjacent.overwrite.md", "nonadjacent overwrite");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/alias.md",
                workspace.Absolute(".agents/physical/target.md"),
                out _),
            "This integration case requires real symbolic-link support.");
        var before = workspace.SnapshotHashes();

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            workspace.Request(),
            TestContext.Current.CancellationToken);

        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.IdentityUnavailable);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.IdentityCollision);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.PhysicalAlias);
        Assert.Contains(catalogue.Issues, issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite);
        var paired = catalogue.Sources.Single(source =>
            source.Identity.CanonicalBasePath == ".agents/paired.md");
        var overwrite = Assert.IsType<SourceLayer>(paired.Overwrite);
        Assert.Equal(".agents/paired.overwrite.md", overwrite.CanonicalPath);
        Assert.DoesNotContain(catalogue.Sources, source =>
            source.Identity.CanonicalBasePath == ".agents/nonadjacent.overwrite.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Neutral catalogue honors pre-cancelled requests before accessing an owned missing root")]
    [Trait("Feature", "source-catalogue"), Trait("Evidence", "Integration")]
    public async Task PreCancelledReadDoesNotAccessTheMissingRoot()
    {
        using var workspace = SourceCatalogueIntegrationWorkspace.Create();
        var before = workspace.SnapshotHashes();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var catalogue = await new SourceCatalogueReader().ReadAsync(
            workspace.Request(".agents/not-created"),
            cancellation.Token);

        Assert.True(catalogue.IsCancelled);
        Assert.Empty(catalogue.Candidates);
        Assert.Empty(catalogue.Sources);
        Assert.Empty(catalogue.Issues);
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
