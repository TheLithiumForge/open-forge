using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Shell.Interaction;

internal sealed class CliTerminal
{
    private readonly CliTerminalCapabilities _capabilities;
    private readonly CliTerminalWrite _write;
    private readonly CliTerminalReadLine _readLine;
    private readonly CliTerminalReadKey _readKey;

    internal CliTerminal(CliTerminalCapabilities capabilities, CliTerminalWrite write,
        CliTerminalReadLine readLine, CliTerminalReadKey readKey)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(readLine);
        ArgumentNullException.ThrowIfNull(readKey);
        _capabilities = capabilities;
        _write = write;
        _readLine = readLine;
        _readKey = readKey;
    }

    internal bool CanPrompt => _capabilities.CanPrompt;
    internal bool CanReadKeys => _capabilities.CanReadKeys;
    internal bool CanRedraw => _capabilities.CanRedraw;

    internal ValueTask WriteAsync(ReadOnlyMemory<char> content, CancellationToken cancellationToken)
    {
        Validate(cancellationToken);
        return _write(content, cancellationToken);
    }

    internal ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        Validate(cancellationToken);
        return _readLine(cancellationToken);
    }

    internal ValueTask<CliKeyStroke?> ReadKeyAsync(CancellationToken cancellationToken)
    {
        Validate(cancellationToken);
        if (!CanReadKeys) throw new InvalidOperationException("Key reads are unavailable for this terminal.");
        return _readKey(cancellationToken);
    }

    private void Validate(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!CanPrompt) throw new InvalidOperationException("Prompting is unavailable for this terminal.");
    }
}
