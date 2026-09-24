using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Models.Observation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsReadTests
{
    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public void EqualObservationsMatchIndependentlyCapturedSnapshots(bool present)
    {
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var before = WorkspaceSettingsRead.Absent(path);
        var current = WorkspaceSettingsRead.Absent(path);
        if (present)
        {
            before = before with { State = WorkspaceSettingsReadState.Complete, Snapshot = FileStateSnapshot.File(path, path, [1, 2]) };
            current = current with { State = WorkspaceSettingsReadState.Complete, Snapshot = FileStateSnapshot.File(path, path, [1, 2]) };
        }
        Assert.True(before.MatchesObservation(current));
        Assert.True(before.MatchesObservation(current with { Cause = "diagnostic" }));
    }

    [Trait("Boundary", "Processing")]
    [Fact]
    public void ChangedStateBytesExpectationOrAdmissionCannotMatch()
    {
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var before = WorkspaceSettingsRead.Absent(path) with { State = WorkspaceSettingsReadState.Complete, Snapshot = FileStateSnapshot.File(path, path, [1, 2]) };
        Assert.False(before.MatchesObservation(before with { State = WorkspaceSettingsReadState.Invalid }));
        Assert.False(before.MatchesObservation(before with { Snapshot = null }));
        Assert.False(before.MatchesObservation(before with { Snapshot = FileStateSnapshot.File(path, path, [1, 3]) }));
        Assert.False(before.MatchesObservation(before with { Snapshot = FileStateSnapshot.File(path + ".other", path + ".other", [1, 2]) }));
        Assert.False(before.MatchesObservation(before with { Document = before.Document with { AllowInstallPaths = ["docs"] } }));
        Assert.False(before.MatchesObservation(before with { Document = before.Document with { RemovedDirectories = ["docs"] } }));
        Assert.False(before.MatchesObservation(before with { Document = before.Document with { RemovedExtensions = ["planning"] } }));
        Assert.False(before.MatchesObservation(before with { Document = before.Document with { RemovedLibraries = ["team"] } }));
    }
}
