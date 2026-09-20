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
        return new ContextBindingInput
        {
            Sources = ReadSources(result, symbols.Sources),
            AdditionsOnly = additionsFacts.IsExplicit,
            ContentValues = ReadValues(result, symbols.Content),
            FollowLinksValues = ReadValues(result, symbols.FollowLinks),
            AdditionsOnlyFacts = additionsFacts,
            ContentFacts = contentFacts,
            FollowLinksFacts = followLinksFacts,
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
        => result.GetValue(option) ?? [];
}
