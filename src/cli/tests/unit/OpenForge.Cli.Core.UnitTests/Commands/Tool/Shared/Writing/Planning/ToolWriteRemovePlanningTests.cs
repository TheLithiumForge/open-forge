using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Request;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteRemovePlanningTests
{
    [Theory(DisplayName = "Project-only account removal never prepares the unchosen native effects")]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public async Task ProjectOnlyDoesNotPrepareNative(bool yes, bool terminal, bool json)
    {
        var fixture = Fixture();
        fixture.Terminal = terminal;
        fixture.FailNativePreparation = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: yes, json: json), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(fixture.Processes);
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.NativePreparation);
        var description = Assert.Single(fixture.SetupRequests);
        Assert.Null(description.InstalledInvocation);
        Assert.Null(description.Setup.NativePlan);
        Assert.Null(description.Setup.DryRunArguments);
        Assert.Empty(description.Setup.Effects);
        Assert.Empty(description.Setup.PossibleOutputs);
        Assert.False(fixture.Records.Portable!.Settings.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.False(fixture.Records.Portable.Lock.ContainsKey(ToolWriteOperationFixture.Id));
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(ToolNativePreviewKind.Description.ToString(), tool.NativePreview);
        var manual = Assert.Single(tool.Commands, command => command.Purpose == ToolWriteProjection.ManualNativeUninstallPurpose);
        Assert.Equal(description.Invocation.PrefixArguments.Concat(fixture.Catalogue.Entries[ToolWriteOperationFixture.Id].Profiles[0].Uninstall.Arguments), manual.Arguments);
        Assert.Contains(tool.Effects, effect => effect.Kind == ToolDeclaredEffectKind.PathChange.ToString());
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Native && outcome.State == ToolWriteStages.Skipped);
        Assert.Equal(yes || json ? 0 : 1, fixture.OptionalQuestions.Count);
    }

    [Theory(DisplayName = "Account removal preview describes original effects without preparation or writes")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewIsDescriptionOnly(bool yes)
    {
        var fixture = Fixture();
        fixture.Terminal = true;
        fixture.AcceptOptional = true;
        fixture.FailNativePreparation = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: yes, preview: true), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Data.Preview);
        Assert.Empty(fixture.OptionalQuestions);
        Assert.Empty(fixture.Processes);
        Assert.Empty(fixture.Writes);
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.NativePreparation);
        Assert.True(fixture.Records.Portable!.Lock.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.Equal(3, Assert.Single(result.Data.Tools).Effects.Count);
    }

    [Theory(DisplayName = "Live account choice prepares the original request before native-first execution")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ConfirmedPreparationUsesOriginalDeclarations(bool failPreparation)
    {
        var fixture = Fixture();
        fixture.Terminal = true;
        fixture.AcceptOptional = true;
        fixture.FailNativePreparation = failPreparation;
        var declaration = fixture.Catalogue.Entries[ToolWriteOperationFixture.Id].Profiles[0].Uninstall;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        var prepared = fixture.SetupRequests[1];
        Assert.Same(declaration, prepared.Setup);
        Assert.NotNull(prepared.InstalledInvocation);
        Assert.Same(fixture.Facts.ClientRoots, prepared.Context.ClientRoots);
        Assert.Same(fixture.Facts.NativeEnvironment, prepared.Context.NativeEnvironment);
        var question = Assert.Single(fixture.OptionalQuestions);
        Assert.Equal(ToolWriteConsentKind.AccountUninstall, question.Kind);
        Assert.Contains(question.Effects, effect => effect.Kind == ToolDeclaredEffectKind.PathChange.ToString());
        Assert.Contains(question.Effects, effect => effect.Kind == ToolDeclaredEffectKind.SharedFileDeletion.ToString());
        Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.OptionalPrompt)
            < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.NativePreparation));
        if (failPreparation)
        {
            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.NativePlanBlocked);
            Assert.Empty(fixture.Processes);
            Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.ConfigRemove);
            Assert.True(fixture.Records.Portable!.Lock.ContainsKey(ToolWriteOperationFixture.Id));
        }
        else
        {
            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Single(fixture.Processes);
            Assert.True(fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.Native)
                < fixture.Calls.FindIndex(call => call.Call == ToolWriteCall.ConfigRemove));
            Assert.DoesNotContain(Assert.Single(result.Data.Tools).Commands, command => command.Purpose == ToolWriteProjection.ManualNativeUninstallPurpose);
        }
    }

    [Fact(DisplayName = "Confirmed native removal failure keeps project receipts and cleanup unstarted")]
    public async Task NativeFailureStopsProjectCleanup()
    {
        var fixture = Fixture();
        fixture.Terminal = true;
        fixture.AcceptOptional = true;
        fixture.FailNative = true;
        var result = await fixture.Create().ExecuteAsync(fixture.Request(ToolWriteKind.Remove, yes: false), CancellationToken.None);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.True(fixture.Records.Portable!.Lock.ContainsKey(ToolWriteOperationFixture.Id));
        Assert.DoesNotContain(fixture.Calls, call => call.Call == ToolWriteCall.ConfigRemove);
        Assert.Empty(fixture.CompletedBackups);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Native && outcome.State == ToolWriteStages.Failed);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Config && outcome.State == ToolWriteStages.NotStarted);
    }

    private static ToolWriteOperationFixture Fixture()
    {
        var fixture = new ToolWriteOperationFixture { AccountUninstall = true };
        fixture.DetectedVersions[ToolWriteOperationFixture.Id] = ToolWriteOperationFixture.OldVersion;
        fixture.SeedChanges(new ToolManagedFileAddition
        {
            Id = ToolWriteOperationFixture.FirstChange,
            Path = ToolWriteOperationFixture.FirstChange,
            Sha256 = ToolWriteOperationFixture.Digest,
        });
        var uninstall = fixture.Catalogue.Entries[ToolWriteOperationFixture.Id].Profiles[0].Uninstall;
        fixture.SetUninstall(uninstall with
        {
            DryRunArguments = [.. uninstall.Arguments, "synthetic-dry-run"],
            Effects = [.. uninstall.Effects,
                new() { Id = "synthetic-settings", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.Configuration, Description = "Synthetic account settings." },
                new() { Id = "synthetic-path", Scope = ToolEffectScope.Account, Kind = ToolDeclaredEffectKind.PathChange, Description = "Synthetic native persistent user PATH removal." }],
        });
        return fixture;
    }
}
