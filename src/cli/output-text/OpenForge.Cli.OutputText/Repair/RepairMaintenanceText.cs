using System.Globalization;

namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairMaintenanceText
{
    internal static string Proof(string oldPath, string newPath, bool moved, string evidence)
        => $"{oldPath} was {(moved ? "moved" : "renamed")} to {newPath}, {evidence}";
    internal static string SameContent() => "same content";
    internal static string Similarity(int score) => string.Create(CultureInfo.InvariantCulture, $"content is {score}% unchanged");
    internal static string MetadataProof() => "recorded description and tags match";
    internal static string ProofTitle() => "Rename proven";
    internal static string Malformed(string path, string command) => $"{path} has invalid frontmatter. Fix its frontmatter, then run {command}.";
    internal static string MalformedTitle() => "Invalid frontmatter";
    internal static string Several(string path) => $"{path} is missing. More than one file looks like its new location.";
    internal static string Weak(string path) => $"{path} is missing. Its new location could not be proven.";
    internal static string NoCandidate(string path) => $"{path} is missing and no replacement was found.";
    internal static string ToUse(string target, string reason) => $"To use {target} ({reason}):";
    internal static string CannotAsk() => "Repair cannot ask for a choice in this session. Nothing was changed.";
    internal static string PossibleTargets(int count)
        => string.Create(CultureInfo.InvariantCulture, $"{count} possible {(count == 1 ? "target" : "targets")}.");
    internal static string SameFileName() => "same file name";
    internal static string SameTitle() => "same title";
    internal static string MatchingText() => "matching text";
    internal static string ChooseTarget() => "Choose a target above, then run its command.";
    internal static string NothingAutomatic(int choices, int edits) => "Nothing could be repaired automatically. " + Remainder(choices, edits);
    internal static string ManualEdit(string source, int line, int column)
        => string.Create(CultureInfo.InvariantCulture, $"Update or remove this link in {source} at line {line}, column {column}.");
    internal static string Remainder(int choices, int edits)
    {
        var parts = new List<string>();
        if (choices > 0) parts.Add(string.Create(CultureInfo.InvariantCulture, $"{choices} {(choices == 1 ? "problem needs" : "problems need")} a choice"));
        if (edits > 0) parts.Add(string.Create(CultureInfo.InvariantCulture, $"{edits} {(edits == 1 ? "problem needs" : "problems need")} an edit"));
        return string.Join(" and ", parts) + ".";
    }
    internal static string Navigation(bool preview) => preview ? "Entries would be updated" : "Entries updated";
    internal static string Metadata(bool preview) => preview ? "metadata would be completed, your content would be kept" : "metadata completed, your content was kept";
    internal static string Entrypoint(bool preview) => preview ? "entrypoint would be created" : "entrypoint created";
    internal static string EntriesUpdatedCount() => "Entries updated";
    internal static string MetadataCompletedCount() => "Metadata completed";
    internal static string EntrypointsCreatedCount() => "Entrypoints created";
    internal static string FilesSkippedCount() => "Files skipped";
    internal static string ApplyPreviewReason() => "Apply the previewed repairs.";
    internal static string CompleteIndexReason() => "Complete the remaining navigation and metadata.";
    internal static string VerifyDoctorReason() => "Verify the workspace after Repair.";
    internal static string ManualEdit(string location) => $"Update or remove this link in {location}.";
    internal static string NoReplacementReason() => "No replacement was proven.";
    internal static string CorrectMetadata(string path, string command) => $"Fix the frontmatter in {path}, then run {command}.";
    internal static string IncompleteWork() => "Some files could not be checked.";
    internal static string CorrectMetadataReason() => "The malformed source needs an authored correction.";

    internal static string GitTimedOut() => "Git did not answer in time, so renamed files could not be proven. Run the command again.";

    internal static string GitRetryReason() => "Retry the request to obtain the missing rename evidence.";
}
