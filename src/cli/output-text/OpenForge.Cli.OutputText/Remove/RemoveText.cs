namespace OpenForge.Cli.OutputText.Remove;

internal static class RemoveText
{
    // @OpenForgeText remove.confirm.missing
    internal static string ConfirmMissing(string target) => $"Record and reconcile the missing target \"{target}\"?";

    // @OpenForgeText remove.confirm.file
    internal static string ConfirmFile() => "Remove this file and record its workspace exclusion?";

    // @OpenForgeText remove.confirm.tree
    internal static string ConfirmTree(int files, int directories)
        => $"Remove {files} files and {directories} directories and record the workspace exclusion?";

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
