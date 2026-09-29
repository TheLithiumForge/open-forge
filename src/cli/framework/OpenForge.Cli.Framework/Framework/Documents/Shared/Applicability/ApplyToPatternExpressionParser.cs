using System.Collections.Immutable;
using System.Text;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;

internal static class ApplyToPatternExpressionParser
{
    internal static ApplyToPatternExpressionParseResult Parse(string expression)
    {
        ArgumentNullException.ThrowIfNull(expression);

        var fragments = new List<string>();
        var fragment = new StringBuilder();
        var inGroup = false;
        var classEnd = -1;
        for (var index = 0; index < expression.Length; index++)
        {
            var character = expression[index];
            if (character == '\\' && index + 1 < expression.Length && expression[index + 1] is ',' or '\\')
            {
                fragment.Append(expression[++index]);
                continue;
            }

            if (index <= classEnd)
            {
                fragment.Append(character);
                continue;
            }

            switch (character)
            {
                case '[':
                    classEnd = ApplyToPatternMatcher.FindClassEnd(expression, index);
                    if (classEnd < 0)
                    {
                        return ApplyToPatternExpressionParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
                    }

                    break;
                case '{':
                    if (inGroup)
                    {
                        return ApplyToPatternExpressionParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
                    }

                    inGroup = true;
                    break;
                case '}':
                    if (!inGroup)
                    {
                        return ApplyToPatternExpressionParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
                    }

                    inGroup = false;
                    break;
                case ',' when !inGroup:
                    fragments.Add(fragment.ToString());
                    fragment.Clear();
                    continue;
            }

            fragment.Append(character);
        }

        if (inGroup)
        {
            return ApplyToPatternExpressionParseResult.Failed(ApplyToPatternFailure.UnsupportedSyntax);
        }

        fragments.Add(fragment.ToString());
        var patterns = ImmutableArray.CreateBuilder<ApplyToPattern>();
        foreach (var text in fragments)
        {
            var trimmed = text.Trim();
            if (trimmed.Length == 0)
            {
                continue;
            }

            var result = ApplyToPatternMatcher.Parse(trimmed);
            if (result.Failure is { } failure)
            {
                return ApplyToPatternExpressionParseResult.Failed(failure);
            }

            if (result.Pattern is { } pattern)
            {
                patterns.Add(pattern);
            }
        }

        return patterns.Count == 0
            ? ApplyToPatternExpressionParseResult.Failed(ApplyToPatternFailure.Empty)
            : ApplyToPatternExpressionParseResult.Succeeded(patterns.ToImmutable());
    }
}
