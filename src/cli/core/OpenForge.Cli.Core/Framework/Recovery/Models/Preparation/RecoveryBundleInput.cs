using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

internal sealed record RecoveryBundleInput
{
    private RecoveryBundleInput(
        RecoveryBundleOperationIdentity identity,
        ImmutableArray<RecoveryBundleTarget> targets,
        ImmutableArray<RecoveryBundleTarget> recoveryTargets)
    {
        Identity = identity;
        Targets = targets;
        RecoveryTargets = recoveryTargets;
    }

    private RecoveryBundleOperationIdentity Identity { get; }

    internal CliWorkspace Workspace => Identity.Workspace;

    internal string Command => Identity.Command;

    internal RecoveryBundleAttribution Attribution => Identity.Attribution;

    internal Guid OperationId => Identity.OperationId;

    internal ImmutableArray<RecoveryBundleTarget> Targets { get; }

    internal ImmutableArray<RecoveryBundleTarget> RecoveryTargets { get; }

    internal static RecoveryBundleInput Create(
        CliWorkspace workspace,
        string command,
        RecoveryBundleAttribution attribution,
        Guid operationId,
        IReadOnlyList<RecoveryBundleTarget> targets)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentException.ThrowIfNullOrWhiteSpace(command);
        ArgumentNullException.ThrowIfNull(attribution);
        ArgumentNullException.ThrowIfNull(targets);
        if (operationId == Guid.Empty)
        {
            throw new ArgumentException("A recovery operation ID cannot be empty.", nameof(operationId));
        }

        var workspaceKey = WorkspaceIdentity.Key(
            WorkspaceIdentity.NormalizePhysicalPath(workspace.PhysicalRoot));
        if (attribution.Subject.Kind != RecoveryBundleSubjectKind.Workspace
            || !string.Equals(attribution.Subject.Identity, workspaceKey, StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "Recovery attribution must identify the selected workspace.",
                nameof(attribution));
        }

        var all = ImmutableArray.CreateBuilder<RecoveryBundleTarget>(targets.Count);
        var recovery = ImmutableArray.CreateBuilder<RecoveryBundleTarget>();
        var paths = new HashSet<string>(PathComparer());
        foreach (var target in targets)
        {
            ArgumentNullException.ThrowIfNull(target);
            ValidateTarget(workspace, target);
            if (!paths.Add(TargetPath(workspace, target)))
            {
                throw new ArgumentException(
                    "A recovery operation cannot contain the same target more than once.",
                    nameof(targets));
            }

            all.Add(target);
            if (target.RequiresRecovery)
            {
                recovery.Add(target);
            }
        }

        return new RecoveryBundleInput(
            new RecoveryBundleOperationIdentity(
                workspace,
                command,
                attribution,
                operationId),
            all.MoveToImmutable(),
            recovery.ToImmutable());
    }

    internal string GetRelativeTarget(PlannedFileChange change)
    {
        ArgumentNullException.ThrowIfNull(change);
        return RelativeTarget(Workspace, change.LogicalPath);
    }

    private static string RelativeTarget(
        CliWorkspace workspace,
        string logicalPath)
    {
        var relative = Path.GetRelativePath(workspace.LexicalRoot, logicalPath);
        if (Path.IsPathFullyQualified(relative)
            || relative == ".."
            || relative.StartsWith($"..{Path.DirectorySeparatorChar}", StringComparison.Ordinal)
            || relative.StartsWith($"..{Path.AltDirectorySeparatorChar}", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A recovery target must be contained by its selected workspace.",
                nameof(logicalPath));
        }

        return relative
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
    }

    internal static string GetRelativeTarget(RelativeFileLinkEffect effect)
    {
        ArgumentNullException.ThrowIfNull(effect);
        return CanonicalRelativePath.Create(effect.LogicalPath.Value).Value;
    }

    private static string TargetPath(
        CliWorkspace workspace,
        RecoveryBundleTarget target)
        => target.LinkEffect is { } effect
            ? effect.LogicalPath.Value
            : RelativeTarget(workspace, target.Change.LogicalPath);

    private static void ValidateTarget(
        CliWorkspace workspace,
        RecoveryBundleTarget target)
    {
        if (target.LinkEffect is { } effect
            && target.LinkBefore is { } linkBefore)
        {
            var logicalPath = Path.GetFullPath(Path.Combine(
                workspace.LexicalRoot,
                effect.LogicalPath.Value.Replace('/', Path.DirectorySeparatorChar)));
            if (!PhysicalContainment.Contains(workspace.LexicalRoot, logicalPath)
                || !string.Equals(
                    linkBefore.LogicalPath,
                    logicalPath,
                    PathComparison()))
            {
                throw new ArgumentException(
                    "A relative file-link recovery target must be contained by its selected workspace.",
                    nameof(target));
            }

            return;
        }

        if (target.LinkEffect is not null || target.LinkBefore is not null)
        {
            throw new ArgumentException(
                "A relative file-link recovery target requires both its effect and prior observation.",
                nameof(target));
        }

        if (!PhysicalContainment.Contains(workspace.LexicalRoot, target.Change.LogicalPath))
        {
            throw new ArgumentException(
                "A recovery target must be logically contained by its selected workspace.",
                nameof(target));
        }

        if (target.Before.PhysicalPath is { } physicalPath
            && !PhysicalContainment.Contains(workspace.PhysicalRoot, physicalPath))
        {
            throw new ArgumentException(
                "A recovery target must be physically contained by its selected workspace.",
                nameof(target));
        }
    }

    private static StringComparer PathComparer()
        => OperatingSystem.IsWindows() ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal;

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    private sealed record RecoveryBundleOperationIdentity(
        CliWorkspace Workspace,
        string Command,
        RecoveryBundleAttribution Attribution,
        Guid OperationId);
}
