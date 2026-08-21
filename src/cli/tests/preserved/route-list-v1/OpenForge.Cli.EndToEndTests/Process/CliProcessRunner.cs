using System.ComponentModel;
using System.Diagnostics;

namespace OpenForge.Cli.EndToEndTests.Process;

internal sealed record CliProcessRequest(
    string ExecutablePath,
    IReadOnlyList<string> Arguments);

internal sealed record CliProcessResult(int ExitCode, string Stdout, string Stderr);

internal static class CliProcessRunner
{
    internal static async Task<CliProcessResult> RunAsync(
        CliProcessRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };
        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = System.Diagnostics.Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Unable to start CLI process: {request.ExecutablePath}");
        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
        try
        {
            await process.WaitForExitAsync(cancellationToken);
        }
        catch (OperationCanceledException)
        {
            KillProcessTreeIfRunning(process);
            await process.WaitForExitAsync(CancellationToken.None);
            await AwaitDrainsAfterCancellationAsync(stdout, stderr);

            throw;
        }

        await Task.WhenAll(stdout, stderr);
        return new CliProcessResult(process.ExitCode, stdout.Result, stderr.Result);
    }

    private static void KillProcessTreeIfRunning(System.Diagnostics.Process process)
    {
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
            }
        }
        catch (InvalidOperationException) when (process.HasExited)
        {
        }
        catch (Win32Exception) when (process.HasExited)
        {
        }
    }

    private static async Task AwaitDrainsAfterCancellationAsync(Task<string> stdout, Task<string> stderr)
    {
        try
        {
            await Task.WhenAll(stdout, stderr);
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
    }
}
