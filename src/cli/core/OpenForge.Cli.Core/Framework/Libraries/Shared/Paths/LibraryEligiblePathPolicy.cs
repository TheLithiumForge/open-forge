using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

internal static class LibraryEligiblePathPolicy
{
    private const string GitMetadataDirectoryName = ".git";

    internal static bool IsGitMetadata(WorkspaceRelativeDirectory sourceRoot, string sourcePath)
        => $"{sourceRoot.Value}/{sourcePath}".Split('/').Any(segment =>
            string.Equals(segment, GitMetadataDirectoryName, StringComparison.OrdinalIgnoreCase));

    internal static bool TryClassifyExclusion(
        WorkspaceRelativeDirectory sourceRoot,
        SourceRelativeEligiblePath sourcePath,
        out LibraryInventoryExclusionKind kind)
    {
        var originalSegments = $"{sourceRoot.Value}/{sourcePath.Value}".Split('/');
        if (IsGitMetadata(sourceRoot, sourcePath.Value))
        {
            kind = LibraryInventoryExclusionKind.ManagerControl;
            return true;
        }

        var agentsIndex = Array.FindIndex(originalSegments, segment =>
            string.Equals(segment, WorkspacePermissionDefinitions.ImplicitDirectoryPath, StringComparison.Ordinal));
        if (agentsIndex < 0)
        {
            kind = default;
            return false;
        }

        var originalPath = string.Join('/', originalSegments[agentsIndex..]);
        if (originalPath == SourceLogicalPath.LoaderPath)
        {
            kind = LibraryInventoryExclusionKind.Loader;
            return true;
        }
        if (originalPath is LibraryPathIdentity.RecordRelativePath
            or LifecycleSchema.RelativePath
            or WorkspacePermissionDefinitions.RelativePath)
        {
            kind = LibraryInventoryExclusionKind.ManagerControl;
            return true;
        }
        if (SourceFormClassifier.TryClassify(originalPath, out var form))
        {
            if (form == SourceDocumentForm.OverwriteCompanion)
            {
                kind = LibraryInventoryExclusionKind.Overwrite;
                return true;
            }
            if (SourceFormClassifier.IsEntrypoint(form))
            {
                kind = LibraryInventoryExclusionKind.Entrypoint;
                return true;
            }
        }

        kind = default;
        return false;
    }
}
