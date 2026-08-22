using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal static class RouteSourceIdentity
{
    private const string AgentsPrefix = ".agents/";

    internal static string? DeriveId(string? canonicalPath)
    {
        if (!RouteLogicalPath.IsCanonical(canonicalPath))
        {
            return null;
        }

        var sourcePath = canonicalPath![AgentsPrefix.Length..];
        var segments = sourcePath.Split('/', StringSplitOptions.None);
        var fileName = segments[^1];
        var directorySegments = segments[..^1];

        RouteSourceFormClassifier.TryClassify(canonicalPath, out var form);
        if (form == RouteSourceForm.OverwriteCompanion)
        {
            var baseFileName = fileName[..^".overwrite.md".Length] + ".md";
            fileName = baseFileName;
            var basePath = string.Join('/', directorySegments.Append(fileName));
            if (!RouteSourceFormClassifier.TryClassify($"{AgentsPrefix}{basePath}", out form))
            {
                return null;
            }
        }

        if (form == RouteSourceForm.Skill
            || RouteSourceFormClassifier.IsEntrypoint(form))
        {
            return directorySegments.Length == 0
                ? null
                : string.Join('/', directorySegments);
        }

        if ((fileName == "SKILL.md" || RouteSourceFormClassifier.IsCompatibilityFileName(fileName))
            && directorySegments.Length == 0
            || fileName.EndsWith(".overwrite.md", StringComparison.Ordinal))
        {
            return null;
        }

        if (fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            fileName = fileName[..^".md".Length];
        }

        return fileName.Length == 0
            ? null
            : string.Join('/', directorySegments.Append(fileName));
    }

    internal static bool IsValidId(string? id)
    {
        return RouteSourceReferenceParser.IsValidId(id);
    }

    internal static bool IsRecognizedEntrypointPath(string canonicalPath)
    {
        return RouteSourceFormClassifier.TryClassify(canonicalPath, out var form)
            && RouteSourceFormClassifier.IsEntrypoint(form);
    }
}
