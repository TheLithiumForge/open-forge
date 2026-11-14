using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

public sealed class ToolJourneyTransitionTests
{
    [Theory(DisplayName = "Synthetic patch Update publishes a new exact lock while teammate Install restores its existing lock")]
    [InlineData(false), InlineData(true), Trait("Feature", "tool-journey"), Trait("Evidence", "Unit")]
    public async Task ExactLockAndPatchTransition(bool update)
    {
        var fixture = new ToolWriteOperationFixture();
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(update ? ToolWriteKind.Update : ToolWriteKind.Install), TestContext.Current.CancellationToken);
        var expected = update ? ToolWriteOperationFixture.LatestVersion : ToolWriteOperationFixture.OldVersion;
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(expected, Assert.Single(result.Data.Tools).Version);
        Assert.Equal(expected, fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Version);
        Assert.Equal(update ? ToolVersionSelectionKind.LatestSupported : ToolVersionSelectionKind.Exact, Assert.Single(fixture.ChannelRequests).Selection.Kind);
        Assert.Equal(expected, Assert.Single(fixture.ExecutedPlans).Version);
    }
}
