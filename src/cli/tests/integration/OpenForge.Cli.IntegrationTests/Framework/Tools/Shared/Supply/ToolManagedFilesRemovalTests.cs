using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.Supply;

[Trait("Feature", "tool-managed-removal"), Trait("Evidence", "Integration")]
public sealed class ToolManagedFilesRemovalTests
{
    [Fact(DisplayName = "Fresh machine records prevent deletion of a runtime used by another tool")]
    public async Task KeepsSharedRuntime()
    {
        using var fixture = new ToolChannelFixture();
        var request = Create(fixture);
        var other = fixture.State(System.IO.Path.Combine(fixture.Native, "other.exe"), request.ApprovedRuntimes[0].RuntimePath) with { ToolId = "other" };
        fixture.Records.Value = request.Records with { Machine = new Dictionary<string, ToolMachineState> { [request.Expected.ToolId] = request.Expected, [other.ToolId] = other } };
        var result = await fixture.Service.RemoveAsync(request with { Records = fixture.Records.Value }, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.Contains(request.ApprovedRuntimes[0].RuntimePath, result.KeptPaths);
        Assert.True(Directory.Exists(request.ApprovedRuntimes[0].RuntimePath));
        Assert.Equal(1, fixture.Records.Reads);
    }

    [Fact(DisplayName = "Approved launcher removal permits deletion of its now-unused private runtime")]
    public async Task RemovesUnusedRuntime()
    {
        using var fixture = new ToolChannelFixture();
        var request = Create(fixture);
        var result = await fixture.Service.RemoveAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(result.Failure);
        Assert.Empty(result.KeptPaths);
        Assert.False(Directory.Exists(request.ApprovedRuntimes[0].RuntimePath));
        Assert.False(File.Exists(request.Expected.CommandPath));
    }

    [Fact(DisplayName = "Changed C record expectations block shared deletion before any file effect")]
    public async Task BlocksStaleRecords()
    {
        using var fixture = new ToolChannelFixture();
        var request = Create(fixture);
        fixture.Records.Value = request.Records with { ExpectedFileSha256 = new Dictionary<string, string?> { [System.IO.Path.Combine(fixture.Home, "synthetic-record.json")] = "changed" } };
        var result = await fixture.Service.RemoveAsync(request, TestContext.Current.CancellationToken);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemovedPaths);
        Assert.True(File.Exists(request.Expected.CommandPath));
    }

    [Fact(DisplayName = "Unapproved paths and an unselected own launcher keep the runtime")]
    public async Task KeepsUnselectedSharedFiles()
    {
        using var fixture = new ToolChannelFixture();
        var request = Create(fixture) with { ApprovedToolPaths = [] };
        var result = await fixture.Service.RemoveAsync(request, TestContext.Current.CancellationToken);
        Assert.Empty(result.RemovedPaths);
        Assert.True(File.Exists(request.Expected.CommandPath));
        Assert.True(Directory.Exists(request.ApprovedRuntimes[0].RuntimePath));
    }

    private static ToolManagedFilesRemoveRequest Create(ToolChannelFixture fixture)
    {
        var runtimePath = ToolSupplyPaths.Runtime(fixture.Home, ToolChannelFixture.RuntimeVersion);
        Directory.CreateDirectory(runtimePath);
        var node = System.IO.Path.Combine(runtimePath, "node.exe");
        File.WriteAllText(node, "synthetic node");
        var bin = ToolSupplyPaths.Bin(fixture.Home);
        Directory.CreateDirectory(bin);
        var command = System.IO.Path.Combine(bin, "sample.cmd");
        File.WriteAllText(command, "synthetic launcher");
        var state = fixture.State(command, node) with
        {
            Launchers = [new() { ToolId = ToolChannelFixture.Id, Kind = ToolLauncherKind.WindowsCmd,
            TargetKind = ToolLauncherTargetKind.RuntimeEntry, LauncherPath = command, RuntimePath = node, EntryPath = System.IO.Path.Combine(fixture.Home, "entry.js"), Arguments = [] }]
        };
        var runtime = new ToolNodeRuntimeRecord
        {
            SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
            Version = ToolChannelFixture.RuntimeVersion,
            RuntimePath = runtimePath,
            Integrity = new() { Kind = ToolVerificationKind.SignedNodeChecksums, Source = "synthetic", Sha256 = new string('a', 64) }
        };
        fixture.Records.Value = fixture.Records.Value with { Machine = new Dictionary<string, ToolMachineState> { [state.ToolId] = state }, Runtimes = new Dictionary<string, ToolNodeRuntimeRecord> { [runtime.Version] = runtime } };
        return new() { Locations = new(fixture.Workspace, fixture.Home), Records = fixture.Records.Value, Expected = state, ApprovedToolPaths = [command], ApprovedRuntimes = [runtime] };
    }
}
