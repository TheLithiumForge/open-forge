using System.CommandLine;
using System.CommandLine.Parsing;
using OpenForge.Cli.Core.Commands.Find.Models.Binding;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Models.Selection;
using OpenForge.Cli.Core.Commands.Find.Shared.Query;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Composition.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Input;

namespace OpenForge.Cli.Core.Commands.Find;

internal sealed class FindRequestBinder(
    FindSymbols symbols,
    FindResultBuilder resultBuilder)
{
    private readonly FindSymbols _symbols = symbols;
    private readonly FindResultBuilder _resultBuilder = resultBuilder;

    internal CliBindResult<FindRequest, FindResult> Bind(
        CliBindingParse parse,
        CliInvocation invocation)
    {
        ArgumentNullException.ThrowIfNull(parse);
        ArgumentNullException.ThrowIfNull(invocation);

        var queryInput = FindBindingSupport.ReadQueryInput(
            parse.Result,
            _symbols,
            invocation.Presentation.Detail);
        var parser = new FindQueryParser();
        try
        {
            var query = parser.Parse(queryInput);
            var presentation = FindBindingSupport.CreatePresentation(parser, queryInput);
            var workspace = invocation.Workspace
                ?? throw new InvalidOperationException("A bound Find invocation requires a selected workspace.");
            var request = new FindRequest(
                workspace,
                new FindUniverseFilter(queryInput.IncludeValues, queryInput.ExcludeValues),
                query,
                presentation);
            return CliBindResult<FindRequest, FindResult>.Bound(request);
        }
        catch (ArgumentException exception)
        {
            return CliBindResult<FindRequest, FindResult>.Invalid(
                FindBindingSupport.CreateInvalidResult(
                    _resultBuilder,
                    queryInput,
                    invocation.Workspace,
                    exception.Message));
        }
    }
}

