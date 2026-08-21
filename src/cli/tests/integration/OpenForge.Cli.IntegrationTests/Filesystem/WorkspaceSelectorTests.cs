using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Invocation;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Filesystem;

public sealed class WorkspaceSelectorTests
{
    [Fact(DisplayName = "Workspace selector normalizes current and explicit roots")]
    [Trait("Feature", "cli-workspace"), Trait("Evidence", "Integration")]
    public void SelectorNormalizesCurrentAndExplicitRoots()
    {
        using var temporary = TemporaryWorkspace.Create("workspace-selection");
        var workspace = temporary.CreateDirectory("workspace");
        var selector = new CliWorkspaceSelector(new PhysicalPathResolver());

        var current = selector.Select(new CliWorkspaceRequest(null, workspace));
        var explicitSelection = selector.Select(new CliWorkspaceRequest("workspace", temporary.Path));

        Assert.Equal(CliWorkspaceSelectionState.Selected, current.State);
        Assert.Equal(CliWorkspaceSelectionMethod.CurrentDirectory, current.Workspace?.SelectedBy);
        Assert.Equal(Path.GetFullPath(workspace), current.Workspace?.LexicalRoot);
        Assert.Equal(CliWorkspaceSelectionState.Selected, explicitSelection.State);
        Assert.Equal(CliWorkspaceSelectionMethod.ExplicitWorkspace, explicitSelection.Workspace?.SelectedBy);
        Assert.Equal(Path.GetFullPath(workspace), explicitSelection.Workspace?.PhysicalRoot);
    }

    [Fact(DisplayName = "Workspace selector preserves lexical root and resolves root aliases")]
    [Trait("Feature", "cli-workspace"), Trait("Evidence", "Integration")]
    public void SelectorPreservesLexicalRootAndResolvesAlias()
    {
        using var temporary = TemporaryWorkspace.Create("workspace-root-alias");
        var workspace = temporary.CreateDirectory("workspace");
        var alias = temporary.CreateDirectorySymbolicLink("alias", workspace);

        var result = new CliWorkspaceSelector(new PhysicalPathResolver()).Select(
            new CliWorkspaceRequest(alias, temporary.Path));

        Assert.Equal(CliWorkspaceSelectionState.Selected, result.State);
        Assert.Equal(Path.GetFullPath(alias), result.Workspace?.LexicalRoot);
        Assert.Equal(Path.GetFullPath(workspace), result.Workspace?.PhysicalRoot);
    }

    [Fact(DisplayName = "Workspace selector returns typed missing state without creating paths")]
    [Trait("Feature", "cli-workspace"), Trait("Evidence", "Integration")]
    public void SelectorReturnsTypedMissingStateWithoutWrites()
    {
        using var temporary = TemporaryWorkspace.Create("workspace-missing");
        var missing = temporary.Combine("missing");
        var before = temporary.SnapshotHashes();

        var result = new CliWorkspaceSelector(new PhysicalPathResolver()).Select(
            new CliWorkspaceRequest(missing, temporary.Path));

        Assert.Equal(CliWorkspaceSelectionState.Missing, result.State);
        Assert.Null(result.Workspace);
        Assert.False(Directory.Exists(missing));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Workspace selector classifies an existing file as invalid")]
    [Trait("Feature", "cli-workspace"), Trait("Evidence", "Integration")]
    public void SelectorClassifiesExistingFileAsInvalid()
    {
        using var temporary = TemporaryWorkspace.Create("workspace-file");
        var file = temporary.CreateFile("workspace.txt", "not a directory");

        var result = new CliWorkspaceSelector(new PhysicalPathResolver()).Select(
            new CliWorkspaceRequest(file, temporary.Path));

        Assert.Equal(CliWorkspaceSelectionState.Invalid, result.State);
        Assert.Equal(FilesystemFailureKind.InvalidPath, result.Failure?.Kind);
        Assert.Null(result.Workspace);
        Assert.True(File.Exists(file));
    }
}
