using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Sources.Identity;

internal static class SourceFormClassifier
{
    private const string OverwriteSuffix = ".overwrite.md";

    internal static bool TryClassify(string? canonicalPath, out SourceDocumentForm form)
    {
        form = default;
        if (!SourceLogicalPath.IsCanonicalSource(canonicalPath))
        {
            return false;
        }

        if (string.Equals(canonicalPath, SourceLogicalPath.LoaderPath, StringComparison.Ordinal))
        {
            form = SourceDocumentForm.Loader;
            return true;
        }

        var fileName = SourceLogicalPath.ReadFileName(canonicalPath);
        if (fileName.EndsWith(OverwriteSuffix, StringComparison.Ordinal))
        {
            form = SourceDocumentForm.OverwriteCompanion;
            return true;
        }

        if (!fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            return false;
        }

        if (fileName == "SKILL.md")
        {
            form = SourceDocumentForm.Skill;
            return true;
        }

        var parent = SourceLogicalPath.ReadParent(canonicalPath);
        form = ReadEntrypointForm(fileName, parent) ?? SourceDocumentForm.Markdown;
        return true;
    }

    internal static bool Matches(string canonicalPath, SourceDocumentForm form)
    {
        return TryClassify(canonicalPath, out var classified) && classified == form;
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
        return fileName is "index.md" or "_index.md" or "references.md" or "_references.md";
    }

    private static SourceDocumentForm? ReadEntrypointForm(string fileName, string parent)
    {
        if (parent == SourceLogicalPath.AgentsRoot)
        {
            return null;
        }

        var compatibility = fileName switch
        {
            "index.md" => SourceDocumentForm.IndexEntrypoint,
            "_index.md" => SourceDocumentForm.UnderscoreIndexEntrypoint,
            "references.md" => SourceDocumentForm.ReferencesEntrypoint,
            "_references.md" => SourceDocumentForm.UnderscoreReferencesEntrypoint,
            _ => (SourceDocumentForm?)null,
        };
        if (compatibility is not null)
        {
            return compatibility;
        }

        var parentName = SourceLogicalPath.ReadFileName(parent);
        return fileName == $"_{parentName}.md"
            ? SourceDocumentForm.CanonicalEntrypoint
            : null;
    }
}
