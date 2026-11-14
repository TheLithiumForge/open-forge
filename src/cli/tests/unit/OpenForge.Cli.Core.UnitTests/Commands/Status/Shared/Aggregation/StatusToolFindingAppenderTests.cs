using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Observation;

namespace OpenForge.Cli.Core.UnitTests.Commands.Status.Shared.Aggregation;

public sealed class StatusToolFindingAppenderTests
{
    [Theory(DisplayName = "Workspace Status appends distinct Tool observations and keeps existing facts"),
     InlineData((int)ToolObservationState.Missing, (int)StatusFindingCode.ToolDeclaredMissing, (int)CliSemanticStatus.Attention),
     InlineData((int)ToolObservationState.Changed, (int)StatusFindingCode.ToolDrift, (int)CliSemanticStatus.Attention),
     InlineData((int)ToolObservationState.Unsupported, (int)StatusFindingCode.ToolUnsupported, (int)CliSemanticStatus.Attention),
     InlineData((int)ToolObservationState.Unavailable, (int)StatusFindingCode.ToolObservationUnavailable, (int)CliSemanticStatus.Incomplete), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void ToolFactsAreSeparate(int state, int code, int status)
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        var result = StatusToolFindingAppender.Append(seed, [ToolObservationSeeds.Create((ToolObservationState)state)]);
        Assert.Same(seed.Facts, result.Facts);
        var finding = Assert.Single(result.Findings);
        Assert.Equal((StatusFindingCode)code, finding.Code);
        Assert.Equal((CliSemanticStatus)status, result.Status);
        Assert.Equal(ToolObservationSeeds.Id, finding.Subject);
    }

    [Fact(DisplayName = "Current Tool observations leave workspace Status unchanged"), Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
    public void CurrentIsNoChange()
    {
        var seed = StatusResultSeeds.Representative(CliSemanticStatus.Complete);
        Assert.Same(seed, StatusToolFindingAppender.Append(seed, [ToolObservationSeeds.Create(ToolObservationState.Current)]));
    }
}
