namespace OpenForge.Cli.Core.Shell.Interaction.Models;

internal sealed class CliInteractiveResponse
{
    public required string? Answer { get; init; }

    public bool IsEndOfInput => Answer is null;
}
