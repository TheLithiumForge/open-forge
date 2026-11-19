using System.CommandLine;
using OpenForge.Cli.Core.Commands.Stripping.Models.Binding;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Stripping;

/// <summary>
/// Declares and reads the selection flags uninstall and export share. The plain-docs Interface
/// contract's Selection Flags section defines them once for both commands.
/// </summary>
internal static class StripSelectionBinding
{
    internal static StripSelectionSymbols AddSymbols(Command command)
    {
        var strip = new Option<string[]>(StrippingDefinitions.Strip.Name)
        {
            Description = StrippingDefinitions.Strip.Description,
            HelpName = StrippingDefinitions.Strip.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var flatten = new Option<string?>(StrippingDefinitions.Flatten.Name)
        {
            Description = StrippingDefinitions.Flatten.Description,
            HelpName = StrippingDefinitions.Flatten.ValueName,
            Arity = ArgumentArity.ExactlyOne,
        };
        var removeEdited = new Option<string[]>(StrippingDefinitions.RemoveEdited.Name)
        {
            Description = StrippingDefinitions.RemoveEdited.Description,
            HelpName = StrippingDefinitions.RemoveEdited.ValueName,
            Arity = ArgumentArity.ZeroOrMore,
            AllowMultipleArgumentsPerToken = false,
        };
        var dryRun = new Option<bool>(StrippingDefinitions.DryRun.Name) { Description = StrippingDefinitions.DryRun.Description, Arity = ArgumentArity.Zero };
        command.Options.Add(strip);
        command.Options.Add(flatten);
        command.Options.Add(removeEdited);
        command.Options.Add(dryRun);
        return new StripSelectionSymbols { Strip = strip, Flatten = flatten, RemoveEdited = removeEdited, DryRun = dryRun };
    }

    internal static StripSelection Read(ParseResult result, StripSelectionSymbols symbols, StripCommandKind kind)
    {
        var stripFacts = CliOptionResultFactsReader.Read(result, symbols.Strip);
        if (stripFacts.IsExplicitWithoutValue)
        {
            return Problem(kind, $"supply a part after each {StrippingDefinitions.StripOption}.");
        }

        if (!StripPartGrammar.TryRead(CliOptionResultFactsReader.ReadValues(result, symbols.Strip), out var parts, out var rejected))
        {
            return Problem(kind, $"'{rejected}' is not a {StrippingDefinitions.StripOption} part. Use {StrippingDefinitions.PartList()}.");
        }

        var editedFacts = CliOptionResultFactsReader.Read(result, symbols.RemoveEdited);
        var edited = CliOptionResultFactsReader.ReadValues(result, symbols.RemoveEdited);
        if (editedFacts.IsExplicitWithoutValue || edited.Any(string.IsNullOrWhiteSpace))
        {
            return Problem(kind, $"supply a workspace-relative path after each {StrippingDefinitions.RemoveEditedOption}.");
        }

        return new StripSelection
        {
            Parts = parts,
            Flatten = result.GetValue(symbols.Flatten),
            RemoveEdited = [.. edited.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)],
            DryRun = result.GetValue(symbols.DryRun),
        };
    }

    private static StripSelection Problem(StripCommandKind kind, string problem)
        => new() { Problem = StripResultFactory.Cannot(kind, problem) };
}
