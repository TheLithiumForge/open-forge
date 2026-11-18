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
                var usable = await GitWorkspaceFileList.ProbeAsync(_git, workspaceRoot, deadline.Token).ConfigureAwait(false);
                if (usable is not null)
                {
                    scope = RouteInspectMatchingFilesScope.GitTrackedAndUntracked;
                }

                if (usable == true
                    && await GitWorkspaceFileList.ListAsync(_git, workspaceRoot, CheckDeadline, deadline.Token).ConfigureAwait(false) is { } paths)
                {
                    var result = RouteInspectFileEnumeration.Success(RouteInspectMatchingFilesScope.GitTrackedAndUntracked, paths);
                    CheckDeadline();
                    return result;
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
