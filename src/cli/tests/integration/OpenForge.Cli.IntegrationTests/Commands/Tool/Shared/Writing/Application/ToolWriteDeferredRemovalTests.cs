using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Application;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Consent;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Application;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Writing.Application;

[Trait("Feature", "tool"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolWriteDeferredRemovalTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Project-only removal cleans real additions without any unchosen preview or output binding")]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, true)]
    public async Task ProjectOnlySkipsAllNativeRequests(bool yes, bool terminal, bool json)
    {
        using var scenario = new ToolWriteRemovalScenario(missingClient: true) { Terminal = terminal };
        await scenario.SeedAsync();
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: yes, json: json), Token);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(scenario.Processes.Previews);
        Assert.Empty(scenario.Processes.Uninstalls);
        Assert.False(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        Assert.Equal(ToolWriteRemovalScenario.Before, await File.ReadAllBytesAsync(scenario.NativeOutput, Token));
        var current = await scenario.Records.ReadAsync(scenario.Locations, Token);
        Assert.False(current.Portable!.Lock.ContainsKey(ToolWriteRemovalScenario.Id));
        Assert.False(current.Portable.Settings.ContainsKey(ToolWriteRemovalScenario.Id));
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(ToolNativePreviewKind.Description.ToString(), tool.NativePreview);
        var manual = Assert.Single(tool.Commands, command => command.Purpose == ToolWriteProjection.ManualNativeUninstallPurpose);
        Assert.Equal(new[] { ToolWriteRemovalScenario.Uninstall, scenario.NativeHome }, manual.Arguments);
        Assert.All(tool.PossibleOutputs, output => Assert.True(output.Conditional));
        Assert.Equal(scenario.Entry.Profiles[0].Uninstall.PossibleOutputs.Count, tool.PossibleOutputs.Count);
        Assert.Contains(tool.Effects, effect => effect.Kind == ToolDeclaredEffectKind.PathChange.ToString());
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Native && outcome.State == ToolWriteStages.Skipped);
        Assert.Empty(result.Data.Plan.Backups);
    }

    [Theory(DisplayName = "Account uninstall preview remains a description even with a live terminal")]
    [InlineData(false)]
    [InlineData(true)]
    public async Task PreviewDoesNotPrepareAccountEffects(bool yes)
    {
        using var scenario = new ToolWriteRemovalScenario(missingClient: true) { Terminal = true, AcceptAccount = true };
        await scenario.SeedAsync();
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: yes, preview: true), Token);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Empty(scenario.Questions);
        Assert.Empty(scenario.Processes.Previews);
        Assert.Empty(scenario.Processes.Uninstalls);
        Assert.True(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        Assert.True((await scenario.Records.ReadAsync(scenario.Locations, Token)).Portable!.Lock.ContainsKey(ToolWriteRemovalScenario.Id));
    }

    [Fact(DisplayName = "Confirmed account removal preserves safe independent bindings and runs native before project cleanup")]
    public async Task ConfirmedRemovalUsesFullSafePreparation()
    {
        using var scenario = new ToolWriteRemovalScenario { Terminal = true, AcceptAccount = true };
        await scenario.SeedAsync();
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: false), Token);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(scenario.Processes.Previews);
        Assert.Single(scenario.Processes.Uninstalls);
        Assert.Equal(new[] { "account-choice", "preview", "native" }, scenario.Events);
        var question = Assert.Single(scenario.Questions);
        Assert.Equal(ToolWriteConsentKind.AccountUninstall, question.Kind);
        Assert.Contains(question.Effects, effect => effect.Kind == ToolDeclaredEffectKind.PathChange.ToString());
        Assert.Contains(question.Effects, effect => effect.Kind == ToolDeclaredEffectKind.SharedFileDeletion.ToString());
        Assert.Equal(ToolWriteRemovalScenario.After, await File.ReadAllBytesAsync(scenario.NativeOutput, Token));
        Assert.False(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        var tool = Assert.Single(result.Data.Tools);
        Assert.Equal(ToolNativePreviewKind.DryRun.ToString(), tool.NativePreview);
        Assert.Contains(tool.PossibleOutputs, output => output.Path == Path.Combine(scenario.Facts.ClientRoots[ToolWriteRemovalScenario.IndependentClient], "settings.json"));
        Assert.Contains(tool.Backups, path => path == scenario.NativeOutput);
        Assert.Empty(result.Data.Plan.Backups);
    }

    [Fact(DisplayName = "Confirmed account removal blocks unavailable required bindings instead of discarding them")]
    public async Task ConfirmedMissingBindingBlocksCleanup()
    {
        using var scenario = new ToolWriteRemovalScenario(missingClient: true) { Terminal = true, AcceptAccount = true };
        await scenario.SeedAsync();
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: false), Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.NativePlanBlocked);
        Assert.Single(scenario.Processes.Previews);
        Assert.Empty(scenario.Processes.Uninstalls);
        Assert.True(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        Assert.True((await scenario.Records.ReadAsync(scenario.Locations, Token)).Portable!.Lock.ContainsKey(ToolWriteRemovalScenario.Id));
        Assert.Equal(ToolWriteRemovalScenario.Before, await File.ReadAllBytesAsync(scenario.NativeOutput, Token));
    }

    [Fact(DisplayName = "Confirmed account preparation keeps backup feasibility checks before native effects")]
    public async Task OccupiedBackupBlocksConfirmedUninstall()
    {
        using var scenario = new ToolWriteRemovalScenario { Terminal = true, AcceptAccount = true };
        await scenario.SeedAsync();
        await File.WriteAllBytesAsync(ToolBackupPaths.Beside(scenario.NativeOutput), ToolWriteRemovalScenario.After, Token);
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: false), Token);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Findings, finding => finding.Code == ToolWriteFindingCode.NativePlanBlocked);
        Assert.Single(scenario.Processes.Previews);
        Assert.Empty(scenario.Processes.Uninstalls);
        Assert.True(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        Assert.Equal(ToolWriteRemovalScenario.Before, await File.ReadAllBytesAsync(scenario.NativeOutput, Token));
    }

    [Fact(DisplayName = "Confirmed account output expectations are rechecked before backup and uninstall")]
    public async Task ChangedAccountOutputStopsApplication()
    {
        using var scenario = new ToolWriteRemovalScenario();
        await scenario.SeedAsync();
        var described = await scenario.PlanAsync();
        scenario.Processes.AllowNative = true;
        var prepared = await scenario.Native.RefreshAsync(described, scenario.Invocation, Token);
        Assert.Empty(prepared.Native!.Problems);
        Assert.Same(described.DeferredNativeRemoval!.Setup, prepared.Native.Request.Setup);
        Assert.Same(scenario.Facts.ClientRoots, prepared.Native.Request.Context.ClientRoots);
        Assert.Equal(ToolWriteRemovalScenario.Hash(ToolWriteRemovalScenario.Before), prepared.Native.ExpectedFileSha256[scenario.NativeOutput]);
        await File.WriteAllBytesAsync(scenario.NativeOutput, ToolWriteRemovalScenario.After, Token);
        var state = new ToolWriteExecution(await scenario.Records.ReadAsync(scenario.Locations, Token));

        Assert.False(await scenario.Native.ApplyAsync(prepared, state, Token));
        Assert.Equal(ToolWriteFindingCode.ExpectedStateChanged, Assert.Single(state.Findings).Code);
        Assert.Empty(scenario.Processes.Uninstalls);
        Assert.Empty(state.Backups);
        Assert.True(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
    }

    [Fact(DisplayName = "Failed confirmed uninstall keeps pending backups and leaves project cleanup unstarted")]
    public async Task FailedUninstallKeepsProgress()
    {
        using var scenario = new ToolWriteRemovalScenario { Terminal = true, AcceptAccount = true };
        await scenario.SeedAsync();
        scenario.Processes.FailUninstall = true;
        var result = await scenario.Writer().ExecuteAsync(scenario.Request(yes: false), Token);

        Assert.Equal(CliSemanticStatus.Failed, result.Status);
        Assert.Single(scenario.Processes.Previews);
        Assert.Single(scenario.Processes.Uninstalls);
        Assert.True(File.Exists(scenario.Files.Combine(ToolWriteRemovalScenario.ProjectFile)));
        Assert.True((await scenario.Records.ReadAsync(scenario.Locations, Token)).Portable!.Lock.ContainsKey(ToolWriteRemovalScenario.Id));
        var pending = Assert.Single(result.Data.Plan.Backups, backup => backup.Target == scenario.NativeOutput);
        Assert.False(pending.Verified);
        Assert.NotNull(pending.Backup);
        Assert.True(File.Exists(pending.Backup));
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Native && outcome.State == ToolWriteStages.Failed);
        Assert.Contains(result.Data.Outcomes, outcome => outcome.Stage == ToolWriteStages.Config && outcome.State == ToolWriteStages.NotStarted);
    }
}
