using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveJsonProjection
{
    private static RouteRemoveJsonReferences References(RouteRemoveReferences references)
        => new()
        {
            Coverage = RouteRemoveDefinitions.ReadMachineName(references.Coverage),
            ScannedSourceCount = references.ScannedSourceCount,
            InspectedSourceCount = references.InspectedSourceCount,
            OccurrenceCount = references.OccurrenceCount,
            Detachments = references.Detachments.Select(ReferenceDetachment).ToArray(),
        };

    private static RouteRemoveJsonReferenceDetachment ReferenceDetachment(
        RouteRemoveReferenceDetachment detachment)
        => new()
        {
            SourcePath = detachment.SourcePath,
            Layer = detachment.Layer is { } layer
                ? RouteRemoveDefinitions.ReadMachineName(layer)
                : null,
            Location = SourceLocation(detachment.Location),
            Before = detachment.Before,
            Expected = detachment.Expected,
            OriginalDestination = detachment.OriginalDestination,
            VisibleLabel = detachment.VisibleLabel,
        };

    private static RouteRemoveJsonSourceLocation SourceLocation(SourceLocation location)
        => new()
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };

    private static RouteRemoveJsonGeneratedNavigation GeneratedNavigation(
        RouteRemoveGeneratedNavigation navigation)
        => new()
        {
            Coverage = RouteRemoveDefinitions.ReadMachineName(navigation.Coverage),
            Regions = navigation.Regions.Select(GeneratedRegion).ToArray(),
        };

    private static RouteRemoveJsonGeneratedRegion GeneratedRegion(RouteRemoveGeneratedRegion region)
        => new()
        {
            Path = region.Path,
            Reasons = region.Reasons.Select(RouteRemoveDefinitions.ReadMachineName).ToArray(),
            State = RouteRemoveDefinitions.ReadMachineName(region.State),
        };
}
