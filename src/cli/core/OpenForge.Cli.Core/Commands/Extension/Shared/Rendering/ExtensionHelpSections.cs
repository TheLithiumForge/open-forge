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
                  Install syntax: install [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--automatic] [--dry-run] [global flags].
                  Planned but unavailable operations: update and remove.
                """),
        ]);
    }
}
