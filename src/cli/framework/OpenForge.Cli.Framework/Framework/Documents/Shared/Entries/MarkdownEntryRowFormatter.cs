using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Entries;

internal static class MarkdownEntryRowFormatter
{
    internal static string FormatApplyToSuffix(IEnumerable<ApplyToPattern> patterns)
    {
        ArgumentNullException.ThrowIfNull(patterns);
        var values = patterns.ToArray();
        if (values.Length == 0)
        {
            return string.Empty;
        }

        if (values.Any(pattern => pattern is null))
        {
            throw new ArgumentException("ApplyTo patterns cannot contain null members.", nameof(patterns));
        }

        return $" - applies to {string.Join(", ", values.Select(FormatCodeSpan))}";
    }

    private static string FormatCodeSpan(ApplyToPattern pattern)
    {
        var longestRun = LongestBacktickRun(pattern.Text);
        var fence = new string('`', checked(longestRun + 1));
        var pad = pattern.Text.Any(character => character != ' ')
            && (pattern.Text.StartsWith(' ')
                || pattern.Text.EndsWith(' ')
                || pattern.Text.StartsWith('`')
                || pattern.Text.EndsWith('`'));
        return pad
            ? $"{fence} {pattern.Text} {fence}"
            : $"{fence}{pattern.Text}{fence}";
    }

    private static int LongestBacktickRun(string value)
    {
        var longest = 0;
        var current = 0;
        foreach (var character in value)
        {
            if (character == '`')
            {
                current++;
                longest = Math.Max(longest, current);
            }
            else
            {
                current = 0;
            }
        }

        return longest;
    }
}
