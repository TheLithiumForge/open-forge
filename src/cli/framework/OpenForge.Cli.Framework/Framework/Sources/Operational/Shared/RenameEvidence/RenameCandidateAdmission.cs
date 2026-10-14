using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Input;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameCandidateAdmission
{
    internal static bool Allows(string path, RenameEvidencePolicy policy)
    {
        if (!SourceFormClassifier.TryClassify(path, out var form)
            || form is SourceDocumentForm.Loader or SourceDocumentForm.Skill or SourceDocumentForm.OverwriteCompanion)
        {
            return false;
        }

        return !policy.ManagedPaths.Any(blocked => IsWithin(path, blocked))
            && !policy.PackageRoots.Any(blocked => IsWithin(path, blocked))
            && !policy.ExcludedPaths.Any(blocked => IsWithin(path, blocked))
            && !policy.PrivatePaths.Any(blocked => IsWithin(path, blocked))
            && !policy.ProjectionPaths.Any(blocked => IsWithin(path, blocked));
    }

    private static bool IsWithin(string path, string root)
        => string.Equals(path, root, PathComparison) || path.StartsWith($"{root.TrimEnd('/')}/", PathComparison);

    private static StringComparison PathComparison => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;

    internal static ImmutableArray<string> ReadBlockedPhysicalPaths(CliWorkspace workspace, PhysicalPathResolver resolver, RenameEvidencePolicy policy)
    {
        var blocked = ImmutableArray.CreateBuilder<string>();
        foreach (var path in policy.ManagedPaths.Concat(policy.PackageRoots).Concat(policy.ExcludedPaths).Concat(policy.PrivatePaths).Concat(policy.ProjectionPaths).Distinct())
        {
            if (!SourceLogicalPath.IsCanonicalRoot(path)) continue;
            var physical = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path));
            if (physical.State == PhysicalPathState.Contained) blocked.Add(physical.GetContainedPhysicalPath());
        }

        return blocked.ToImmutable();
    }

    internal static bool IsPhysicallyBlocked(string physicalPath, ImmutableArray<string> blockedPaths)
        => blockedPaths.Any(blocked => PhysicalContainment.Contains(blocked, physicalPath));

    internal static bool IsPathPhysicallyBlocked(CliWorkspace workspace, PhysicalPathResolver resolver, string path, ImmutableArray<string> blockedPaths)
    {
        if (blockedPaths.IsEmpty) return false;
        var logical = SourceLogicalPath.ToLexicalPath(workspace.LexicalRoot, path);
        while (true)
        {
            var physical = resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logical);
            if (physical.State == PhysicalPathState.Contained) return IsPhysicallyBlocked(physical.GetContainedPhysicalPath(), blockedPaths);
            if (physical.State != PhysicalPathState.Missing) return true;
            var parent = Path.GetDirectoryName(logical);
            if (parent is null) return true;
            logical = parent;
        }
    }
}
