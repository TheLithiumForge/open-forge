namespace OpenForge.Cli.OutputText.Repair;

internal static class RepairProtectionText
{
    internal static string Protected(string path) => $"{path} is protected from Repair.";
    internal static string Managed(string path) => $"{path} is managed. Its managing Update owns this file.";
    internal static string NativePackage(string path) => $"{path} is inside a native Skill package. Edit its SKILL.md or package resource.";
    internal static string PrivateOrExcluded(string path) => $"{path} is excluded or private. Restore its route or sharing intent before repairing this link.";
    internal static string ManagingUpdateReason() => "Use the managing Update to reconcile this file.";
}
