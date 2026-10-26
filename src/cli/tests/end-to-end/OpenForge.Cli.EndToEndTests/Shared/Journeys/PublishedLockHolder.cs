using System.Diagnostics;
using System.Text;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

/// <summary>
/// Holds a workspace lock from an owned child of the EndToEnd executable.
/// The child is deliberately a cooperative test fixture, not a suspended CLI.
/// </summary>
internal sealed class PublishedLockHolder : IDisposable
{
    internal const string ChildArgument = "--workspace-lock-holder";
    private const string LockedMarker = "LOCKED";
    private const string ReleasedMarker = "RELEASED";
    private const string ReadWriteAccess = "read-write";
    private const string ReadOnlyAccess = "read-only";
    private const string ReleaseCommand = "release";
    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(60);

    internal static int RunChild(string[] arguments)
    {
        try
        {
            if (arguments.Length != 4 || arguments[0] != ChildArgument)
            {
                throw new ArgumentException("Invalid lock holder arguments.");
            }

            var access = arguments[3] switch
            {
                ReadWriteAccess => FileAccess.ReadWrite,
                ReadOnlyAccess => FileAccess.Read,
                _ => throw new ArgumentException("Invalid lock holder access."),
            };
            ValidateAbsoluteOrdinaryFile(arguments[1], nameof(arguments));
            ValidateAbsoluteMutationPath(arguments[2]);
            using (var handle = new FileStream(arguments[1], FileMode.Open, access, FileShare.None))
            {
                File.WriteAllText(arguments[2], "Open Forge first-writer marker");
                Console.Out.WriteLine(LockedMarker);
                Console.Out.Flush();
                if (Console.In.ReadLine() != ReleaseCommand)
                {
                    throw new InvalidOperationException("The lock holder received an unexpected command.");
                }
            }

            Console.Out.WriteLine(ReleasedMarker);
            Console.Out.Flush();
            return 0;
        }
        catch (Exception exception)
        {
            Console.Error.WriteLine(exception);
            return 1;
        }
    }

    private readonly Process _process;
    private readonly StreamWriter _standardInput;
    private readonly Task _standardOutputReader;
    private readonly Task<string> _standardErrorReader;
    private readonly TaskCompletionSource<bool> _locked =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private readonly TaskCompletionSource<bool> _released =
        new(TaskCreationOptions.RunContinuationsAsynchronously);
    private bool _terminalCommandSent;
    private bool _disposed;

    private PublishedLockHolder(Process process)
    {
        _process = process;
        _standardInput = process.StandardInput;
        _standardInput.AutoFlush = true;
        _standardErrorReader = process.StandardError.ReadToEndAsync(CancellationToken.None);
        _standardOutputReader = ReadStandardOutputAsync(process.StandardOutput);
        ProcessId = process.Id;
    }

    internal int ProcessId { get; }

    internal static async Task<PublishedLockHolder> StartAsync(
        string lockPath,
        string ownedMutationPath,
        bool readOnly = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownedMutationPath);
        ValidateAbsoluteOrdinaryFile(lockPath, nameof(lockPath));
        ValidateAbsoluteMutationPath(ownedMutationPath);
        var executable = Environment.ProcessPath
            ?? throw new InvalidOperationException("The test process executable is unavailable.");

        var startInfo = new ProcessStartInfo
        {
            FileName = executable,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        if (string.Equals(Path.GetFileNameWithoutExtension(executable), "dotnet", StringComparison.OrdinalIgnoreCase))
        {
            startInfo.ArgumentList.Add(Path.Combine(AppContext.BaseDirectory, "OpenForge.Cli.EndToEndTests.dll"));
        }

        startInfo.ArgumentList.Add(ChildArgument);
        startInfo.ArgumentList.Add(lockPath);
        startInfo.ArgumentList.Add(ownedMutationPath);
        startInfo.ArgumentList.Add(readOnly ? ReadOnlyAccess : ReadWriteAccess);

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start the owned test lock holder.");
        var holder = new PublishedLockHolder(process);
        var started = Stopwatch.GetTimestamp();
        try
        {
            await holder.WaitForLockedAsync().ConfigureAwait(false);
            TestContext.Current.TestOutputHelper?.WriteLine(
                $"Lock holder LOCKED: {Stopwatch.GetElapsedTime(started).TotalMilliseconds:F1} ms");
            return holder;
        }
        catch (Exception startFailure)
        {
            try
            {
                await holder.StopAndDrainAsync().ConfigureAwait(false);
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException(
                    "The lock holder failed its LOCKED handshake and could not be terminated cleanly.",
                    startFailure,
                    cleanupFailure);
            }

            var standardError = holder.StandardError;
            holder.Dispose();
            throw new InvalidOperationException(
                $"The lock holder failed its LOCKED handshake. stderr: {standardError}",
                startFailure);
        }
    }

