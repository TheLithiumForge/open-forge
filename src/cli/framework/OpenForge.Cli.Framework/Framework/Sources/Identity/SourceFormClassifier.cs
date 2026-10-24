using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceFormClassifier
{
    internal const string MarkdownExtension = WorkspaceRoutePath.MarkdownExtension;
    internal const string NativeSkillFileName = "SKILL.md";
    internal const string IndexFileName = "index" + MarkdownExtension;
    internal const string UnderscoreIndexFileName = "_" + IndexFileName;
    internal const string ReferencesFileName = "references" + MarkdownExtension;
    internal const string UnderscoreReferencesFileName = "_" + ReferencesFileName;

    internal static bool TryClassify(WorkspaceLayout layout, string? canonicalPath, out SourceDocumentForm form)
    {
        if (SourceLogicalPath.IsOutsideSource(layout, canonicalPath))
            return TryClassifyDocument(canonicalPath, nativeSkill: false, mainRoot: null, out form);
        return TryClassify(new SourceContentRoots(layout), canonicalPath, out form);
    }

    internal static bool TryClassify(SourceContentRoots roots, string? canonicalPath, out SourceDocumentForm form)
    {
        form = default;
        if (!SourceLogicalPath.IsPrimarySource(roots, canonicalPath))
        {
            return false;
        }

        if (canonicalPath.Length == roots.MainRoot.Length + 1 + WorkspaceLayoutNames.LoaderFileName.Length
            && canonicalPath.StartsWith(roots.MainRoot, StringComparison.Ordinal)
            && canonicalPath.AsSpan(roots.MainRoot.Length + 1).SequenceEqual(WorkspaceLayoutNames.LoaderFileName))
        {
            form = SourceDocumentForm.Loader;
            return true;
        }

        return TryClassifyDocument(canonicalPath,
            nativeSkill: roots.IsDefault || canonicalPath.StartsWith(WorkspaceLayout.NativeSkillsPath + "/", StringComparison.Ordinal),
            mainRoot: roots.MainRoot, out form);
    }

    private static bool TryClassifyDocument(string canonicalPath, bool nativeSkill, string? mainRoot, out SourceDocumentForm form)
    {
        form = default;
        var fileName = SourceLogicalPath.ReadFileName(canonicalPath);
        if (SourceOverwritePath.HasSuffix(fileName))
        {
            form = SourceDocumentForm.OverwriteCompanion;
            return true;
        }

        if (!fileName.EndsWith(MarkdownExtension, StringComparison.Ordinal))
        {
            return false;
        }

        if (fileName == NativeSkillFileName && nativeSkill)
        {
            form = SourceDocumentForm.Skill;
            return true;
        }

        if (!fileName.StartsWith('_') && fileName is not (IndexFileName or ReferencesFileName))
        {
            form = SourceDocumentForm.Markdown;
            return true;
        }

        var parent = SourceLogicalPath.ReadParent(canonicalPath);
        form = parent == mainRoot ? SourceDocumentForm.Markdown : ReadEntrypointForm(fileName, parent) ?? SourceDocumentForm.Markdown;
        return true;
    }

    internal static bool Matches(WorkspaceLayout layout, string canonicalPath, SourceDocumentForm form)
        => TryClassify(layout, canonicalPath, out var classified) && classified == form;

    internal static bool Matches(SourceContentRoots roots, string canonicalPath, SourceDocumentForm form)
        => TryClassify(roots, canonicalPath, out var classified) && classified == form;

    internal static bool IsEntryDocument(WorkspaceLayout layout, [NotNullWhen(true)] string? canonicalPath)
        => IsEntryDocument(new SourceContentRoots(layout), canonicalPath);

    internal static bool IsEntryDocument(SourceContentRoots roots, [NotNullWhen(true)] string? canonicalPath)
    {
        return string.Equals(canonicalPath, SourceLogicalPath.WorkspaceEntryPath, StringComparison.Ordinal)
            || string.Equals(canonicalPath, roots.LoaderPath, StringComparison.Ordinal);
    }

    internal static bool IsEntrypoint(SourceDocumentForm form)
    {
        return form is SourceDocumentForm.CanonicalEntrypoint
            or SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }

    internal static bool IsCompatibilityEntrypoint(SourceDocumentForm form)
    {
        return form is SourceDocumentForm.IndexEntrypoint
            or SourceDocumentForm.UnderscoreIndexEntrypoint
            or SourceDocumentForm.ReferencesEntrypoint
            or SourceDocumentForm.UnderscoreReferencesEntrypoint;
    }

    internal static bool IsCompatibilityFileName(string fileName)
    {
        return fileName is IndexFileName or UnderscoreIndexFileName or ReferencesFileName or UnderscoreReferencesFileName;
    }

    internal static string CanonicalEntrypointFileName(string directoryName)
        => WorkspaceRoutePath.CanonicalEntrypointFileName(directoryName);

    internal static bool IsEntrypointPath(string path)
    {
        return SourceLogicalPath.IsCanonicalSource(path)
            && SourceLogicalPath.ReadParent(path) is { } parent && parent != "."
            && ReadEntrypointForm(SourceLogicalPath.ReadFileName(path), parent) is not null;
    }

    private static SourceDocumentForm? ReadEntrypointForm(string fileName, string parent)
    {
        var compatibility = fileName switch
        {
            IndexFileName => SourceDocumentForm.IndexEntrypoint,
            UnderscoreIndexFileName => SourceDocumentForm.UnderscoreIndexEntrypoint,
            ReferencesFileName => SourceDocumentForm.ReferencesEntrypoint,
            UnderscoreReferencesFileName => SourceDocumentForm.UnderscoreReferencesEntrypoint,
            _ => (SourceDocumentForm?)null,
        };
        if (compatibility is not null)
        {
            return compatibility;
        }

        var parentName = SourceLogicalPath.ReadFileName(parent);
        return fileName == CanonicalEntrypointFileName(parentName)
            ? SourceDocumentForm.CanonicalEntrypoint
            : null;
    }
}
