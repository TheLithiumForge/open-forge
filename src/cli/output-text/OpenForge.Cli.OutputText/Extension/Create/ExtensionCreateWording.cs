using System.Globalization;

namespace OpenForge.Cli.OutputText.Extension.Create;

internal static class ExtensionCreateWording
{
    // @OpenForgeText extension.create.wording.is-not-a-valid-id-use-lowercase-letters-digits-and-hyphens
    internal static string InvalidExtensionId(string value)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"'{value}' is not a valid ID. Use lowercase letters, digits and hyphens.");

    // @OpenForgeText extension.create.wording.created-the-extension-scaffold-at
    internal static string Created(string id, string folder)
        => $"Created the {id} Extension scaffold at {folder}";

    // @OpenForgeText extension.create.wording.would-create-the-extension-scaffold-at
    internal static string WouldCreate(string id, string folder)
        => $"Would create the {id} Extension scaffold at {folder}";

    // @OpenForgeText extension.create.wording.the-scaffold-at-already-matches-nothing-to-do
    internal static string AlreadyMatches(string id, string folder)
        => $"The {id} scaffold at {folder} already matches. Nothing to do.";

    // @OpenForgeText extension.create.wording.extension-create-stopped-after-of-files
    internal static string Failed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Extension create stopped after {completed} of {total} files.");

    internal static string WouldCreateEffect(string path) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FormatWouldCreate(path);

    // @OpenForgeText extension.create.wording.name
    internal static string ManifestName(string value) => $"name: {value}";

    internal static string ManifestDescription(string value) => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.MetadataDescription(value);

    // @OpenForgeText extension.create.wording.version
    internal static string ManifestVersion(string value) => $"version: {value}";

    // @OpenForgeText extension.create.wording.could-not-be-read
    internal static string CatalogueUnavailable(string folder)
        => $"{folder} could not be read.";

    // @OpenForgeText extension.create.wording.cannot-be-used
    internal static string CatalogueUnsafe(string folder, string reason)
        => $"{folder} cannot be used: {reason}.";

    internal static string DestinationCollision(string destination)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.TargetContentDiffers(destination);

    internal static string DestinationChanged(string destination)
        => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.TargetChanged(destination);

    // @OpenForgeText extension.create.wording.extension-create-stopped-after-of-files-created-files-were-left-in-place
    internal static string ApplicationFailed(int completed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Extension create stopped after {completed} of {total} files. Created files were left in place.");

    // @OpenForgeText extension.create.wording.could-not-be-verified-after-it-was-written-no-recovery-bundle-was-created
    internal static string VerificationFailed(string destination)
        => $"{destination} could not be verified after it was written. No recovery bundle was created.";
}
