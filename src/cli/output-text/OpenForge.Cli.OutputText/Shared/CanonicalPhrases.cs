namespace OpenForge.Cli.OutputText.Shared;

internal static class CanonicalPhrases
{
    // @OpenForgeText shared.phrase.would-create
    internal static string FormatWouldCreate(string pathText)
        => $"Would create {pathText}";

    // @OpenForgeText shared.wording.description
    internal static string MetadataDescription(string value)
        => $"description: {value}";

    // @OpenForgeText shared.wording.already-exists-with-different-content
    internal static string TargetContentDiffers(string path)
        => $"{path} already exists with different content.";

    // @OpenForgeText shared.wording.updated-the-entries-section-of
    internal static string EntriesUpdated(string path)
        => $"Updated the Entries section of {path}";

    // @OpenForgeText shared.phrase.the-deleted-files-are-kept-in-a-recovery-bundle-at
    internal static string FormatTheDeletedFilesAreKeptInARecoveryBundleAt(string pathText)
        => $"The deleted files are kept in a recovery bundle at {pathText}.";

    // @OpenForgeText shared.phrase.recovery-data
    internal static string FormatRecoveryData(string pathText)
        => $"Recovery data: {pathText}";

    // @OpenForgeText shared.wording.saved-a-grant-for-to-agents-open-forge-json
    internal static string SavedGrant(string path)
        => $"Saved a grant for {path} to .agents/open-forge.json";

    // @OpenForgeText shared.wording.would-save-a-grant-for-to-agents-open-forge-json
    internal static string WouldSaveGrant(string path)
        => $"Would save a grant for {path} to .agents/open-forge.json";

    // @OpenForgeText shared.wording.is-not-a-source-id-or-a-path-under-agents
    internal static string FilterNotASource(string value)
        => $"{value} is not a source ID or a path under .agents.";

    // @OpenForgeText shared.wording.framework-fingerprint
    internal static string FrameworkFingerprint(string value)
        => $"Framework fingerprint: {value}";
}
