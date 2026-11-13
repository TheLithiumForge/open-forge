using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;
using OpenForge.Cli.TestSupport.Tools;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolHookSelectionTests
{
    public static IEnumerable<object[]> Scenarios() => ToolVocabulary.Targets.Order(StringComparer.Ordinal)
        .SelectMany(target => new[] { false, true }.Select(shared => new object[] { target, shared }));
    private static string Target => ToolVocabulary.Targets.Order(StringComparer.Ordinal).First();

    [Theory(DisplayName = "Each OS family can declare its own replacement syntax")]
    [InlineData(ToolHookPlatforms.Windows, "win-", "\"${openForge}\"")]
    [InlineData(ToolHookPlatforms.MacOS, "osx-", "'${openForge}'")]
    [InlineData(ToolHookPlatforms.Linux, "linux-", "${openForge}")]
    public void UsesPlatformReplacement(string platform, string prefix, string executable)
    {
        var target = ToolVocabulary.Targets.First(target => target.StartsWith(prefix, StringComparison.Ordinal));
        var hook = ToolHookCase.Hook;
        var original = platform == ToolHookPlatforms.Windows ? ToolRegistrationFixtureSource.WindowsHookCommand : hook.Expected;
        var replacement = executable + " tool run " + ToolRegistrationFixtureSource.UnixHookCommand;
        hook = hook with { PlatformOverrides = new() { [platform] = new(original, replacement) } };
        var request = ToolHookCase.Request(target) with { Rewrites = [hook] };
        var files = ToolHookCase.Files(ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, target));
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, files).Edits);
        files[hook.File] = edit.After;
        var resolved = Assert.IsType<ToolHookCommandRewrite>(Assert.Single(new ToolEntryVerifier().Verify(request, files).Rewrites).Rewrite);
        Assert.Equal(replacement.Replace("${openForge}", ToolEntryDeclarations.PathCommand, StringComparison.Ordinal), resolved.Replacement);
    }

    [Fact(DisplayName = "Exact event selection keeps substring matches and matching commands in another event")]
    public void LimitsSelectionToItsEvent()
    {
        var hook = ToolHookCase.Hook;
        var root = JsonNode.Parse(ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, Target))?.AsObject() ?? throw new InvalidDataException();
        var user = Event(root)[0]?[ToolHookEntries.Hooks]?[0] ?? throw new InvalidDataException();
        user[ToolJsonEntries.Command] = user[ToolJsonEntries.Command]?.GetValue<string>() + " " + hook.Expected;
        var session = root[ToolHookEntries.Hooks]?["SessionStart"]?[0]?[ToolHookEntries.Hooks]?.AsArray() ?? throw new InvalidDataException();
        session.Add(JsonNode.Parse("{\"type\":\"command\",\"command\":\"" + hook.Expected + "\"}"));
        var before = ToolEntryCase.Bytes(root.ToJsonString());
        var edit = Assert.Single(new ToolEntryPlanner().Plan(ToolHookCase.Request(Target), ToolHookCase.Files(before)).Edits);
        foreach (var scope in new[] { hook.Pointer + "/0", "/" + ToolHookEntries.Hooks + "/SessionStart" })
        {
            using var expected = new ToolJsonEntries(before);
            using var after = new ToolJsonEntries(edit.After);
            Assert.True(JsonElement.DeepEquals(Assert.IsType<JsonElement>(expected.Value(scope)), Assert.IsType<JsonElement>(after.Value(scope))));
        }
    }

    [Theory(DisplayName = "Event hooks rewrite, decline and remove without losing pre-existing user commands"), MemberData(nameof(Scenarios))]
    public void KeepsUserHooks(string target, bool shared)
    {
        var request = ToolHookCase.Request(target);
        var resource = shared ? ToolRegistrationFixtureSource.SharedHookGroup : ToolRegistrationFixtureSource.CreatedHookGroup;
        var files = ToolHookCase.Files(ToolHookCase.Output(resource, target));
        var planner = new ToolEntryPlanner();
        var edit = Assert.Single(planner.Plan(request, files).Edits);
        files[edit.File] = edit.After;
        var verification = new ToolEntryVerifier().Verify(request, files);
        var receipt = Assert.Single(verification.Rewrites);
        var hook = Assert.IsType<ToolHookCommandRewrite>(receipt.Rewrite);
        Assert.Null(hook.PlatformOverrides);
        Assert.True(hook.Replacement.Contains(hook.RequiredReplacementSubstring ?? throw new InvalidDataException(), StringComparison.Ordinal));
        Assert.Equal(!shared, receipt.CreatedHookGroup is not null);
        using (var after = new ToolJsonEntries(edit.After)) Assert.NotNull(ToolHookEntries.Find(after, hook.Pointer, hook.Replacement));
        var removal = ToolHookCase.Removal(receipt, target);
        var removed = Assert.Single(planner.Plan(removal, files).Edits);
        AssertUserDocument(removed.After);
        var declined = ToolHookCase.Request(target, decline: true);
        var decline = Assert.Single(planner.Plan(declined, ToolHookCase.Files(ToolHookCase.Output(resource, target))).Edits);
        AssertUserDocument(decline.After);
        var absence = new ToolEntryVerifier().Verify(declined, ToolHookCase.Files(decline.After));
        Assert.Single(absence.DeclinedComponents);
        var restoredReplacement = ToolHookCase.Files(edit.After);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(planner.Observe(declined, restoredReplacement)).State);
    }

    [Theory(DisplayName = "An empty group stays when its creation is unknown or the group pre-existed")]
    [InlineData(false), InlineData(true)]
    public void KeepsPreExistingEmptyGroup(bool priorExists)
    {
        var bytes = ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, Target);
        var request = ToolHookCase.Request(Target, decline: true) with { BeforeInstallerFiles = null };
        var expected = JsonNode.Parse(bytes)?.AsObject() ?? throw new InvalidDataException();
        var groups = Event(expected);
        groups[^1]?[ToolHookEntries.Hooks]?.AsArray().Clear();
        if (priorExists) request = request with { BeforeInstallerFiles = new Dictionary<string, byte[]?> { [ToolHookCase.Hook.File] = ToolEntryCase.Bytes(expected.ToJsonString()) } };
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, ToolHookCase.Files(bytes)).Edits);
        using var desired = JsonDocument.Parse(expected.ToJsonString());
        using var actual = JsonDocument.Parse(edit.After);
        Assert.True(JsonElement.DeepEquals(desired.RootElement, actual.RootElement));
    }

    [Fact(DisplayName = "A saved creation receipt cannot remove an ambiguous group")]
    public void RefusesAmbiguousCreatedGroup()
    {
        var request = ToolHookCase.Request(Target);
        var planner = new ToolEntryPlanner();
        var files = ToolHookCase.Files(ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, Target));
        var edit = Assert.Single(planner.Plan(request, files).Edits);
        files[edit.File] = edit.After;
        var receipt = Assert.Single(new ToolEntryVerifier().Verify(request, files).Rewrites);
        var root = JsonNode.Parse(edit.After)?.AsObject() ?? throw new InvalidDataException();
        Event(root).Add(JsonNode.Parse((receipt.CreatedHookGroup ?? throw new InvalidDataException()).EmptyGroup.GetRawText()));
        var plan = planner.Plan(ToolHookCase.Removal(receipt, Target), ToolHookCase.Files(ToolEntryCase.Bytes(root.ToJsonString())));
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(plan.Checks).State);
    }

    [Theory(DisplayName = "Malformed hook parent settings drift instead of confirming absence")]
    [InlineData("[]"), InlineData("{\"hooks\":[]}"), InlineData("{\"hooks\":null}")]
    public void RefusesMalformedAbsence(string json)
    {
        var checks = new ToolEntryPlanner().Observe(ToolHookCase.Request(Target, decline: true), ToolHookCase.Files(ToolEntryCase.Bytes(json)));
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(checks).State);
    }

    [Theory(DisplayName = "Hook command matches are independent of group and command array positions")]
    [InlineData(false), InlineData(true)]
    public void FindsMovedEntry(bool shared)
    {
        var resource = shared ? ToolRegistrationFixtureSource.SharedHookGroup : ToolRegistrationFixtureSource.CreatedHookGroup;
        var root = JsonNode.Parse(ToolHookCase.Output(resource, Target))?.AsObject() ?? throw new InvalidDataException();
        var groups = Event(root);
        var moved = groups.Select(group => group?.DeepClone()).Reverse().ToArray();
        groups.Clear();
        foreach (var group in moved)
        {
            var hooks = group?[ToolHookEntries.Hooks]?.AsArray() ?? throw new InvalidDataException();
            var reversed = hooks.Select(hook => hook?.DeepClone()).Reverse().ToArray();
            hooks.Clear();
            foreach (var hook in reversed) hooks.Add(hook);
            groups.Add(group);
        }
        var request = ToolHookCase.Request(Target);
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, ToolHookCase.Files(ToolEntryCase.Bytes(root.ToJsonString()))).Edits);
        Assert.Equal(ToolEntryState.Matching, Assert.Single(new ToolEntryPlanner().Observe(request, ToolHookCase.Files(edit.After))).State);
    }

    [Theory(DisplayName = "Ambiguous, changed and unexpected hook event shapes refuse the whole edit")]
    [InlineData("duplicate-command"), InlineData("duplicate-group"), InlineData("changed-entry"), InlineData("wrong-type")]
    [InlineData("wrong-event"), InlineData("wrong-group"), InlineData("non-string-command"), InlineData("missing-command")]
    public void RefusesUncertainEvent(string situation)
    {
        var root = JsonNode.Parse(ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, Target))?.AsObject() ?? throw new InvalidDataException();
        var groups = Event(root);
        var group = groups[^1]?.AsObject() ?? throw new InvalidDataException();
        var hooks = group[ToolHookEntries.Hooks]?.AsArray() ?? throw new InvalidDataException();
        var hook = hooks[0]?.AsObject() ?? throw new InvalidDataException();
        switch (situation)
        {
            case "duplicate-command": hooks.Add(hook.DeepClone()); break;
            case "duplicate-group": groups.Add(group.DeepClone()); break;
            case "changed-entry": hook["fixture-extra"] = true; break;
            case "wrong-type": hook[ToolHookEntries.Type] = "fixture-other"; break;
            case "wrong-event": root[ToolHookEntries.Hooks]?["UserPromptSubmit"] = new JsonObject(); break;
            case "wrong-group": group[ToolHookEntries.Hooks] = true; break;
            case "non-string-command": hook[ToolJsonEntries.Command] = 7; break;
            case "missing-command": hooks.Clear(); break;
        }
        var request = ToolHookCase.Request(Target, decline: true);
        var plan = new ToolEntryPlanner().Plan(request, ToolHookCase.Files(ToolEntryCase.Bytes(root.ToJsonString())));
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(plan.Checks).State);
    }

    [Theory(DisplayName = "Missing targets, unknown platforms and lost uninstaller tokens are invalid declarations")]
    [InlineData("missing-target"), InlineData("unknown-platform"), InlineData("lost-token")]
    public void RefusesInvalidPlatform(string situation)
    {
        var request = ToolHookCase.Request(Target);
        var hook = ToolHookCase.Hook;
        if (situation == "missing-target") request = request with { Values = new Dictionary<string, string> { [ToolVocabulary.OpenForgeVariable] = ToolEntryDeclarations.PathCommand } };
        if (situation == "unknown-platform") hook = hook with { PlatformOverrides = new() { ["fixture-unknown"] = new(hook.Expected, hook.Replacement) } };
        if (situation == "lost-token") hook = hook with { Replacement = "fixture-lost", PlatformOverrides = null };
        request = request with { Rewrites = [hook] };
        Assert.Throws<InvalidDataException>(() => new ToolEntryPlanner().Plan(request, ToolHookCase.Files(ToolHookCase.Output(ToolRegistrationFixtureSource.CreatedHookGroup, Target))));
    }

    private static JsonArray Event(JsonObject root) => root[ToolHookEntries.Hooks]?["UserPromptSubmit"]?.AsArray() ?? throw new InvalidDataException();
    private static void AssertUserDocument(byte[] bytes)
    {
        using var expected = JsonDocument.Parse(ToolRegistrationFixtureSource.Read(ToolRegistrationFixtureSource.UserHooks));
        using var actual = JsonDocument.Parse(bytes);
        Assert.True(JsonElement.DeepEquals(expected.RootElement, actual.RootElement));
    }
}
