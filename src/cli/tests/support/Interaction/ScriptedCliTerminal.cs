using System.Text;
using OpenForge.Cli.Core.Shell.Interaction;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.TestSupport.Interaction;

internal sealed class ScriptedCliTerminal
{
    private readonly Queue<string?> _lines;
    private readonly Queue<CliKeyStroke?> _keys;

    internal ScriptedCliTerminal(
        CliTerminalCapabilities capabilities,
        IEnumerable<string?>? lines = null,
        IEnumerable<CliKeyStroke?>? keys = null)
    {
        ArgumentNullException.ThrowIfNull(capabilities);
        _lines = new Queue<string?>(lines ?? []);
        _keys = new Queue<CliKeyStroke?>(keys ?? []);
        var selectionView = capabilities.CanRedraw ? new CliTerminalSelectionView(() => Viewport, Clear) : null;
        Terminal = new CliTerminal(capabilities, WriteAsync, ReadLineAsync, ReadKeyAsync, selectionView);
    }

    internal CliTerminal Terminal { get; }
    internal StringBuilder Output { get; } = new();
    internal int WriteCalls { get; private set; }
    internal int LineReadCalls { get; private set; }
    internal int KeyReadCalls { get; private set; }
    internal int ClearCalls { get; private set; }
    internal List<string> Frames { get; } = [];
    internal CliTerminalViewport? Viewport { get; set; } = new(80, 24);
    internal bool CanClear { get; set; } = true;
    internal Action<int>? BeforeKeyRead { get; set; }
    private bool _selectionWrite;

    internal static ScriptedCliTerminal Lines(
        IEnumerable<string?> lines,
        bool canPrompt = true)
        => new(new CliTerminalCapabilities(canPrompt, false, false), lines: lines);

    internal static ScriptedCliTerminal Keys(
        IEnumerable<CliKeyStroke?> keys,
        bool canPrompt = true,
        bool canRedraw = true)
    {
        var capabilities = canPrompt
            ? new CliTerminalCapabilities(true, true, canRedraw)
            : new CliTerminalCapabilities(false, false, false);
        return new(capabilities, keys: keys);
    }

    private ValueTask WriteAsync(ReadOnlyMemory<char> content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WriteCalls++;
        Output.Append(content.Span);
        if (_selectionWrite)
        {
            Frames.Add(content.ToString());
            _selectionWrite = false;
        }
        return ValueTask.CompletedTask;
    }

    private bool Clear()
    {
        ClearCalls++;
        _selectionWrite = CanClear;
        return CanClear;
    }

    private ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        LineReadCalls++;
        return ValueTask.FromResult(_lines.Count == 0 ? null : _lines.Dequeue());
    }

    private ValueTask<CliKeyStroke?> ReadKeyAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        KeyReadCalls++;
        BeforeKeyRead?.Invoke(KeyReadCalls);
        return ValueTask.FromResult<CliKeyStroke?>(_keys.Count == 0 ? null : _keys.Dequeue());
    }
}
