using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Extension;

internal static class ExtensionBinding
{
    internal static Command CreateGroup()
    {
        var command = new Command(
            name: ExtensionDefinitions.ExtensionGroup.Name,
            description: ExtensionDefinitions.ExtensionGroup.Description);
        command.SetAction(static _ => 0);
        return command;
    }
}
