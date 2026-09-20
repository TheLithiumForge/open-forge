using OpenForge.Cli.Core.Commands.Index.Models.Binding;
using OpenForge.Cli.Core.Commands.Index.Models.Request;
using OpenForge.Cli.Core.Commands.Index.Models.Result;
using OpenForge.Cli.Core.Commands.Index.Shared.Binding;
using OpenForge.Cli.Core.Commands.Index.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Index;

internal sealed class IndexRequestBinder(
    IndexSymbols symbols,
    IndexResultBuilder resultBuilder)
{
    private readonly IndexSymbols _symbols = symbols;
    private readonly IndexResultBuilder _resultBuilder = resultBuilder;

    internal CliBindResult<IndexRequest, IndexResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var input = IndexBindingInputReader.Read(parse.Result, _symbols);
        var findings = ReadInputFindings(input);
        if (findings.Count != 0)
        {
            return CliBindResult<IndexRequest, IndexResult>.Invalid(
                _resultBuilder.CreateInvalid(input, invocation.Workspace, findings));
        }

        var workspace = invocation.Workspace
            ?? throw new InvalidOperationException("A bound Index invocation requires a selected workspace.");
        return CliBindResult<IndexRequest, IndexResult>.Bound(
            new IndexRequest(workspace, input.SourceReferences, input.Mode));
    }

    internal static IReadOnlyList<IndexFinding> ReadInputFindings(IndexBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var findings = new List<IndexFinding>();
        for (var index = 0; index < input.SourceReferences.Count; index++)
        {
            var parsed = SourceReferenceParser.Parse(input.SourceReferences[index]);
            if (parsed.State == SourceReferenceParseState.Valid)
            {
                continue;
            }

            findings.Add(new IndexFinding(
                IndexFindingCode.InvalidSource,
                sourceOccurrence: index + 1,
                source: null,
                cause: parsed.Cause ?? "The source reference is invalid.",
                candidates: []));
        }

        return findings;
    }
}
