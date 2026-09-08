using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;

namespace OpenForge.Cli.Core.Framework.Libraries.Shared.Source;

internal static class LibrarySourceRootReader
{
    internal static LibrarySourceRootObservation Read(
        PhysicalPathResolver resolver,
        LibrarySourceRootRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(resolver);
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Workspace);
        ArgumentNullException.ThrowIfNull(request.SourceRoot);
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = request.Workspace;
        var lexicalSource = Path.GetFullPath(Path.Combine(
            workspace.LexicalRoot,
            request.SourceRoot.Value.Replace('/', Path.DirectorySeparatorChar)));
        var lexicallyContained = PhysicalContainment.Contains(
            workspace.LexicalRoot,
            lexicalSource);
        if (!lexicallyContained)
        {
            return Classified(
                request,
                LibrarySourceRootState.Invalid,
                lexicalSource,
                physicalSource: null,
                lexicallyContained: false,
                physicallyContained: null,
                physicallyDisjoint: null,
                "The Library source root is outside the selected workspace.");
        }

        var sourceBoundary = LibraryDirectoryBoundaryObserver.Observe(
            workspace,
            lexicalSource,
            cancellationToken);
        if (sourceBoundary.State != NoFollowLeafState.Directory)
        {
            return FromBoundary(
                request,
                lexicalSource,
                sourceBoundary,
                lexicallyContained);
        }

        var resolution = resolver.ResolveCandidate(
            workspace.LexicalRoot,
            workspace.PhysicalRoot,
            lexicalSource);
        if (resolution.State != PhysicalPathState.Contained)
        {
            return Classified(
                request,
                resolution.State == PhysicalPathState.Missing
                    ? LibrarySourceRootState.Missing
                    : LibrarySourceRootState.Blocked,
                lexicalSource,
                resolution.ResolvedPhysicalPath,
                lexicallyContained,
                physicallyContained: false,
                physicallyDisjoint: null,
                resolution.Failure?.DirectCause
                    ?? "The Library source root is not physically contained.");
        }

        var physicalSource = resolution.GetContainedPhysicalPath();
        var physicallyContained = PhysicalContainment.Contains(
            workspace.PhysicalRoot,
            physicalSource);
        if (!physicallyContained)
        {
            return Classified(
                request,
                LibrarySourceRootState.Blocked,
                lexicalSource,
                physicalSource,
                lexicallyContained,
                physicallyContained: false,
                physicallyDisjoint: null,
                "The resolved Library source root escaped the selected workspace.");
        }

        var logicalAgents = Path.Combine(lexicalSource, ".agents");
        var agentsBoundary = LibraryDirectoryBoundaryObserver.Observe(
            workspace,
            logicalAgents,
            cancellationToken);
        if (agentsBoundary.State == NoFollowLeafState.Missing)
        {
            return Classified(
                request,
                LibrarySourceRootState.Invalid,
                lexicalSource,
                physicalSource,
                lexicallyContained,
                physicallyContained,
                physicallyDisjoint: null,
                "The Library source root does not contain its mandatory ordinary .agents directory.",
                LibrarySourceRootCondition.RequiredAgentsMissing);
        }

        if (agentsBoundary.State != NoFollowLeafState.Directory)
        {
            return FromBoundary(
                request,
                lexicalSource,
                agentsBoundary,
                lexicallyContained,
                physicalSource,
                physicallyContained);
        }

        var physicalAgents = LibraryDirectoryBoundaryObserver.PhysicalPath(
            workspace,
            logicalAgents);
        if (!PhysicalContainment.Contains(physicalSource, physicalAgents))
        {
            return Classified(
                request,
                LibrarySourceRootState.Blocked,
                lexicalSource,
                physicalSource,
                lexicallyContained,
                physicallyContained,
                physicallyDisjoint: null,
                "The Library source .agents directory escaped its source root.");
        }

