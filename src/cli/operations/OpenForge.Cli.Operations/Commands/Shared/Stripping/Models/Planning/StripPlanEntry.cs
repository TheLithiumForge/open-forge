using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Shared.Stripping.Models.Planning;

internal enum StripPlanAction
{
    Delete,
    Rewrite,
    Move,
    DeleteDirectory,
}

internal enum StripPlanReason
{
    Framework,
    Extension,
    RootHost,
    Scaffold,
    State,
    Strip,
    Flatten,
    EmptyFolder,
}

internal enum StripRewritePart
{
    Metadata,
    Entries,
    Links,
    Relink,
    ManagedSection,
}

internal enum StripEffectState
{
    Planned,
    NotStarted,
    Done,
    Unknown,
    Failed,
}

/// <summary>One reviewed plan line. Paths are canonical workspace-relative paths.</summary>
internal sealed record StripPlanEntry
{
    public required StripPlanAction Action { get; init; }

    public required string Path { get; init; }

    public string? To { get; init; }

    public required StripPlanReason Reason { get; init; }

    public ImmutableArray<StripRewritePart> Parts { get; init; } = [];

    public string? Before { get; init; }

    public string? After { get; init; }

    public StripEffectState State { get; init; } = StripEffectState.Planned;

    internal bool SameAs(StripPlanEntry other)
        => Action == other.Action && Path == other.Path && To == other.To && Reason == other.Reason
            && Parts.SequenceEqual(other.Parts) && Before == other.Before && After == other.After;
}
