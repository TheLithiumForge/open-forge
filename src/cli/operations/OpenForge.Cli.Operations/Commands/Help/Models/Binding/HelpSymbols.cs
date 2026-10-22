using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Help.Models.Binding;

internal sealed record HelpSymbols(Command HelpCommand, Argument<string?> Topic)
{
    internal static HelpSymbols Create()
    {
        var topic = new Argument<string?>(HelpDefinitions.TopicArgument.Name)
        {
            Description = HelpDefinitions.TopicArgument.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var command = new Command(HelpDefinitions.HelpCommand.Name, HelpDefinitions.HelpCommand.Description);
        command.Arguments.Add(topic);
        return new(command, topic);
    }
}
