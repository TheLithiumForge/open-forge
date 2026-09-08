using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning;

internal static class LibraryConsumerBoundaryReader
{
    // Read the consumer root and each explicitly requested ancestor without
    // following the final leaf. Preserve positive Missing separately from
    // unavailable/unsafe observations and from incomplete coverage. A mapping
    // leaf observation cannot establish the absence of any parent directory.
    internal static ValueTask<LibraryConsumerBoundaryFacts> ReadAsync(
        PhysicalPathResolver physicalPathResolver,
        LibraryConsumerBoundaryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(request);
        request.Validate();
        cancellationToken.ThrowIfCancellationRequested();

        var workspace = request.Workspace;
        var consumerRootPath = Path.Combine(workspace.LexicalRoot, ".agents");
        var consumerRoot = Observe(
            physicalPathResolver,
            workspace,
            consumerRootPath,
            cancellationToken);
        var ancestors = request.RequiredAncestorPaths
            .Select(path => Observe(
                physicalPathResolver,
                workspace,
                Path.Combine(
                    workspace.LexicalRoot,
                    path.Value.Replace('/', Path.DirectorySeparatorChar)),
                cancellationToken))
            .ToImmutableArray();

        return ValueTask.FromResult(new LibraryConsumerBoundaryFacts
        {
            Request = request,
            ConsumerRoot = consumerRoot,
            Ancestors = ancestors,
            IsComplete = true,
        });
    }

    private static LibraryConsumerDirectoryObservation Observe(
        PhysicalPathResolver physicalPathResolver,
        CliWorkspace workspace,
        string logicalPath,
        CancellationToken cancellationToken)
    {
        var leaf = LibraryDirectoryBoundaryObserver.Observe(
            workspace,
            logicalPath,
            cancellationToken);
        var containment = physicalPathResolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            logicalPath);
        return new LibraryConsumerDirectoryObservation(leaf, containment);
    }
}
