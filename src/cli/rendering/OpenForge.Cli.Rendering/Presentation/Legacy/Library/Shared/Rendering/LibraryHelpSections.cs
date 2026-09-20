using OpenForge.Cli.Core.Commands.Library;
using OpenForge.Cli.Core.Commands.Library.Shared.Serialization;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Legacy.Library.Shared.Rendering;

internal static class LibraryHelpSections
{
    internal static CliHelpContent CreateGroup() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCommandHelp(), ("  " + global::OpenForge.Cli.OutputText.Library.Shared.LibrarySharedText.HelpCommandHelp())),
    ]);
}
