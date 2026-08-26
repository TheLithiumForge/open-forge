using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Shared.Binding;
using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;

namespace OpenForge.Cli.Core.Commands.Context;

internal sealed class ContextWorkspaceResultFactory(ContextSymbols symbols)
{
    private readonly ContextSymbols _symbols = symbols;

    internal ContextResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var parsed = ContextBindingInputReader.Read(
            input.BindingParse.Result,
            _symbols,
            input.GlobalInput.ViewOccurrences == 0 ? null : input.GlobalInput.View,
            input.GlobalInput.View);
        var parser = new ContextRequestParser();
        var invalid = ContextRequestBinder.ReadInputFindings(parsed).ToList();
        var content = ContextRequestBinder.ReadContent(parser, parsed, invalid);
        var linkExpansion = ContextRequestBinder.ReadLinkExpansion(parser, parsed, invalid);
        if (invalid.Count != 0)
        {
            return ContextPreOperationResultBuilder.Invalid(
                parsed,
                null,
                content,
                linkExpansion,
                invalid);
        }

        var subject = input.GlobalInput.WorkspaceValue
            ?? input.ProcessEnvironment.CurrentDirectory;
        var cause = input.InvalidInput.Diagnostics.Count == 1
            ? input.InvalidInput.Diagnostics[0]
            : string.Join(" ", input.InvalidInput.Diagnostics);
        var finding = new ContextFinding(
            code: ContextFindingCode.WorkspaceUnavailable,
            subject: subject,
            cause: cause,
            reference: null,
            source: null,
            layer: null,
            path: null,
            part: null,
            location: null,
            destinationLocation: null,
            candidates: []);
        return ContextPreOperationResultBuilder.WorkspaceBlocked(
            parsed,
            content,
            linkExpansion,
            finding);
    }
}
