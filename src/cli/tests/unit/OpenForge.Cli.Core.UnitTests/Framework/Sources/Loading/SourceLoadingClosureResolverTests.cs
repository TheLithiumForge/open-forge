using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Loading;

public sealed class SourceLoadingClosureResolverTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loading closure visits each generated branch depth-first"), Trait("Feature", "source-loading-closure"), Trait("Evidence", "Unit")]
    public void BranchingSourcesUseDepthFirstGeneratedOrder()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(
                Entry("memory/_memory.md", "LoadNow"),
                Entry("patterns/_patterns.md", "LoadNow"),
                Entry("skills/_skills.md", "LoadNow")),
            parentPath: null);
        var memory = Source(
            ".agents/memory/_memory.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(
                Entry("crystallized/_crystallized.md", "LoadNow"),
                Entry("working/_working.md", "LoadNow")),
            null,
            "LoadNow");
        var crystallized = Source(
            ".agents/memory/crystallized/_crystallized.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("analysis/_analysis.md", "LoadNow")),
            memory.Path,
            "LoadNow");
        var analysis = Source(
            ".agents/memory/crystallized/analysis/_analysis.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            crystallized.Path,
            "LoadNow");
        var working = Source(
            ".agents/memory/working/_working.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            memory.Path,
            "LoadNow");
        var patterns = Source(
            ".agents/patterns/_patterns.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            null,
            "LoadNow");
        var skills = Source(
            ".agents/skills/_skills.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            null,
            "LoadNow");

        var resolution = Resolve(
            [loader, memory, crystallized, analysis, working, patterns, skills],
            [memory.Path, patterns.Path, skills.Path]);

        Assert.Empty(resolution.Issues);
        Assert.Equal(
            [
                "AGENTS.md",
                loader.Path,
                memory.Path,
                crystallized.Path,
                analysis.Path,
                working.Path,
                patterns.Path,
                skills.Path,
            ],
            Paths(resolution));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loading closure handles a deep finite chain iteratively"), Trait("Feature", "source-loading-closure"), Trait("Evidence", "Unit")]
    public void DeepFiniteChainDoesNotRequireRecursion()
    {
        const int depth = 4096;
        var sources = new List<SourceLoadingClosureSource>(depth + 1);
        var expectedPaths = new List<string>(depth + 2) { "AGENTS.md", ".agents/loader.md" };
        var currentDirectory = ".agents/deep";
        var currentPath = $"{currentDirectory}/_deep.md";
        string? parentPath = null;

        for (var index = 0; index < depth; index++)
        {
            var nextSegment = index == depth - 1 ? null : $"n{index:X}";
            var generated = nextSegment is null
                ? Entries()
                : Entries(Entry($"{nextSegment}/_{nextSegment}.md", "LoadNow"));
            sources.Add(Source(
                currentPath,
                SourceDocumentForm.CanonicalEntrypoint,
                generated,
                parentPath,
                "LoadNow"));
            expectedPaths.Add(currentPath);

            if (nextSegment is not null)
            {
                parentPath = currentPath;
                currentDirectory = $"{currentDirectory}/{nextSegment}";
                currentPath = $"{currentDirectory}/_{nextSegment}.md";
            }
        }

        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(Entry("deep/_deep.md", "LoadNow")),
            parentPath: null);
        sources.Insert(0, loader);

        var resolution = Resolve(sources, [".agents/deep/_deep.md"]);

        Assert.Empty(resolution.Issues);
        Assert.Empty(resolution.ContinuityPaths);
        Assert.Equal(expectedPaths, Paths(resolution));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loading closure carries LoadNow and KeepInMind through continuity"), Trait("Feature", "source-loading-closure"), Trait("Evidence", "Unit")]
    public void ContinuityPropagatesThroughSelectedDescendants()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(
                Entry("mind/_mind.md", "KeepInMind"),
                Entry("now/_now.md", "LoadNow")),
            parentPath: null);
        var mind = Source(
            ".agents/mind/_mind.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            null,
            "KeepInMind");
        var child = Source(
            ".agents/mind/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            mind.Path,
            "LoadNow");
        var now = Source(
            ".agents/now/_now.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            null,
            "LoadNow");

        var resolution = Resolve([loader, mind, child, now], [mind.Path, now.Path]);

        Assert.Empty(resolution.Issues);
        Assert.Equal([mind.Path, child.Path], resolution.ContinuityPaths);
        Assert.Equal(["AGENTS.md", loader.Path, mind.Path, child.Path, now.Path], Paths(resolution));
        Assert.Contains(
            resolution.Startup.Single(selection => selection.Path == mind.Path).Reasons,
            reason => reason.Kind == SourceLoadingClosureReasonKind.KeepInMind
                && reason.SourcePath == loader.Path);
        Assert.Contains(
            resolution.Startup.Single(selection => selection.Path == child.Path).Reasons,
            reason => reason.Kind == SourceLoadingClosureReasonKind.LoadNow
                && reason.SourcePath == mind.Path);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loading closure merges duplicate reasons and terminates cycles"), Trait("Feature", "source-loading-closure"), Trait("Evidence", "Unit")]
    public void DuplicateReasonsAndCyclesRemainBounded()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(
                Entry("alpha/_alpha.md", "LoadNow"),
                Entry("alpha/_alpha.md", "KeepInMind")),
            parentPath: null);
        var alpha = Source(
            ".agents/alpha/_alpha.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child/_child.md", "LoadNow")),
            parentPath: ".agents/alpha/child/_child.md",
            "LoadNow",
            "KeepInMind");
        var child = Source(
            ".agents/alpha/child/_child.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("../_alpha.md", "LoadNow")),
            alpha.Path,
            "LoadNow");

        var resolution = Resolve([loader, alpha, child], [alpha.Path]);

        Assert.Empty(resolution.Issues);
        Assert.Equal(["AGENTS.md", loader.Path, alpha.Path, child.Path], Paths(resolution));
        Assert.Equal([alpha.Path, child.Path], resolution.ContinuityPaths);
        var alphaSelection = resolution.Startup.Single(selection => selection.Path == alpha.Path);
        Assert.Equal(3, alphaSelection.Reasons.Count);
        Assert.Contains(
            alphaSelection.Reasons,
            reason => reason.Kind == SourceLoadingClosureReasonKind.LoadNow
                && reason.SourcePath == loader.Path);
        Assert.Contains(
            alphaSelection.Reasons,
            reason => reason.Kind == SourceLoadingClosureReasonKind.LoadNow
                && reason.SourcePath == child.Path);
        Assert.Contains(
            alphaSelection.Reasons,
            reason => reason.Kind == SourceLoadingClosureReasonKind.KeepInMind
                && reason.SourcePath == loader.Path);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Loading closure leaves an unselected scope untouched"), Trait("Feature", "source-loading-closure"), Trait("Evidence", "Unit")]
    public void UnselectedScopeDoesNotActivateDescendants()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(
                Entry("selected/_selected.md", "LoadNow"),
                Entry("inactive/_inactive.md")),
            parentPath: null);
        var selected = Source(
            ".agents/selected/_selected.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            null,
            "LoadNow");
        var selectedChild = Source(
            ".agents/selected/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            selected.Path,
            "LoadNow");
        var inactive = Source(
            ".agents/inactive/_inactive.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            parentPath: null);
        var inactiveChild = Source(
            ".agents/inactive/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            inactive.Path,
            "LoadNow");

        var resolution = Resolve(
            [loader, selected, selectedChild, inactive, inactiveChild],
            [selected.Path, inactive.Path]);

        Assert.Empty(resolution.Issues);
        Assert.Equal(
            ["AGENTS.md", loader.Path, selected.Path, selectedChild.Path],
            Paths(resolution));
        Assert.DoesNotContain(resolution.Startup, selection => selection.Path == inactive.Path);
        Assert.DoesNotContain(resolution.Startup, selection => selection.Path == inactiveChild.Path);
    }

    private static SourceLoadingClosureSource Source(
        string path,
        SourceDocumentForm form,
        SourceGeneratedEntriesFacts generatedEntries,
        string? parentPath,
        params string[] tags)
        => new()
        {
            Path = path,
            Form = form,
            RouteState = SourceRouteState.Routed,
            ParentPath = parentPath,
            Metadata = form == SourceDocumentForm.Loader
                ? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.NotApplicable)
                : SourceAuthoredMetadataFacts.Complete("Test source.", tags),
            GeneratedEntries = generatedEntries,
        };

    private static SourceLoadingClosureResolution Resolve(
        IReadOnlyList<SourceLoadingClosureSource> sources,
        IReadOnlyList<string> loaderRootPaths)
        => new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            WorkspaceEntryPath = "AGENTS.md",
            Sources = sources,
            LoaderRootPaths = loaderRootPaths,
        });

    private static IReadOnlyList<string> Paths(SourceLoadingClosureResolution resolution)
        => resolution.Startup.Select(selection => selection.Path).ToArray();

    private static SourceGeneratedEntriesFacts Entries(params SourceGeneratedEntry[] entries)
        => SourceGeneratedEntriesFacts.Complete(entries);

    private static SourceGeneratedEntry Entry(
        string destination,
        params string[] tags)
        => new(destination, destination, tags, new MarkdownTextSpan(0, 1));
}
