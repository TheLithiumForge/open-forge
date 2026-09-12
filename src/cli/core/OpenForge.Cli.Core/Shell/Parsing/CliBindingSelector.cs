using System.CommandLine;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;

namespace OpenForge.Cli.Core.Shell.Parsing;

internal static class CliBindingSelector
{
    internal static CliBindingSelection Select(CliParseOutcome parse)
    {
        ArgumentNullException.ThrowIfNull(parse);
        var command = parse.Result.CommandResult.Command;
        var binding = parse.Tree.FindBinding(command);
        if (binding is not null)
        {
            return new CliBindingSelection(CliBindingSelectionState.Leaf, command, binding);
        }

        if (ReferenceEquals(command, parse.Tree.Root))
        {
            return new CliBindingSelection(CliBindingSelectionState.Root, command, null);
        }

        return parse.Tree.IsGroup(command)
            ? new CliBindingSelection(CliBindingSelectionState.Group, command, null)
            : new CliBindingSelection(CliBindingSelectionState.NoLeaf, command, null);
    }
}
