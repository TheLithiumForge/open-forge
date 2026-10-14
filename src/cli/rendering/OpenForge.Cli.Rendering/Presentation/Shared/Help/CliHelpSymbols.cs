using System.CommandLine;
using System.CommandLine.Parsing;
using System.Collections;
using System.Globalization;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.OutputText.Shared;

namespace OpenForge.Cli.Core.Presentation.Shared.Help;

// System.CommandLine 2.0.11 exposes symbol metadata, while its layout builder is internal.
internal static class CliHelpSymbols
{
    internal static string Usage(ParseResult result)
    {
        var commands = new Stack<string>();
        for (var current = result.CommandResult; current is not null; current = current.Parent as CommandResult)
        {
            if (current.Command is not RootCommand)
            {
                commands.Push(current.Command.Name);
            }
        }

        var command = result.CommandResult.Command;
        var path = string.Join(' ', commands.Prepend(CliSyntaxDefinitions.ExecutableName));
        if (command.Subcommands.Count > 0)
        {
            return $"{path} <command> [options]";
        }

        return $"{path} [options]";
    }

    internal static string CommandLabel(Command command)
        => string.Join(' ', new[] { command.Name }.Concat(command.Arguments.Where(argument => !argument.Hidden).Select(argument => $"<{argument.Name}>")));

    internal static string GlobalOptionNames(ParseResult result)
    {
        var root = result.CommandResult;
        while (root.Parent is CommandResult parent)
        {
            root = parent;
        }

        return string.Join(", ", root.Command.Options.Where(option => !option.Hidden).Select(option => option.Name));
    }

    internal static IEnumerable<(string Label, string? Description)> Options(ParseResult result, bool includeInherited = true)
    {
        var seen = new HashSet<Option>();
        for (var current = result.CommandResult; current is not null; current = current.Parent as CommandResult)
        {
            if (!includeInherited && current != result.CommandResult)
            {
                yield break;
            }

            foreach (var option in current.Command.Options)
            {
                if (option.Hidden || (current != result.CommandResult && !option.Recursive) || !seen.Add(option))
                {
                    continue;
                }

                var label = string.Join(", ", new[] { option.Name }.Concat(option.Aliases));
                if (option.Arity.MaximumNumberOfValues > 0)
                {
                    label = $"{label} <{option.HelpName ?? option.Name.TrimStart('-')}>";
                }

                var description = option.Description;
                if (option.HasDefaultValue && option.Arity.MaximumNumberOfValues > 0 && option.GetDefaultValue() is { } value)
                {
                    var defaultValue = DefaultValue(value);
                    if (defaultValue is not null)
                    {
                        description = $"{description} {SharedText.HelpDefaultValue(defaultValue)}";
                    }
                }

                yield return (label, description);
            }
        }
    }

    private static string? DefaultValue(object value)
        => value switch
        {
            string text => text,
            Enum or bool => value.ToString()?.ToLowerInvariant(),
            IEnumerable => null,
            _ => Convert.ToString(value, CultureInfo.InvariantCulture),
        };
}
