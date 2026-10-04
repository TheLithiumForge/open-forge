using System.Text;
using System.Text.RegularExpressions;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys.Shared.Terminal;

internal sealed partial class PublishedTerminalTranscript
{
    private readonly object _gate = new();
    private readonly StringBuilder _text = new();
    private readonly Decoder _decoder = new UTF8Encoding(false, true).GetDecoder();
    private TaskCompletionSource _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private int _projectedPosition;

    internal void Append(byte[] bytes, int count)
    {
        lock (_gate)
        {
            var characters = new char[Encoding.UTF8.GetMaxCharCount(count)];
            var written = _decoder.GetChars(bytes, 0, count, characters, 0);
            _text.Append(characters, 0, written);
            _changed.TrySetResult();
            _changed = new(TaskCreationOptions.RunContinuationsAsynchronously);
        }
    }

    internal async Task WaitForAsync(string marker, TimeSpan timeout, CancellationToken cancellationToken)
    {
        var projectedMarker = Project(marker);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        deadline.CancelAfter(timeout);
        try
        {
            while (true)
            {
                Task changed;
                lock (_gate)
                {
                    // ConPTY may insert CSI controls and repaint whitespace inside
                    // a visible marker. Keep raw diagnostics and projected positions.
                    var projected = Project(_text.ToString());
                    var position = projected.IndexOf(projectedMarker, _projectedPosition, StringComparison.Ordinal);
                    if (position >= 0)
                    {
                        _projectedPosition = position + projectedMarker.Length;
                        return;
                    }
                    changed = _changed.Task;
                }
                await changed.WaitAsync(deadline.Token).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            lock (_gate) throw new TimeoutException($"Terminal did not produce '{marker}'. Transcript: {_text}");
        }
    }

    private static string Project(string text)
        => Whitespace().Replace(CsiControls().Replace(text, string.Empty), string.Empty);

    [GeneratedRegex("\u001b\\[[0-?]*[ -/]*[@-~]", RegexOptions.CultureInvariant)]
    private static partial Regex CsiControls();

    [GeneratedRegex(@"\s+", RegexOptions.CultureInvariant)]
    private static partial Regex Whitespace();
}
