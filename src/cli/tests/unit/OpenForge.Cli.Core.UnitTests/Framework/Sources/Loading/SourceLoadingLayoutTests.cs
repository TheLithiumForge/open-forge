using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Sources.Models.Metadata;
using OpenForge.Cli.Core.Framework.Sources.Models.Routing;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Loading;

[Trait("Boundary", "Processing"), Trait("Feature", "source-loading"), Trait("Evidence", "Unit")]
public sealed class SourceLoadingLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string Description = "Required source";
    private const string SkillName = "example";
    private const string SkillsCatalogue = WorkspaceLayout.NativeSkillsPath + "/_" + WorkspaceLayoutNames.SkillsDirectoryName + SourceFormClassifier.MarkdownExtension;
    private const string Skill = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.NativeSkillFileName;
    private const string References = WorkspaceLayout.NativeSkillsPath + "/" + SkillName + "/" + SourceFormClassifier.ReferencesFileName;
    private const string Outside = "project/guide" + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Loader destinations and closure preserve native Skills across content locations")]
    [InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(CustomRoot)]
    public void NativeSkillsRemainPrimary(string mainRoot)
    {
        var layout = WorkspaceLayout.Create(new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory,
            CliWorkspaceSelectionMethod.CurrentDirectory), mainRoot);
        var nativeDestination = mainRoot == WorkspaceLayout.DefaultMainRoot
            ? WorkspaceLayoutNames.SkillsDirectoryName + "/" + Path.GetFileName(SkillsCatalogue)
            : "../../" + SkillsCatalogue;
        var outsideDestination = string.Concat(Enumerable.Repeat("../", mainRoot.Split('/').Length)) + Outside;
        var parsed = SourceLoaderDestinationParser.Parse(layout, nativeDestination);
        Assert.Equal(SourceLoaderDestinationParseState.Valid, parsed.State);
        Assert.Equal(SkillsCatalogue, parsed.CanonicalPath);
        Assert.Equal(SkillsCatalogue, SourceGeneratedDestinationResolver.Resolve(layout, layout.LoaderPath, true, nativeDestination));
        Assert.Null(SourceGeneratedDestinationResolver.ResolveLinked(layout, layout.LoaderPath, true, nativeDestination));
        var external = SourceLoaderDestinationParser.Parse(layout, outsideDestination);
        Assert.Equal(SourceLoaderDestinationParseState.Linked, external.State);
        Assert.Equal(Outside, external.CanonicalPath);
        if (layout.IsDefault)
            Assert.Null(SourceGeneratedDestinationResolver.Resolve(layout, layout.LoaderPath, true, "../" + SkillsCatalogue));

        var result = new SourceLoadingClosureResolver().Resolve(new SourceLoadingClosureRequest
        {
            Layout = layout,
            WorkspaceEntryPath = SourceLogicalPath.WorkspaceEntryPath,
            LoaderRootPaths = [SkillsCatalogue],
            Sources =
            [
                Source(layout.LoaderPath, SourceDocumentForm.Loader, null, Entry(nativeDestination)),
                Source(SkillsCatalogue, SourceDocumentForm.CanonicalEntrypoint, layout.LoaderPath,
                    Entry(SkillName + "/" + SourceFormClassifier.NativeSkillFileName),
                    Entry(SkillName + "/" + SourceFormClassifier.ReferencesFileName)),
                Source(Skill, SourceDocumentForm.Skill, SkillsCatalogue),
                Source(References, SourceDocumentForm.ReferencesEntrypoint, SkillsCatalogue),
            ],
        });

        Assert.Empty(result.Issues);
        Assert.Equal(new[] { SourceLogicalPath.WorkspaceEntryPath, mainRoot + "/" + WorkspaceLayoutNames.LoaderFileName,
            SkillsCatalogue, Skill, References }, result.Startup.Select(source => source.Path));
    }

    private static SourceGeneratedEntry Entry(string destination)
        => new(Description, destination, [SourceLoadingClosureResolver.LoadNowTag], new MarkdownTextSpan(0, 1));

    private static SourceLoadingClosureSource Source(string path, SourceDocumentForm form, string? parent, params SourceGeneratedEntry[] entries)
        => new()
        {
            Path = path, Form = form, ParentPath = parent, RouteState = SourceRouteState.Routed,
            Metadata = SourceAuthoredMetadataFacts.Complete(Description, [SourceLoadingClosureResolver.LoadNowTag]),
            GeneratedEntries = SourceGeneratedEntriesFacts.Complete(entries),
        };
}
