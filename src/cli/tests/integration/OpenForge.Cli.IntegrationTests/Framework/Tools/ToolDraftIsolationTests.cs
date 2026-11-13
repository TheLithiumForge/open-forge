using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools;

[Trait("Feature", "tool-draft"), Trait("Evidence", "Integration"), Trait("Boundary", "Filesystem")]
public sealed class ToolDraftIsolationTests
{
    [Theory(DisplayName = "Packaged review drafts cannot become a local catalogue source or create state")]
    [InlineData(ToolDraftSource.CodeGraph)]
    public async Task DraftSourcesCannotActivate(string pin)
    {
        using var workspace = TemporaryWorkspace.Create("tool-draft-source");
        var path = pin + ToolDraftSource.DefinitionExtension;
        workspace.WriteBytes(path, ToolDraftSource.ReadDraft(typeof(ToolCatalogueReader).Assembly, pin));
        var before = workspace.SnapshotHashes();
        var reader = new ToolCatalogueReader();
        var draft = await reader.ReadAsync(workspace.Combine(path), TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Invalid, draft.State);
        Assert.Empty(draft.Definitions);
        var bundled = await reader.ReadAsync(null, TestContext.Current.CancellationToken);
        Assert.Equal(ToolSourceState.Complete, bundled.State);
        Assert.Empty(bundled.Definitions);
        Assert.DoesNotContain(typeof(ToolCatalogueReader).Assembly.GetManifestResourceNames(),
            name => name.Contains(ToolDraftSource.CodebaseMemory, StringComparison.Ordinal));
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
