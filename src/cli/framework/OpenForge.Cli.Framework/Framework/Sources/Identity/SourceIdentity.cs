using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceIdentity
{
    internal static string? DeriveId(string? canonicalPath) => DeriveId(SourceContentRoots.Default, canonicalPath);

    internal static string? DeriveId(WorkspaceLayout layout, string? canonicalPath)
        => DeriveId(new SourceContentRoots(layout), canonicalPath);

    internal static string? DeriveId(SourceContentRoots roots, string? canonicalPath)
    {
        if (!SourceLogicalPath.IsPrimarySource(roots, canonicalPath))
        {
            return null;
        }

        var sourcePath = SourceLogicalPath.ReadSourceCoordinate(roots, canonicalPath);
        var segments = sourcePath.Split('/', StringSplitOptions.None);
        var fileName = segments[^1];
        var directorySegments = segments[..^1];

        SourceFormClassifier.TryClassify(roots, canonicalPath, out var form);
        if (form == SourceDocumentForm.OverwriteCompanion)
        {
            fileName = SourceOverwritePath.ReadBasePath(fileName);
            var canonicalBasePath = SourceOverwritePath.ReadBasePath(canonicalPath);
            if (!SourceFormClassifier.TryClassify(roots, canonicalBasePath, out form))
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

        if ((fileName == SourceFormClassifier.NativeSkillFileName || SourceFormClassifier.IsCompatibilityFileName(fileName))
            && directorySegments.Length == 0)
        {
            return null;
        }

        if (fileName.EndsWith(SourceFormClassifier.MarkdownExtension, StringComparison.Ordinal))
        {
            fileName = fileName[..^SourceFormClassifier.MarkdownExtension.Length];
        }

        return fileName.Length == 0
            ? null
            : string.Join('/', directorySegments.Append(fileName));
    }

    internal static bool IsValidId(string? id)
    {
        if (id is null || id.StartsWith(SourceLogicalPath.AgentsRoot + "/", StringComparison.Ordinal) || id.Length == 0)
        {
            return false;
        }

        return id.Split('/', StringSplitOptions.None).All(SourceLogicalPath.IsCanonicalSegment);
    }

    internal static bool IsRecognizedEntrypointPath(string canonicalPath)
        => IsRecognizedEntrypointPath(SourceContentRoots.Default, canonicalPath);

    internal static bool IsRecognizedEntrypointPath(WorkspaceLayout layout, string canonicalPath)
        => IsRecognizedEntrypointPath(new SourceContentRoots(layout), canonicalPath);

    internal static bool IsRecognizedEntrypointPath(SourceContentRoots roots, string canonicalPath)
    {
        return SourceFormClassifier.TryClassify(roots, canonicalPath, out var form)
            && SourceFormClassifier.IsEntrypoint(form);
    }
}
