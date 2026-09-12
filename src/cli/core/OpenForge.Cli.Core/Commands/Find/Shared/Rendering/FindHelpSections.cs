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
                "  open-forge find [--include=<source-reference>]... [--exclude=<source-reference>]... "
                + "[--tag=<tag>]... [--heading=<heading>]... [--require=<all|any>] "
                + "[--within=<part>[,<part>...]] [--content=<part>[,<part>...]] [global flags]"),
            new CliHelpSection(
                "Source references",
                "  --include and --exclude accept one exact source ID or one exact .agents/... Markdown path "
                + "per occurrence. Includes form a union, exclusions win, and unresolved selectors fail closed."
                ),
            new CliHelpSection(
                "Predicates and regions",
                "  Repeat --tag or --heading for scalar predicates. --require accepts all or any. "
                + "--within selects document, frontmatter, body, or section:<name>; parts compose in one value."
                ),
            new CliHelpSection(
                "Content and views",
                "  --content projects metadata, frontmatter, headings, body, or section:<name>. "
                + "Human --view values are compact and expanded; --view is a no-op with --json."
                ),
            new CliHelpSection(
                "Inherited global options",
                "  Find inherits --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, "
                + "and --version. Standard grammar and terminal behavior remain Shell-owned."
                ),
            new CliHelpSection("Results and streams", ResultsAndStreams()),
            new CliHelpSection(
                "Examples",
                "  open-forge find\n"
                + "  open-forge find --tag=Architecture\n"
                + "  open-forge find --heading=Instructions --view=compact\n"
                + "  open-forge find --tag=Architecture --content=section:Target\n"
                + "  open-forge find --include=docs --exclude=guide --json"
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
            "  JSON writes one complete schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use stderr and remain bounded.",
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
