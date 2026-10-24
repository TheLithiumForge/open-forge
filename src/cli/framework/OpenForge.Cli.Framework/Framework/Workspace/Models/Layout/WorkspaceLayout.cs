using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

internal sealed class WorkspaceLayout
{
    internal const string DefaultMainRoot = ".agents";
    internal const string NativeSkillsPath = DefaultMainRoot + "/" + WorkspaceLayoutNames.SkillsDirectoryName;

    private WorkspaceLayout(CliWorkspace workspace, string mainRoot, ImmutableArray<WorkspaceOutsideRoute> outsideRoutes)
    {
        Workspace = workspace;
        MainRoot = mainRoot;
        SourceRoots = IsDefault ? [DefaultMainRoot] : [mainRoot, NativeSkillsPath];
        OutsideRoutes = outsideRoutes;
        InventoryRoots = [.. SourceRoots, .. outsideRoutes.Select(route => route.DirectoryPath)];
    }

    internal static WorkspaceLayout Create(CliWorkspace workspace, string mainRoot)
    {
        if (!WorkspaceMainRootPath.IsCanonical(mainRoot))
            throw new ArgumentException("The main root must be a canonical workspace content directory.", nameof(mainRoot));
        return new(workspace, mainRoot, []);
    }

    internal CliWorkspace Workspace { get; }
    internal string MainRoot { get; }
    internal string LoaderPath => MainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
    internal string HumanGuidePath => MainRoot + "/" + WorkspaceLayoutNames.HumanGuideFileName;
    /// <summary>
    /// Primary content locations only. These do not determine loading membership
    /// or grant unrestricted write authority.
    /// </summary>
    internal ImmutableArray<string> SourceRoots { get; }
    internal bool IsDefault => MainRoot == DefaultMainRoot;

    internal ImmutableArray<WorkspaceOutsideRoute> OutsideRoutes { get; }
    internal ImmutableArray<string> InventoryRoots { get; }

    internal WorkspaceLayout WithOutsideRoutes(IEnumerable<string> directories)
        => new(Workspace, MainRoot, WorkspaceOutsideRoutePath.Validate(MainRoot, directories));

    /// <summary>Classifies maintenance scope without granting loading, ownership or package permission.</summary>
    internal bool TryGetMaintenanceBoundary(string canonicalPath, out WorkspaceMaintenanceBoundary boundary)
    {
        boundary = new(WorkspaceMaintenanceBoundaryKind.Primary, MainRoot, LoaderPath);
        if (!PortableWorkspacePath.TryNormalize(canonicalPath, out _)) return false;
        if (IsOrdinallyAtOrBelow(canonicalPath, NativeSkillsPath))
        {
            boundary = new(WorkspaceMaintenanceBoundaryKind.NativeSkills, NativeSkillsPath, WorkspaceRoutePath.CanonicalEntrypointPath(NativeSkillsPath));
            return true;
        }
        if (IsOrdinallyAtOrBelow(canonicalPath, MainRoot)) return true;
        foreach (var route in OutsideRoutes)
        {
            if (!IsOrdinallyAtOrBelow(canonicalPath, route.DirectoryPath)) continue;
            boundary = new(WorkspaceMaintenanceBoundaryKind.Outside, route.DirectoryPath, route.CataloguePath);
            return true;
        }
        return false;
    }

    private static bool IsOrdinallyAtOrBelow(string path, string root)
        => path == root || path.StartsWith(root + "/", StringComparison.Ordinal);

    /// <summary>Maps a category to its primary layout location without selecting other roots.</summary>
    internal string CategoryDirectory(string category)
    {
        if (!PortableWorkspacePath.TryNormalize(category, out _) || category.Contains('/'))
            throw new ArgumentException("A category must be one canonical path segment.", nameof(category));
        return SourcePath(category);
    }

    /// <summary>Maps a source coordinate to its primary layout location without selecting other roots.</summary>
    internal string SourcePath(string sourceRelativePath)
    {
        if (!PortableWorkspacePath.TryNormalize(sourceRelativePath, out _))
            throw new ArgumentException("A source path must be canonical and relative.", nameof(sourceRelativePath));
        if (IsDefault) return DefaultMainRoot + "/" + sourceRelativePath;
        return IsAtOrBelow(sourceRelativePath, WorkspaceLayoutNames.SkillsDirectoryName)
            ? NativeSkillsPath + sourceRelativePath[WorkspaceLayoutNames.SkillsDirectoryName.Length..]
            : MainRoot + "/" + sourceRelativePath;
    }

    /// <summary>Maps a package destination through the primary layout without selecting other roots.</summary>
    internal string MapPayloadPath(string packagePath)
    {
        if (!PortableWorkspacePath.TryNormalize(packagePath, out _))
            throw new ArgumentException("A package path must be canonical and relative.", nameof(packagePath));
        if (IsDefault || !packagePath.StartsWith(DefaultMainRoot + "/", StringComparison.Ordinal)
            || IsFixedControl(packagePath)) return packagePath;
        return SourcePath(packagePath[(DefaultMainRoot.Length + 1)..]);
    }

    internal bool TryGetSourceRelativePath(string path, out string relativePath)
    {
        relativePath = string.Empty;
        if (!PortableWorkspacePath.TryNormalize(path, out _) || IsFixedControl(path)
            || path.Split('/').Contains(WorkspaceLayoutNames.GitMetadataDirectoryName, StringComparer.OrdinalIgnoreCase)) return false;
        if (IsAtOrBelow(path, NativeSkillsPath))
        {
            relativePath = path[(DefaultMainRoot.Length + 1)..];
            return true;
        }
        if (!path.StartsWith(MainRoot + "/", StringComparison.OrdinalIgnoreCase)) return false;
        var candidate = path[(MainRoot.Length + 1)..];
        if (!IsDefault && IsAtOrBelow(candidate, WorkspaceLayoutNames.SkillsDirectoryName)) return false;
        relativePath = candidate;
        return true;
    }

    internal bool IsContentPath(string path)
        => TryGetSourceRelativePath(path, out _)
            || SourceRoots.Any(root => string.Equals(path, root, StringComparison.OrdinalIgnoreCase));

    /// <summary>Identifies implicit content destinations independently of any outside-route permission.</summary>
    /// <remarks>Roots match ordinally, so a case alias of a content root needs explicit permission. On a
    /// case-sensitive filesystem that alias is another folder outside the content locations.</remarks>
    internal bool IsImplicitContentDestination(string path)
        => TryGetSourceRelativePath(path, out _)
            && (path.StartsWith(MainRoot + "/", StringComparison.Ordinal)
                || path.StartsWith(NativeSkillsPath + "/", StringComparison.Ordinal));

    private static bool IsFixedControl(string path)
        => IsAtOrBelow(path, DefaultMainRoot + "/" + WorkspaceLayoutNames.SettingsFileName)
            || IsAtOrBelow(path, DefaultMainRoot + "/" + WorkspaceLayoutNames.OwnershipFileName);

    private static bool IsAtOrBelow(string path, string root)
        => string.Equals(PortableWorkspacePath.CreatePortableKey(path), PortableWorkspacePath.CreatePortableKey(root), StringComparison.Ordinal)
            || path.StartsWith(root + "/", StringComparison.OrdinalIgnoreCase);
}
