using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Tool;

internal static class ToolBinding
{
    internal static Command CreateGroup()
    {
        var group = new Command("tool", "Manage reviewed external programs and their workspace configuration.");
        group.SetAction(static _ => 0);
        return group;
    }
}