internal static class FindBindingSupport
{
    internal static FindQueryInput ReadQueryInput(
        ParseResult result,
        FindSymbols symbols,
        CliDetail effectiveView)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);
        if (!Enum.IsDefined(effectiveView))
        {
            throw new ArgumentOutOfRangeException(nameof(effectiveView), effectiveView, "The Find effective view is not defined.");
        }

        var includeValues = result.GetValue(symbols.Include) ?? [];
        var excludeValues = result.GetValue(symbols.Exclude) ?? [];
        var tagValues = result.GetValue(symbols.Tag) ?? [];
        var headingValues = result.GetValue(symbols.Heading) ?? [];
        var includeFacts = CliOptionResultFactsReader.Read(result, symbols.Include);
        var excludeFacts = CliOptionResultFactsReader.Read(result, symbols.Exclude);
        var tagFacts = CliOptionResultFactsReader.Read(result, symbols.Tag);
        var headingFacts = CliOptionResultFactsReader.Read(result, symbols.Heading);
        var requireFacts = CliOptionResultFactsReader.Read(result, symbols.Require);
        var withinFacts = CliOptionResultFactsReader.Read(result, symbols.Within);
        var contentFacts = CliOptionResultFactsReader.Read(result, symbols.Content);
        var requireValue = ReadSingletonValue(result, symbols.Require, requireFacts);
        var withinValue = ReadSingletonValue(result, symbols.Within, withinFacts);
        var contentValue = ReadSingletonValue(result, symbols.Content, contentFacts);
        var suppliedDetail = HasExplicitOption(result, CliSyntaxDefinitions.Detail.Name)
            ? (CliDetail?)effectiveView
            : null;

        return new FindQueryInput(
            includeValues,
            excludeValues,
            tagValues,
            headingValues,
            requireValue,
            withinValue,
            contentValue,
            ReadPredicateOccurrences(result, tagValues, headingValues),
            includeFacts,
            excludeFacts,
            tagFacts,
            headingFacts,
            requireFacts,
            withinFacts,
            contentFacts,
            ReadSingletonSpelling(result, symbols.Require, requireFacts, requireValue),
            ReadSingletonSpelling(result, symbols.Within, withinFacts, withinValue),
            ReadSingletonSpelling(result, symbols.Content, contentFacts, contentValue),
            suppliedDetail,
            effectiveView);
    }

    internal static FindPresentationSelection CreatePresentation(
        FindQueryParser parser,
        FindQueryInput input)
    {
        ArgumentNullException.ThrowIfNull(parser);
        ArgumentNullException.ThrowIfNull(input);
        var content = parser.ParseContentSelection(input.ContentValue ?? input.ContentSpelling);
        return new FindPresentationSelection(
            input.SuppliedDetail,
            input.EffectiveView,
            new FindContentSelection(content.Supplied, content.Effective, input.ContentFacts.IsExplicit));
    }

    internal static FindResult CreateInvalidResult(
        FindResultBuilder resultBuilder,
        FindQueryInput input,
        CliWorkspace? workspace,
        string cause)
    {
        ArgumentNullException.ThrowIfNull(resultBuilder);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);

        var parser = new FindQueryParser();
        var query = RecoverQuery(parser, input);
        var content = RecoverContent(parser, input);
        var presentation = new FindPresentationSelection(
            input.SuppliedDetail,
            input.EffectiveView,
            content);
        var request = new FindRequestEcho(
            workspace,
            new FindUniverseFilter(input.IncludeValues, input.ExcludeValues),
            query,
            presentation);
        var finding = new FindFinding(
            FindFindingCode.InvalidInput,
            CliSemanticStatus.Invalid,
            null,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        return resultBuilder.Build(
            CreatePreOperationInput(
                input,
                request,
                finding,
                new FindStageCompletion(
                    FindCoverageState.NotStarted,
                    content.IsRequested
                        ? FindProjectionCoverageState.NotStarted
                        : FindProjectionCoverageState.NotRequested)));
    }

    internal static FindResult CreateWorkspaceUnavailableResult(
        FindResultBuilder resultBuilder,
        FindQueryInput input,
        CliInvalidBindingInput invalidInput)
    {
        ArgumentNullException.ThrowIfNull(resultBuilder);
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(invalidInput);

        var parser = new FindQueryParser();
        var query = parser.Parse(input);
        var presentation = CreatePresentation(parser, input);
        var cause = invalidInput.InvalidInput.Diagnostics.Count == 1
            ? invalidInput.InvalidInput.Diagnostics[0]
            : string.Join(" ", invalidInput.InvalidInput.Diagnostics);
        var subject = invalidInput.GlobalInput.WorkspaceValue
            ?? invalidInput.ProcessEnvironment.CurrentDirectory;
        var finding = new FindFinding(
            FindFindingCode.WorkspaceUnavailable,
            CliSemanticStatus.Blocked,
            subject,
            cause,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            []);
        var request = new FindRequestEcho(
            null,
            new FindUniverseFilter(input.IncludeValues, input.ExcludeValues),
            query,
            presentation);
        return resultBuilder.Build(
            CreatePreOperationInput(
                input,
                request,
                finding,
                new FindStageCompletion(
                    FindCoverageState.Blocked,
                    presentation.Content.IsRequested
                        ? FindProjectionCoverageState.Blocked
                        : FindProjectionCoverageState.NotRequested)));
    }

    private static FindResultInput CreatePreOperationInput(
        FindQueryInput input,
        FindRequestEcho request,
        FindFinding finding,
        FindStageCompletion stageCompletion)
        => new(
            request,
            CreateUnresolvedUniverse(input),
            [],
            [],
            [],
            [finding],
            stageCompletion,
            null);

    private static FindQuery RecoverQuery(
        FindQueryParser parser,
        FindQueryInput input)
    {
        try
        {
            return parser.Recover(input);
        }
        catch (ArgumentException)
        {
            var occurrences = input.PredicateOccurrences.Count != 0
                ? input.PredicateOccurrences.OrderBy(value => value.Position).ToArray()
                : input.TagValues
                    .Select(value => (Kind: FindPredicateKind.Tag, Value: value))
                    .Concat(input.HeadingValues.Select(value => (Kind: FindPredicateKind.Heading, Value: value)))
                    .Select((value, index) => new FindPredicateOccurrence(value.Kind, value.Value, index + 1))
                    .ToArray();
            var predicates = occurrences
                .Select(occurrence => new FindPredicate(
                    occurrence.Kind,
                    occurrence.Value,
                    occurrence.Kind == FindPredicateKind.Tag && occurrence.Value.StartsWith('#')
                        ? occurrence.Value[1..]
                        : occurrence.Value))
                .ToArray();
            var effective = predicates
                .Where((predicate, index) => predicates
                    .Take(index)
                    .All(previous => !AreEquivalent(previous, predicate)))
                .ToArray();
            return new FindQuery(
                predicates,
                effective,
                string.Equals(
                    input.RequireValue ?? input.RequireSpelling,
                    FindDefinitions.Any,
                    StringComparison.Ordinal)
                    ? FindRequirement.Any
                    : FindRequirement.All,
                DefaultRegions());
        }
    }

    private static FindContentSelection RecoverContent(
        FindQueryParser parser,
        FindQueryInput input)
    {
        try
        {
            var content = parser.ParseContentSelection(input.ContentValue ?? input.ContentSpelling);
            return new FindContentSelection(content.Supplied, content.Effective, input.ContentFacts.IsExplicit);
        }
        catch (ArgumentException)
        {
            return new FindContentSelection([], [], input.ContentFacts.IsExplicit);
        }
    }

    private static FindUniverse CreateUnresolvedUniverse(FindQueryInput input)
    {
        return new FindUniverse(
            input.IncludeValues.Count == 0 && input.ExcludeValues.Count == 0
                ? FindUniverseMode.Default
                : FindUniverseMode.Filtered,
            input.IncludeValues.Select(CreateUnresolvedSelector),
            input.ExcludeValues.Select(CreateUnresolvedSelector),
            null,
            null,
            null);
    }

    private static FindSelector CreateUnresolvedSelector(string value)
    {
        var parsed = SourceReferenceParser.Parse(value);
        return new FindSelector(
            value,
            parsed.Kind,
            FindSelectorResolution.Invalid,
            null,
            null,
            null,
            []);
    }

    private static FindRegionSelection DefaultRegions()
    {
        return new FindRegionSelection(
            [],
            [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
            [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]);
    }

    private static bool AreEquivalent(FindPredicate left, FindPredicate right)
        => left.Kind == right.Kind
            && string.Equals(left.ComparisonValue, right.ComparisonValue, StringComparison.OrdinalIgnoreCase);

    private static IReadOnlyList<FindPredicateOccurrence> ReadPredicateOccurrences(
        ParseResult result,
        IReadOnlyList<string> tagValues,
        IReadOnlyList<string> headingValues)
    {
        var tags = new Queue<string>(tagValues);
        var headings = new Queue<string>(headingValues);
        var occurrences = new List<FindPredicateOccurrence>();
        for (var index = 0; index < result.Tokens.Count; index++)
        {
            var token = result.Tokens[index];
            if (token.Type != TokenType.Option)
            {
                continue;
            }

            var kind = token.Value switch
            {
                "--tag" => FindPredicateKind.Tag,
                "--heading" => FindPredicateKind.Heading,
                _ => (FindPredicateKind?)null,
            };
            if (kind is null)
            {
                continue;
            }

            if (index + 1 >= result.Tokens.Count
                || result.Tokens[index + 1].Type != TokenType.Argument)
            {
                continue;
            }

            var queue = kind == FindPredicateKind.Tag ? tags : headings;
            var value = queue.Count == 0 ? null : queue.Dequeue();
            if (value is null
                || !string.Equals(value, result.Tokens[index + 1].Value, StringComparison.Ordinal))
            {
                throw new ArgumentException("Find predicate tokens must agree with typed parser values.");
            }

            occurrences.Add(new FindPredicateOccurrence(kind.Value, value, occurrences.Count + 1));
        }

        if (tags.Count != 0 || headings.Count != 0)
        {
            throw new ArgumentException("Find predicate values must have parser-owned occurrences.");
        }

        return occurrences;
    }

    private static string? ReadSingletonSpelling<T>(
        ParseResult result,
        Option<T> option,
        CliOptionResultFacts facts,
        string? typedValue)
    {
        if (!facts.IsExplicit || facts.ValueCount == 0)
        {
            return null;
        }

        if (result.GetResult(option) is not OptionResult optionResult)
        {
            return typedValue;
        }

        return optionResult.Tokens
            .LastOrDefault(token => token.Type == TokenType.Argument)
            ?.Value
            ?? typedValue;
    }

    private static string? ReadSingletonValue(
        ParseResult result,
        Option<string?> option,
        CliOptionResultFacts facts)
    {
        try
        {
            return result.GetValue(option);
        }
        catch (InvalidOperationException) when (facts.IdentifierCount != 1 || facts.ValueCount != 1)
        {
            return null;
        }
    }

    private static bool HasExplicitOption(ParseResult result, string optionName)
        => result.Tokens.Any(token => token.Type == TokenType.Option
            && string.Equals(token.Value, optionName, StringComparison.Ordinal));
}
