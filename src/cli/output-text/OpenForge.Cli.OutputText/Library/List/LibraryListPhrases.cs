using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.List;

internal static class LibraryListPhrases
{
    // @OpenForgeText library.list.phrase.registered
    internal static string FormatRegistered(string countText, string pluralText)
        => $"{countText} {pluralText} registered.";

    // @OpenForgeText library.list.phrase.registered-attention
    internal static string FormatRegisteredAttention(string librariesText, string libraryWordText, string linksText, string linkWordText, string verbText)
        => $"{librariesText} {libraryWordText} registered. {linksText} {linkWordText} {verbText} attention.";

    internal static string FormatAgentsOpenForgeLockJsonIsInvalid(string reasonText)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.LifecycleBlocked(reasonText);

    // @OpenForgeText library.list.phrase.cannot-list-libraries
    internal static string FormatCannotListLibraries(string reasonText)
        => $"Cannot list Libraries: {reasonText}.";

    // @OpenForgeText library.list.phrase.current
    internal static string FormatCurrent(string currentText, string pluralText)
        => $"{currentText} {pluralText} current";

    // @OpenForgeText library.list.phrase.missing
    internal static string FormatMissing(string missingText)
        => $"{missingText} missing";

    // @OpenForgeText library.list.phrase.changed
    internal static string FormatChanged(string changedText)
        => $"{changedText} changed";

    // @OpenForgeText library.list.phrase.unavailable
    internal static string FormatUnavailable(string unavailableText)
        => $"{unavailableText} unavailable";

    // @OpenForgeText library.list.phrase.source-id
    internal static string FormatSourceId(string valueText)
        => $"Source ID: {valueText}";
}
