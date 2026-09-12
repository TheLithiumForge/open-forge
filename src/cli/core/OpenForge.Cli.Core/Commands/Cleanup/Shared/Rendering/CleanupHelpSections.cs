using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge cleanup [--dry-run] [global flags]"),
            new CliHelpSection(
                "Catalogue",
                """
                  Remove every positively recognized recovery final and ordinary exact-name draft
                  for the selected workspace.
                  Unknown, malformed, unsupported, unavailable, and unsafe items remain preserved.
                """),
            new CliHelpSection(
                "Write policy",
                """
                  Omit --dry-run to apply the complete deletion plan under one same-workspace lease.
                  --dry-run previews the same plan without acquiring a lease or writing files.
                """),
            new CliHelpSection(
                "Global options",
                """
                  --workspace <path>, --json, --view=<compact|expanded>, --verbose, --help, and --version
                  retain their shared Shell meaning.
                """),
            new CliHelpSection(
                "Notes",
                """
                  Cleanup accepts no operands, selectors, prompts, confirmations, force mode, age filters, glob filters,
                  or recursive arbitrary deletion. Every application revalidates the exact catalogue before effects
                  and verifies absence after each deletion.
                """),
        ]);
}
