using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal static class RouteRemoveOwnershipProjector
{
    internal static RouteRemoveOwnership Project(
        LifecycleOwnershipReadResult ownership,
        RouteRemoveResolvedSubject subject)
    {
        var claims = SelectSubjectClaims(ownership, subject);
        return new RouteRemoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership.Framework.State),
            Extensions = ReadTrust(ownership.Extensions.State),
            Claims = ProjectClaims(claims),
        };
    }

    internal static bool HasSubjectClaims(
        LifecycleOwnershipReadResult ownership,
        RouteRemoveResolvedSubject subject)
        => ownership.Claims.Any(claim => IsWithinSubject(subject, claim.Path));

    internal static RouteRemoveOwnership Project(
        LifecycleOwnershipReadResult ownership,
        RouteRemoveAbsenceScope scope)
    {
        var claims = ownership.Claims.Where(claim => scope.Contains(claim.Path)).ToImmutableArray();
        return new RouteRemoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership.Framework.State),
            Extensions = ReadTrust(ownership.Extensions.State),
            Claims = ProjectClaims(claims),
        };
    }

    private static ImmutableArray<LifecycleOwnershipClaim> SelectSubjectClaims(
        LifecycleOwnershipReadResult ownership,
        RouteRemoveResolvedSubject subject)
        => ownership.Claims
            .Where(claim => IsWithinSubject(subject, claim.Path))
            .ToImmutableArray();

    private static ImmutableArray<RouteRemoveOwnershipClaim> ProjectClaims(
        IEnumerable<LifecycleOwnershipClaim> claims)
        => claims.Select(claim => new RouteRemoveOwnershipClaim
        {
            Path = claim.Path,
            Manager = claim.Manager == LifecycleOwnershipManager.Framework
                    ? RouteRemoveOwnershipManager.Framework
                    : RouteRemoveOwnershipManager.Extension,
            Owner = claim.Owner,
        })
            .ToImmutableArray();

    private static RouteRemoveOwnershipState ReadState(
        LifecycleOwnershipReadResult ownership,
        ImmutableArray<LifecycleOwnershipClaim> claims)
    {
        if (ownership.Framework.State == LifecycleOwnershipReadState.Interrupted
            || ownership.Extensions.State == LifecycleOwnershipReadState.Interrupted)
        {
            return RouteRemoveOwnershipState.Interrupted;
        }

        if (ownership.Framework.State != LifecycleOwnershipReadState.Trusted
            || ownership.Extensions.State != LifecycleOwnershipReadState.Trusted)
        {
            return RouteRemoveOwnershipState.Blocked;
        }

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
                StringComparison.Ordinal));
        }

        var root = SourceLogicalPath.ReadParent(subject.SelectedSource.Identity.CanonicalBasePath);
        return string.Equals(path, root, StringComparison.Ordinal)
            || path.StartsWith($"{root}/", StringComparison.Ordinal);
    }

    private static RouteRemoveOwnershipTrust ReadTrust(LifecycleOwnershipReadState state)
        => state switch
        {
            LifecycleOwnershipReadState.Trusted => RouteRemoveOwnershipTrust.Trusted,
            LifecycleOwnershipReadState.Blocked => RouteRemoveOwnershipTrust.Blocked,
            LifecycleOwnershipReadState.Interrupted => RouteRemoveOwnershipTrust.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle ownership state is not defined."),
        };
}
