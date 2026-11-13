using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolRegistrationRefusalTests
{
    [Theory(DisplayName = "Hook rewrites require the exact string at one exact Pointer")]
    [InlineData("changed"), InlineData("missing"), InlineData("non-string"), InlineData("duplicate-key"), InlineData("array-alias")]
    public void RefusesUnexpectedHook(string situation)
    {
        var hook = ToolEntryCase.Definition().Rewrites.OfType<ToolHookCommandRewrite>().Single();
        var text = ToolEntryCase.Text(ToolEntryCase.Original(hook));
        text = situation switch
        {
            "changed" => text.Replace(hook.Expected, hook.Expected + " fixture-extra", StringComparison.Ordinal),
            "missing" => text.Replace("\"command\": \"" + hook.Expected + "\"", "\"fixture-other\": true", StringComparison.Ordinal),
            "non-string" => text.Replace("\"" + hook.Expected + "\"", "7", StringComparison.Ordinal),
            "duplicate-key" => text.Replace("\"command\": \"" + hook.Expected + "\"", "\"command\": \"" + hook.Expected + "\", \"command\": \"" + hook.Expected + "\"", StringComparison.Ordinal),
            "array-alias" => text,
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        if (situation == "array-alias") hook = hook with { Pointer = hook.Pointer.Replace("/0/", "/00/", StringComparison.Ordinal) };
        var request = ToolEntryCase.Request(hook);
        var files = ToolEntryCase.Files(hook, ToolEntryCase.Bytes(text));
        var planner = new ToolEntryPlanner();
        Assert.Empty(planner.Plan(request, files).Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(planner.Observe(request, files)).State);
    }

    [Theory(DisplayName = "Invalid identities, path aliases and unresolved launch values are refused before planning")]
    [InlineData("duplicate-id"), InlineData("bad-host"), InlineData("bad-file"), InlineData("path-alias")]
    [InlineData("relative-launch"), InlineData("missing-launch"), InlineData("unexpanded-launch"), InlineData("unknown-variable")]
    public void RefusesInvalidDeclarations(string situation)
    {
        var json = ToolEntryCase.Definition().Rewrites.OfType<ToolJsonCommandRewrite>().Single();
        var request = ToolEntryCase.Request(json);
        request = situation switch
        {
            "duplicate-id" => request with { Rewrites = [json, json] },
            "bad-host" => request with { Rewrites = [json with { Host = "fixture-unknown" }] },
            "bad-file" => request with { Rewrites = [json with { File = "../fixture.json" }] },
            "path-alias" => request with { Rewrites = [json, json with { Id = json.Id + "-second", File = json.File.ToUpperInvariant() }] },
            "relative-launch" => request with { Values = new Dictionary<string, string> { [ToolVocabulary.OpenForgeVariable] = "fixture/bin" } },
            "missing-launch" => request with { Values = new Dictionary<string, string>() },
            "unexpanded-launch" => request with { Values = new Dictionary<string, string> { [ToolVocabulary.OpenForgeVariable] = Path.GetFullPath("${fixture}") } },
            "unknown-variable" => request with { Rewrites = [json with { Replacement = json.Replacement with { Command = "${fixture}" } }] },
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        var planner = new ToolEntryPlanner();
        var files = ToolEntryCase.Files(json, ToolEntryCase.Original(json));
        if (situation == "missing-launch") Assert.Throws<InvalidOperationException>(() => planner.Plan(request, files));
        else if (situation == "unknown-variable") Assert.Throws<System.Text.Json.JsonException>(() => planner.Plan(request, files));
        else Assert.Throws<InvalidDataException>(() => planner.Plan(request, files));
    }

    [Fact(DisplayName = "Overlapping declined entries and retained rewrites stop the whole plan")]
    public void RefusesRemovalOverlap()
    {
        var definition = ToolEntryCase.Definition();
        var hook = definition.Rewrites.OfType<ToolHookCommandRewrite>().Single() with { Component = null };
        var request = ToolEntryCase.Request(hook) with { DeclinedComponents = definition.Components };
        var plan = new ToolEntryPlanner().Plan(request, ToolEntryCase.Files(hook, ToolEntryCase.Original(hook)));
        Assert.Empty(plan.Edits);
        Assert.All(plan.Checks, check => Assert.Equal(ToolEntryState.Drifted, check.State));
    }

    [Fact(DisplayName = "TOML command-line whitespace and comments stay byte-for-byte outside the changed values")]
    public void KeepsTomlLineFormatting()
    {
        var toml = ToolEntryCase.Definition().Rewrites.OfType<ToolTomlCommandRewrite>().Single();
        var text = ToolEntryCase.Text(ToolEntryCase.Original(toml));
        var command = "command = \"" + toml.Expected.Command + "\"";
        const string args = "args = [\"serve\", \"--mcp\"]";
        text = text.Replace(command, "  command  =  \"" + toml.Expected.Command + "\" # fixture command", StringComparison.Ordinal)
            .Replace(args, "\targs = [ \"serve\", \"--mcp\" ] # fixture args", StringComparison.Ordinal);
        var edit = Assert.Single(new ToolEntryPlanner().Plan(ToolEntryCase.Request(toml), ToolEntryCase.Files(toml, ToolEntryCase.Bytes(text))).Edits);
        var expected = text.Replace("\"" + toml.Expected.Command + "\" # fixture command", "\"" + ToolEntryDeclarations.PathCommand + "\" # fixture command", StringComparison.Ordinal)
            .Replace("[ \"serve\", \"--mcp\" ] # fixture args", "[\"tool\", \"run\", \"codegraph\", \"serve\", \"--mcp\"] # fixture args", StringComparison.Ordinal);
        Assert.Equal(ToolEntryCase.Bytes(expected), edit.After);
    }

    [Theory(DisplayName = "TOML text selection refuses hidden table headers and escaped aliases")]
    [InlineData(false), InlineData(true)]
    public void RefusesAmbiguousTomlText(bool escaped)
    {
        var toml = ToolEntryCase.Definition().Rewrites.OfType<ToolTomlCommandRewrite>().Single();
        var text = ToolEntryCase.Text(ToolEntryCase.Original(toml));
        text = escaped ? text + "\n[\"mcp_servers\".\"code\\u0067raph\"]\n"
            : "fixture = \"\"\"\n" + text + "\"\"\"\n";
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(toml), ToolEntryCase.Files(toml, ToolEntryCase.Bytes(text)));
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(plan.Checks).State);
    }
}
