using System.Globalization;

namespace OpenForge.Cli.OutputText.Index;

internal static class IndexWording
{
    // @OpenForgeText index.wording.cannot-index
    internal static string CannotIndex(string operand, string reason) => $"Cannot index {operand}: {reason}.";

    // @OpenForgeText index.wording.cannot-rebuild-the-entries-section-of
    internal static string Blocked(string parent) => $"Cannot rebuild the Entries section of {parent}.";

    // @OpenForgeText index.wording.entries-section
    internal static string EntriesSection(string path) => $"--- {path}  (Entries section)";

    // @OpenForgeText index.wording.selection
    internal static string ExplicitSelection(string sources) => $"Selection: {sources}";

    internal static string RecoveryRetained(string path) => global::OpenForge.Cli.OutputText.Shared.SharedPhrases.FormatTheRecoveryBundleWasRetainedAt(path);

    internal static string UnknownSource(string operand) => global::OpenForge.Cli.OutputText.Shared.CliFindingWording.UnknownSource(operand);

    // @OpenForgeText index.wording.is-a-folder-not-a-source
    internal static string FolderSource(string operand) => $"{operand} is a folder, not a source.";

    // @OpenForgeText index.wording.open-forge-index
    internal static string CorrectedSource(string id) => $"open-forge index {id}";

    // @OpenForgeText index.wording.the-routes-under-could-not-be-resolved-to-one-structure
    internal static string TopologyAmbiguous(string source, string reason) => $"The routes under {source} could not be resolved to one structure: {reason}.";

    // @OpenForgeText index.wording.is-routed-but-no-parent-lists-it-so-its-entry-has-nowhere-to-go
    internal static string TargetUnexposed(string path) => $"{path} is routed but no parent lists it, so its entry has nowhere to go.";

    // @OpenForgeText index.wording.could-not-be-read-so-the-routes-below-it-are-unknown
    internal static string DiscoveryIncomplete(string path) => $"{path} could not be read, so the routes below it are unknown.";
}
