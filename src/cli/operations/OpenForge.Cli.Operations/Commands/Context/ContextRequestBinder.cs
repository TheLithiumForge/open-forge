using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Commands.Context.Models.Request;
using OpenForge.Cli.Core.Commands.Context.Models.Result;
using OpenForge.Cli.Core.Commands.Context.Models.Selection;
using OpenForge.Cli.Core.Commands.Context.Shared.Binding;
using OpenForge.Cli.Core.Commands.Context.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Invocation.Models;

namespace OpenForge.Cli.Core.Commands.Context;

internal sealed class ContextRequestBinder(ContextSymbols symbols)
{
    private readonly ContextSymbols _symbols = symbols;

    internal CliBindResult<ContextRequest, ContextResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);
        var input = ContextBindingInputReader.Read(
            parse.Result,
            _symbols,
            invocation.SuppliedDetail,
            invocation.Presentation.Detail);
        var parser = new ContextRequestParser();
        var findings = ReadInputFindings(input).ToList();
        var workspace = invocation.Workspace;
        var workingPaths = ReadWorkingPaths(input, workspace, findings);
        input = input with
        {
            WorkingPaths = workingPaths,
            WorkingPathsSupplied = input.ForFacts?.IsExplicit == true,
        };
        var content = ReadContent(parser, input, findings);
        var linkExpansion = ReadLinkExpansion(parser, input, findings);
        if (findings.Count != 0)
        {
            return CliBindResult<ContextRequest, ContextResult>.Invalid(
                ContextPreOperationResultBuilder.Invalid(
                    input,
                    invocation.Workspace,
                    content,
                    linkExpansion,
                    findings));
        }

        if (workspace is null)
        {
            throw new InvalidOperationException("A bound Context invocation requires a selected workspace.");
        }
        return CliBindResult<ContextRequest, ContextResult>.Bound(
            new ContextRequest(
                workspace: workspace,
                sourceReferences: input.Sources,
                additionsOnly: input.AdditionsOnly,
                content: content,
                linkExpansion: linkExpansion,
                suppliedDetail: input.SuppliedDetail,
                effectiveView: input.EffectiveView)
            {
                WorkingPaths = input.WorkingPaths,
                WorkingPathsSupplied = input.WorkingPathsSupplied,
            });
    }

    internal static IReadOnlyList<ContextFinding> ReadInputFindings(ContextBindingInput input)
    {
        ArgumentNullException.ThrowIfNull(input);
        var findings = new List<ContextFinding>();
        if (input.AdditionsOnly && input.Sources.Count == 0)
        {
            findings.Add(InvalidFinding(
                ContextFindingCode.InvalidInput,
                ContextDefinitions.AdditionsOnly.Name,
                "Additions-only requires at least one explicit source reference."));
        }

        if (input.ForFacts?.IsExplicit == true
            && (input.ForValues.Count == 0
                || input.ForFacts.IdentifierCount != input.ForFacts.ValueCount))
        {
            findings.Add(InvalidFinding(
                ContextFindingCode.InvalidWorkingPath,
                ContextDefinitions.For.Name,
                string.Empty));
        }

        foreach (var source in input.Sources)
        {
            var parsed = SourceReferenceParser.Parse(source);
            if (parsed.State == SourceReferenceParseState.Invalid)
            {
                findings.Add(new ContextFinding(
                    code: ContextFindingCode.InvalidSource,
                    subject: source,
                    cause: parsed.Cause ?? "The source reference is invalid.",
                    reference: source,
                    source: null,
                    layer: null,
                    path: parsed.AttemptedPath,
                    part: null,
                    location: null,
                    destinationLocation: null,
                    candidates: [],
                    invalidSourceKind: ContextInvalidSourceKind.Other));
            }
        }
        return findings;
    }

    private static IReadOnlyList<string> ReadWorkingPaths(
        ContextBindingInput input,
        OpenForge.Cli.Core.Framework.Workspace.Models.CliWorkspace? workspace,
        ICollection<ContextFinding> findings)
    {
        if (input.ForFacts?.IsExplicit != true)
        {
            return [];
        }

        if (workspace is null)
        {
            return [];
        }

        var normalized = SourceWorkingPathNormalizer.Normalize(workspace.LexicalRoot, input.ForValues);
        foreach (var path in normalized.InvalidPaths)
        {
            findings.Add(InvalidFinding(
                ContextFindingCode.InvalidWorkingPath,
                path,
                string.Empty));
        }

        return normalized.Paths;
    }

    internal static ContextContentSelection ReadContent(
        ContextRequestParser parser,
        ContextBindingInput input,
        ICollection<ContextFinding> findings)
    {
        try
        {
            return parser.ParseContent(input);
        }
        catch (ContextBindingException exception)
        {
            findings.Add(InvalidFinding(
                ContextFindingCode.InvalidContent,
                exception.Subject,
                exception.Message));
            return new ContextContentSelection(supplied: [], effective: []);
        }
    }

    internal static ContextLinkExpansion ReadLinkExpansion(
        ContextRequestParser parser,
        ContextBindingInput input,
        ICollection<ContextFinding> findings)
    {
        try
        {
            return parser.ParseLinkExpansion(input);
        }
        catch (ContextBindingException exception)
        {
            findings.Add(InvalidFinding(
                ContextFindingCode.InvalidLinkDepth,
                exception.Subject,
                exception.Message));
            return ContextLinkExpansion.None;
        }
    }

    internal static ContextFinding InvalidFinding(
        ContextFindingCode code,
        string? subject,
        string cause)
        => new(
            code: code,
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
}
