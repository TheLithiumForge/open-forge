using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Update;

internal static class RouteUpdatePhrases
{
    internal static string FormatTheTemplateCouldNotBeVerifiedSafely(string templateText)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatTheTemplateCouldNotBeVerifiedSafely(templateText);

    internal static string FormatTheTemplateCouldNotBeRead(string templateText)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatTheTemplateCouldNotBeRead(templateText);

    // @OpenForgeText route.update.phrase.could-not-be-updated-nothing-was-changed
    internal static string FormatCouldNotBeUpdatedNothingWasChanged(string idText, string trimSentenceText)
        => $"{idText} could not be updated: {trimSentenceText}. Nothing was changed.";
}
