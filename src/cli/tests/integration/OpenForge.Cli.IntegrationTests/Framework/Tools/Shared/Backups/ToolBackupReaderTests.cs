using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Backups;

[Trait("Feature", "tool-backups"), Trait("Evidence", "Integration")]
public sealed class ToolBackupReaderTests
{
    [Theory(DisplayName = "Tool backup discovery needs neither valid current settings nor an existing target"), InlineData(false), InlineData(true)]
    public void FindsBackupWithoutValidTarget(bool missingTarget)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-discovery");
        if (!missingTarget) temporary.WriteText(WorkspaceSettingsDefinitions.RelativePath, "{");
        temporary.WriteBytes(ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath), [0, 127, 255]);
        var before = temporary.SnapshotHashes();
        var selected = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var result = Assert.Single(ToolBackupReader.Read(selected, [WorkspaceSettingsDefinitions.RelativePath], TestContext.Current.CancellationToken));
        Assert.True(result.SafePaths);
        Assert.Equal(temporary.Combine(WorkspaceSettingsDefinitions.RelativePath), result.Target);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool backup discovery ignores unselected files and leaves missing paths absent")]
    public void BoundsDiscovery()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-bounded");
        temporary.WriteText(ToolBackupPaths.Beside("unrelated.txt"), "saved");
        var before = temporary.SnapshotHashes();
        var selected = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Assert.Empty(ToolBackupReader.Read(selected, [WorkspaceSettingsDefinitions.RelativePath], TestContext.Current.CancellationToken));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Fact(DisplayName = "Tool backup discovery reports a file used as an ancestor as unavailable")]
    public void RejectsFileAncestor()
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-ancestor");
        var ancestor = Path.GetDirectoryName(WorkspaceSettingsDefinitions.RelativePath) ?? throw new InvalidOperationException();
        temporary.WriteBytes(ancestor, [1, 2, 3]);
        var before = temporary.SnapshotHashes();
        var selected = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var result = Assert.Single(ToolBackupReader.Read(selected, [WorkspaceSettingsDefinitions.RelativePath], TestContext.Current.CancellationToken));
        Assert.False(result.SafePaths);
        Assert.NotNull(result.Cause);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Tool backup discovery refuses directory and symbolic-link leaves without following them"), InlineData(false), InlineData(true)]
    public void RejectsUnsafeBackup(bool link)
    {
        using var temporary = TemporaryWorkspace.Create("tool-backup-unsafe");
        var path = ToolBackupPaths.Beside(WorkspaceSettingsDefinitions.RelativePath);
        if (link)
        {
            temporary.WriteText("outside.txt", "kept");
            Assert.True(temporary.TryCreateFileSymbolicLink(path, temporary.Combine("outside.txt"), out _));
        }
        else temporary.CreateDirectory(path);
        var selected = new CliWorkspace(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var result = Assert.Single(ToolBackupReader.Read(selected, [WorkspaceSettingsDefinitions.RelativePath], TestContext.Current.CancellationToken));
        Assert.False(result.SafePaths);
        Assert.NotNull(result.Cause);
        if (link) Assert.Equal(4, new FileInfo(temporary.Combine("outside.txt")).Length);
    }
}
