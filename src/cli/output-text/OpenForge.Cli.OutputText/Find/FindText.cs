using System.Globalization;

namespace OpenForge.Cli.OutputText.Find;

internal static class FindText
{
    // @OpenForgeText find.message.the-search-is-incomplete
    internal static string MessageTheSearchIsIncomplete()
        => "The search is incomplete.";

    // @OpenForgeText find.message.find-was-cancelled
    internal static string MessageFindWasCancelled()
        => "Find was cancelled.";

    // @OpenForgeText find.message.list-source-ids-and-exact-paths-then-rerun-find
    internal static string MessageListSourceIdsAndExactPathsThenRerunFind()
        => "List source IDs and exact paths, then rerun Find.";

    // @OpenForgeText find.message.inspect-the-unavailable-source-or-projection-facts-before-relying-on-this-find-result
    internal static string MessageInspectTheUnavailableSourceOrProjectionFactsBeforeRelyingOnThisFindResult()
        => "Inspect the unavailable source or projection facts before relying on this Find result.";

    // @OpenForgeText find.heading.search-details
    internal static string HeadingSearchDetails()
        => "Search details:";

    // @OpenForgeText find.label.the-requested-search-could-not-be-started
    internal static string LabelTheRequestedSearchCouldNotBeStarted()
        => "the requested search could not be started";

    // @OpenForgeText find.label.matches
    internal static string LabelMatches()
        => "matches";

    // @OpenForgeText find.label.sources-inspected
    internal static string LabelSourcesInspected()
        => "sources inspected";

    // @OpenForgeText find.label.source-candidates
    internal static string LabelSourceCandidates()
        => "source candidates";

    // @OpenForgeText find.label.source
    internal static string LabelSource()
        => "source";

    // @OpenForgeText find.label.match
    internal static string LabelMatch()
        => "match";

    // @OpenForgeText find.message.no-markdown-sources-were-found-under-agents
    internal static string MessageNoMarkdownSourcesWereFoundUnderAgents()
        => "No Markdown sources were found under .agents.";

    // @OpenForgeText find.title.invalid-selector
    internal static string TitleInvalidSelector()
        => "Invalid selector";

    // @OpenForgeText find.title.selector-is-ambiguous
    internal static string TitleSelectorIsAmbiguous()
        => "Selector is ambiguous";

    // @OpenForgeText find.title.selector-is-unsafe
    internal static string TitleSelectorIsUnsafe()
        => "Selector is unsafe";

    // @OpenForgeText find.title.source-candidate-is-unsafe
    internal static string TitleSourceCandidateIsUnsafe()
        => "Source candidate is unsafe";

    // @OpenForgeText find.title.overwrite-has-no-base-source
    internal static string TitleOverwriteHasNoBaseSource()
        => "Overwrite has no base source";

    // @OpenForgeText find.title.source-could-not-be-inspected
    internal static string TitleSourceCouldNotBeInspected()
        => "Source could not be inspected";

    // @OpenForgeText find.title.find-failed
    internal static string TitleFindFailed()
        => "Find failed";

    // @OpenForgeText find.title.find-was-cancelled
    internal static string TitleFindWasCancelled()
        => "Find was cancelled";

    // @OpenForgeText find.label.the-selector
    internal static string LabelTheSelector()
        => "the selector";

    // @OpenForgeText find.title.find
    internal static string TitleFind()
        => "Find";

    // @OpenForgeText find.label.content
    internal static string LabelContent()
        => "content";

    // @OpenForgeText find.message.require-must-be-all-or-any
    internal static string MessageRequireMustBeAllOrAny()
        => "--require must be all or any.";

    // @OpenForgeText find.help.syntax
    internal static string HelpSyntax()
        => "open-forge find [--include <source-reference>]... [--exclude <source-reference>]... [--tag <tag>]... [--heading <heading>]... [--require <all|any>] [--within <part>[,<part>...]] [--content <part>[,<part>...]] [global options]";

    // @OpenForgeText find.help.source-references
    internal static string HelpSourceReferences()
        => "--include and --exclude accept one exact source ID or one exact .agents/... Markdown path per occurrence. All includes are combined, exclusions take priority, and an unresolved reference prevents the search.";

    // @OpenForgeText find.help.heading.predicates-and-regions
    internal static string HelpHeadingPredicatesAndRegions()
        => "Predicates and regions";

    // @OpenForgeText find.help.predicates-and-regions
    internal static string HelpPredicatesAndRegions()
        => "Repeat --tag or --heading to match more values. --require accepts all or any. --within selects document, frontmatter, body, or section:<name>; combine parts with commas in one value.";

    // @OpenForgeText find.help.heading.content-and-views
    internal static string HelpHeadingContentAndViews()
        => "Content and views";

    // @OpenForgeText find.help.content-and-views
    internal static string HelpContentAndViews()
        => "--content shows metadata, frontmatter, headings, body, or section:<name>. --detail selects minimal, standard, full, or debug detail in text and JSON.";

    // @OpenForgeText find.help.inherited-global-options
    internal static string HelpInheritedGlobalOptions()
        => "Find inherits --workspace <path>, --format <text|json>, --detail <minimal|standard|full|debug>, --detail-filter <error|warning|info|all>, --help, and --version. These options work the same way across commands.";

    // @OpenForgeText find.help.examples
    internal static string HelpExamples()
        => "open-forge find\n  open-forge find --tag Architecture\n  open-forge find --heading Instructions --detail minimal\n  open-forge find --tag Architecture --content section:Target\n  open-forge find --include docs --exclude guide --format json";

    // @OpenForgeText find.help.related-commands
    internal static string HelpRelatedCommands()
        => "open-forge route list — list routed source identities.\n  open-forge doctor — inspect unavailable or blocked source facts.\n  open-forge context — select and measure command context.";

    // @OpenForgeText find.help.notes
    internal static string HelpNotes()
        => "Find is a deterministic, read-only source inventory. It creates no index, cache, receipt, network request, mutation authority, or workspace write.";
}
