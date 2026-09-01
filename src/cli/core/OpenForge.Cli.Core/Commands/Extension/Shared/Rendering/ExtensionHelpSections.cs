using OpenForge.Cli.Core.Shell.Presentation;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;

internal static class ExtensionHelpSections
{
    internal static CliHelpContent CreateGroup()
    {
        return new CliHelpContent(
        [
            new CliHelpSection(
                "Notes",
                """
                  The extension group performs no operation.
                  Planned but unavailable operations: install, update, and remove.
                """),
        ]);
    }
}
