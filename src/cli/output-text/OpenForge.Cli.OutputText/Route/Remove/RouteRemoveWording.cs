using System.Globalization;

namespace OpenForge.Cli.OutputText.Route.Remove;

internal static class RouteRemoveWording
{
    // @OpenForgeText route.remove.wording.removed
    internal static string Removed(string path) => $"Removed {path}";

    // @OpenForgeText route.remove.wording.would-remove
    internal static string WouldRemove(string path) => $"Would remove {path}";

    // @OpenForgeText route.remove.wording.route-remove-stopped-after-of-changes
    internal static string Failed(int completed, int total)
        => string.Create(CultureInfo.InvariantCulture, $"Route remove stopped after {completed} of {total} changes.");

    internal static string EntryRemoved(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.EntryRemoved(path);

    // @OpenForgeText route.remove.wording.would-remove-entry-from
    internal static string WouldEntryRemoved(string path) => $"Would remove entry from {path}";

    internal static string NotStarted(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatNotStarted(path);

    internal static string Unknown(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFinalStateUnknown(path);

    internal static string FailedEffect(string path) => global::OpenForge.Cli.OutputText.Route.Shared.CanonicalPhrases.FormatFailed(path);

    internal static string InvalidSource(string reference)
        => global::OpenForge.Cli.OutputText.Shared.CanonicalPhrases.FilterNotASource(reference);

    internal static string OverwriteAmbiguous(string name)
        => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatCouldBelongToMoreThanOneBaseFile(name);
}
