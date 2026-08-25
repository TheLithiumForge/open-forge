using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceIdentity
{
    internal static string? DeriveId(string? canonicalPath)
    {
        if (!SourceLogicalPath.IsCanonicalSource(canonicalPath))
        {
            return null;
        }

        var sourcePath = canonicalPath[".agents/".Length..];
        var segments = sourcePath.Split('/', StringSplitOptions.None);
        var fileName = segments[^1];
        var directorySegments = segments[..^1];

        SourceFormClassifier.TryClassify(canonicalPath, out var form);
        if (form == SourceDocumentForm.OverwriteCompanion)
        {
            fileName = fileName[..^".overwrite.md".Length] + ".md";
            var basePath = string.Join('/', directorySegments.Append(fileName));
            var canonicalBasePath = $".agents/{basePath}";
            if (!SourceFormClassifier.TryClassify(canonicalBasePath, out form))
            {
                return null;
            }
        }

        if (form == SourceDocumentForm.Skill || SourceFormClassifier.IsEntrypoint(form))
        {
            return directorySegments.Length == 0
                ? null
                : string.Join('/', directorySegments);
        }

        if ((fileName == "SKILL.md" || SourceFormClassifier.IsCompatibilityFileName(fileName))
            && directorySegments.Length == 0)
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
        if (id is null || id.StartsWith(".agents/", StringComparison.Ordinal) || id.Length == 0)
        {
            return false;
        }

        return id.Split('/', StringSplitOptions.None).All(SourceLogicalPath.IsCanonicalSegment);
    }

    internal static bool IsRecognizedEntrypointPath(string canonicalPath)
    {
        return SourceFormClassifier.TryClassify(canonicalPath, out var form)
            && SourceFormClassifier.IsEntrypoint(form);
    }
}
