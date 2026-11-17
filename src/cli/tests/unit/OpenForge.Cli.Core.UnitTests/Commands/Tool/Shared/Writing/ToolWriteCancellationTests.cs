using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteCancellationTests
{
    [Theory(DisplayName = "Typed channel config and record cancellation keeps completed prefixes and stops the batch")]
    [InlineData((int)ToolWriteCall.ChannelInstall, false)]
    [InlineData((int)ToolWriteCall.ConfigApply, false)]
    [InlineData((int)ToolWriteCall.Write, false)]
    [InlineData((int)ToolWriteCall.ChannelInstall, true)]
    [InlineData((int)ToolWriteCall.ConfigApply, true)]
    [InlineData((int)ToolWriteCall.Write, true)]
    public async Task CompletedPrefixesSurvive(int stage, bool cancelToken)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new ToolWriteOperationFixture(secondTool: true)
        {
            CancelAt = (ToolWriteCall)stage,
            Cancellation = cancelToken ? cancellation : null,
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.Interrupted);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Completed.Count != 0);
        Assert.NotEmpty(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Single(fixture.ExecutedPlans);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.ToolId == ToolWriteOperationFixture.OtherId && outcome.State == ToolWriteStages.NotStarted);
        if ((ToolWriteCall)stage == ToolWriteCall.ConfigApply)
        {
            Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Config
                && outcome.Completed.Contains(ToolWriteOperationFixture.FirstChange, StringComparer.Ordinal));
            Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Records && outcome.State == ToolWriteStages.NotStarted);
        }
    }

    [Fact(DisplayName = "Cancellation returned by the last config item still overrides a completed effect")]
    public async Task LastConfigItemCancels()
    {
        var fixture = new ToolWriteOperationFixture { CancelAt = ToolWriteCall.ConfigApply, CancelId = ToolWriteOperationFixture.SecondChange };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(2, fixture.ConfigCalls.Count);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Completed.Contains(ToolWriteOperationFixture.SecondChange, StringComparer.Ordinal));
        Assert.Contains(fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes, change => change.Id == ToolWriteOperationFixture.FirstChange);
        Assert.DoesNotContain(fixture.Records.Portable.Lock[ToolWriteOperationFixture.Id].Changes, change => change.Id == ToolWriteOperationFixture.SecondChange);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Typed removal cancellation reports the verified removal without guessing its unpublished receipt")]
    public async Task RemovalCancellationKeepsUnpublishedProgress()
    {
        var fixture = new ToolWriteOperationFixture { CancelAt = ToolWriteCall.ConfigRemove };
        fixture.SeedChanges(new ToolManagedFileAddition
        { Id = ToolWriteOperationFixture.FirstChange, Path = ToolWriteOperationFixture.FirstChange, Sha256 = ToolWriteOperationFixture.Digest });
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Config
            && outcome.Completed.Contains(ToolWriteOperationFixture.FirstChange, StringComparer.Ordinal));
        Assert.Single(fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes);
        Assert.Empty(fixture.Writes);
        Assert.NotEmpty(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Theory(DisplayName = "Grant cancellation keeps saved settings and backups before tool planning")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task GrantCancellation(bool cancelToken)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new ToolWriteOperationFixture { CancelAt = ToolWriteCall.Grant, Cancellation = cancelToken ? cancellation : null };
        var request = fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(request.AllowPaths, fixture.SavedGrants);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.Empty(fixture.ChannelRequests);
        Assert.Empty(fixture.Writes);
        Assert.NotEmpty(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Theory(DisplayName = "Caller cancellation during description or final PATH publication stays Interrupted")]
    [InlineData((int)ToolWriteCall.PathAdd)]
    [InlineData((int)ToolWriteCall.BackupComplete)]
    public async Task DescriptionAndLastBoundaryCancellation(int stage)
    {
        using var cancellation = new CancellationTokenSource();
        var fixture = new ToolWriteOperationFixture { CancelAt = (ToolWriteCall)stage, Cancellation = cancellation };
        if ((ToolWriteCall)stage == ToolWriteCall.PathAdd)
            fixture.Facts = fixture.Facts with { Path = fixture.Facts.Path with { Readiness = Core.Framework.Tools.Models.Launchers.ToolPathReadiness.Missing } };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        if ((ToolWriteCall)stage == ToolWriteCall.PathAdd)
        {
            Assert.Empty(fixture.ExecutedPlans);
            Assert.Empty(fixture.Writes);
        }
        else Assert.Contains(result.Data.Outcomes, outcome => outcome.State == ToolWriteStages.Completed);
    }

    [Fact(DisplayName = "An ordinary channel failure remains Failed without cancellation")]
    public async Task FailureDoesNotBecomeCancellation()
    {
        var fixture = new ToolWriteOperationFixture { FailChannel = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.DoesNotContain(result.Findings, finding => finding.Code == ToolWriteFindingCode.Interrupted);
    }
}
