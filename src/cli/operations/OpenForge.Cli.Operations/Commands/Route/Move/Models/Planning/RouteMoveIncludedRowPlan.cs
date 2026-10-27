using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Loading;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveIncludedRowPlan
{
    public required string IntendedText { get; init; }
    public ImmutableArray<RouteMoveReferenceDocumentEdit> Edits { get; init; } = [];
    public ImmutableArray<SourceGeneratedEntry> OriginalEntries { get; init; } = [];
    public ImmutableArray<SourceGeneratedEntry> ExpectedEntries { get; init; } = [];
    public ImmutableArray<RouteMoveReferenceMeaning> Meanings { get; init; } = [];
}

internal sealed record RouteMoveIncludedRowProjection(RouteMoveIncludedRowPlan? Plan, RouteMoveResultFormation? Boundary);
