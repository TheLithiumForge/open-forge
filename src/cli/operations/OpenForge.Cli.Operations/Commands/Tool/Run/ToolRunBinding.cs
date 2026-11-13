using System.CommandLine;
using OpenForge.Cli.Core.Commands.Tool.Run.Models.Parsing;

namespace OpenForge.Cli.Core.Commands.Tool.Run;

internal static class ToolRunBinding
{
    internal static ToolRunSymbols CreateSymbols(Command group)
    {
        var command = new Command("run", "Run one declared, installed and verified workspace tool pin.");
        var id = new Argument<string>("tool") { Arity = ArgumentArity.ExactlyOne };
        command.Add(id);
        command.SetAction(static _ => 0);
        group.Add(command);
        return new(command, id);
    }
}
