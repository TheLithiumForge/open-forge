using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Hosting.Shared.Interaction;

internal sealed class CliHostTerminalAdapter
{
    private readonly TextReader _standardInput;
    private readonly TextWriter _promptOutput;
    private readonly Func<bool> _keyAvailable;
    private readonly Func<ConsoleKeyInfo> _readKey;
    private readonly object _lineReadGate = new();
    private Task<string?>? _pendingLineRead;

    internal CliHostTerminalAdapter(
        TextReader standardInput,
        TextWriter promptOutput,
        Func<bool> keyAvailable,
        Func<ConsoleKeyInfo> readKey)
    {
        ArgumentNullException.ThrowIfNull(standardInput);
        ArgumentNullException.ThrowIfNull(promptOutput);
        ArgumentNullException.ThrowIfNull(keyAvailable);
        ArgumentNullException.ThrowIfNull(readKey);
        _standardInput = standardInput;
        _promptOutput = promptOutput;
        _keyAvailable = keyAvailable;
        _readKey = readKey;
    }

    internal async ValueTask WriteAsync(ReadOnlyMemory<char> content, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _promptOutput.WriteAsync(content, cancellationToken).ConfigureAwait(false);
        await _promptOutput.FlushAsync(cancellationToken).ConfigureAwait(false);
    }

    internal async ValueTask<string?> ReadLineAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var pending = GetPendingLineRead();
        try
        {
            var answer = await pending.WaitAsync(cancellationToken).ConfigureAwait(false);
            ConsumePendingLineRead(pending);
            return answer;
        }
        catch (Exception) when (pending.IsCompleted && !cancellationToken.IsCancellationRequested)
        {
            // A faulted underlying read cannot yield an answer on a later retry.
            // Release it after the non-cancelled caller observes the fault.
            ConsumePendingLineRead(pending);
            throw;
        }
    }

    internal async ValueTask<CliKeyStroke?> ReadKeyAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (HasPendingLineRead())
            throw new InvalidOperationException("A pending line read must finish before reading a key.");

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                if (_keyAvailable())
                    return Map(_readKey());
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (IOException)
            {
                return null;
            }
            catch (NotSupportedException)
            {
                return null;
            }

            await Task.Delay(TimeSpan.FromMilliseconds(20), cancellationToken).ConfigureAwait(false);
        }
    }

    private Task<string?> GetPendingLineRead()
    {
        lock (_lineReadGate)
        {
            if (_pendingLineRead is { } pending)
                return pending;

            // Console.In can expose a synchronous TextReader wrapper. Starting the
            // invocation on a process-owned worker keeps the command thread
            // cancellable while retaining this single underlying read task.
            var read = Task.Run(() => _standardInput.ReadLineAsync());
            _pendingLineRead = read;
            return read;
        }
    }

    private void ConsumePendingLineRead(Task<string?> read)
    {
        lock (_lineReadGate)
        {
            if (ReferenceEquals(_pendingLineRead, read))
                _pendingLineRead = null;
        }
    }

    private bool HasPendingLineRead()
    {
        lock (_lineReadGate)
        {
            return _pendingLineRead is not null;
        }
    }

    private static CliKeyStroke Map(ConsoleKeyInfo key)
        => key.Key switch
        {
            ConsoleKey.UpArrow => new CliKeyStroke(CliKey.Up),
            ConsoleKey.DownArrow => new CliKeyStroke(CliKey.Down),
            ConsoleKey.Enter => new CliKeyStroke(CliKey.Enter),
            ConsoleKey.Escape => new CliKeyStroke(CliKey.Escape),
            ConsoleKey.Spacebar => new CliKeyStroke(CliKey.Space, ' '),
            _ => new CliKeyStroke(CliKey.Character, key.KeyChar),
        };
}
