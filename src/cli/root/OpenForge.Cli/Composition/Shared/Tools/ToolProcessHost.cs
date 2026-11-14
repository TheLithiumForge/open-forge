using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

namespace OpenForge.Cli.Composition.Shared.Tools;

internal sealed class ToolProcessHost : IToolProcessHost
{
    internal const string CodeGraphHostPpidVariable = "CODEGRAPH_HOST_PPID";
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
        var budget = request.MaximumOutputBytes;
        var exceeded = false;
        string? failure = null;
        int? exitCode = null;
        try
        {
            stopping.Token.ThrowIfCancellationRequested();
            process.Start();
            process.StandardInput.Close();
            var stdout = DrainAsync(process.StandardOutput.BaseStream, output);
            var stderr = DrainAsync(process.StandardError.BaseStream, error);
            try
            {
                await Task.WhenAll(process.WaitForExitAsync(stopping.Token), stdout, stderr).ConfigureAwait(false);
            }
            finally
            {
                if (!process.HasExited)
                {
                    try { process.Kill(entireProcessTree: true); }
                    catch (InvalidOperationException) when (process.HasExited) { }
                }
                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
                await Task.WhenAll(stdout, stderr).ConfigureAwait(ConfigureAwaitOptions.SuppressThrowing);
                exitCode = process.ExitCode;
            }
        }
        catch (OperationCanceledException) when (stopping.IsCancellationRequested) { }
        catch (Exception exception) when (exception is Win32Exception or IOException or InvalidOperationException)
        {
            failure = "The tool process couldn't be started or read.";
        }
        if (exceeded) failure = "The tool process exceeded its output limit.";
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
            while (await source.ReadAsync(buffer, stopping.Token).ConfigureAwait(false) is var count && count > 0)
            {
                var remaining = Interlocked.Add(ref budget, -count);
                var kept = Math.Clamp(remaining + count, 0, count);
                if (kept > 0) await destination.WriteAsync(buffer.AsMemory(0, kept), CancellationToken.None).ConfigureAwait(false);
                if (remaining < 0)
                {
                    exceeded = true;
                    await stopping.CancelAsync().ConfigureAwait(false);
                    return;
                }
            }
        }
    }
}
