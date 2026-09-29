using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextSourceLayerKind
{
    Base,
    Overwrite,
}

internal enum ContextInclusionReasonKind
{
    WorkspaceEntry,
    Loader,
    LoadNow,
    KeepInMind,
    AncestorRequired,
    SelectedSource,
    ScopeLocal,
    LinkedSource,
    OverwriteCompanion,
}

internal sealed record ContextInclusionReason
{
    internal ContextInclusionReason(
        ContextInclusionReasonKind kind,
        ContextSourceIdentity? source,
        string? reference,
        int? depth,
        SourceLocation? location)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "The Context inclusion reason is not defined.");
        }

        if (depth is < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(depth), depth, "A Context link depth must be positive.");
        }

        if (kind == ContextInclusionReasonKind.LinkedSource
            ? depth is null || location is null
            : depth is not null || location is not null)
        {
            throw new ArgumentException("Only a linked-source reason carries depth and location.");
        }

        Kind = kind;
        Source = source;
        Reference = reference;
        Depth = depth;
        Location = location;
    }

    internal ContextInclusionReasonKind Kind { get; }

    internal ContextSourceIdentity? Source { get; }

    internal string? Reference { get; }

    internal int? Depth { get; }

    internal SourceLocation? Location { get; }
}
