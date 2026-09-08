using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.Commands.Library.Models.Planning;

internal sealed record LibraryConsumerBoundaryRequest
{
    public required CliWorkspace Workspace { get; init; }
    public required ImmutableArray<CanonicalRelativePath> RequiredAncestorPaths { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Workspace);
        if (RequiredAncestorPaths.IsDefault || RequiredAncestorPaths.Any(path => path is null))
        {
            throw new ArgumentException("Consumer boundary requests require initialized ancestor paths.", nameof(RequiredAncestorPaths));
        }
    }
}

internal sealed record LibraryConsumerDirectoryObservation
{
    internal LibraryConsumerDirectoryObservation(NoFollowLeafObservation leaf, PhysicalPathResolution containment)
    {
        ArgumentNullException.ThrowIfNull(leaf);
        ArgumentNullException.ThrowIfNull(containment);
        if (!Path.IsPathFullyQualified(containment.LogicalPath)
            || !PhysicalIdentityTracker.PathComparer.Equals(leaf.LogicalPath, Path.GetFullPath(containment.LogicalPath)))
        {
            throw new ArgumentException("Consumer leaf and containment observations must name the same absolute target.", nameof(containment));
        }

        Leaf = leaf;
        Containment = containment;
    }

    internal NoFollowLeafObservation Leaf { get; }
    internal PhysicalPathResolution Containment { get; }
}

internal sealed record LibraryConsumerBoundaryFacts
{
    public required LibraryConsumerBoundaryRequest Request { get; init; }
    public required LibraryConsumerDirectoryObservation? ConsumerRoot { get; init; }
    public required ImmutableArray<LibraryConsumerDirectoryObservation> Ancestors { get; init; }
    public required bool IsComplete { get; init; }

    internal void Validate(CliWorkspace workspace)
    {
        ArgumentNullException.ThrowIfNull(workspace);
        ArgumentNullException.ThrowIfNull(Request);
        Request.Validate();
        if (Request.Workspace != workspace)
        {
            throw new ArgumentException("Consumer boundary facts must belong to the selected workspace.", nameof(workspace));
        }

        if (Ancestors.IsDefault || Ancestors.Any(observation => observation is null))
        {
            throw new ArgumentException("Consumer boundary facts require initialized ancestor observations.");
        }
    }
}
