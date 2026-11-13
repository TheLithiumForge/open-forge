using System.Diagnostics;
using OpenForge.Cli.Core.Framework.Tools.Models.Execution;

namespace OpenForge.Cli.Composition.Shared.Tools;

// Registration point for ToolDispatchProcess. No shell, captured output or execution timeout.
internal static class ToolDispatchProcessHost
{
    internal static async ValueTask<int> RunAsync(ToolDispatchRequest request, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!Path.IsPathFullyQualified(request.Launch.Executable) || !Path.IsPathFullyQualified(request.WorkingDirectory))
            throw new ArgumentException("Dispatch requires an absolute executable and working directory.", nameof(request));
        var relayInput = Console.IsInputRedirected && request.Launch.McpArguments is { } vectors
            && vectors.Any(arguments => arguments.SequenceEqual(request.Arguments, StringComparer.Ordinal));
        var start = new ProcessStartInfo(request.Launch.Executable)
        {
            UseShellExecute = false,
            WorkingDirectory = request.WorkingDirectory,
            RedirectStandardInput = relayInput,
            RedirectStandardOutput = false,
            RedirectStandardError = false,
            CreateNoWindow = false,
        };
        foreach (var argument in request.Launch.PrefixArguments) start.ArgumentList.Add(argument);
        foreach (var argument in request.Arguments) start.ArgumentList.Add(argument);
        foreach (var (key, value) in request.Launch.Environment) start.Environment[key] = value;
        token.ThrowIfCancellationRequested();
        using var signals = new ToolDispatchSignals();
        using var process = new Process { StartInfo = start };
        if (!process.Start()) throw new InvalidOperationException("The verified tool process couldn't start.");
        using var inputCancellation = new CancellationTokenSource();
        using var input = relayInput ? Console.OpenStandardInput() : null;
        var forwarding = input is null ? Task.CompletedTask : ToolDispatchInput.RelayAsync(input, process.StandardInput.BaseStream, signals, inputCancellation.Token);
        try { return await signals.WaitAsync(process, process.WaitForExitAsync(CancellationToken.None)).ConfigureAwait(false); }
        finally
        {
            inputCancellation.Cancel();
            // A console pipe read may stay blocked until this host exits. It never delays the child's exit.
            if (forwarding.IsCompleted) await forwarding.ConfigureAwait(false);
        }
    }
}
