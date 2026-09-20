using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;

internal sealed record ExtensionRemoveLibraryBoundary
{
    internal ExtensionRemoveLibraryBoundary(
        ExtensionRemoveRequest request,
        string path,
        WorkspaceOwnershipRead ownership,
        NoFollowLeafObservation leaf)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(leaf);
        var logicalPath = System.IO.Path.GetFullPath(System.IO.Path.Combine(request.Workspace.LexicalRoot, path.Replace('/', System.IO.Path.DirectorySeparatorChar)));
        if (!PhysicalIdentityTracker.PathComparer.Equals(logicalPath, leaf.LogicalPath))
        {
            throw new ArgumentException("Library ownership and no-follow observations must identify the requested Extension target.", nameof(leaf));
        }

        Request = request;
        Path = path;
        Ownership = ownership;
        Leaf = leaf;
    }

    internal ExtensionRemoveRequest Request { get; }
    internal string Path { get; }
    internal WorkspaceOwnershipRead Ownership { get; }
    internal NoFollowLeafObservation Leaf { get; }
}
