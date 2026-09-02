using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Framework.Documents.Yaml.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;

internal sealed record RouteUpdateMetadataLayout
{
    public required string Source { get; init; }
    public required string? Description { get; init; }
    public required ImmutableArray<string> Tags { get; init; }
    public required string? Responsibility { get; init; }
    public required RouteUpdateMetadataMember? DescriptionMember { get; init; }
    public required RouteUpdateMetadataMember? ResponsibilityMember { get; init; }
    public required RouteUpdateMetadataMember? TagsMember { get; init; }
}

internal sealed record RouteUpdateMetadataMember
{
    public required string Name { get; init; }
    public required YamlMappingEntry Entry { get; init; }
    public required RouteUpdateMetadataMemberLine? Line { get; init; }
}

internal sealed record RouteUpdateMetadataMemberLine
{
    public required int Start { get; init; }
    public required int ContentEnd { get; init; }
    public required int End { get; init; }
    public required string Indentation { get; init; }
    public required string RawValue { get; init; }
}

internal sealed class RouteUpdateMetadataLayoutRead
{
    private RouteUpdateMetadataLayoutRead(
        RouteUpdateMetadataLayout? layout,
        string? cause)
    {
        if ((layout is null) == (cause is null))
        {
            throw new ArgumentException(
                "A metadata layout read requires either one layout or one unsafe cause.");
        }

        Layout = layout;
        Cause = cause;
    }

    public RouteUpdateMetadataLayout? Layout { get; }
    public string? Cause { get; }

    internal static RouteUpdateMetadataLayoutRead Complete(RouteUpdateMetadataLayout layout)
        => new(layout, cause: null);

    internal static RouteUpdateMetadataLayoutRead Unsafe(string cause)
        => new(layout: null, cause);
}

internal sealed record RouteUpdateMetadataEditPlanningInput
{
    public required RouteUpdateMetadataLayout Layout { get; init; }
    public required RouteUpdatePatchRequest Request { get; init; }
}

internal sealed record RouteUpdateCanonicalMetadataValues
{
    public required string Description { get; init; }
    public required string Tags { get; init; }
    public required string? Responsibility { get; init; }
}
