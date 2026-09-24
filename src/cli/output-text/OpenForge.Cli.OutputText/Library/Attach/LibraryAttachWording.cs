using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Attach;

internal static class LibraryAttachWording
{
    // @OpenForgeText library.attach.wording.registered-the-library-from
    internal static string Registered(string id, string source)
        => $"Registered the {id} Library from {source}.";

    // @OpenForgeText library.attach.wording.registered-the-library-from-it-has-no-eligible-files-yet
    internal static string RegisteredEmpty(string id, string source)
        => $"Registered the {id} Library from {source}. It has no eligible files yet.";

    // @OpenForgeText library.attach.wording.would-register-the-library-from
    internal static string WouldRegister(string id, string source)
        => $"Would register the {id} Library from {source}.";

    // @OpenForgeText library.attach.wording.remove-library-id-from-removed-libraries-and-rerun
    internal static string RemoveExcludedLibraryNext(string id)
        => $"Remove Library ID '{id}' from removedLibraries in .agents/open-forge.json, then rerun open-forge library attach.";

    // @OpenForgeText library.attach.wording.library-attach-stopped-after-of-links-were-created
    internal static string Failed(int created, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Library attach stopped after {created} of {total} links were created.");

    // @OpenForgeText library.attach.wording.library-attach-was-cancelled-stopped-after-of-links-were-created
    internal static string CancelledAfter(int created, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Library attach was cancelled. Stopped after {created} of {total} links were created.");

    internal static string UpdatedEntries(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.EntriesUpdated(path);

    // @OpenForgeText library.attach.wording.would-update-the-entries-section-of
    internal static string WouldUpdateEntries(string path) => $"Would update the Entries section of {path}";

    internal static string SavedGrant(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.SavedGrant(path);

    internal static string WouldSaveGrant(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.WouldSaveGrant(path);

    // @OpenForgeText library.attach.wording.the-library-was-not-recorded-recovery-data
    internal static string NotRecordedRecovery(string path)
        => $"The Library was not recorded. Recovery data: {path}";

    // @OpenForgeText library.attach.wording.a-library-with-the-id-is-already-registered
    internal static string DuplicateId(string id)
        => $"A Library with the ID {id} is already registered.";

    // @OpenForgeText library.attach.wording.is-not-a-folder-inside-the-workspace
    internal static string SourceRootInvalid(string source)
        => $"{source} is not a folder inside the workspace.";

    // @OpenForgeText library.attach.wording.cannot-be-read
    internal static string SourceRootUnavailable(string source)
        => $"{source} cannot be read.";

    // @OpenForgeText library.attach.wording.resolves-to-an-unsafe-location
    internal static string SourceRootBlocked(string source)
        => $"{source} resolves to an unsafe location.";

    // @OpenForgeText library.attach.wording.to-must-be-a-folder-inside-the-workspace
    internal static string DestinationRootInvalid(string value)
        => $"--to {value} must be a folder inside the workspace.";

    internal static string DestinationCollision(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.DestinationCollision(path);

    // @OpenForgeText library.attach.wording.some-files-under-could-not-be-listed
    internal static string InventoryIncomplete(string source)
        => $"Some files under {source} could not be listed.";

    internal static string ConsumerBlocked(string path)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.ConsumerBlocked(path);

    // @OpenForgeText library.attach.wording.is-outside-agents-and-no-grant-allows-writing-there
    internal static string PermissionRequired(string folder)
        => $"{folder} is outside .agents and no grant allows writing there.";

    // @OpenForgeText library.attach.wording.creating-the-link-failed-stopped-after-of-links
    internal static string ApplicationFailed(string path, int created, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Creating the link {path} failed. Stopped after {created} of {total} links.");
}
