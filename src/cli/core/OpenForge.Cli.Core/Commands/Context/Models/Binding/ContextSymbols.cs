using System.CommandLine;

namespace OpenForge.Cli.Core.Commands.Context.Models.Binding;

internal sealed record ContextSymbols(
    Command ContextCommand,
    Argument<string[]> Sources,
    Option<bool> AdditionsOnly,
    Option<string[]> Content,
    Option<string[]> FollowLinks)
{
    internal static ContextSymbols Create()
    {
        var command = new Command(
            ContextDefinitions.ContextCommand.Name,
            ContextDefinitions.ContextCommand.Description);
        var sources = new Argument<string[]>(ContextDefinitions.SourcesName)
        {
            Description = ContextDefinitions.SourcesDescription,
            Arity = ArgumentArity.ZeroOrMore,
        };
        var additionsOnly = new Option<bool>(ContextDefinitions.AdditionsOnly.Name)
        {
            Description = ContextDefinitions.AdditionsOnly.Description,
            Arity = ArgumentArity.Zero,
        };
        var content = CreateScalar(ContextDefinitions.Content);
        var followLinks = CreateScalar(ContextDefinitions.FollowLinks);

        command.Arguments.Add(sources);
        command.Options.Add(additionsOnly);
        command.Options.Add(content);
        command.Options.Add(followLinks);
        return new ContextSymbols(
            ContextCommand: command,
            Sources: sources,
            AdditionsOnly: additionsOnly,
            Content: content,
            FollowLinks: followLinks);
    }

    private static Option<string[]> CreateScalar(
        Shell.Definitions.CliOptionDefinition<string[]> definition)
        => new(definition.Name)
        {
            Description = definition.Description,
            HelpName = definition.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
}
