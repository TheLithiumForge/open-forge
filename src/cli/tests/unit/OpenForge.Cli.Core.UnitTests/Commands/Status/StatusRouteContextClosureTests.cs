using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.Routes.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status;

public sealed class StatusRouteContextClosureTests
{
    [Fact(DisplayName = "Status route closure excludes unrouted global continuity"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void UnroutedGlobalContinuityIsExcludedAndIncomplete()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(),
            parentPath: null);
        var root = Source(
            ".agents/team/_team.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            parentPath: null);
        var detached = Source(
            ".agents/team/detached.md",
            SourceDocumentForm.Markdown,
            Entries(),
            root.Path,
            ["KeepInMind"]) with
        {
            RouteState = SourceRouteState.Unrouted,
        };

        var closure = Resolve([loader, root, detached], [root.Path]);

        Assert.DoesNotContain(closure.Startup, source => source.Path == detached.Path);
        Assert.DoesNotContain(closure.Continuity, source => source.Path == detached.Path);
        Assert.False(closure.IsComplete);
    }

    [Fact(DisplayName = "Status route closure keeps each continuity source with its visible closure"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void ContinuitySourceVisibleClosurePrecedesNextSource()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(),
            parentPath: null);
        var alphaRoot = Source(
            ".agents/alpha/_alpha.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            parentPath: null);
        var alphaContinuity = Source(
            ".agents/alpha/continuity.md",
            SourceDocumentForm.Markdown,
            Entries(),
            alphaRoot.Path,
            ["KeepInMind"]);
        var alphaChild = Source(
            ".agents/alpha/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            alphaRoot.Path,
            ["LoadNow"]);
        var betaRoot = Source(
            ".agents/beta/_beta.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            parentPath: null);
        var betaContinuity = Source(
            ".agents/beta/continuity.md",
            SourceDocumentForm.Markdown,
            Entries(),
            betaRoot.Path,
            ["KeepInMind"]);
        var betaChild = Source(
            ".agents/beta/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            betaRoot.Path,
            ["LoadNow"]);

        var closure = Resolve(
            [loader, alphaRoot, alphaContinuity, alphaChild, betaRoot, betaContinuity, betaChild],
            [alphaRoot.Path, betaRoot.Path]);

        Assert.Equal(
            [
                alphaRoot.Path,
                alphaContinuity.Path,
                alphaChild.Path,
                betaRoot.Path,
                betaContinuity.Path,
                betaChild.Path,
            ],
            closure.Continuity.Select(source => source.Path));
    }

    [Fact(DisplayName = "Status route closure keeps traversing after a duplicate entrypoint"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void DuplicateTraversalBeforeLaterUniqueEntryPointDoesNotSuppressDescendant()
    {
        var loader = Source(
            ".agents/loader.md",
            SourceDocumentForm.Loader,
            Entries(
                Entry("alpha/_alpha.md", "KeepInMind"),
                Entry("alpha/_alpha.md", "KeepInMind"),
                Entry("beta/_beta.md", "KeepInMind")),
            parentPath: null);
        var alpha = Source(
            ".agents/alpha/_alpha.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(),
            parentPath: null);
        var beta = Source(
            ".agents/beta/_beta.md",
            SourceDocumentForm.CanonicalEntrypoint,
            Entries(Entry("child.md", "LoadNow")),
            parentPath: null);
        var child = Source(
            ".agents/beta/child.md",
            SourceDocumentForm.Markdown,
            Entries(),
            beta.Path,
            ["LoadNow"]);
        var closure = Resolve([loader, alpha, beta, child], [alpha.Path, beta.Path]);

        Assert.Contains(closure.Startup, source => source.Path == child.Path);
        Assert.Contains(closure.Continuity, source => source.Path == child.Path);
    }

    private static RouteContextSource Source(
        string path,
        SourceDocumentForm form,
        SourceGeneratedEntriesFacts generatedEntries,
        string? parentPath,
        IReadOnlyList<string>? tags = null)
        => new()
        {
            Id = path,
            Path = path,
            Form = form,
            RouteState = SourceRouteState.Routed,
            Layers = [new RouteContextLayer(path, "# Test\n")],
            Metadata = form == SourceDocumentForm.Loader
                ? SourceAuthoredMetadataFacts.WithoutValues(SourceAuthoredMetadataState.NotApplicable)
                : SourceAuthoredMetadataFacts.Complete("Test source.", tags ?? []),
            GeneratedEntries = generatedEntries,
            ParentPath = parentPath,
        };

    private static RouteContextClosure Resolve(
        IReadOnlyList<RouteContextSource> sources,
        IReadOnlyList<string> rootPaths)
        => new RouteContextClosureResolver().Resolve(new RouteContextSet
        {
            WorkspaceEntry = Source(
                "AGENTS.md",
                SourceDocumentForm.Markdown,
                SourceGeneratedEntriesFacts.Absent,
                parentPath: null),
            Sources = sources,
            RootPaths = rootPaths,
            RootCategories = [],
            IsComplete = true,
        });

    private static SourceGeneratedEntriesFacts Entries(params SourceGeneratedEntry[] entries)
        => SourceGeneratedEntriesFacts.Complete(entries);

    private static SourceGeneratedEntry Entry(string destination, string tag)
        => new(destination, destination, [tag], new MarkdownTextSpan(0, 1));
}
