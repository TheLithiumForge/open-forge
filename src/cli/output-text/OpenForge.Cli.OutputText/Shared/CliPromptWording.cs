using System.Globalization;

namespace OpenForge.Cli.OutputText.Shared;

internal static class CliPromptWording
{
    // @OpenForgeText shared.wording.choose-a-number-1-or-press-enter-to-cancel
    internal static string SelectLine(int count) => string.Create(CultureInfo.InvariantCulture, $"Choose a number (1-{count}), or press Enter to cancel:");

    // @OpenForgeText shared.wording.is-required-by-unchoose-that-first
    internal static string Required(string label, string owners) => $"{label} is required by {owners}. Unchoose that first.";

    // @OpenForgeText shared.wording.also-installing-required-by
    internal static string AlsoInstalling(string label, string owners) => $"Also installing {label}, required by {owners}.";

    // @OpenForgeText shared.wording.also-removing-needed-by
    internal static string AlsoRemoving(string label, string owners) => $"Also removing {label}, needed by {owners}.";

    // @OpenForgeText shared.wording.needs
    internal static string Needs(string labels) => $"needs: {labels}";

    // @OpenForgeText shared.wording.needed-by
    internal static string NeededBy(string labels) => $"needed by: {labels}";

    // @OpenForgeText shared.wording.required-by
    internal static string RequiredBy(string labels) => $"required by {labels}";

    // @OpenForgeText shared.wording.writes-outside-agents
    internal static string Permission(string owner) => $"{owner} writes outside .agents:";

    // @OpenForgeText shared.wording.directory-everything-under-it
    internal static string Directory(string path) => $"{path}   (directory: everything under it)";
}
