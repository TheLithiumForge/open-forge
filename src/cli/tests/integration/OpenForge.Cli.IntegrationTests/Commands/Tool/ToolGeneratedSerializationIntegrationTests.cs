using System.Text.Json;
using OpenForge.Cli.Core.Commands.Tool.Models.Read;
using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;
using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Binding;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition;
using OpenForge.Cli.IntegrationTests.Serialization.Shared.Assertions;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool;

public sealed class ToolGeneratedSerializationIntegrationTests
{
    [Theory(DisplayName = "Every Tool leaf uses one source-generated schema-3 report with ordered facts at every detail"),
     InlineData("list"), InlineData("inspect"), InlineData("status"), InlineData("install"), InlineData("update"), InlineData("remove"),
     Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task Schema3AtEveryDetail(string leaf)
    {
        using var fixture = new ToolCompositionFixture(twoTools: true);
        fixture.Declare();
        var before = fixture.Snapshot();
        var writer = leaf is "install" or "update" or "remove";
        string[] operand = leaf is "inspect" or "remove" ? [ToolCompositionFixture.FirstId] : [];
        string[] preview = writer ? [ToolWriteBinding.DryRunOption] : [];
        JsonElement? baseline = null;
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            var run = await fixture.RunAsync(["tool", leaf, .. operand, .. preview, "--format", "json", "--detail", detail]);
            using var json = run.Json();
            Schema3Assertions.Envelope(json.RootElement, "tool " + leaf, CliStatusDefinitions.Read(run.Completion.Status).MachineName, detail);
            var data = ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data));
            var tools = ToolCompositionFixture.Field(data, nameof(ToolReadPayload.Tools));
            var ids = tools.EnumerateArray().Select(tool => ToolCompositionFixture.Field(tool, nameof(ToolReadTool.Id)).GetString()).ToArray();
            Assert.NotEmpty(ids);
            Assert.Equal(ids.Order(StringComparer.Ordinal), ids);
            if (baseline is { } prior) Assert.True(JsonElement.DeepEquals(prior, data));
            baseline = data.Clone();
            if (writer)
            {
                Assert.True(ToolCompositionFixture.Field(data, nameof(ToolWriteData.Preview)).GetBoolean());
                Assert.Equal(JsonValueKind.Object, ToolCompositionFixture.Field(data, nameof(ToolWriteData.Plan)).ValueKind);
                Assert.Equal(JsonValueKind.Array, ToolCompositionFixture.Field(data, nameof(ToolWriteData.Outcomes)).ValueKind);
            }
            Assert.Empty(run.Prompts);
            if (detail != "debug") Assert.Empty(run.Error);
        }
        Assert.False(JsonSerializer.IsReflectionEnabledByDefault);
        Assert.Equal(before, fixture.Snapshot());
        Assert.False(Directory.Exists(fixture.Home));
        Assert.Equal(0, fixture.DownloadCalls);
        Assert.Equal(0, fixture.PathEffects);
    }
    [Fact(DisplayName = "Inherited native values are reported only as effective on or off facts"), Trait("Feature", "tool-composition"), Trait("Evidence", "Integration")]
    public async Task NativeEnvironmentPrivacy()
    {
        using var fixture = new ToolCompositionFixture();
        var real = Core.Framework.Tools.Catalogue.ToolCatalogueFactory.Create(global::OpenForge.Cli.Composition.Shared.Tools.ToolHostFactory.CreateNodeRuntimes());
        var entry = real.Entries[Core.Framework.Tools.Catalogue.Shared.CodeGraph.CodeGraphDefinitions.Id];
        fixture.Catalogue = real with { Entries = new Dictionary<string, Core.Framework.Tools.Models.Catalogue.ToolCatalogueEntry> { [entry.Identity.Id] = entry } };
        fixture.EnsureCommands();
        fixture.DetectedVersion = entry.Profiles[0].MinimumToolVersion;
        fixture.EnvironmentValues[Core.Framework.Tools.Models.Configuration.ToolNativeConfigDefinitions.CodeGraphTelemetryVariable] = ToolCompositionFixture.Secret;
        var before = fixture.Snapshot();
        var run = await fixture.RunAsync(["tool", "inspect", entry.Identity.Id, "--format", "json", "--detail", "full"], fullRoot: true);
        using var json = run.Json();
        var tool = Assert.Single(ToolCompositionFixture.Field(ToolCompositionFixture.Field(json.RootElement, nameof(ToolWriteResult.Data)), nameof(ToolReadPayload.Tools)).EnumerateArray());
        var changes = ToolCompositionFixture.Field(tool, nameof(ToolReadTool.Changes));
        Assert.Contains(changes.EnumerateArray(), change => change.TryGetProperty(JsonNamingPolicy.CamelCase.ConvertName(nameof(ToolWriteChangeView.NativeEnvironmentOverrides)), out var overrides)
            && overrides.TryGetProperty(Core.Framework.Tools.Models.Configuration.ToolNativeConfigDefinitions.CodeGraphTelemetryVariable, out var effective)
            && effective.GetString() == Core.Framework.Tools.Models.Configuration.ToolNativeConfigDefinitions.EnabledValue);
        Assert.DoesNotContain(Strings(json.RootElement), value => value == ToolCompositionFixture.Secret);
        Assert.Equal(before, fixture.Snapshot());
    }

    private static IEnumerable<string?> Strings(JsonElement value)
    {
        if (value.ValueKind == JsonValueKind.String) yield return value.GetString();
        if (value.ValueKind == JsonValueKind.Object)
            foreach (var property in value.EnumerateObject())
                foreach (var text in Strings(property.Value)) yield return text;
        if (value.ValueKind == JsonValueKind.Array)
            foreach (var item in value.EnumerateArray())
                foreach (var text in Strings(item)) yield return text;
    }

}
