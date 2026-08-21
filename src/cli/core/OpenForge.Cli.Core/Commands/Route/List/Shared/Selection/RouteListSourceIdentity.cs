namespace OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;

internal static class RouteListSourceIdentity
{
    private const string AgentsPrefix = ".agents/";

    internal static string? DeriveId(string? canonicalPath)
    {
        if (!RouteListSourceReferenceParser.IsValidCanonicalPath(canonicalPath))
        {
            return null;
        }

        var sourcePath = canonicalPath![AgentsPrefix.Length..];
        var segments = sourcePath.Split('/', StringSplitOptions.None).ToArray();
        var fileName = segments[^1];
        var directorySegments = segments[..^1];

        if (fileName.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            var baseFileName = fileName[..^".overwrite.md".Length] + ".md";
            if (baseFileName == ".md")
            {
                return null;
            }

            fileName = baseFileName;
        }

        if (fileName == "SKILL.md"
            || IsCompatibilityEntrypoint(fileName)
            || directorySegments.Length > 0
            && IsCanonicalEntrypoint(fileName, directorySegments[^1]))
        {
            return directorySegments.Length == 0
                ? null
                : string.Join('/', directorySegments);
        }

        if (fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            fileName = fileName[..^".md".Length];
        }

        if (fileName.Length == 0)
        {
            return null;
        }

        var idSegments = directorySegments.Append(fileName);
        return string.Join('/', idSegments);
    }

    internal static bool IsValidId(string? id)
    {
        return RouteListSourceReferenceParser.IsValidId(id);
    }

    internal static bool IsRecognizedEntrypointPath(string canonicalPath)
    {
        if (!RouteListSourceReferenceParser.IsValidCanonicalPath(canonicalPath)
            || canonicalPath.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return false;
        }

        var segments = canonicalPath[AgentsPrefix.Length..].Split('/', StringSplitOptions.None);
        if (segments.Length < 2)
        {
            return false;
        }

        var fileName = segments[^1];
        return IsCompatibilityEntrypoint(fileName)
            || IsCanonicalEntrypoint(fileName, segments[^2]);
    }

    private static bool IsCompatibilityEntrypoint(string fileName)
    {
        return fileName is "index.md" or "_index.md" or "references.md" or "_references.md";
    }

    private static bool IsCanonicalEntrypoint(string fileName, string folderName)
    {
        return fileName == $"_{folderName}.md";
    }
}
