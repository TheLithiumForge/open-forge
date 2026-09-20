using System.Globalization;

namespace OpenForge.Cli.OutputText.References;

internal static class ReferencesPhrases
{
    // @OpenForgeText references.phrase.has-no-authored-links-in-or-out
    internal static string FormatHasNoAuthoredLinksInOrOut(string pathText, string entriesNotCountedText)
        => $"{pathText} has no authored links in or out. {entriesNotCountedText}";

    // @OpenForgeText references.phrase.has-no-authored-links-in
    internal static string FormatHasNoAuthoredLinksIn(string pathText, string entriesNotCountedText)
        => $"{pathText} has no authored links in. {entriesNotCountedText}";

    // @OpenForgeText references.phrase.has-no-authored-links-out
    internal static string FormatHasNoAuthoredLinksOut(string pathText, string entriesNotCountedText)
        => $"{pathText} has no authored links out. {entriesNotCountedText}";

    // @OpenForgeText references.phrase.cannot-list-references
    internal static string FormatCannotListReferences(string reasonText)
        => $"Cannot list references: {reasonText}.";

    // @OpenForgeText references.phrase.the-physical-alias-involving-could-not-be-described-because-one-of-its-paths-is-unavailable
    internal static string FormatThePhysicalAliasInvolvingCouldNotBeDescribedBecauseOneOfItsPathsIsUnavailable(string valueText)
        => $"The physical alias involving {valueText} could not be described because one of its paths is unavailable.";

    // @OpenForgeText references.phrase.and-resolve-to-the-same-physical-file
    internal static string FormatAndResolveToTheSamePhysicalFile(string valueText, string valueText2)
        => $"{valueText} and {valueText2} resolve to the same physical file.";

    // @OpenForgeText references.phrase.the-identity-of-could-not-be-determined
    internal static string FormatTheIdentityOfCouldNotBeDetermined(string sourcePathText)
        => $"The identity of {sourcePathText} could not be determined.";

    // @OpenForgeText references.phrase.the-incoming-scan-used-only
    internal static string FormatTheIncomingScanUsedOnly(string joinText)
        => $"The incoming scan used only {joinText}.";

    // @OpenForgeText references.phrase.the-incoming-scan-skipped
    internal static string FormatTheIncomingScanSkipped(string joinText)
        => $"The incoming scan skipped {joinText}.";

    // @OpenForgeText references.phrase.the-incoming-scan-used-only-and-skipped
    internal static string FormatTheIncomingScanUsedOnlyAndSkipped(string joinText, string joinText2)
        => $"The incoming scan used only {joinText} and skipped {joinText2}.";

    // @OpenForgeText references.phrase.scanned-for-incoming-links
    internal static string FormatScannedForIncomingLinks(string countText, string pluralText)
        => $"Scanned {countText} {pluralText} for incoming links.";
}
