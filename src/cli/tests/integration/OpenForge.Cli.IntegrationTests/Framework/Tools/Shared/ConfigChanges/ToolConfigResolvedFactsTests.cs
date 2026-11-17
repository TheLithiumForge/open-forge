using System.Text.Json;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Records;

namespace OpenForge.Cli.IntegrationTests.Framework.Tools.Shared.ConfigChanges;

[Trait("Feature", "tool-config-resolved-facts"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class ToolConfigResolvedFactsTests
{
    private static CancellationToken Token => TestContext.Current.CancellationToken;

    [Theory(DisplayName = "Resolved JSON facts show custom-root values while apply expands the original recipe only once")]
    [InlineData(false), InlineData(true)]
    public async Task JsonFacts(bool union)
    {
        using var scenario = new ToolServiceScenario();
        const string target = "${unknown}.json";
        scenario.Files.ConfigurePermissions("knowledge", target);
        scenario.Context = scenario.Context with
        {
            MainRoot = "knowledge"
        };
        var change = ToolServiceScenario.Set("value", ToolRecipeDefinitions.DollarEscapeToken + "{unknown}.json", "null") with
        {
            Operation = union ? ToolJsonValueOperation.Union : ToolJsonValueOperation.SetIfAbsent,
            InitialState = union ? ToolInitialConfigState.JsonArrayOrMissing : ToolInitialConfigState.AbsentMember,
            Value = Value(union)
        };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change), Token);
        var observation = Assert.Single(plan.Changes);
        Assert.Same(change, observation.Change);
        var resolved = Assert.IsType<ToolJsonValueChange>(observation.ResolvedChange);
        Assert.Equal(target, resolved.PathTemplate);
        var desired = resolved.Value ?? throw new InvalidOperationException();
        Assert.Equal(scenario.Context.MainRoot + "/synthetic", union ? desired[0].GetString() : desired.GetString());
        if (union) Assert.Equal("${unknown}", desired[1].GetString());
        Assert.False(File.Exists(scenario.Files.Combine(target)));
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        using var current = JsonDocument.Parse(await File.ReadAllBytesAsync(scenario.Files.Combine(target), Token));
        Assert.True(JsonElement.DeepEquals(desired, current.RootElement.GetProperty("value")));
    }

    [Fact(DisplayName = "Resolved marked block facts contain only managed content and literal markers")]
    public async Task BlockFacts()
    {
        using var scenario = new ToolServiceScenario();
        const string target = "notes.txt";
        scenario.Files.Grant(target);
        scenario.Files.WriteText(target, "synthetic unrelated private content\n");
        var before = await File.ReadAllBytesAsync(scenario.Files.Combine(target), Token);
        var change = new ToolMarkedBlockChange
        {
            Id = "block",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.UniqueMarkedBlock,
            Description = "synthetic managed block",
            PathTemplate = target,
            StartMarker = $"# synthetic {ToolRecipeDefinitions.MainRootToken} start",
            EndMarker = $"# synthetic {ToolRecipeDefinitions.MainRootToken} end",
            ContentTemplate = ToolRecipeDefinitions.MainRootToken + "/synthetic\n" + ToolRecipeDefinitions.DollarEscapeToken + "{unknown}"
        };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change), Token);
        var observation = Assert.Single(plan.Changes);
        var resolved = Assert.IsType<ToolMarkedBlockChange>(observation.ResolvedChange);
        Assert.Same(change, observation.Change);
        Assert.Equal(scenario.Context.MainRoot + "/synthetic\n${unknown}", resolved.ContentTemplate);
        Assert.Equal(change.StartMarker, resolved.StartMarker);
        Assert.Equal(change.EndMarker, resolved.EndMarker);
        Assert.Equal(before, await File.ReadAllBytesAsync(scenario.Files.Combine(target), Token));
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        Assert.True((await File.ReadAllBytesAsync(scenario.Files.Combine(target), Token)).AsSpan().EndsWith(before));
    }

    [Fact(DisplayName = "Managed-file plans expose the resolved destination and a hash of only the new template bytes")]
    public async Task ManagedFileFacts()
    {
        using var scenario = new ToolServiceScenario();
        var target = scenario.Context.MainRoot + ".txt";
        scenario.Files.Grant(target);
        var change = new ToolManagedFileChange
        {
            Id = "template",
            Scope = ToolEffectScope.Workspace,
            InitialState = ToolInitialConfigState.AbsentFile,
            Description = "synthetic template",
            PathTemplate = ToolRecipeDefinitions.MainRootToken + ".txt",
            TemplateResource = "synthetic-resource"
        };
        var plan = await scenario.Service.PlanAsync(scenario.Request(change), Token);
        var observation = Assert.Single(plan.Changes);
        Assert.Same(change, observation.Change);
        var resolved = Assert.IsType<ToolManagedFileChange>(observation.ResolvedChange);
        Assert.Equal(target, resolved.PathTemplate);
        Assert.Equal(change.TemplateResource, resolved.TemplateResource);
        var desired = Assert.IsType<byte[]>(observation.Desired.Bytes);
        var expected = System.Text.Encoding.UTF8.GetBytes(scenario.Context.MainRoot + "\n");
        Assert.Equal(expected, desired);
        var result = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(result.Failure);
        var receipt = Assert.IsType<ToolManagedFileAddition>(Assert.Single(result.RemainingAdditions));
        Assert.Equal(FileExpectation.Hash(desired), receipt.Sha256);
    }

    [Fact(DisplayName = "Resolved native command vectors and stored paths match their normal invocation")]
    public async Task NativeFacts()
    {
        using var scenario = new ToolServiceScenario();
        scenario.WriteNative(enabled: true);
        var change = ToolServiceScenario.Native();
        var plan = await scenario.Service.PlanAsync(scenario.Request(change), Token);
        var observation = Assert.Single(plan.Changes);
        Assert.Same(change, observation.Change);
        var resolved = Assert.IsType<ToolNativeConfigChange>(observation.ResolvedChange);
        Assert.NotSame(change, resolved);
        Assert.Equal(change.ApplyArguments, resolved.ApplyArguments);
        Assert.Equal(change.ReadArguments, resolved.ReadArguments);
        Assert.Equal(scenario.NativeFile, resolved.StoredValue?.PathTemplate);
        Assert.Equal(new[] { change.ApplyArguments[0], ToolNativeConfigDefinitions.EnabledValue }, resolved.ManualRestoreArgumentsTemplate);
        scenario.Run = _ => { scenario.WriteNative(enabled: false); return ToolServiceScenario.Success(); };
        var applied = await scenario.Service.ApplyAsync(plan, Token);
        Assert.Null(applied.Failure);
        Assert.Equal(scenario.Invocation.PrefixArguments.Concat(resolved.ApplyArguments), Assert.Single(scenario.Calls).Arguments);
        Assert.Empty(scenario.Calls[0].Environment);
        Assert.Equal(ToolNativeConfigDefinitions.EnabledValue, Assert.Single(applied.NativeSettings).Previous.Native);
    }

    private static JsonElement Value(bool union)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            if (union) writer.WriteStartArray();
            writer.WriteStringValue(ToolRecipeDefinitions.MainRootToken + "/synthetic");
            if (union)
            {
                writer.WriteStringValue(ToolRecipeDefinitions.DollarEscapeToken + "{unknown}");
                writer.WriteEndArray();
            }
        }
        using var document = JsonDocument.Parse(stream.ToArray());
        return document.RootElement.Clone();
    }
}
