using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Integration")]
public sealed class WorkspaceSettingsRemovalPlanIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A removal replacement plan retains exact recovery inputs without writing")]
    public static async Task PlansReplacementFromObservedSettingsWithoutApplyingIt()
    {
        using var temporary = TemporaryWorkspace.Create("settings-removal-plan-existing");
        const string original = "{\"future\":{\"keep\":true},\"removedFiles\":[\"AGENTS.md\"]}";
        temporary.CreateDirectory(".agents");
        temporary.CreateFile(".agents/open-forge.json", original);
        var before = temporary.SnapshotHashes();
        var observation = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(),
            Workspace(temporary),
            TestContext.Current.CancellationToken);
        var selection = new WorkspaceRemovalSelection
        {
            Files = ["docs/old.md"],
            Directories = ["docs/archive"],
            Extensions = ["planning"],
            Libraries = ["team-knowledge"],
        };

        var change = Assert.IsType<PlannedFileChange>(
            WorkspaceSettingsChangePlanner.PlanRemovals(observation, selection));
        var snapshot = Assert.IsType<FileStateSnapshot>(observation.Snapshot);
        Assert.Equal(WorkspaceSettingsReadState.Complete, observation.State);
        Assert.Equal(FileExpectationKind.File, snapshot.Kind);
        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        var recovery = RecoveryBundleTarget.Create(change, snapshot);
        Assert.Same(change, recovery.Change);
        Assert.Equal(
            original,
            await File.ReadAllTextAsync(
                temporary.Combine(".agents/open-forge.json"),
                TestContext.Current.CancellationToken));
        Assert.Equal(before, temporary.SnapshotHashes());

        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        Assert.True(json.RootElement.GetProperty("future").GetProperty("keep").GetBoolean());
        Assert.Equal(["AGENTS.md", "docs/old.md"],
            json.RootElement.GetProperty("removedFiles").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["docs/archive"],
            json.RootElement.GetProperty("removedDirectories").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["planning"],
            json.RootElement.GetProperty("removedExtensions").EnumerateArray().Select(entry => entry.GetString()));
        Assert.Equal(["team-knowledge"],
            json.RootElement.GetProperty("removedLibraries").EnumerateArray().Select(entry => entry.GetString()));
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A removal create plan retains proven absence without creating settings")]
    public static async Task PlansCreateFromAbsentSettingsWithoutApplyingIt()
    {
        using var temporary = TemporaryWorkspace.Create("settings-removal-plan-absent");
        var before = temporary.SnapshotHashes();
        var observation = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(),
            Workspace(temporary),
            TestContext.Current.CancellationToken);

        var change = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanRemovals(
            observation,
            new WorkspaceRemovalSelection { Categories = ["templates"] }));
        var snapshot = Assert.IsType<FileStateSnapshot>(observation.Snapshot);
        Assert.Equal(WorkspaceSettingsReadState.Absent, observation.State);
        Assert.Equal(FileExpectationKind.Missing, snapshot.Kind);
        Assert.Equal(PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        var recovery = RecoveryBundleTarget.CreateReversible(change, snapshot);
        Assert.Same(change, recovery.Change);
        Assert.False(Directory.Exists(temporary.Combine(".agents")));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "A malformed removal settings observation cannot be planned over")]
    public static async Task InvalidSettingsCannotProduceAPlan()
    {
        using var temporary = TemporaryWorkspace.Create("settings-removal-plan-invalid");
        temporary.CreateDirectory(".agents");
        temporary.CreateFile(".agents/open-forge.json", "{ broken");
        var before = temporary.SnapshotHashes();
        var observation = await WorkspaceSettingsReader.ReadAsync(
            new PhysicalPathResolver(),
            Workspace(temporary),
            TestContext.Current.CancellationToken);

        Assert.Equal(WorkspaceSettingsReadState.Invalid, observation.State);
        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanRemovals(
            observation,
            new WorkspaceRemovalSelection { Files = ["docs/old.md"] }));
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
