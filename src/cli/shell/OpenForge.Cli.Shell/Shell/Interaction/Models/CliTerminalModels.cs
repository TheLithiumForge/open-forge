namespace OpenForge.Cli.Core.Shell.Interaction.Models;

internal sealed record CliTerminalCapabilities
{
    internal CliTerminalCapabilities(bool canPrompt, bool canReadKeys, bool canRedraw)
    {
        if (canReadKeys && !canPrompt || canRedraw && !canReadKeys)
            throw new ArgumentException("Redraw requires key reads, and key reads require prompting.");
        CanPrompt = canPrompt;
        CanReadKeys = canReadKeys;
        CanRedraw = canRedraw;
    }

    internal bool CanPrompt { get; }
    internal bool CanReadKeys { get; }
    internal bool CanRedraw { get; }
}

internal enum CliKey { Up, Down, Enter, Escape, Space, Character }
internal readonly record struct CliKeyStroke(CliKey Key, char Character = '\0');
internal delegate ValueTask CliTerminalWrite(ReadOnlyMemory<char> content, CancellationToken cancellationToken);
internal delegate ValueTask<string?> CliTerminalReadLine(CancellationToken cancellationToken);
internal delegate ValueTask<CliKeyStroke?> CliTerminalReadKey(CancellationToken cancellationToken);
