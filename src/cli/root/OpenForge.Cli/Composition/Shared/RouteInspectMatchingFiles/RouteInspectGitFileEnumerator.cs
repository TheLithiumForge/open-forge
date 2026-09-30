using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

namespace OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;

internal sealed class RouteInspectGitFileEnumerator
{
    private readonly string _gitExecutable;
    private readonly Action<ProcessStartInfo>? _configureProcess;
    private readonly TimeSpan _deadline;
    private readonly TimeProvider _timeProvider;
    private readonly Action<Process>? _processStarted;

    internal RouteInspectGitFileEnumerator(
        string gitExecutable = "git",
        Action<ProcessStartInfo>? configureProcess = null,
        TimeSpan? deadline = null,
        TimeProvider? timeProvider = null,
        Action<Process>? processStarted = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gitExecutable);
        _gitExecutable = gitExecutable;
        _configureProcess = configureProcess;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _processStarted = processStarted;
        _deadline = deadline ?? Core.Commands.Route.Inspect.Models.Result.RouteInspectMatchingFiles.ScanDeadline;
        if (_deadline <= TimeSpan.Zero || _deadline > Core.Commands.Route.Inspect.Models.Result.RouteInspectMatchingFiles.ScanDeadline)
        {
            throw new ArgumentOutOfRangeException(nameof(deadline));
        }
    }

    internal async ValueTask<RouteInspectFileEnumeration> EnumerateAsync(string workspaceRoot, CancellationToken cancellationToken)
    {
        RouteInspectMatchingFilesScope? scope = null;
        var started = _timeProvider.GetTimestamp();
        using var timeout = new CancellationTokenSource(_deadline, _timeProvider);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);

        void CheckDeadline()
        {
            if (_timeProvider.GetElapsedTime(started) >= _deadline)
            {
                timeout.Cancel();
            }

            deadline.Token.ThrowIfCancellationRequested();
        }

        try
        {
            CheckDeadline();
            try
            {
                var probe = await RunGitAsync(workspaceRoot, ["rev-parse", "--is-inside-work-tree"], deadline.Token).ConfigureAwait(false);
                if (probe is { ExitCode: 0 } confirmed && confirmed.Output.Trim() == "true")
                {
                    scope = RouteInspectMatchingFilesScope.GitTrackedAndUntracked;
                    if (string.IsNullOrWhiteSpace(confirmed.Error))
                    {
                        var inventory = await RunGitAsync(workspaceRoot,
                            ["ls-files", "--cached", "--others", "--exclude-standard", "-z", "--", "."], deadline.Token).ConfigureAwait(false);
                        if (inventory is { ExitCode: 0 } files && string.IsNullOrWhiteSpace(files.Error)
                            && (files.Output.Length == 0 || files.Output[^1] == '\0'
                                && files.Output[0] != '\0' && !files.Output.Contains("\0\0", StringComparison.Ordinal)))
                        {
                            var paths = new List<string>();
                            foreach (var range in files.Output.AsSpan().Split('\0'))
                            {
                                CheckDeadline();
                                if (!files.Output.AsSpan(range).IsEmpty)
                                {
                                    paths.Add(files.Output[range]);
                                }
                            }

                            var result = RouteInspectFileEnumeration.Success(scope.Value, paths);
                            CheckDeadline();
                            return result;
                        }
                    }
                }
            }
            catch (Exception exception) when (exception is DecoderFallbackException or IOException or UnauthorizedAccessException)
            {
                // An unusable Git inventory falls back under the same deadline.
            }

            CheckDeadline();
            scope = RouteInspectMatchingFilesScope.WorkspaceFiles;
            var walked = RouteInspectWorkspaceFileWalker.Enumerate(workspaceRoot, deadline.Token);
            CheckDeadline();
            return walked;
        }
        catch (OperationCanceledException)
        {
            return RouteInspectFileEnumeration.Unavailable(scope, cancellationToken.IsCancellationRequested
                ? RouteInspectMatchingFilesReason.Cancelled
                : RouteInspectMatchingFilesReason.ScanTimeout);
        }
        catch (Exception exception) when (exception is DecoderFallbackException or IOException or UnauthorizedAccessException)
        {
            return RouteInspectFileEnumeration.Unavailable(scope, RouteInspectMatchingFilesReason.FilesUnavailable);
        }
        catch (Exception)
        {
            return RouteInspectFileEnumeration.Unavailable(scope, RouteInspectMatchingFilesReason.ScanFailed);
        }
    }

    private async Task<(int ExitCode, string Output, string Error)?> RunGitAsync(string root, string[] arguments, CancellationToken cancellationToken)
    {
        var start = new ProcessStartInfo(_gitExecutable)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            StandardOutputEncoding = new UTF8Encoding(false, true),
        };
        start.ArgumentList.Add("--no-optional-locks");
        start.ArgumentList.Add("-c");
        start.ArgumentList.Add("core.fsmonitor=false");
        start.ArgumentList.Add("-C");
        start.ArgumentList.Add(root);
        foreach (var argument in arguments)
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
        _configureProcess?.Invoke(start);
        using var process = new Process { StartInfo = start };
        try
        {
            process.Start();
        }
        catch (Win32Exception)
        {
            return null;
        }

        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        try
        {
            _processStarted?.Invoke(process);
            await process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
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

                await process.WaitForExitAsync(CancellationToken.None).ConfigureAwait(false);
            }

            try
            {
                await Task.WhenAll(output, error).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                // Both readers are observed after the child has been reaped.
            }
        }
    }
}
