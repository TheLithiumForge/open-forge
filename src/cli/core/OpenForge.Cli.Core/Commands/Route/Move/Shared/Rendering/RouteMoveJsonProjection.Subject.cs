using OpenForge.Cli.Core.Commands.Route.Move.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Rendering;

internal static partial class RouteMoveJsonProjection
{
    private static RouteMoveJsonSubject Subject(RouteMoveSubject subject)
        => new()
        {
            Kind = subject.Kind is { } kind
                ? RouteMoveDefinitions.ReadMachineName(kind)
                : null,
            Layers = subject.Layers.Select(SubjectLayer).ToArray(),
            Items = subject.Items.Select(SubjectItem).ToArray(),
        };

    private static RouteMoveJsonSubjectLayer SubjectLayer(RouteMoveSubjectLayer layer)
        => new()
        {
            Layer = RouteMoveDefinitions.ReadMachineName(layer.Layer),
            SourcePath = layer.SourcePath,
            DestinationPath = layer.DestinationPath,
        };

    private static RouteMoveJsonSubjectItem SubjectItem(RouteMoveSubjectItem item)
        => new()
        {
            Kind = RouteMoveDefinitions.ReadMachineName(item.Kind),
            Layer = item.Layer is { } layer
                ? RouteMoveDefinitions.ReadMachineName(layer)
                : null,
            SourceId = item.SourceId,
            SourcePath = item.SourcePath,
            DestinationPath = item.DestinationPath,
        };

    private static RouteMoveJsonOwnership Ownership(RouteMoveOwnership ownership)
        => new()
        {
            State = RouteMoveDefinitions.ReadMachineName(ownership.State),
            Framework = RouteMoveDefinitions.ReadMachineName(ownership.Framework),
            Extensions = RouteMoveDefinitions.ReadMachineName(ownership.Extensions),
            Claims = ownership.Claims.Select(OwnershipClaim).ToArray(),
        };

    private static RouteMoveJsonOwnershipClaim OwnershipClaim(RouteMoveOwnershipClaim claim)
        => new()
        {
            Path = claim.Path,
            Manager = RouteMoveDefinitions.ReadMachineName(claim.Manager),
            Owner = claim.Owner,
        };
}
