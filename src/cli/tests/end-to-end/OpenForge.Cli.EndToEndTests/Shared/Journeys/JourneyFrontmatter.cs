using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Shared.Journeys;

// Independent test oracle for the Frontmatter Form section of
// .agents/memory/crystallized/documents/cli/shared-operation-contract.md.
// It handles only canonical repository payload Markdown: LF line endings, a
// leading frontmatter block, and an `open-forge:` block mapping indented by two
// spaces. It deliberately restates the rule so a product change fails loudly.
internal static class JourneyFrontmatter
{
    private const string Delimiter = "---";
    private const string ScopeLine = "open-forge:";
    private const string EmptyScopeLine = "open-forge: {}";
    private const string ScopeIndent = "  ";

    internal static string Flag(JourneyFrontmatterForm form) => form switch
    {
        JourneyFrontmatterForm.Root => "root",
        JourneyFrontmatterForm.Scoped => "scoped",
        _ => throw new ArgumentOutOfRangeException(nameof(form), form, "The journey frontmatter form is not defined."),
    };

    internal static string[] InstallArguments(JourneyFrontmatterForm form, params string[] arguments)
    {
        ArgumentNullException.ThrowIfNull(arguments);
        return ["install", "--frontmatter", Flag(form), .. arguments];
    }

    internal static string RenderCanonical(string canonical, JourneyFrontmatterForm form)
    {
        ArgumentNullException.ThrowIfNull(canonical);
        if (form == JourneyFrontmatterForm.Scoped)
        {
            return canonical;
        }

        if (form != JourneyFrontmatterForm.Root)
        {
            throw new ArgumentOutOfRangeException(nameof(form), form, "The journey frontmatter form is not defined.");
        }

        var lines = canonical.Split('\n');
        if (lines.Length < 3 || lines[0] != Delimiter)
        {
            return canonical;
        }

        var closing = Array.IndexOf(lines, Delimiter, 1);
        if (closing < 0)
        {
            throw new InvalidDataException("Canonical payload frontmatter must close with a delimiter line.");
        }

        var rendered = new List<string>(lines.Length) { lines[0] };
        var insideScope = false;
        for (var index = 1; index < closing; index++)
        {
            var line = lines[index];
            if (line == EmptyScopeLine)
            {
                insideScope = false;
                continue;
            }

            if (line == ScopeLine)
            {
                insideScope = true;
                continue;
            }

            if (insideScope && line.StartsWith(ScopeIndent, StringComparison.Ordinal))
            {
                rendered.Add(line[ScopeIndent.Length..]);
                continue;
            }

            insideScope = false;
            rendered.Add(line);
        }

        rendered.AddRange(lines[closing..]);
        return string.Join('\n', rendered);
    }
}
