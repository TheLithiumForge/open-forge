using System.Globalization;

namespace OpenForge.Cli.OutputText.Update;

internal static class UpdatePhrases
{
    // @OpenForgeText update.framework.updated
    internal static string FrameworkUpdated(string countText, string pluralText)
        => $"Updated {countText} Framework {pluralText}.";

    // @OpenForgeText update.phrase.would-update-framework
    internal static string FormatWouldUpdateFramework(string countText, string pluralText)
        => $"Would update {countText} Framework {pluralText}.";

    // @OpenForgeText update.phrase.the-framework-is-up-to-date-but
    internal static string FormatTheFrameworkIsUpToDateBut(string keptCountText)
        => $"The Framework is up to date, but {keptCountText}.";

    // @OpenForgeText update.framework.updated-with-kept
    internal static string FrameworkUpdatedWithKept(string updatedText, string pluralText, string keptCountText)
        => $"Updated {updatedText} Framework {pluralText}. {keptCountText}.";

    // @OpenForgeText update.phrase.update-could-not-start-nothing-was-changed
    internal static string FormatUpdateCouldNotStartNothingWasChanged(string trimSentenceText)
        => $"Update could not start: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText update.phrase.is-no-longer-part-of-this-release-but-cannot-be-deleted-safely
    internal static string FormatIsNoLongerPartOfThisReleaseButCannotBeDeletedSafely(string pathText, string trimSentenceText)
        => $"{pathText} is no longer part of this release but cannot be deleted safely: {trimSentenceText}.";

    // @OpenForgeText update.phrase.prevents-the-update
    internal static string FormatPreventsTheUpdate(string pathText, string trimSentenceText)
        => $"{pathText} prevents the update: {trimSentenceText}.";

    // @OpenForgeText update.phrase.could-not-be-written
    internal static string FormatCouldNotBeWritten(string pathText, string trimSentenceText)
        => $"{pathText} could not be written: {trimSentenceText}.";

    // @OpenForgeText update.phrase.bundled-framework-id-version-fingerprint
    internal static string FormatBundledFrameworkIdVersionFingerprint(string pathText, string idText, string valueText, string fingerprintText)
        => $"{pathText} bundled Framework: id={idText}; version={valueText}; fingerprint={fingerprintText}";

    // @OpenForgeText update.phrase.from-an-earlier-version-kept
    internal static string FormatFromAnEarlierVersionKept(string countText, string pluralText, string valueText)
        => $"{countText} {pluralText} from an earlier version {valueText} kept";
}
