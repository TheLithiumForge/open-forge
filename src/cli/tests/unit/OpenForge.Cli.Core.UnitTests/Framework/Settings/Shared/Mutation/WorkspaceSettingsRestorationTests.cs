using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class WorkspaceSettingsRestorationTests
{
    [Fact(DisplayName = "Restoration clears exact removal values while retaining descendants, unrelated values and unknown authored keys")]
    public void ClearsOnlyExactSelectedValues()
    {
        var bytes = Encoding.UTF8.GetBytes("""
            {"first":{"keep":true},"removedCategories":["skills","patterns"],
             "removedDirectories":[".agents/skills",".agents/skills/private"],
             "removedFiles":[".agents/skills/_skills.md",".agents/skills/open-forge-cli/SKILL.md"],
             "removedExtensions":["planning"],"last":42}
            """);
        var path = Path.GetFullPath("restoration-unit/open-forge.json");
        var document = WorkspaceSettingsCodec.Read(bytes).Document;
        Assert.NotNull(document);
        var snapshot = FileStateSnapshot.File(path, path, bytes);
        var observation = new WorkspaceSettingsRead(WorkspaceSettingsReadState.Complete, document, path, null)
        { Snapshot = snapshot };
        var change = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanRestoration(observation,
            new WorkspaceRemovalSelection { Categories = ["skills"], Directories = [".agents/skills"], Files = [".agents/skills/_skills.md"] }));

        Assert.Equal(PlannedFileChangeKind.Replace, change.Kind);
        Assert.Equal(snapshot.Expectation, change.Expectation);
        Assert.Equal(bytes, snapshot.Bytes);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        Assert.Equal(["first", "removedCategories", "removedDirectories", "removedFiles", "removedExtensions", "last"],
            json.RootElement.EnumerateObject().Select(property => property.Name));
        Assert.True(json.RootElement.GetProperty("first").GetProperty("keep").GetBoolean());
        Assert.Equal(42, json.RootElement.GetProperty("last").GetInt32());
        Assert.Equal(["patterns"], json.RootElement.GetProperty("removedCategories").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/skills/private"], json.RootElement.GetProperty("removedDirectories").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal([".agents/skills/open-forge-cli/SKILL.md"], json.RootElement.GetProperty("removedFiles").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(["planning"], json.RootElement.GetProperty("removedExtensions").EnumerateArray().Select(value => value.GetString()));
    }

    [Fact(DisplayName = "Restoration of absent or unmatched exclusions preserves settings without creating a file")]
    public void NoMatchingExclusionsHaveNoEffect()
    {
        var path = Path.GetFullPath("restoration-unit/open-forge.json");
        var selection = new WorkspaceRemovalSelection { Categories = ["skills"] };
        Assert.Null(WorkspaceSettingsChangePlanner.PlanRestoration(WorkspaceSettingsRead.Absent(path), selection));
        Assert.Null(WorkspaceSettingsCodec.ClearRemovals("{\"future\":true}"u8.ToArray(), selection));
    }

    [Theory(DisplayName = "Restoration refuses malformed and unavailable settings observations")]
    [InlineData((int)WorkspaceSettingsReadState.Invalid), InlineData((int)WorkspaceSettingsReadState.Unavailable)]
    public void UnsafeSettingsAreRejected(int state)
    {
        var observation = WorkspaceSettingsRead.Absent(Path.GetFullPath("restoration-unit/open-forge.json")) with { State = (WorkspaceSettingsReadState)state };
        Assert.Throws<InvalidOperationException>(() => WorkspaceSettingsChangePlanner.PlanRestoration(observation,
            new WorkspaceRemovalSelection { Categories = ["skills"] }));
    }
}
