using OpenForge.Cli.Core.Commands.Help.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Commands.Help;

internal static class HelpInvalidResultFactory
{
    internal static HelpResult Create(CliInvalidBindingInput input)
        => new()
        {
            Status = CliSemanticStatus.Invalid,
            Topic = null,
            Cause = string.Join(" ", input.InvalidInput.Diagnostics),
            Next = new CliNextAction("open-forge help --help", "Correct the Help input and rerun."),
        };
}
