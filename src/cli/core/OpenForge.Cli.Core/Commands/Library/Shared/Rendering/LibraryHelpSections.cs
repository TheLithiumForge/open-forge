using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Rendering;

internal static class LibraryHelpSections
{
    internal static CliHelpContent CreateGroup() => new(
    [
        new CliHelpSection("Command help", "  Use open-forge library <command> --help for arguments and examples."),
    ]);
}
