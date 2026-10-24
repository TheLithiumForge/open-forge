using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Observation;

[Trait("Feature", "workspace-layout"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceLayoutReaderTests
{
    private const string CustomRoot = "docs/forge";

    [Theory(DisplayName = "A missing selected loader is a mismatch only when its removal is not recorded")]
    [InlineData(WorkspaceLayout.DefaultMainRoot, false), InlineData(WorkspaceLayout.DefaultMainRoot, true)]
    [InlineData(CustomRoot, false), InlineData(CustomRoot, true)]
    public void MissingLoaderRemoval(string root, bool removed)
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, root);
        var loader = root + "/" + WorkspaceLayoutNames.LoaderFileName;
        var settings = WorkspaceSettingsDocument.Empty with { RemovedFiles = removed ? [loader] : [] };
        Assert.Equal(!removed, WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.Missing));
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, false, FileExpectationKind.Missing));
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.File));
    }

    [Fact(DisplayName = "Loader removal follows selected-layout directory exclusions and ordinal paths")]
    public void DirectoryRemoval()
    {
        var workspace = new CliWorkspace(AppContext.BaseDirectory, AppContext.BaseDirectory, CliWorkspaceSelectionMethod.CurrentDirectory);
        var layout = WorkspaceLayout.Create(workspace, CustomRoot);
        var settings = WorkspaceSettingsDocument.Empty with { RemovedDirectories = [CustomRoot] };
        Assert.False(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout, settings, true, FileExpectationKind.Missing));
        Assert.True(WorkspaceLayoutReader.IsUnexpectedMissingLoader(layout,
            settings with { RemovedDirectories = [CustomRoot.ToUpperInvariant()] }, true, FileExpectationKind.Missing));
    }
}
