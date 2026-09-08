using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Remove.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Record;

namespace OpenForge.Cli.Core.Commands.Extension.Remove.Shared.Planning;

internal static class ExtensionRemoveLibraryBoundaryReader
{
    internal static async ValueTask<ExtensionRemoveLibraryBoundary> ReadAsync(
        PhysicalPathResolver resolver,
        ExtensionRemoveRequest request,
        string path,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(path);
        cancellationToken.ThrowIfCancellationRequested();
        var record = await LibrariesRecordReader.ReadAsync(resolver, request.Workspace, cancellationToken).ConfigureAwait(false);
        var logicalPath = Path.GetFullPath(Path.Combine(request.Workspace.LexicalRoot, path.Replace('/', Path.DirectorySeparatorChar)));
        var leaf = NoFollowLeafObserver.Observe(resolver, request.Workspace, logicalPath, cancellationToken);
        return new ExtensionRemoveLibraryBoundary(request, path, record, leaf);
    }
}
