using System.Globalization;

namespace OpenForge.Cli.OutputText.Library.Detach;

internal static class LibraryDetachWording
{
    // @OpenForgeText library.detach.wording.detached-it-had-no-links
    internal static string DetachedNoLinks(string id)
        => $"Detached {id}. It had no links.";

    // @OpenForgeText library.detach.wording.no-ownership-record-exists-so-cannot-be-detached-nothing-was-changed
    internal static string NoOwnership(string id)
        => $"No ownership record exists, so {id} cannot be detached. Nothing was changed.";

    // @OpenForgeText library.detach.wording.library-detach-stopped-after-of-links-were-removed
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture,
            $"Library detach stopped after {completed} of {total} links were removed.");

    internal static string InvalidId(string value)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.FormatIsNotAValidLibraryId(value);

    // @OpenForgeText library.detach.wording.no-ownership-record-exists-so-cannot-be-detached
    internal static string OwnershipObservation(string id)
        => $"No ownership record exists, so {id} cannot be detached.";

    // @OpenForgeText library.detach.wording.registered-link-is-already-absent
    internal static string RegisteredLinkMissing(string path)
        => $"{path} is already absent, so no file was removed.";

    internal static string MappingBlocked(string path, string kind)
        => global::OpenForge.Cli.OutputText.Library.Shared.CanonicalPhrases.MappingBlocked(path, kind);

    // @OpenForgeText library.detach.wording.changed-ordinary-destination-was-kept
    internal static string RetainedDestination(string path)
        => $"{path} is an ordinary file, so its bytes were kept.";

    // @OpenForgeText library.detach.wording.is-protected-owned-by-the-source-or-registered-to-another-library
    internal static string DestinationProtected(string path)
        => $"{path} is protected, owned by the source, or registered to another Library.";

    // @OpenForgeText library.detach.wording.could-not-be-checked-so-nothing-was-changed
    internal static string MappingUnavailable(string path)
        => $"{path} could not be checked, so nothing was changed.";

    // @OpenForgeText library.detach.wording.is-managed-by-open-forge-and-cannot-be-changed-by-detach
    internal static string ConsumerBlocked(string path)
        => $"{path} is managed by Open Forge and cannot be changed by detach.";
}
