using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveOwnershipProjector
{
    internal static RouteMoveOwnership Project(
        WorkspaceOwnershipRead ownership,
        RouteMoveResolvedSubject subject)
    {
        var claims = SelectSubjectClaims(ownership, subject);
        return new RouteMoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership),
            Extensions = ReadTrust(ownership),
            Claims = claims.Select(claim => new RouteMoveOwnershipClaim
            {
                Path = claim.Path,
                Manager = claim.Manager == OwnedPathManager.Framework
                    ? RouteMoveOwnershipManager.Framework
                    : RouteMoveOwnershipManager.Extension,
                Owner = claim.Owner,
            }).ToImmutableArray(),
        };
    }

    internal static bool HasSubjectClaims(
        WorkspaceOwnershipRead ownership,
        RouteMoveResolvedSubject subject)
        => RouteOwnershipEvidence.Claims(ownership).Any(claim => IsWithinSubject(subject, claim.Path));

    private static ImmutableArray<OwnedPath> SelectSubjectClaims(
        WorkspaceOwnershipRead ownership,
        RouteMoveResolvedSubject subject)
        => RouteOwnershipEvidence.Claims(ownership)
            .Where(claim => IsWithinSubject(subject, claim.Path))
            .ToImmutableArray();

    private static RouteMoveOwnershipState ReadState(
        WorkspaceOwnershipRead ownership,
        ImmutableArray<OwnedPath> claims)
    {
        if (!RouteOwnershipEvidence.IsEstablished(ownership)) return RouteMoveOwnershipState.NotEstablished;

        return claims.IsEmpty
            ? RouteMoveOwnershipState.Unmanaged
            : RouteMoveOwnershipState.Claimed;
    }

    private static bool IsWithinSubject(RouteMoveResolvedSubject subject, string path)
    {
        if (subject.Kind == RouteMoveSubjectKind.Leaf)
        {
            return subject.Layers.Any(layer => string.Equals(
                layer.Layer.CanonicalPath,
                path,
                StringComparison.OrdinalIgnoreCase));
        }

        var root = SourceLogicalPath.ReadParent(subject.SelectedSource.Identity.CanonicalBasePath);
        return string.Equals(path, root, StringComparison.OrdinalIgnoreCase)
            || path.StartsWith($"{root}/", StringComparison.OrdinalIgnoreCase);
    }

    private static RouteMoveOwnershipTrust ReadTrust(WorkspaceOwnershipRead ownership)
        => RouteOwnershipEvidence.IsEstablished(ownership) ? RouteMoveOwnershipTrust.Trusted : RouteMoveOwnershipTrust.NotEstablished;
}
