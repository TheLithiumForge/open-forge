using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal sealed class ExtensionInspectWorkspaceResultFactory(ExtensionInspectSymbols symbols)
{
    private readonly ExtensionInspectSymbols _symbols = symbols;

    internal ExtensionInspectResult Create(CliInvalidBindingInput input)
    {
        var supplied = input.BindingParse.Result.GetValue(_symbols.StableId);
        var source = input.BindingParse.Result.GetValue(_symbols.Source);
        var sourceFacts = CliOptionResultFactsReader.Read(input.BindingParse.Result, _symbols.Source);
        if (sourceFacts.IdentifierCount > 1 || sourceFacts.IsExplicitWithoutValue)
        {
            return ExtensionInspectResultBuilder.Invalid(
                null,
                supplied,
                "--source accepts exactly one non-empty package or catalogue path.");
        }

        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        return ExtensionInspectResultBuilder.WorkspaceBlocked(supplied, cause);
    }
}
