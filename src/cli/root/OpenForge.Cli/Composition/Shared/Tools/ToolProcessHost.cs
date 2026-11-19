using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Models;

namespace OpenForge.Cli.Composition.Shared.Tools;

/// <summary>
/// Runs a catalogue command the way a person would type it: through cmd.exe on Windows, /bin/sh elsewhere, or
/// PowerShell for a PowerShell command. The text is always a fixed catalogue constant, never user input. Input is
/// closed, the environment is inherited, and timeout or cancellation stops the whole process tree.
/// </summary>
internal sealed class ToolProcessHost : IToolProcessHost
{
    private const int TailCharacters = 8 * 1024;
    private static readonly TimeSpan CleanupTimeout = TimeSpan.FromSeconds(5);

    public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var start = Start(request.Command);
        start.WorkingDirectory = request.WorkingDirectory;
        start.UseShellExecute = false;
        start.CreateNoWindow = true;
        start.RedirectStandardInput = true;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;
        var tail = new StringBuilder();
        using var timeout = new CancellationTokenSource(request.Timeout);
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        Process process;
        try
        {
            process = Process.Start(start) ?? throw new InvalidOperationException("The process didn't start.");
        }
        catch (Exception exception) when (exception is Win32Exception or InvalidOperationException)
        {
            return new() { Started = false };
        }
        using (process)
        {
            process.StandardInput.Close();
            var output = DrainAsync(process.StandardOutput);
            var error = DrainAsync(process.StandardError);
            try
            {
                await process.WaitForExitAsync(stopping.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (stopping.IsCancellationRequested)
            {
                try { process.Kill(entireProcessTree: true); }
                catch (Exception exception) when (exception is InvalidOperationException or Win32Exception or AggregateException) { }
                process.WaitForExit(CleanupTimeout);
                cancellationToken.ThrowIfCancellationRequested();
                return new() { Started = true, TimedOut = true, OutputTail = Tail() };
            }
            // A background process the tool started can keep the pipes open, so the output gets a short grace period only.
            try { await Task.WhenAll(output, error).WaitAsync(CleanupTimeout, CancellationToken.None).ConfigureAwait(false); }
            catch (TimeoutException) { }
            // The synchronous wait observes the process handle, so the exit code is final.
            process.WaitForExit();
            return new() { Started = true, ExitCode = process.ExitCode, OutputTail = Tail() };
        }

        async Task DrainAsync(StreamReader reader)
        {
            var buffer = new char[4096];
            while (await reader.ReadAsync(buffer, CancellationToken.None).ConfigureAwait(false) is var count && count > 0)
            {
                lock (tail)
                {
                    tail.Append(buffer, 0, count);
                    if (tail.Length > TailCharacters) tail.Remove(0, tail.Length - TailCharacters);
                }
            }
        }

        string Tail()
        {
            lock (tail) return tail.ToString();
        }
    }

    private static ProcessStartInfo Start(ToolCommand command)
    {
        if (command.Shell == ToolShell.PowerShell)
        {
            // An encoded command needs no quoting, so the catalogue text reaches PowerShell exactly.
            var encoded = Convert.ToBase64String(Encoding.Unicode.GetBytes(command.Text));
            var start = new ProcessStartInfo("powershell");
            foreach (var argument in new[] { "-NoProfile", "-NonInteractive", "-ExecutionPolicy", "Bypass", "-EncodedCommand", encoded })
                start.ArgumentList.Add(argument);
            return start;
        }
        if (OperatingSystem.IsWindows())
        {
            // cmd.exe resolves npm.cmd and other shims from PATH. /s keeps the quoted text exactly as written.
            return new ProcessStartInfo(Path.Combine(Environment.SystemDirectory, "cmd.exe")) { Arguments = $"/d /s /c \"{command.Text}\"" };
        }
        var shell = new ProcessStartInfo("/bin/sh");
        shell.ArgumentList.Add("-c");
        shell.ArgumentList.Add(command.Text);
        return shell;
    }
}
