using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Rendering;

internal static class InstallHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge install [--force] [--automatic] [--dry-run] [global options]"),
            new CliHelpSection(
                heading: "Establishment",
                body: "  Install adds the complete bundled Framework to the selected workspace. An identical managed installation needs no changes. Existing managed changes are preserved; use open-forge update to reconcile them."),
            new CliHelpSection(
                heading: "Write policy",
                body: "  --force may replace only one eligible existing item during initial installation. It never updates or adopts managed state. --automatic suppresses confirmation without adding force. --dry-run previews the same complete checked plan and writes nothing. Repeating the command makes no further changes."),
            new CliHelpSection(
                heading: "Confirmation",
                body: "  An interactive text request asks for confirmation once, after checks, if it would write files. Dry-run, unchanged installations, --automatic, JSON, and redirected requests never prompt. Redirected text requests that would write require --automatic."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view selects detail in text and JSON."),
            new CliHelpSection(
                heading: "Examples",
                body: "  open-forge install\n"
                + "  open-forge install --automatic --dry-run --json\n"
                + "  open-forge install --force\n"
                + "  open-forge install --force --automatic --dry-run"),
            new CliHelpSection(
                heading: "Related commands",
                body: "  open-forge update — reconcile an existing managed Framework installation.\n"
                + "  open-forge doctor — inspect blocked or unavailable lifecycle and safety facts.\n"
                + "  open-forge cleanup — remove a reported retained recovery artifact after review."),
            new CliHelpSection(heading: "Results and streams", body: CliResultHelp.ResultsAndStreams(InstallDefinitions.SchemaVersion)),
            new CliHelpSection(
                heading: "Notes",
                body: "  Install uses only the Framework payload embedded in the running CLI. It does not discover another workspace, fetch content, manipulate Git, repair markers, reconcile managed divergence, or roll back target effects."),
        ]);
}
