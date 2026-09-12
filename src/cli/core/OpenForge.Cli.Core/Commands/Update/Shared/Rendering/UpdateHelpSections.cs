using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Rendering;

internal static class UpdateHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge update [--force] [--prune] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                heading: "Reconciliation",
                body: "  Update compares the managed Framework installation with the complete version bundled in this CLI. An installation already at that version needs no writes."),
            new CliHelpSection(
                heading: "Authority",
                body: "  --force may replace changed or restore missing current managed content. "
                + "--prune may delete eligible retired managed content. The flags are independent; "
                + "--automatic enables neither. Safe source changes and new targets do not require force."),
            new CliHelpSection(
                heading: "Execution",
                body: "  --automatic suppresses confirmation without adding force or prune. "
                + "--dry-run previews the same complete checked plan and writes nothing. "
                + "An interactive text request asks once after checks if it would write files. "
                + "JSON and redirected execution never prompt."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view is ignored with --json."),
            new CliHelpSection(
                heading: "Examples",
                body: "  open-forge update\n"
                + "  open-forge update --automatic --dry-run --json\n"
                + "  open-forge update --force\n"
                + "  open-forge update --force --prune --automatic"),
            new CliHelpSection(
                heading: "Related commands",
                body: "  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n"
                + "  open-forge cleanup — remove a reported retained recovery artifact after review."),
            new CliHelpSection(heading: "Results and streams", body: CliResultHelp.ResultsAndStreams(UpdateDefinitions.SchemaVersion)),
            new CliHelpSection(
                heading: "Notes",
                body: "  Update uses only trusted local lifecycle state and the Framework payload embedded in the running CLI. "
                + "It does not fetch content, adopt unmanaged files, manipulate Git, restore targets automatically, "
                + "or remove recovery artifacts owned by Cleanup."),
        ]);
}
