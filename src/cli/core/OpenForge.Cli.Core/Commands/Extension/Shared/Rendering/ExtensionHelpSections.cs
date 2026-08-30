using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;

internal static class ExtensionHelpSections
{
    internal static CliHelpContent CreateGroup()
        => new(
        [
            new CliHelpSection(
                heading: "Operations",
                body: """
                  list     Report separate Installed and Available package facts.
                  inspect  Inspect one exact stable package ID.
                  create   Create one local catalogue scaffold without installing it.
                  install  Planned; establish managed Extension ownership.
                  update   Planned; reconcile trusted managed packages.
                  remove   Planned; release trusted managed ownership.
                """),
            new CliHelpSection(
                heading: "Notes",
                body: "  The bare extension group performs no package, catalogue, lifecycle, or mutation work."),
        ]);
}
