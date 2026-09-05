using OpenForge.Cli.Core.Commands.Route.Remove.Models.Presentation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Rendering;

internal static partial class RouteRemoveJsonProjection
{
    private static RouteRemoveJsonSubject Subject(RouteRemoveSubject subject)
        => new()
        {
            Kind = subject.Kind is { } kind
                ? RouteRemoveDefinitions.ReadMachineName(kind)
                : null,
            Layers = subject.Layers.Select(SubjectLayer).ToArray(),
            Items = subject.Items.Select(SubjectItem).ToArray(),
        };

    private static RouteRemoveJsonSubjectLayer SubjectLayer(RouteRemoveSubjectLayer layer)
        => new()
        {
            Layer = RouteRemoveDefinitions.ReadMachineName(layer.Layer),
            SourcePath = layer.SourcePath,
        };

    private static RouteRemoveJsonSubjectItem SubjectItem(RouteRemoveSubjectItem item)
        => new()
        {
            Kind = RouteRemoveDefinitions.ReadMachineName(item.Kind),
            Layer = item.Layer is { } layer
                ? RouteRemoveDefinitions.ReadMachineName(layer)
                : null,
            SourceId = item.SourceId,
            SourcePath = item.SourcePath,
            RelativePath = item.RelativePath,
        };

    private static RouteRemoveJsonOwnership Ownership(RouteRemoveOwnership ownership)
        => new()
        {
            State = RouteRemoveDefinitions.ReadMachineName(ownership.State),
            Framework = RouteRemoveDefinitions.ReadMachineName(ownership.Framework),
            Extensions = RouteRemoveDefinitions.ReadMachineName(ownership.Extensions),
            Claims = ownership.Claims.Select(OwnershipClaim).ToArray(),
        };

    private static RouteRemoveJsonOwnershipClaim OwnershipClaim(RouteRemoveOwnershipClaim claim)
        => new()
        {
            Path = claim.Path,
            Manager = RouteRemoveDefinitions.ReadMachineName(claim.Manager),
            Owner = claim.Owner,
        };
}
