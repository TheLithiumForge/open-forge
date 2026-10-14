using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models.Syntax;

namespace OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Completion.Models;

internal enum FrameworkMetadataCompletionState
{
    Completed,
    NoOp,
    Malformed,
    Unsafe,
}

internal sealed record FrameworkMetadataCompletionResult
{
    public required FrameworkMetadataCompletionState State { get; init; }
    public required ImmutableArray<byte> Bytes { get; init; }
    public required ImmutableArray<FrameworkMetadataField> Fields { get; init; }
    internal FrontmatterForm? Form { get; init; }
    internal string? Description { get; init; }
    internal ImmutableArray<string>? Tags { get; init; }
    internal string? Cause { get; init; }

    internal static FrameworkMetadataCompletionResult Unchanged(ImmutableArray<byte> bytes)
        => new() { State = FrameworkMetadataCompletionState.NoOp, Bytes = bytes, Fields = [] };
}
