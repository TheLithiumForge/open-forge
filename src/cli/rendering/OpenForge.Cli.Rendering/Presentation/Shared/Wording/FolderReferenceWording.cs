namespace OpenForge.Cli.Core.Presentation.Shared.Wording;

internal static class FolderReferenceWording
{
    // Command boundaries project primitive display facts into shared presentation.
    internal static string Message(string destination, bool isMissing)
        => isMissing
            ? global::OpenForge.Cli.OutputText.Shared.SharedPhrases.LinkedFolderMissing(destination)
            : global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FolderHasNoHeadings(destination);
}
