using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Models.Services;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;
using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;
using System.Globalization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal sealed class ToolEffectiveIgnore(IToolProcessHost processes)
{
    internal const string IgnoreFileName = ".gitignore";
    private const string NativeDataRules = $"*\n!{IgnoreFileName}";

    internal async Task<bool> IsIgnoredAsync(ToolRecipeContext context, string path, CancellationToken token)
    {
        var scope = ToolFileReader.Scope(context.WorkspacePath);
        var clean = path.TrimEnd('/');
        var absolute = ToolFileReader.Target(scope, clean);
        var state = ToolFileReader.Observe(scope, absolute, token);
        if (state is not (NoFollowLeafState.Missing or NoFollowLeafState.Directory or NoFollowLeafState.OrdinaryFile))
            throw new IOException("The effective-ignore target has a linked or unreadable path.");
        if (path.EndsWith('/') || Directory.Exists(absolute)) clean += "/";
        var git = await processes.ResolveAsync(ToolGitEnvironment.Executable, context.WorkspacePath, token).ConfigureAwait(false)
            ?? throw new IOException("Git is unavailable for the effective-ignore condition.");
        return await CoverageAsync(git.Invocation, context.WorkspacePath, clean, token).ConfigureAwait(false);
    }

    internal async Task RequireCoverageAsync(ToolRecipeContext context, ToolMarkedBlockChange change, byte[]? candidate, CancellationToken token)
    {
        var condition = change.WhenNotIgnored ?? throw new InvalidDataException("The block has no effective-ignore condition.");
        var git = await processes.ResolveAsync(ToolGitEnvironment.Executable, context.WorkspacePath, token).ConfigureAwait(false)
            ?? throw new IOException("Git is unavailable for the effective-ignore condition.");
        var root = await ReadPathAsync(git.Invocation, context.WorkspacePath, "--show-toplevel", token).ConfigureAwait(false);
        var gitDirectory = await ReadPathAsync(git.Invocation, context.WorkspacePath, "--absolute-git-dir", token).ConfigureAwait(false);
        var scope = ToolFileReader.Scope(root);
        var workspaceScope = ToolFileReader.Scope(context.WorkspacePath);
        var target = ToolFileReader.Target(workspaceScope, change.PathTemplate);
        var data = ToolFileReader.Target(workspaceScope, condition.WorkspaceRelativePath.TrimEnd('/'));
        var relativeData = Path.GetRelativePath(root, data).Replace(Path.DirectorySeparatorChar, '/');
        _ = ToolFileReader.Target(scope, relativeData);
        var relativeTarget = Path.GetRelativePath(root, target).Replace(Path.DirectorySeparatorChar, '/');
        _ = ToolFileReader.Target(scope, relativeTarget);
        var preview = Directory.CreateTempSubdirectory("open-forge-ignore-");
        try
        {
            if (condition.WorkspaceRelativePath.EndsWith('/') || Directory.Exists(data))
                Directory.CreateDirectory(ToolFileReader.Target(ToolFileReader.Scope(preview.FullName), relativeData));
            var current = condition.WorkspaceRelativePath.EndsWith('/') || Directory.Exists(data) ? data : Path.GetDirectoryName(data);
            while (current is not null)
            {
                var ignore = Path.Combine(current, IgnoreFileName);
                var bytes = ToolNativePaths.Comparer.Equals(ignore, target) ? candidate
                    : (await ToolFileReader.ReadAsync(scope, ignore, token).ConfigureAwait(false)).Bytes.ToArray();
                if (bytes is { Length: > 0 })
                {
                    var destination = ToolFileReader.Target(ToolFileReader.Scope(preview.FullName), Path.GetRelativePath(root, ignore).Replace(Path.DirectorySeparatorChar, '/'));
                    Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new IOException("The ignore preview has no parent."));
                    await File.WriteAllBytesAsync(destination, bytes, token).ConfigureAwait(false);
                }
                if (ToolNativePaths.Comparer.Equals(current, scope.LexicalRoot)) break;
                current = Path.GetDirectoryName(current);
            }
            if (candidate is not null)
            {
                var destination = ToolFileReader.Target(ToolFileReader.Scope(preview.FullName), relativeTarget);
                Directory.CreateDirectory(Path.GetDirectoryName(destination) ?? throw new IOException("The ignore preview has no parent."));
                await File.WriteAllBytesAsync(destination, candidate, token).ConfigureAwait(false);
            }
            if (condition.WorkspaceRelativePath.EndsWith('/') || Directory.Exists(data)) relativeData += "/";
            var previewGit = git.Invocation with { PrefixArguments = git.Invocation.PrefixArguments.Concat(new[] { $"--git-dir={gitDirectory}", $"--work-tree={preview.FullName}" }).ToArray() };
            var covered = await CoverageAsync(previewGit, preview.FullName, relativeData, token).ConfigureAwait(false);
            if (!covered) throw new InvalidDataException("The supported marked-block edit does not provide effective ignore coverage.");
        }
        finally
        {
            if (preview.LinkTarget is not null) throw new IOException("The owned ignore preview became linked.");
            preview.Delete(recursive: true);
        }
    }

    private async Task<bool> CoverageAsync(ToolCommandTarget git, string directory, string path, CancellationToken token)
    {
        var clean = path.TrimEnd('/');
        if (await CheckAsync(git, directory, ["check-ignore", "--no-index", "--quiet", "--", clean], token).ConfigureAwait(false)) return true;
        if (!path.EndsWith('/') || !await CheckAsync(git, directory, ["check-ignore", "--no-index", "--quiet", "--", path], token).ConfigureAwait(false)) return false;
        var match = await RunAsync(git, directory, ["check-ignore", "--no-index", "--verbose", "--", path], token).ConfigureAwait(false);
        RequireResult(match);
        var (sourceName, pattern) = ReadMatch(match.StandardOutput);
        if (sourceName.StartsWith('"')) return false;
        var root = await ReadPathAsync(git, directory, "--show-toplevel", token).ConfigureAwait(false);
        var source = Path.GetFullPath(Path.IsPathFullyQualified(sourceName) ? sourceName : Path.Combine(root, sourceName));
        var scope = ToolFileReader.Scope(directory);
        var internalIgnore = ToolFileReader.Target(scope, clean + "/" + IgnoreFileName);
        if (!ToolNativePaths.Comparer.Equals(source, internalIgnore))
            return pattern.EndsWith('/') && pattern.IndexOfAny(['*', '?', '[']) < 0;
        // Only the native all-data forms establish internal coverage. Other forms are uncertain.
        // Git still decides matching and precedence. This does not interpret arbitrary ignore patterns.
        var state = await ToolFileReader.ReadAsync(scope, internalIgnore, token).ConfigureAwait(false);
        if (!state.HasBytes) return false;
        var text = ToolConfigBytes.Utf8.GetString(state.Bytes.AsSpan()).ReplaceLineEndings("\n").TrimStart('\uFEFF').TrimEnd('\n');
        return text is "*" or NativeDataRules;
    }

    private static (string Source, string Pattern) ReadMatch(string output)
    {
        var delimiter = output.LastIndexOf('\t');
        if (delimiter < 0 || output.AsSpan(0, delimiter).IndexOfAny('\r', '\n') >= 0)
            throw new IOException("Git returned an incomplete ignore match.");
        var header = output.AsSpan(0, delimiter);
        var colon = header.IndexOf(':');
        while (colon >= 0)
        {
            var next = header[(colon + 1)..].IndexOf(':');
            if (next < 0) break;
            next += colon + 1;
            if (int.TryParse(header[(colon + 1)..next], NumberStyles.None, CultureInfo.InvariantCulture, out var line) && line > 0)
                return (header[..colon].ToString(), header[(next + 1)..].ToString());
            colon = next;
        }
        throw new IOException("Git returned an incomplete ignore match.");
    }

    private async Task<string> ReadPathAsync(ToolCommandTarget git, string directory, string argument, CancellationToken token)
    {
        var result = await RunAsync(git, directory, ["rev-parse", argument], token).ConfigureAwait(false);
        RequireResult(result);
        var path = result.StandardOutput.TrimEnd('\r', '\n');
        if (result.ExitCode != 0 || !Path.IsPathFullyQualified(path)) throw new IOException("Git could not resolve the effective-ignore repository.");
        return Path.GetFullPath(path);
    }

    private async Task<bool> CheckAsync(ToolCommandTarget git, string directory, IReadOnlyList<string> arguments, CancellationToken token)
    {
        var result = await RunAsync(git, directory, arguments, token).ConfigureAwait(false);
        RequireResult(result);
        return result.ExitCode == 0;
    }

    private Task<ToolProcessResult> RunAsync(ToolCommandTarget git, string directory, IReadOnlyList<string> arguments, CancellationToken token)
        => processes.RunAsync(new()
        {
            ExecutablePath = git.ExecutablePath,
            Arguments = git.PrefixArguments.Concat(arguments).ToArray(),
            WorkingDirectory = directory,
            Environment = ToolGitEnvironment.Probe,
            Timeout = TimeSpan.FromSeconds(10),
            MaximumOutputBytes = 16 * 1024,
        }, token);

    private static void RequireResult(ToolProcessResult result)
    {
        if (result is not { TimedOut: false, Cancelled: false, Failure: null } || result.ExitCode is not (0 or 1))
            throw new IOException("Git could not determine effective ignore coverage.");
    }
}
