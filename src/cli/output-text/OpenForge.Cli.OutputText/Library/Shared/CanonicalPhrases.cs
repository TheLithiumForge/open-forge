namespace OpenForge.Cli.OutputText.Library.Shared;

internal static class CanonicalPhrases
{
    // @OpenForgeText library.shared.wording.already-exists-and-is-not-a-link-this-library-would-create
    internal static string DestinationCollision(string path)
        => $"{path} already exists and is not a link this Library would create.";

    // @OpenForgeText library.shared.wording.is-managed-by-open-forge-and-cannot-receive-library-links
    internal static string ConsumerBlocked(string path)
        => $"{path} is managed by Open Forge and cannot receive Library links.";

    // @OpenForgeText library.shared.phrase.is-not-a-valid-library-id
    internal static string FormatIsNotAValidLibraryId(string isNullOrWhiteSpaceText)
        => $"{isNullOrWhiteSpaceText} is not a valid Library ID.";

    // @OpenForgeText library.shared.wording.is-registered-but-no-longer-exists-so-its-removal-cannot-be-confirmed
    internal static string RetiredLinkMissing(string path)
        => $"{path} is registered but no longer exists, so its removal cannot be confirmed.";

    // @OpenForgeText library.shared.wording.is-and-is-not-the-link-the-library-created
    internal static string MappingBlocked(string path, string occupant)
        => $"{path} is {occupant} and is not the link the Library created.";

    // @OpenForgeText library.shared.wording.the-source-folder-is-not-a-folder-inside-the-workspace
    internal static string SourceRootInvalid(string path)
        => $"The source folder {path} is not a folder inside the workspace.";

    // @OpenForgeText library.shared.wording.the-source-folder-resolves-to-an-unsafe-location
    internal static string SourceRootBlocked(string path)
        => $"The source folder {path} resolves to an unsafe location.";

    // @OpenForgeText library.shared.wording.missing
    internal static string LinkMissing(string path)
        => $"{path}  missing";

    // @OpenForgeText library.shared.wording.is-no-longer-the-link-created
    internal static string LinkChanged(string path, string id)
        => $"{path}  is no longer the link {id} created";
}
