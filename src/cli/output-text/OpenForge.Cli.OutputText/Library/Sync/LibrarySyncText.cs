using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Sync;

internal static class LibrarySyncText
{
    // @OpenForgeText library.sync.message.library-sync-was-cancelled-nothing-was-changed
    internal static string MessageLibrarySyncWasCancelledNothingWasChanged()
        => "Library sync was cancelled. Nothing was changed.";

    // @OpenForgeText library.sync.message.settings-would-be-updated
    internal static string MessageSettingsWouldBeUpdated()
        => ".agents/open-forge.json would be updated.";

    // @OpenForgeText library.sync.message.settings-were-updated
    internal static string MessageSettingsWereUpdated()
        => ".agents/open-forge.json was updated.";

    // @OpenForgeText library.sync.message.settings-were-not-updated
    internal static string MessageSettingsWereNotUpdated()
        => ".agents/open-forge.json was not updated.";

    // @OpenForgeText library.sync.message.settings-update-failed
    internal static string MessageSettingsUpdateFailed()
        => ".agents/open-forge.json update failed.";

    // @OpenForgeText library.sync.message.settings-update-final-state-is-unknown
    internal static string MessageSettingsUpdateFinalStateIsUnknown()
        => ".agents/open-forge.json final state is unknown.";

    // @OpenForgeText library.sync.help.syntax
    internal static string HelpSyntax()
        => "open-forge library sync <library-id> [--dry-run] [--automatic] [--allow-path <path>] [global options]";

    // @OpenForgeText library.sync.help.write-policy
    internal static string HelpWritePolicy()
        => "Reconcile a registered Library with its complete source inventory. Use --dry-run to preview all changes without writing, or --automatic for a non-interactive apply.";

    // @OpenForgeText library.sync.help.examples
    internal static string HelpExamples()
        => "open-forge library sync shared --dry-run\n  open-forge library sync shared\n  open-forge library sync shared --automatic";

    // @OpenForgeText library.sync.message.inspect-the-destination-link-before-synchronizing-it
    internal static string MessageInspectTheDestinationLinkBeforeSynchronizingIt()
        => "Inspect the destination link before synchronizing it.";

    // @OpenForgeText library.sync.label.its-source-file-is-gone
    internal static string LabelItsSourceFileIsGone()
        => "its source file is gone";

    // @OpenForgeText library.sync.label.links-added
    internal static string LabelLinksAdded()
        => "links added";

    // @OpenForgeText library.sync.label.links-unchanged
    internal static string LabelLinksUnchanged()
        => "links unchanged";

    // @OpenForgeText library.sync.label.would-be-updated
    internal static string LabelWouldBeUpdated()
        => "would be updated";

    // @OpenForgeText library.sync.message.no-ownership-record-exists
    internal static string MessageNoOwnershipRecordExists()
        => "No ownership record exists.";

    // @OpenForgeText library.sync.label.required-library-facts-are-unavailable
    internal static string LabelRequiredLibraryFactsAreUnavailable()
        => "required Library facts are unavailable";

    // @OpenForgeText library.sync.label.the-synchronization-is-blocked
    internal static string LabelTheSynchronizationIsBlocked()
        => "the synchronization is blocked";

    // @OpenForgeText library.sync.wording.remove-library-id-from-removed-libraries-and-rerun
    internal static string RemoveExcludedLibraryNext(string id)
        => $"Remove Library ID '{id}' from removedLibraries in .agents/open-forge.json, then rerun open-forge library sync.";

    // @OpenForgeText library.sync.title.add
    internal static string TitleAdd()
        => "Add";

    // @OpenForgeText library.sync.title.remove
    internal static string TitleRemove()
        => "Remove";

    // @OpenForgeText library.sync.title.added
    internal static string TitleAdded()
        => "Added";

    // @OpenForgeText library.sync.title.updated
    internal static string TitleUpdated()
        => "Updated";

    // @OpenForgeText library.sync.title.permission-data-is-invalid
    internal static string TitlePermissionDataIsInvalid()
        => "Permission data is invalid";

    // @OpenForgeText library.sync.title.permission-data-is-unavailable
    internal static string TitlePermissionDataIsUnavailable()
        => "Permission data is unavailable";

    // @OpenForgeText library.sync.title.permission-data-changed
    internal static string TitlePermissionDataChanged()
        => "Permission data changed";

    // @OpenForgeText library.sync.title.permission-write-failed
    internal static string TitlePermissionWriteFailed()
        => "Permission write failed";

    // @OpenForgeText library.sync.title.consumer-path-is-managed
    internal static string TitleConsumerPathIsManaged()
        => "Consumer path is managed";

    // @OpenForgeText library.sync.title.library-mapping-is-blocked
    internal static string TitleLibraryMappingIsBlocked()
        => "Library mapping is blocked";

    // @OpenForgeText library.sync.title.library-mapping-is-unavailable
    internal static string TitleLibraryMappingIsUnavailable()
        => "Library mapping is unavailable";

    // @OpenForgeText library.sync.title.library-ownership-conflicts
    internal static string TitleLibraryOwnershipConflicts()
        => "Library ownership conflicts";

    // @OpenForgeText library.sync.title.entries-navigation-is-unavailable
    internal static string TitleEntriesNavigationIsUnavailable()
        => "Entries navigation is unavailable";

    // @OpenForgeText library.sync.title.entries-navigation-is-unsafe
    internal static string TitleEntriesNavigationIsUnsafe()
        => "Entries navigation is unsafe";

    // @OpenForgeText library.sync.title.library-sync-could-not-write
    internal static string TitleLibrarySyncCouldNotWrite()
        => "Library sync could not write";

    // @OpenForgeText library.sync.title.library-sync-verification-failed
    internal static string TitleLibrarySyncVerificationFailed()
        => "Library sync verification failed";

    // @OpenForgeText library.sync.title.library-sync-failed
    internal static string TitleLibrarySyncFailed()
        => "Library sync failed";

    // @OpenForgeText library.sync.title.library-sync-was-cancelled
    internal static string TitleLibrarySyncWasCancelled()
        => "Library sync was cancelled";

    // @OpenForgeText library.sync.title.destination-is-occupied
    internal static string TitleDestinationIsOccupied()
        => "Destination is occupied";

    // @OpenForgeText library.sync.title.registered-link-was-restored
    internal static string TitleRegisteredLinkWasRestored()
        => "Registered link was restored";
}
