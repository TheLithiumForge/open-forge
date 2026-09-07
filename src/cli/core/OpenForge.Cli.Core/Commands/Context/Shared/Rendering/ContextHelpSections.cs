using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge context [source-reference...] [--additions-only] [--content=<part>[,<part>...]] [--follow-links=<positive-depth|all>] [global flags]"),
            new CliHelpSection(
                heading: "Selection",
                body: "  With no source operand, Context returns the startup-required closure. Each explicit source ID or exact .agents path adds its routed closure. --additions-only requires an explicit source and removes the startup set from the result."),
            new CliHelpSection(
                heading: "Content",
                body: "  Parts are metadata, paths, frontmatter, headings, body, and section:<name>. Default: frontmatter,body. Use one --content value; comma and backslash may be escaped inside section names."),
            new CliHelpSection(
                heading: "Links",
                body: "  --follow-links accepts a positive base-10 depth or all. It follows contained local Markdown links breadth-first, never fetches external URLs, and records broken edges."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version retain their shared Shell meaning. --view is a no-op with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: """
                  open-forge context
                  open-forge context directives/public-facing-writing --content=headings,section:Instructions
                  open-forge context memory --additions-only --follow-links=2 --json
                """),
            new CliHelpSection(heading: "Results and streams", body: ResultsAndStreams()),
            new CliHelpSection(
                heading: "Notes",
                body: "  Context is deterministic, stateless, and read-only. It does not infer relevance, write receipts, persist a graph, fetch URLs, or modify the workspace."),
        ]);

    private static string ResultsAndStreams()
    {
        var lines = new List<string>
        {
            "  Human complete, attention, and incomplete results use stdout; invalid, blocked, failed, and interrupted results use stderr.",
            "  JSON writes one schema-version-1 envelope to stdout for every semantic status. Verbose diagnostics use bounded stderr.",
        };
        foreach (var status in Enum.GetValues<CliSemanticStatus>())
        {
            var definition = CliStatusDefinitions.Read(status);
            lines.Add(string.Create(
                CultureInfo.InvariantCulture,
                $"  {definition.MachineName}: exit {definition.Disposition.ExitCode} and human {Stream(definition.Disposition.HumanOutputTarget)}."));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string Stream(CliOutputTarget target)
        => target switch
        {
            CliOutputTarget.StandardOutput => "stdout",
            CliOutputTarget.StandardError => "stderr",
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "The output target is not defined."),
        };
}
