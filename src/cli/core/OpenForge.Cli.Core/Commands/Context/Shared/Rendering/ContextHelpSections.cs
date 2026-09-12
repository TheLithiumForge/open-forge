using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Rendering;

internal static class ContextHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge context [source-reference...] [--additions-only] [--content <part>[,<part>...]] [--follow-links <positive-depth|all>] [global options]"),
            new CliHelpSection(
                heading: "Selection",
                body: "  With no source reference, Context returns all required startup context. Each source ID or exact .agents path adds the source and its required route context. --additions-only requires an explicit source and removes the startup set from the result."),
            new CliHelpSection(
                heading: "Content",
                body: "  Parts are metadata, paths, frontmatter, headings, body, and section:<name>. Default: frontmatter,body. Use one --content value; comma and backslash may be escaped inside section names."),
            new CliHelpSection(
                heading: "Links",
                body: "  --follow-links accepts a positive base-10 depth or all. It follows contained local Markdown links breadth-first, never fetches external URLs, and records broken edges."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view is ignored with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: """
                  open-forge context
                  open-forge context directives/public-facing-writing --content headings,section:Instructions
                  open-forge context memory --additions-only --follow-links 2 --json
                """),
            new CliHelpSection(heading: "Results and streams", body: CliResultHelp.ResultsAndStreams(schemaVersion: 1)),
            new CliHelpSection(
                heading: "Notes",
                body: "  Context is deterministic, stateless, and read-only. It does not infer relevance, write receipts, persist a graph, fetch URLs, or modify the workspace."),
        ]);
}
