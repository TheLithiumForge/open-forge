using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-native-settings"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolNativeSettingsTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Stored telemetry preflight needs no installation and never creates consent or identity files")]
    public async Task PreinstallRead()
    {
        using var scenario = new ToolServiceScenario();
        var request = scenario.Request(ToolServiceScenario.Native()) with { Invocation = null };
        var plan = await scenario.Service.PlanAsync(request, Token);
        var current = Assert.Single(plan.Changes).Current;
        Assert.False(current.Present);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, current.Native);
        Assert.Empty(scenario.Calls);
        Assert.False(File.Exists(scenario.NativeFile));
        Assert.False(Directory.Exists(scenario.Context.NativeHomePath));
    }

    [Fact(DisplayName = "Native apply verifies the stored setting and keeps the first previous value during reapply")]
    public async Task ApplyAndReapply()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Run = _ => { scenario.WriteNative(false); return ToolServiceScenario.Success(); };
        scenario.Context = scenario.Context with
        {
            NativeEnvironment = new Dictionary<string, string>
            {
                [ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = "synthetic-raw-override"
            }
        };
        var request = scenario.Request(ToolServiceScenario.Native());
        var plan = await scenario.Service.PlanAsync(request, Token);
        var current = Assert.Single(plan.Changes).Current;
        Assert.False(current.Present);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, current.Native);
        var applied = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(applied.Failure);
        Assert.IsType<ToolNativeCommandAddition>(Assert.Single(applied.RemainingAdditions));
        var fact = Assert.Single(applied.NativeSettings);
        Assert.True(fact.Previous.Present);
        Assert.Equal(ToolRecordedValueKind.Native, fact.Previous.Kind);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, fact.Previous.Native);
        Assert.True(fact.Applied.Present);
        Assert.Equal(ToolRecordedValueKind.Native, fact.Applied.Kind);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, fact.Applied.Native);
        Assert.Equal(scenario.Context.NativeHomePath, fact.NativeHomePath);
        Assert.Equal(new[] { ToolNativeSettings.ApplyArguments[0], ToolNativeConfigDefinitions.EnabledValue }, fact.ManualRestoreArguments);
        Assert.Empty(Assert.Single(scenario.Calls).Environment);
        Assert.Equal(scenario.Invocation.PrefixArguments.Concat(ToolNativeSettings.ApplyArguments), scenario.Calls[0].Arguments);
        var reapply = request with { AppliedChanges = applied.RemainingAdditions, NativeSettings = applied.NativeSettings };
        var result = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(reapply, Token), Token);
        Assert.Null(result.Failure);
        Assert.Single(scenario.Calls);
        Assert.Equal(fact.Previous, Assert.Single(result.NativeSettings).Previous);
        Assert.False(current.Present);
        var removalRequest = new ToolConfigRemoveRequest
        {
            ToolId = request.ToolId,
            Context = request.Context,
            Lock = new() { Version = "1.2.3", Channel = "synthetic", Changes = result.RemainingAdditions },
            NativeSettings = result.NativeSettings
        };
        var removal = await scenario.Service.RemoveAsync(await scenario.Service.PlanRemoveAsync(removalRequest, Token), Token);
        Assert.Null(removal.Failure);
        Assert.Empty(removal.RemainingAdditions);
        Assert.Single(removal.KeptNativeSettings);
        Assert.Single(scenario.Calls);
    }

    [Fact(DisplayName = "Changed native state after preflight blocks before a setting write")]
    public async Task NativeDrift()
    {
        using var scenario = new ToolServiceScenario();
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native()), Token);
        scenario.WriteNative(false);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.NotNull(result.Failure);
        Assert.Empty(result.RemainingAdditions);
        Assert.Empty(scenario.Calls);
    }

    [Fact(DisplayName = "A different native home reads its own previous setting and retains the older home fact")]
    public async Task DifferentHome()
    {
        using var scenario = new ToolServiceScenario();
        scenario.WriteNative(false);
        var first = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native()), Token), Token);
        Assert.Null(first.Failure);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, Assert.Single(first.NativeSettings).Previous.Native);
        scenario.Context = scenario.Context with { NativeHomePath = scenario.Files.Combine("second-native-home") };
        scenario.Run = _ => { scenario.WriteNative(false); return ToolServiceScenario.Success(); };
        var request = scenario.Request(ToolServiceScenario.Native()) with { AppliedChanges = first.RemainingAdditions, NativeSettings = first.NativeSettings };
        var second = await scenario.Service.ApplyAsync(await scenario.Service.PlanAsync(request, Token), Token);
        Assert.Null(second.Failure);
        Assert.Equal(2, second.NativeSettings.Count);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, second.NativeSettings.Single(fact => fact.NativeHomePath == scenario.Context.NativeHomePath).Previous.Native);
    }

    [Fact(DisplayName = "Malformed stored telemetry blocks without falling back to an installed command")]
    public async Task StoredReaderWins()
    {
        using var scenario = new ToolServiceScenario();
        scenario.WriteNative(false);
        await File.WriteAllTextAsync(scenario.NativeFile, "{", Token);
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native()) with { InstalledInvocation = scenario.Invocation }, Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        Assert.Empty(scenario.Calls);
    }

    [Fact(DisplayName = "Native command reads use the installed invocation and only their declared environment overlay")]
    public async Task CommandRead()
    {
        using var scenario = new ToolServiceScenario();
        var prefix = scenario.Invocation with { PrefixArguments = ["installed-prefix"] };
        scenario.Run = _ => ToolServiceScenario.Success($"{ToolNativeConfigDefinitions.TelemetryStatusPrefix}{ToolNativeConfigDefinitions.EnabledStatus} ({ToolNativeConfigDefinitions.DefaultReason})\n");
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native(stored: false)) with { InstalledInvocation = prefix }, Token);
        Assert.NotEqual(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        var call = Assert.Single(scenario.Calls);
        Assert.Equal(prefix.PrefixArguments.Concat(ToolNativeSettings.ReadArguments), call.Arguments);
        Assert.Null(call.Environment[ToolNativeConfigDefinitions.DoNotTrackVariable]);
        Assert.Null(call.Environment[ToolNativeConfigDefinitions.CodeGraphTelemetryVariable]);
        Assert.Equal("1", call.Environment[ToolNativeConfigDefinitions.CodeGraphNoDownloadVariable]);
        Assert.True(call.Timeout > TimeSpan.Zero);
        Assert.True(call.MaximumOutputBytes > 0);
    }

    [Theory(DisplayName = "Unavailable installed reads and invalid process bounds block command-only setting preflight")]
    [InlineData(false, true), InlineData(true, false)]
    public async Task UnavailableRead(bool installed, bool bounded)
    {
        using var scenario = new ToolServiceScenario();
        var change = ToolServiceScenario.Native(stored: false) with { ReadTimeout = bounded ? TimeSpan.FromSeconds(1) : TimeSpan.Zero };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change) with { InstalledInvocation = installed ? scenario.Invocation : null }, Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        Assert.Empty(scenario.Calls);
    }

    [Theory(DisplayName = "Oversized stored files and unknown stored parsers block before native effects")]
    [InlineData(true), InlineData(false)]
    public async Task StoredReaderBounds(bool oversized)
    {
        using var scenario = new ToolServiceScenario();
        scenario.WriteNative(true);
        var change = ToolServiceScenario.Native();
        var stored = Assert.IsType<ToolNativeStoredValueSpec>(change.StoredValue);
        change = change with { StoredValue = oversized ? stored with { MaximumBytes = 1 } : stored with { Parser = (ToolNativeStoredValueParser)999 } };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change), Token);
        Assert.Equal(ToolChangeDisposition.Conflict, Assert.Single(plan.Changes).Disposition);
        Assert.Empty(scenario.Calls);
    }

    [Fact(DisplayName = "Failed native writes keep complete input receipts and original native facts")]
    public async Task FailedWrite()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Run = _ => ToolServiceScenario.Success() with { ExitCode = 1 };
        var untouched = new ToolManagedFileAddition { Id = "untouched", Path = "other", Sha256 = new string('a', 64) };
        var request = scenario.Request(ToolServiceScenario.Native()) with { AppliedChanges = [untouched] };
        var planned = await scenario.Service.PlanAsync(request with { AppliedChanges = [] }, Token);
        var result = await scenario.Service.ApplyAsync(planned with { Request = request }, Token);
        Assert.NotNull(result.Failure);
        Assert.Same(untouched, Assert.Single(result.RemainingAdditions));
        Assert.Single(result.Unfinished);
        Assert.Empty(result.NativeSettings);
    }
}
