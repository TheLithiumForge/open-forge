using System.Collections.Immutable;
using System.Diagnostics;
using System.Text;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Composition.Shared.Git;
using OpenForge.Cli.Composition.Shared.Git.Models;

namespace OpenForge.Cli.Composition.Shared.RouteReferences;

internal sealed class RouteGitIgnoredPathEnumerator
{
    private readonly GitProcessRunner _git;
    private readonly GitInvocationDeadline _deadline = new();

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
        try
        {
            var probe = GitWorkTreeCheck.Read(await Run(GitWorkTreeCheck.Arguments).ConfigureAwait(false));
            if (probe is not { State: GitWorkTreeState.Inside, ExitCode: 0 })
            {
                return null;
            }

            // --directory collapses ignored trees, including junctions, without reading their contents.
            // Tracked files remain eligible even when their parent matches an ignore rule.
            var inventory = await Run(
                ["ls-files", "--others", "--ignored", "--exclude-standard", "--directory", "-z", "--", "."]).ConfigureAwait(false);
            if (inventory is not { ExitCode: 0 } files || GitOutput.IsUntrusted(files)
                || GitOutput.ReadNulEntries(files.Output) is not { } entries)
            {
                return null;
            }

            var paths = ImmutableArray.CreateBuilder<string>();
            foreach (var path in entries)
            {
                cancellationToken.ThrowIfCancellationRequested();
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

        Task<(int ExitCode, string Output, string Error)?> Run(string[] arguments)
            => _deadline.RunAsync(token => _git.RunAsync(workspaceRoot, arguments, token), cancellationToken);
    }
}
