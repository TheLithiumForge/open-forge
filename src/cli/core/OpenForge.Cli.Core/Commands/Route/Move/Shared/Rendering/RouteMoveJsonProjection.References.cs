using OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static partial class RouteMoveJsonProjection
{
    private static RouteMoveJsonReferences References(RouteMoveReferences references)
        => new()
        {
            Coverage = RouteMoveDefinitions.ReadMachineName(references.Coverage),
            ScannedSourceCount = references.ScannedSourceCount,
            InspectedSourceCount = references.InspectedSourceCount,
            OccurrenceCount = references.OccurrenceCount,
            Rewrites = references.Rewrites.Select(ReferenceRewrite).ToArray(),
        };

    private static RouteMoveJsonReferenceRewrite ReferenceRewrite(RouteMoveReferenceRewrite rewrite)
        => new()
        {
            SourcePath = rewrite.SourcePath,
            DestinationSourcePath = rewrite.DestinationSourcePath,
            Layer = rewrite.Layer is { } layer
                ? RouteMoveDefinitions.ReadMachineName(layer)
                : null,
            Location = SourceLocation(rewrite.Location),
            Before = rewrite.Before,
            Expected = rewrite.Expected,
            OldTarget = ReferenceTarget(rewrite.OldTarget),
            ExpectedTarget = ReferenceTarget(rewrite.ExpectedTarget),
        };

    private static RouteMoveJsonReferenceTarget ReferenceTarget(RouteMoveReferenceTarget target)
        => new()
        {
            Id = target.Id,
            Path = target.Path,
        };

    private static RouteMoveJsonSourceLocation SourceLocation(SourceLocation location)
        => new()
        {
            Line = location.Line,
            Column = location.Column,
            ByteOffset = location.ByteOffset,
            ByteLength = location.ByteLength,
        };

    private static RouteMoveJsonGeneratedNavigation GeneratedNavigation(
        RouteMoveGeneratedNavigation navigation)
        => new()
        {
            Coverage = RouteMoveDefinitions.ReadMachineName(navigation.Coverage),
            Regions = navigation.Regions.Select(GeneratedRegion).ToArray(),
        };

    private static RouteMoveJsonGeneratedRegion GeneratedRegion(RouteMoveGeneratedRegion region)
        => new()
        {
            Path = region.Path,
            Reasons = region.Reasons.Select(RouteMoveDefinitions.ReadMachineName).ToArray(),
            State = RouteMoveDefinitions.ReadMachineName(region.State),
        };
}
