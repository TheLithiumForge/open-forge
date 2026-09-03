using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

internal sealed record RouteMoveSource
{
    public required string Requested { get; init; }

    public RouteMoveSourceSelection? SelectedBy { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }

    public RouteMoveSourceForm? Form { get; init; }
}

internal sealed record RouteMoveDestination
{
    public required string Requested { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }

    public string? ParentId { get; init; }

    public string? ParentPath { get; init; }
}

internal sealed record RouteMoveSubjectLayer
{
    public required RouteMoveLayerKind Layer { get; init; }

    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }
}

internal sealed record RouteMoveSubjectItem
{
    public required RouteMoveItemKind Kind { get; init; }

    public RouteMoveLayerKind? Layer { get; init; }

    public string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string DestinationPath { get; init; }
}

internal sealed record RouteMoveSubject
{
    public RouteMoveSubjectKind? Kind { get; init; }

    public ImmutableArray<RouteMoveSubjectLayer> Layers { get; init; } = [];

    public ImmutableArray<RouteMoveSubjectItem> Items { get; init; } = [];
}

internal sealed record RouteMoveOwnershipClaim
{
    public required string Path { get; init; }

    public required RouteMoveOwnershipManager Manager { get; init; }

    public required string Owner { get; init; }
}

internal sealed record RouteMoveOwnership
{
    public required RouteMoveOwnershipState State { get; init; }

    public required RouteMoveOwnershipTrust Framework { get; init; }

    public required RouteMoveOwnershipTrust Extensions { get; init; }

    public ImmutableArray<RouteMoveOwnershipClaim> Claims { get; init; } = [];
}

internal sealed record RouteMoveReferenceTarget
{
    public string? Id { get; init; }

    public required string Path { get; init; }
}

internal sealed record RouteMoveReferenceRewrite
{
    public required string SourcePath { get; init; }

    public required string DestinationSourcePath { get; init; }

    public RouteMoveLayerKind? Layer { get; init; }

    public required SourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }

    public required RouteMoveReferenceTarget OldTarget { get; init; }

    public required RouteMoveReferenceTarget ExpectedTarget { get; init; }
}

internal sealed record RouteMoveReferences
{
    public required RouteMoveCoverage Coverage { get; init; }

    public required int ScannedSourceCount { get; init; }

    public required int InspectedSourceCount { get; init; }

    public required int OccurrenceCount { get; init; }

    public ImmutableArray<RouteMoveReferenceRewrite> Rewrites { get; init; } = [];
}

internal sealed record RouteMoveGeneratedRegion
{
    public required string Path { get; init; }

    public ImmutableArray<RouteMoveGeneratedReason> Reasons { get; init; } = [];

    public required RouteMoveGeneratedState State { get; init; }
}

internal sealed record RouteMoveGeneratedNavigation
{
    public required RouteMoveCoverage Coverage { get; init; }

    public ImmutableArray<RouteMoveGeneratedRegion> Regions { get; init; } = [];
}

internal sealed record RouteMovePlanFacts
{
    public required RouteMovePlanCompleteness Completeness { get; init; }

    public required RouteMovePlanSafety Safety { get; init; }
}

internal sealed record RouteMovePathState
{
    internal RouteMovePathState(
        RouteMovePathStateKind kind,
        string? contentSha256)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Route Move path-state kind is not defined.");
        }

        if (kind == RouteMovePathStateKind.File)
        {
            ValidateSha256(contentSha256);
        }
        else if (contentSha256 is not null)
        {
            throw new ArgumentException(
                "Only a Route Move file state can carry a content fingerprint.",
                nameof(contentSha256));
        }

        Kind = kind;
        ContentSha256 = contentSha256;
    }

    public RouteMovePathStateKind Kind { get; }

    public string? ContentSha256 { get; }

    private static void ValidateSha256(string? contentSha256)
    {
        if (contentSha256 is null
            || contentSha256.Length != 64
            || contentSha256.Any(character => character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A Route Move file state requires a lowercase SHA-256 fingerprint.",
                nameof(contentSha256));
        }
    }
}

internal sealed record RouteMoveEffect
{
    public required string Path { get; init; }

    public required RouteMoveEffectKind Kind { get; init; }

    public required RouteMoveEffectAction Action { get; init; }

    public required RouteMovePathState Before { get; init; }

    public required RouteMovePathState Expected { get; init; }

    public required RouteMoveEffectOutcome Outcome { get; init; }

    public required RouteMoveEffectResidual Residual { get; init; }
}

internal sealed record RouteMoveRecovery
{
    public required RouteMoveRecoveryState State { get; init; }

    public ImmutableArray<string> ProtectedPaths { get; init; } = [];

    public string? ResidualPath { get; init; }
}
