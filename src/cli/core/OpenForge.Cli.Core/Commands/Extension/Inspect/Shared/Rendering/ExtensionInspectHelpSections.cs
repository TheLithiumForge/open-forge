using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.Core.Shell.Presentation.Shared.Help;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Rendering;

internal static class ExtensionInspectHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge extension inspect <stable-id> [--source <package-or-catalogue-path>] [global options]"),
            new CliHelpSection(
                "Subject and source",
                "  Supply one exact lowercase stable ID. Available package details come from the bundled catalogue, or only from the separate local package or catalogue named by --source. There is no fallback, network, registry, cache, or approximate ID matching."),
            new CliHelpSection(
                "Inspection",
                "  Inspect reports independently trusted lifecycle, available package, dependency, path, current-byte, fingerprint, generated-boundary, and comparison facts. It never writes, mutates, executes package content, or invokes another command."),
            new CliHelpSection(
                "Global options",
                "  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command. --view selects detail in text and JSON."),
            new CliHelpSection(
                "Examples",
                """
                  open-forge extension inspect development-toolkit
                  open-forge extension inspect development-toolkit --source ./packages/toolkit --json
                """),
            new CliHelpSection(
                "Results and streams",
                CliResultHelp.ResultsAndStreams(schemaVersion: 1)),
            new CliHelpSection(
                "Fingerprint boundary",
                "  open-forge-markdown-v1 uses strict UTF-8, LF-only normalization, exact final Entries marker exclusion, and exact-byte fallback. An authored lifecycle exact-bytes record remains read-only evidence and never creates mutation authority."),
        ]);
}
