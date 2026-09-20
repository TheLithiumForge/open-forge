using OpenForge.Cli.Core.Commands.Shared.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextLinkTargetKind
{
    Local,
    External,
    Unsupported,
}

internal enum ContextLinkResolution
{
    Complete,
    Missing,
    FragmentMissing,
    CaseMismatch,
    Malformed,
    Absolute,
    Query,
    EncodingUnsupported,
    OutsideWorkspace,
    PhysicalEscape,
    Ambiguous,
    Unreadable,
    Unsupported,
    ExternalUnchecked,
}

internal enum ContextLinkNetwork
{
    NetworkNotAttempted,
}

internal enum ContextLinkDisposition
{
    Selected,
    AlreadySelected,
    Cycle,
    ExternalUnchecked,
    Unresolved,
}

internal enum ContextLinkFollowState
{
    Followed,
    NotFollowed,
}

internal sealed record ContextLinkSource
{
    public required string? Id { get; init; }

    public required string Path { get; init; }

    public required ContextSourceLayerKind Layer { get; init; }
}

internal sealed record ContextLinkTarget
{
    public required ContextLinkTargetKind Kind { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required ContextSourceLayerKind? Layer { get; init; }

    public required ContextLinkResolution Resolution { get; init; }

    public required ContextLinkNetwork? Network { get; init; }
}

internal sealed record ContextLink
{
    internal ContextLink(
        int depth,
        ContextLinkSource source,
        SourceLocation location,
        SourceLocation? destinationLocation,
        string rawDestination,
        string? fragment,
        ContextLinkTarget target,
        ContextLinkDisposition disposition)
    {
        if (depth < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "A Context link depth must be positive.");
        }

        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(location);
        ArgumentNullException.ThrowIfNull(rawDestination);
        ArgumentNullException.ThrowIfNull(target);
        if (!Enum.IsDefined(disposition))
        {
            throw new ArgumentOutOfRangeException(nameof(disposition), disposition, "The Context link disposition is not defined.");
        }

        Depth = depth;
        Source = source;
        Location = location;
        DestinationLocation = destinationLocation;
        RawDestination = rawDestination;
        Fragment = fragment;
        Target = target;
        Disposition = disposition;
    }

    internal int Depth { get; }

    internal ContextLinkSource Source { get; }

    internal SourceLocation Location { get; }

    internal CommandSourceLocation LocationView => CommandSourceLocation.From(Location);

    internal SourceLocation? DestinationLocation { get; }

    internal string RawDestination { get; }

    internal string? Fragment { get; }

    internal ContextLinkTarget Target { get; }

    internal ContextLinkDisposition Disposition { get; }

    internal ContextLinkFollowState FollowState
        => Disposition switch
        {
            ContextLinkDisposition.Selected
                or ContextLinkDisposition.AlreadySelected
                or ContextLinkDisposition.Cycle => ContextLinkFollowState.Followed,
            ContextLinkDisposition.ExternalUnchecked
                or ContextLinkDisposition.Unresolved => ContextLinkFollowState.NotFollowed,
            _ => throw new ArgumentOutOfRangeException(nameof(Disposition), Disposition, "The Context link disposition is not defined."),
        };
}
