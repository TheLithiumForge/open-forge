using System.Text.Json;
using System.Text.Json.Nodes;
using OpenForge.Cli.Core.Framework.Tools;
using OpenForge.Cli.Core.Framework.Tools.Models.Registration;
using OpenForge.Cli.Core.Framework.Tools.Shared.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Registration;

[Trait("Feature", "tool-registration"), Trait("Evidence", "Unit")]
public sealed class ToolEntryPlannerTests
{
    [Theory(DisplayName = "Every rewrite form verifies resolved receipts and refuses an already rewritten original")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public void RewritesExactOriginal(int form)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites[form];
        var before = ToolEntryCase.Original(rewrite);
        var request = ToolEntryCase.Request(rewrite);
        var planner = new ToolEntryPlanner();
        var edit = Assert.Single(planner.Plan(request, ToolEntryCase.Files(rewrite, before)).Edits);
        Assert.Equal(before, edit.Before);
        Assert.Equal(rewrite.Id, Assert.Single(edit.EntryIds));
        var after = ToolEntryCase.Files(rewrite, edit.After);
        var verified = new ToolEntryVerifier().Verify(request, after);
        Assert.Equal(ToolEntryState.Matching, Assert.Single(verified.Checks).State);
        Assert.False(Assert.Single(verified.Rewrites).Declined);
        Assert.Empty(verified.DeclinedComponents);
        Assert.Empty(planner.Plan(request, after).Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(planner.Plan(request, after).Checks).State);
        Assert.Equal(before, ToolEntryCase.Original(rewrite));
    }

