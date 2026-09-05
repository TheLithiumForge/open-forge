using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

internal sealed record RouteRemoveSource
{
    public required string Requested { get; init; }

    public RouteRemoveSourceSelection? SelectedBy { get; init; }

    public string? Id { get; init; }

    public string? Path { get; init; }

    public RouteRemoveSourceForm? Form { get; init; }
}

internal sealed record RouteRemoveSubjectLayer
{
    public required RouteRemoveLayerKind Layer { get; init; }

    public required string SourcePath { get; init; }
}

internal sealed record RouteRemoveSubjectItem
{
    public required RouteRemoveItemKind Kind { get; init; }

    public RouteRemoveLayerKind? Layer { get; init; }

    public string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string RelativePath { get; init; }
}

internal sealed record RouteRemoveSubject
{
    public RouteRemoveSubjectKind? Kind { get; init; }

    public ImmutableArray<RouteRemoveSubjectLayer> Layers { get; init; } = [];

    public ImmutableArray<RouteRemoveSubjectItem> Items { get; init; } = [];
}

internal sealed record RouteRemoveOwnershipClaim
{
    public required string Path { get; init; }

    public required RouteRemoveOwnershipManager Manager { get; init; }

    public required string Owner { get; init; }
}

internal sealed record RouteRemoveOwnership
{
    public required RouteRemoveOwnershipState State { get; init; }

    public required RouteRemoveOwnershipTrust Framework { get; init; }

    public required RouteRemoveOwnershipTrust Extensions { get; init; }

    public ImmutableArray<RouteRemoveOwnershipClaim> Claims { get; init; } = [];
}

internal sealed record RouteRemoveReferenceDetachment
{
    public required string SourcePath { get; init; }

    public RouteRemoveLayerKind? Layer { get; init; }

    public required SourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }

    public required string OriginalDestination { get; init; }

    public required string VisibleLabel { get; init; }
}

internal sealed record RouteRemoveReferences
{
    public required RouteRemoveCoverage Coverage { get; init; }

    public required int ScannedSourceCount { get; init; }

    public required int InspectedSourceCount { get; init; }

    public required int OccurrenceCount { get; init; }

    public ImmutableArray<RouteRemoveReferenceDetachment> Detachments { get; init; } = [];
}

internal sealed record RouteRemoveGeneratedRegion
{
    public required string Path { get; init; }

    public ImmutableArray<RouteRemoveGeneratedReason> Reasons { get; init; } = [];

    public required RouteRemoveGeneratedState State { get; init; }
}

internal sealed record RouteRemoveGeneratedNavigation
{
    public required RouteRemoveCoverage Coverage { get; init; }

    public ImmutableArray<RouteRemoveGeneratedRegion> Regions { get; init; } = [];
}

internal sealed record RouteRemovePlanFacts
{
    public required RouteRemovePlanCompleteness Completeness { get; init; }

    public required RouteRemovePlanSafety Safety { get; init; }
}

internal sealed record RouteRemovePathState
{
    internal RouteRemovePathState(
        RouteRemovePathStateKind kind,
        string? contentSha256)
    {
        if (!Enum.IsDefined(kind))
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "The Route Remove path-state kind is not defined.");
        }

        if (kind == RouteRemovePathStateKind.File)
        {
            ValidateSha256(contentSha256);
        }
        else if (contentSha256 is not null)
        {
            throw new ArgumentException(
                "Only a Route Remove file state can carry a content fingerprint.",
                nameof(contentSha256));
        }

        Kind = kind;
        ContentSha256 = contentSha256;
    }

    public RouteRemovePathStateKind Kind { get; }

    public string? ContentSha256 { get; }

    private static void ValidateSha256(string? contentSha256)
    {
        if (contentSha256 is null
            || contentSha256.Length != 64
            || contentSha256.Any(character => character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A Route Remove file state requires a lowercase SHA-256 fingerprint.",
                nameof(contentSha256));
        }
    }
}

internal sealed record RouteRemoveEffect
{
    public required string Path { get; init; }

    public required RouteRemoveEffectKind Kind { get; init; }

    public required RouteRemoveEffectAction Action { get; init; }

    public required RouteRemovePathState Before { get; init; }

    public required RouteRemovePathState Expected { get; init; }

    public required RouteRemoveEffectOutcome Outcome { get; init; }

    public required RouteRemoveEffectResidual Residual { get; init; }
}

internal sealed record RouteRemoveRecovery
{
    public required RouteRemoveRecoveryState State { get; init; }

    public ImmutableArray<string> ProtectedPaths { get; init; } = [];

    public string? ResidualPath { get; init; }
}
