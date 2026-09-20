using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Inspect;

internal static class LibraryInspectPhrases
{
    // @OpenForgeText library.inspect.phrase.is-current-from-linked-under
    internal static string FormatIsCurrentFromLinkedUnder(string idText, string filesText, string pluralText, string sourceText, string valueText, string destinationText)
        => $"{idText} is current: {filesText} {pluralText} from {sourceText} {valueText} linked under {destinationText}.";

    // @OpenForgeText library.inspect.phrase.needs-a-sync-between-and
    internal static string FormatNeedsASyncBetweenAnd(string idText, string filesText, string pluralText, string valueText, string sourceText, string destinationText)
        => $"{idText} needs a sync: {filesText} {pluralText} {valueText} between {sourceText} and {destinationText}.";

    // @OpenForgeText library.inspect.phrase.could-not-be-inspected-completely
    internal static string FormatCouldNotBeInspectedCompletely(string idText, string reasonText)
        => $"{idText} could not be inspected completely: {reasonText}.";

    // @OpenForgeText library.inspect.phrase.source-inventory
    internal static string FormatSourceInventory(string eligibleText, string pluralText, string excludedText, string pluralText2)
        => $"Source inventory: {eligibleText} {pluralText}, {excludedText} {pluralText2}.";
}
