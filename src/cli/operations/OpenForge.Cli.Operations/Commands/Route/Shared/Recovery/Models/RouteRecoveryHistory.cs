using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Commands.Route.Shared.Recovery.Models;

internal sealed record RouteRecoveryHistory(
    ImmutableArray<string> Paths,
    ImmutableArray<RouteRecoveryCoverageFailure> UnavailableStores)
{
    internal static RouteRecoveryHistory Empty { get; } = new([], []);
}

internal sealed record RouteRecoveryCoverageFailure(string? Path, string Cause);
