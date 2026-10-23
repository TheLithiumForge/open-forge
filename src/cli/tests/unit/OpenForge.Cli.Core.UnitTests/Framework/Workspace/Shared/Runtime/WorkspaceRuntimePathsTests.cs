using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;

namespace OpenForge.Cli.Core.UnitTests.Framework.Workspace.Shared.Runtime;

[Trait("Feature", "workspace-runtime-reservation"), Trait("Evidence", "Unit")]
public sealed class WorkspaceRuntimePathsTests
{
    [Theory(DisplayName = "Runtime reservation ignores segment case and preserves directory boundaries")]
    [InlineData(".OPEN-FORGE-STATE", true)]
    [InlineData(".Open-Forge-State/notes.md", true)]
    [InlineData(".OPEN-forge-STATE/new/file.md", true)]
    [InlineData(".OPEN-FORGE-STATE/child/../notes.md", true)]
    [InlineData(".OPEN-FORGE-STATE/../notes.md", false)]
    [InlineData(".OPEN-FORGE-STATE-sibling/notes.md", false)]
    [InlineData("nested/.OPEN-FORGE-STATE/notes.md", false)]
    [InlineData(".", false)]
    public void ContainsReservesOnlyTheRootRuntimeSegment(string relative, bool reserved)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "runtime-case-workspace"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        Assert.Equal(reserved, WorkspaceRuntimePaths.Contains(workspace, Path.GetFullPath(relative, root)));
    }

    [Fact(DisplayName = "Case-independent runtime reservation also works at a volume root")]
    public void ContainsHandlesVolumeRoot()
    {
        var root = Path.GetPathRoot(Path.GetFullPath(Path.GetTempPath()));
        Assert.NotNull(root);
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);

        Assert.True(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(root, ".OPEN-FORGE-STATE", "new.md")));
        Assert.False(WorkspaceRuntimePaths.Contains(workspace, Path.Combine(root, ".OPEN-FORGE-STATE-sibling", "new.md")));
    }
}
