using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Move;

internal static class RouteMovePhrases
{
    // @OpenForgeText route.move.phrase.moved-the-route-to
    internal static string FormatMovedTheRouteTo(string idText, string folderText, string countText, string pluralText)
        => $"Moved the route {idText} to {folderText}  ({countText} {pluralText})";

    // @OpenForgeText route.move.phrase.could-not-be-moved-nothing-was-changed
    internal static string FormatCouldNotBeMovedNothingWasChanged(string idText, string trimSentenceText)
        => $"{idText} could not be moved: {trimSentenceText}. Nothing was changed.";

    // @OpenForgeText route.move.phrase.cannot-move
    internal static string FormatCannotMove(string referenceText, string trimSentenceText)
        => $"Cannot move {referenceText}: {trimSentenceText}.";

    // @OpenForgeText route.move.phrase.would-move
    internal static string FormatWouldMove(string fromText, string toText)
        => $"Would move {fromText} -> {toText}";

    // @OpenForgeText route.move.phrase.rewrote-that-pointed-at-the
    internal static string FormatRewroteThatPointedAtThe(string countText, string pluralText, string valueText)
        => $"Rewrote {countText} {pluralText} that pointed at the {valueText}:";

    // @OpenForgeText route.move.phrase.scanned-found
    internal static string FormatScannedFound(string filesText, string pluralText, string occurrencesText, string pluralText2)
        => $"{filesText} {pluralText} scanned; {occurrencesText} {pluralText2} found.";

    // @OpenForgeText route.move.phrase.is-an-overwrite-file-move-its-base-file
    internal static string FormatIsAnOverwriteFileMoveItsBaseFile(string referenceText)
        => $"{referenceText} is an overwrite file; move its base file.";

    // @OpenForgeText route.move.phrase.is-the-loader-and-cannot-be-moved
    internal static string FormatIsTheLoaderAndCannotBeMoved(string referenceText)
        => $"{referenceText} is the Loader and cannot be moved.";

    // @OpenForgeText route.move.phrase.must-be-an-entrypoint-path-when-moving-a-route
    internal static string FormatMustBeAnEntrypointPathWhenMovingARoute(string targetText)
        => $"{targetText} must be an entrypoint path when moving a route.";

    // @OpenForgeText route.move.phrase.must-be-a-markdown-file-path-under-agents
    internal static string FormatMustBeAMarkdownFilePathUnderAgents(string targetText)
        => $"{targetText} must be a Markdown file path under .agents.";

    // @OpenForgeText route.move.phrase.contains-a-file-that-cannot-be-moved-safely
    internal static string FormatContainsAFileThatCannotBeMovedSafely(string folderText, string pathText, string trimSentenceText)
        => $"{folderText} contains a file that cannot be moved safely: {pathText} ({trimSentenceText}).";

    // @OpenForgeText route.move.reference.cannot-rewrite-location
    internal static string ReferenceCannotRewriteLocation(string pathText, string lineText, string columnText, string trimSentenceText)
        => $"The link at {pathText}:{lineText}:{columnText} cannot be rewritten safely: {trimSentenceText}.";

    // @OpenForgeText route.move.reference.cannot-rewrite-path
    internal static string ReferenceCannotRewritePath(string pathText, string trimSentenceText)
        => $"The link at {pathText} cannot be rewritten safely: {trimSentenceText}.";
}
