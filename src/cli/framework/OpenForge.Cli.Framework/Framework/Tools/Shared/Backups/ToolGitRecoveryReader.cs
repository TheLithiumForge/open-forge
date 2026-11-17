using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

// Git covers only a tracked blob whose raw bytes match HEAD. A working tree alone gives no coverage.
internal sealed class ToolGitRecoveryReader(IToolProcessHost processes)
{
    private const int MaximumOutputBytes = 16 * 1024;
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(10);

    internal async Task<string?> ReadAsync(string target, CancellationToken token)
    {
        var parent = Path.GetDirectoryName(target) ?? throw new IOException("The backup target has no parent.");
        if (!Directory.Exists(parent)) return null;
        var resolved = await processes.ResolveAsync(ToolGitEnvironment.Executable, parent, token).ConfigureAwait(false);
        if (resolved is null) return null;
        var root = await RunAsync(resolved.Invocation, parent, ["rev-parse", "--show-toplevel"], token).ConfigureAwait(false);
        if (root is null || !Path.IsPathFullyQualified(root)) return null;
        var relative = Path.GetRelativePath(root, target).Replace(Path.DirectorySeparatorChar, '/');
        if (await RunAsync(resolved.Invocation, parent, ["ls-files", "--error-unmatch", "--", target], token).ConfigureAwait(false) is null) return null;
        var head = await RunAsync(resolved.Invocation, parent, ["rev-parse", "--verify", "HEAD:" + relative], token).ConfigureAwait(false);
        var current = await RunAsync(resolved.Invocation, parent, ["hash-object", "--no-filters", "--", target], token).ConfigureAwait(false);
        if (head is null || head.Length is not (40 or 64) || !head.All(char.IsAsciiHexDigit)) return null;
        return string.Equals(head, current, StringComparison.Ordinal) ? head : null;
    }

    private async Task<string?> RunAsync(ToolCommandTarget invocation, string directory, string[] arguments, CancellationToken token)
    {
        var result = await processes.RunAsync(new()
        {
            ExecutablePath = invocation.ExecutablePath,
            Arguments = invocation.PrefixArguments.Concat(arguments).ToArray(),
            WorkingDirectory = directory,
            Environment = ToolGitEnvironment.Probe,
            Timeout = Timeout,
            MaximumOutputBytes = MaximumOutputBytes,
        }, token).ConfigureAwait(false);
        token.ThrowIfCancellationRequested();
        if (result.Cancelled) throw new OperationCanceledException("The Git backup probe was cancelled.", token);
        return result is { ExitCode: 0, TimedOut: false, Failure: null } ? result.StandardOutput.TrimEnd('\r', '\n') : null;
    }
}
