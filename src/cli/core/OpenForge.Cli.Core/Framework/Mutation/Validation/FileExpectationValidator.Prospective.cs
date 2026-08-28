using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal sealed partial class FileExpectationValidator
{
    private bool TryResolveProspectivePhysicalPath(
        CliWorkspace workspace,
        string logicalPath,
        out string physicalPath)
    {
        physicalPath = string.Empty;
        var current = Path.GetDirectoryName(logicalPath);
        var suffix = Path.GetFileName(logicalPath);
        while (current is not null
            && PhysicalContainment.Contains(workspace.LexicalRoot, current))
        {
            var resolution = Resolve(workspace, current);
            if (resolution.State == PhysicalPathState.Contained)
            {
                var physicalParent = resolution.GetContainedPhysicalPath();
                try
                {
                    var attributes = File.GetAttributes(physicalParent);
                    if ((attributes & FileAttributes.Directory) == 0
                        || (attributes & FileAttributes.Device) != 0)
                    {
                        return false;
                    }
                }
                catch (Exception exception) when (exception is FileNotFoundException
                    or DirectoryNotFoundException
                    or UnauthorizedAccessException
                    or IOException)
                {
                    return false;
                }

                var candidate = Path.GetFullPath(Path.Combine(physicalParent, suffix));
                if (!PhysicalContainment.Contains(workspace.PhysicalRoot, candidate))
                {
                    return false;
                }

                physicalPath = candidate;
                return true;
            }

            if (resolution.State != PhysicalPathState.Missing)
            {
                return false;
            }

            var segment = Path.GetFileName(current);
            if (string.IsNullOrEmpty(segment))
            {
                return false;
            }

            suffix = Path.Combine(segment, suffix);
            current = Path.GetDirectoryName(current);
        }

        return false;
    }
}
