using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsOutsideRouteChangeTests
{
    private const string Folder = "project/team";

    [Theory(DisplayName = "Outside registration plans use the exact settings snapshot and converge without changing file registrations")]
    [InlineData(false), InlineData(true)]
    public void RegistrationAndDeregistration(bool present)
    {
        var path = Path.GetFullPath("settings-unit/outside-settings.json");
        var prior = WorkspaceSettingsRead.Absent(path);
        if (present) prior = Read(path, "{\"roots\":[\"project/note.md\"],\"custom\":true}"u8.ToArray());
        var original = prior.Snapshot!.Bytes;
        var added = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanOutsideRouteRegistration(prior, Folder));
        Assert.Equal(prior.Snapshot.Expectation, added.Expectation);
        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, added.Kind);
        var registered = Read(path, added.IntendedBytes.ToArray());
        Assert.Equal([Folder], registered.Document.OutsideRoutes);
        Assert.Equal(prior.Document.Roots, registered.Document.Roots);
        Assert.Null(WorkspaceSettingsChangePlanner.PlanOutsideRouteRegistration(registered, Folder));
        var removed = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanOutsideRouteDeregistration(registered, Folder));
        Assert.Equal(registered.Snapshot!.Expectation, removed.Expectation);
        var unregistered = Read(path, removed.IntendedBytes.ToArray());
        Assert.Empty(unregistered.Document.OutsideRoutes);
        Assert.Equal(registered.Document.Roots, unregistered.Document.Roots);
        Assert.Equal(registered.Document.SchemaVersion, unregistered.Document.SchemaVersion);
        Assert.Null(WorkspaceSettingsChangePlanner.PlanOutsideRouteDeregistration(unregistered, Folder));
        Assert.Equal(original, prior.Snapshot.Bytes);
    }

    [Theory(DisplayName = "Outside settings changes refuse unsafe observations")]
    [InlineData((int)WorkspaceSettingsReadState.Invalid), InlineData((int)WorkspaceSettingsReadState.Unavailable)]
    public void UnsafeObservation(int state)
    {
        var observation = WorkspaceSettingsRead.Absent(Path.GetFullPath("settings-unit/outside-settings.json")) with { State = (WorkspaceSettingsReadState)state };
        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanOutsideRouteRegistration(observation, Folder));
        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanOutsideRouteDeregistration(observation, Folder));
    }

    private static WorkspaceSettingsRead Read(string path, byte[] bytes)
        => WorkspaceSettingsRead.Absent(path) with
        {
            State = WorkspaceSettingsReadState.Complete,
            Document = WorkspaceSettingsCodec.Read(bytes).Document!,
            Snapshot = FileStateSnapshot.File(path, path, bytes),
        };
}
