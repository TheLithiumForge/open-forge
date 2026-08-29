using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Binding;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Index;

internal sealed class IndexWorkspaceResultFactory(
    IndexSymbols symbols,
    IndexResultBuilder resultBuilder)
{
    private readonly IndexSymbols _symbols = symbols;
    private readonly IndexResultBuilder _resultBuilder = resultBuilder;

    internal IndexResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var parsed = IndexBindingInputReader.Read(input.BindingParse.Result, _symbols);
        var invalid = IndexRequestBinder.ReadInputFindings(parsed);
        if (invalid.Count != 0)
        {
            return _resultBuilder.CreateInvalid(parsed, null, invalid);
        }

        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return _resultBuilder.CreateInvalid(
            parsed,
            null,
            [
                new IndexFinding(
                    IndexFindingCode.WorkspaceUnavailable,
                    sourceOccurrence: null,
                    source: null,
                    cause: cause,
                    candidates: []),
            ]);
    }
}
