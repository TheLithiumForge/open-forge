using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsChangePlannerTests
{
    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public void GrantUsesExactPriorStateAndPreservesUnknownAuthoredKeys(bool present)
    {
        const string original = "{ \"first\":{\"keep\":true}, \"allowInstallPaths\":[\"kept\"], \"last\":7 }\r\n";
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var observation = WorkspaceSettingsRead.Absent(path);
        if (present)
        {
            var bytes = Encoding.UTF8.GetBytes(original);
            observation = observation with { State = WorkspaceSettingsReadState.Complete, Document = WorkspaceSettingsCodec.Read(bytes).Document!, Snapshot = FileStateSnapshot.File(path, path, bytes) };
        }
        var change = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanGrant(observation, ["docs", "README.md"]));
        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(observation.Snapshot!.Expectation, change.Expectation);
        Assert.NotNull(WorkspaceSettingsChangePlanner.ReadActionAndRecovery(observation, change).RecoveryTarget);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        Assert.Contains(json.RootElement.GetProperty("allowInstallPaths").EnumerateArray(), entry => entry.GetString() == "docs");
        if (present)
        {
            Assert.Equal(["first", "allowInstallPaths", "last"], json.RootElement.EnumerateObject().Select(value => value.Name));
            Assert.True(json.RootElement.GetProperty("first").GetProperty("keep").GetBoolean());
            Assert.Equal(Encoding.UTF8.GetBytes(original), observation.Snapshot.Bytes);
        }
    }

    [Trait("Boundary", "Processing")]
    [Fact]
    public void AlreadyAdmittedGrantKeepsExactAuthoredBytes()
    {
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var bytes = "{ \"allowInstallPaths\": [\"docs\"], \"keep\": true }\r\n"u8.ToArray();
        var observation = WorkspaceSettingsRead.Absent(path) with { State = WorkspaceSettingsReadState.Complete, Document = WorkspaceSettingsCodec.Read(bytes).Document!, Snapshot = FileStateSnapshot.File(path, path, bytes) };
        Assert.Null(WorkspaceSettingsChangePlanner.PlanGrant(observation, ["docs/a.md"]));
    }

    [Trait("Boundary", "Processing")]
    [Theory]
    [InlineData((int)WorkspaceSettingsReadState.Invalid)]
    [InlineData((int)WorkspaceSettingsReadState.Unavailable)]
    public void UnsafeSettingsCannotBeReplacedByApproval(int state)
    {
        var observation = WorkspaceSettingsRead.Absent(Path.GetFullPath("settings-unit/open-forge.json")) with { State = (WorkspaceSettingsReadState)state };
        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanGrant(observation, ["docs"]));
    }
}
