using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Sync;

internal static class LibrarySyncWording
{
    // @OpenForgeText library.sync.wording.the-library-is-up-to-date-nothing-to-do
    internal static string UpToDate(string id)
        => $"The {id} Library is up to date. Nothing to do.";

    // @OpenForgeText library.sync.wording.no-ownership-record-exists-so-cannot-be-synchronized-nothing-was-changed
    internal static string NoOwnership(string id)
        => $"No ownership record exists, so {id} cannot be synchronized. Nothing was changed.";

    // @OpenForgeText library.sync.wording.library-sync-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Library sync stopped after {completed} of {total} changes.");

    // @OpenForgeText library.sync.wording.library-sync-was-cancelled-stopped-after-of-changes
    internal static string CancelledAfter(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Library sync was cancelled. Stopped after {completed} of {total} changes.");

    internal static string SourceRootInvalid(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.SourceRootInvalid(path);

    // @OpenForgeText library.sync.wording.the-source-folder-cannot-be-read-so-nothing-was-changed
    internal static string SourceRootUnavailable(string path)
        => $"The source folder {path} cannot be read, so nothing was changed.";

    internal static string SourceRootBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.SourceRootBlocked(path);

    // @OpenForgeText library.sync.wording.some-files-under-could-not-be-listed-so-nothing-was-changed
    internal static string InventoryIncomplete(string source)
        => $"Some files under {source} could not be listed, so nothing was changed.";

    internal static string MappingBlocked(string path, string occupant)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.MappingBlocked(path, occupant);

    internal static string DestinationCollision(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.DestinationCollision(path);

    internal static string RetiredLinkMissing(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.RetiredLinkMissing(path);

    // @OpenForgeText library.sync.wording.was-registered-but-missing-so-it-was-restored
    internal static string RegisteredLinkRestored(string path)
        => $"{path} was registered but missing, so it was restored.";

    internal static string ConsumerBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.ConsumerBlocked(path);

    // @OpenForgeText library.sync.wording.is-no-longer-the-link-the-library-created
    internal static string ChangedDestination(string path)
        => $"{path} is no longer the link the Library created.";

    // @OpenForgeText library.sync.wording.it-is-now-move-it-away-or-restore-the-link-then-rerun
    internal static string ChangedDestinationResolution(string occupant)
        => $"It is now {occupant}. Move it away or restore the link, then rerun.";

    internal static string Target(string value) => global::OpenForge.Cli.OutputText.Library.Sync.LibrarySyncPhrases.FormatTarget(value);

    // @OpenForgeText library.sync.wording.agents-open-forge-lock-json
    internal static string Lock(string action)
        => $".agents/open-forge.lock.json  {action}";

    internal static string EntriesUpdated(string path)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.EntriesUpdated(path);
}
