using System.CommandLine;
using OpenForge.Cli.Core.Commands.Context.Models.Binding;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;

namespace OpenForge.Cli.Core.Commands.Context.Shared.Binding;

internal static class ContextBindingInputReader
{
    internal static ContextBindingInput Read(
        ParseResult result,
        ContextSymbols symbols,
        CliDetail? suppliedDetail,
        CliDetail effectiveView)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(symbols);
        var additionsFacts = CliOptionResultFactsReader.Read(result, symbols.AdditionsOnly);
        var contentFacts = CliOptionResultFactsReader.Read(result, symbols.Content);
        var followLinksFacts = CliOptionResultFactsReader.Read(result, symbols.FollowLinks);
        var forFacts = CliOptionResultFactsReader.Read(result, symbols.For);
        return new ContextBindingInput
        {
            Sources = ReadSources(result, symbols.Sources),
            AdditionsOnly = additionsFacts.IsExplicit,
            ContentValues = ReadValues(result, symbols.Content),
            FollowLinksValues = ReadValues(result, symbols.FollowLinks),
            ForValues = ReadValues(result, symbols.For),
            AdditionsOnlyFacts = additionsFacts,
            ContentFacts = contentFacts,
            FollowLinksFacts = followLinksFacts,
            ForFacts = forFacts,
            SuppliedDetail = suppliedDetail,
            EffectiveView = effectiveView,
        };
    }

    private static IReadOnlyList<string> ReadSources(
        ParseResult result,
        Argument<string[]> sources)
    {
        return result.GetValue(sources) ?? [];
    }

    private static IReadOnlyList<string> ReadValues(ParseResult result, Option<string[]> option)
        => CliOptionResultFactsReader.ReadValues(result, option);
}
