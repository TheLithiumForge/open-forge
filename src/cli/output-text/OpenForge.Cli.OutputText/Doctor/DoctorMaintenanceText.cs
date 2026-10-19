namespace OpenForge.Cli.OutputText.Doctor;

internal static class DoctorMaintenanceText
{
    internal static string ProtectedLinkEdit(string source, string protectedPath, int? line, int? column)
    {
        if (line is { } number)
            return string.Create(System.Globalization.CultureInfo.InvariantCulture,
                $"Correct this link in {source} at line {number}, column {column} through the owner of {protectedPath}.");
        return $"Correct this link in {source} through the owner of {protectedPath}.";
    }

    internal static string GitTimeout()
        => "Rename evidence did not finish in time, so renamed files could not be proven. Run the command again.";
    internal static string RenameTitle() => "Linked file was renamed or moved";
    internal static string MissingFields(string path)
        => $"{path} needs a description or tags. Index can fill the missing fields and keep your content.";
    internal static string ChainGap(string path)
        => $"{path} is inside folders without navigation files. Index can create the missing chain.";
    internal static string OutsideFolders(string path)
        => $"{path} is outside the folders Open Forge routes.";
    internal static string MoveIntoRoute(string path)
        => $"Move {path} into a category such as .agents/guidance/, or add its folder to .agents/loader.md.";
    internal static string SeveralStrong(string path)
        => $"{path} is missing. More than one file looks like its new location.";
    internal static string WeakOnly(string path)
        => $"{path} is missing. Its new location could not be proven.";
    internal static string ChooseTarget() => "Choose the intended target.";
    internal static string RenamedFragmentMissing(string oldPath, string newPath, string? fragment)
        => $"{oldPath} was renamed or moved to {newPath}, but #{fragment} is missing.";
    internal static string ChooseFragment() => "Choose a valid heading, or link to the file without a fragment.";
    internal static string ManagedLink(string path) => $"{path} is managed. Its managing Update owns this file.";
    internal static string UpdateProtectedLink() => "Restore this file through its managing Update operation.";
    internal static string NativeLink(string path) => $"{path} is inside a native Skill package.";
    internal static string ProtectedLink(string path) => $"{path} is protected from link repair.";
    internal static string EditPackageLink(string path) => $"Edit the link in {path} through its SKILL.md or package resource.";
    internal static string NoCandidate(string oldPath)
        => $"{oldPath} is missing and no replacement was found.";
    internal static string Malformed(string path, string? cause)
        => string.IsNullOrWhiteSpace(cause) ? $"{path} has invalid frontmatter." : $"{path} has invalid frontmatter. {cause}";
    internal static string FixFrontmatter(string path, string command)
        => $"Fix the frontmatter in {path}, then run {command}.";
    internal static string EditLink(string path, int? line, int? column)
    {
        if (line is { } number) return $"Update or remove this link in {path} at line {number}, column {column}.";
        return $"Update or remove this link in {path}.";
    }
    internal static string OldNavigation(string path)
        => $"{path} looks like this folder's old navigation file.";
    internal static string KeepOldRules(string path, string canonicalName, string command)
        => $"Rename {path} to {canonicalName} to keep its rules, or delete it, then run {command}.";
    internal static string CopyOldRules(string path, string canonicalPath, string command)
        => $"Copy any rules you want to keep from {path} into {canonicalPath}, then delete {path} and run {command}.";
    internal static string Rename(string oldPath, string newPath, bool moved, string basis)
        => $"{oldPath} was {(moved ? "moved" : "renamed")} to {newPath}, {basis}. Repair can update the links and affected Entries.";
    internal static string SameContent() => "same content";
    internal static string Unchanged(int similarity) => $"content is {similarity}% unchanged";
    internal static string MetadataMatch() => "recorded description and tags match";
    internal static string RepairReason() => "Repair can update the links and affected Entries.";
    internal static string IndexReason() => "Complete missing metadata and navigation while keeping your content.";
    internal static string ManagedMetadata(string path) => $"{path} needs a description or tags. Its managing Update operation can restore its metadata.";
    internal static string NativeMetadata(string path) => $"{path} needs its package metadata. Complete its native Skill metadata by hand.";
    internal static string NativeResourceMetadata(string path)
        => $"{path} needs a description and tags. Index does not edit files inside a Skill package, so add them by hand.";
    internal static string AddNativeResourceMetadata(string path) => $"Add the missing description and tags to {path} by hand.";
    internal static string ReviewBoundary(string path) => $"Review the route boundary in {path} before continuing.";
    internal static string RestoreRoute(string path) => $"Restore the intentionally removed or private route for {path} before indexing it.";
}
