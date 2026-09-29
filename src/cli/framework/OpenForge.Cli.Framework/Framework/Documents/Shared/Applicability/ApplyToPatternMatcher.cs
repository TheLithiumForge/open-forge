using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;

internal static class ApplyToPatternMatcher
{
    private const int MaximumAlternatives = 1000;

    internal static ApplyToPatternParseResult Parse(string pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);

        if (!TryExpandBraces(pattern, out var expanded))
        {
            return ApplyToPatternParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
        }

        var alternatives = ImmutableArray.CreateBuilder<ImmutableArray<string>>();
        foreach (var alternative in expanded)
        {
            var failure = ValidatePath(alternative);
            if (failure is not null)
            {
                return ApplyToPatternParseResult.Failed(failure.Value);
            }

            alternatives.Add([.. alternative.Split('/')]);
        }

        return ApplyToPatternParseResult.Succeeded(new ApplyToPattern(pattern, [.. pattern.Split('/')])
        {
            Alternatives = alternatives.ToImmutable()
        });
    }

    private static ApplyToPatternFailure? ValidatePath(string pattern)
    {
        if (pattern.Length == 0)
        {
            return ApplyToPatternFailure.Empty;
        }

        if (IsDrivePrefixed(pattern) || pattern[0] == '/')
        {
            return ApplyToPatternFailure.AbsolutePath;
        }

        var segments = pattern.Split('/');
        if (segments.Any(string.IsNullOrEmpty))
        {
            return ApplyToPatternFailure.Empty;
        }

        if (segments.Any(segment => segment is "." or ".."))
        {
            return ApplyToPatternFailure.Traversal;
        }

        if (pattern.Contains('\\')
            || pattern.Any(char.IsControl))
        {
            return ApplyToPatternFailure.UnsupportedSyntax;
        }

        foreach (var segment in segments)
        {
            for (var index = 0; index < segment.Length; index++)
            {
                if (segment[index] == '[')
                {
                    index = FindClassEnd(segment, index);
                    if (index < 0)
                    {
                        return ApplyToPatternFailure.UnsupportedSyntax;
                    }
                }
            }
        }

        return null;
    }

    internal static bool IsMatch(ApplyToPattern pattern, string workspaceRelativePath)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        ArgumentNullException.ThrowIfNull(workspaceRelativePath);

        if (!TrySplitWorkspaceRelativePath(workspaceRelativePath, out var pathSegments))
        {
            return false;
        }

        if (pattern.Alternatives.IsDefaultOrEmpty)
        {
            return MatchesSegments(pattern.Segments, pathSegments);
        }

        foreach (var alternative in pattern.Alternatives)
        {
            if (MatchesSegments(alternative, pathSegments))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesSegments(ImmutableArray<string> patternSegments, string[] pathSegments)
    {
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
                    && MatchesSegment(patternSegment, pathSegments[pathIndex]))
                {
                    matches[patternIndex, pathIndex] = matches[patternIndex + 1, pathIndex + 1];
                }
            }
        }

        return matches[0, 0];
    }

    private static bool MatchesSegment(string pattern, string value)
    {
        var previous = new bool[value.Length + 1];
        previous[0] = true;
        for (var index = 0; index < pattern.Length; index++)
        {
            var token = pattern[index];
            var classEnd = token == '[' ? FindClassEnd(pattern, index) : index;
            if (classEnd < 0)
            {
                return false;
            }

            var next = new bool[value.Length + 1];
            next[0] = token == '*' && previous[0];
            for (var offset = 1; offset <= value.Length; offset++)
            {
                if (token == '*')
                {
                    next[offset] = previous[offset] || next[offset - 1];
                }
                else if (token == '[')
                {
                    next[offset] = previous[offset - 1]
                        && MatchesClass(pattern.AsSpan(index + 1, classEnd - index - 1), value[offset - 1]);
                }
                else
                {
                    next[offset] = previous[offset - 1] && (token == '?' || token == value[offset - 1]);
                }
            }

            previous = next;
            index = classEnd;
        }

        return previous[value.Length];
    }

    private static bool MatchesClass(ReadOnlySpan<char> members, char value)
    {
        if (value == '/')
        {
            return false;
        }

        var negated = members[0] is '!' or '^';
        var start = negated ? 1 : 0;
        var matched = false;
        for (var index = start; index < members.Length; index++)
        {
            if (index + 2 < members.Length && members[index + 1] == '-'
                && !(index == start && members[index] == '-'))
            {
                matched |= value >= members[index] && value <= members[index + 2];
                index += 2;
            }
            else
            {
                matched |= value == members[index];
            }
        }

        return negated != matched;
    }

    internal static int FindClassEnd(string pattern, int start)
    {
        var index = start + 1;
        if (index < pattern.Length && pattern[index] is '!' or '^')
        {
            index++;
        }

        if (index < pattern.Length && pattern[index] == ']')
        {
            index++;
        }

        return pattern.IndexOf(']', index);
    }

    private static bool TryExpandBraces(string pattern, out List<string> expanded)
    {
        expanded = [string.Empty];
        var choices = new List<string>();
        var groupStart = -1;
        var literalStart = 0;
        for (var index = 0; index < pattern.Length; index++)
        {
            switch (pattern[index])
            {
                case '[':
                    index = FindClassEnd(pattern, index);
                    if (index < 0)
                    {
                        return false;
                    }

                    break;
                case '{':
                    if (groupStart >= 0)
                    {
                        return false;
                    }

                    AppendLiteral(expanded, pattern[literalStart..index]);
                    groupStart = index + 1;
                    choices.Clear();
                    break;
                case ',' when groupStart >= 0:
                    choices.Add(pattern[groupStart..index]);
                    groupStart = index + 1;
                    break;
                case '}':
                    if (groupStart < 0)
                    {
                        return false;
                    }

                    choices.Add(pattern[groupStart..index]);
                    if (expanded.Count > MaximumAlternatives / choices.Count)
                    {
                        return false;
                    }

                    var product = new List<string>(expanded.Count * choices.Count);
                    foreach (var prefix in expanded)
                    {
                        foreach (var choice in choices)
                        {
                            product.Add($"{prefix}{choice}");
                        }
                    }

                    expanded = product;
                    groupStart = -1;
                    literalStart = index + 1;
                    break;
            }
        }

        if (groupStart >= 0)
        {
            return false;
        }

        AppendLiteral(expanded, pattern[literalStart..]);
        return true;
    }

    private static void AppendLiteral(List<string> alternatives, string literal)
    {
        for (var index = 0; index < alternatives.Count; index++)
        {
            alternatives[index] = $"{alternatives[index]}{literal}";
        }
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
