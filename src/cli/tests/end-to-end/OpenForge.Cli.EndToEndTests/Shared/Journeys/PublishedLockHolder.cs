using System.Diagnostics;
using System.Text;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

/// <summary>
/// Holds one published workspace lock from a real, owned Windows PowerShell child.
/// The child is deliberately a cooperative test fixture, not a suspended CLI.
/// </summary>
internal sealed class PublishedLockHolder : IDisposable
{
    private const string LockPathVariable = "OPEN_FORGE_F20_LOCK_PATH";
    private const string MutationPathVariable = "OPEN_FORGE_F20_MUTATION_PATH";
    private const string LockedMarker = "LOCKED";
    private const string ReleasedMarker = "RELEASED";
    private static readonly TimeSpan HandshakeTimeout = TimeSpan.FromSeconds(10);
    private const string HolderScript = """
        $ErrorActionPreference = 'Stop'
        $lockPath = [Environment]::GetEnvironmentVariable('OPEN_FORGE_F20_LOCK_PATH')
        $mutationPath = [Environment]::GetEnvironmentVariable('OPEN_FORGE_F20_MUTATION_PATH')
        $lock = $null
        try {
            $lock = [System.IO.File]::Open(
                $lockPath,
                [System.IO.FileMode]::Open,
                [System.IO.FileAccess]::ReadWrite,
                [System.IO.FileShare]::None)
            [System.IO.File]::WriteAllText(
                $mutationPath,
                'Open Forge F20 first-writer marker',
                [System.Text.UTF8Encoding]::new($false))
            [Console]::Out.WriteLine('LOCKED')
            [Console]::Out.Flush()
            $command = [Console]::In.ReadLine()
            if ($command -cne 'release') {
                throw [System.InvalidOperationException]::new('The lock holder received an unexpected command.')
            }
        }
        finally {
            if ($null -ne $lock) {
                $lock.Dispose()
            }
        }
        [Console]::Out.WriteLine('RELEASED')
        [Console]::Out.Flush()
        exit 0
        """;

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
        string ownedMutationPath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(lockPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(ownedMutationPath);
        if (!OperatingSystem.IsWindows())
        {
            throw new PlatformNotSupportedException(
                "The published lock holder requires Windows PowerShell and Windows file sharing.");
        }

        ValidateAbsoluteOrdinaryZeroByteFile(lockPath, nameof(lockPath));
        ValidateAbsoluteMutationPath(ownedMutationPath);

        var powershellPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.System),
            "WindowsPowerShell",
            "v1.0",
            "powershell.exe");
        if (!File.Exists(powershellPath))
        {
            throw new FileNotFoundException(
                "The OS-provided Windows PowerShell executable is unavailable.",
                powershellPath);
        }

        var startInfo = new ProcessStartInfo
        {
            FileName = powershellPath,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            StandardErrorEncoding = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("-NoLogo");
        startInfo.ArgumentList.Add("-NoProfile");
        startInfo.ArgumentList.Add("-NonInteractive");
        startInfo.ArgumentList.Add("-EncodedCommand");
        startInfo.ArgumentList.Add(Convert.ToBase64String(Encoding.Unicode.GetBytes(HolderScript)));
        startInfo.Environment[LockPathVariable] = lockPath;
        startInfo.Environment[MutationPathVariable] = ownedMutationPath;

        var process = Process.Start(startInfo)
            ?? throw new InvalidOperationException("Unable to start the owned Windows PowerShell lock holder.");
        var holder = new PublishedLockHolder(process);
        try
        {
            await holder.WaitForLockedAsync().ConfigureAwait(false);
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
            await _standardInput.WriteLineAsync("release").ConfigureAwait(false);
            await _released.Task.WaitAsync(HandshakeTimeout).ConfigureAwait(false);
            await WaitForExitAsync().ConfigureAwait(false);
            await DrainAsync().ConfigureAwait(false);
            EnsureSuccessfulExit("release");
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
        await _locked.Task.WaitAsync(HandshakeTimeout).ConfigureAwait(false);
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

    private static void ValidateAbsoluteOrdinaryZeroByteFile(string path, string parameterName)
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

        if (new FileInfo(path).Length != 0)
        {
            throw new InvalidOperationException("The lock path must be the exact zero-byte lock file.");
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
