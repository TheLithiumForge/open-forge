using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindSourceUniverseIntegrationTests
{
    [Fact(DisplayName = "Find query enumerates every eligible source form and applies physical selector expansion without writes")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task RealWorkspaceEnumeratesEveryEligibleFormAndAppliesPhysicalFilterExpansion()
    {
        using var workspace = TemporaryWorkspace.Create("find-source-universe-forms");
        workspace.CreateDirectory(".agents");
        WriteEveryEligibleSourceForm(workspace);
        var cliWorkspace = CreateWorkspace(workspace);
        var before = workspace.SnapshotHashes();

        var firstCatalogue = await ReadCatalogueAsync(cliWorkspace);
        var secondCatalogue = await ReadCatalogueAsync(cliWorkspace);

        Assert.False(firstCatalogue.IsCancelled);
        Assert.Empty(firstCatalogue.Issues);
        Assert.Equal(
            [
                ".agents/guidance/_guidance.md",
                ".agents/guidance/leaf.md",
                ".agents/guidance/leaf.overwrite.md",
                ".agents/guidance/unrouted.md",
                ".agents/index/index.md",
                ".agents/loader.md",
                ".agents/references/references.md",
                ".agents/skills/tool/SKILL.md",
                ".agents/skills/tool/reference.md",
                ".agents/underscore-index/_index.md",
                ".agents/underscore-references/_references.md",
            ],
            firstCatalogue.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            [
                SourceDocumentForm.CanonicalEntrypoint,
                SourceDocumentForm.Markdown,
                SourceDocumentForm.OverwriteCompanion,
                SourceDocumentForm.Markdown,
                SourceDocumentForm.IndexEntrypoint,
                SourceDocumentForm.Loader,
                SourceDocumentForm.ReferencesEntrypoint,
                SourceDocumentForm.Skill,
                SourceDocumentForm.Markdown,
                SourceDocumentForm.UnderscoreIndexEntrypoint,
                SourceDocumentForm.UnderscoreReferencesEntrypoint,
            ],
            firstCatalogue.Candidates.Select(candidate => candidate.Form));
        Assert.Equal(
            [
                "guidance",
                "guidance/leaf",
                "guidance/unrouted",
                "index",
                "loader",
                "references",
                "skills/tool",
                "skills/tool/reference",
                "underscore-index",
                "underscore-references",
            ],
            firstCatalogue.Sources.Select(source => source.Identity.AutomaticId));
        Assert.Equal(
            firstCatalogue.Candidates.Select(candidate => candidate.CanonicalPath),
            secondCatalogue.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            firstCatalogue.Sources.Select(source => source.Identity.CanonicalBasePath),
            secondCatalogue.Sources.Select(source => source.Identity.CanonicalBasePath));

        var paired = Assert.Single(
            firstCatalogue.Sources,
            source => source.Identity.CanonicalBasePath == ".agents/guidance/leaf.md");
        Assert.Equal(
            ".agents/guidance/leaf.overwrite.md",
            Assert.IsType<SourceLayer>(paired.Overwrite).CanonicalPath);

        var include = new[]
        {
            "loader",
            ".agents/guidance/_guidance.md",
            ".agents/index/index.md",
            ".agents/underscore-index/_index.md",
            ".agents/references/references.md",
            ".agents/underscore-references/_references.md",
            ".agents/skills/tool/SKILL.md",
            "guidance/leaf",
        };
        var exclude = new[] { ".agents/guidance/unrouted.md" };
        var firstResolution = ResolveUniverse(firstCatalogue, cliWorkspace, include, exclude);
        var secondResolution = ResolveUniverse(secondCatalogue, cliWorkspace, include, exclude);

        Assert.Equal(FindUniverseMode.Filtered, firstResolution.Universe.Mode);
        Assert.Collection(
            firstResolution.Universe.Include,
            selector => AssertResolvedSelector(
                selector,
                "loader",
                SourceReferenceKind.SourceId,
                FindSourceKind.Loader,
                FindSelectorExpansion.Folder,
                "loader",
                ".agents/loader.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/guidance/_guidance.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                "guidance",
                ".agents/guidance/_guidance.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/index/index.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                "index",
                ".agents/index/index.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/underscore-index/_index.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                "underscore-index",
                ".agents/underscore-index/_index.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/references/references.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                "references",
                ".agents/references/references.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/underscore-references/_references.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Entrypoint,
                FindSelectorExpansion.Folder,
                "underscore-references",
                ".agents/underscore-references/_references.md"),
            selector => AssertResolvedSelector(
                selector,
                ".agents/skills/tool/SKILL.md",
                SourceReferenceKind.SourcePath,
                FindSourceKind.Skill,
                FindSelectorExpansion.Folder,
                "skills/tool",
                ".agents/skills/tool/SKILL.md"),
            selector => AssertResolvedSelector(
                selector,
                "guidance/leaf",
                SourceReferenceKind.SourceId,
                FindSourceKind.Ordinary,
                FindSelectorExpansion.Source,
                "guidance/leaf",
                ".agents/guidance/leaf.md"));
        var excludedSelector = Assert.Single(firstResolution.Universe.Exclude);
        AssertResolvedSelector(
            excludedSelector,
            ".agents/guidance/unrouted.md",
            SourceReferenceKind.SourcePath,
            FindSourceKind.Ordinary,
            FindSelectorExpansion.Source,
            "guidance/unrouted",
            ".agents/guidance/unrouted.md");

        Assert.Equal(
            [
                "guidance",
                "guidance/leaf",
                "index",
                "loader",
                "references",
                "skills/tool",
                "skills/tool/reference",
                "underscore-index",
                "underscore-references",
            ],
            firstResolution.Selection.Sources.Select(source => source.Identity.AutomaticId));
        Assert.Equal(9, firstResolution.Universe.CandidateCount);
        Assert.Equal(9, firstResolution.Selection.Sources.Count);
        Assert.Equal(10, firstResolution.Selection.Candidates.Count);
        Assert.Contains(
            firstResolution.Selection.Candidates,
            candidate => candidate.CanonicalPath == ".agents/guidance/leaf.overwrite.md");
        Assert.DoesNotContain(
            firstResolution.Selection.Candidates,
            candidate => candidate.CanonicalPath == ".agents/guidance/unrouted.md");
        Assert.Empty(firstResolution.Selection.Issues);
        Assert.Empty(firstResolution.Findings);
        Assert.Equal(
            firstResolution.Selection.Sources.Select(source => source.Identity.CanonicalBasePath),
            secondResolution.Selection.Sources.Select(source => source.Identity.CanonicalBasePath));
        Assert.Equal(
            firstResolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath),
            secondResolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            firstResolution.Findings.Select(finding => finding.Code),
            secondResolution.Findings.Select(finding => finding.Code));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Find query excludes unreadable physical areas before inspection while retaining unsafe and orphan findings")]
    [Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    public async Task ExcludedUnreadableAreasRemainUnreadWhileUnsafeAndOrphanCandidatesStayVisible()
    {
        using var outside = TemporaryWorkspace.Create("find-source-universe-outside");
        var excludedTarget = outside.CreateFile("excluded-unreadable.md", [0xFF, 0xFE]);
        var retainedTarget = outside.CreateFile("retained-unreadable.md", [0xFF, 0xFE]);
        using var workspace = TemporaryWorkspace.Create("find-source-universe-excluded");
        workspace.CreateDirectory(".agents");
        workspace.WriteText(".agents/excluded/_excluded.md", "# Excluded\n");
        workspace.WriteText(".agents/excluded/safe.md", "# Excluded safe\n");
        workspace.WriteText(".agents/excluded/orphan.overwrite.md", "excluded orphan\n");
        workspace.WriteText(".agents/retained/_retained.md", "# Retained\n");
        workspace.WriteText(".agents/retained/safe.md", "# Retained safe\n");
        workspace.WriteText(".agents/retained/orphan.overwrite.md", "retained orphan\n");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/excluded/unreadable.md",
                excludedTarget,
                out _),
            "This integration case requires real symbolic-link support.");
        Assert.True(
            workspace.TryCreateFileSymbolicLink(
                ".agents/retained/unreadable.md",
                retainedTarget,
                out _),
            "This integration case requires real symbolic-link support.");

        var cliWorkspace = CreateWorkspace(workspace);
        var before = workspace.SnapshotHashes();
        var outsideBefore = outside.SnapshotHashes();
        var catalogue = await ReadCatalogueAsync(cliWorkspace);

        Assert.Contains(
            catalogue.Issues,
            issue => issue.Code == SourceCatalogueIssueCode.CandidateUnsafe
                && issue.AttemptedCanonicalPath == ".agents/excluded/unreadable.md");
        Assert.Contains(
            catalogue.Issues,
            issue => issue.Code == SourceCatalogueIssueCode.CandidateUnsafe
                && issue.AttemptedCanonicalPath == ".agents/retained/unreadable.md");
        Assert.Contains(
            catalogue.Issues,
            issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite
                && issue.AttemptedCanonicalPath == ".agents/excluded/orphan.overwrite.md");
        Assert.Contains(
            catalogue.Issues,
            issue => issue.Code == SourceCatalogueIssueCode.OrphanOverwrite
                && issue.AttemptedCanonicalPath == ".agents/retained/orphan.overwrite.md");
        Assert.DoesNotContain(
            catalogue.Sources,
            source => source.Identity.CanonicalBasePath.EndsWith("unreadable.md", StringComparison.Ordinal));
        Assert.DoesNotContain(
            catalogue.Sources,
            source => source.Identity.CanonicalBasePath.EndsWith("orphan.overwrite.md", StringComparison.Ordinal));

        var firstResolution = ResolveUniverse(
            catalogue,
            cliWorkspace,
            [],
            [".agents/excluded/_excluded.md"]);
        var secondResolution = ResolveUniverse(
            catalogue,
            cliWorkspace,
            [],
            [".agents/excluded/_excluded.md"]);

        Assert.Equal(FindUniverseMode.Filtered, firstResolution.Universe.Mode);
        AssertResolvedSelector(
            Assert.Single(firstResolution.Universe.Exclude),
            ".agents/excluded/_excluded.md",
            SourceReferenceKind.SourcePath,
            FindSourceKind.Entrypoint,
            FindSelectorExpansion.Folder,
            "excluded",
            ".agents/excluded/_excluded.md");
        Assert.Equal(
            ["retained", "retained/safe"],
            firstResolution.Selection.Sources.Select(source => source.Identity.AutomaticId));
        Assert.Equal(
            [
                ".agents/retained/_retained.md",
                ".agents/retained/orphan.overwrite.md",
                ".agents/retained/safe.md",
                ".agents/retained/unreadable.md",
            ],
            firstResolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(4, firstResolution.Universe.CandidateCount);
        Assert.DoesNotContain(
            firstResolution.Selection.Candidates,
            candidate => candidate.CanonicalPath.StartsWith(".agents/excluded/", StringComparison.Ordinal));
        Assert.DoesNotContain(
            firstResolution.Selection.Issues,
            issue => issue.AttemptedCanonicalPath.StartsWith(".agents/excluded/", StringComparison.Ordinal));
        Assert.Collection(
            firstResolution.Selection.Issues,
            issue =>
            {
                Assert.Equal(SourceCatalogueIssueCode.CandidateUnsafe, issue.Code);
                Assert.Equal(".agents/retained/unreadable.md", issue.AttemptedCanonicalPath);
            },
            issue =>
            {
                Assert.Equal(SourceCatalogueIssueCode.OrphanOverwrite, issue.Code);
                Assert.Equal(".agents/retained/orphan.overwrite.md", issue.AttemptedCanonicalPath);
            });
        Assert.Collection(
            firstResolution.Findings,
            finding =>
            {
                Assert.Equal(FindFindingCode.CandidateUnsafe, finding.Code);
                Assert.Equal(CliSemanticStatus.Incomplete, finding.Status);
                Assert.Equal(".agents/retained/unreadable.md", finding.Path);
            },
            finding =>
            {
                Assert.Equal(FindFindingCode.LayerUnresolved, finding.Code);
                Assert.Equal(CliSemanticStatus.Incomplete, finding.Status);
                Assert.Equal(".agents/retained/orphan.overwrite.md", finding.Path);
            });
        Assert.Equal(
            firstResolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath),
            secondResolution.Selection.Candidates.Select(candidate => candidate.CanonicalPath));
        Assert.Equal(
            firstResolution.Findings.Select(finding => (finding.Code, finding.Path)),
            secondResolution.Findings.Select(finding => (finding.Code, finding.Path)));
        Assert.Equal(before, workspace.SnapshotHashes());
        Assert.Equal(outsideBefore, outside.SnapshotHashes());
    }

    private static void WriteEveryEligibleSourceForm(TemporaryWorkspace workspace)
    {
        workspace.WriteText(".agents/loader.md", "# Loader\n");
        workspace.WriteText(".agents/guidance/_guidance.md", "# Guidance\n");
        workspace.WriteText(".agents/guidance/leaf.md", "# Leaf\n");
        workspace.WriteText(".agents/guidance/leaf.overwrite.md", "# Leaf overwrite\n");
        workspace.WriteText(".agents/guidance/unrouted.md", "# Unrouted\n");
        workspace.WriteText(".agents/index/index.md", "# Index\n");
        workspace.WriteText(".agents/references/references.md", "# References\n");
        workspace.WriteText(".agents/skills/tool/SKILL.md", "# Tool\n");
        workspace.WriteText(".agents/skills/tool/reference.md", "# Reference\n");
        workspace.WriteText(".agents/underscore-index/_index.md", "# Underscore index\n");
        workspace.WriteText(".agents/underscore-references/_references.md", "# Underscore references\n");
    }

    private static CliWorkspace CreateWorkspace(TemporaryWorkspace workspace)
    {
        return new CliWorkspace(
            workspace.Path,
            workspace.Path,
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
    }

    private static async ValueTask<SourceCatalogue> ReadCatalogueAsync(CliWorkspace workspace)
    {
        return await new SourceCatalogueReader().ReadAsync(
            new SourceCatalogueRequest(workspace, [".agents"]),
            TestContext.Current.CancellationToken);
    }

    private static FindUniverseResolution ResolveUniverse(
        SourceCatalogue catalogue,
        CliWorkspace workspace,
        IEnumerable<string> include,
        IEnumerable<string> exclude)
    {
        var sourceSession = new SourceReadSession(
            catalogue,
            new SourceDocumentReader(workspace),
            new SourceCatalogueSelectionScope(
                ".agents",
                SourceLogicalPath.ToLexicalPath(workspace.PhysicalRoot, ".agents")));
        var input = new FindUniverseInput(
            CreateRequest(workspace, include, exclude),
            sourceSession);
        return new FindUniverseResolver(ResolvePhysicalPath).Resolve(input);
    }

    private static FindRequest CreateRequest(
        CliWorkspace workspace,
        IEnumerable<string> include,
        IEnumerable<string> exclude)
    {
        var query = new FindQuery(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
                [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]));
        var presentation = new FindPresentationSelection(
            null,
            CliView.Expanded,
            new FindContentSelection([], []));
        return new FindRequest(
            workspace,
            new FindUniverseFilter(include, exclude),
            query,
            presentation);
    }

    private static PhysicalPathResolution ResolvePhysicalPath(
        CliWorkspace workspace,
        string canonicalPath)
    {
        return new PhysicalPathResolver().ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, canonicalPath));
    }

    private static void AssertResolvedSelector(
        FindSelector selector,
        string value,
        SourceReferenceKind form,
        FindSourceKind sourceKind,
        FindSelectorExpansion expansion,
        string id,
        string path)
    {
        Assert.Equal(value, selector.Value);
        Assert.Equal(form, selector.Form);
        Assert.Equal(FindSelectorResolution.Resolved, selector.Resolution);
        Assert.Equal(sourceKind, selector.SourceKind);
        Assert.Equal(expansion, selector.Expansion);
        var identity = Assert.IsType<FindSourceIdentity>(selector.Identity);
        Assert.Equal(id, identity.Id);
        Assert.Equal(path, identity.Path);
        Assert.Empty(selector.Candidates);
    }
}
