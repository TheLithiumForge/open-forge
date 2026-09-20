using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Inspect;

internal static class RouteInspectPhrases
{
    // @OpenForgeText route.inspect.phrase.compatibility-name-the-canonical-name-is-md
    internal static string FormatCompatibilityNameTheCanonicalNameIsMd(string nameText, string folderText)
        => $"{nameText} (compatibility name; the canonical name is _{folderText}.md)";

    // @OpenForgeText route.inspect.phrase.about-tokens
    internal static string FormatAboutTokens(string estimatedTokensText)
        => $"about {estimatedTokensText} tokens";

    internal static string FormatCannotUseAsTheWorkspaceItDoesNotExistOrCannotBeRead(string pathText)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WorkspaceUnavailable(pathText);

    internal static string FormatCannotUseAsTheWorkspaceItsLocationCouldNotBeVerified(string subjectText, string trimSentenceText)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.WorkspaceUnsafe(subjectText, trimSentenceText);

    // @OpenForgeText route.inspect.phrase.is-read
    internal static string FormatIsRead(string relatedSourceIdText)
        => $"{relatedSourceIdText} is read";

    // @OpenForgeText route.inspect.phrase.read-automatically-when
    internal static string FormatReadAutomaticallyWhen(string automaticExplanationText)
        => $"Read automatically when {automaticExplanationText}";

    // @OpenForgeText route.inspect.reading.final-alternative
    internal static string ReadingFinalAlternative(string valueText)
        => $", or {valueText}";

    // @OpenForgeText route.inspect.phrase.has-no-beside-it
    internal static string FormatHasNoBesideIt(string fileNameText, string baseNameText)
        => $"{fileNameText} has no {baseNameText} beside it.";

    // @OpenForgeText route.inspect.phrase.the-route-chain-above-could-not-be-established-completely
    internal static string FormatTheRouteChainAboveCouldNotBeEstablishedCompletely(string pathText, string trimSentenceText)
        => $"The route chain above {pathText} could not be established completely: {trimSentenceText}.";

    // @OpenForgeText route.inspect.phrase.could-not-be-measured
    internal static string FormatCouldNotBeMeasured(string factText, string trimSentenceText)
        => $"{factText} could not be measured: {trimSentenceText}.";

    // @OpenForgeText route.inspect.phrase.route-inspect-stopped-because-of-an-unexpected-error
    internal static string FormatRouteInspectStoppedBecauseOfAnUnexpectedError(string trimSentenceText)
        => $"Route inspect stopped because of an unexpected error: {trimSentenceText}.";

    // @OpenForgeText route.inspect.phrase.ki-b
    internal static string FormatKiB(string valueText)
        => $"{valueText} KiB";

    // @OpenForgeText route.inspect.phrase.mi-b
    internal static string FormatMiB(string valueText)
        => $"{valueText} MiB";

    // @OpenForgeText route.inspect.reading.alternatives
    internal static string ReadingAlternatives(string joinText, string valueText)
        => $"{joinText}, or {valueText}";
}
