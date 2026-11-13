using System.Diagnostics;
using System.Runtime.InteropServices;

namespace OpenForge.Cli.Composition.Shared.Tools;

// Tool Run's lifetime policy. Console interrupts belong to the shared console/process group.
internal sealed class ToolDispatchSignals : IDisposable
{
    internal const int HangupExitCode = 129;
    internal const int TerminateExitCode = 143;
    internal static readonly TimeSpan StopGracePeriod = TimeSpan.FromSeconds(1);
    private static readonly ConsoleCancelEventHandler CancelTermination = static (_, args) => args.Cancel = true;
    private readonly TaskCompletionSource<int> _stop = new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly List<PosixSignalRegistration> _registrations = [];

    internal ToolDispatchSignals()
    {
        Console.CancelKeyPress += CancelTermination;
        if (OperatingSystem.IsWindows()) return;
        _registrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGINT, static context => context.Cancel = true));
        _registrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGQUIT, static context => context.Cancel = true));
        _registrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGTERM, context =>
        {
            context.Cancel = true;
            _stop.TrySetResult(TerminateExitCode);
        }));
        _registrations.Add(PosixSignalRegistration.Create(PosixSignal.SIGHUP, context =>
        {
            context.Cancel = true;
            RequestHangup();
        }));
    }

    internal void RequestHangup() => _stop.TrySetResult(HangupExitCode);

    internal async Task<int> WaitAsync(Process process, Task exit)
    {
        await Task.WhenAny(exit, _stop.Task).ConfigureAwait(false);
        if (!_stop.Task.IsCompleted)
        {
            await exit.ConfigureAwait(false);
            return process.ExitCode;
        }
        var code = await _stop.Task.ConfigureAwait(false);
        if (await Task.WhenAny(exit, Task.Delay(StopGracePeriod)).ConfigureAwait(false) != exit)
        {
            try { process.Kill(entireProcessTree: true); }
            catch (InvalidOperationException) when (process.HasExited) { }
        }
        await exit.ConfigureAwait(false);
        return code;
    }

    public void Dispose()
    {
        foreach (var registration in _registrations) registration.Dispose();
        Console.CancelKeyPress -= CancelTermination;
    }
}
