namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal static class ToolBackupPaths
{
    internal const string Suffix = ".bak";
    internal static string Beside(string target) => target + Suffix;
}
