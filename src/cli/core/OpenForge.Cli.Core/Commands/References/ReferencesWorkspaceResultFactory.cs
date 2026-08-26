using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Binding;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.References;

internal sealed class ReferencesWorkspaceResultFactory(
    ReferencesSymbols symbols,
    ReferencesResultBuilder resultBuilder)
{
    private readonly ReferencesSymbols _symbols = symbols;
    private readonly ReferencesResultBuilder _resultBuilder = resultBuilder;

    internal ReferencesResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var parsed = ReferencesBindingInputReader.Read(input.BindingParse.Result, _symbols);
        var invalid = ReferencesBindingValidator.ReadInvalidFindings(parsed);
        if (invalid.Count != 0)
        {
            return _resultBuilder.CreateInvalidResult(parsed, null, invalid);
        }

        var subject = input.GlobalInput.WorkspaceValue
            ?? input.ProcessEnvironment.CurrentDirectory;
        var finding = new ReferencesFinding(
            ReferencesFindingCode.WorkspaceUnavailable,
            null,
            subject,
            ReadWorkspaceCause(input),
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        return _resultBuilder.CreateBlockedResult(parsed, finding);
    }

    private static string ReadWorkspaceCause(CliInvalidBindingInput input)
        => input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
}
