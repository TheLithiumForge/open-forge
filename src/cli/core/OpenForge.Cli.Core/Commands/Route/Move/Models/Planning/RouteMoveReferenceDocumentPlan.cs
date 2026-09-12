using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;
using OpenForge.Cli.Core.Framework.Documents.Markdown.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;

internal sealed record RouteMoveReferenceDocument
{
    public required string Text { get; init; }

    public required MarkdownDocumentFacts Facts { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }
}

internal sealed record RouteMoveReferenceReplacement
{
    public required MarkdownTextSpan Span { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }

    public required string OldTarget { get; init; }

    public required string NewTarget { get; init; }

    public SourceLayerKind? Layer { get; init; }
}

internal sealed record RouteMoveReferenceTargetMove
{
    public required string OldPath { get; init; }

    public required string NewPath { get; init; }
}

internal sealed record RouteMoveReferenceReplacementProjection
{
    public RouteMoveReferenceReplacement? Replacement { get; init; }

    public RouteMoveReferenceMeaning? Meaning { get; init; }

    public RouteMoveResultFormation? Boundary { get; init; }
}

internal sealed record RouteMoveReferenceDocumentPlan
{
    public required string SourcePath { get; init; }

    public required string DestinationSourcePath { get; init; }

    public required FileStateSnapshot Snapshot { get; init; }

    public required string IntendedText { get; init; }

    public ImmutableArray<RouteMoveReferenceDocumentEdit> Edits { get; init; } = [];

    public ImmutableArray<RouteMoveReferenceMeaning> Meanings { get; init; } = [];
}

internal sealed record RouteMoveReferenceMeaning
{
    public required string? TargetId { get; init; }

    public required string TargetPath { get; init; }

    public required SourceLayerKind? Layer { get; init; }

    public required SourceLinkTargetResolution Resolution { get; init; }

    public required string? Fragment { get; init; }
}

internal sealed record RouteMoveReferenceDocumentEdit
{
    public required SourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }
}

internal sealed record RouteMoveReferenceDocumentInspection
{
    public required RouteMoveReferenceDocumentPlan Document { get; init; }

    public ImmutableArray<RouteMoveReferenceRewrite> Rewrites { get; init; } = [];

    public required int OccurrenceCount { get; init; }
}

internal sealed record RouteMoveReferenceReplacementScan
{
    public ImmutableArray<RouteMoveReferenceReplacement> Replacements { get; init; } = [];

    public ImmutableArray<RouteMoveReferenceMeaning> Meanings { get; init; } = [];

    public required int OccurrenceCount { get; init; }
}

internal sealed record RouteMoveReferenceReplacementScanResult
{
    internal RouteMoveReferenceReplacementScanResult(
        RouteMoveReferenceReplacementScan? scan,
        RouteMoveResultFormation? boundary)
    {
        if ((scan is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move reference replacement scanning requires exactly one scan or boundary.");
        }

        Scan = scan;
        Boundary = boundary;
    }

    internal RouteMoveReferenceReplacementScan? Scan { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}

internal sealed record RouteMoveReferenceDocumentInspectionResult
{
    internal RouteMoveReferenceDocumentInspectionResult(
        RouteMoveReferenceDocumentInspection? inspection,
        RouteMoveResultFormation? boundary)
    {
        if ((inspection is null) == (boundary is null))
        {
            throw new ArgumentException(
                "Route Move reference inspection requires exactly one document or boundary.");
        }

        Inspection = inspection;
        Boundary = boundary;
    }

    internal RouteMoveReferenceDocumentInspection? Inspection { get; }

    internal RouteMoveResultFormation? Boundary { get; }
}
