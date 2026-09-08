using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Shared.Paths;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Framework.Libraries.Models.Observation;

internal sealed record LibraryMapping
{
    private LibraryMapping(
        SourceRelativeEligiblePath sourcePath,
        WorkspaceRelativeEligiblePath destinationPath,
        RawRelativeLinkTarget expectedRelativeLink)
    {
        SourcePath = sourcePath;
        DestinationPath = destinationPath;
        ExpectedRelativeLink = expectedRelativeLink;
    }

    internal SourceRelativeEligiblePath SourcePath { get; }

    internal WorkspaceRelativeEligiblePath DestinationPath { get; }

    internal RawRelativeLinkTarget ExpectedRelativeLink { get; }

    internal static LibraryMapping Create(
        WorkspaceRelativeDirectory sourceRoot,
        SourceRelativeEligiblePath sourcePath)
    {
        ArgumentNullException.ThrowIfNull(sourceRoot);
        ArgumentNullException.ThrowIfNull(sourcePath);
        var destination = WorkspaceRelativeEligiblePath.Create(sourcePath.Value);
        var source = $"{sourceRoot.Value}/{sourcePath.Value}";
        var parent = Path.GetDirectoryName(
                destination.Value.Replace('/', Path.DirectorySeparatorChar))
            ?? throw new ArgumentException(
                "An eligible destination must have a parent directory.",
                nameof(sourcePath));
        var target = Path.GetRelativePath(
            parent,
            source.Replace('/', Path.DirectorySeparatorChar));
        return new LibraryMapping(
            sourcePath,
            destination,
            RawRelativeLinkTarget.Create(target.Replace(
                Path.DirectorySeparatorChar,
                '/')));
    }
}

internal enum LibraryMappingObservationState
{
    Current,
    Missing,
    Changed,
    Blocked,
    Unavailable,
}

internal sealed record LibraryMappingObservation
{
    private LibraryMappingObservation(
        string logicalDestinationPath,
        LibraryMapping mapping,
        NoFollowLeafObservation leaf,
        LibraryMappingObservationState state,
        string? cause)
    {
        LogicalDestinationPath = PortableRelativePath.Validate(
            logicalDestinationPath,
            nameof(logicalDestinationPath),
            requireAgentsPrefix: true);
        Mapping = mapping;
        ExpectedLink = RelativeFileLinkIdentity.Create(
            NoFollowLinkKind.SymbolicLink,
            mapping.ExpectedRelativeLink.Value);
        Leaf = leaf;
        State = state;
        Cause = cause;
    }

    internal string LogicalDestinationPath { get; }

    internal LibraryMapping Mapping { get; }

    internal RelativeFileLinkIdentity ExpectedLink { get; }

    internal NoFollowLeafObservation Leaf { get; }

    internal LibraryMappingObservationState State { get; }

    internal string? Cause { get; }

    internal static LibraryMappingObservation Create(
        LibraryMapping mapping,
        NoFollowLeafObservation leaf,
        LibraryMappingObservationState state,
        string? cause = null)
    {
        ArgumentNullException.ThrowIfNull(mapping);
        ArgumentNullException.ThrowIfNull(leaf);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The mapping observation state is not defined.");
        }

        if (state is LibraryMappingObservationState.Blocked
            or LibraryMappingObservationState.Unavailable)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        }
        else if (cause is not null)
        {
            throw new ArgumentException(
                "Only blocked or unavailable mapping observations carry a cause.",
                nameof(cause));
        }

        return new LibraryMappingObservation(
            mapping.DestinationPath.Value,
            mapping,
            leaf,
            state,
            cause);
    }
}