        var logicalConsumerAgents = Path.Combine(workspace.LexicalRoot, ".agents");
        var consumerBoundary = LibraryDirectoryBoundaryObserver.Observe(
            workspace,
            logicalConsumerAgents,
            cancellationToken);
        if (consumerBoundary.State is not (NoFollowLeafState.Directory
            or NoFollowLeafState.Missing))
        {
            return Classified(
                request,
                consumerBoundary.State is NoFollowLeafState.Inaccessible
                    or NoFollowLeafState.Unknown
                    ? LibrarySourceRootState.Unavailable
                    : LibrarySourceRootState.Blocked,
                lexicalSource,
                physicalSource,
                lexicallyContained,
                physicallyContained,
                physicallyDisjoint: null,
                consumerBoundary.Failure?.DirectCause
                    ?? "The consumer .agents boundary is not a real ordinary directory.");
        }

        var consumerAgents = LibraryDirectoryBoundaryObserver.PhysicalPath(
            workspace,
            logicalConsumerAgents);
        var disjoint = !PhysicalContainment.Contains(consumerAgents, physicalAgents)
            && !PhysicalContainment.Contains(physicalAgents, consumerAgents);
        if (!disjoint)
        {
            return Classified(
                request,
                LibrarySourceRootState.Blocked,
                lexicalSource,
                physicalSource,
                lexicallyContained,
                physicallyContained,
                physicallyDisjoint: false,
                "The Library source and consumer .agents trees are not physically disjoint.");
        }

        return new LibrarySourceRootObservation
        {
            Request = request,
            State = LibrarySourceRootState.Available,
            LexicalSourceRoot = lexicalSource,
            PhysicalSourceRoot = physicalSource,
            LexicallyContained = true,
            PhysicallyContained = physicallyContained,
            PhysicalAgentsDirectory = physicalAgents,
            PhysicallyDisjoint = true,
            Condition = LibrarySourceRootCondition.None,
            Cause = null,
        };
    }

    private static LibrarySourceRootObservation FromBoundary(
        LibrarySourceRootRequest request,
        string lexicalSource,
        NoFollowLeafObservation boundary,
        bool lexicallyContained,
        string? physicalSource = null,
        bool? physicallyContained = null)
        => Classified(
            request,
            boundary.State switch
            {
                NoFollowLeafState.Missing => LibrarySourceRootState.Missing,
                NoFollowLeafState.OrdinaryFile or NoFollowLeafState.Directory =>
                    LibrarySourceRootState.Invalid,
                NoFollowLeafState.Inaccessible => LibrarySourceRootState.Inaccessible,
                NoFollowLeafState.Unknown => LibrarySourceRootState.Unavailable,
                NoFollowLeafState.RelativeFileLink
                    or NoFollowLeafState.Link
                    or NoFollowLeafState.ReparsePoint
                    or NoFollowLeafState.Special => LibrarySourceRootState.Blocked,
                _ => throw new ArgumentOutOfRangeException(
                    nameof(boundary),
                    boundary.State,
                    "The no-follow leaf state is not defined."),
            },
            lexicalSource,
            physicalSource,
            lexicallyContained,
            physicallyContained,
            physicallyDisjoint: null,
            boundary.Failure?.DirectCause
                ?? "The Library source boundary is missing, non-directory, or unsafe.");

    private static LibrarySourceRootObservation Classified(
        LibrarySourceRootRequest request,
        LibrarySourceRootState state,
        string? lexicalSource,
        string? physicalSource,
        bool? lexicallyContained,
        bool? physicallyContained,
        bool? physicallyDisjoint,
        string cause,
        LibrarySourceRootCondition condition = LibrarySourceRootCondition.None)
        => new()
        {
            Request = request,
            State = state,
            LexicalSourceRoot = lexicalSource,
            PhysicalSourceRoot = physicalSource,
            LexicallyContained = lexicallyContained,
            PhysicallyContained = physicallyContained,
            PhysicalAgentsDirectory = null,
            PhysicallyDisjoint = physicallyDisjoint,
            Condition = condition,
            Cause = cause,
        };
}
