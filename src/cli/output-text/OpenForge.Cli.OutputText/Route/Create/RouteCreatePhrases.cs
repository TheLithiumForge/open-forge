using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Create;

internal static class RouteCreatePhrases
{
    // @OpenForgeText route.create.phrase.the-file-could-not-be-created-nothing-was-changed
    internal static string FormatTheFileCouldNotBeCreatedNothingWasChanged(string trimSentenceText)
        => $"The file could not be created: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText route.create.phrase.cannot-create-the-routed-file
    internal static string FormatCannotCreateTheRoutedFile(string trimSentenceText)
        => $"Cannot create the routed file: {trimSentenceText}.";

    // @OpenForgeText route.create.phrase.cannot-create
    internal static string FormatCannotCreate(string pathText, string trimSentenceText)
        => $"Cannot create {pathText}: {trimSentenceText}.";

    // @OpenForgeText route.create.phrase.is-an-entrypoint-a-folder-or-an-overwrite-file-route-create-makes-ordinary-files
    internal static string FormatIsAnEntrypointAFolderOrAnOverwriteFileRouteCreateMakesOrdinaryFiles(string targetText)
        => $"{targetText} is an entrypoint, a folder or an overwrite file; route create makes ordinary files.";

    // @OpenForgeText route.create.phrase.must-be-a-markdown-file-below-an-existing-route
    internal static string FormatMustBeAMarkdownFileBelowAnExistingRoute(string targetText)
        => $"{targetText} must be a Markdown file below an existing route.";
}
