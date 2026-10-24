using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Selection;
using OpenForge.Cli.Core.Framework.Sources.Selection;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Selection;

[Trait("Boundary", "Processing"), Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
public sealed class SourceUniverseLayoutTests
{
    private const string CustomRoot = "docs/forge";
    private const string ExcludedId = "excluded";
    private const string MissingName = "unresolved" + SourceFormClassifier.MarkdownExtension;
    private const string BlockedName = "blocked";

    [Theory(DisplayName = "Default scopes preserve unresolved candidates and unavailable directories in every content location")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public void DefaultAndExclusionOnlyPreserveEvidence(string mainRoot, bool exclude)
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, mainRoot);
        var path = mainRoot + "/" + ExcludedId + SourceFormClassifier.MarkdownExtension;
        var source = new SourceLogicalSource(new SourceLogicalIdentity(ExcludedId, path),
            new SourceLayer(path, Physical(path), SourceDocumentForm.Markdown, SourceLayerKind.Base));
        var candidates = layout.SourceRoots.Select(root => new SourceCandidate(root + "/" + MissingName,
            SourceDocumentForm.Markdown, null, PhysicalPathState.Missing, null, Physical(root))).ToArray();
        var issues = layout.SourceRoots.Select(root => new SourceCatalogueIssue(SourceCatalogueIssueCode.DirectoryUnavailable,
            root + "/" + BlockedName, [], Physical(root + "/" + BlockedName), null)).ToArray();
        var catalogue = new SourceCatalogue(layout, candidates, [source], issues, false);
        var scopes = layout.SourceRoots.Select(root => new SourceCatalogueSelectionScope(root, Physical(root))).ToArray();
        var request = new SourceUniverseFilterRequest(catalogue, scopes,
            exclude ? [new SourceUniverseSelectorOccurrence(SourceUniverseSelectorRole.Exclude, ExcludedId, 1)] : []);
        var resolver = new SourceUniverseFilterResolver(new SourceReferenceResolver((_, candidate) =>
            PhysicalPathResolution.Classified(PhysicalPathState.Missing, candidate)));

        var result = resolver.Resolve(request);

        Assert.True(result.IsResolved);
        Assert.Same(layout, request.Layout);
        Assert.Equal(catalogue.Candidates, result.Selection.Candidates);
        Assert.Equal(catalogue.Issues, result.Selection.Issues);
        Assert.Equal(exclude ? 0 : 1, result.Selection.Sources.Count);
        if (!exclude)
        {
            var all = catalogue.SelectAll();
            Assert.Equal(all.Sources, result.Selection.Sources);
            Assert.Equal(all.RootIssues, result.Selection.RootIssues);
        }
    }

    private static string Physical(string path) => Path.Combine(AppContext.BaseDirectory, path.Replace('/', Path.DirectorySeparatorChar));
}
