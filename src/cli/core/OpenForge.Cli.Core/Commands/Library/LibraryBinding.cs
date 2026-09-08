using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Library;

internal static class LibraryBinding
{
    internal static Command CreateGroup()
    {
        var group = new Command(LibraryDefinitions.Group.Name, LibraryDefinitions.Group.Description);
        group.SetAction(static _ => 0);
        return group;
    }
}
