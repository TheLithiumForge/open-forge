using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Composition.Shared.Git.Models;

namespace OpenForge.Cli.Composition.Shared.Git;

internal sealed class GitProcessRunner(
    string gitExecutable,
    Action<ProcessStartInfo>? configureProcess = null,
    Action<Process>? processStarted = null)
{
    internal Task<(int ExitCode, string Output, string Error)?> RunAsync(string root, string[] arguments, CancellationToken cancellationToken)
        => RunCoreAsync(new(root, arguments),
            readOutput: process => process.StandardOutput.ReadToEndAsync(CancellationToken.None),
            readError: process => process.StandardError.ReadToEndAsync(CancellationToken.None), cancellationToken);

    internal Task<(int ExitCode, byte[] Output, byte[] Error)?> RunBytesAsync(string root, string[] arguments, CancellationToken cancellationToken)
        => RunCoreAsync(new(root, arguments),
            readOutput: process => ReadBytesAsync(process.StandardOutput.BaseStream),
            readError: process => ReadBytesAsync(process.StandardError.BaseStream), cancellationToken);

    internal Task<(int ExitCode, byte[] Output, byte[] Error)?> RunBytesWithInputAsync(
        string root, string[] arguments, byte[] standardInput, CancellationToken cancellationToken)
        => RunCoreAsync(new(root, arguments, standardInput),
            readOutput: process => ReadBytesAsync(process.StandardOutput.BaseStream),
            readError: process => ReadBytesAsync(process.StandardError.BaseStream), cancellationToken);

    private static async Task<byte[]> ReadBytesAsync(Stream stream)
    {
        using var bytes = new MemoryStream();
        await stream.CopyToAsync(bytes, CancellationToken.None).ConfigureAwait(false);
        return bytes.ToArray();
    }

    private async Task<(int ExitCode, T Output, T Error)?> RunCoreAsync<T>(GitProcessInvocation invocation,
        Func<Process, Task<T>> readOutput, Func<Process, Task<T>> readError, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(gitExecutable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            RedirectStandardInput = invocation.StandardInput is not null,
            StandardOutputEncoding = new UTF8Encoding(false, true),
        };
        start.ArgumentList.Add("--no-optional-locks");
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("core.fsmonitor=false");
        start.ArgumentList.Add("-C");
        start.ArgumentList.Add(invocation.Root);
        foreach (var argument in invocation.Arguments)
        {
            start.ArgumentList.Add(argument);
        }

        // Ambient repository selectors must not replace the selected workspace.
        foreach (var name in new[] { "GIT_DIR", "GIT_WORK_TREE", "GIT_INDEX_FILE", "GIT_COMMON_DIR", "GIT_PREFIX" })
        {
            start.Environment.Remove(name);
        }

        start.Environment["GIT_OPTIONAL_LOCKS"] = "0";
        start.Environment["GIT_TERMINAL_PROMPT"] = "0";
        configureProcess?.Invoke(start);
        using var process = new Process { StartInfo = start };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return null;
        }

        // A cancelled Windows pipe read or write can complete its task while the native IO is still pending, and the
        // late completion then crashes the process with "'overlapped' has already been freed". So no pipe IO takes the
        // token: cancel only the process wait, then reap the child, which ends every pipe operation, and await them all
        // before disposal.
        var output = readOutput(process);
        var error = readError(process);
        var input = Task.CompletedTask;
        try
        {
            processStarted?.Invoke(process);
            if (invocation.StandardInput is { } bytes)
            {
                input = WriteInputAsync(process, bytes);
            }

            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
            await input.ConfigureAwait(false);
            return (process.ExitCode, await output.ConfigureAwait(false), await error.ConfigureAwait(false));
        }
        finally
        {
            if (!process.HasExited)
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch (InvalidOperationException) when (process.HasExited)
                {
                    // The child exited between observation and cancellation cleanup.
                }
            }

            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            await Task.WhenAll(output, error).ConfigureAwait(false);
            await input.ConfigureAwait(false);
        }
    }

    // A child that exits or is killed before reading all of its input breaks the pipe. That is not a Git failure.
    private static async Task WriteInputAsync(Process process, byte[] bytes)
    {
        try
        {
            await process.StandardInput.BaseStream.WriteAsync(bytes, CancellationToken.None).ConfigureAwait(false);
            await process.StandardInput.BaseStream.FlushAsync(CancellationToken.None).ConfigureAwait(false);
            process.StandardInput.Close();
        }
        catch (IOException)
        {
        }
    }
}
