using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Records;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Writing;

[Trait("Feature", "tool-writers"), Trait("Evidence", "Integration")]
public sealed class ToolWriterPermissionsTests
{
    private const string Output = "host/config.json";
    private const string Parent = "host";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "A declared native output requires a destination grant and preview doesn't save it")]
    public async Task RequiresGrant()
    {
        using var fixture = new ToolRecordFixture();
        var state = await ToolWriterReader.ReadAsync(fixture.Workspace, Token);
        var before = fixture.Temporary.SnapshotHashes();
        var refused = ToolWriterPermissions.Check(state, [Output], [], Token);
        Assert.False(Assert.Single(refused.Decisions).Allowed);
        Assert.Equal(ToolWriteIssueCode.PermissionMissing, Assert.Single(refused.Issues).Code);
        var allowed = ToolWriterPermissions.Check(state, [Output], [Parent, "unused"], Token);
        Assert.Empty(allowed.Issues);
        var decision = Assert.Single(allowed.Decisions);
        Assert.True(decision.Allowed);
        Assert.True(decision.SaveGrant);
        Assert.Equal(Parent, Assert.Single(allowed.Grants));
        Assert.Equal(before, fixture.Temporary.SnapshotHashes());
    }

    [Theory(DisplayName = "Exclusions block native outputs and their future descendants despite covering grants")]
    [InlineData(false), InlineData(true)]
    public async Task HonorsExclusions(bool ancestor)
    {
        using var fixture = new ToolRecordFixture();
        var state = await ToolWriterReader.ReadAsync(fixture.Workspace, Token);
        state = state with
        {
            Settings = state.Settings with { Document = state.Settings.Document with { RemovedFiles = [Output] } },
        };
        var result = ToolWriterPermissions.Check(state, [ancestor ? Parent : Output], [Parent], Token);
        Assert.False(Assert.Single(result.Decisions).Allowed);
        Assert.Equal(ToolWriteIssueCode.PathExcluded, Assert.Single(result.Issues).Code);
    }

    [Fact(DisplayName = "Existing generated files and control records cannot become tool-owned outputs")]
    public async Task RejectsOwnedAndControlPaths()
    {
        using var fixture = new ToolRecordFixture();
        var state = await ToolWriterReader.ReadAsync(fixture.Workspace, Token);
        var owned = Assert.Single(state.Lock.Document.ManagedPaths()).Path;
        var result = ToolWriterPermissions.Check(state, [owned, WorkspaceSettingsDefinitions.RelativePath], [owned, WorkspaceSettingsDefinitions.DirectoryName], Token);
        Assert.Equal(2, result.Decisions.Length);
        Assert.All(result.Decisions, decision => Assert.False(decision.Allowed));
        Assert.All(result.Issues, issue => Assert.Equal(ToolWriteIssueCode.VerificationFailed, issue.Code));
    }

    [Fact(DisplayName = "Required native Skills use their shared managed destination without a saved grant")]
    public async Task AllowsNativeSkillDestination()
    {
        using var fixture = new ToolRecordFixture();
        var state = await ToolWriterReader.ReadAsync(fixture.Workspace, Token);
        var path = WorkspaceLayout.NativeSkillsPath + "/fixture/SKILL.md";
        var result = ToolWriterPermissions.Check(state, [path], [], Token);
        Assert.Empty(result.Issues);
        Assert.True(Assert.Single(result.Decisions).Allowed);
        Assert.False(result.Decisions[0].SaveGrant);
    }
}
