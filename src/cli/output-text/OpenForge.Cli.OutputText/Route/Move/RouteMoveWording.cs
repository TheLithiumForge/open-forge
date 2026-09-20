using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Move;

internal static class RouteMoveWording
{
    // @OpenForgeText route.move.wording.moved-to
    internal static string MovedFile(string id, string path)
        => $"Moved {id} to {path}";

    // @OpenForgeText route.move.wording.would-move-to
    internal static string WouldMove(string id, string path)
        => $"Would move {id} to {path}";

    // @OpenForgeText route.move.wording.route-move-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Route move stopped after {completed} of {total} changes.");

    internal static string EntryUpdated(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntryUpdated(path);

    internal static string EntryRemoved(string path)
        => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntryRemoved(path);

    // @OpenForgeText route.move.wording.entry-added-to
    internal static string EntryAdded(string path)
        => $"Entry added to {path}";

    internal static string Before(string value) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.BeforeHash(value);

    internal static string After(string value) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.AfterHash(value);

    internal static string InvalidSource(string reference)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FilterNotASource(reference);

    // @OpenForgeText route.move.wording.already-exists
    internal static string DestinationOccupied(string path)
        => $"{path} already exists.";

    // @OpenForgeText route.move.wording.does-not-exist-or-has-no-entrypoint-so-the-moved-file-could-not-be-listed
    internal static string DestinationParentMissing(string folder)
        => $"{folder} does not exist or has no entrypoint, so the moved file could not be listed.";

    // @OpenForgeText route.move.wording.overwrite-md-could-belong-to-more-than-one-base-file
    internal static string OverwriteAmbiguous(string name)
        => $"{name}.overwrite.md could belong to more than one base file.";

    // @OpenForgeText route.move.wording.some-files-under-could-not-be-listed-so-the-move-was-not-planned
    internal static string CategoryInventoryIncomplete(string folder)
        => $"Some files under {folder} could not be listed, so the move was not planned.";

    // @OpenForgeText route.move.wording.some-files-could-not-be-scanned-for-links-to-so-the-move-was-not-planned
    internal static string ReferenceCoverageIncomplete(string path)
        => $"Some files could not be scanned for links to {path}, so the move was not planned.";
}
