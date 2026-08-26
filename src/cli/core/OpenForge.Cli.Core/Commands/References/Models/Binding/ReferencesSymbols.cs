using System.CommandLine;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.References.Models.Binding;

internal sealed record ReferencesSymbols(
    Command ReferencesCommand,
    Argument<string?> Source,
    Option<string[]> Direction,
    Option<string[]> Include,
    Option<string[]> Exclude)
{
    internal static ReferencesSymbols CreateSymbols()
    {
        var command = new Command(
            ReferencesDefinitions.ReferencesCommand.Name,
            ReferencesDefinitions.ReferencesCommand.Description);
        var source = new Argument<string?>(ReferencesDefinitions.Source.Name)
        {
            Description = ReferencesDefinitions.Source.Description,
            Arity = ArgumentArity.ZeroOrOne,
        };
        var direction = new Option<string[]>(ReferencesDefinitions.Direction.Name)
        {
            Description = ReferencesDefinitions.Direction.Description,
            HelpName = ReferencesDefinitions.Direction.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var include = CreateRepeatable(ReferencesDefinitions.Include);
        var exclude = CreateRepeatable(ReferencesDefinitions.Exclude);

        command.Arguments.Add(source);
        command.Options.Add(direction);
        command.Options.Add(include);
        command.Options.Add(exclude);
        return new(command, source, direction, include, exclude);
    }

    private static Option<string[]> CreateRepeatable(
        CliOptionDefinition<string[]> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
}
