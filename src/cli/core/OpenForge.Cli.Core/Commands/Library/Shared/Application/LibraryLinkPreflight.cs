using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Directories;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Application;

internal sealed class LibraryLinkPreflight(WorkspaceLockLease lease, IReadOnlyList<PlannedDirectoryCreation> directories)
{
    private const string UnsafeParentCause = "A Library link parent is neither an ordinary directory nor an explicitly planned missing directory.";
    private readonly PhysicalPathResolver _resolver = new();
    private readonly HashSet<string> _missingDirectories = directories.Where(directory => directory.Expectation.Kind == FileExpectationKind.Missing)
        .Select(directory => directory.LogicalPath).ToHashSet(PhysicalIdentityTracker.PathComparer);

    internal ValueTask<RelativeFileLinkValidationResult> ValidateAsync(
        RelativeFileLinkEffect effect,
        CancellationToken cancellationToken)
    {
        var workspace = lease.Request.Workspace;
        var logicalPath = Path.Combine(workspace.LexicalRoot, effect.DestinationPath.Value.Replace('/', Path.DirectorySeparatorChar));
        var segments = effect.DestinationPath.Value.Split('/');
        var current = workspace.LexicalRoot;
        var missingParent = false;
        for (var index = 0; index < segments.Length - 1; index++)
        {
            cancellationToken.ThrowIfCancellationRequested();
            current = Path.Combine(current, segments[index]);
            var component = LinkTargetReader.Read(current);
            if (component.State == PathComponentState.Missing
                && _missingDirectories.Contains(current))
            {
                missingParent = true;
                continue;
            }
            if (component.State != PathComponentState.Ordinary || component.Attributes is not { } attributes
                || (attributes & FileAttributes.Directory) == 0 || (attributes & (FileAttributes.ReparsePoint | FileAttributes.Device)) != 0)
            {
                return ValueTask.FromResult(RelativeFileLinkValidationResult.Blocked(effect,
                    NoFollowLeafObservation.Classified(logicalPath, NoFollowLeafState.Unknown,
                        component.Failure ?? new FilesystemFailure(FilesystemFailureKind.Unsupported, UnsafeParentCause)),
                    UnsafeParentCause));
            }
        }
        if (!missingParent)
        {
            return RelativeFileLinkRevalidator.ValidateAsync(_resolver, lease, effect, cancellationToken);
        }
        var expected = NoFollowLeafObservation.Missing(logicalPath);
        var actual = NoFollowLeafObserver.Observe(_resolver, workspace, logicalPath, cancellationToken);
        return ValueTask.FromResult(RelativeFileLinkValidator.Validate(effect, expected, actual));
    }
}
