using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Rendering;

internal static class ExtensionListHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                heading: "Syntax",
                body: "  open-forge extension list [--installed] [--available] [--source <package-or-catalogue-path>] [global options]"),
            new CliHelpSection(
                heading: "Sections",
                body: "  With neither filter, show Installed then Available. --installed or --available alone selects one section; together they select both. "
                + "Repeating a section flag has no additional effect."),
            new CliHelpSection(
                heading: "Source and workspace",
                body: "  Installed facts use only .agents/open-forge.lifecycle.json schema v1 in the exact selected workspace. "
                + "Available facts use the embedded catalogue or one exact disjoint local package/catalogue supplied by --source. No network, registry, cache, or fallback is used."),
            new CliHelpSection(
                heading: "Global options",
                body: "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. "
                + "--view selects detail in text and JSON."),
            new CliHelpSection(
                heading: "Examples",
                body: """
                  open-forge extension list
                  open-forge extension list --installed --json
                  open-forge extension list --available --source D:/packages/open-forge
                """),
            new CliHelpSection(
                heading: "Results and streams",
                body: CliResultHelp.ResultsAndStreams(schemaVersion: 1)),
            new CliHelpSection(
                heading: "Notes",
                body: "  Extension List is deterministic and read-only. An available package is not necessarily installed. Installed packages are still listed when their source is unavailable. "
                + "The command writes no payload, lifecycle, generated navigation, backup, cache, temporary file, or diagnostic artifact."),
        ]);
}
