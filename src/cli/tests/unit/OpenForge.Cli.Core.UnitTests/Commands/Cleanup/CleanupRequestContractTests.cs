using OpenForge.Cli.Core.Commands.Cleanup;
using OpenForge.Cli.Core.Commands.Cleanup.Models.Request;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Cleanup;

public sealed class CleanupRequestContractTests
{
    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Cleanup request preserves the exact workspace and apply or dry-run mode"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void RequestPreservesWorkspaceAndMode()
    {
        var workspace = CleanupTestData.Workspace("request");
        var apply = new CleanupRequest(workspace, CleanupMode.Apply);
        var dryRun = new CleanupRequest(workspace, CleanupMode.DryRun);

        Assert.Same(workspace, apply.Workspace);
        Assert.Equal(CleanupMode.Apply, apply.Mode);
        Assert.False(apply.IsDryRun);
        Assert.Same(workspace, dryRun.Workspace);
        Assert.Equal(CleanupMode.DryRun, dryRun.Mode);
        Assert.True(dryRun.IsDryRun);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Cleanup request rejects null workspaces and undefined modes"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void RequestFailsClosedAtItsBoundary()
    {
        var workspace = CleanupTestData.Workspace("request-validation");

        Assert.Throws<ArgumentNullException>(() =>
            new CleanupRequest(null!, CleanupMode.Apply));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            new CleanupRequest(workspace, (CleanupMode)int.MaxValue));
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Cleanup request keeps selection to the exact supplied workspace without a selector"),
     Trait("Feature", "cleanup"), Trait("Evidence", "UnitContract")]
    public void RequestKeepsExactWorkspaceSelection()
    {
        var workspace = new CliWorkspace(
            Path.Combine("relative", "cleanup-workspace"),
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "cleanup-physical-workspace")),
            CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var request = new CleanupRequest(workspace, CleanupMode.Apply);

        Assert.Same(workspace, request.Workspace);
        Assert.Equal(CliWorkspaceSelectionMethod.ExplicitWorkspace, request.Workspace.SelectedBy);
        Assert.Equal(Path.GetFullPath(Path.Combine("relative", "cleanup-workspace")), request.Workspace.LexicalRoot);
        Assert.Equal(
            Path.GetFullPath(Path.Combine(Path.GetTempPath(), "cleanup-physical-workspace")),
            request.Workspace.PhysicalRoot);
    }
}
