using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
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

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Removal plans retain exact prior snapshots for recovery")]
    [InlineData(false), InlineData(true)]
    public void PlanRemovalsCarriesExactRecoveryInputs(bool present)
    {
        const string original = "{ \"future\": {\"keep\": true}, \"removedFiles\": [\"AGENTS.md\"] }\r\n";
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var observation = WorkspaceSettingsRead.Absent(path);
        if (present)
        {
            var bytes = Encoding.UTF8.GetBytes(original);
            observation = observation with
            {
                State = WorkspaceSettingsReadState.Complete,
                Document = WorkspaceSettingsCodec.Read(bytes).Document!,
                Snapshot = FileStateSnapshot.File(path, path, bytes),
            };
        }

        var snapshot = observation.Snapshot!;
        var beforeBytes = snapshot.Bytes.ToArray();
        var selection = new WorkspaceRemovalSelection
        {
            Files = ["docs/old.md"],
            Directories = ["docs/archive"],
            Extensions = ["planning"],
            Libraries = ["team-knowledge"],
        };
        var change = Assert.IsType<PlannedFileChange>(
            WorkspaceSettingsChangePlanner.PlanRemovals(observation, selection));

        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        Assert.Equal(beforeBytes, snapshot.Bytes);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        string[] expectedFiles = present ? ["AGENTS.md", "docs/old.md"] : ["docs/old.md"];
        Assert.Equal(expectedFiles, json.RootElement.GetProperty("removedFiles").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["docs/archive"], json.RootElement.GetProperty("removedDirectories").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["planning"], json.RootElement.GetProperty("removedExtensions").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["team-knowledge"], json.RootElement.GetProperty("removedLibraries").EnumerateArray().Select(entry => entry.GetString()));

        var recovery = change.Kind == PlannedFileChangeKind.Create
            ? RecoveryBundleTarget.CreateReversible(change, snapshot)
            : RecoveryBundleTarget.Create(change, snapshot);
        Assert.Same(change, recovery.Change);
        Assert.Equal(beforeBytes, recovery.Before.Bytes);
        Assert.True(recovery.RequiresRecovery);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "An unchanged removal selection produces no planned change")]
    public void PlanRemovalsReturnsNullWhenAlreadyRecorded()
    {
        var path = Path.GetFullPath("settings-unit/open-forge.json");
        var bytes = "{\"removedDirectories\":[\"docs/archive\"]}"u8.ToArray();
        var observation = WorkspaceSettingsRead.Absent(path) with
        {
            State = WorkspaceSettingsReadState.Complete,
            Document = WorkspaceSettingsCodec.Read(bytes).Document!,
            Snapshot = FileStateSnapshot.File(path, path, bytes),
        };

        Assert.Null(WorkspaceSettingsChangePlanner.PlanRemovals(
            observation,
            new WorkspaceRemovalSelection { Directories = ["docs/archive"] }));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Removal planning refuses unsafe settings observations")]
    [InlineData((int)WorkspaceSettingsReadState.Invalid)]
    [InlineData((int)WorkspaceSettingsReadState.Unavailable)]
    public void PlanRemovalsRequiresSafeObservation(int state)
    {
        var observation = WorkspaceSettingsRead.Absent(Path.GetFullPath("settings-unit/open-forge.json")) with
        {
            State = (WorkspaceSettingsReadState)state,
        };

        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanRemovals(
            observation,
            new WorkspaceRemovalSelection { Files = ["docs/old.md"] }));
    }
}
