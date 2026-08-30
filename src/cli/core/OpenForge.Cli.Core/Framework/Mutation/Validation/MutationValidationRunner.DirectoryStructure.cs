using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation;

internal static partial class MutationValidationRunner
{
    private sealed class CombinedPlanStructure
    {
        private readonly FileExpectationValidator _validator;
        private readonly CliWorkspace _workspace;
        private readonly string _lockLogicalPath;
        private readonly string? _lockPhysicalPath;
        private readonly string _bootstrapDirectoryPath;
        private readonly HashSet<string> _logicalPaths = new(PhysicalIdentityTracker.PathComparer);
        private readonly HashSet<string> _expectedPhysicalPaths = new(PhysicalIdentityTracker.PathComparer);
        private readonly HashSet<string> _plannedDirectories = new(PhysicalIdentityTracker.PathComparer);

        internal CombinedPlanStructure(
            FileExpectationValidator validator,
            CliWorkspace workspace)
        {
            _validator = validator;
            _workspace = workspace;
            _lockLogicalPath = Path.Combine(
                workspace.LexicalRoot,
                WorkspaceLockRequest.RelativePath);
            var lockResolution = validator.ResolvePath(workspace, _lockLogicalPath);
            _lockPhysicalPath = lockResolution.State == PhysicalPathState.Contained
                ? lockResolution.GetContainedPhysicalPath()
                : null;
            _bootstrapDirectoryPath = Path.Combine(
                workspace.LexicalRoot,
                WorkspaceLockRequest.DirectoryName);
        }

        internal bool TryRegisterTarget(
            string logicalPath,
            string? expectedPhysicalPath,
            out string cause)
        {
            if (IsReservedPlanTarget(logicalPath))
            {
                cause = "A combined mutation plan cannot target the workspace root or lock bootstrap directory.";
                return false;
            }

            if (!_logicalPaths.Add(logicalPath))
            {
                cause = "Directory and file effects cannot repeat a logical target.";
                return false;
            }

            if (PhysicalIdentityTracker.PathComparer.Equals(logicalPath, _lockLogicalPath)
                || expectedPhysicalPath is not null
                    && _lockPhysicalPath is not null
                    && PhysicalIdentityTracker.PathComparer.Equals(
                        expectedPhysicalPath,
                        _lockPhysicalPath)
                || IsReservedLockPhysicalPath(
                    _validator,
                    _workspace,
                    logicalPath,
                    _lockPhysicalPath,
                    PhysicalIdentityTracker.PathComparer))
            {
                cause = "A mutation plan cannot target its workspace lock file.";
                return false;
            }

            if (expectedPhysicalPath is not null
                && !_expectedPhysicalPaths.Add(expectedPhysicalPath))
            {
                cause = "Directory and file effects cannot repeat an expected resolved physical target.";
                return false;
            }

            cause = string.Empty;
            return true;
        }

        private bool IsReservedPlanTarget(string logicalPath)
            => PhysicalIdentityTracker.PathComparer.Equals(
                logicalPath,
                _workspace.LexicalRoot)
                || PhysicalIdentityTracker.PathComparer.Equals(
                    logicalPath,
                    _bootstrapDirectoryPath);

        internal bool HasPermittedDirectParent(
            string logicalPath,
            out string cause)
        {
            var parentPath = Path.GetDirectoryName(logicalPath);
            if (parentPath is null
                || !PhysicalContainment.Contains(_workspace.LexicalRoot, parentPath))
            {
                cause = "A planned filesystem effect requires one contained direct parent directory.";
                return false;
            }

            if (_plannedDirectories.Contains(parentPath))
            {
                cause = string.Empty;
                return true;
            }

            var resolution = _validator.ResolvePath(_workspace, parentPath);
            if (resolution.State == PhysicalPathState.Missing
                && PhysicalIdentityTracker.PathComparer.Equals(
                    parentPath,
                    _bootstrapDirectoryPath))
            {
                cause = string.Empty;
                return true;
            }

            if (resolution.State != PhysicalPathState.Contained
                || !IsOrdinaryDirectory(resolution.GetContainedPhysicalPath()))
            {
                cause = "A planned filesystem effect requires an existing ordinary contained direct parent or an earlier planned directory creation.";
                return false;
            }

            cause = string.Empty;
            return true;
        }

        internal void AddPlannedDirectory(string logicalPath)
            => _plannedDirectories.Add(logicalPath);

        private static bool IsOrdinaryDirectory(string physicalPath)
        {
            try
            {
                var attributes = File.GetAttributes(physicalPath);
                return (attributes & FileAttributes.Directory) != 0
                    && (attributes & (FileAttributes.Device | FileAttributes.ReparsePoint)) == 0;
            }
            catch (Exception exception) when (exception is FileNotFoundException
                or DirectoryNotFoundException
                or UnauthorizedAccessException
                or IOException
                or NotSupportedException
                or PlatformNotSupportedException
                or ArgumentException)
            {
                return false;
            }
        }
    }
}
