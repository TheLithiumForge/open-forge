using System.CommandLine;
using OpenForge.Cli.Core.Shell.Definitions.Models;

namespace OpenForge.Cli.Core.Commands.Find.Models.Binding;

internal sealed record FindSymbols(
    Command FindCommand,
    Option<string[]> Include,
    Option<string[]> Exclude,
    Option<string[]> Tag,
    Option<string[]> Heading,
    Option<string?> Require,
    Option<string?> Within,
    Option<string?> Content)
{
    internal static FindSymbols Create()
    {
        var findCommand = new Command(
            FindDefinitions.FindCommand.Name,
            FindDefinitions.FindCommand.Description);
        var include = CreateRepeatable(FindDefinitions.Include);
        var exclude = CreateRepeatable(FindDefinitions.Exclude);
        var tag = CreateRepeatable(FindDefinitions.Tag);
        var heading = CreateRepeatable(FindDefinitions.Heading);
        var require = CreateSingleton(FindDefinitions.Require);
        var within = CreateSingleton(FindDefinitions.Within);
        var content = CreateSingleton(FindDefinitions.Content);

        findCommand.Options.Add(include);
        findCommand.Options.Add(exclude);
        findCommand.Options.Add(tag);
        findCommand.Options.Add(heading);
        findCommand.Options.Add(require);
        findCommand.Options.Add(within);
        findCommand.Options.Add(content);

        return new(
            findCommand,
            include,
            exclude,
            tag,
            heading,
            require,
            within,
            content);
    }

    private static Option<string[]> CreateRepeatable(CliOptionDefinition<string[]> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };

    private static Option<string?> CreateSingleton(CliOptionDefinition<string?> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrOne,
        };
}
