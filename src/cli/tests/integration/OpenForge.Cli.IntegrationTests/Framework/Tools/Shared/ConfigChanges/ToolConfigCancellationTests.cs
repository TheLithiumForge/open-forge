using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;
using OpenForge.Cli.Core.Framework.Tools.Shared.NativeSettings;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config-cancellation"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolConfigCancellationTests
{
    private const string First = "first.json";
    private const string Second = "second.json";
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Cancellation before apply or remove keeps all receipts and native facts without effects")]
    public async Task BeforeEffects()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(First, Second);
        scenario.Files.WriteText(First, "{\"value\":true}");
        var first = ToolServiceScenario.Set("first", First, "true");
        var receipt = Receipt(first);
        var fact = Fact(scenario);
        var request = scenario.Request(first, ToolServiceScenario.Set("second", Second, "false")) with { AppliedChanges = [receipt], NativeSettings = [fact] };
        var plan = await scenario.Service.PlanAsync(request, Token);
        var removal = await scenario.Service.PlanRemoveAsync(scenario.Removal([receipt]) with { NativeSettings = [fact] }, Token);
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var applied = await scenario.Service.ApplyAsync(plan, cancellation.Token);
        Assert.True(applied.Cancelled);
        Assert.NotNull(applied.Failure);
        Assert.NotEmpty(applied.Unfinished);
        Assert.Same(receipt, Assert.Single(applied.RemainingAdditions));
        Assert.Same(fact, Assert.Single(applied.NativeSettings));
        Assert.Empty(applied.CompletedChanges);
        Assert.Empty(applied.PendingBackups);
        var removed = await scenario.Service.RemoveAsync(removal, cancellation.Token);
        Assert.True(removed.Cancelled);
        Assert.Same(receipt, Assert.Single(removed.RemainingAdditions));
        Assert.Same(fact, Assert.Single(removed.KeptNativeSettings));
        Assert.Empty(removed.RemovedChanges);
        Assert.Empty(removed.PendingBackups);
        Assert.False(File.Exists(scenario.Files.Combine(Second)));
    }

    [Fact(DisplayName = "Cancellation during a later backup keeps the verified apply prefix and its pending backup")]
    public async Task ApplyPrefix()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(First, Second);
        scenario.Files.WriteText(First, "{}");
        scenario.Files.WriteText(Second, "{}");
        var fact = Fact(scenario);
        var request = scenario.Request(ToolServiceScenario.Set("first", First, "true"), ToolServiceScenario.Set("second", Second, "false")) with { NativeSettings = [fact] };
        var plan = await scenario.Service.PlanAsync(request, Token);
        using var cancellation = new CancellationTokenSource();
        var probes = 0;
        scenario.Resolving = _ => { if (++probes == 2) cancellation.Cancel(); };
        var result = await scenario.Service.ApplyAsync(plan, cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Equal("first", Assert.Single(result.CompletedChanges));
        Assert.Equal("first", Assert.Single(result.RemainingAdditions).Id);
        Assert.Equal("second", Assert.Single(result.Unfinished));
        Assert.Same(fact, Assert.Single(result.NativeSettings));
        var pending = Assert.Single(result.PendingBackups);
        Assert.True(pending.Verified);
        Assert.True(File.Exists(pending.Backup.BackupPath));
        Assert.Equal("{}"u8.ToArray(), await File.ReadAllBytesAsync(scenario.Files.Combine(Second), Token));
    }

    [Fact(DisplayName = "Cancellation during later removal keeps verified removals unstarted receipts native facts and backups")]
    public async Task RemovalPrefix()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(First, Second);
        scenario.Files.WriteText(First, "{\"value\":true}");
        scenario.Files.WriteText(Second, "{\"value\":false}");
        var first = Receipt(ToolServiceScenario.Set("first", First, "true"));
        var second = Receipt(ToolServiceScenario.Set("second", Second, "false"));
        var native = new ToolNativeCommandAddition { Id = "native", AllowListId = ToolNativeConfigDefinitions.CodeGraphTelemetryAllowListId };
        var fact = Fact(scenario);
        var plan = await scenario.Service.PlanRemoveAsync(scenario.Removal([first, second, native]) with { NativeSettings = [fact] }, Token);
        using var cancellation = new CancellationTokenSource();
        var probes = 0;
        scenario.Resolving = _ => { if (++probes == 2) cancellation.Cancel(); };
        var result = await scenario.Service.RemoveAsync(plan, cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Equal(first.Id, Assert.Single(result.RemovedChanges));
        Assert.Equal(new[] { second.Id, native.Id }, result.RemainingAdditions.Select(change => change.Id));
        Assert.Same(fact, Assert.Single(result.KeptNativeSettings));
        var pending = Assert.Single(result.PendingBackups);
        Assert.True(pending.Verified);
        Assert.True(File.Exists(pending.Backup.BackupPath));
        Assert.Equal("{\"value\":false}"u8.ToArray(), await File.ReadAllBytesAsync(scenario.Files.Combine(Second), Token));
    }

    [Theory(DisplayName = "Typed native cancellation and OCE keep completed receipts while timeout stays a failure")]
    [InlineData("cancelled"), InlineData("exception"), InlineData("timeout")]
    public async Task NativeCancellation(string outcome)
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(First);
        scenario.Files.WriteText(First, "{}");
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Set("first", First, "true"), ToolServiceScenario.Native()), Token);
        scenario.Run = _ => outcome switch
        {
            "cancelled" => ToolServiceScenario.Success() with { Cancelled = true, ExitCode = null },
            "exception" => throw new OperationCanceledException(),
            "timeout" => ToolServiceScenario.Success() with { TimedOut = true, ExitCode = null },
            _ => throw new ArgumentOutOfRangeException(nameof(outcome)),
        };
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Equal(outcome != "timeout", result.Cancelled);
        Assert.NotNull(result.Failure);
        Assert.Equal("first", Assert.Single(result.CompletedChanges));
        Assert.Equal("first", Assert.Single(result.RemainingAdditions).Id);
        Assert.Single(result.PendingBackups);
        Assert.Empty(result.NativeSettings);
        Assert.Equal(ToolServiceScenario.Native().Id, Assert.Single(result.Unfinished));
    }

    [Fact(DisplayName = "A verified native account setting remains in machine facts when a later file step is cancelled")]
    public async Task NativePrefix()
    {
        using var scenario = new ToolServiceScenario();
        scenario.Files.Grant(Second);
        scenario.Files.WriteText(Second, "{}");
        var native = ToolServiceScenario.Native();
        var plan = await scenario.Service.PlanAsync(scenario.Request(native, ToolServiceScenario.Set("second", Second, "true")), Token);
        scenario.Run = _ => { scenario.WriteNative(enabled: false); return ToolServiceScenario.Success(); };
        using var cancellation = new CancellationTokenSource();
        scenario.Resolving = _ => cancellation.Cancel();
        var result = await scenario.Service.ApplyAsync(plan, cancellation.Token);
        Assert.True(result.Cancelled);
        Assert.Equal(native.Id, Assert.Single(result.CompletedChanges));
        Assert.IsType<ToolNativeCommandAddition>(Assert.Single(result.RemainingAdditions));
        var fact = Assert.Single(result.NativeSettings);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, fact.Previous.Native);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, fact.Applied.Native);
        Assert.Equal("second", Assert.Single(result.Unfinished));
    }

    private static ToolJsonValueAddition Receipt(ToolJsonValueChange change) => new()
    { Id = change.Id, Path = change.PathTemplate, Pointer = change.Pointer, Value = change.Value ?? throw new InvalidOperationException() };

    private static ToolNativeSettingRecord Fact(ToolServiceScenario scenario) => new()
    {
        ToolId = ToolServiceScenario.ToolId,
        ChangeId = "native",
        NativeHomePath = scenario.Context.NativeHomePath,
        Previous = ToolNativeSettings.Value(ToolNativeConfigDefinitions.EnabledValue),
        Applied = ToolNativeSettings.Value(ToolNativeConfigDefinitions.DisabledValue),
        ManualRestoreArguments = [ToolNativeSettings.ApplyArguments[0], ToolNativeConfigDefinitions.EnabledValue]
    };
}
