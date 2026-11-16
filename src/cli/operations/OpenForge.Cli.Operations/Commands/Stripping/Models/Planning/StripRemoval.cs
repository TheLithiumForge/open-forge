using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Stripping.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

namespace OpenForge.Cli.Core.Commands.Stripping.Models.Planning;

/// <summary>A whole-file deletion or exact replacement chosen before strip plans its edits.</summary>
internal sealed record StripRemoval
{
    public required string Path { get; init; }

    public required StripPlanReason Reason { get; init; }

    public required FileStateSnapshot Before { get; init; }

    // Null deletes the file. Otherwise the host keeps these exact bytes.
    public ImmutableArray<byte>? After { get; init; }
}

internal sealed record StripRemovalSet
{
    internal static StripRemovalSet Empty { get; } = new();

    public ImmutableArray<StripRemoval> Removals { get; init; } = [];

    public ImmutableArray<StripKeptFile> Kept { get; init; } = [];

    public ImmutableArray<StripFinding> Findings { get; init; } = [];

    // A blocking finding stops planning before strip plans any edit.
    public StripFinding? Stop { get; init; }
}
