namespace OpenForge.Cli.OutputText.Remove;

internal static class RemoveText
{
    // @OpenForgeText remove.confirm.missing
    internal static string ConfirmMissing(string target) => Shared.SharedText.TitleApplyTheseChangesYN();

    // @OpenForgeText remove.confirm.file
    internal static string ConfirmFile() => Shared.SharedText.TitleApplyTheseChangesIncludingDeletingYN(1);

    // @OpenForgeText remove.confirm.tree
    internal static string ConfirmTree(int files, int directories)
        => files > 0
            ? Shared.SharedText.TitleApplyTheseChangesIncludingDeletingYN(files)
            : Shared.SharedText.TitleApplyTheseChangesYN();

    // @OpenForgeText remove.headline.preview
    internal static string Preview(string target) => $"Would remove {target}";

    // @OpenForgeText remove.headline.done
    internal static string Done(string target) => $"Removed {target}";

    // @OpenForgeText remove.headline.unchanged
    internal static string Unchanged(string target) => $"No changes for {target}";

    // @OpenForgeText remove.headline.reconciled
    internal static string Reconciled(string target) => $"Reconciled removal of {target}";

    // @OpenForgeText remove.effect
    internal static string Effect(string kind, string action, string outcome, string path)
    {
        if (outcome is "planned" or "done")
        {
            var preview = outcome == "planned";
            if (action == "create")
            {
                return $"{path}  {Shared.ChangeLabels.Created(preview)}";
            }
            var label = kind switch
            {
                "setting" => Shared.ChangeLabels.Settings(preview),
                "navigation" => Shared.ChangeLabels.Entries(preview),
                "record" => Shared.ChangeLabels.OwnershipRecord(preview),
                _ => null,
            };
            if (label is not null)
            {
                return $"{path}  {label}";
            }
        }
        var subject = kind switch
        {
            "directory" => "directory",
            "link" => "link",
            "navigation" => "navigation file",
            "setting" => "setting",
            "record" => "record",
            _ => "file",
        };
        var wording = action switch
        {
            "create" => (Verb: "create", Past: "Created", Participle: "created"),
            "delete" => (Verb: "remove", Past: "Removed", Participle: "removed"),
            "persist" or "update" => (Verb: "update", Past: "Updated", Participle: "updated"),
            "record-removal" => (Verb: "record removal in", Past: "Recorded removal in", Participle: "recorded"),
            "release-ownership" => (Verb: "release ownership from", Past: "Released ownership from", Participle: "released"),
            _ => (Verb: "change", Past: "Changed", Participle: "changed"),
        };
        return outcome switch
        {
            "planned" => $"Would {wording.Verb} {subject} {path}",
            "done" => $"{wording.Past} {subject} {path}",
            "failed" => $"Could not {wording.Verb} {subject} {path}",
            "unknown" => $"Could not confirm whether {subject} {path} was {wording.Participle}",
            "not-started" => $"Did not {wording.Verb} {subject} {path}",
            _ => $"Could not confirm the outcome for {subject} {path}",
        };
    }
}
