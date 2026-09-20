using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Commands.Route.Shared.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveOwnershipProjector
{
    internal static RouteRemoveOwnership Project(
        WorkspaceOwnershipRead ownership,
        RouteRemoveResolvedSubject subject)
    {
        var claims = SelectSubjectClaims(ownership, subject);
        return new RouteRemoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership),
            Extensions = ReadTrust(ownership),
            Claims = ProjectClaims(claims),
        };
    }

    internal static bool HasSubjectClaims(
        WorkspaceOwnershipRead ownership,
        RouteRemoveResolvedSubject subject)
        => RouteOwnershipEvidence.Claims(ownership).Any(claim => IsWithinSubject(subject, claim.Path));

    internal static RouteRemoveOwnership Project(
        WorkspaceOwnershipRead ownership,
        RouteRemoveAbsenceScope scope)
    {
        var claims = RouteOwnershipEvidence.Claims(ownership)
            .Where(claim => scope.ContainsIgnoreCase(claim.Path))
            .ToImmutableArray();
        return new RouteRemoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership),
            Extensions = ReadTrust(ownership),
            Claims = ProjectClaims(claims),
        };
    }

    private static ImmutableArray<OwnedPath> SelectSubjectClaims(
        WorkspaceOwnershipRead ownership,
        RouteRemoveResolvedSubject subject)
        => RouteOwnershipEvidence.Claims(ownership)
            .Where(claim => IsWithinSubject(subject, claim.Path))
            .ToImmutableArray();

    private static ImmutableArray<RouteRemoveOwnershipClaim> ProjectClaims(
        IEnumerable<OwnedPath> claims)
        => claims.Select(claim => new RouteRemoveOwnershipClaim
        {
            Path = claim.Path,
            Manager = claim.Manager == OwnedPathManager.Framework
                    ? RouteRemoveOwnershipManager.Framework
                    : RouteRemoveOwnershipManager.Extension,
            Owner = claim.Owner,
        })
            .ToImmutableArray();

    private static RouteRemoveOwnershipState ReadState(
        WorkspaceOwnershipRead ownership,
        ImmutableArray<OwnedPath> claims)
    {
        if (!RouteOwnershipEvidence.IsEstablished(ownership)) return RouteRemoveOwnershipState.NotEstablished;

        return claims.IsEmpty
            ? RouteRemoveOwnershipState.Unmanaged
            : RouteRemoveOwnershipState.Claimed;
    }

    private static bool IsWithinSubject(RouteRemoveResolvedSubject subject, string path)
    {
        if (subject.Kind == RouteRemoveSubjectKind.Leaf)
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

    private static RouteRemoveOwnershipTrust ReadTrust(WorkspaceOwnershipRead ownership)
        => RouteOwnershipEvidence.IsEstablished(ownership) ? RouteRemoveOwnershipTrust.Trusted : RouteRemoveOwnershipTrust.NotEstablished;
}
