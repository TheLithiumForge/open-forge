using OpenForge.Cli.Core.Commands.References.Models.Binding;
using OpenForge.Cli.Core.Commands.References.Models.Request;
using OpenForge.Cli.Core.Commands.References.Models.Result;
using OpenForge.Cli.Core.Commands.References.Shared.Binding;
using OpenForge.Cli.Core.Commands.References.Shared.Result;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.References;

internal sealed class ReferencesRequestBinder(ReferencesSymbols symbols)
{
    private readonly ReferencesSymbols _symbols = symbols;

    internal CliBindResult<ReferencesRequest, ReferencesResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);

        var input = ReferencesBindingInputReader.Read(parse.Result, _symbols);
        var invalid = ReferencesBindingValidator.ReadInvalidFindings(input);
        if (invalid.Count != 0)
        {
            return CliBindResult<ReferencesRequest, ReferencesResult>.Invalid(
                ReferencesBindingResultBuilder.CreateInvalidResult(input, invocation.Workspace, invalid));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound References invocation requires a selected workspace.");
        if (input.Direction is not { } direction)
        {
            throw new InvalidOperationException("Nullable object must have a value.");
        }

        if (input.SourceReference is not { } sourceReference)
        {
            throw new ArgumentNullException(nameof(sourceReference));
        }

        return CliBindResult<ReferencesRequest, ReferencesResult>.Bound(
            new ReferencesRequest(
                workspace,
                sourceReference,
                direction,
                input.SelectorOccurrences));
    }
}
