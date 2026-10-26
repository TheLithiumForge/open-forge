using System.Globalization;

namespace OpenForge.Cli.OutputText.Cleanup;

internal static class CleanupWording
{
    // @OpenForgeText cleanup.wording.store-validation-unavailable
    internal static string StoreValidationUnavailable(string path, string cause)
        => $"Recovery data at {path} could not be validated for deletion: {cause}.";

    // @OpenForgeText cleanup.wording.partial-stores
    internal static string Partial(int removed, int total, bool preview)
        => preview
            ? $"Would remove {removed} of {total} observed items. Other recovery stores could not be cleaned safely."
            : $"Removed {removed} of {total} observed items. Other recovery stores could not be cleaned safely.";

    // @OpenForgeText cleanup.wording.cleanup-stopped-after-removing-of-items
    internal static string Failed(int removed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Cleanup stopped after removing {removed} of {total} items.");
}
