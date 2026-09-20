using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Remove;

internal static class RouteRemovePhrases
{
    // @OpenForgeText route.remove.phrase.some-files-under-could-not-be-listed-so-the-removal-was-not-planned
    internal static string FormatSomeFilesUnderCouldNotBeListedSoTheRemovalWasNotPlanned(string categoryFolderText)
        => $"Some files under {categoryFolderText} could not be listed, so the removal was not planned.";

    // @OpenForgeText route.remove.phrase.some-files-could-not-be-scanned-for-links-to-so-the-removal-was-not-planned
    internal static string FormatSomeFilesCouldNotBeScannedForLinksToSoTheRemovalWasNotPlanned(string valueText)
        => $"Some files could not be scanned for links to {valueText}, so the removal was not planned.";

    // @OpenForgeText route.remove.phrase.removed-the-route
    internal static string FormatRemovedTheRoute(string idText, string countText, string pluralText)
        => $"Removed the route {idText}  ({countText} {pluralText})";

    // @OpenForgeText route.remove.phrase.would-remove-the-route
    internal static string FormatWouldRemoveTheRoute(string idText, string countText, string pluralText)
        => $"Would remove the route {idText}  ({countText} {pluralText})";

    // @OpenForgeText route.remove.phrase.could-not-be-removed-nothing-was-changed
    internal static string FormatCouldNotBeRemovedNothingWasChanged(string idText, string sentenceText)
        => $"{idText} could not be removed: {sentenceText}. Nothing was changed.";

    // @OpenForgeText route.remove.phrase.detached-that-pointed-at-it-the-link-text-was-kept
    internal static string FormatDetachedThatPointedAtItTheLinkTextWasKept(string countText, string pluralText)
        => $"Detached {countText} {pluralText} that pointed at it; the link text was kept:";

    // @OpenForgeText route.remove.phrase.would-detach-that-pointed-at-it-the-link-text-would-be-kept
    internal static string FormatWouldDetachThatPointedAtItTheLinkTextWouldBeKept(string countText, string pluralText)
        => $"Would detach {countText} {pluralText} that pointed at it; the link text would be kept:";

    // @OpenForgeText route.remove.phrase.the-deleted-file-is-kept-in-a-recovery-bundle-at
    internal static string FormatTheDeletedFileIsKeptInARecoveryBundleAt(string pathText)
        => $"The deleted file is kept in a recovery bundle at {pathText}.";

    internal static string FormatTheDeletedFilesAreKeptInARecoveryBundleAt(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatTheDeletedFilesAreKeptInARecoveryBundleAt(pathText);

    // @OpenForgeText route.remove.phrase.scanned-and-found
    internal static string FormatScannedAndFound(string filesText, string pluralText, string occurrencesText, string pluralText2)
        => $"Scanned {filesText} {pluralText} and found {occurrencesText} {pluralText2}.";

    // @OpenForgeText route.remove.phrase.is-the-loader-and-cannot-be-removed
    internal static string FormatIsTheLoaderAndCannotBeRemoved(string referenceText)
        => $"{referenceText} is the Loader and cannot be removed.";

    // @OpenForgeText route.remove.phrase.is-an-overwrite-file-remove-its-base-file
    internal static string FormatIsAnOverwriteFileRemoveItsBaseFile(string referenceText)
        => $"{referenceText} is an overwrite file; remove its base file.";

    // @OpenForgeText route.remove.phrase.contains-a-file-that-cannot-be-removed-safely
    internal static string FormatContainsAFileThatCannotBeRemovedSafely(string folderText, string pathText, string sentenceText)
        => $"{folderText} contains a file that cannot be removed safely: {pathText} ({sentenceText}).";

    // @OpenForgeText route.remove.phrase.the-link-at-cannot-be-detached-safely
    internal static string FormatTheLinkAtCannotBeDetachedSafely(string pathText, string lineText, string columnText, string sentenceText)
        => $"The link at {pathText}:{lineText}:{columnText} cannot be detached safely: {sentenceText}.";
}
