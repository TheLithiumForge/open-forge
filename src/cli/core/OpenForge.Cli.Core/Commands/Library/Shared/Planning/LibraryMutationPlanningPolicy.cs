using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Library.Models.Planning;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using OpenForge.Cli.Core.Framework.Permissions;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Shared.Planning;

internal static class LibraryMutationPlanningPolicy
{
    internal static LibraryConsumerBoundaryEvaluation EvaluateConsumerBoundary(
        LibraryConsumerBoundaryFacts facts,
        bool allowMissingAncestors)
    {
        ArgumentNullException.ThrowIfNull(facts);
        var workspace = facts.Request.Workspace;
        if (!facts.IsComplete
            || facts.ConsumerRoot is null
            || facts.Ancestors.Length != facts.Request.RequiredAncestorPaths.Length)
        {
            return Incomplete("The Library consumer-directory boundary was not observed completely.");
        }

        var rootPath = Path.Combine(workspace.LexicalRoot, ".agents");
        var root = EvaluateDirectory(facts.ConsumerRoot, rootPath, allowMissing: false);
        if (root.State != LibraryPlanState.Complete)
        {
            return root;
        }

        var directories = ImmutableArray.CreateBuilder<PlannedDirectoryCreation>();
        var observedPaths = new HashSet<string>(PhysicalIdentityTracker.PathComparer);
        for (var index = 0; index < facts.Request.RequiredAncestorPaths.Length; index++)
        {
            var relativePath = facts.Request.RequiredAncestorPaths[index];
            var logicalPath = Path.GetFullPath(Path.Combine(
                workspace.LexicalRoot,
                relativePath.Value.Replace('/', Path.DirectorySeparatorChar)));
            if (!observedPaths.Add(logicalPath))
            {
                return Blocked(logicalPath, "The Library consumer boundary contains an aliased ancestor path.");
            }

            var observation = facts.Ancestors[index];
            var evaluation = EvaluateDirectory(observation, logicalPath, allowMissingAncestors);
            if (evaluation.State != LibraryPlanState.Complete)
            {
                return evaluation;
            }

            if (observation.Leaf.State == NoFollowLeafState.Missing)
            {
                directories.Add(PlannedDirectoryCreation.Create(FileExpectation.Missing(logicalPath)));
            }
        }

        return new LibraryConsumerBoundaryEvaluation(
            LibraryPlanState.Complete,
            [.. directories
                .OrderBy(directory => Depth(directory.LogicalPath))
                .ThenBy(directory => directory.LogicalPath, StringComparer.Ordinal)],
            Path: null,
            Cause: null);
    }

    internal static bool TryFindOwnershipConflict(
        LifecycleOwnershipReadResult ownership,
        IEnumerable<string> destinationPaths,
        out LifecycleOwnershipClaim? conflict)
    {
        ArgumentNullException.ThrowIfNull(ownership);
        ArgumentNullException.ThrowIfNull(destinationPaths);
        var paths = destinationPaths.Select(PortableWorkspacePath.CreatePortableKey).ToHashSet(StringComparer.Ordinal);
        conflict = ownership.Claims.FirstOrDefault(claim => paths.Contains(PortableWorkspacePath.CreatePortableKey(claim.Path)));
        return conflict is not null;
    }

    internal static bool HasDestinationAlias(
        CliWorkspace workspace,
        IEnumerable<string> destinationPaths)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(destinationPaths);
        var paths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in destinationPaths)
        {
            if (!paths.Add(PortableWorkspacePath.CreatePortableKey(path)))
            {
                return true;
            }
        }

        return false;
    }

    internal static bool CoversDestinationAncestors(
        LibraryConsumerBoundaryFacts facts,
        IEnumerable<string> destinationPaths)
    {
        ArgumentNullException.ThrowIfNull(facts);
        ArgumentNullException.ThrowIfNull(destinationPaths);
        var required = new HashSet<string>(StringComparer.Ordinal);
        foreach (var destinationPath in destinationPaths)
        {
            var segments = destinationPath.Split('/');
            for (var length = 1; length < segments.Length; length++)
            {
                var ancestor = string.Join('/', segments.AsSpan(0, length).ToArray());
                if (ancestor != WorkspacePermissionDefinitions.ImplicitDirectoryPath)
                {
                    required.Add(ancestor);
                }
            }
        }

        var observed = facts.Request.RequiredAncestorPaths
            .Select(path => path.Value)
            .ToHashSet(StringComparer.Ordinal);
        return required.IsSubsetOf(observed);
    }

    internal static PlannedFileChange? CreateRecordChange(
        FileStateSnapshot snapshot,
        ReadOnlySpan<byte> intendedBytes,
        bool delete)
    {
        ArgumentNullException.ThrowIfNull(snapshot);
        if (delete)
        {
            return PlannedFileChange.Delete(snapshot.Expectation);
        }

        return snapshot.Kind switch
        {
            FileExpectationKind.Missing => PlannedFileChange.Create(snapshot.Expectation, intendedBytes),
            FileExpectationKind.File when string.Equals(
                snapshot.ContentHash,
                FileExpectation.Hash(intendedBytes),
                StringComparison.Ordinal) => null,
            FileExpectationKind.File => PlannedFileChange.Replace(snapshot.Expectation, intendedBytes),
            _ => throw new InvalidOperationException("A Library record change requires a missing or ordinary-file snapshot."),
        };
    }

    private static LibraryConsumerBoundaryEvaluation EvaluateDirectory(
        LibraryConsumerDirectoryObservation observation,
        string expectedLogicalPath,
        bool allowMissing)
    {
        if (!PhysicalIdentityTracker.PathComparer.Equals(
            observation.Leaf.LogicalPath,
            Path.GetFullPath(expectedLogicalPath)))
        {
            return Incomplete("The Library consumer-directory observation names an unexpected path.", expectedLogicalPath);
        }

        if (observation.Leaf.State == NoFollowLeafState.Directory
            && observation.Containment.State == PhysicalPathState.Contained)
        {
            return Complete();
        }

        if (allowMissing
            && observation.Leaf.State == NoFollowLeafState.Missing
            && observation.Containment.State == PhysicalPathState.Missing)
        {
            return Complete();
        }

        if (observation.Leaf.State is NoFollowLeafState.Inaccessible or NoFollowLeafState.Unknown
            || observation.Containment.State is PhysicalPathState.Inaccessible
                or PhysicalPathState.Unsupported
                or PhysicalPathState.InputOutputFailure)
        {
            return Incomplete(
                observation.Leaf.Failure?.DirectCause
                    ?? observation.Containment.Failure?.DirectCause
                    ?? "The Library consumer directory could not be observed safely.",
                expectedLogicalPath);
        }

        return Blocked(
            expectedLogicalPath,
            "The Library consumer directory is missing, unsafe, or not an ordinary directory.");
    }

    private static LibraryConsumerBoundaryEvaluation Complete()
        => new(LibraryPlanState.Complete, [], Path: null, Cause: null);

    private static LibraryConsumerBoundaryEvaluation Incomplete(string cause, string? path = null)
        => new(LibraryPlanState.Incomplete, [], path, cause);

    private static LibraryConsumerBoundaryEvaluation Blocked(string path, string cause)
        => new(LibraryPlanState.Blocked, [], path, cause);

    private static int Depth(string path)
        => path.Count(character => character is '/' or '\\');
}
