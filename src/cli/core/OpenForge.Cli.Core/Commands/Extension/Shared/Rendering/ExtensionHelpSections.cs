using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Commands.Extension.Shared.Rendering;

internal static class ExtensionHelpSections
{
    internal static CliHelpContent CreateGroup() => new(
    [
        new CliHelpSection("Command help", "  Use open-forge extension <command> --help for selection, write policy, and examples."),
    ]);
}
