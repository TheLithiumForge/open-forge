namespace OpenForge.Cli.OutputText.Shared;

// .agents/memory/crystallized/documents/cli/contracts/install/interface.md,
// Human Output, defines these labels for plans and results across mutation commands.
internal static class ChangeLabels
{
    // @OpenForgeText shared.change.created
    internal static string Created(bool preview)
        => preview ? "would be created" : SharedText.LabelCreated();

    // @OpenForgeText shared.change.entries
    internal static string Entries(bool preview)
        => preview ? "Entries would be updated" : "Entries updated";

    // @OpenForgeText shared.change.open-forge-section
    internal static string OpenForgeSection(bool preview)
        => preview ? "Open Forge section would be updated" : "Open Forge section updated";

    // @OpenForgeText shared.change.added-open-forge-section
    internal static string AddedOpenForgeSection(bool preview)
        => preview
            ? "Open Forge section would be added, your content would be kept"
            : "Open Forge section added, your content was kept";

    // @OpenForgeText shared.change.settings
    internal static string Settings(bool preview)
        => preview ? "settings would be updated" : "settings updated";

    // @OpenForgeText shared.change.ownership-record
    internal static string OwnershipRecord(bool preview)
        => preview ? "ownership record would be updated" : "ownership record updated";

    // @OpenForgeText shared.change.ownership-record-unchanged
    internal static string OwnershipRecordUnchanged() => "ownership record unchanged";

    // @OpenForgeText shared.change.ownership-record-not-updated
    internal static string OwnershipRecordNotUpdated() => "ownership record not updated";

    // @OpenForgeText shared.change.ownership-record-unconfirmed
    internal static string OwnershipRecordUnconfirmed() => "ownership record final state could not be confirmed";

    // @OpenForgeText shared.change.updated
    internal static string Updated(bool preview)
        => preview ? "would be updated" : SharedText.LabelUpdated();

    // @OpenForgeText shared.change.replaced
    internal static string Replaced(bool preview, bool recoveryKept)
    {
        if (recoveryKept)
            return preview
                ? "would be replaced (your previous file would be kept in a recovery bundle)"
                : "replaced (your previous file is in the recovery bundle)";

        return preview ? "would be replaced" : SharedText.LabelReplaced();
    }
}
