using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.Framework.Sources.Models.Routing;

internal enum SourceRouteState
{
    Routed,
    Unrouted,
    Ambiguous,
    Unavailable,
}

internal sealed class SourceRouteFact
{
    internal SourceRouteFact(
        SourceLogicalIdentity identity,
        SourceRouteState state,
        bool isIdentityUnique)
    {
        ArgumentNullException.ThrowIfNull(identity);
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source route state is not defined.");
        }

        Identity = identity;
        State = state;
        IsIdentityUnique = isIdentityUnique;
        Route = state == SourceRouteState.Routed && isIdentityUnique
            ? identity.AutomaticId
            : null;
    }

    internal SourceLogicalIdentity Identity { get; }

    internal SourceRouteState State { get; }

    internal string? Route { get; }

    internal bool IsIdentityUnique { get; }
}
