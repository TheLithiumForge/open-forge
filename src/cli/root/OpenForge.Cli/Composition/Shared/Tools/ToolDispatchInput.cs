namespace OpenForge.Cli.Composition.Shared.Tools;

internal static class ToolDispatchInput
{
    internal static async Task RelayAsync(Stream source, Stream destination, ToolDispatchSignals signals, CancellationToken token)
    {
        try
        {
            try { await source.CopyToAsync(destination, token).ConfigureAwait(false); }
            catch (IOException)
            {
                // A child can close stdin while still running. Keep watching its parent pipe for EOF.
                await source.CopyToAsync(Stream.Null, token).ConfigureAwait(false);
            }
            if (!token.IsCancellationRequested) signals.RequestHangup();
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested) { }
        catch (IOException) { if (!token.IsCancellationRequested) signals.RequestHangup(); }
        catch (ObjectDisposedException) when (token.IsCancellationRequested) { }
        finally { destination.Close(); }
    }
}
