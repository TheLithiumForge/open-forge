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

    // @OpenForgeText remove.remove.help.helpsyntax
    internal static string HelpSyntax()
        => "open-forge remove <target> [options]";

    // @OpenForgeText remove.remove.help.helpexamples
    internal static string HelpExamples()
        => "open-forge remove .agents/guidance/old.md --dry-run\nopen-forge remove .agents/guidance/old.md\nopen-forge remove planning --kind extension --dry-run";

    // @OpenForgeText remove.remove.help.helpbehavior
    internal static string HelpBehavior()
        => "The default kind is path. Select one exact target.\n--dry-run shows the checked removal plan and writes nothing.\nAn interactive writing plan asks for confirmation unless --automatic is supplied.\nRedirected or JSON requests that would write need --automatic.\nSupported managed removals are remembered so later updates do not restore them. Interrupted writes report recovery data.";

    // @OpenForgeText remove.remove.help.helpheadingkinds
    internal static string HelpHeadingKinds()
        => "Kinds";

    // @OpenForgeText remove.remove.help.helpkinds
    internal static string HelpKinds()
        => "path removes an exact workspace path. Route removes a routed source or category. Extension removes an installed package. Library detaches its registration and links while keeping source files. Each kind uses its own safety and permission rules. --allow-path grants only the exact additional destination required by the selected operation.";

    // @OpenForgeText remove.remove.help.helprelatedcommands
    internal static IReadOnlyList<(string Label, string? Description)> HelpRelatedCommands()
        =>
        [
            ("route remove --help", "Remove a routed file or category."),
            ("extension remove --help", "Remove an installed Extension."),
            ("library detach --help", "Detach links while keeping source files."),
        ];
    // @OpenForgeText remove.help.details-1
    internal static string HelpKindsDetails()
        => "path removes an exact workspace path. Route removes a routed source or category. Extension removes an installed package. Library removes its registration and links while keeping source files. Each manager retains its own safety and permission rules. --allow-path grants only the exact additional destination needed.";
}
