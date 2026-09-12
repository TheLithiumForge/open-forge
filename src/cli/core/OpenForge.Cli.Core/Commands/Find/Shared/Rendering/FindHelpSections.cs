using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Find.Shared.Rendering;

internal static class FindHelpSections
{
    internal static CliHelpContent Create()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge find [--include <source-reference>]... [--exclude <source-reference>]... "
                + "[--tag <tag>]... [--heading <heading>]... [--require <all|any>] "
                + "[--within <part>[,<part>...]] [--content <part>[,<part>...]] [global options]"),
            new CliHelpSection(
                "Source references",
                "  --include and --exclude accept one exact source ID or one exact .agents/... Markdown path "
                + "per occurrence. All includes are combined, exclusions take priority, and an unresolved reference prevents the search."
                ),
            new CliHelpSection(
                "Predicates and regions",
                "  Repeat --tag or --heading to match more values. --require accepts all or any. "
                + "--within selects document, frontmatter, body, or section:<name>; combine parts with commas in one value."
                ),
            new CliHelpSection(
                "Content and views",
                "  --content shows metadata, frontmatter, headings, body, or section:<name>. "
                + "--view selects compact or expanded detail in text and JSON."
                ),
            new CliHelpSection(
                "Inherited global options",
                "  Find inherits --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, "
                + "and --version. These options work the same way across commands."
                ),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Examples",
                "  open-forge find\n"
                + "  open-forge find --tag Architecture\n"
                + "  open-forge find --heading Instructions --view compact\n"
                + "  open-forge find --tag Architecture --content section:Target\n"
                + "  open-forge find --include docs --exclude guide --json"
                ),
            new CliHelpSection(
                "Related commands",
                "  open-forge route list — list routed source identities.\n"
                + "  open-forge doctor — inspect unavailable or blocked source facts.\n"
                + "  open-forge context — select and measure command context."
                ),
            new CliHelpSection(
                "Notes",
                "  Find is a deterministic, read-only source inventory. It creates no index, cache, "
                + "receipt, network request, mutation authority, or workspace write."
                ),
        ]);
    }

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  Expanded JSON writes one complete schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use stderr and remain bounded.",
        };
        foreach (var status in new[]
        {
            CliSemanticStatus.Complete,
            CliSemanticStatus.Attention,
            CliSemanticStatus.Incomplete,
            CliSemanticStatus.Invalid,
            CliSemanticStatus.Blocked,
            CliSemanticStatus.Failed,
            CliSemanticStatus.Interrupted,
        })
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(
                $"  {definition.MachineName}: exit "
                + $"{definition.Disposition.ExitCode.ToString(CultureInfo.InvariantCulture)} "
                + $"and human {Stream(definition.Disposition.HumanOutputTarget)}.");
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => "stdout",
            CliOutputTarget.StandardError => "stderr",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "The CLI output target is not defined."),
        };
}
