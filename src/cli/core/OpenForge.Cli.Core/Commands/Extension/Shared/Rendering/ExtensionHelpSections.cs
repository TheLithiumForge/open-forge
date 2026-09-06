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
                  Update syntax:
                    update [<stable-id>...] [--source <package-or-catalogue-path>] [--all] [--force] [--prune]
                    [--automatic] [--dry-run] [global flags].
                  Remove syntax: remove [<stable-id>...] [--prune] [--automatic] [--dry-run] [global flags].
                """),
        ]);
    }
}
