using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.UnitTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Unit")]
public sealed class WorkspaceSettingsConfigurationTests
{
    private const string Foreign = "foreign";
    private const string Keep = "keep";
    private const string OldFile = "docs/old.md";
    private const string NewFile = "docs/new.md";
    private const string KeptFile = "docs/keep.md";

    [Theory(DisplayName = "Configuration combines removals and the frontmatter preference in one settings effect"), Trait("Boundary", "Processing")]
    [InlineData(false), InlineData(true)]
    public void ConfigurationCombinesRemovalsAndFormInOneEffect(bool present)
    {
        var path = Path.GetFullPath("settings-unit/" + WorkspaceSettingsDefinitions.FileName);
        var observation = WorkspaceSettingsRead.Absent(path);
        if (present)
        {
            var bytes = Encoding.UTF8.GetBytes($$$"""{"{{{Foreign}}}":{"{{{Keep}}}":true},"{{{WorkspaceSettingsDefinitions.FrontmatterProperty}}}":"{{{WorkspaceSettingsDefinitions.FrontmatterScopedName}}}","{{{WorkspaceSettingsDefinitions.RemovedFilesProperty}}}":["{{{OldFile}}}","{{{KeptFile}}}"]}""").ToArray();
            observation = observation with
            {
                State = WorkspaceSettingsReadState.Complete,
                Document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document),
                Snapshot = FileStateSnapshot.File(path, path, bytes),
            };
        }

        var change = Assert.IsType<PlannedFileChange>(WorkspaceSettingsChangePlanner.PlanConfiguration(
            observation,
            additions: new WorkspaceRemovalSelection { Files = [NewFile] },
            clear: new WorkspaceRemovalSelection { Files = [OldFile] },
            frontmatter: FrontmatterForm.Root));

        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(observation.Snapshot!.Expectation, change.Expectation);
        var document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(change.IntendedBytes.AsMemory()).Document);
        Assert.Equal(FrontmatterForm.Root, document.DeclaredFrontmatter);
        string[] expectedFiles = present ? [KeptFile, NewFile] : [NewFile];
        Assert.Equal(expectedFiles, document.RemovedFiles);
        using var json = JsonDocument.Parse(change.IntendedBytes.AsMemory());
        if (present)
        {
            Assert.True(json.RootElement.GetProperty(Foreign).GetProperty(Keep).GetBoolean());
            Assert.Equal([Foreign, WorkspaceSettingsDefinitions.FrontmatterProperty, WorkspaceSettingsDefinitions.RemovedFilesProperty], json.RootElement.EnumerateObject().Select(property => property.Name));
        }
        else
        {
            Assert.Equal(WorkspaceSettingsDefinitions.SchemaVersion, json.RootElement.GetProperty(WorkspaceSettingsDefinitions.SchemaVersionProperty).GetInt32());
        }
    }

    [Fact(DisplayName = "Configuration with no requested settings changes retains authored bytes"), Trait("Boundary", "Processing")]
    public void ConfigurationWithoutFrontmatterChangePreservesExistingBehavior()
    {
        var path = Path.GetFullPath("settings-unit/" + WorkspaceSettingsDefinitions.FileName);
        var bytes = Encoding.UTF8.GetBytes($"{{ /* keep */ \"{WorkspaceSettingsDefinitions.FrontmatterProperty}\":\"{WorkspaceSettingsDefinitions.FrontmatterRootName}\", \"{Foreign}\":true }}\r\n").ToArray();
        var observation = WorkspaceSettingsRead.Absent(path) with
        {
            State = WorkspaceSettingsReadState.Complete,
            Document = Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(bytes).Document),
            Snapshot = FileStateSnapshot.File(path, path, bytes),
        };

        Assert.Null(WorkspaceSettingsChangePlanner.PlanConfiguration(observation, new(), new(), frontmatter: null));
        Assert.Null(WorkspaceSettingsChangePlanner.PlanConfiguration(observation, new(), new(), frontmatter: FrontmatterForm.Root));
        Assert.Null(WorkspaceSettingsChangePlanner.PlanConfiguration(WorkspaceSettingsRead.Absent(path), new(), new(), frontmatter: null));
    }

    [Theory(DisplayName = "A form-only configuration explicitly authors scoped when the key is missing"), Trait("Boundary", "Processing")]
    [InlineData(false), InlineData(true)]
    public void FormOnlyConfigurationAuthorsMissingScopedKey(bool present)
    {
        var path = Path.GetFullPath("settings-unit/" + WorkspaceSettingsDefinitions.FileName);
        var observation = WorkspaceSettingsRead.Absent(path);
        if (present)
        {
            observation = observation with
            {
                State = WorkspaceSettingsReadState.Complete,
                Snapshot = FileStateSnapshot.File(path, path, "{}"u8.ToArray()),
            };
        }

        var change = Assert.IsType<PlannedFileChange>(
            WorkspaceSettingsChangePlanner.PlanConfiguration(observation, new(), new(), frontmatter: FrontmatterForm.Scoped));

        Assert.Equal(present ? PlannedFileChangeKind.Replace : PlannedFileChangeKind.Create, change.Kind);
        Assert.Equal(FrontmatterForm.Scoped,
            Assert.IsType<WorkspaceSettingsDocument>(WorkspaceSettingsCodec.Read(change.IntendedBytes.AsMemory()).Document).DeclaredFrontmatter);
    }
}
