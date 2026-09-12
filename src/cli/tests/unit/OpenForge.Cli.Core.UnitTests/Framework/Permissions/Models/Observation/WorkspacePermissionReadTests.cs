using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Permissions.Models.Document;
using OpenForge.Cli.Core.Framework.Permissions.Models.Observation;

namespace OpenForge.Cli.Core.UnitTests.Framework.Permissions.Models.Observation;

[Trait("Feature", "workspace-permissions"), Trait("Evidence", "Unit")]
public sealed class WorkspacePermissionReadTests
{
    [Theory(DisplayName = "Permission observations match independently captured equal file and missing snapshots")]
    [InlineData(false), InlineData(true)]
    public void EqualSnapshotFactsMatch(bool present)
    {
        var path = Path.GetFullPath("permission-unit/permissions.json");
        var expected = present ? FileStateSnapshot.File(path, path, [1, 2]) : FileStateSnapshot.Missing(path);
        var actual = present ? FileStateSnapshot.File(path, path, [1, 2]) : FileStateSnapshot.Missing(path);
        var state = present ? WorkspacePermissionReadState.Complete : WorkspacePermissionReadState.Missing;
        var before = new WorkspacePermissionRead(state, Document: null, expected, Cause: null);
        var current = new WorkspacePermissionRead(state, Document: null, actual, Cause: null);

        Assert.NotSame(expected, actual);
        Assert.True(before.MatchesObservation(current));
    }

    [Fact(DisplayName = "Permission observation comparison ignores decoded documents and diagnostic causes")]
    public void DocumentAndCauseDoNotChangeSnapshotIdentity()
    {
        var path = Path.GetFullPath("permission-unit/permissions.json");
        var snapshot = FileStateSnapshot.File(path, path, [1, 2]);
        var before = new WorkspacePermissionRead(WorkspacePermissionReadState.Complete, WorkspacePermissionDocument.Empty, snapshot, Cause: null);
        var differentDocument = new WorkspacePermissionDocument([new("team", [".apm/a.md"])], []);

        Assert.True(before.MatchesObservation(before with { Document = differentDocument }));
        Assert.True(before.MatchesObservation(before with { Cause = "different diagnostic" }));
        Assert.True(before.MatchesObservation(before with { Document = null, Cause = "different diagnostic" }));
    }

    [Theory(DisplayName = "Permission observation comparison requires both snapshots")]
    [InlineData(false, false), InlineData(false, true), InlineData(true, false)]
    public void AbsentSnapshotsDoNotMatch(bool hasExpected, bool hasActual)
    {
        var snapshot = FileStateSnapshot.Missing(Path.GetFullPath("permission-unit/permissions.json"));
        var before = new WorkspacePermissionRead(WorkspacePermissionReadState.Missing, Document: null, hasExpected ? snapshot : null, Cause: null);
        var current = new WorkspacePermissionRead(WorkspacePermissionReadState.Missing, Document: null, hasActual ? snapshot : null, Cause: null);

        Assert.False(before.MatchesObservation(current));
    }

    [Fact(DisplayName = "Permission observation comparison rejects changed state, path expectation and file content")]
    public void ChangedObservationFactsDoNotMatch()
    {
        var path = Path.GetFullPath("permission-unit/permissions.json");
        var otherPath = Path.GetFullPath("permission-unit/other.json");
        var before = new WorkspacePermissionRead(WorkspacePermissionReadState.Complete, Document: null,
            FileStateSnapshot.File(path, path, [1, 2]), Cause: null);

        Assert.False(before.MatchesObservation(before with { State = WorkspacePermissionReadState.Invalid }));
        Assert.False(before.MatchesObservation(before with { Snapshot = FileStateSnapshot.File(otherPath, otherPath, [1, 2]) }));
        Assert.False(before.MatchesObservation(before with { Snapshot = FileStateSnapshot.File(path, path, [1, 3]) }));
    }
}
