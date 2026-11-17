using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Observation;

public sealed class ToolWorkspaceObservationReaderTests
{
    private const string Id = "synthetic-tool";

    [Theory(DisplayName = "Project-only eligibility skips deferred Tool construction without declarations"),
     InlineData("absent"), InlineData("empty"), InlineData("legacy"), InlineData("empty-tools"), InlineData("unrelated"), InlineData("malformed-lock"),
     Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task NoDeclaration(string shape)
    {
        using var workspace = TemporaryWorkspace.Create("tool-project-eligibility");
        if (shape != "absent")
        {
            var settings = new JsonObject();
            if (shape == "legacy") settings[WorkspaceSettingsDefinitions.SchemaVersionProperty] = 5;
            if (shape == "empty-tools") settings[ToolRecordDefinitions.ToolsProperty] = new JsonObject();
            if (shape == "unrelated") settings["synthetic-future-setting"] = true;
            workspace.CreateFile(WorkspaceSettingsDefinitions.RelativePath, settings.ToJsonString());
        }
        if (shape == "malformed-lock") workspace.CreateFile(WorkspaceOwnershipDefinitions.RelativePath, "null");
        var before = workspace.SnapshotHashes(includeRuntime: true);
        var calls = 0;
        var reader = Reader(() => calls++);
        Assert.Empty(await reader.ReadAsync(workspace.Path, WorkspaceSettingsDefinitions.DirectoryName, TestContext.Current.CancellationToken));
        Assert.Equal(0, calls);
        Assert.Equal(before, workspace.SnapshotHashes(includeRuntime: true));
    }

    [Fact(DisplayName = "A declared tool enters the same deferred observation exactly once"), Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task DeclaredProject()
    {
        using var workspace = TemporaryWorkspace.Create("tool-project-declared");
        var codec = new ToolPortableCodec(new ToolVersionService());
        workspace.CreateFile(WorkspaceSettingsDefinitions.RelativePath,
            codec.WriteSettings(new JsonObject(), new Dictionary<string, ToolSetting> { [Id] = new(new(3, 2)) }).ToJsonString());
        var calls = 0;
        await Assert.ThrowsAsync<IOException>(() => Reader(() => calls++).ReadAsync(workspace.Path,
            WorkspaceSettingsDefinitions.DirectoryName, TestContext.Current.CancellationToken));
        Assert.Equal(1, calls);
    }

    [Fact(DisplayName = "Invalid project settings fail before deferred Tool construction"), Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task InvalidProject()
    {
        using var workspace = TemporaryWorkspace.Create("tool-project-invalid");
        workspace.CreateFile(WorkspaceSettingsDefinitions.RelativePath, "[]");
        var calls = 0;
        await Assert.ThrowsAsync<IOException>(() => Reader(() => calls++).ReadAsync(workspace.Path,
            WorkspaceSettingsDefinitions.DirectoryName, TestContext.Current.CancellationToken));
        Assert.Equal(0, calls);
    }

    [Theory(DisplayName = "The existing portable codec rejects malformed Tool declarations before construction"), InlineData(false), InlineData(true),
     Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task InvalidDeclaration(bool invalidRange)
    {
        using var workspace = TemporaryWorkspace.Create("tool-declaration-invalid");
        var settings = new JsonObject
        {
            [ToolRecordDefinitions.ToolsProperty] = invalidRange
                ? new JsonObject { [Id] = new JsonObject { [ToolRecordDefinitions.VersionProperty] = "latest" } }
                : null,
        };
        workspace.CreateFile(WorkspaceSettingsDefinitions.RelativePath, settings.ToJsonString());
        var calls = 0;
        await Assert.ThrowsAnyAsync<ArgumentException>(() => Reader(() => calls++).ReadAsync(workspace.Path,
            WorkspaceSettingsDefinitions.DirectoryName, TestContext.Current.CancellationToken));
        Assert.Equal(0, calls);
    }

    [Fact(DisplayName = "Cancellation stops project eligibility before Tool construction"), Trait("Feature", "tool-read-isolation"), Trait("Evidence", "Integration")]
    public async Task Cancelled()
    {
        using var workspace = TemporaryWorkspace.Create("tool-project-cancelled");
        using var cancellation = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        cancellation.Cancel();
        var calls = 0;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => Reader(() => calls++).ReadAsync(workspace.Path,
            WorkspaceSettingsDefinitions.DirectoryName, cancellation.Token));
        Assert.Equal(0, calls);
    }

    private static ToolWorkspaceObservationReader Reader(Action constructed) => new(new ToolVersionService(), () =>
    {
        constructed();
        throw new IOException("The declared Tool observation factory was entered.");
    });
}
