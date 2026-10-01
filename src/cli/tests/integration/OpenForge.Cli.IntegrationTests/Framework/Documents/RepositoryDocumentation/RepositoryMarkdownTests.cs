using OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation.Shared;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Documents.RepositoryDocumentation;

public sealed class RepositoryMarkdownTests
{
    [Fact(DisplayName = "Repository Markdown gate reports no diagnostics for the current document inventory")]
    [Trait("Boundary", "Host"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task RepositoryMarkdownGateHasNoDiagnostics()
    {
        var repositoryRoot = RepositoryRoot();
        var sourcePaths = RepositoryMarkdownScope.Enumerate(repositoryRoot);

        var diagnostics = await RepositoryMarkdownChecker.CheckAsync(
            repositoryRoot,
            sourcePaths,
            TestContext.Current.CancellationToken);

        Assert.True(
            diagnostics.Count == 0,
            string.Join(Environment.NewLine, diagnostics));
    }

    [Fact(DisplayName = "Repository Markdown scope matches the literal inventory and includes published demos")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public void ScopeMatchesLiteralInventory()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateScopeCases();

        var expected = new[]
        {
            ".agents/maps/generated/map.md",
            ".agents/maps/root.md",
            ".agents/maps/working/map.md",
            ".agents/memory/crystallized/decisions/framework/workspace-state-files.md",
            ".agents/memory/crystallized/documents/cli/_cli.md",
            ".agents/memory/crystallized/documents/cli/architecture.md",
            ".agents/memory/crystallized/documents/cli/command-contract-set.md",
            ".agents/memory/crystallized/documents/cli/contracts/contract.md",
            ".agents/memory/crystallized/documents/cli/distribution.md",
            ".agents/memory/crystallized/documents/cli/layers/layer.md",
            ".agents/memory/crystallized/documents/cli/shared-operation-contract.md",
            ".agents/memory/crystallized/documents/cli/technical-designs/design.md",
            "README.md",
            "docs/cli.md",
            "docs/demos/demo.md",
            "docs/development.md",
            "docs/extensions.md",
            "docs/generated/in-scope.md",
            "docs/guide.md",
            "docs/templates/in-scope.md",
            "docs/working/in-scope.md",
            "src/docusaurus/docs/demos/demo.md",
            "src/docusaurus/docs/generated/in-scope.md",
            "src/docusaurus/docs/guide.md",
            "src/docusaurus/docs/templates/in-scope.md",
            "src/docusaurus/docs/working/in-scope.md",
            "src/extensions/README.md",
            "src/extensions/alpha/README.md",
            "src/extensions/beta/README.md",
        };

        Assert.Equal(expected, RepositoryMarkdownScope.Enumerate(workspace.Path));
        Assert.True(RepositoryMarkdownScope.IsPublishedSiteSource("src\\docusaurus\\docs\\demos\\demo.md"));
        Assert.True(RepositoryMarkdownScope.IsPublishedSiteSource("docs/cli.md"));
        Assert.True(RepositoryMarkdownScope.IsPublishedSiteSource("docs/extensions.md"));
        Assert.True(RepositoryMarkdownScope.IsPublishedSiteSource("docs/development.md"));
        Assert.False(RepositoryMarkdownScope.IsPublishedSiteSource("docs/generated/in-scope.md"));
    }

    [Fact(DisplayName = "Repository Markdown scope requires every fixed required member")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public void ScopeRejectsMissingRequiredMember()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateScopeCases();
        File.Delete(workspace.Combine("README.md"));

        Assert.Throws<FileNotFoundException>(() => RepositoryMarkdownScope.Enumerate(workspace.Path));
    }

    [Fact(DisplayName = "Repository Markdown scope rejects an optional reparse-point traversal")]
    [Trait("Boundary", "OS"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public void ScopeRejectsReparsePointWhenTheHostPermitsCreation()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateScopeCases();
        var created = workspace.TryCreateFileSymbolicLink(
            "docs/reparse.md",
            workspace.Combine("README.md"),
            out var linkPath);
        if (!created || linkPath is null)
        {
            return;
        }

        try
        {
            Assert.Throws<InvalidOperationException>(() => RepositoryMarkdownScope.Enumerate(workspace.Path));
        }
        finally
        {
            File.Delete(linkPath);
        }
    }

    [Fact(DisplayName = "Repository Markdown checker preserves inline reference and image occurrences")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerCoversInlineReferencesAndImages()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "inline-cases.md");

        Assert.Equal(6, diagnostics.Count);
        Assert.All(
            diagnostics,
            diagnostic => Assert.Contains(": target-missing: ", diagnostic, StringComparison.Ordinal));
        Assert.Equal(
            2,
            diagnostics.Count(diagnostic => diagnostic.EndsWith(
                ": targets/missing-inline.md",
                StringComparison.Ordinal)));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(": targets/missing-full.md", StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(": targets/missing-collapsed.md", StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(": targets/missing-shortcut.md", StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(": images/missing.png", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("missing-unused", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("unresolved", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Repository Markdown checker delegates decoding and rejects malformed local destinations")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerCoversEncodedSpacesUnicodeQueriesCasingAndContainment()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "edge-cases.md");

        Assert.Equal(5, diagnostics.Count);
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": invalid-encoding: targets/bad%ZZ.md",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": destination-malformed: targets/ok.md?view=one",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": target-missing: targets/missing-edge.md",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": target-unsafe: ../outside.md",
            StringComparison.Ordinal));

        var casing = Assert.Single(
            diagnostics,
            diagnostic => diagnostic.Contains(": targets/CaseTarget.md", StringComparison.Ordinal));
        Assert.True(
            casing.Contains(": target-case-mismatch: ", StringComparison.Ordinal)
            || casing.Contains(": target-missing: ", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Repository Markdown checker resolves canonical same-file and cross-file fragments")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerCoversCanonicalFragmentsAndDuplicateSlugs()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "fragments.md");

        Assert.Empty(diagnostics);
    }

    [Fact(DisplayName = "Repository Markdown checker reports uncertain and missing fragment outcomes")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerCoversMissingCaseSetextHtmlAndNonMarkdownFragments()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "fragment-failures.md");

        Assert.Equal(6, diagnostics.Count);
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": fragment-missing: fragment-target.md#missing-heading",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": fragment-missing: fragment-target.md#Target-Heading",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": unverified-anchor: setext-target.md#setext-heading",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": unverified-anchor: html-target.md#html-heading",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": unverified-anchor: assets/page.html#html-heading",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.Contains(
            ": unverified-anchor: directories#anything",
            StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Repository Markdown checker ignores code blocks and frontmatter links")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerDoesNotInventLinksFromCodeOrFrontmatter()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "code-cases.md", "frontmatter-cases.md");

        Assert.Empty(diagnostics);
    }

    [Fact(DisplayName = "Repository Markdown checker delegates published site routes and checks excluded-source destinations")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerDelegatesSiteRoutesAndChecksExcludedSources()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(
            workspace,
            "src/docusaurus/docs/site.md",
            "docs/cli-experience-fixtures/excluded.md",
            "docs/extension-candidates/candidate.md");

        Assert.Equal(3, diagnostics.Count);
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(
            ": ../../../targets/site-missing.md",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(
            ": missing-excluded.md",
            StringComparison.Ordinal));
        Assert.Single(diagnostics, diagnostic => diagnostic.EndsWith(
            ": missing-candidate.md",
            StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("/docs/guide", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("/guides/guide", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("/img/logo.png", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("example.com", StringComparison.Ordinal));
        Assert.DoesNotContain(diagnostics, diagnostic => diagnostic.Contains("mailto:", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Repository Markdown checker preserves stable source and occurrence diagnostic order")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerOrdersDiagnosticsAndPreservesFixtureHashes()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();
        var before = workspace.SnapshotHashes();

        var diagnostics = await RepositoryMarkdownChecker.CheckAsync(
            workspace.Path,
            ["order-z.md", "order-a.md"],
            TestContext.Current.CancellationToken);

        Assert.Equal(
            [
                "order-a.md:1:1: target-missing: missing-first.md",
                "order-a.md:2:1: target-missing: missing-second.md",
                "order-z.md:1:1: target-missing: missing-third.md",
            ],
            diagnostics);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Repository Markdown checker reports invalid UTF-8 and unavailable Markdown facts")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerReportsSourceReadFailures()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();

        var diagnostics = await CheckAsync(workspace, "invalid-utf8.md", "unavailable.md");

        Assert.Equal(
            [
                "invalid-utf8.md:1:1: invalid-encoding: invalid-utf8.md",
                "unavailable.md:1:1: markdown-unavailable: unavailable.md",
            ],
            diagnostics);
    }

    [Fact(DisplayName = "Repository Markdown checker propagates cancellation before reading sources")]
    [Trait("Boundary", "Input"), Trait("Feature", "repository-documentation"), Trait("Evidence", "Integration")]
    public async Task CheckerPropagatesCancellation()
    {
        using var workspace = RepositoryMarkdownFixtures.CreateLinkCases();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Assert.ThrowsAsync<OperationCanceledException>(() =>
            RepositoryMarkdownChecker.CheckAsync(
                workspace.Path,
                ["inline-cases.md"],
                cancellation.Token));
    }

    private static Task<IReadOnlyList<string>> CheckAsync(
        TemporaryWorkspace workspace,
        params string[] sourcePaths)
        => RepositoryMarkdownChecker.CheckAsync(
            workspace.Path,
            sourcePaths,
            TestContext.Current.CancellationToken);

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "OpenForge.Cli.slnx")))
        {
            directory = directory.Parent;
        }

        Assert.NotNull(directory);
        return directory.FullName;
    }
}
