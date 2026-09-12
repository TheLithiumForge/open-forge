using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemovePostRemoveObserver(
    RouteRemovePlanBuilder planBuilder,
    RouteRemovePostRemoveVerifier verifier)
{
    private readonly RouteRemovePlanBuilder _planBuilder = planBuilder;
    private readonly RouteRemovePostRemoveVerifier _verifier = verifier;

    internal async ValueTask<RouteRemovePostRemoveVerification> VerifyAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var observation = await _planBuilder.BuildAbsenceAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        return _verifier.Verify(plan, observation);
    }
}
