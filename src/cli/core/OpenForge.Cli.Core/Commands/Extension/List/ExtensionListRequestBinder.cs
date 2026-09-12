using System.CommandLine;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Binding;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal sealed class ExtensionListRequestBinder(ExtensionListSymbols symbols)
{
    private readonly ExtensionListSymbols _symbols = symbols;

    internal CliBindResult<ExtensionListRequest, ExtensionListResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Extension List invocation requires a selected workspace.");
        var selection = ReadSelection(parse.Result, _symbols);
        var sourceFacts = CliOptionResultFactsReader.Read(parse.Result, _symbols.Source);
        var source = parse.Result.GetValue(_symbols.Source);
        if (sourceFacts.IdentifierCount > 1
            || sourceFacts.IsExplicitWithoutValue
            || source is not null && string.IsNullOrWhiteSpace(source))
        {
            return CliBindResult<ExtensionListRequest, ExtensionListResult>.Invalid(
                CreateInvalidResult(
                    workspace: workspace,
                    selection: selection,
                    source: source,
                    cause: "--source accepts exactly one non-empty package or catalogue path."));
        }

        return CliBindResult<ExtensionListRequest, ExtensionListResult>.Bound(
            new ExtensionListRequest
            {
                Workspace = workspace,
                Selection = selection,
                ExplicitSource = source,
            });
    }

    internal static ExtensionListSelection ReadSelection(ParseResult result, ExtensionListSymbols symbols)
        => ExtensionListSelection.Create(
            installedFlag: result.GetValue(symbols.Installed),
            availableFlag: result.GetValue(symbols.Available));

    internal static ExtensionListResult CreateInvalidResult(
        Framework.Workspace.Models.CliWorkspace? workspace,
        ExtensionListSelection selection,
        string? source,
        string cause)
        => new(
            status: CliSemanticStatus.Invalid,
            workspace: workspace,
            selection: selection,
            source: source is null
                ? null
                : new ExtensionListSource
                {
                    Identity = source,
                    Kind = null,
                    State = ExtensionSourceReadState.Invalid,
                },
            lifecycleTrust: null,
            installedCoverage: selection.Installed ? ExtensionListCoverage.Incomplete : ExtensionListCoverage.NotRequested,
            availableCoverage: selection.Available ? ExtensionListCoverage.Incomplete : ExtensionListCoverage.NotRequested,
            installed: [],
            available: [],
            findings:
            [
                new ExtensionListFinding(
                    code: ExtensionListFindingCode.InvalidInput,
                    status: CliSemanticStatus.Invalid,
                    subject: source,
                    cause: cause),
            ],
            next: ExtensionListDefinitions.InvalidNext);
}
