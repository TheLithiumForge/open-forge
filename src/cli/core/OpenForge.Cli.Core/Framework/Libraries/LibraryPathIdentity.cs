using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;

namespace OpenForge.Cli.Core.Framework.Libraries;

internal static class LibraryPathIdentity
{
    internal const string RecordRelativePath = ".agents/open-forge.libraries.json";

    internal static LibraryMapping Map(
        WorkspaceRelativeDirectory sourceRoot,
        SourceRelativeEligiblePath sourcePath)
        => LibraryMapping.Create(sourceRoot, sourcePath);

    internal static string RecordPath(string workspacePhysicalRoot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(workspacePhysicalRoot);
        return Path.GetFullPath(Path.Combine(
            workspacePhysicalRoot,
            RecordRelativePath.Replace('/', Path.DirectorySeparatorChar)));
    }
}
