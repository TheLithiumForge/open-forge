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
        Terminal = new CliTerminal(capabilities, WriteAsync, ReadLineAsync, ReadKeyAsync);
    }

    internal CliTerminal Terminal { get; }
    internal StringBuilder Output { get; } = new();
    internal int WriteCalls { get; private set; }
    internal int LineReadCalls { get; private set; }
    internal int KeyReadCalls { get; private set; }

    internal static ScriptedCliTerminal Lines(
        IEnumerable<string?> lines,
        bool canPrompt = true)
        => new(new CliTerminalCapabilities(canPrompt, false, false), lines: lines);

    internal static ScriptedCliTerminal Keys(
        IEnumerable<CliKeyStroke?> keys,
        bool canPrompt = true,
        bool canRedraw = false)
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
        return ValueTask.CompletedTask;
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
        return ValueTask.FromResult<CliKeyStroke?>(_keys.Count == 0 ? null : _keys.Dequeue());
    }
}
