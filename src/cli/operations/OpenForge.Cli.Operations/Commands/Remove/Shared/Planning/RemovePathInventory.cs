using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Remove.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.RelativeFileLinks;
using OpenForge.Cli.Core.Framework.Ownership.Models.Mutation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.Commands.Remove.Shared.Planning;

internal sealed class RemovePathInventory
{
    private readonly PhysicalPathResolver _resolver;

    internal RemovePathInventory(PhysicalPathResolver resolver)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        _resolver = resolver;
    }

    internal async ValueTask<RemovePathFileCapture> CaptureFileAsync(
        CliWorkspace workspace,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        var resolution = _resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logicalPath);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return new RemovePathFileCapture.Unavailable(
                "The selected file does not resolve to a contained physical path.");
        }

        var physicalPath = resolution.GetContainedPhysicalPath();
        try
        {
            var bytes = await File.ReadAllBytesAsync(physicalPath, cancellationToken).ConfigureAwait(false);
            var after = NoFollowLeafObserver.Observe(_resolver, workspace, logicalPath, cancellationToken);
            if (after.State != NoFollowLeafState.OrdinaryFile)
            {
                return new RemovePathFileCapture.Unavailable(
                    "The selected file changed kind during its inventory.");
            }
            var confirmed = _resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logicalPath);
            if (confirmed.State != PhysicalPathState.Contained
                || !RemovePathSafety.PathComparer().Equals(physicalPath, confirmed.GetContainedPhysicalPath()))
            {
                return new RemovePathFileCapture.Unavailable(
                    "The selected file changed physical identity during its inventory.");
            }
            return new RemovePathFileCapture.Captured(FileStateSnapshot.File(logicalPath, physicalPath, bytes));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new RemovePathFileCapture.Cancelled();
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
        {
            return new RemovePathFileCapture.Unavailable("The selected file bytes could not be read exactly.");
        }
    }

    internal async ValueTask<RemovePathDirectoryInventory> InventoryDirectoryAsync(
        CliWorkspace workspace,
        string logicalRoot,
        LibraryRegistrationSet? registrations,
        CancellationToken cancellationToken)
    {
        var files = ImmutableArray.CreateBuilder<FileStateSnapshot>();
        var links = ImmutableArray.CreateBuilder<RemovePathLibraryLink>();
        var directories = ImmutableArray.CreateBuilder<FileStateSnapshot>();
        var pending = new Stack<string>();
        pending.Push(logicalRoot);
        while (pending.Count > 0)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                return new RemovePathDirectoryInventory.Cancelled("Directory inventory was cancelled.");
            }
            var logicalDirectory = pending.Pop();
            var resolution = _resolver.ResolveCandidate(workspace.LexicalRoot, workspace.PhysicalRoot, logicalDirectory);
            if (resolution.State != PhysicalPathState.Contained)
            {
                return new RemovePathDirectoryInventory.Unavailable(
                    "A selected directory no longer resolves inside the workspace.");
            }
            var physicalDirectory = resolution.GetContainedPhysicalPath();
            var state = NoFollowLeafObserver.Observe(_resolver, workspace, logicalDirectory, cancellationToken);
            if (state.State != NoFollowLeafState.Directory)
            {
                return new RemovePathDirectoryInventory.Unavailable(
                    "A selected directory changed kind during its inventory.");
            }
            directories.Add(FileStateSnapshot.Directory(logicalDirectory, physicalDirectory));

            string[] children;
            try
            {
                children = Directory.GetFileSystemEntries(physicalDirectory);
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException)
            {
                return new RemovePathDirectoryInventory.Unavailable(
                    "A selected directory could not be completely enumerated.");
            }

            foreach (var childPhysical in children.OrderBy(value => value, StringComparer.Ordinal))
            {
                var relative = Path.GetRelativePath(workspace.PhysicalRoot, childPhysical)
                    .Replace(Path.DirectorySeparatorChar, '/')
                    .Replace(Path.AltDirectorySeparatorChar, '/');
                if (RemovePathSafety.ContainsGitMetadataSegment(relative))
                {
                    return new RemovePathDirectoryInventory.Unavailable(
                        "The selected directory contains protected Git metadata.");
                }
                var childLogical = Path.Combine(
                    workspace.LexicalRoot,
                    relative.Replace('/', Path.DirectorySeparatorChar));
                var childState = NoFollowLeafObserver.Observe(_resolver, workspace, childLogical, cancellationToken);
                switch (childState.State)
                {
                    case NoFollowLeafState.Directory:
                        pending.Push(childLogical);
                        break;
                    case NoFollowLeafState.OrdinaryFile:
                        var capture = await CaptureFileAsync(workspace, childLogical, cancellationToken).ConfigureAwait(false);
                        if (capture is not RemovePathFileCapture.Captured captured)
                        {
                            return capture is RemovePathFileCapture.Cancelled
                                ? new RemovePathDirectoryInventory.Cancelled("Directory inventory was cancelled.")
                                : new RemovePathDirectoryInventory.Unavailable(
                                    (capture as RemovePathFileCapture.Unavailable)?.Cause
                                    ?? "A selected file changed during inventory.");
                        }
                        files.Add(captured.Snapshot);
                        break;
                    case NoFollowLeafState.RelativeFileLink:
                        var link = ResolveLibraryLink(relative, childState, registrations);
                        if (link is null)
                        {
                            return new RemovePathDirectoryInventory.Unavailable(
                                "The selected directory contains a relative link that is not a recorded Library mapping.");
                        }
                        links.Add(link);
                        break;
                    case NoFollowLeafState.Link:
                        return new RemovePathDirectoryInventory.Unavailable(
                            "The selected directory contains a link that is not a known removable Library link.");
                    default:
                        return new RemovePathDirectoryInventory.Unavailable(childState.Failure?.DirectCause
                            ?? "The selected directory contains a special, changed, or unreadable child.");
                }
            }
        }

        return new RemovePathDirectoryInventory.Complete(files.ToImmutable(), links.ToImmutable(), directories.ToImmutable());
    }

    internal static RemovePathLibraryLink? ResolveLibraryLink(
        string relativePath,
        NoFollowLeafObservation observation,
        LibraryRegistrationSet? registrations)
    {
        if (registrations is null || observation.RelativeFileLink is not { } linkIdentity)
        {
            return null;
        }

        var matches = registrations.Libraries
            .SelectMany(library => LibraryPathIdentity.Mappings(library)
                .Where(mapping => string.Equals(mapping.DestinationPath.Value, relativePath, StringComparison.Ordinal)
                    && mapping.ExpectedRelativeLink.Value == linkIdentity.RawRelativeTarget)
                .Select(mapping => (Library: library, Mapping: mapping)))
            .ToArray();
        if (matches.Length != 1)
        {
            return null;
        }

        var match = matches[0];
        return new RemovePathLibraryLink(
            relativePath,
            RelativeFileLinkEffect.Delete(CanonicalRelativePath.Create(relativePath), linkIdentity),
            observation,
            new LibraryPathRelease(match.Library.Id.Value, match.Mapping.SourcePath.Value));
    }
}
