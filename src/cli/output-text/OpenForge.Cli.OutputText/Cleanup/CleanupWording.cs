using System.Globalization;

namespace OpenForge.Cli.OutputText.Cleanup;

internal static class CleanupWording
{
    // @OpenForgeText cleanup.wording.cleanup-stopped-after-removing-of-items
    internal static string Failed(int removed, int total)
        => string.Create(
            CultureInfo.InvariantCulture,
            $"Cleanup stopped after removing {removed} of {total} items.");
}
