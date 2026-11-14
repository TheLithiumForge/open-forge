using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool;

internal static class ToolBinding
{
    internal static Command CreateGroup()
    {
        var group = new Command(ToolDefinitions.Group.Name, ToolDefinitions.Group.Description);
        group.SetAction(static _ => 0);
        return group;
    }
}
