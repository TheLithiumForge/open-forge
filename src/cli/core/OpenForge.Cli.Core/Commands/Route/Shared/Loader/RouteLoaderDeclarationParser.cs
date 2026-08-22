namespace OpenForge.Cli.Core.Commands.Route.Shared.Loader;

internal static class RouteLoaderDeclarationParser
{
    internal static bool TryParse(
        string line,
        out string? destination,
        out string? cause)
    {
        destination = null;
        cause = null;
        if (!line.StartsWith("- [", StringComparison.Ordinal))
        {
            cause = "A Loader declaration must start with a single hyphen list marker and label.";
            return false;
        }

        var destinationStart = line.IndexOf("](", 3, StringComparison.Ordinal);
        if (destinationStart < 0
            || destinationStart == 3
            || !IsCanonicalLinkLabel(line.AsSpan(3, destinationStart - 3)))
        {
            cause = "A Loader declaration must contain a non-empty inline link label.";
            return false;
        }

        var destinationEnd = line.IndexOf(')', destinationStart + 2);
        if (destinationEnd < 0 || destinationEnd == destinationStart + 2)
        {
            cause = "A Loader declaration must contain a non-empty destination.";
            return false;
        }

        const string separator = " - ";
        var separatorStart = destinationEnd + 1;
        if (!line.AsSpan(separatorStart).StartsWith(separator, StringComparison.Ordinal))
        {
            cause = "A Loader declaration must contain the canonical tag separator.";
            return false;
        }

        var tags = line[(separatorStart + separator.Length)..];
        if (!HasCanonicalTags(tags))
        {
            cause = "A Loader declaration must contain one or more bare tags.";
            return false;
        }

        destination = line[(destinationStart + 2)..destinationEnd];
        return true;
    }

    private static bool HasCanonicalTags(string tags)
    {
        if (tags.Length == 0 || tags.Contains('\t'))
        {
            return false;
        }

        var parts = tags.Split(' ', StringSplitOptions.None);
        return parts.All(IsCanonicalTag);
    }

    private static bool IsCanonicalTag(string tag)
    {
        if (tag.Length < 2 || tag[0] != '#' || !char.IsLetter(tag[1]) || tag[^1] == '-')
        {
            return false;
        }

        foreach (var character in tag.AsSpan(2))
        {
            if (!char.IsLetterOrDigit(character) && character != '-')
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsCanonicalLinkLabel(ReadOnlySpan<char> value)
    {
        var hasNonWhitespace = false;
        foreach (var character in value)
        {
            if (char.IsControl(character) || character is '[' or ']')
            {
                return false;
            }

            hasNonWhitespace |= !char.IsWhiteSpace(character);
        }

        return hasNonWhitespace;
    }
}
