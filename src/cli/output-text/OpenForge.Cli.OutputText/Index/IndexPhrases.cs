using System.Globalization;

namespace OpenForge.Cli.OutputText.Index;

internal static class IndexPhrases
{
    // @OpenForgeText index.phrase.entries-sections-are-current-in-all-files-nothing-to-do
    internal static string FormatEntriesSectionsAreCurrentInAllFilesNothingToDo(string filesText)
        => $"Entries sections are current in all {filesText} files. Nothing to do.";

    // @OpenForgeText index.phrase.updated-the-entries-section-in-of
    internal static string FormatUpdatedTheEntriesSectionInOf(string updatedText, string filesText, string pluralText)
        => $"Updated the Entries section in {updatedText} of {filesText} {pluralText}.";

    // @OpenForgeText index.phrase.would-update-the-entries-section-in-of
    internal static string FormatWouldUpdateTheEntriesSectionInOf(string updatedText, string filesText, string pluralText)
        => $"Would update the Entries section in {updatedText} of {filesText} {pluralText}.";

    // @OpenForgeText index.phrase.index-stopped-after-of
    internal static string FormatIndexStoppedAfterOf(string updatedText, string filesText, string pluralText)
        => $"Index stopped after {updatedText} of {filesText} {pluralText}.";

    // @OpenForgeText index.phrase.index-was-cancelled-stopped-after-of
    internal static string FormatIndexWasCancelledStoppedAfterOf(string updatedText, string filesText, string pluralText)
        => $"Index was cancelled. Stopped after {updatedText} of {filesText} {pluralText}.";

    // @OpenForgeText index.phrase.nothing-was-written-the-other-current
    internal static string FormatNothingWasWrittenTheOtherCurrent(string filesText, string pluralText, string pluralText2)
        => $"Nothing was written. The other {filesText} {pluralText} {pluralText2} current.";

    // @OpenForgeText index.phrase.current
    internal static string FormatCurrent(string filesText, string pluralText, string pluralText2)
        => $"{filesText} {pluralText} {pluralText2} current.";

    // @OpenForgeText index.phrase.entries
    internal static string FormatEntries(string toStringText, string toStringText2)
        => $"{toStringText} -> {toStringText2} entries";

    // @OpenForgeText index.phrase.updated-the-entries-section-in-of-other-sources-were-skipped
    internal static string FormatUpdatedTheEntriesSectionInOfOtherSourcesWereSkipped(
        string updatedText,
        string filesText,
        string pluralText)
        => $"Updated the Entries section in {updatedText} of {filesText} {pluralText}; other sources were skipped.";

    // @OpenForgeText index.phrase.would-update-the-entries-section-in-of-other-sources-were-skipped
    internal static string FormatWouldUpdateTheEntriesSectionInOfOtherSourcesWereSkipped(
        string updatedText,
        string filesText,
        string pluralText)
        => $"Would update the Entries section in {updatedText} of {filesText} {pluralText}; other sources were skipped.";

    // @OpenForgeText index.phrase.optional-metadata-is-missing-for-observed-values-were-used
    internal static string FormatOptionalMetadataIsMissingForObservedValuesWereUsed(string pathText)
        => $"Optional metadata is missing for {pathText}; observed values were used.";

    // @OpenForgeText index.phrase.skipped-because-authored-metadata-is-malformed
    internal static string FormatSkippedBecauseAuthoredMetadataIsMalformed(string pathText)
        => $"Skipped {pathText} because its authored frontmatter metadata is malformed; other safe Entries sections may be updated.";
}
