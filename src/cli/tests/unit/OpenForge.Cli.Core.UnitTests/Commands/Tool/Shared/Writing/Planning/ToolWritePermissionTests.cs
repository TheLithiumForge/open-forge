using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWritePermissionTests
{
    [Theory(DisplayName = "A missing config destination grant blocks every tool in the preflighted batch")]
    [InlineData((int)ToolWriteKind.Install)]
    [InlineData((int)ToolWriteKind.Update)]
    public async Task MissingDestinationBlocksBatch(int kind)
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true);
        fixture.Permissions[ToolWriteOperationFixture.OtherId] = new([ToolWriteOperationFixture.SecondChange], [ToolWriteOperationFixture.SecondChange], WorkspacePermissionDecision.Required);
        var result = await fixture.Create().ExecuteAsync(fixture.Request((ToolWriteKind)kind), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(2, fixture.ChannelRequests.Count);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.PermissionRequired && finding.Path == ToolWriteOperationFixture.SecondChange);
        Assert.Equal([ToolWriteOperationFixture.SecondChange], result.Data.Tools.Single(tool => tool.Id == ToolWriteOperationFixture.OtherId).Permissions!.Missing);
        Assert.NotNull(result.PermissionCommand);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.Writes);
    }

    [Fact(DisplayName = "Removal permissions block native uninstall before any cleanup")]
    public async Task MissingRemovalGrantBlocksNative()
    {
        var fixture = new ToolWriteOperationFixture();
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        fixture.Permissions[ToolWriteOperationFixture.Id] = new([ToolWriteOperationFixture.FirstChange], [ToolWriteOperationFixture.FirstChange], WorkspacePermissionDecision.Required);
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.Writes);
    }

    [Theory(DisplayName = "Bound explicit grants reach config and native contexts in preview and application")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ContextUsesExplicitGrants(bool preview)
    {
        var fixture = new ToolWriteOperationFixture();
        var request = fixture.Request(preview: preview) with { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.All(fixture.ConfigRequests, value => Assert.Equal(request.AllowPaths, value.Context.AdditionalAllowInstallPaths));
        Assert.All(fixture.SetupRequests, value => Assert.Equal(request.AllowPaths, value.Context.AdditionalAllowInstallPaths));
        Assert.Equal(preview ? 0 : 1, fixture.GrantRequests.Count);
    }
}
