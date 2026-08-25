using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Find;

internal sealed class FindWorkspaceResultFactory(
    FindSymbols symbols,
    FindResultBuilder resultBuilder)
{
    private readonly FindSymbols _symbols = symbols;
    private readonly FindResultBuilder _resultBuilder = resultBuilder;

    internal FindResult Create(CliInvalidBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        var queryInput = FindBindingSupport.ReadQueryInput(
            input.BindingParse.Result,
            _symbols,
            input.GlobalInput.View);
        try
        {
            var parser = new Shared.Query.FindQueryParser();
            _ = parser.Parse(queryInput);
            _ = FindBindingSupport.CreatePresentation(parser, queryInput);
        }
        catch (ArgumentException exception)
        {
            return FindBindingSupport.CreateInvalidResult(
                _resultBuilder,
                queryInput,
                null,
                exception.Message);
        }

        return FindBindingSupport.CreateWorkspaceUnavailableResult(
            _resultBuilder,
            queryInput,
            input);
    }
}
