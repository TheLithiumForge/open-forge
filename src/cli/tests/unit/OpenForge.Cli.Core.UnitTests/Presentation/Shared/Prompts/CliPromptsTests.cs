using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliPromptsTests
{
    [Trait("Boundary", "Input")]
    [Theory(DisplayName = "Confirm accepts the line yes spellings and cancels the negative spellings"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    [InlineData("y", nameof(CliPromptState.Answered), true)]
    [InlineData("yes", nameof(CliPromptState.Answered), true)]
    [InlineData("n", nameof(CliPromptState.Cancelled), false)]
    [InlineData("no", nameof(CliPromptState.Cancelled), false)]
    [InlineData("", nameof(CliPromptState.Cancelled), false)]
    public async Task ConfirmLineSemantics(string answer, string expectedState, bool expected)
    {
        var scripted = ScriptedCliTerminal.Lines([answer]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        var state = Enum.Parse<CliPromptState>(expectedState);
        Assert.Equal(state, reply.State);
        if (state == CliPromptState.Answered)
            Assert.Equal(expected, reply.Value);
        else
            Assert.Throws<InvalidOperationException>(() => _ = reply.Value);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Confirm treats end of input as cancellation in line and key modes"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task ConfirmEndOfInputCancels()
    {
        var lineScript = ScriptedCliTerminal.Lines([null]);
        var lineReply = await new CliPrompts(lineScript.Terminal).ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, lineReply.State);

        var keyScript = ScriptedCliTerminal.Keys([]);
        var keyReply = await new CliPrompts(keyScript.Terminal).ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, keyReply.State);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Confirm uses the same affirmative answer in key and line modes"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task ConfirmKeyAndLineAffirmationMatch()
    {
        var keyScript = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Character, 'y')]);
        var keyReply = await new CliPrompts(keyScript.Terminal).ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        var lineScript = ScriptedCliTerminal.Lines(["yes"]);
        var lineReply = await new CliPrompts(lineScript.Terminal).ConfirmAsync(
            new CliConfirmQuestion("Apply these changes? [y/N]"),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.True(keyReply.Value);
        Assert.True(lineReply.Value);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Select uses the same semantic choice in key mode and redraws when supported"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task SelectKeyModeUsesCursorAndRedraw()
    {
        var scripted = ScriptedCliTerminal.Keys(
            [new CliKeyStroke(CliKey.Down), new CliKeyStroke(CliKey.Enter)],
            canRedraw: true);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliSelectQuestion<string>("Which one?", [
            new CliChoice<string>("first", "first"),
            new CliChoice<string>("second", "second"),
        ]);

        var reply = await prompts.SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal("second", reply.Value);
        Assert.Contains("\u001b[", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Select keeps key semantics when redraw is unavailable"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task SelectKeyModeFallsBackWithoutRedraw()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Down),
            new CliKeyStroke(CliKey.Enter),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliSelectQuestion<string>("Which one?", [
            new CliChoice<string>("first", "first"),
            new CliChoice<string>("second", "second"),
        ]);

        var reply = await prompts.SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal("second", reply.Value);
        Assert.DoesNotContain("\u001b[", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Select supports up, digit, and escape key rules"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task SelectKeyRulesRemainTypedAndEquivalent()
    {
        var question = new CliSelectQuestion<string>("Which one?", [
            new CliChoice<string>("first", "first"),
            new CliChoice<string>("second", "second"),
            new CliChoice<string>("third", "third"),
        ]);

        var up = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Up), new CliKeyStroke(CliKey.Enter)]);
        var upReply = await new CliPrompts(up.Terminal).SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal("third", upReply.Value);

        var digit = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Character, '2')]);
        var digitReply = await new CliPrompts(digit.Terminal).SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal("second", digitReply.Value);

        var escape = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Escape)]);
        var escapeReply = await new CliPrompts(escape.Terminal).SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, escapeReply.State);
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Select line mode chooses a numbered row and Enter cancels"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task SelectLineModeUsesNumberedRows()
    {
        var scripted = ScriptedCliTerminal.Lines(["2"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliSelectQuestion<string>("Which one?", [
            new CliChoice<string>("first", "first"),
            new CliChoice<string>("second", "second"),
        ]);

        var reply = await prompts.SelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal("second", reply.Value);
        Assert.Contains("Choose a number (1-2), or press Enter to cancel:", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Multi-select returns chosen and transitive required values through a cycle"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectComputesCycleSafeClosure()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Space),
            new CliKeyStroke(CliKey.Enter),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = MultiQuestion(
            [new CliDependency<string>("a", "b"), new CliDependency<string>("b", "c"), new CliDependency<string>("c", "a")]);

        var reply = await prompts.MultiSelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(["a"], reply.Value.Chosen);
        Assert.Equal(["b", "c"], reply.Value.Required);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Multi-select refuses a required row and clears its notice on the next key"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectRequiredRowRefusalIsTransient()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Space),
            new CliKeyStroke(CliKey.Down),
            new CliKeyStroke(CliKey.Space),
            new CliKeyStroke(CliKey.Character, 'x'),
            new CliKeyStroke(CliKey.Enter),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliMultiSelectQuestion<string>(
            "Which packages?",
            [new CliChoice<string>("app", "app"), new CliChoice<string>("runtime", "runtime")],
            [new CliDependency<string>("app", "runtime")],
            new HashSet<string>());

        var reply = await prompts.MultiSelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(["app"], reply.Value.Chosen);
        Assert.Equal(["runtime"], reply.Value.Required);
        var notice = "runtime is required by app. Unchoose that first.";
        Assert.Equal(1, CountOccurrences(scripted.Output.ToString(), notice));
        Assert.DoesNotContain(notice + Environment.NewLine + notice, scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Multi-select supports all and none key commands"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectAllAndNoneCommandsAreDeterministic()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Character, 'a'),
            new CliKeyStroke(CliKey.Character, 'n'),
            new CliKeyStroke(CliKey.Character, 'a'),
            new CliKeyStroke(CliKey.Enter),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.MultiSelectAsync(
            MultiQuestion([new CliDependency<string>("a", "b")]),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(["a", "b", "c"], reply.Value.Chosen);
        Assert.Empty(reply.Value.Required);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Disabled dependency rows satisfy closure and cannot become mutation targets"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectSkipsDisabledDependencies()
    {
        var scripted = ScriptedCliTerminal.Lines(["1"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliMultiSelectQuestion<string>(
            "Which packages?",
            [new CliChoice<string>("app", "app"), new CliChoice<string>("runtime", "runtime")],
            [new CliDependency<string>("app", "runtime")],
            new HashSet<string>(["runtime"]));

        var reply = await prompts.MultiSelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(["app"], reply.Value.Chosen);
        Assert.Empty(reply.Value.Required);
        Assert.Contains("installed", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Dependents mode returns the original reverse closure and uses removal wording"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectUsesDependentsDirection()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Down),
            new CliKeyStroke(CliKey.Space),
            new CliKeyStroke(CliKey.Enter),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliMultiSelectQuestion<string>(
            "Which packages?",
            [new CliChoice<string>("app", "app"), new CliChoice<string>("runtime", "runtime")],
            [new CliDependency<string>("app", "runtime")],
            new HashSet<string>(),
            CliDependencyDirection.Dependents);

        var reply = await prompts.MultiSelectAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(["runtime"], reply.Value.Chosen);
        Assert.Equal(["app"], reply.Value.Required);
        Assert.Contains("needed by", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Empty multi-select Enter stays in the prompt and Escape cancels"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task MultiSelectEmptyEnterDoesNotAnswer()
    {
        var scripted = ScriptedCliTerminal.Keys([
            new CliKeyStroke(CliKey.Enter),
            new CliKeyStroke(CliKey.Escape),
        ]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.MultiSelectAsync(MultiQuestion([]), new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Cancelled, reply.State);
        Assert.Contains("Choose at least one package, or press esc to cancel.", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Text input retries the rule and then returns the validated typed value"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task TextInputRetriesInvalidValue()
    {
        var scripted = ScriptedCliTerminal.Lines(["My Tools", "my-tools"]);
        var prompts = new CliPrompts(scripted.Terminal);
        var question = new CliTextQuestion<string>(
            "Extension ID (lowercase, digits and hyphens):",
            "Use lowercase letters, digits and hyphens.",
            Required: true,
            value => value is "my-tools"
                ? new CliTextValidation<string>(true, value, null)
                : new CliTextValidation<string>(false, null, "'My Tools' is not a valid ID. Use lowercase letters, digits and hyphens."));

        var reply = await prompts.TextAsync(question, new CliPromptPolicy(Allowed: true), CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal("my-tools", reply.Value);
        Assert.Contains("'My Tools' is not a valid ID.", scripted.Output.ToString(), StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Required text cancels after two empty answers"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task TextInputCancelsAfterRepeatedEmptyAnswers()
    {
        var scripted = ScriptedCliTerminal.Lines(["", ""]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.TextAsync(
            new CliTextQuestion<string>("ID:", "Use an ID.", Required: true,
                value => new CliTextValidation<string>(true, value, null)),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Cancelled, reply.State);
        Assert.Equal(2, scripted.LineReadCalls);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Text validation cannot answer with a null typed value"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task TextValidationRequiresAnAnswerValue()
    {
        var scripted = ScriptedCliTerminal.Lines(["candidate"]);
        var prompts = new CliPrompts(scripted.Terminal);

        await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await prompts.TextAsync(
                new CliTextQuestion<string>("ID:", "Use an ID.", Required: false,
                    _ => new CliTextValidation<string>(true, null, null)),
                new CliPromptPolicy(Allowed: true),
                CancellationToken.None));
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Permission prompt keeps the path frame and fixed choices together"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PermissionKeyFrameHasOneBlankLineBeforeChoices()
    {
        var scripted = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Enter)]);
        var prompts = new CliPrompts(scripted.Terminal);

        var reply = await prompts.PermissionAsync(
            new CliPermissionQuestion("memory-starters", [new CliPermissionPath("templates/memory", true)]),
            new CliPromptPolicy(Allowed: true),
            CancellationToken.None);

        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(CliPermissionChoice.Always, reply.Value);
        var output = scripted.Output.ToString();
        var newLine = Environment.NewLine;
        Assert.Contains($"templates/memory   (directory: everything under it){newLine}{newLine}  > Allow always", output, StringComparison.Ordinal);
        Assert.DoesNotContain($"everything under it){newLine}{newLine}{newLine}", output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Permission line mode accepts always and once and cancels cancel or end of input"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PermissionLineRulesRemainTyped()
    {
        var always = ScriptedCliTerminal.Lines(["always"]);
        var alwaysReply = await new CliPrompts(always.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Always, alwaysReply.Value);
        Assert.Contains("always, once, cancel:", always.Output.ToString(), StringComparison.Ordinal);

        var once = ScriptedCliTerminal.Lines(["once"]);
        var onceReply = await new CliPrompts(once.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Once, onceReply.Value);

        var cancel = ScriptedCliTerminal.Lines(["cancel"]);
        var cancelReply = await new CliPrompts(cancel.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, cancelReply.State);

        var eof = ScriptedCliTerminal.Lines([null]);
        var eofReply = await new CliPrompts(eof.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, eofReply.State);
    }

    [Trait("Boundary", "Input")]
    [Fact(DisplayName = "Permission key digits choose rows one and two and cancel row three"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PermissionKeyDigitsUseTheSharedSelectionRules()
    {
        var always = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Character, '1')]);
        var alwaysReply = await new CliPrompts(always.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Always, alwaysReply.Value);

        var once = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Character, '2')]);
        var onceReply = await new CliPrompts(once.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPermissionChoice.Once, onceReply.Value);

        var cancel = ScriptedCliTerminal.Keys([new CliKeyStroke(CliKey.Character, '3')]);
        var cancelReply = await new CliPrompts(cancel.Terminal).PermissionAsync(
            PermissionQuestion(), new CliPromptPolicy(Allowed: true), CancellationToken.None);
        Assert.Equal(CliPromptState.Cancelled, cancelReply.State);
    }

    private static CliMultiSelectQuestion<string> MultiQuestion(IReadOnlyList<CliDependency<string>> dependencies)
        => new(
            "Which packages?",
            [new CliChoice<string>("a", "a"), new CliChoice<string>("b", "b"), new CliChoice<string>("c", "c")],
            dependencies,
            new HashSet<string>());

    private static CliPermissionQuestion PermissionQuestion()
        => new("memory-starters", [new CliPermissionPath("templates/memory", true)]);

    private static int CountOccurrences(string value, string search)
    {
        var count = 0;
        var offset = 0;
        while ((offset = value.IndexOf(search, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += search.Length;
        }

        return count;
    }
}
