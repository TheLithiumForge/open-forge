using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Loading;

[Trait("Boundary", "Processing"), Trait("Feature", "source-loading"), Trait("Evidence", "Unit")]
public sealed class SourceOutsideLoadingTests
{
    private const string Directory = "docs/team";
    private const string ChildName = "guide" + SourceFormClassifier.MarkdownExtension;
    private const string Description = "Row-owned policy";
    private const string Condition = "src/**";
    private static readonly string Anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);

    [Theory(DisplayName = "Registered intrinsic catalogues preserve linked row conditions without acquiring frontmatter loading policy")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false), InlineData(true, true)]
    public void RegistrationPreservesLinkedPolicy(bool registered, bool matches)
    {
        var layout = Layout(registered);
        var condition = ApplyToMetadataFacts.FromEntryLine([new ApplyToPattern(Condition, [.. Condition.Split('/')])]);
        var catalogue = Source(Anchor, registered ? SourceDocumentForm.CanonicalEntrypoint : SourceDocumentForm.Markdown,
            layout.LoaderPath, Entry(ChildName)) with
        { IsLinked = true, LinkedApplyTo = condition };
        var child = Source(Directory + "/" + ChildName, SourceDocumentForm.Markdown, Anchor) with { IsLinked = true };
        var result = new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            Layout = layout,
            WorkspaceEntryPath = SourceLogicalPath.WorkspaceEntryPath,
            LoaderRootPaths = registered ? [Anchor] : [],
            IncludeLinkedSources = true,
            EvaluateApplicability = true,
            WorkingPathsSupplied = true,
            WorkingPaths = [matches ? "src/feature.cs" : "tests/feature.cs"],
            Sources = [Source(layout.LoaderPath, SourceDocumentForm.Loader, null, Entry("../" + Anchor)), catalogue, child],
        });

        Assert.Empty(result.Issues);
        Assert.Equal(matches, result.Startup.Any(selection => selection.Path == Anchor));
        Assert.Equal(matches, result.Startup.Any(selection => selection.Path == child.Path));
    }

    [Fact(DisplayName = "Unlisted registered catalogue forms do not activate loading")]
    public void RegistrationDoesNotActivate()
    {
        var layout = Layout(true);
        var result = new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            Layout = layout,
            WorkspaceEntryPath = SourceLogicalPath.WorkspaceEntryPath,
            LoaderRootPaths = [Anchor],
            Sources = [Source(layout.LoaderPath, SourceDocumentForm.Loader, null), Source(Anchor, SourceDocumentForm.CanonicalEntrypoint, null)],
        });

        Assert.Empty(result.Issues);
        Assert.DoesNotContain(result.Startup, selection => selection.Path == Anchor);
    }

    private static SourceLoadingClosureSource Source(string path, SourceDocumentForm form, string? parent, params SourceGeneratedEntry[] entries)
        => new()
        {
            Path = path,
            Form = form,
            ParentPath = parent,
            RouteState = SourceRouteState.Unrouted,
            Metadata = SourceAuthoredMetadataFacts.Complete(Description, []),
            GeneratedEntries = SourceGeneratedEntriesFacts.Complete(entries),
        };

    private static SourceGeneratedEntry Entry(string destination)
        => new(Description, destination, [SourceLoadingClosureResolver.LoadNowTag], new MarkdownTextSpan(0, 1));

    private static WorkspaceLayout Layout(bool registered)
    {
        var layout = WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory), WorkspaceLayout.DefaultMainRoot);
        return registered ? layout.WithOutsideRoutes([Directory]) : layout;
    }
}
