using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Shared.Selection;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.GeneratedNavigation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Reading;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.UnitTests.Commands.Index.Shared;

namespace OpenForge.Cli.Core.UnitTests.Commands.Index.Shared.Selection;

[Trait("Feature", "index-command"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class IndexOutsideSelectionTests
{
    private const string Directory = "docs/team";

    [Theory(DisplayName = "Only retained Loader rows admit registered roots to automatic and Loader Index closures")]
    [InlineData(false), InlineData(true)]
    public void ListingControlsAutomaticClosure(bool listed)
    {
        var layout = IndexTestData.Layout().WithOutsideRoutes([Directory]);
        var anchor = WorkspaceRoutePath.CanonicalEntrypointPath(Directory);
        var sources = new[] { Source(layout.LoaderPath, SourceDocumentForm.Loader), Source(anchor, SourceDocumentForm.CanonicalEntrypoint) };
        SourceIncludedEntryRow[] rows = listed
            ? [new(anchor, layout.LoaderPath, new SourceGeneratedEntry("Authored policy", anchor, [], new(0, 0)), PhysicalPathState.Contained)] : [];
        var catalogue = new SourceCatalogue(layout, [], sources, [], false, null, rows);
        var formation = new GeneratedNavigationFormationBuilder().Build(catalogue);
        var resolver = new IndexSelectionResolver(new SourceReferenceResolver((_, path) => PhysicalPathResolution.Classified(PhysicalPathState.Missing, path)));
        foreach (var references in new string[][] { [], [layout.LoaderPath] })
        {
            var resolution = resolver.Resolve(new(layout.Workspace, references, IndexMode.Apply), formation);
            Assert.True(resolution.IsComplete);
            Assert.Equal(listed ? new[] { layout.LoaderPath, anchor } : [layout.LoaderPath], resolution.Targets.Select(source => source.Identity.CanonicalBasePath));
        }
        var explicitSelection = resolver.Resolve(new(layout.Workspace, [SourceWorkspaceRelativePath.LiteralReferencePrefix + anchor], IndexMode.Apply), formation);
        Assert.True(explicitSelection.IsComplete);
        Assert.Equal(anchor, Assert.Single(explicitSelection.Targets).Identity.CanonicalBasePath);

        SourceLogicalSource Source(string path, SourceDocumentForm form)
            => new(new(SourceIdentity.DeriveId(layout, path) ?? throw new InvalidOperationException(), path),
                new(path, Path.Combine(layout.Workspace.PhysicalRoot, path.Replace('/', Path.DirectorySeparatorChar)), form, SourceLayerKind.Base));
    }
}
