using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal sealed class ExtensionListWorkspaceResultFactory(ExtensionListSymbols symbols)
{
    private readonly ExtensionListSymbols _symbols = symbols;

    internal ExtensionListResult Create(CliInvalidBindingInput input)
    {
        var selection = ExtensionListRequestBinder.ReadSelection(input.BindingParse.Result, _symbols);
        var source = input.BindingParse.Result.GetValue(_symbols.Source);
        var sourceFacts = Shell.Parsing.CliOptionResultFactsReader.Read(input.BindingParse.Result, _symbols.Source);
        if (sourceFacts.IdentifierCount > 1 || sourceFacts.IsExplicitWithoutValue)
        {
            return ExtensionListRequestBinder.CreateInvalidResult(
                workspace: null,
                selection: selection,
                source: source,
                cause: "--source accepts exactly one non-empty package or catalogue path.");
        }

        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        var subject = input.GlobalInput.WorkspaceValue ?? input.ProcessEnvironment.CurrentDirectory;
        return new ExtensionListResult(
            status: CliSemanticStatus.Blocked,
            workspace: null,
            selection: selection,
            source: source is null
                ? null
                : new ExtensionListSource
                {
                    Identity = source,
                    Kind = null,
                    State = ExtensionSourceReadState.Unavailable,
                },
            lifecycleTrust: null,
            installedCoverage: selection.Installed ? ExtensionListCoverage.Blocked : ExtensionListCoverage.NotRequested,
            availableCoverage: selection.Available ? ExtensionListCoverage.Blocked : ExtensionListCoverage.NotRequested,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    code: ExtensionListFindingCode.WorkspaceUnavailable,
                    status: CliSemanticStatus.Blocked,
                    subject: subject,
                    cause: cause),
            ],
            next: ExtensionListDefinitions.BlockedNext);
    }
}
