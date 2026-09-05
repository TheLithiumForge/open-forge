using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;

internal sealed record RouteRemoveJsonDocument
{
    public required int SchemaVersion { get; init; }

    public required string Command { get; init; }

    public required string Status { get; init; }

    public required RouteRemoveJsonWorkspace? Workspace { get; init; }

    public required RouteRemoveJsonResult Result { get; init; }

    public required RouteRemoveJsonNext? Next { get; init; }
}

internal sealed record RouteRemoveJsonWorkspace
{
    public required string Path { get; init; }

    public required string SelectedBy { get; init; }
}

internal sealed record RouteRemoveJsonResult
{
    public required string Mode { get; init; }

    public required RouteRemoveJsonSource Source { get; init; }

    public required RouteRemoveJsonSubject Subject { get; init; }

    public required RouteRemoveJsonOwnership Ownership { get; init; }

    public required RouteRemoveJsonReferences References { get; init; }

    public required RouteRemoveJsonGeneratedNavigation GeneratedNavigation { get; init; }

    public required RouteRemoveJsonPlan Plan { get; init; }

    public required RouteRemoveJsonEffect[] Effects { get; init; }

    public required string[] UnchangedPaths { get; init; }

    public required RouteRemoveJsonRecovery Recovery { get; init; }

    public required string Verification { get; init; }

    public required RouteRemoveJsonFinding[] Findings { get; init; }
}

internal sealed record RouteRemoveJsonSource
{
    public required string Requested { get; init; }

    public required string? SelectedBy { get; init; }

    public required string? Id { get; init; }

    public required string? Path { get; init; }

    public required string? Form { get; init; }
}

internal sealed record RouteRemoveJsonSubject
{
    public required string? Kind { get; init; }

    public required RouteRemoveJsonSubjectLayer[] Layers { get; init; }

    public required RouteRemoveJsonSubjectItem[] Items { get; init; }
}

internal sealed record RouteRemoveJsonSubjectLayer
{
    public required string Layer { get; init; }

    public required string SourcePath { get; init; }
}

internal sealed record RouteRemoveJsonSubjectItem
{
    public required string Kind { get; init; }

    public required string? Layer { get; init; }

    public required string? SourceId { get; init; }

    public required string SourcePath { get; init; }

    public required string RelativePath { get; init; }
}

internal sealed record RouteRemoveJsonOwnership
{
    public required string State { get; init; }

    public required string Framework { get; init; }

    public required string Extensions { get; init; }

    public required RouteRemoveJsonOwnershipClaim[] Claims { get; init; }
}

internal sealed record RouteRemoveJsonOwnershipClaim
{
    public required string Path { get; init; }

    public required string Manager { get; init; }

    public required string Owner { get; init; }
}

internal sealed record RouteRemoveJsonReferences
{
    public required string Coverage { get; init; }

    public required int ScannedSourceCount { get; init; }

    public required int InspectedSourceCount { get; init; }

    public required int OccurrenceCount { get; init; }

    public required RouteRemoveJsonReferenceDetachment[] Detachments { get; init; }
}

internal sealed record RouteRemoveJsonReferenceDetachment
{
    public required string SourcePath { get; init; }

    public required string? Layer { get; init; }

    public required RouteRemoveJsonSourceLocation Location { get; init; }

    public required string Before { get; init; }

    public required string Expected { get; init; }

    public required string OriginalDestination { get; init; }

    public required string VisibleLabel { get; init; }
}

internal sealed record RouteRemoveJsonSourceLocation
{
    public required int Line { get; init; }

    public required int Column { get; init; }

    public required long ByteOffset { get; init; }

    public required long ByteLength { get; init; }
}

internal sealed record RouteRemoveJsonGeneratedNavigation
{
    public required string Coverage { get; init; }

    public required RouteRemoveJsonGeneratedRegion[] Regions { get; init; }
}

internal sealed record RouteRemoveJsonGeneratedRegion
{
    public required string Path { get; init; }

    public required string[] Reasons { get; init; }

    public required string State { get; init; }
}

internal sealed record RouteRemoveJsonPlan
{
    public required string Completeness { get; init; }

    public required string Safety { get; init; }
}

internal sealed record RouteRemoveJsonEffect
{
    public required string Path { get; init; }

    public required string Kind { get; init; }

    public required string Action { get; init; }

    public required RouteRemoveJsonPathState Before { get; init; }

    public required RouteRemoveJsonPathState Expected { get; init; }

    public required string Outcome { get; init; }

    public required string Residual { get; init; }
}

internal sealed record RouteRemoveJsonPathState
{
    public required string Kind { get; init; }

    public required string? ContentSha256 { get; init; }
}

internal sealed record RouteRemoveJsonRecovery
{
    public required string State { get; init; }

    public required string[] ProtectedPaths { get; init; }

    public required string? ResidualPath { get; init; }
}

internal sealed record RouteRemoveJsonFinding
{
    public required string Code { get; init; }

    public required string Status { get; init; }

    public required string? Target { get; init; }

    public required string Cause { get; init; }
}

internal sealed record RouteRemoveJsonNext
{
    public required string Command { get; init; }

    public required string Reason { get; init; }
}

[JsonSourceGenerationOptions(
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Serialization)]
[JsonSerializable(typeof(RouteRemoveJsonDocument))]
internal sealed partial class RouteRemoveJsonContext : JsonSerializerContext;
