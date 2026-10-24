using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.References;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.References;

[Trait("Feature", "references"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class SourceLinkFolderDestinationTests
{
    [Theory(DisplayName = "Explicit missing folder destinations retain folder wording without acquiring bytes"),
        InlineData("missing/"), InlineData("missing%2F"), InlineData("missing/#heading")]
    public async Task MissingFolderUsesFolderWording(string destination)
    {
        var workspace = SourceInventoryTestData.Workspace();
        var catalogue = new SourceCatalogue(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [], [], [], isCancelled: false);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, path) => PhysicalPathResolution.Classified(PhysicalPathState.Missing, path),
            strictUtf8Reader: (_, _, _) => throw new InvalidOperationException("Missing folders have no bytes to acquire."),
            markdownParser: _ => throw new InvalidOperationException("Missing folders have no headings to parse."));

        var result = await resolver.ResolveAsync(new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = destination,
        }, TestContext.Current.CancellationToken);

        Assert.Equal(SourceLinkTargetResolution.Missing, result.Target.Resolution);
        Assert.Equal(".agents/missing", result.Target.Path);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetMissing, result.Finding?.Code);
        Assert.Equal("The local destination folder does not exist.", result.Finding?.Cause);
    }

    [Fact(DisplayName = "An explicit folder destination retains physical escape refusal before target acquisition")]
    public async Task FolderEscapeRetainsFileBoundary()
    {
        var workspace = SourceInventoryTestData.Workspace();
        var catalogue = new SourceCatalogue(WorkspaceLayout.Create(workspace, WorkspaceLayout.DefaultMainRoot), [], [], [], isCancelled: false);
        var resolver = new SourceLinkDestinationResolver(
            physicalPathResolver: (_, path) => PhysicalPathResolution.Classified(PhysicalPathState.External, path),
            strictUtf8Reader: (_, _, _) => throw new InvalidOperationException(),
            markdownParser: _ => throw new InvalidOperationException());

        var result = await resolver.ResolveAsync(new SourceLinkDestinationInput
        {
            Workspace = workspace,
            Catalogue = catalogue,
            SourceCanonicalPath = ".agents/source.md",
            RawDestination = "alias/",
        }, TestContext.Current.CancellationToken);

        Assert.Equal(SourceLinkTargetResolution.PhysicalEscape, result.Target.Resolution);
        Assert.Equal(SourceLinkDestinationFindingCode.TargetUnsafe, result.Finding?.Code);
    }
}
