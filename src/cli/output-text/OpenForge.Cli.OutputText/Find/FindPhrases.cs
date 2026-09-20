using System.Globalization;

namespace OpenForge.Cli.OutputText.Find;

internal static class FindPhrases
{
    // @OpenForgeText find.phrase.matched-in
    internal static string FormatMatchedIn(string kindText, string valueText, string regionText)
        => $"  matched {kindText} {valueText} in {regionText}";

    // @OpenForgeText find.phrase.source-set
    internal static string FormatSourceSet(string modeText)
        => $"  Source set: {modeText}";

    // @OpenForgeText find.phrase.of-sources-inspected
    internal static string FormatOfSourcesInspected(string inspectedText, string candidatesText)
        => $"  {inspectedText} of {candidatesText} sources inspected";

    // @OpenForgeText find.phrase.filters
    internal static string FormatFilters(string joinText)
        => $"  Filters: {joinText}";

    // @OpenForgeText find.phrase.require
    internal static string FormatRequire(string requireText)
        => $"  Require: {requireText}";

    // @OpenForgeText find.phrase.regions-searched
    internal static string FormatRegionsSearched(string joinText)
        => $"  Regions searched: {joinText}";

    // @OpenForgeText find.phrase.no-sources-match
    internal static string FormatNoSourcesMatch(string queryText)
        => $"No sources match {queryText}.";

    // @OpenForgeText find.phrase.cannot-search
    internal static string FormatCannotSearch(string trimEndText)
        => $"Cannot search: {trimEndText}.";

    // @OpenForgeText find.phrase.find-stopped-because-of-an-unexpected-error
    internal static string FormatFindStoppedBecauseOfAnUnexpectedError(string trimEndText)
        => $"Find stopped because of an unexpected error: {trimEndText}.";

    // @OpenForgeText find.phrase.could-not-be-checked-safely-and-was-skipped
    internal static string FormatCouldNotBeCheckedSafelyAndWasSkipped(string pathText)
        => $"{pathText} could not be checked safely and was skipped.";

    // @OpenForgeText find.phrase.has-no-base-file-and-was-skipped
    internal static string FormatHasNoBaseFileAndWasSkipped(string valueText)
        => $"{valueText} has no base file and was skipped.";

    // @OpenForgeText find.phrase.could-not-be-read-and-was-skipped
    internal static string FormatCouldNotBeReadAndWasSkipped(string pathText)
        => $"{pathText} could not be read and was skipped.";

    // @OpenForgeText find.phrase.is-not-valid-utf-8-and-was-skipped
    internal static string FormatIsNotValidUtf8AndWasSkipped(string pathText)
        => $"{pathText} is not valid UTF-8 and was skipped.";

    // @OpenForgeText find.phrase.the-frontmatter-of-could-not-be-read-so-its-tags-were-not-matched
    internal static string FormatTheFrontmatterOfCouldNotBeReadSoItsTagsWereNotMatched(string pathText)
        => $"The frontmatter of {pathText} could not be read, so its tags were not matched.";

    // @OpenForgeText find.phrase.has-more-than-one-section-named-none-was-returned
    internal static string FormatHasMoreThanOneSectionNamedNoneWasReturned(string pathText, string valueText)
        => $"{pathText} has more than one section named {valueText}. None was returned.";

    // @OpenForgeText find.phrase.within-is-not-a-known-region
    internal static string FormatWithinIsNotAKnownRegion(string valueText)
        => $"--within {valueText} is not a known region.";

    // @OpenForgeText find.phrase.is-not-a-source-id-or-a-path-under-agents
    internal static string FormatIsNotASourceIdOrAPathUnderAgents(string optionText, string valueText)
        => $"{optionText} {valueText} is not a source ID or a path under .agents.";
}
