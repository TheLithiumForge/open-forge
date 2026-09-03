using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Lifecycle.Models.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;

internal static class RouteMoveOwnershipProjector
{
    internal static RouteMoveOwnership Project(
        LifecycleOwnershipReadResult ownership,
        RouteMoveResolvedSubject subject)
    {
        var claims = SelectSubjectClaims(ownership, subject);
        return new RouteMoveOwnership
        {
            State = ReadState(ownership, claims),
            Framework = ReadTrust(ownership.Framework.State),
            Extensions = ReadTrust(ownership.Extensions.State),
            Claims = claims.Select(claim => new RouteMoveOwnershipClaim
            {
                Path = claim.Path,
                Manager = claim.Manager == LifecycleOwnershipManager.Framework
                    ? RouteMoveOwnershipManager.Framework
                    : RouteMoveOwnershipManager.Extension,
                Owner = claim.Owner,
            }).ToImmutableArray(),
        };
    }

    internal static bool HasSubjectClaims(
        LifecycleOwnershipReadResult ownership,
        RouteMoveResolvedSubject subject)
        => ownership.Claims.Any(claim => IsWithinSubject(subject, claim.Path));

    private static ImmutableArray<LifecycleOwnershipClaim> SelectSubjectClaims(
        LifecycleOwnershipReadResult ownership,
        RouteMoveResolvedSubject subject)
        => ownership.Claims
            .Where(claim => IsWithinSubject(subject, claim.Path))
            .ToImmutableArray();

    private static RouteMoveOwnershipState ReadState(
        LifecycleOwnershipReadResult ownership,
        ImmutableArray<LifecycleOwnershipClaim> claims)
    {
        if (ownership.Framework.State == LifecycleOwnershipReadState.Interrupted
            || ownership.Extensions.State == LifecycleOwnershipReadState.Interrupted)
        {
            return RouteMoveOwnershipState.Interrupted;
        }

        if (ownership.Framework.State != LifecycleOwnershipReadState.Trusted
            || ownership.Extensions.State != LifecycleOwnershipReadState.Trusted)
        {
            return RouteMoveOwnershipState.Blocked;
        }

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
                StringComparison.Ordinal));
        }

        var root = SourceLogicalPath.ReadParent(subject.SelectedSource.Identity.CanonicalBasePath);
        return string.Equals(path, root, StringComparison.Ordinal)
            || path.StartsWith($"{root}/", StringComparison.Ordinal);
    }

    private static RouteMoveOwnershipTrust ReadTrust(LifecycleOwnershipReadState state)
        => state switch
        {
            LifecycleOwnershipReadState.Trusted => RouteMoveOwnershipTrust.Trusted,
            LifecycleOwnershipReadState.Blocked => RouteMoveOwnershipTrust.Blocked,
            LifecycleOwnershipReadState.Interrupted => RouteMoveOwnershipTrust.Interrupted,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state,
                "The lifecycle ownership state is not defined."),
        };
}
