namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceWorkspaceRelativePath
{
    internal static string Validate(
        string path,
        string parameterName,
        bool allowWorkspaceRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        if (allowWorkspaceRoot && path == ".")
        {
            return path;
        }

        if (path == "."
            || path.StartsWith("/", StringComparison.Ordinal)
            || IsDriveQualified(path)
            || path.Contains('\\')
            || path.Split('/', StringSplitOptions.None)
                .Any(segment => !SourceLogicalPath.IsCanonicalSegment(segment)))
        {
            throw new ArgumentException(
                "The path must be slash-separated and relative to the selected workspace.",
                parameterName);
        }

        return path;
    }

    internal static string ValidateMarkdown(string path, string parameterName)
    {
        var validated = Validate(path, parameterName, allowWorkspaceRoot: false);
        if (!validated.EndsWith(".md", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "The workspace-relative source path must identify a Markdown file.",
                parameterName);
        }

        return validated;
    }

    internal static string ValidateExtension(string extension, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(extension, parameterName);
        if (extension.Length < 2
            || extension[0] != '.'
            || !HasValidExtensionTail(extension))
        {
            throw new ArgumentException(
                "A supported Markdown extension must contain one leading dot and ASCII letters or digits.",
                parameterName);
        }

        return extension;
    }

    private static bool IsDriveQualified(string path)
        => path.Length >= 2
            && char.IsAsciiLetter(path[0])
            && path[1] == ':';

    private static bool HasValidExtensionTail(string extension)
    {
        for (var index = 1; index < extension.Length; index++)
        {
            if (!char.IsAsciiLetterOrDigit(extension[index]))
            {
                return false;
            }
        }

        return true;
    }
}
