using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Recovery.Models;

internal sealed record RecoveryBundlePreparation
{
    internal RecoveryBundlePreparation(RecoveryBundleReader.VerifiedFinalToken token)
    {
        ArgumentNullException.ThrowIfNull(token);
        var verified = token.Verified;
        BundlePath = verified.BundlePath;
        WorkspacePhysicalPath = verified.WorkspacePhysicalPath;
        WorkspaceKey = verified.WorkspaceKey;
        Command = verified.Command;
        OperationId = verified.OperationId;
        Entries = verified.Entries;
    }

    internal string BundlePath { get; }

    internal string WorkspacePhysicalPath { get; }

    internal string WorkspaceKey { get; }

    internal string Command { get; }

    internal Guid OperationId { get; }

    internal ImmutableArray<RecoveryBundleEntry> Entries { get; }

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
        if (!MatchesOperation(request) || change.Kind == PlannedFileChangeKind.Create)
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
        if (entry is null
            || entry.ChangeKind != change.Kind
            || !string.Equals(entry.Prior.Sha256, change.Expectation.ContentHash, StringComparison.Ordinal))
        {
            return false;
        }

        if (change.Kind == PlannedFileChangeKind.Delete)
        {
            return entry.Intended is null;
        }

        return entry.Intended?.Matches(change.IntendedBytes.AsSpan()) == true;
    }

    private static string RelativeTarget(CliWorkspace workspace, string logicalPath)
    {
        var relative = Path.GetRelativePath(workspace.LexicalRoot, logicalPath);
        return RecoveryBundleEntry.ValidateRelativeTarget(relative
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/'));
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
