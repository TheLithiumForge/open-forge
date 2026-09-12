using OpenForge.Cli.Core.Commands.Status;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Status.Shared.Rendering;

internal static class StatusHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge status [global options]"),
            new CliHelpSection(
                "Inspection",
                "  Inspect the selected workspace's installation, startup and continuity context, generated navigation, lifecycle, managed targets, and exact recovery candidates."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view selects detail in text and JSON."),
            new CliHelpSection(
                "Related commands",
                """
                  open-forge doctor — diagnose unavailable or attention-requiring operational facts.
                  open-forge context — return startup or selected authored context.
                """),
            new CliHelpSection("Results and streams", CliResultHelp.ResultsAndStreams(StatusDefinitions.SchemaVersion)),
            new CliHelpSection(
                "Notes",
                "  Status is deterministic, stateless, and read-only. It does not acquire a workspace lock, repair lifecycle state, restore recovery data, or modify the workspace."),
        ]);
}
