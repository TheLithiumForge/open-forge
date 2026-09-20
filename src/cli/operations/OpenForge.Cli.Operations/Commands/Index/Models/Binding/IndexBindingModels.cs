using System.CommandLine;
using OpenForge.Cli.Core.Commands.Index.Models.Request;

namespace OpenForge.Cli.Core.Commands.Index.Models.Binding;

internal sealed record IndexSymbols(
    Command IndexCommand,
    Argument<string[]> Sources,
    Option<bool> DryRun)
{
    internal static IndexSymbols Create()
    {
        var command = new Command(
            IndexDefinitions.IndexCommand.Name,
            IndexDefinitions.IndexCommand.Description);
        var sources = new Argument<string[]>(IndexDefinitions.Sources.Name)
        {
            Description = IndexDefinitions.Sources.Description,
            Arity = ArgumentArity.ZeroOrMore,
        };
        var dryRun = new Option<bool>(IndexDefinitions.DryRun.Name)
        {
            Description = IndexDefinitions.DryRun.Description,
            Arity = ArgumentArity.Zero,
        };
        command.Arguments.Add(sources);
        command.Options.Add(dryRun);
        return new IndexSymbols(command, sources, dryRun);
    }
}

internal sealed record IndexBindingInput(
    IReadOnlyList<string> SourceReferences,
    IndexMode Mode);
