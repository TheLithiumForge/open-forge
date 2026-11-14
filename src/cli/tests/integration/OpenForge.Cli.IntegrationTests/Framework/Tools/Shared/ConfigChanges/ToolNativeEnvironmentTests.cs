using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-native-environment"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolNativeEnvironmentTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Fact(DisplayName = "Unavailable native environment remains unchecked independently of a readable stored default")]
    public async Task UnavailableEnvironment()
    {
        using var scenario = new ToolServiceScenario();
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native()), Token);
        var observation = Assert.Single(plan.Changes);
        Assert.NotEqual(ToolChangeDisposition.Conflict, observation.Disposition);
        Assert.Null(observation.NativeEnvironmentOverrides);
        Assert.False(observation.Current.Present);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, observation.Current.Native);
        Assert.Empty(scenario.Calls);
    }

    [Theory(DisplayName = "Native observations report only the deciding variable and effective value without trimming raw inputs")]
    [InlineData(null, null, null, null)]
    [InlineData("", "", null, null)]
    [InlineData("0", null, null, null)]
    [InlineData("false", null, null, null)]
    [InlineData("FaLsE", null, null, null)]
    [InlineData(null, "", null, null)]
    [InlineData(null, "0", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(null, "false", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(null, "FaLsE", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(null, "1", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData(null, "synthetic-raw-value", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData(null, " false ", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData(null, " 0 ", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData(null, " ", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData("1", null, ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData("TRUE", "1", ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData("synthetic-raw-value", "0", ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(" false ", "false", ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(" 0 ", "1", ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData(" ", "", ToolNativeConfigDefinitions.DoNotTrackVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData("0", "1", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData("FALSE", "0", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.DisabledValue)]
    [InlineData("false", "synthetic-raw-value", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.EnabledValue)]
    [InlineData("", "false", ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, ToolNativeConfigDefinitions.DisabledValue)]
    public async Task EffectiveOverride(string? track, string? telemetry, string? decidingVariable, string? effectiveValue)
    {
        using var scenario = new ToolServiceScenario();
        scenario.WriteNative(false);
        var environment = new Dictionary<string, string>(StringComparer.Ordinal);
        if (track is not null) environment.Add(ToolNativeConfigDefinitions.DoNotTrackVariable, track);
        if (telemetry is not null) environment.Add(ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, telemetry);
        scenario.Context = scenario.Context with { NativeEnvironment = environment };
        var plan = await scenario.Service.PlanAsync(scenario.Request(ToolServiceScenario.Native()), Token);
        var observation = Assert.Single(plan.Changes);
        Assert.NotEqual(ToolChangeDisposition.Conflict, observation.Disposition);
        var overrides = Assert.IsAssignableFrom<IReadOnlyDictionary<string, string>>(observation.NativeEnvironmentOverrides);
        if (decidingVariable is null) Assert.Empty(overrides);
        else
        {
            var deciding = Assert.Single(overrides);
            Assert.Equal(decidingVariable, deciding.Key);
            Assert.Equal(effectiveValue, deciding.Value);
        }
        Assert.True(observation.Current.Present);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, observation.Current.Native);
        Assert.Equal(ToolNativeConfigDefinitions.DisabledValue, observation.Desired.Native);
        Assert.Empty(scenario.Calls);
    }
}
