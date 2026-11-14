using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteOperationTests
{
    [Fact(DisplayName = "Tool Install restores the lock version without forcing the lock channel")]
    public async Task BareInstallRestoresVersionOnly()
    {
        var fixture = new ToolWriteOperationFixture();
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ToolWriteOperationFixture.OldVersion, Assert.Single(result.Data.Tools).Version);
        Assert.Equal(ToolVersionSelectionKind.Exact, Assert.Single(fixture.ChannelRequests).Selection.Kind);
        Assert.Null(Assert.Single(fixture.ChannelRequests).ChannelOverride);
        Assert.Same(Assert.Single(fixture.ResolvedPlans), Assert.Single(fixture.ExecutedPlans));
    }

    [Theory(DisplayName = "Named Tool writers use the parsed latest minor or exact selection")]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public async Task NamedSelections(bool update, bool exact)
    {
        var fixture = new ToolWriteOperationFixture();
        var spec = new ToolSpec(ToolWriteOperationFixture.Id, new()
        {
            Kind = exact ? ToolVersionSelectionKind.Exact : ToolVersionSelectionKind.LatestSupported,
            ExactVersion = exact ? ToolWriteOperationFixture.OldVersion : null,
        });
        var result = await fixture.Create().ExecuteAsync(fixture.Request(
            update ? ToolWriteKind.Update : ToolWriteKind.Install, spec: spec), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(exact ? ToolWriteOperationFixture.OldVersion : ToolWriteOperationFixture.LatestVersion, result.Data.Tools[0].Version);
        Assert.Same(spec.Version, fixture.ChannelRequests[0].Selection);
    }

    [Fact(DisplayName = "Fresh bare Tool Install keeps the declared range and discloses latest network resolution")]
    public async Task FreshInstallUsesDeclaration()
    {
        var fixture = new ToolWriteOperationFixture(locked: false);
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(ToolVersionSelectionKind.MinorRange, fixture.ChannelRequests[0].Selection.Kind);
        Assert.Equal(ToolWriteOperationFixture.Range, fixture.ChannelRequests[0].Selection.Range);
        Assert.True(result.Data.Plan.UsedNetwork);
    }

    [Fact(DisplayName = "Tool batch preflights every selected tool and blocks all effects when one tool is blocked")]
    public async Task BatchPreflightHasNoEffects()
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true) { FailPreflight = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Equal(2, fixture.Calls.Count(call => call.Call == ToolWriteCall.ChannelPlan));
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Tool dry run uses concrete plans without install config record or backup effects")]
    public async Task PreviewHasNoEffects()
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false, preview: true), CancellationToken.None);

        Assert.True(result.Data.Preview);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.Writes);
        Assert.Empty(fixture.Processes);
        Assert.DoesNotContain(fixture.Calls, call => call.Call is ToolWriteCall.PlanPrompt or ToolWriteCall.OptionalPrompt);
    }

    [Fact(DisplayName = "Tool apply without yes or a terminal reports missing consent without effects")]
    public async Task MissingTerminalRequiresConsent()
    {
        var fixture = new ToolWriteOperationFixture();
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.MissingConsent);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Empty(fixture.Writes);
    }

    [Fact(DisplayName = "Tool JSON yes applies without prompting")]
    public async Task JsonYesNeverPrompts()
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(json: true), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(fixture.Calls, call => call.Call is ToolWriteCall.PlanPrompt or ToolWriteCall.OptionalPrompt or ToolWriteCall.ManagerPrompt);
    }

    [Fact(DisplayName = "Tool application passes current complete receipts and publishes after each config step")]
    public async Task PublishesCompleteSetsBetweenSteps()
    {
        var fixture = new ToolWriteOperationFixture();
        var retained = new ToolNativeCommandAddition { Id = ToolWriteOperationFixture.NativeChange, AllowListId = ToolWriteOperationFixture.NativeChange };
        fixture.SeedChanges(retained);
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(retained, fixture.ConfigCalls[0].Request.AppliedChanges);
        Assert.Contains(fixture.ConfigCalls[1].Request.AppliedChanges, receipt => receipt.Id == ToolWriteOperationFixture.FirstChange);
        Assert.Contains(retained, fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes);
        Assert.Equal(3, fixture.Records.Portable.Lock[ToolWriteOperationFixture.Id].Changes.Count);
        var first = fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ConfigApply);
        var second = fixture.Calls.FindLastIndex(call => call.Call == ToolWriteCall.ConfigApply);
        Assert.Contains(fixture.Calls.Skip(first + 1).Take(second - first - 1), call => call.Call == ToolWriteCall.Write);
    }

    [Fact(DisplayName = "Tool backups complete after final publication using the latest target hash")]
    public async Task BackupsCompleteLastWithLatestHash()
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true);
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.BackupComplete) > fixture.Calls.FindLastIndex(call => call.Call == ToolWriteCall.Write));
        var completion = Assert.Single(fixture.CompletedBackups);
        Assert.Same(fixture.IssuedBackups[0].Backup, completion.Backup);
        Assert.Equal(fixture.IssuedBackups[^1].ExpectedAppliedSha256, completion.ExpectedAppliedSha256);
        Assert.True(completion.Verified);
    }

    [Fact(DisplayName = "Tool config failure stops the batch and keeps verified receipts and backups")]
    public async Task FirstConfigFailureStopsBatch()
    {
        var fixture = new ToolWriteOperationFixture(secondTool: true) { FailConfig = ToolWriteOperationFixture.SecondChange };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Single(fixture.ExecutedPlans);
        Assert.Contains(fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes, receipt => receipt.Id == ToolWriteOperationFixture.FirstChange);
        Assert.DoesNotContain(fixture.Records.Portable.Lock[ToolWriteOperationFixture.Id].Changes, receipt => receipt.Id == ToolWriteOperationFixture.SecondChange);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.ToolId == ToolWriteOperationFixture.OtherId && outcome.State == ToolWriteStages.NotStarted);
    }

    [Fact(DisplayName = "Tool record failure stops before native setup and retains pending backups")]
    public async Task RecordFailureStopsBeforeNative()
    {
        var fixture = new ToolWriteOperationFixture { FailWrite = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.ConfigCalls);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Tool native failure stops before config and keeps the verified channel receipt")]
    public async Task NativeFailureStopsBeforeConfig()
    {
        var fixture = new ToolWriteOperationFixture { FailNative = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Empty(fixture.ConfigCalls);
        Assert.Equal(ToolWriteOperationFixture.OldVersion, fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Version);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Fact(DisplayName = "Tool reapply retains receipts and runs upstream reconfiguration")]
    public async Task ReapplyKeepsReceiptSet()
    {
        var fixture = new ToolWriteOperationFixture();
        var writer = fixture.Create();
        await writer.ExecuteAsync(fixture.Request(), CancellationToken.None);
        var result = await writer.ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(2, fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes.Count);
        Assert.Equal(2, fixture.Processes.Count);
        Assert.True(fixture.ExecutedPlans[^1].ReuseExisting);
    }

    [Fact(DisplayName = "Tool reapply accepts a supported self-update and records the detected latest version")]
    public async Task SelfUpdateReusesExisting()
    {
        var fixture = new ToolWriteOperationFixture();
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.LatestVersion;
        var spec = new ToolSpec(ToolWriteOperationFixture.Id, new() { Kind = ToolVersionSelectionKind.LatestSupported });
        var result = await fixture.Create().ExecuteAsync(fixture.Request(spec: spec), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(fixture.ExecutedPlans[0].ReuseExisting);
        Assert.Equal(ToolWriteOperationFixture.LatestVersion, fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Version);
    }

    [Fact(DisplayName = "Tool confirmed local native removal precedes project cleanup")]
    public async Task NativeRemovalPrecedesCleanup()
    {
        var fixture = RemovalFixture();
        fixture.Terminal = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.Native) < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ConfigRemove));
    }

    [Theory(DisplayName = "Tool account uninstall yes or decline still removes project additions")]
    [InlineData(true, false)]
    [InlineData(true, true)]
    [InlineData(false, true)]
    public async Task AccountUninstallSkippedButProjectRemoved(bool yes, bool terminal)
    {
        var fixture = RemovalFixture();
        fixture.AccountUninstall = true;
        fixture.Terminal = terminal;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: yes), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(fixture.Processes);
        Assert.False(fixture.Records.Portable!.Lock.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.False(fixture.Records.Portable.Settings.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Native && outcome.State == ToolWriteStages.Skipped);
        if (!yes) Assert.Equal(ToolWriteConsentKind.AccountUninstall, Assert.Single(fixture.OptionalQuestions).Kind);
    }

    [Fact(DisplayName = "Tool live account uninstall confirmation authorizes native-first removal")]
    public async Task LiveAccountConfirmationRunsNative()
    {
        var fixture = RemovalFixture();
        fixture.AccountUninstall = true;
        fixture.Terminal = true;
        fixture.AcceptOptional = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(fixture.Processes);
        Assert.Equal(ToolWriteConsentKind.AccountUninstall, Assert.Single(fixture.OptionalQuestions).Kind);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.Native) < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ConfigRemove));
    }

    [Fact(DisplayName = "Tool Remove keeps account settings without making successful project cleanup incomplete")]
    public async Task AccountSettingsStayAfterProjectRemoval()
    {
        var fixture = RemovalFixture();
        var setting = new ToolNativeSettingRecord
        {
            ToolId = ToolWriteOperationFixture.Id,
            ChangeId = ToolWriteOperationFixture.NativeChange,
            NativeHomePath = fixture.Facts.NativeHomes[ToolNativeHomePolicy.Node],
            Previous = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "synthetic-prior" },
            Applied = new() { Present = true, Kind = ToolRecordedValueKind.Native, Native = "synthetic-applied" },
            ManualRestoreArguments = ["synthetic-restore"],
        };
        fixture.Records = fixture.Records with
        {
            WorkspaceMachineState = ToolRecordReadState.Available,
            WorkspaceMachine = new()
            {
                SchemaVersion = ToolRecordDefinitions.LocalSchemaVersion,
                WorkspaceKey = WorkspaceIdentity.Key(fixture.Root),
                WorkspacePath = fixture.Root,
                NativeSettings = [setting],
            },
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Same(setting, Assert.Single(fixture.Records.WorkspaceMachine!.NativeSettings));
        Assert.Equal(setting.ManualRestoreArguments, Assert.Single(result.Data.Tools[0].KeptNativeSettings).ManualArguments);
        Assert.DoesNotContain(fixture.Processes, process => process.Arguments.SequenceEqual(setting.ManualRestoreArguments));
    }

    [Fact(DisplayName = "Tool repeated project removal is an effect-free no-op")]
    public async Task RepeatedRemovalIsNoOp()
    {
        var fixture = RemovalFixture();
        var writer = fixture.Create();
        await writer.ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);
        var writes = fixture.Writes.Count;
        var natives = fixture.Processes.Count;
        var result = await writer.ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(writes, fixture.Writes.Count);
        Assert.Equal(natives, fixture.Processes.Count);
    }

    [Fact(DisplayName = "Tool removal conflict keeps its receipt while other recorded additions are removed")]
    public async Task RemovalConflictKeepsReceipt()
    {
        var fixture = RemovalFixture();
        fixture.ConflictRemoval = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(ToolWriteOperationFixture.FirstChange, Assert.Single(fixture.Records.Portable!.Lock[ToolWriteOperationFixture.Id].Changes).Id);
        Assert.True(fixture.Records.Portable.Settings.ContainsKey(ToolWriteOperationFixture.Id));
    }


    [Fact(DisplayName = "Tool manager ambiguity is resolved interactively before plan consent")]
    public async Task ManagerChoicePrecedesConsent()
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, MultipleManagers = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Core.Framework.Tools.Models.Channels.ToolNpmClient.Npm, fixture.ChannelRequests[^1].ChosenManager);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ManagerPrompt) < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.PlanPrompt));
    }

    [Fact(DisplayName = "Tool yes delegates the deterministic manager fallback without prompting")]
    public async Task YesUsesDefaultManagerMode()
    {
        var fixture = new ToolWriteOperationFixture { MultipleManagers = true };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(Core.Framework.Tools.Models.Channels.ToolManagerChoiceMode.Default, fixture.ChannelRequests[0].ManagerChoice);
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.ManagerPrompt);
    }

    [Fact(DisplayName = "Tool named channel override reaches the concrete channel planner")]
    public async Task NamedChannelOverride()
    {
        var fixture = new ToolWriteOperationFixture();
        var request = fixture.Request(spec: new(ToolWriteOperationFixture.Id, new() { Kind = ToolVersionSelectionKind.LatestSupported })) with
        {
            Channel = ToolWriteOperationFixture.ChannelId,
        };
        var result = await fixture.Create().ExecuteAsync(request, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(request.Channel, fixture.ChannelRequests[0].ChannelOverride);
    }

    [Fact(DisplayName = "Unsupported exact lock versions block before channel effects")]
    public async Task UnsupportedLockedVersionBlocks()
    {
        var fixture = new ToolWriteOperationFixture();
        var portable = Assert.IsType<ToolPortableRecords>(fixture.Records.Portable);
        fixture.Records = fixture.Records with
        {
            Portable = portable with
            {
                Lock = new Dictionary<string, ToolLockRecord>
                {
                    [ToolWriteOperationFixture.Id] = portable.Lock[ToolWriteOperationFixture.Id] with { Version = "9.9.9" },
                },
            },
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(fixture.ExecutedPlans);
    }

    [Fact(DisplayName = "Tool record changes during confirmation block all effects")]
    public async Task ChangedRecordsBlockEffects()
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true };
        fixture.OnPlanConfirmed = () => fixture.Records = fixture.Records with
        {
            ExpectedFileSha256 = new Dictionary<string, string?> { [Path.Combine(fixture.Root, "synthetic-record.json")] = ToolWriteOperationFixture.Digest },
        };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Empty(fixture.ExecutedPlans);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.ExpectedStateChanged);
    }

    [Fact(DisplayName = "Tool yes removal keeps Open Forge shared bytes without calling shared deletion")]
    public async Task YesNeverDeletesSharedBytes()
    {
        var fixture = new ToolWriteOperationFixture { PlacedByOpenForge = true };
        var writer = fixture.Create();
        await writer.ExecuteAsync(fixture.Request(), CancellationToken.None);
        var result = await writer.ExecuteAsync(fixture.Request(ToolWriteKind.Remove), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(fixture.Records.Machine.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.SharedRemove);
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.OptionalPrompt);
    }

    [Fact(DisplayName = "Tool explicit grants save before planning through the separate settings method")]
    public async Task ExplicitGrantsSaveBeforePlanning()
    {
        var fixture = new ToolWriteOperationFixture();
        var result = await fixture.Create().ExecuteAsync(fixture.Request() with { AllowPaths = [ToolWriteOperationFixture.FirstChange] }, CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal([ToolWriteOperationFixture.FirstChange], fixture.SavedGrants);
        Assert.Equal(ToolWriteStages.Completed, Assert.Single(result.Data.Grants).State);
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.Grant) < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ChannelPlan));
        Assert.NotSame(Assert.Single(fixture.GrantRequests).Expected, fixture.Writes[0].Expected);
    }

    [Fact(DisplayName = "Tool config progress stays reported when its record publication fails")]
    public async Task VerifiedButUnpublishedProgressIsReported()
    {
        var fixture = new ToolWriteOperationFixture { FailWriteAt = 2 };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Config
            && outcome.Completed.Contains(ToolWriteOperationFixture.FirstChange, StringComparer.Ordinal));
        Assert.Empty(Assert.IsType<ToolPortableRecords>(fixture.Records.Portable).Lock[ToolWriteOperationFixture.Id].Changes);
        Assert.Empty(fixture.CompletedBackups);
    }

    [Theory(DisplayName = "Tool PATH changes need a live separate choice even when the session has a terminal")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PathChoiceIsSeparateFromYes(bool yes)
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, AddPathChoice = true };
        fixture.Facts = fixture.Facts with { Path = fixture.Facts.Path with { Readiness = Core.Framework.Tools.Models.Launchers.ToolPathReadiness.Missing } };
        var result = await fixture.Create().ExecuteAsync(fixture.Request(yes: yes), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(!yes, fixture.PathRequests.Any(request => request.Choice == Core.Framework.Tools.Models.Launchers.ToolPathChoice.AddForMe));
        Assert.Equal(!yes, fixture.Records.Path is not null);
        Assert.Equal(!yes, fixture.Calls.Any(call => call.Call == ToolWriteCall.PathPrompt));
    }

    [Fact(DisplayName = "Tool last shared installation removal offers and publishes confirmed PATH cleanup")]
    public async Task LastToolOffersPathRemoval()
    {
        var fixture = new ToolWriteOperationFixture { Terminal = true, AddPathChoice = true, AcceptOptional = true, PlacedByOpenForge = true };
        fixture.Facts = fixture.Facts with { Path = fixture.Facts.Path with { Readiness = Core.Framework.Tools.Models.Launchers.ToolPathReadiness.Missing } };
        var writer = fixture.Create();
        await writer.ExecuteAsync(fixture.Request(yes: false), CancellationToken.None);
        var result = await writer.ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(fixture.Calls, call => call.Call == ToolWriteCall.PathRemove);
        Assert.Contains(fixture.OptionalQuestions, question => question.Kind == ToolWriteConsentKind.PathChange);
        Assert.Empty(Assert.IsType<ToolPathState>(fixture.Records.Path).Changes);
        Assert.Empty(fixture.Records.Machine);
    }

    private static ToolWriteOperationFixture RemovalFixture()
    {
        var fixture = new ToolWriteOperationFixture();
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        fixture.SeedChanges(
            new ToolManagedFileAddition { Id = ToolWriteOperationFixture.FirstChange, Path = ToolWriteOperationFixture.FirstChange, Sha256 = ToolWriteOperationFixture.Digest },
            new ToolManagedFileAddition { Id = ToolWriteOperationFixture.SecondChange, Path = ToolWriteOperationFixture.SecondChange, Sha256 = ToolWriteOperationFixture.Digest });
        return fixture;
    }
}
