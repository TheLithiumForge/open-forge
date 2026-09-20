using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Remove;

internal static class ExtensionRemovePhrases
{
    // @OpenForgeText extension.remove.phrase.owners-before-owners-after
    internal static string FormatOwnersBeforeOwnersAfter(string ownersText, string ownersText2)
        => $"owners before: {ownersText}; owners after: {ownersText2}";

    // @OpenForgeText extension.remove.phrase.removal-order
    internal static string FormatRemovalOrder(string joinText)
        => $"Removal order: {joinText}";

    // @OpenForgeText extension.remove.phrase.removed-the-extension
    internal static string FormatRemovedTheExtension(string valueText)
        => $"Removed the {valueText} Extension.";

    // @OpenForgeText extension.remove.phrase.removed-extensions
    internal static string FormatRemovedExtensions(string countText, string joinText)
        => $"Removed {countText} Extensions: {joinText}.";

    // @OpenForgeText extension.remove.phrase.the-extension-could-not-be-removed-nothing-was-changed
    internal static string FormatTheExtensionCouldNotBeRemovedNothingWasChanged(string idText, string sentenceText)
        => $"The {idText} Extension could not be removed: {sentenceText}. Nothing was changed.";

    // @OpenForgeText extension.remove.phrase.cannot-remove
    internal static string FormatCannotRemove(string sentenceText)
        => $"Cannot remove: {sentenceText}.";

    // @OpenForgeText extension.remove.phrase.ownership-record-unavailable
    internal static string FormatOwnershipRecordUnavailable(string causeText)
        => $"The ownership record is unavailable: {causeText}";

    // @OpenForgeText extension.remove.phrase.the-extension-still-needs
    internal static string FormatTheExtensionStillNeeds(string dependentText, string dependencyText)
        => $"The {dependentText} Extension still needs {dependencyText}.";

    // @OpenForgeText extension.remove.phrase.kept-still-owned-by
    internal static string FormatKeptStillOwnedBy(string joinText)
        => $"kept; still owned by {joinText}";

    // @OpenForgeText extension.remove.phrase.remains-installed-and-is-no-longer-needed-by
    internal static string FormatRemainsInstalledAndIsNoLongerNeededBy(string dependencyText, string idText)
        => $"{dependencyText} remains installed and is no longer needed by {idText}.";
}
