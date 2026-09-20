namespace OpenForge.Cli.OutputText.Extension.Shared;

internal static class CanonicalPhrases
{
    // @OpenForgeText extension.shared.wording.the-source-could-not-be-read
    internal static string SourceUnreadable(string path)
        => $"The source {path} could not be read.";

    // @OpenForgeText extension.shared.wording.source
    internal static string Source(string path)
        => $"Source: {path}";

    // @OpenForgeText extension.shared.phrase.is-inside-the-workspace-and-cannot-be-used-as-a-source
    internal static string FormatIsInsideTheWorkspaceAndCannotBeUsedAsASource(string pathText)
        => $"{pathText} is inside the workspace and cannot be used as a source.";
}
