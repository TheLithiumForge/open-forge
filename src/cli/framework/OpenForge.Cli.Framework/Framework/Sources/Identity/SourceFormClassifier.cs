using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceFormClassifier
{
    internal const string MarkdownExtension = ".md";
    internal const string NativeSkillFileName = "SKILL.md";
    internal const string IndexFileName = "index" + MarkdownExtension;
    internal const string UnderscoreIndexFileName = "_" + IndexFileName;
    internal const string ReferencesFileName = "references" + MarkdownExtension;
    internal const string UnderscoreReferencesFileName = "_" + ReferencesFileName;

    internal static bool TryClassify(string? canonicalPath, out SourceDocumentForm form)
        => TryClassify(SourceContentRoots.Default, canonicalPath, out form);

    internal static bool TryClassify(WorkspaceLayout layout, string? canonicalPath, out SourceDocumentForm form)
        => TryClassify(new SourceContentRoots(layout), canonicalPath, out form);

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

        if (fileName == NativeSkillFileName && (roots.IsDefault || canonicalPath.StartsWith(WorkspaceLayout.NativeSkillsPath + "/", StringComparison.Ordinal)))
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
        form = ReadEntrypointForm(roots, fileName, parent) ?? SourceDocumentForm.Markdown;
        return true;
    }

    internal static bool Matches(string canonicalPath, SourceDocumentForm form)
        => Matches(SourceContentRoots.Default, canonicalPath, form);

    internal static bool Matches(WorkspaceLayout layout, string canonicalPath, SourceDocumentForm form)
        => Matches(new SourceContentRoots(layout), canonicalPath, form);

    internal static bool Matches(SourceContentRoots roots, string canonicalPath, SourceDocumentForm form)
        => TryClassify(roots, canonicalPath, out var classified) && classified == form;

    internal static bool IsEntryDocument([NotNullWhen(true)] string? canonicalPath)
        => IsEntryDocument(SourceContentRoots.Default, canonicalPath);

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

    private static SourceDocumentForm? ReadEntrypointForm(SourceContentRoots roots, string fileName, string parent)
    {
        if (parent == roots.MainRoot)
        {
            return null;
        }

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
        return fileName == $"_{parentName}{MarkdownExtension}"
            ? SourceDocumentForm.CanonicalEntrypoint
            : null;
    }
}
