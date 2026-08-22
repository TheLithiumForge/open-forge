using OpenForge.Cli.Core.Commands.Route.Shared.Models.Source;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Source;

internal static class RouteSourceFormClassifier
{
    private const string LoaderPath = ".agents/loader.md";
    private const string OverwriteSuffix = ".overwrite.md";

    internal static bool TryClassify(string? canonicalPath, out RouteSourceForm form)
    {
        form = default;
        if (!RouteLogicalPath.IsCanonical(canonicalPath))
        {
            return false;
        }

        if (string.Equals(canonicalPath, LoaderPath, StringComparison.Ordinal))
        {
            form = RouteSourceForm.Loader;
            return true;
        }

        var fileName = RouteLogicalPath.ReadFileName(canonicalPath!);
        if (fileName.EndsWith(OverwriteSuffix, StringComparison.Ordinal))
        {
            form = RouteSourceForm.OverwriteCompanion;
            return true;
        }

        if (!fileName.EndsWith(".md", StringComparison.Ordinal))
        {
            return false;
        }

        var parent = RouteLogicalPath.ReadParent(canonicalPath!);
        if (fileName == "SKILL.md")
        {
            form = RouteSourceForm.Skill;
            return true;
        }

        form = ReadEntrypointForm(fileName, parent) ?? RouteSourceForm.Markdown;
        return true;
    }

    internal static bool Matches(string canonicalPath, RouteSourceForm form)
    {
        return TryClassify(canonicalPath, out var classified) && classified == form;
    }

    internal static bool IsEntrypoint(RouteSourceForm form)
    {
        return form is RouteSourceForm.CanonicalEntrypoint
            or RouteSourceForm.IndexEntrypoint
            or RouteSourceForm.UnderscoreIndexEntrypoint
            or RouteSourceForm.ReferencesEntrypoint
            or RouteSourceForm.UnderscoreReferencesEntrypoint;
    }

    internal static bool IsCompatibilityEntrypoint(RouteSourceForm form)
    {
        return form is RouteSourceForm.IndexEntrypoint
            or RouteSourceForm.UnderscoreIndexEntrypoint
            or RouteSourceForm.ReferencesEntrypoint
            or RouteSourceForm.UnderscoreReferencesEntrypoint;
    }

    internal static bool IsCompatibilityFileName(string fileName)
    {
        return fileName is "index.md" or "_index.md" or "references.md" or "_references.md";
    }

    private static RouteSourceForm? ReadEntrypointForm(string fileName, string parent)
    {
        if (parent == RouteLogicalPath.AgentsRoot)
        {
            return null;
        }

        var compatibility = fileName switch
        {
            "index.md" => RouteSourceForm.IndexEntrypoint,
            "_index.md" => RouteSourceForm.UnderscoreIndexEntrypoint,
            "references.md" => RouteSourceForm.ReferencesEntrypoint,
            "_references.md" => RouteSourceForm.UnderscoreReferencesEntrypoint,
            _ => (RouteSourceForm?)null,
        };
        if (compatibility is not null)
        {
            return compatibility;
        }

        var parentName = RouteLogicalPath.ReadFileName(parent);
        return fileName == $"_{parentName}.md"
            ? RouteSourceForm.CanonicalEntrypoint
            : null;
    }
}
