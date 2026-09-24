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

    internal async ValueTask<RouteRemovePostRemoveVerification> VerifyContentAbsenceAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        RouteRemovePlanBuild observation;
        try
        {
            observation = await _planBuilder.BuildContentAbsenceAsync(plan, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new RouteRemovePostRemoveVerification(
                RouteRemovePostRemoveVerificationState.Interrupted,
                "Route Remove pre-publication absence verification was interrupted.");
        }
        catch (Exception)
        {
            return new RouteRemovePostRemoveVerification(
                RouteRemovePostRemoveVerificationState.Failed,
                "Route Remove pre-publication absence verification failed unexpectedly.");
        }

        return _verifier.VerifyContentAbsence(plan, observation);
    }
}
