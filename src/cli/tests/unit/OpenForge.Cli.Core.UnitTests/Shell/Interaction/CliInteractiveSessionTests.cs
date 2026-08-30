using OpenForge.Cli.Core.Shell.Interaction;

namespace OpenForge.Cli.Core.UnitTests.Shell.Interaction;

public sealed class CliInteractiveSessionTests
{
    [Fact(DisplayName = "CLI interaction writes the exact prompt and preserves the exact answer"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task CapableSessionWritesExactPromptAndPreservesExactAnswer()
    {
        using var standardInput = new StringReader("  exact answer  \nremaining");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        var response = await session.AskAsync(
            prompt: "Choose: ",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.True(session.CanPrompt);
        Assert.Equal("Choose: ", promptOutput.ToString());
        Assert.Equal("  exact answer  ", response.Answer);
        Assert.False(response.IsEndOfInput);
        Assert.Equal("remaining", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "CLI interaction preserves a blank answered line"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task CapableSessionPreservesBlankAnsweredLine()
    {
        using var standardInput = new StringReader("\n");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        var response = await session.AskAsync(
            prompt: "Answer: ",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Equal(string.Empty, response.Answer);
        Assert.False(response.IsEndOfInput);
    }

    [Fact(DisplayName = "CLI interaction reports end of input separately from a blank answer"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task CapableSessionReportsEndOfInput()
    {
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        var response = await session.AskAsync(
            prompt: "Answer: ",
            cancellationToken: TestContext.Current.CancellationToken);

        Assert.Null(response.Answer);
        Assert.True(response.IsEndOfInput);
    }

    [Fact(DisplayName = "CLI interaction rejects a disabled session before input or output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task DisabledSessionRejectsBeforeInputOrOutput()
    {
        using var standardInput = new StringReader("unread answer");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: false);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await session.AskAsync(
                prompt: "Answer: ",
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.False(session.CanPrompt);
        Assert.Contains("disabled", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(string.Empty, promptOutput.ToString());
        Assert.Equal("unread answer", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "CLI interaction observes pre-cancellation before input or output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task PreCancelledSessionRejectsBeforeInputOrOutput()
    {
        using var cancellation = new CancellationTokenSource();
        await cancellation.CancelAsync();
        using var standardInput = new StringReader("unread answer");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await session.AskAsync(
                prompt: "Answer: ",
                cancellationToken: cancellation.Token));

        Assert.Equal(string.Empty, promptOutput.ToString());
        Assert.Equal("unread answer", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "CLI interaction rejects a null prompt before input or output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task NullPromptRejectsBeforeInputOrOutput()
    {
        using var standardInput = new StringReader("unread answer");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await session.AskAsync(
                prompt: null!,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(string.Empty, promptOutput.ToString());
        Assert.Equal("unread answer", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "CLI interaction rejects an empty prompt before input or output"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public async Task EmptyPromptRejectsBeforeInputOrOutput()
    {
        using var standardInput = new StringReader("unread answer");
        using var promptOutput = new StringWriter();
        var session = new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: promptOutput,
            canPrompt: true);

        await Assert.ThrowsAsync<ArgumentException>(async () =>
            await session.AskAsync(
                prompt: string.Empty,
                cancellationToken: TestContext.Current.CancellationToken));

        Assert.Equal(string.Empty, promptOutput.ToString());
        Assert.Equal("unread answer", await standardInput.ReadLineAsync(TestContext.Current.CancellationToken));
    }

    [Fact(DisplayName = "CLI interaction rejects null injected streams"), Trait("Feature", "cli-interaction"), Trait("Evidence", "Unit")]
    public void ConstructorRejectsNullInjectedStreams()
    {
        using var standardInput = new StringReader(string.Empty);
        using var promptOutput = new StringWriter();

        Assert.Throws<ArgumentNullException>(() => new CliInteractiveSession(
            standardInput: null!,
            promptOutput: promptOutput,
            canPrompt: true));
        Assert.Throws<ArgumentNullException>(() => new CliInteractiveSession(
            standardInput: standardInput,
            promptOutput: null!,
            canPrompt: true));
    }
}
