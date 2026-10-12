using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Composition.Shared.Git;

namespace OpenForge.Cli.Composition.Shared.RouteReferences;

internal sealed class RouteGitIgnoredPathEnumerator
{
    private static readonly TimeSpan GitDeadline = TimeSpan.FromSeconds(5);
    private readonly GitProcessRunner _git;

    internal RouteGitIgnoredPathEnumerator(
        string gitExecutable = "git",
        Action<ProcessStartInfo>? configureProcess = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(gitExecutable);
        _git = new GitProcessRunner(gitExecutable, configureProcess);
    }

    internal async ValueTask<ImmutableArray<string>?> EnumerateAsync(
        string workspaceRoot,
        CancellationToken cancellationToken)
    {
        using var timeout = new CancellationTokenSource(GitDeadline);
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        try
        {
            var probe = await _git.RunAsync(
                workspaceRoot, ["rev-parse", "--is-inside-work-tree"], deadline.Token).ConfigureAwait(false);
            if (probe is not { ExitCode: 0 } confirmed
                || confirmed.Output.Trim() != "true"
                || !string.IsNullOrWhiteSpace(confirmed.Error))
            {
                return null;
            }

            // --directory collapses ignored trees, including junctions, without reading their contents.
            // Tracked files remain eligible even when their parent matches an ignore rule.
            var inventory = await _git.RunAsync(
                workspaceRoot,
                ["ls-files", "--others", "--ignored", "--exclude-standard", "--directory", "-z", "--", "."],
                deadline.Token).ConfigureAwait(false);
            if (inventory is not { ExitCode: 0 } files || !string.IsNullOrWhiteSpace(files.Error)
                || files.Output.Length > 0 && (files.Output[^1] != '\0'
                    || files.Output[0] == '\0' || files.Output.Contains("\0\0", StringComparison.Ordinal)))
            {
                return null;
            }

            var paths = ImmutableArray.CreateBuilder<string>();
            foreach (var path in files.Output.Split('\0', StringSplitOptions.RemoveEmptyEntries))
            {
                deadline.Token.ThrowIfCancellationRequested();
                var canonicalPath = path.TrimEnd('/');
                paths.Add(SourceWorkspaceRelativePath.Validate(canonicalPath, nameof(inventory), allowWorkspaceRoot: false));
            }

            return paths.ToImmutable();
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return null;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException
            or DecoderFallbackException or ArgumentException or InvalidOperationException)
        {
            return null;
        }
    }
}