    internal async Task ReleaseAsync()
    {
        ThrowIfDisposed();
        if (_terminalCommandSent)
        {
            return;
        }

        try
        {
            await _standardInput.WriteLineAsync(ReleaseCommand).ConfigureAwait(false);
            await _released.Task.WaitAsync(HandshakeTimeout).ConfigureAwait(false);
            await WaitForExitAsync().ConfigureAwait(false);
            await DrainAsync().ConfigureAwait(false);
            EnsureSuccessfulExit(ReleaseCommand);
            _terminalCommandSent = true;
        }
        catch (Exception releaseFailure)
        {
            try
            {
                await StopAndDrainAsync().ConfigureAwait(false);
            }
            catch (Exception cleanupFailure)
            {
                throw new AggregateException(
                    "The lock holder failed to release and could not be terminated cleanly.",
                    releaseFailure,
                    cleanupFailure);
            }

            throw new InvalidOperationException(
                $"The lock holder failed to release. stderr: {StandardError}",
                releaseFailure);
        }
    }

    internal async Task CrashAsync()
    {
        ThrowIfDisposed();
        if (_terminalCommandSent)
        {
            return;
        }

        if (_process.HasExited)
        {
            await DrainAsync().ConfigureAwait(false);
            throw new InvalidOperationException(
                $"The lock holder exited before CrashAsync. stderr: {StandardError}");
        }

        _process.Kill(entireProcessTree: false);
        await WaitForExitAsync().ConfigureAwait(false);
        await DrainAsync().ConfigureAwait(false);
        if (!_process.HasExited)
        {
            throw new InvalidOperationException("CrashAsync did not terminate the owned lock holder.");
        }

        _terminalCommandSent = true;
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        Exception? failure = null;
        try
        {
            if (!_process.HasExited)
            {
                try
                {
                    _process.Kill(entireProcessTree: false);
                    if (!_process.WaitForExit((int)HandshakeTimeout.TotalMilliseconds))
                    {
                        throw new TimeoutException("The lock holder did not terminate during disposal.");
                    }
                }
                catch (Exception exception)
                {
                    failure = exception;
                }
            }

            try
            {
                Task.WhenAll(_standardOutputReader, _standardErrorReader)
                    .WaitAsync(HandshakeTimeout)
                    .GetAwaiter()
                    .GetResult();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }
        }
        finally
        {
            try
            {
                _standardInput.Dispose();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                _process.StandardOutput.Dispose();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                _process.StandardError.Dispose();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            try
            {
                _process.Dispose();
            }
            catch (Exception exception)
            {
                failure = Combine(failure, exception);
            }

            _disposed = true;
        }

        if (failure is not null)
        {
            throw failure;
        }
    }

    private string StandardError => _standardErrorReader.GetAwaiter().GetResult();

    private async Task WaitForLockedAsync()
    {
        await _locked.Task.WaitAsync(HandshakeTimeout, TestContext.Current.CancellationToken).ConfigureAwait(false);
        if (_process.HasExited)
        {
            throw new InvalidOperationException(
                $"The lock holder exited after LOCKED. stderr: {StandardError}");
        }
    }

    private async Task WaitForExitAsync()
    {
        using var cancellation = new CancellationTokenSource(HandshakeTimeout);
        try
        {
            await _process.WaitForExitAsync(cancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException exception)
        {
            throw new TimeoutException("The lock holder did not terminate within the bounded deadline.", exception);
        }
    }

    private async Task StopAndDrainAsync()
    {
        if (!_process.HasExited)
        {
            _process.Kill(entireProcessTree: false);
        }

        await WaitForExitAsync().ConfigureAwait(false);
        await DrainAsync().ConfigureAwait(false);
    }

    private async Task DrainAsync()
        => await Task.WhenAll(_standardOutputReader, _standardErrorReader)
            .WaitAsync(HandshakeTimeout).ConfigureAwait(false);

    private void EnsureSuccessfulExit(string operation)
    {
        if (_process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"The lock holder {operation} command exited with {_process.ExitCode}. stderr: {StandardError}");
        }
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }

    private async Task ReadStandardOutputAsync(StreamReader reader)
    {
        try
        {
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
            {
                if (string.Equals(line, LockedMarker, StringComparison.Ordinal))
                {
                    _locked.TrySetResult(true);
                }
                else if (string.Equals(line, ReleasedMarker, StringComparison.Ordinal))
                {
                    _released.TrySetResult(true);
                }
            }

            _locked.TrySetException(new InvalidOperationException(
                "The lock holder output ended before the LOCKED handshake completed."));
            _released.TrySetException(new InvalidOperationException(
                "The lock holder output ended before the RELEASED handshake completed."));
        }
        catch (Exception exception)
        {
            _locked.TrySetException(exception);
            _released.TrySetException(exception);
            throw;
        }
    }

    private static Exception Combine(Exception? first, Exception second)
        => first is null ? second : new AggregateException(first, second);

    private static void ValidateAbsoluteOrdinaryFile(string path, string parameterName)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The lock path must be absolute.", parameterName);
        }

        var attributes = File.GetAttributes(path);
        if ((attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
        {
            throw new InvalidOperationException("The lock path is not an ordinary file.");
        }

    }

    private static void ValidateAbsoluteMutationPath(string path)
    {
        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("The owned mutation path must be absolute.", nameof(path));
        }

        var parent = Path.GetDirectoryName(path)
            ?? throw new ArgumentException("The owned mutation path has no parent directory.", nameof(path));
        if (!Directory.Exists(parent))
        {
            throw new DirectoryNotFoundException(parent);
        }
    }
}
