using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;

internal static class LibraryEligiblePathPolicy
{
    internal static bool TryClassifyExclusion(
        string path,
        out LibraryInventoryExclusionKind kind)
    {
        if (string.Equals(path, SourceLogicalPath.LoaderPath, StringComparison.Ordinal))
        {
            kind = LibraryInventoryExclusionKind.Loader;
            return true;
        }

        if (string.Equals(path, LibraryPathIdentity.RecordRelativePath, StringComparison.Ordinal)
            || string.Equals(path, LifecycleSchema.RelativePath, StringComparison.Ordinal))
        {
            kind = LibraryInventoryExclusionKind.ManagerControl;
            return true;
        }

        if (SourceFormClassifier.TryClassify(path, out var form))
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
