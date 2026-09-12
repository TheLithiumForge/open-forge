using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Index.Shared.Rendering;

internal static class IndexHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge index [source-reference...] [--dry-run] [global options]"),
            new CliHelpSection(
                "Selection",
                """
                  With no source operands, Index rebuilds the Loader and every reachable entrypoint region. An entrypoint selects its subtree and direct exposing parent; a routed leaf selects its direct exposing parent.
                """),
            new CliHelpSection(
                "Write policy",
                "  Omit --dry-run to apply bounded generated-interior changes. --dry-run previews the same complete checked plan and writes nothing. Repeating the command makes no further changes."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view is ignored with --json."),
            new CliHelpSection(
                "Examples",
                """
                  open-forge index
                  open-forge index memory --dry-run
                  open-forge index .agents/memory/_memory.md --json
                """),
            new CliHelpSection(
                "Related commands",
                """
                  open-forge doctor — inspect blocked topology, metadata, or generated-region facts.
                  open-forge cleanup — remove a reported retained recovery artifact after review.
                """),
            new CliHelpSection("Results and streams", CliResultHelp.ResultsAndStreams(IndexDefinitions.SchemaVersion)),
            new CliHelpSection(
                "Notes",
                "  Index changes only valid bounded generated Entries interiors. It does not repair markers, format complete files, modify overwrites, search for another workspace, or create Git commits."),
        ]);
}
