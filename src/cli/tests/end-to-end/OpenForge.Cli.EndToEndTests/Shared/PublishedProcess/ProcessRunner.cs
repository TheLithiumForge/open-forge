using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

/// <summary>
/// Describes one process invocation without constructing a shell command line.
/// </summary>
internal sealed class ProcessRunRequest
{
    /// <summary>
    /// Creates a process invocation.
    /// </summary>
    public ProcessRunRequest(
        string executablePath,
        IEnumerable<string> arguments,
        string workingDirectory,
        TimeSpan? timeout = null,
        Action<int>? processStarted = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(executablePath);
        ArgumentNullException.ThrowIfNull(arguments);
        ArgumentException.ThrowIfNullOrWhiteSpace(workingDirectory);
        ValidateTimeout(timeout);

        var argumentCopy = arguments.ToArray();
        if (argumentCopy.Any(argument => argument is null))
        {
            throw new ArgumentException("Process arguments cannot contain null values.", nameof(arguments));
        }

        ExecutablePath = executablePath;
        Arguments = new ReadOnlyCollection<string>(argumentCopy);
        WorkingDirectory = workingDirectory;
        Timeout = timeout;
        ProcessStarted = processStarted;
    }

    /// <summary>
    /// Gets the executable path or executable name resolved by the operating system.
    /// </summary>
    public string ExecutablePath { get; }

    /// <summary>
    /// Gets the individual arguments passed through ProcessStartInfo.ArgumentList.
    /// </summary>
    public IReadOnlyList<string> Arguments { get; }

    /// <summary>
    /// Gets the explicit process working directory.
    /// </summary>
    public string WorkingDirectory { get; }

    /// <summary>
    /// Gets the optional process lifetime. A null or infinite value means no timeout.
    /// </summary>
    public TimeSpan? Timeout { get; }

    /// <summary>
    /// Gets an optional observer invoked once with the started child process ID.
    /// </summary>
    public Action<int>? ProcessStarted { get; }

    private static void ValidateTimeout(TimeSpan? timeout)
    {
        if (timeout is null
            || timeout == System.Threading.Timeout.InfiniteTimeSpan
            || timeout > TimeSpan.Zero)
        {
            return;
        }

        throw new ArgumentOutOfRangeException(nameof(timeout), "The process timeout must be positive or infinite.");
    }
}

/// <summary>
/// Immutable captured output and exit status from one completed process.
/// </summary>
internal sealed record ProcessRunResult(int ExitCode, string StandardOutput, string StandardError);

/// <summary>
/// Reports cancellation after an owned child has been stopped and drained.
/// </summary>
internal sealed class ProcessRunCanceledException : OperationCanceledException
{
    /// <summary>
    /// Creates a cancellation result for one started child.
    /// </summary>
    public ProcessRunCanceledException(int processId, bool killRequested, CancellationToken cancellationToken)
        : base("The owned process run was cancelled.", cancellationToken)
    {
        ProcessId = processId;
        KillRequested = killRequested;
    }

    /// <summary>
    /// Gets the child process ID.
    /// </summary>
    public int ProcessId { get; }

    /// <summary>
    /// Gets whether the runner requested termination before draining the child.
    /// </summary>
    public bool KillRequested { get; }
}

/// <summary>
/// Starts actual executables with explicit process-start options and no shell.
/// </summary>
internal static class ProcessRunner
{
    /// <summary>
    /// Runs a process to completion and captures standard output and standard
    /// error separately. Caller cancellation kills and awaits the owned process;
    /// a configured timeout does the same and then throws TimeoutException.
    /// </summary>
    public static async Task<ProcessRunResult> RunAsync(
        ProcessRunRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var startInfo = CreateStartInfo(request);
        using var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException($"Unable to start executable: {request.ExecutablePath}");
        var stdout = process.StandardOutput.ReadToEndAsync(CancellationToken.None);
        var stderr = process.StandardError.ReadToEndAsync(CancellationToken.None);
        using var timeoutSource = CreateTimeoutSource(request.Timeout);
        using var linkedSource = timeoutSource is null
            ? null
            : CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutSource.Token);
        var waitToken = linkedSource?.Token ?? cancellationToken;

        try
        {
            request.ProcessStarted?.Invoke(process.Id);
            cancellationToken.ThrowIfCancellationRequested();
            await process.WaitForExitAsync(waitToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (waitToken.IsCancellationRequested)
        {
            var killRequested = await StopAndDrainAsync(process, stdout, stderr).ConfigureAwait(false);
            if (timeoutSource?.IsCancellationRequested == true
                && !cancellationToken.IsCancellationRequested
                && request.Timeout is { } timeout)
            {
                throw new TimeoutException(
                    $"Executable '{request.ExecutablePath}' did not exit within {timeout}.");
            }

            if (cancellationToken.IsCancellationRequested)
            {
                throw new ProcessRunCanceledException(process.Id, killRequested, cancellationToken);
            }

            throw;
        }
        catch
        {
            await StopAndDrainAsync(process, stdout, stderr).ConfigureAwait(false);
            throw;
        }

        try
        {
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        }
        catch
        {
            await StopAndDrainAsync(process, stdout, stderr).ConfigureAwait(false);
            throw;
        }

        return new ProcessRunResult(process.ExitCode, stdout.Result, stderr.Result);
    }

    private static ProcessStartInfo CreateStartInfo(ProcessRunRequest request)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = request.ExecutablePath,
            WorkingDirectory = request.WorkingDirectory,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        foreach (var argument in request.Arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        return startInfo;
    }

    private static CancellationTokenSource? CreateTimeoutSource(TimeSpan? timeout)
    {
        if (timeout is null || timeout == System.Threading.Timeout.InfiniteTimeSpan)
        {
            return null;
        }

        return new CancellationTokenSource(timeout.Value);
    }

    private static async Task<bool> StopAndDrainAsync(
        Process process,
        Task<string> stdout,
        Task<string> stderr)
    {
        var killRequested = false;
        try
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                killRequested = true;
            }
        }
        catch (InvalidOperationException) when (HasExited(process))
        {
        }
        catch (Win32Exception) when (HasExited(process))
        {
        }

        try
        {
            await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
        }
        catch (InvalidOperationException) when (HasExited(process))
        {
        }

        try
        {
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
        }
        catch (IOException)
        {
        }
        catch (ObjectDisposedException)
        {
        }

        return killRequested;
    }

    private static bool HasExited(Process process)
    {
        try
        {
            return process.HasExited;
        }
        catch (InvalidOperationException)
        {
            return true;
        }
    }
}
