using OpenForge.Cli.Core.Framework.Filesystem.Shared.GitIgnore;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Backups;

internal static class ToolBackupPaths
{
    internal const string Suffix = ".bak";
    internal static readonly string[] WorkspaceTargets = [WorkspaceSettingsDefinitions.RelativePath, WorkspaceOwnershipDefinitions.RelativePath, ManagedGitIgnoreSection.Path];

    internal static string Beside(string target) => target + Suffix;
}
