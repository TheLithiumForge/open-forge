using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Templates.Models;

internal enum RouteTemplateResolutionState
{
    Resolved,
    Invalid,
    Blocked,
    Incomplete,
}

internal enum RouteTemplateResolutionIssue
{
    InvalidReference,
    InvalidSourceKind,
    OverwriteUnsafe,
    MetadataUnsafe,
    MissingClassification,
    BodyBoundaryUnsafe,
    SourceUnavailable,
    SourceUnsafe,
    ResolutionInterrupted,
    ReadInterrupted,
}

internal sealed record RouteTemplateSelection
{
    public required string Requested { get; init; }

    public required string Id { get; init; }

    public required string Path { get; init; }

    public required long BodyByteLength { get; init; }
}

internal sealed record RouteTemplateResolution
{
    public required RouteTemplateResolutionState State { get; init; }

    public RouteTemplateSelection? Template { get; init; }

    public SourceLogicalSource? Source { get; init; }

    public required ImmutableArray<byte> BodyBytes { get; init; }

    public RouteTemplateResolutionIssue? Issue { get; init; }

    public string? Cause { get; init; }

    public string? Target { get; init; }
}