    [Theory(DisplayName = "All rewrite forms keep UTF-8 BOM and CRLF bytes outside selected values")]
    [InlineData(0), InlineData(1), InlineData(2), InlineData(3)]
    public void KeepsEncodingAndUnrelatedBytes(int form)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites[form];
        var before = ToolEntryCase.Bytes("\uFEFF" + ToolEntryCase.Text(ToolEntryCase.Original(rewrite)).ReplaceLineEndings("\r\n"));
        var edit = Assert.Single(new ToolEntryPlanner().Plan(ToolEntryCase.Request(rewrite), ToolEntryCase.Files(rewrite, before)).Edits);
        Assert.Equal(before[..3], edit.After[..3]);
        using var jsonEntries = rewrite is ToolJsonCommandRewrite or ToolHookCommandRewrite ? new ToolJsonEntries(before) : null;
        var spans = rewrite switch
        {
            ToolJsonCommandRewrite json when jsonEntries is not null => jsonEntries.Rewrite(json with { Replacement = new(ToolEntryDeclarations.PathCommand, json.Replacement.Args) }),
            ToolTomlCommandRewrite toml => ToolTextEntries.Rewrite(before, toml with { Replacement = new(ToolEntryDeclarations.PathCommand, toml.Replacement.Args) }),
            ToolHookCommandRewrite hook when jsonEntries is not null => [jsonEntries.Rewrite(hook with { Replacement = hook.Replacement.Replace("${openForge}", ToolEntryDeclarations.PathCommand, StringComparison.Ordinal) })],
            ToolMarkedLineRewrite line => [ToolTextEntries.Rewrite(before, line with { Replacement = line.Replacement.Replace("${openForge}", ToolEntryDeclarations.PathCommand, StringComparison.Ordinal) })],
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };
        var originalOffset = 0;
        var outputOffset = 0;
        foreach (var span in spans.OrderBy(span => span.Start))
        {
            var count = span.Start - originalOffset;
            Assert.Equal(before.AsSpan(originalOffset, count).ToArray(), edit.After.AsSpan(outputOffset, count).ToArray());
            originalOffset = span.End;
            outputOffset += count + span.Replacement.Length;
        }
        Assert.Equal(before[originalOffset..], edit.After[outputOffset..]);
    }

    [Theory(DisplayName = "JSON rewrites refuse missing, changed, malformed and ambiguous entries")]
    [InlineData("missing-entry"), InlineData("missing-command"), InlineData("missing-args"), InlineData("wrong-command")]
    [InlineData("wrong-args"), InlineData("non-string-arg"), InlineData("wrong-arg-shape"), InlineData("duplicate-key")]
    [InlineData("invalid-json"), InlineData("duplicate-selector"), InlineData("overlap"), InlineData("invalid-pointer")]
    public void RefusesJsonShape(string situation)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites.OfType<ToolJsonCommandRewrite>().Single();
        var root = JsonNode.Parse(ToolEntryCase.Original(rewrite))?.AsObject() ?? throw new InvalidDataException();
        var entry = root["mcpServers"]?[rewrite.Expected.Command]?.AsObject() ?? throw new InvalidDataException();
        ToolEntryRewrite[] declarations = [rewrite];
        switch (situation)
        {
            case "missing-entry": root["mcpServers"]?.AsObject().Remove(rewrite.Expected.Command); break;
            case "missing-command": entry.Remove(ToolJsonEntries.Command); break;
            case "missing-args": entry.Remove(ToolJsonEntries.Args); break;
            case "wrong-command": entry[ToolJsonEntries.Command] = "fixture-changed"; break;
            case "wrong-args": entry[ToolJsonEntries.Args] = new JsonArray("fixture-changed"); break;
            case "non-string-arg": entry[ToolJsonEntries.Args] = new JsonArray(7); break;
            case "wrong-arg-shape": entry[ToolJsonEntries.Args] = "fixture-changed"; break;
            case "duplicate-selector": declarations = [rewrite, rewrite with { Id = rewrite.Id + "-second" }]; break;
            case "overlap":
                declarations = [rewrite, new ToolHookCommandRewrite { Id = rewrite.Id + "-hook", Host = rewrite.Host, File = rewrite.File,
                Pointer = rewrite.Pointer + "/" + ToolJsonEntries.Command, Expected = rewrite.Expected.Command, Replacement = "fixture-changed" }]; break;
            case "invalid-pointer": declarations = [rewrite with { Pointer = rewrite.Pointer + "~2" }]; break;
        }
        var text = root.ToJsonString();
        if (situation == "duplicate-key") text = text.Replace("\"mcpServers\":", "\"mcpServers\":{},\"mcpServers\":", StringComparison.Ordinal);
        if (situation == "invalid-json") text = text[..^1];
        var files = ToolEntryCase.Files(rewrite, ToolEntryCase.Bytes(text));
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(declarations), files);
        Assert.Empty(plan.Edits);
        Assert.All(plan.Checks, check => Assert.Equal(ToolEntryState.Drifted, check.State));
        Assert.Empty(new ToolEntryVerifier().Verify(ToolEntryCase.Request(declarations), files).Rewrites);
    }

    [Theory(DisplayName = "TOML rewrites refuse missing, changed, duplicated and complex selected lines")]
    [InlineData("missing-table"), InlineData("duplicate-table"), InlineData("quoted-alias"), InlineData("missing-command")]
    [InlineData("missing-args"), InlineData("duplicate-command"), InlineData("duplicate-args"), InlineData("quoted-key")]
    [InlineData("multiline-args"), InlineData("wrong-command"), InlineData("wrong-args"), InlineData("array-table")]
    [InlineData("non-string-arg"), InlineData("suffix"), InlineData("multiline-string")]
    public void RefusesTomlShape(string situation)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites.OfType<ToolTomlCommandRewrite>().Single();
        var text = ToolEntryCase.Text(ToolEntryCase.Original(rewrite));
        var command = $"command = \"{rewrite.Expected.Command}\"";
        const string args = "args = [\"serve\", \"--mcp\"]";
        text = situation switch
        {
            "missing-table" => text.Replace("[" + rewrite.Table + "]", "[fixture-changed]", StringComparison.Ordinal),
            "duplicate-table" => text + $"\n[{rewrite.Table}]\n{command}\n{args}\n",
            "quoted-alias" => text + $"\n[\"mcp_servers\".\"{rewrite.Expected.Command}\"]\n{command}\n{args}\n",
            "missing-command" => text.Replace(command, string.Empty, StringComparison.Ordinal),
            "missing-args" => text.Replace(args, string.Empty, StringComparison.Ordinal),
            "duplicate-command" => text.Replace(command, command + "\n" + command, StringComparison.Ordinal),
            "duplicate-args" => text.Replace(args, args + "\n" + args, StringComparison.Ordinal),
            "quoted-key" => text.Replace(command, "\"command\" = \"" + rewrite.Expected.Command + "\"", StringComparison.Ordinal),
            "multiline-args" => text.Replace(args, "args = [\n\"serve\", \"--mcp\"\n]", StringComparison.Ordinal),
            "wrong-command" => text.Replace(command, "command = \"fixture-changed\"", StringComparison.Ordinal),
            "wrong-args" => text.Replace(args, "args = [\"fixture-changed\"]", StringComparison.Ordinal),
            "array-table" => text.Replace("[" + rewrite.Table + "]", "[[" + rewrite.Table + "]]", StringComparison.Ordinal),
            "non-string-arg" => text.Replace(args, "args = [7]", StringComparison.Ordinal),
            "suffix" => text.Replace(args, args + " unexpected", StringComparison.Ordinal),
            "multiline-string" => text.Replace(command, "command = \"\"\"" + rewrite.Expected.Command + "\"\"\"", StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(rewrite), ToolEntryCase.Files(rewrite, ToolEntryCase.Bytes(text)));
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(plan.Checks).State);
    }

    [Theory(DisplayName = "Marked lines refuse missing, duplicated and reversed selectors")]
    [InlineData("missing-start"), InlineData("missing-end"), InlineData("duplicate-start"), InlineData("duplicate-end")]
    [InlineData("reversed"), InlineData("missing-line"), InlineData("repeated-line"), InlineData("changed-line")]
    public void RefusesMarkedShape(string situation)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites.OfType<ToolMarkedLineRewrite>().Single();
        var text = ToolEntryCase.Text(ToolEntryCase.Original(rewrite));
        text = situation switch
        {
            "missing-start" => text.Replace(rewrite.StartMarker, string.Empty, StringComparison.Ordinal),
            "missing-end" => text.Replace(rewrite.EndMarker, string.Empty, StringComparison.Ordinal),
            "duplicate-start" => text + "\n" + rewrite.StartMarker,
            "duplicate-end" => text + "\n" + rewrite.EndMarker,
            "reversed" => rewrite.EndMarker + "\n" + rewrite.Expected + "\n" + rewrite.StartMarker + "\n",
            "missing-line" => text.Replace(rewrite.Expected, string.Empty, StringComparison.Ordinal),
            "repeated-line" => text.Replace(rewrite.Expected, rewrite.Expected + "\n" + rewrite.Expected, StringComparison.Ordinal),
            "changed-line" => text.Replace(rewrite.Expected, "fixture-changed", StringComparison.Ordinal),
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(rewrite), ToolEntryCase.Files(rewrite, ToolEntryCase.Bytes(text)));
        Assert.Empty(plan.Edits);
        Assert.Equal(ToolEntryState.Drifted, Assert.Single(plan.Checks).State);
    }

    [Fact(DisplayName = "Escaped JSON Pointer tokens select exactly one declared object")]
    public void UsesExactPointer()
    {
        var rewrite = ToolEntryCase.Definition().Rewrites.OfType<ToolJsonCommandRewrite>().Single() with { Pointer = "/a~1b/~0entry" };
        var text = "{\"a/b\":{\"~entry\":{\"command\":\"codegraph\",\"args\":[\"serve\",\"--mcp\"]}}}";
        var request = ToolEntryCase.Request(rewrite);
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, ToolEntryCase.Files(rewrite, ToolEntryCase.Bytes(text))).Edits);
        Assert.Equal(ToolEntryState.Matching, Assert.Single(new ToolEntryPlanner().Observe(request, ToolEntryCase.Files(rewrite, edit.After))).State);
    }

    [Theory(DisplayName = "Host-supplied executable values keep arguments separate and escape JSON and TOML safely")]
    [InlineData(0), InlineData(1)]
    public void ResolvesAbsoluteExecutable(int form)
    {
        var rewrite = ToolEntryCase.Definition().Rewrites[form];
        var launch = Path.GetFullPath(Path.Combine("fixture space 🛠", "open-forge"));
        var request = ToolEntryCase.Request(rewrite) with { Values = new Dictionary<string, string> { [ToolVocabulary.OpenForgeVariable] = launch } };
        var edit = Assert.Single(new ToolEntryPlanner().Plan(request, ToolEntryCase.Files(rewrite, ToolEntryCase.Original(rewrite))).Edits);
        var receipt = Assert.Single(new ToolEntryVerifier().Verify(request, ToolEntryCase.Files(rewrite, edit.After)).Rewrites);
        var command = receipt.Rewrite switch
        {
            ToolJsonCommandRewrite json => json.Replacement,
            ToolTomlCommandRewrite toml => toml.Replacement,
            _ => throw new ArgumentOutOfRangeException(nameof(form)),
        };
        Assert.Equal(launch, command.Command);
        Assert.Equal(ToolEntryState.Matching, Assert.Single(new ToolEntryPlanner().Observe(request, ToolEntryCase.Files(rewrite, edit.After))).State);
    }

    [Fact(DisplayName = "A missing file blocks the complete rewrite plan without partial edits")]
    public void RefusesIncompletePlan()
    {
        var definition = ToolEntryCase.Definition();
        var files = definition.Rewrites.ToDictionary(rewrite => rewrite.File, rewrite => (byte[]?)ToolEntryCase.Original(rewrite), StringComparer.Ordinal);
        files[definition.Rewrites[1].File] = null;
        var plan = new ToolEntryPlanner().Plan(ToolEntryCase.Request(definition.Rewrites), files);
        Assert.Empty(plan.Edits);
        Assert.Equal(1, plan.Checks.Count(check => check.State == ToolEntryState.Missing));
    }
}
