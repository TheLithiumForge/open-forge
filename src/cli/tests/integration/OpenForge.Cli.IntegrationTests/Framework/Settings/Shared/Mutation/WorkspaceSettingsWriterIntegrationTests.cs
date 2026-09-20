using System.Text.Json;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings.Models.Mutation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Mutation;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Framework.Settings.Shared.Mutation;

[Trait("Feature", "workspace-settings"), Trait("Evidence", "Integration")]
public sealed class WorkspaceSettingsWriterIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData("{ broken")]
    [InlineData("[]")]
    [InlineData("{\"allowInstallPaths\": false}")]
    public static async Task RefusesMalformedAuthoredSettingsWithoutChangingBytes(string original)
    {
        using var temporary = TemporaryWorkspace.Create("settings-write-invalid");
        temporary.CreateFile(".agents/open-forge.json", original);
        var before = temporary.SnapshotHashes();
        var result = await WorkspaceSettingsWriter.AddAllowInstallPathsAsync(new PhysicalPathResolver(), Workspace(temporary), ["docs"], TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsWriteState.Refused, result.State);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    [Trait("Boundary", "OS")]
    [Theory]
    [InlineData(false), InlineData(true)]
    public static async Task RefusesSettingsReachedThroughLinkedParentOrLeaf(bool parent)
    {
        using var temporary = TemporaryWorkspace.Create("settings-write-link");
        const string original = "{\"allowInstallPaths\": []}";
        temporary.CreateFile("actual/open-forge.json", original);
        if (parent)
        {
            temporary.CreateDirectorySymbolicLink(".agents", "actual");
        }
        else
        {
            temporary.CreateFileSymbolicLink(".agents/open-forge.json", "../actual/open-forge.json");
        }
        var before = temporary.SnapshotHashes();
        var result = await WorkspaceSettingsWriter.AddAllowInstallPathsAsync(new PhysicalPathResolver(), Workspace(temporary), ["docs"], TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsWriteState.Refused, result.State);
        Assert.Equal(before, temporary.SnapshotHashes());
        Assert.Equal(original, File.ReadAllText(temporary.Combine("actual/open-forge.json")));
    }

    [Trait("Boundary", "OS")]
    [Fact]
    public async Task ExplicitGrantPreservesUnknownKeysLegacyBytesAndUnrelatedTemporaryName()
    {
        using var temporary = TemporaryWorkspace.Create("settings-write-preserve");
        temporary.CreateFile(".agents/open-forge.json", "{\"first\":1,\"allowInstallPaths\":[\"tools\"],\"last\":{\"kept\":true}}");
        temporary.CreateFile(".agents/open-forge.permissions.json", "malformed retired data");
        temporary.CreateFile(".agents/open-forge.json.tmp", "unrelated user file");
        var result = await WorkspaceSettingsWriter.AddAllowInstallPathsAsync(new PhysicalPathResolver(), Workspace(temporary), ["docs", "site", "docs"], TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsWriteState.Written, result.State);
        using var json = JsonDocument.Parse(File.ReadAllText(temporary.Combine(".agents/open-forge.json")));
        Assert.Equal(["first", "allowInstallPaths", "last"], json.RootElement.EnumerateObject().Select(value => value.Name));
        Assert.Equal(["tools", "docs", "site"], json.RootElement.GetProperty("allowInstallPaths").EnumerateArray().Select(value => value.GetString()));
        Assert.True(json.RootElement.GetProperty("last").GetProperty("kept").GetBoolean());
        Assert.Equal("malformed retired data", File.ReadAllText(temporary.Combine(".agents/open-forge.permissions.json")));
        Assert.Equal("unrelated user file", File.ReadAllText(temporary.Combine(".agents/open-forge.json.tmp")));
        var before = temporary.SnapshotHashes();
        var repeat = await WorkspaceSettingsWriter.AddAllowInstallPathsAsync(new PhysicalPathResolver(), Workspace(temporary), ["docs/a.md"], TestContext.Current.CancellationToken);
        Assert.Equal(WorkspaceSettingsWriteState.AlreadyAdmitted, repeat.State);
        Assert.Equal(before, temporary.SnapshotHashes());
    }

    private static CliWorkspace Workspace(TemporaryWorkspace temporary)
        => new(temporary.Path, temporary.Path, CliWorkspaceSelectionMethod.ExplicitWorkspace);
}
