using OpenForge.Cli.Core.Framework.Settings.Models.Permissions;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

/// <summary>
/// Matches a destination against the authored <c>allowInstallPaths</c>.
///
/// The entries are paths, not patterns. An entry naming a file admits that file;
/// an entry naming a directory admits everything beneath it, which is what makes
/// one entry enough for a Library that links fifty files. There is deliberately
/// no pattern language: a reader can tell what an entry admits by looking at it,
/// and a mistyped entry admits nothing rather than something unexpected.
/// </summary>
internal static class WorkspaceAllowList
{
    internal static WorkspacePermissionEvaluation Evaluate(
        ImmutableArray<string> allowInstallPaths,
        ImmutableArray<string> requirements)
    {
        ImmutableArray<string> required = [.. requirements.Distinct(StringComparer.Ordinal).Order(StringComparer.Ordinal)];
        if (required.IsEmpty)
        {
            return new(required, [], WorkspacePermissionDecision.NotRequired);
        }
        ImmutableArray<string> missing = [.. required.Where(path => !Admits(allowInstallPaths, path))];
        return new(required, missing, missing.IsEmpty ? WorkspacePermissionDecision.Granted : WorkspacePermissionDecision.Required);
    }

    internal static bool Admits(ImmutableArray<string> allowInstallPaths, string path)
    {
        if (allowInstallPaths.IsDefaultOrEmpty || string.IsNullOrWhiteSpace(path))
        {
            return false;
        }

        var target = PortableWorkspacePath.CreatePortableKey(path);
        foreach (var entry in allowInstallPaths)
        {
            if (string.IsNullOrWhiteSpace(entry))
            {
                continue;
            }

            var allowed = PortableWorkspacePath.CreatePortableKey(entry);
            if (allowed.Length == 0)
            {
                continue;
            }

            if (target == allowed
                || target.StartsWith(allowed + "/", StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }
}
