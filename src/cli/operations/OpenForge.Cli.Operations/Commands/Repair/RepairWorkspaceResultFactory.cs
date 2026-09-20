using OpenForge.Cli.Core.Commands.Repair.Models.Binding;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Repair;

internal sealed class RepairWorkspaceResultFactory(RepairSymbols symbols)
{
    private readonly RepairSymbols _symbols = symbols;

    internal RepairResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var binder = new RepairRequestBinder(_symbols);
        var parsed = binder.ReadInput(input.BindingParse.Result);
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return RepairResult.Empty(
            null,
            parsed.Mode,
            parsed.Automatic,
            RepairSelectionMode.NonInteractiveBlocked,
            new RepairFinding(RepairFindingCode.InvalidInput, cause));
    }
}
