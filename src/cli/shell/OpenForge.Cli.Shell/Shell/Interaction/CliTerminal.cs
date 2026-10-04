using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Shell.Interaction;

internal sealed class CliTerminal
{
    private readonly CliTerminalCapabilities _capabilities;
    private readonly CliTerminalWrite _write;
    private readonly CliTerminalReadLine _readLine;
    private readonly CliTerminalReadKey _readKey;
    private readonly CliTerminalSelectionView? _selectionView;
    private bool _redrawUnavailable;

    internal CliTerminal(CliTerminalCapabilities capabilities, CliTerminalWrite write,
        CliTerminalReadLine readLine, CliTerminalReadKey readKey, CliTerminalSelectionView? selectionView = null)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        ArgumentNullException.ThrowIfNull(write);
        ArgumentNullException.ThrowIfNull(readLine);
        ArgumentNullException.ThrowIfNull(readKey);
        _capabilities = capabilities;
        _write = write;
        _readLine = readLine;
        _readKey = readKey;
        if (selectionView is not null)
        {
            ArgumentNullException.ThrowIfNull(selectionView.ReadViewport);
            ArgumentNullException.ThrowIfNull(selectionView.Clear);
        }
        _selectionView = selectionView;
    }

    internal bool CanPrompt => _capabilities.CanPrompt;
    internal bool CanReadKeys => _capabilities.CanReadKeys;
    internal bool CanRedraw => _capabilities.CanRedraw && _selectionView is not null && !_redrawUnavailable;

    internal CliTerminalViewport? ReadSelectionViewport()
    {
        if (!CanRedraw || _selectionView is null) return null;
        var viewport = _selectionView.ReadViewport();
        return viewport is { SupportsSelection: true } ? viewport : null;
    }

    internal bool ClearSelection(CancellationToken cancellationToken)
    {
        Validate(cancellationToken);
        if (!CanRedraw || _selectionView is null) return false;
        if (_selectionView.Clear()) return true;
        _redrawUnavailable = true;
        return false;
    }

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
