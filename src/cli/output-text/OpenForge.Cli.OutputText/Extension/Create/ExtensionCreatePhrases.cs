using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Create;

internal static class ExtensionCreatePhrases
{
    // @OpenForgeText extension.create.phrase.the-scaffold-could-not-be-created-nothing-was-changed
    internal static string FormatTheScaffoldCouldNotBeCreatedNothingWasChanged(string sentenceText)
        => $"The scaffold could not be created: {sentenceText}. Nothing was changed.";

    // @OpenForgeText extension.create.phrase.cannot-create-the-extension
    internal static string FormatCannotCreateTheExtension(string sentenceText)
        => $"Cannot create the Extension: {sentenceText}.";

    // @OpenForgeText extension.create.phrase.cannot-create-at
    internal static string FormatCannotCreateAt(string idText, string folderText, string sentenceText)
        => $"Cannot create {idText} at {folderText}: {sentenceText}.";

    // @OpenForgeText extension.create.phrase.dependencies
    internal static string FormatDependencies(string joinText)
        => $"dependencies: {joinText}";
}
