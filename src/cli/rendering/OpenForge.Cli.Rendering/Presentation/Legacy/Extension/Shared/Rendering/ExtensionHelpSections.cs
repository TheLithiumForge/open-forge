using OpenForge.Cli.Core.Commands.Extension;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Presentation.Legacy.Extension.Shared.Rendering;

internal static class ExtensionHelpSections
{
    internal static CliHelpContent CreateGroup() => new(
    [
        new CliHelpSection(global::OpenForge.Cli.OutputText.Shared.SharedText.HelpHeadingCommandHelp(), ("  " + global::OpenForge.Cli.OutputText.Extension.Shared.ExtensionSharedText.HelpCommandHelp())),
    ]);
}
