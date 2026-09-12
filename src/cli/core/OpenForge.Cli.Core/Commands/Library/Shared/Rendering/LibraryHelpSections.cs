using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryHelpSections
{
    internal static CliHelpContent CreateGroup() => new(
    [
        new CliHelpSection("Commands", """
              list     Observe registered Libraries without inventorying source files.
              inspect  Inspect one Library's complete inventory and projection.
              attach   Register one contained source root and project its eligible files.
              sync     Reconcile one registered Library with its complete source inventory.
              detach   Remove one exact registered projection while preserving its source.
            """),
    ]);
}
