using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

internal sealed record RecoveryBundlePreparation
{
    internal RecoveryBundlePreparation(RecoveryBundleReader.VerifiedFinalToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        Verified = token.Verified;
    }

    internal RecoveryBundleVerifiedRead Verified { get; }

    internal string BundlePath => Verified.BundlePath;

    internal string WorkspacePhysicalPath => Verified.WorkspacePhysicalPath;

    internal string WorkspaceKey => Verified.WorkspaceKey;

    internal string Command => Verified.Command;

    internal RecoveryBundleAttribution Attribution => Verified.Attribution;

    internal Guid OperationId => Verified.OperationId;

    internal ImmutableArray<RecoveryEntry> Entries => Verified.Entries;

    internal bool MatchesWorkspace(CliWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        return string.Equals(
                WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot),
                WorkspacePhysicalPath,
                PathComparison())
            && string.Equals(
                WorkspaceIdentity.Key(workspace.PhysicalRoot),
                WorkspaceKey,
                StringComparison.Ordinal);
    }

    internal bool MatchesOperation(WorkspaceLockRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        return MatchesWorkspace(request.Workspace)
            && request.OperationId == OperationId
            && string.Equals(request.Command, Command, StringComparison.Ordinal);
    }

    internal bool MatchesChange(
        WorkspaceLockRequest request,
        PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        if (!MatchesOperation(request))
        {
            return false;
        }

        string targetPath;
        try
        {
            targetPath = RelativeTarget(request.Workspace, change.LogicalPath);
        }
        catch (ArgumentException)
        {
            return false;
        }

        var entry = Entries.SingleOrDefault(item => string.Equals(
            item.TargetPath,
            targetPath,
            StringComparison.Ordinal));
        return entry is not null && entry.Matches(targetPath, change);
    }

    internal bool MatchesRelativeFileLink(
        WorkspaceLockRequest request,
        RelativeFileLinkEffect effect)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(effect);
        if (!MatchesOperation(request))
        {
            return false;
        }

        var entry = Entries.SingleOrDefault(item => string.Equals(
            item.TargetPath,
            effect.LogicalPath.Value,
            StringComparison.Ordinal));
        return entry is not null
            && entry.Matches(effect.LogicalPath.Value, effect);
    }

    private static string RelativeTarget(CliWorkspace workspace, string logicalPath)
    {
        var relative = Path.GetRelativePath(workspace.LexicalRoot, logicalPath);
        return CanonicalRelativePath.Create(relative
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/')).Value;
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
