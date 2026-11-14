using System.Text.RegularExpressions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;

// The CLI spec reader intentionally accepts stable tool versions only. This guard
// compares the running Open Forge version, whose release can be a prerelease.
internal static partial class ToolWriteMinimumVersion
{
    internal static bool IsSatisfied(string current, string minimum)
    {
        var actual = VersionPattern().Match(current);
        var required = VersionPattern().Match(minimum);
        if (!Valid(actual) || !Valid(required)) return false;
        for (var index = 1; index <= 3; index++)
        {
            var compared = Numeric(actual.Groups[index].Value, required.Groups[index].Value);
            if (compared != 0) return compared > 0;
        }
        var actualPre = actual.Groups[4].Value;
        var requiredPre = required.Groups[4].Value;
        if (actualPre.Length == 0) return true;
        if (requiredPre.Length == 0) return false;
        var left = actualPre.Split('.');
        var right = requiredPre.Split('.');
        for (var index = 0; index < Math.Min(left.Length, right.Length); index++)
        {
            var leftNumeric = Digits(left[index]);
            var rightNumeric = Digits(right[index]);
            if (leftNumeric != rightNumeric) return !leftNumeric;
            var compared = leftNumeric ? Numeric(left[index], right[index]) : string.CompareOrdinal(left[index], right[index]);
            if (compared != 0) return compared > 0;
        }
        return left.Length >= right.Length;
    }

    private static bool Valid(Match match)
    {
        if (!match.Success) return false;
        for (var index = 1; index <= 3; index++)
            if (LeadingZero(match.Groups[index].Value)) return false;
        return match.Groups[4].Value.Split('.').All(part => !Digits(part) || !LeadingZero(part));
    }

    private static bool Digits(string value) => value.Length != 0 && value.All(character => character is >= '0' and <= '9');
    private static bool LeadingZero(string value) => value.Length > 1 && value[0] == '0';
    private static int Numeric(string left, string right)
    {
        var length = left.Length.CompareTo(right.Length);
        return length == 0 ? string.CompareOrdinal(left, right) : length;
    }

    [GeneratedRegex(@"\A([0-9]+)\.([0-9]+)\.([0-9]+)(?:-([0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*))?(?:\+[0-9A-Za-z-]+(?:\.[0-9A-Za-z-]+)*)?\z", RegexOptions.CultureInvariant)]
    private static partial Regex VersionPattern();
}
