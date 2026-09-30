using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Shared.MatchingFiles;

internal static class RouteInspectIgnoreRules
{
    internal static RouteInspectIgnoreParseResult Parse(string baseDirectory, IEnumerable<string> lines, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var rules = ImmutableArray.CreateBuilder<RouteInspectIgnoreRule>();
        var skipped = 0;
        foreach (var original in lines)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var line = TrimTrailingSpaces(original, cancellationToken);
            if (line.Length == 0 || line[0] == '#')
            {
                continue;
            }

            var include = line[0] == '!';
            if (include)
            {
                line = line[1..];
            }

            var translated = Translate(line, cancellationToken);
            var directoryOnly = line.EndsWith('/');
            if (translated is null)
            {
                skipped++;
                continue;
            }

            if (directoryOnly)
            {
                translated = translated[..^1];
            }

            var anchored = translated.Contains('/');
            if (translated.StartsWith('/'))
            {
                translated = translated[1..];
            }

            if (translated.Length == 0)
            {
                skipped++;
                continue;
            }

            if (!anchored)
            {
                translated = "**/" + translated;
            }

            if (translated.EndsWith("/**", StringComparison.Ordinal))
            {
                translated += "/*";
            }

            var parsed = ApplyToPatternMatcher.Parse(translated);
            cancellationToken.ThrowIfCancellationRequested();
            if (parsed.Pattern is not { } pattern)
            {
                skipped++;
                continue;
            }

            rules.Add(new(baseDirectory, pattern, include, directoryOnly));
        }

        var result = new RouteInspectIgnoreParseResult(rules.ToImmutable(), skipped);
        cancellationToken.ThrowIfCancellationRequested();
        return result;
    }

    internal static bool IsIncluded(IEnumerable<RouteInspectIgnoreRule> rules, string path, bool isDirectory, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var included = true;
        foreach (var rule in rules)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (rule.DirectoryOnly && !isDirectory)
            {
                continue;
            }

            var relative = path;
            if (rule.BaseDirectory.Length > 0)
            {
                var prefix = rule.BaseDirectory + "/";
                if (!path.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }

                relative = path[prefix.Length..];
            }

            if (ApplyToPatternMatcher.IsMatch(rule.Pattern, relative))
            {
                included = rule.Include;
            }
        }

        cancellationToken.ThrowIfCancellationRequested();
        return included;
    }

    private static string TrimTrailingSpaces(string line, CancellationToken cancellationToken)
    {
        var end = line.Length;
        while (end > 0 && line[end - 1] == ' ')
        {
            cancellationToken.ThrowIfCancellationRequested();
            var escapes = 0;
            for (var index = end - 2; index >= 0 && line[index] == '\\'; index--)
            {
                cancellationToken.ThrowIfCancellationRequested();
                escapes++;
            }

            if (escapes % 2 != 0)
            {
                break;
            }

            end--;
        }

        return line[..end];
    }

    private static string? Translate(string line, CancellationToken cancellationToken)
    {
        var result = new StringBuilder();
        for (var index = 0; index < line.Length; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var character = line[index];
            if (character == '\\')
            {
                if (++index == line.Length || line[index] is '/' or '\\')
                {
                    return null;
                }

                var literal = line[index];
                if (literal is '*' or '?' or '[' or ']' or '{' or '}')
                {
                    result.Append('[').Append(literal).Append(']');
                }
                else
                {
                    result.Append(literal);
                }

                continue;
            }

            if (character == '[')
            {
                var end = ApplyToPatternMatcher.FindClassEnd(line, index);
                if (end < 0)
                {
                    return null;
                }

                var members = line.AsSpan(index, end - index + 1);
                if (members.Contains('\\') || members.Contains("[:", StringComparison.Ordinal)
                    || members.Contains("[.", StringComparison.Ordinal) || members.Contains("[=", StringComparison.Ordinal))
                {
                    return null;
                }

                result.Append(members);
                index = end;
            }
            else if (character is '{' or '}')
            {
                result.Append('[').Append(character).Append(']');
            }
            else
            {
                result.Append(character);
            }
        }

        return result.ToString();
    }
}
