using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal sealed class ToolProcessHost : IToolProcessHost
{
    internal const string CodeGraphHostPpidVariable = "CODEGRAPH_HOST_PPID";
    internal static readonly TimeSpan CleanupTimeout = TimeSpan.FromSeconds(5);
    private readonly IReadOnlyDictionary<string, string?> _environment;
    private readonly ToolCatalogue? _catalogue;
    internal ToolProcessHost(IReadOnlyDictionary<string, string?>? environment = null, ToolCatalogue? catalogue = null)
    {
        _environment = environment ?? Environment.GetEnvironmentVariables().Cast<System.Collections.DictionaryEntry>()
            .ToDictionary(pair => (string)pair.Key, pair => pair.Value?.ToString(), OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal);
        _catalogue = catalogue;
    }

    public Task<ToolResolvedCommand?> ResolveAsync(string command, string workingDirectory, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return Task.FromResult(new ToolCommandResolver(_environment, _catalogue).Resolve(command, workingDirectory));
    }
    private static readonly Encoding OutputEncoding = Encoding.GetEncoding(Encoding.UTF8.CodePage,
        EncoderFallback.ExceptionFallback, new DecoderReplacementFallback("?"));
    public async Task<ToolProcessResult> RunAsync(ToolProcessRequest request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (request.Timeout <= TimeSpan.Zero || request.MaximumOutputBytes <= 0 || !System.IO.Path.IsPathFullyQualified(request.WorkingDirectory)
            || !System.IO.Path.IsPathFullyQualified(request.ExecutablePath))
            throw new ArgumentException("A process needs a working directory and positive bounds.", nameof(request));
        using var timeout = new CancellationTokenSource(request.Timeout);
        using var stopping = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        using var reading = new CancellationTokenSource();
        var start = new ProcessStartInfo(request.ExecutablePath)
        {
            WorkingDirectory = request.WorkingDirectory,
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        start.Environment.Clear();
        foreach (var (key, value) in _environment) if (value is not null) start.Environment[key] = value;
        foreach (var (key, value) in request.Environment)
        {
            if (value is null) start.Environment.Remove(key);
            else start.Environment[key] = value;
        }
        if (!request.Environment.ContainsKey(CodeGraphHostPpidVariable)
            && (!start.Environment.TryGetValue(CodeGraphHostPpidVariable, out var hostPid) || string.IsNullOrEmpty(hostPid))
            && new ToolCommandResolver(_environment, _catalogue).IsCodeGraphBundle(request))
            start.Environment[CodeGraphHostPpidVariable] = Environment.ProcessId.ToString(System.Globalization.CultureInfo.InvariantCulture);
        using var process = new Process { StartInfo = start };
        using var output = new MemoryStream();
        using var error = new MemoryStream();
        long budget = request.MaximumOutputBytes;
        var exceeded = false;
        string? failure = null;
        int? exitCode = null;
        try
        {
            stopping.Token.ThrowIfCancellationRequested();
            process.Start();
            var stdout = DrainAsync(process.StandardOutput.BaseStream, output);
            var stderr = DrainAsync(process.StandardError.BaseStream, error);
            var execution = Task.WhenAll(process.WaitForExitAsync(stopping.Token), stdout, stderr);
            try
            {
                process.StandardInput.Close();
                await execution.WaitAsync(stopping.Token).ConfigureAwait(false);
            }
            finally
            {
                try
                {
                    if (stopping.IsCancellationRequested || !process.WaitForExit(TimeSpan.Zero))
                    {
                        try { process.Kill(entireProcessTree: true); }
                        catch (InvalidOperationException) when (process.WaitForExit(TimeSpan.Zero)) { }
                        catch (Exception exception) when (exception is Win32Exception or AggregateException)
                        {
                            failure = "The tool process tree couldn't be terminated completely.";
                        }
                    }
                    // HasExited and WaitForExitAsync can see a Windows exit code before handles close.
                    // The synchronous bounded wait observes the process handle's signal instead.
                    if (process.WaitForExit(CleanupTimeout)) exitCode = process.ExitCode;
                    else failure = "The tool process didn't exit within its cleanup limit.";
                    try
                    {
                        // Inherited pipes must reach EOF after tree termination, including child writers.
                        await Task.WhenAll(stdout, stderr).WaitAsync(CleanupTimeout).ConfigureAwait(false);
                    }
                    catch (TimeoutException) { failure = "The tool process pipes didn't close within their cleanup limit."; }
                }
                finally
                {
                    await stopping.CancelAsync().ConfigureAwait(false);
                    await reading.CancelAsync().ConfigureAwait(false);
                    await execution.ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                }
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (Exception exception) when (exception is Win32Exception or IOException or InvalidOperationException)
        {
            failure ??= "The tool process couldn't be started or read.";
        }
        if (exceeded) failure ??= "The tool process exceeded its output limit.";
        return new()
        {
            ExitCode = exitCode,
            TimedOut = timeout.IsCancellationRequested && !cancellationToken.IsCancellationRequested,
            Cancelled = cancellationToken.IsCancellationRequested,
            StandardOutput = OutputEncoding.GetString(output.ToArray()),
            StandardError = OutputEncoding.GetString(error.ToArray()),
            Failure = failure,
        };

        async Task DrainAsync(Stream source, Stream destination)
        {
            var buffer = new byte[4096];
            while (await source.ReadAsync(buffer, reading.Token).ConfigureAwait(false) is var count && count > 0)
            {
                var remaining = Interlocked.Add(ref budget, -count);
                var kept = (int)Math.Clamp(remaining + count, 0, count);
                if (kept > 0) await destination.WriteAsync(buffer.AsMemory(0, kept), CancellationToken.None).ConfigureAwait(false);
                if (remaining < 0 && !exceeded)
                {
                    exceeded = true;
                    await stopping.CancelAsync().ConfigureAwait(false);
                }
            }
        }
    }
}
