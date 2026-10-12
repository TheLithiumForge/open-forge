using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

namespace OpenForge.Cli.Composition.Shared.RouteInspectMatchingFiles;

internal sealed class RouteInspectGitFileEnumerator
{
    private readonly GitProcessRunner _git;
    private readonly TimeSpan _deadline;
    private readonly TimeProvider _timeProvider;

    internal RouteInspectGitFileEnumerator(
        string gitExecutable = "git",
        Action<ProcessStartInfo>? configureProcess = null,
        TimeSpan? deadline = null,
        TimeProvider? timeProvider = null,
        Action<Process>? processStarted = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gitExecutable);
        _git = new GitProcessRunner(gitExecutable, configureProcess, processStarted);
        _timeProvider = timeProvider ?? TimeProvider.System;
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
                var probe = await _git.RunAsync(workspaceRoot, ["rev-parse", "--is-inside-work-tree"], deadline.Token).ConfigureAwait(false);
                if (probe is { ExitCode: 0 } confirmed && confirmed.Output.Trim() == "true")
                {
                    scope = RouteInspectMatchingFilesScope.GitTrackedAndUntracked;
                    if (string.IsNullOrWhiteSpace(confirmed.Error))
                    {
                        var inventory = await _git.RunAsync(workspaceRoot,
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

}
