using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteGrantTests
{
    [Fact(DisplayName = "Saved grants survive a blocked batch without creating tool records")]
    public async Task BlockedBatchKeepsSettingsEdit()
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true) { FailPreflight = true };
        var before = fixture.Records;
        var request = fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(request.AllowPaths, fixture.SavedGrants);
        Assert.Same(before.Portable, fixture.Records.Portable);
        Assert.Same(before.Machine, fixture.Records.Machine);
        Assert.Same(before.Runtimes, fixture.Records.Runtimes);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
    }

    [Theory(DisplayName = "Saved grants and their pending backups survive later cancellation")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task CancellationKeepsSettingsEdit(bool duringPlanning)
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, AcceptPlan = false, CancelPlanning = duringPlanning };
        var request = fixture.Request(yes: false) with { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(request.AllowPaths, fixture.SavedGrants);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.ExecutedPlans);
    }

    [Fact(DisplayName = "Preview describes requested grants without authoring settings")]
    public async Task PreviewDoesNotWriteGrants()
    {
        var fixture = new ToolWriteOperationFixture();
        var request = fixture.Request(preview: true) with { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ToolWriteStages.Planned, Assert.Single(result.Data.Grants).State);
        Assert.Empty(fixture.GrantRequests);
        Assert.Empty(fixture.SavedGrants);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.IssuedBackups);
    }

    [Fact(DisplayName = "Grant authoring failure stops before tool planning and keeps returned backups")]
    public async Task GrantFailureStopsPlanning()
    {
        var fixture = new ToolWriteOperationFixture { FailGrants = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(ToolWriteStages.Failed, Assert.Single(result.Data.Grants).State);
        Assert.NotEmpty(result.Data.Grants[0].Unfinished);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Empty(fixture.ChannelRequests);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Grant verification reread failure still reports the saved edit and pending backup")]
    public async Task FreshReadFailureKeepsGrantReceipt()
    {
        var fixture = new ToolWriteOperationFixture { FailGrantRead = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Empty(fixture.ChannelRequests);
    }

    [Fact(DisplayName = "Missing tool consent keeps the separately authored path grant")]
    public async Task MissingConsentKeepsGrant()
    {
        var fixture = new ToolWriteOperationFixture();
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false) with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.MissingConsent);
        Assert.NotEmpty(fixture.SavedGrants);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Invalid tool selection keeps grants but creates no tool records")]
    public async Task InvalidSelectionKeepsGrant()
    {
        var fixture = new ToolWriteOperationFixture();
        var request = fixture.Request(spec: new("synthetic-unknown", new() { Kind = ToolVersionSelectionKind.LatestSupported }))
            with
        { AllowPaths = [ToolWriteOperationFixture.FirstChange] };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.NotEmpty(fixture.SavedGrants);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Grant-only completion verifies and completes its pending backup after planning")]
    public async Task EmptyToolSelectionKeepsGrant()
    {
        var fixture = new ToolWriteOperationFixture(declared: false, locked: false);
        var result = await fixture.Create().ExecuteAsync(fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(result.Data.Tools);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.Empty(result.Data.Plan.Backups);
        Assert.Single(fixture.CompletedBackups);
        Assert.Empty(fixture.Writes);
    }

    [Fact(DisplayName = "Grant-only backup completion failure keeps its handle in the final result")]
    public async Task GrantOnlyBackupFailureKeepsHandle()
    {
        var fixture = new ToolWriteOperationFixture(declared: false, locked: false) { FailBackupCompletion = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.Single(result.Data.Plan.Backups);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.BackupFailed);
        Assert.Empty(fixture.Writes);
    }
}
