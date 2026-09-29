using System.IO.Enumeration;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;

internal static class ApplyToPatternMatcher
{
    internal static ApplyToPatternParseResult Parse(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (pattern.Length == 0)
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.Empty);
        }

        if (IsDrivePrefixed(pattern) || pattern[0] == '/')
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.AbsolutePath);
        }

        var segments = pattern.Split('/');
        if (segments.Any(string.IsNullOrEmpty))
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.Empty);
        }

        if (segments.Any(segment => segment is "." or ".."))
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.Traversal);
        }

        if (pattern.Contains('\\')
            || pattern.Any(char.IsControl)
            || pattern[0] == '!'
            || segments.Any(segment => segment.Contains('{')
                || segment.Contains('}')
                || segment.Contains('[')
                || segment.Contains(']')
                || segment.Contains("**", StringComparison.Ordinal) && segment != "**"))
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
        }

        return ApplyToPatternParseResult.Succeeded(new ApplyToPattern(pattern, [.. segments]));
    }

    internal static bool IsMatch(ApplyToPattern pattern, string workspaceRelativePath)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(workspaceRelativePath);

        if (!TrySplitWorkspaceRelativePath(workspaceRelativePath, out var pathSegments))
        {
            return false;
        }

        var patternSegments = pattern.Segments;
        if (patternSegments.IsDefaultOrEmpty)
        {
            return false;
        }

        var matches = new bool[patternSegments.Length + 1, pathSegments.Length + 1];
        matches[patternSegments.Length, pathSegments.Length] = true;

        for (var patternIndex = patternSegments.Length - 1; patternIndex >= 0; patternIndex--)
        {
            var patternSegment = patternSegments[patternIndex];
            for (var pathIndex = pathSegments.Length; pathIndex >= 0; pathIndex--)
            {
                if (patternSegment == "**")
                {
                    matches[patternIndex, pathIndex] = matches[patternIndex + 1, pathIndex]
                        || pathIndex < pathSegments.Length && matches[patternIndex, pathIndex + 1];
                    continue;
                }

                if (pathIndex < pathSegments.Length
                    && FileSystemName.MatchesSimpleExpression(
                        patternSegment,
                        pathSegments[pathIndex],
                        ignoreCase: false))
                {
                    matches[patternIndex, pathIndex] = matches[patternIndex + 1, pathIndex + 1];
                }
            }
        }

        return matches[0, 0];
    }

    private static bool IsDrivePrefixed(string path)
        => path.Length >= 2 && char.IsAsciiLetter(path[0]) && path[1] == ':';

    private static bool TrySplitWorkspaceRelativePath(string path, out string[] segments)
    {
        segments = [];
        if (path.Length == 0 || path[0] == '/' || IsDrivePrefixed(path)
            || path.Contains('\\') || path.Any(char.IsControl))
        {
            return false;
        }

        var candidateSegments = path.Split('/');
        if (candidateSegments.Any(segment => string.IsNullOrEmpty(segment) || segment is "." or ".."))
        {
            return false;
        }

        segments = candidateSegments;
        return true;
    }
}
