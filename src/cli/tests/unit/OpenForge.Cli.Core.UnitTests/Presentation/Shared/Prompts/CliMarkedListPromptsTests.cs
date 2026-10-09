using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Prompts;

public sealed class CliMarkedListPromptsTests
{
    [Fact(DisplayName = "Marked list sets each mark key and returns every row in question order")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task MarkKeysReturnCompleteAnswer()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Character, '-'), new(CliKey.Down), new(CliKey.Character, '+'),
            new(CliKey.Down), new(CliKey.Character, '~'), new(CliKey.Enter)]);
        var reply = await Run(script);
        Assert.Equal(["first", "second", "third"], reply.Value.Rows.Select(row => row.Value));
        Assert.Equal([2, 0, 1], reply.Value.Rows.Select(row => row.Mark));
    }

    [Fact(DisplayName = "Marked list wraps movement and cycles marks in declared order")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task MovementAndSpaceWrap()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Up), new(CliKey.Space), new(CliKey.Space), new(CliKey.Space),
            new(CliKey.Down), new(CliKey.Space), new(CliKey.Enter)]);
        var reply = await Run(script);
        Assert.Equal([1, 1, 2], reply.Value.Rows.Select(row => row.Mark));
        Assert.Contains("> [-] third", script.Frames[1], StringComparison.Ordinal);
        Assert.Contains("> [+] first", script.Frames[5], StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Marked list leaves locked rows unchanged and shows the reason")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task LockedRowRejectsKeysAndSpace()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Character, '-'), new(CliKey.Space), new(CliKey.Enter)]);
        var reply = await Run(script, locked: true);
        Assert.Equal(0, reply.Value.Rows[0].Mark);
        Assert.All(script.Frames, frame => Assert.Contains("Set by an explicit option.", frame, StringComparison.Ordinal));
    }

    [Theory(DisplayName = "Marked list cancels Escape end of input and explicit line cancellation")]
    [InlineData("escape"), InlineData("key-eof"), InlineData("line-eof"), InlineData("cancel"), InlineData("CANCEL")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task Cancellation(string mode)
    {
        var script = mode switch
        {
            "escape" => ScriptedCliTerminal.Keys([new(CliKey.Escape)]),
            "key-eof" => ScriptedCliTerminal.Keys([]),
            "line-eof" => ScriptedCliTerminal.Lines([null]),
            _ => ScriptedCliTerminal.Lines([mode]),
        };
        Assert.Equal(CliPromptState.Cancelled, (await Run(script)).State);
    }

    [Theory(DisplayName = "Marked list accepts the complete initial answer on Enter in either mode")]
    [InlineData(true), InlineData(false)]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task EnterAcceptsInitialAnswer(bool keys)
    {
        var script = keys ? ScriptedCliTerminal.Keys([new(CliKey.Enter)]) : ScriptedCliTerminal.Lines([""]);
        Assert.Equal([0, 1, 2], (await Run(script)).Value.Rows.Select(row => row.Mark));
    }

    [Theory(DisplayName = "Marked list applies multiple line edits together and reprints the changed marks")]
    [InlineData("1- 2+ 3~"), InlineData("1-,2+,3~"), InlineData("1-, 2+ 3~")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task MultipleLineEdits(string edits)
    {
        var script = ScriptedCliTerminal.Lines([edits, ""]);
        Assert.Equal([2, 0, 1], (await Run(script)).Value.Rows.Select(row => row.Mark));
        Assert.Equal(2, script.LineReadCalls);
        Assert.Contains("1. [-] first", script.Output.ToString(), StringComparison.Ordinal);
    }

    [Theory(DisplayName = "Marked list rejects an entire invalid line without changing any row")]
    [InlineData("1- 4+"), InlineData("1- 2x"), InlineData("1- 0+"), InlineData("1- 2"), InlineData("1- words"), InlineData(",,,")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task InvalidLinesAreAtomic(string edits)
    {
        var script = ScriptedCliTerminal.Lines([edits, ""]);
        Assert.Equal([0, 1, 2], (await Run(script)).Value.Rows.Select(row => row.Mark));
        Assert.Contains("Use a row number from 1 to 3 followed by +, ~ or -, such as 2+.", script.Output.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "A locked line edit leaves earlier edits on the same line unapplied")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task LockedLineIsAtomic()
    {
        var script = ScriptedCliTerminal.Lines(["2+ 1-", ""]);
        Assert.Equal([0, 1, 2], (await Run(script, locked: true)).Value.Rows.Select(row => row.Mark));
        Assert.Contains("Set by an explicit option.", script.Output.ToString(), StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Marked list redraws resize without losing focus or marks")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task ResizePreservesState()
    {
        var script = ScriptedCliTerminal.Keys([new(CliKey.Down), new(CliKey.Character, '-'), new(CliKey.Resize), new(CliKey.Enter)]);
        script.BeforeKeyRead = call => { if (call == 3) script.Viewport = new(40, 12); };
        var reply = await Run(script);
        Assert.Equal(2, reply.Value.Rows[1].Mark);
        Assert.Contains("> [-] second", script.Frames[^1], StringComparison.Ordinal);
        Assert.All(script.Frames[^1].Split(Environment.NewLine), line => Assert.True(line.Length < 40));
        Assert.InRange(script.Frames[^1].Split(Environment.NewLine).Length - 1, 1, 11);
    }

    [Fact(DisplayName = "Marked list honors unavailable policy and caller cancellation before reading")]
    [Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
    public async Task PolicyAndCancellation()
    {
        var script = ScriptedCliTerminal.Lines([""]);
        var prompts = new CliPrompts(script.Terminal);
        Assert.Equal(CliPromptState.Unavailable, (await prompts.MarkedListAsync(Question(), new(false), CancellationToken.None)).State);
        Assert.Equal(CliPromptState.Cancelled, (await prompts.MarkedListAsync(Question(), new(true), new CancellationToken(true))).State);
        Assert.Equal(0, script.WriteCalls);
        Assert.Equal(0, script.LineReadCalls);
    }

    private static ValueTask<CliPromptReply<CliMarkedSelection<string>>> Run(ScriptedCliTerminal script, bool locked = false)
        => new CliPrompts(script.Terminal).MarkedListAsync(Question(locked), new(true), CancellationToken.None);

    private static CliMarkedListQuestion<string> Question(bool locked = false)
        => new("Choose routes", [new("+", "add"), new("~", "keep local"), new("-", "leave out")],
            [new("first", "first", "First summary", 0, locked ? "Set by an explicit option." : null),
                new("second", "second", "Second summary", 1, null), new("third", "third", "Third summary", 2, null)],
            static (_, mark) => mark == 1 ? "Files stay on this machine." : null);
}
