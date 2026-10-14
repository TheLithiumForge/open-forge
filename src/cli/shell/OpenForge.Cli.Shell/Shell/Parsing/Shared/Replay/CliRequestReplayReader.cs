using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

namespace OpenForge.Cli.Core.Shell.Parsing.Shared.Replay;

internal static class CliRequestReplayReader
{
    internal static CliRequestReplay Read(CliBindingParse parse, Command command)
    {
        var names = new List<string>();
        for (Command? current = command; current is not null && current is not RootCommand; current = current.Parents.OfType<Command>().SingleOrDefault())
            names.Insert(0, current.Name);
        var subject = names.Concat(parse.Result.CommandResult.Children.OfType<ArgumentResult>().SelectMany(result => result.Tokens).Select(token => token.Value));
        var options = Options(parse.Result.CommandResult).ToArray();
        var optionTokens = Tokens(options);
        var excluded = Tokens(options.Where(option => option.Option.Name == "--detail"));
        var debugArguments = parse.Result.Tokens.Where(token => !excluded.Contains(token)).Select(token => token.Value);
        var request = CliRetryCommand.Build(parse.OriginalArguments);
        var automaticOptions = options.Where(option => option.Option.Name == "--automatic").ToArray();
        var hasAutomatic = automaticOptions.Any(option => option.Option is Option<bool> boolean && parse.Result.GetValue(boolean));
        var automaticExcluded = Tokens(automaticOptions);
        var automatic = CliRetryCommand.Build(parse.Result.Tokens.Where(token => !automaticExcluded.Contains(token)).Select(token => token.Value));
        var previewExcluded = Tokens(options.Where(option => option.Option.Name is "--prune" or "--dry-run"));
        var preview = CliRetryCommand.Build(parse.Result.Tokens.Where(token => !previewExcluded.Contains(token)).Select(token => token.Value));
        var applyExcluded = Tokens(options.Where(option => option.Option.Name == "--dry-run"));
        var apply = CliRetryCommand.Build(parse.Result.Tokens.Where(token => !applyExcluded.Contains(token)).Select(token => token.Value));
        var interactiveExcluded = Tokens(options.Where(option => option.Option.Name is "--automatic" or "--dry-run"));
        var interactive = CliRetryCommand.Build(parse.Result.Tokens.Where(token => !interactiveExcluded.Contains(token)).Select(token => token.Value));
        return new(CliRetryCommand.Build(names), CliRetryCommand.Build(subject), request,
            $"{CliRetryCommand.Build(debugArguments)} --detail debug", hasAutomatic ? request : $"{automatic} --automatic")
        {
            OptionArguments = string.Join(' ', parse.Result.Tokens.Where(optionTokens.Contains).Select(token => CliCommandText.Quote(token.Value))),
            WorkspaceSpecified = options.Any(option => option.Option.Name == "--workspace" && !option.Implicit),
            PrunePreviewCommand = $"{preview} --prune --dry-run",
            ApplyCommand = apply,
            AutomaticApplyCommand = hasAutomatic ? apply : $"{apply} --automatic",
            InteractiveCommand = interactive,
        };
    }

    private static IEnumerable<OptionResult> Options(CommandResult command)
    {
        for (CommandResult? current = command; current is not null; current = current.Parent as CommandResult)
            foreach (var option in current.Children.OfType<OptionResult>()) yield return option;
    }

    private static HashSet<Token> Tokens(IEnumerable<OptionResult> options)
    {
        var tokens = new HashSet<Token>();
        foreach (var option in options.Where(option => !option.Implicit))
        {
            tokens.UnionWith(option.Tokens);
            if (option.IdentifierToken is { } identifier) tokens.Add(identifier);
        }
        return tokens;
    }
}
