using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Shell.Interaction;

internal sealed class CliInteractiveSession
{
    private readonly TextReader _standardInput;
    private readonly TextWriter _promptOutput;

    internal CliInteractiveSession(
        TextReader standardInput,
        TextWriter promptOutput,
        bool canPrompt)
    {
        ArgumentNullException.ThrowIfNull(standardInput);
        ArgumentNullException.ThrowIfNull(promptOutput);

        _standardInput = standardInput;
        _promptOutput = promptOutput;
        CanPrompt = canPrompt;
    }

    internal bool CanPrompt { get; }

    internal async ValueTask<CliInteractiveResponse> AskAsync(
        string prompt,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(prompt);
        if (!CanPrompt)
        {
            throw new InvalidOperationException("Interactive prompting is disabled for this session.");
        }

        cancellationToken.ThrowIfCancellationRequested();
        await _promptOutput.WriteAsync(prompt.AsMemory(), cancellationToken).ConfigureAwait(false);
        await _promptOutput.FlushAsync(cancellationToken).ConfigureAwait(false);

        var answer = await _standardInput.ReadLineAsync(cancellationToken).ConfigureAwait(false);
        return new CliInteractiveResponse { Answer = answer };
    }
}
