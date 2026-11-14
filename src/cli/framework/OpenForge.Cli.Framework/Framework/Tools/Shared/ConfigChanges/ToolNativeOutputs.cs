using System.IO.Enumeration;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Tools.Models.Recipes;
using OpenForge.Cli.Core.Framework.Tools.Shared.Filesystem;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.ConfigChanges;

internal static class ToolNativeOutputs
{
    internal static string PathFor(ToolRecipeContext context, string template, ToolEffectScope scope)
        => Resolve(context, ToolRecipeExpansion.Expand(template, context), scope);

    internal static string? AvailablePathFor(ToolRecipeContext context, string template, ToolEffectScope scope)
    {
        var expanded = ToolRecipeExpansion.ExpandAvailable(template, context);
        return expanded is null ? null : Resolve(context, expanded, scope);
    }

    private static string Resolve(ToolRecipeContext context, string expanded, ToolEffectScope scope)
    {
        var path = Path.GetFullPath(expanded, context.WorkspacePath);
        if (scope == ToolEffectScope.Workspace && !PhysicalContainment.Contains(context.WorkspacePath, path))
            throw new InvalidDataException("A native workspace output escapes the workspace.");
        if (scope is not (ToolEffectScope.Workspace or ToolEffectScope.Account)) throw new ArgumentOutOfRangeException(nameof(scope));
        _ = ToolNativePaths.Scope(context, Path.Combine(path.Replace('*', 'x').Replace('?', 'x'), ".open-forge-output-probe"));
        return path;
    }

    internal static IReadOnlyList<string> Expand(ToolRecipeContext context, string pattern, ToolNativeOutputMatch match, CancellationToken token)
    {
        ValidatePattern(pattern, match);
        if (match == ToolNativeOutputMatch.Exact)
        {
            return [pattern];
        }
        var scope = ToolNativePaths.Scope(context, pattern.Replace('*', 'x').Replace('?', 'x'));
        var segments = Path.GetRelativePath(scope.LexicalRoot, pattern).Split([Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar], StringSplitOptions.RemoveEmptyEntries);
        var parents = new List<string> { scope.LexicalRoot };
        for (var index = 0; index < segments.Length; index++)
        {
            var final = index == segments.Length - 1;
            var next = new List<string>();
            foreach (var parent in parents)
            {
                token.ThrowIfCancellationRequested();
                var wildcard = segments[index].IndexOfAny(['*', '?']) >= 0;
                var candidates = wildcard ? Directory.EnumerateFileSystemEntries(parent).Where(path => FileSystemName.MatchesSimpleExpression(segments[index], Path.GetFileName(path), OperatingSystem.IsWindows()))
                    : new[] { Path.Combine(parent, segments[index]) };
                foreach (var candidate in candidates)
                {
                    var state = ToolFileReader.Observe(scope, candidate, token);
                    if (state is not (NoFollowLeafState.Missing or NoFollowLeafState.Directory or NoFollowLeafState.OrdinaryFile))
                        throw new IOException("A native output pattern encountered a linked or unreadable path.");
                    if (final && state is NoFollowLeafState.Missing or NoFollowLeafState.OrdinaryFile) next.Add(candidate);
                    else if (!final && state == NoFollowLeafState.Directory) next.Add(candidate);
                }
            }
            parents = next;
        }
        return parents;
    }

    internal static void ValidatePattern(string pattern, ToolNativeOutputMatch match)
    {
        switch (match)
        {
            case ToolNativeOutputMatch.Exact:
                if (pattern.IndexOfAny(['*', '?']) >= 0) throw new InvalidDataException("An exact native output cannot contain wildcards.");
                break;
            case ToolNativeOutputMatch.ExistingPattern:
                if (pattern.Contains("**", StringComparison.Ordinal)) throw new InvalidDataException("Native output patterns cannot recurse.");
                break;
            default: throw new ArgumentOutOfRangeException(nameof(match));
        }
    }

    internal static bool Matches(string pattern, string path, ToolNativeOutputMatch match)
    {
        if (match == ToolNativeOutputMatch.Exact) return ToolNativePaths.Comparer.Equals(pattern, path);
        if (match != ToolNativeOutputMatch.ExistingPattern) throw new ArgumentOutOfRangeException(nameof(match));
        var parts = pattern.Replace('\u005c', '/').Split('/');
        var values = path.Replace('\u005c', '/').Split('/');
        return parts.Length == values.Length && parts.Select((part, index) => FileSystemName.MatchesSimpleExpression(part, values[index], OperatingSystem.IsWindows())).All(value => value);
    }
}
