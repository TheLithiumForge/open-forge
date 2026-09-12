using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Cleanup.Shared.Rendering;

internal static class CleanupHelpSections
{
    internal static CliHelpContent Create()
        => new(
        [
            new CliHelpSection(
                "Syntax",
                "  open-forge cleanup [--dry-run] [global options]"),
            new CliHelpSection(
                "Catalogue",
                """
                  Remove every recognized completed recovery bundle and draft with an exact expected name for the selected workspace.
                  Unknown, malformed, unsupported, unavailable, and unsafe items remain preserved.
                """),
            new CliHelpSection(
                "Write policy",
                """
                  Omit --dry-run to apply the complete deletion plan while holding the workspace lock.
                  --dry-run previews the same plan without locking the workspace or writing files.
                """),
            new CliHelpSection(
                "Global options",
                """
                  --workspace <path>, --json, --view <compact|expanded>, --verbose, --help, and --version apply to this command.
                """),
            new CliHelpSection(
                "Notes",
                """
                  Cleanup accepts no operands, selectors, prompts, confirmations, force mode, age filters, glob filters, or arbitrary recursive deletion.
                  Each run rechecks the exact catalogue before making changes and verifies each deletion.
                """),
        ]);
}
