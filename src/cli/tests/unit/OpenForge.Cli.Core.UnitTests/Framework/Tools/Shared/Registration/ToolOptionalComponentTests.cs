using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolOptionalComponentTests
{
    [Theory(DisplayName = "Decline removes an exact object member while keeping its sibling members")]
    [InlineData("a"), InlineData("b"), InlineData("c")]
    public void RemovesObjectEntry(string key)
    {
        const string file = "fixture-object.json";
        var root = JsonNode.Parse("{\"hooks\":{\"a\":{\"id\":1},\"b\":{\"id\":2},\"c\":{\"id\":3}}}") ?? throw new InvalidDataException();
        var before = ToolEntryCase.Bytes(root.ToJsonString());
        using var document = new ToolJsonEntries(before);
        var component = Assert.Single(ToolEntryCase.Definition().Components);
        var seed = component.DeclinedEntries[0];
        component = component with
        {
            DeclinedEntries = [seed with
        { File = file, Pointer = "/hooks/" + key, Expected = Assert.IsType<JsonElement>(document.Value("/hooks/" + key)).Clone() }]
        };
        var request = ToolEntryCase.Request() with { DeclinedComponents = [component] };
        var files = new Dictionary<string, byte[]?> { [file] = before };
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, files).Edits);
        root["hooks"]?.AsObject().Remove(key);
        using var expected = JsonDocument.Parse(root.ToJsonString());
        using var actual = JsonDocument.Parse(edit.After);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
        files[file] = edit.After;
        Assert.Single(new ToolEntryVerifier().Verify(request, files).DeclinedComponents);
    }

    [Fact(DisplayName = "Decline skips the linked rewrite, removes one exact hook and keeps its group and sibling hooks")]
    public void RemovesOnlyDeclaredHook()
    {
        var definition = ToolEntryCase.Definition();
        var hook = definition.Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var component = Assert.Single(definition.Components);
        var request = ToolEntryCase.Request(hook) with { DeclinedComponents = [component] };
        var before = ToolEntryCase.Original(hook);
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, ToolEntryCase.Files(hook, before)).Edits);
        using var prior = new ToolJsonEntries(before);
        using var after = new ToolJsonEntries(edit.After);
        var removal = Assert.Single(component.DeclinedEntries);
        var parent = ToolJsonEntries.Parent(removal.Pointer);
        Assert.Equal(0, after.Value(parent)?.GetArrayLength());
        Assert.NotNull(after.Value(ToolJsonEntries.Parent(parent)));
        Assert.True(JsonElement.DeepEquals(Assert.IsType<JsonElement>(prior.Value("/hooks/UserPromptSubmit/1")), Assert.IsType<JsonElement>(after.Value("/hooks/UserPromptSubmit/1"))));
        Assert.True(JsonElement.DeepEquals(Assert.IsType<JsonElement>(prior.Value("/hooks/SessionStart")), Assert.IsType<JsonElement>(after.Value("/hooks/SessionStart"))));
        var verified = new ToolEntryVerifier().Verify(request, ToolEntryCase.Files(hook, edit.After));
        Assert.Empty(verified.Rewrites);
        var receipt = Assert.Single(verified.DeclinedComponents);
        Assert.Equal(component.Id, receipt.Id);
        Assert.Equal(removal.Pointer, Assert.Single(receipt.AbsentEntries).Pointer);
        Assert.Equal(ToolEntryState.Declined, Assert.Single(verified.Checks).State);
        Assert.Empty(new ToolEntryPlanner().Plan(request, ToolEntryCase.Files(hook, edit.After)).Edits);
    }

    [Theory(DisplayName = "Decline checks every member of the original entry and refuses missing or ambiguous entries")]
    [InlineData("changed-command"), InlineData("changed-type"), InlineData("extra-member"), InlineData("missing-member")]
    [InlineData("duplicate-entry"), InlineData("missing-entry"), InlineData("duplicate-selector")]
    public void RefusesChangedDecline(string situation)
    {
        var definition = ToolEntryCase.Definition();
        var hook = definition.Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var component = Assert.Single(definition.Components);
        var root = JsonNode.Parse(ToolEntryCase.Original(hook))?.AsObject() ?? throw new InvalidDataException();
        var hooks = root["hooks"]?["UserPromptSubmit"]?[0]?["hooks"]?.AsArray() ?? throw new InvalidDataException();
        var entry = hooks[0]?.AsObject() ?? throw new InvalidDataException();
        switch (situation)
        {
            case "changed-command": entry[ToolJsonEntries.Command] = "fixture-changed"; break;
            case "changed-type": entry["type"] = "fixture-changed"; break;
            case "extra-member": entry["fixture-extra"] = true; break;
            case "missing-member": entry.Remove("type"); break;
            case "duplicate-entry": hooks.Add(entry.DeepClone()); break;
            case "missing-entry": hooks.Clear(); break;
            case "duplicate-selector": component = component with { DeclinedEntries = [component.DeclinedEntries[0], component.DeclinedEntries[0]] }; break;
        }
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(hook) with { DeclinedComponents = [component] },
            ToolEntryCase.Files(hook, ToolEntryCase.Bytes(root.ToJsonString())));
        Assert.Empty(plan.Edits);
        Assert.All(plan.Checks, check => Assert.Equal(ToolEntryState.Drifted, check.State));
    }

    [Theory(DisplayName = "Removing exact array entries keeps shifted siblings and verifies the expected absence")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3), InlineData(4), InlineData(5), InlineData(6)]
    public void RemovesAdjacentAndSeparatedEntries(int situation)
    {
        const string file = "fixture-array.json";
        var root = JsonNode.Parse("{\"hooks\":[{\"id\":\"a\"},{\"id\":\"b\"},{\"id\":\"c\"}]}") ?? throw new InvalidDataException();
        using var document = new ToolJsonEntries(ToolEntryCase.Bytes(root.ToJsonString()));
        int[][] selections = [[0], [1], [2], [0, 1], [1, 2], [0, 2], [0, 1, 2]];
        var indices = selections[situation];
        var component = Assert.Single(ToolEntryCase.Definition().Components) with
        {
            DeclinedEntries = [.. indices.Select(index => new ToolJsonEntryRemoval
            {
                File = file, Host = Assert.Single(ToolEntryCase.Definition().Components).DeclinedEntries[0].Host,
                Pointer = "/hooks/" + index, Expected = document.Value("/hooks/" + index)?.Clone() ?? throw new InvalidDataException(),
            })],
        };
        var request = ToolEntryCase.Request() with { DeclinedComponents = [component] };
        var files = new Dictionary<string, byte[]?> { [file] = ToolEntryCase.Bytes(root.ToJsonString()) };
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, files).Edits);
        using var after = new ToolJsonEntries(edit.After);
        var expected = root["hooks"]?.AsArray().Where((_, index) => !indices.Contains(index)).Select(node => node?.DeepClone()).ToArray()
            ?? throw new InvalidDataException();
        using var retained = JsonDocument.Parse(new JsonArray(expected).ToJsonString());
        Assert.True(JsonElement.DeepEquals(retained.RootElement, Assert.IsType<JsonElement>(after.Value("/hooks"))));
        files[file] = edit.After;
        Assert.Single(new ToolEntryVerifier().Verify(request, files).DeclinedComponents);
        Assert.All(new ToolEntryPlanner().Observe(request, files), check => Assert.Equal(ToolEntryState.Declined, check.State));
    }

    [Fact(DisplayName = "A declined component without any rewrite still produces an absence receipt")]
    public void ReceiptsNeedNoLinkedRewrite()
    {
        var definition = ToolEntryCase.Definition();
        var hook = definition.Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var request = ToolEntryCase.Request() with { DeclinedComponents = definition.Components };
        var files = ToolEntryCase.Files(hook, ToolEntryCase.Original(hook));
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, files).Edits);
        files[hook.File] = edit.After;
        Assert.Single(new ToolEntryVerifier().Verify(request, files).DeclinedComponents);
        files[hook.File] = null;
        Assert.Equal(ToolEntryState.Declined, Assert.Single(new ToolEntryPlanner().Observe(request, files)).State);
    }

    [Theory(DisplayName = "Supplied component choices override saved choices and unresolved choices keep their prompt metadata")]
    [InlineData(null, null), InlineData(null, false), InlineData(null, true), InlineData(false, true), InlineData(true, false)]
    public void ResolvesChoices(bool? supplied, bool? saved)
    {
        var components = ToolEntryCase.Definition().Components;
        var component = Assert.Single(components);
        var suppliedChoices = new Dictionary<string, bool>();
        var savedChoices = new Dictionary<string, bool>();
        if (supplied is { } input) suppliedChoices.Add(component.Id, input);
        if (saved is { } recorded) savedChoices.Add(component.Id, recorded);
        var result = ToolOptionalComponents.Resolve(components, suppliedChoices, savedChoices);
        if (supplied is not null || saved is not null)
        {
            Assert.Empty(result.Missing);
            Assert.Equal(supplied ?? saved, result.Choices[component.Id]);
        }
        else
        {
            Assert.Empty(result.Choices);
            Assert.Same(component, Assert.Single(result.Missing));
            Assert.True(result.Missing[0].DefaultEnabled);
            Assert.False(string.IsNullOrWhiteSpace(result.Missing[0].Description));
        }
    }

    [Theory(DisplayName = "The finite prompt-hook flag accepts only yes and no")]
    [InlineData(ToolOptionalComponents.Yes, true), InlineData(ToolOptionalComponents.No, false)]
    public void ParsesFiniteChoice(string value, bool expected) => Assert.Equal(expected, ToolOptionalComponents.ParsePromptHook(value));

    [Theory(DisplayName = "Arbitrary component flags and unknown choices are refused")]
    [InlineData("flag"), InlineData("choice"), InlineData("value"), InlineData("id"), InlineData("description")]
    public void RejectsUnknownComponentInput(string situation)
    {
        var components = ToolEntryCase.Definition().Components;
        if (situation == "value") { Assert.Throws<ArgumentException>(() => ToolOptionalComponents.ParsePromptHook("true")); return; }
        if (situation == "flag") components = [components[0] with { Flag = "fixture-arbitrary" }];
        if (situation == "id") components = [components[0] with { Id = "../fixture" }];
        if (situation == "description") components = [components[0] with { Description = string.Empty }];
        var choices = situation == "choice" ? new Dictionary<string, bool> { ["fixture-unknown"] = true } : [];
        if (situation == "choice") Assert.Throws<ArgumentException>(() => ToolOptionalComponents.Resolve(components, choices, new Dictionary<string, bool>()));
        else Assert.Throws<InvalidDataException>(() => ToolOptionalComponents.Resolve(components, choices, new Dictionary<string, bool>()));
    }
}
