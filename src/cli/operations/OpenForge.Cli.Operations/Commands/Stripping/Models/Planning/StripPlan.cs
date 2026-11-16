using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Request;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

/// <summary>One Markdown file below an inventory root, read once as exact bytes.</summary>
internal sealed record StripDocument(string Path, FileStateSnapshot Snapshot, string Source);

internal sealed record StripFileEffect(PlannedFileChange Change, FileStateSnapshot Before, int Entry);

internal sealed record StripDirectoryEffect(FileExpectation Expectation, int Entry);

internal sealed record StripPlan
{
    public required StripRequest Request { get; init; }

    public required WorkspaceLayoutRead LayoutRead { get; init; }

    public required ImmutableArray<StripPlanEntry> Entries { get; init; }

    // Application order: move destinations, rewrites, deletions, then empty folders.
    public required ImmutableArray<StripFileEffect> Files { get; init; }

    public required ImmutableArray<StripDirectoryEffect> Directories { get; init; }

    public ImmutableArray<StripKeptFile> Kept { get; init; } = [];

    public ImmutableArray<StripFinding> Findings { get; init; } = [];

    internal bool IsNoOp => Entries.IsEmpty;

    internal bool SameAs(StripPlan other)
        => Entries.Length == other.Entries.Length && Entries.Zip(other.Entries).All(pair => pair.First.SameAs(pair.Second));
}

internal sealed record StripPlanBuild(StripResult? Stopped, StripPlan? Plan);
