using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemoveApplicationCompletion(
    RouteRemoveAppliedVerifier verifier)
{
    private readonly RouteRemoveAppliedVerifier _verifier = verifier;

    internal async ValueTask<RouteRemoveApplicationProgress> CompleteAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveRecoveryPreparationResult preparation,
        RouteRemoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        var verification = await VerifyAsync(held, application, cancellationToken)
            .ConfigureAwait(false);
        if (verification.Boundary is { } boundary)
        {
            return boundary;
        }

        var result = verification.Verification
            ?? throw new InvalidOperationException(
                "Successful Route Remove completion verification requires its result.");
        if (result.State != RouteRemoveAppliedVerificationState.Verified)
        {
            return RouteRemoveApplicationProgressProjector.VerificationBoundary(
                held.Plan,
                application,
                result);
        }

        return await DeleteRecoveryAsync(
            held,
            preparation,
            application with { Verification = RouteRemoveVerificationState.Verified },
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<VerificationAttempt> VerifyAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _verifier.VerifyAsync(
                new RouteRemoveAppliedVerificationInput
                {
                    Plan = held.Plan,
                    Lease = held.Lease,
                    Progress = application,
                },
                cancellationToken).ConfigureAwait(false);
            return new VerificationAttempt(result, Boundary: null);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return VerificationAttempt.Stop(Unexpected(
                held,
                application,
                interrupted: true,
                "Final Route Remove verification was interrupted unexpectedly."));
        }
        catch (Exception)
        {
            return VerificationAttempt.Stop(Unexpected(
                held,
                application,
                interrupted: false,
                "Final Route Remove verification failed unexpectedly."));
        }
    }

    private static async ValueTask<RouteRemoveApplicationProgress> DeleteRecoveryAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveRecoveryPreparationResult preparation,
        RouteRemoveApplicationProgress verified,
        CancellationToken cancellationToken)
    {
        try
        {
            var prepared = preparation.Preparation
                ?? throw new InvalidOperationException(
                    "Prepared Route Remove recovery requires its verified bundle identity.");
            var deletion = await RouteRemoveRecoveryLifecycle.DeleteExactAsync(
                new RouteRemoveRecoveryDeletionInput
                {
                    Plan = held.Plan,
                    OperationId = held.OperationId,
                    Lease = held.Lease,
                    Preparation = prepared,
                },
                cancellationToken).ConfigureAwait(false);
            return verified with
            {
                Recovery = deletion.Recovery,
                Findings = deletion.Finding is null ? [] : [deletion.Finding],
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Unexpected(
                held,
                verified,
                interrupted: true,
                "Route Remove recovery deletion was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return Unexpected(
                held,
                verified,
                interrupted: false,
                "Route Remove recovery deletion failed unexpectedly.");
        }
    }

    private static RouteRemoveApplicationProgress Unexpected(
        RouteRemoveHeldApplication held,
        RouteRemoveApplicationProgress progress,
        bool interrupted,
        string cause)
        => RouteRemoveApplicationProgressProjector.UnexpectedAfterApplication(
            held.Plan,
            progress,
            interrupted,
            cause);

    private sealed record VerificationAttempt(
        RouteRemoveAppliedVerification? Verification,
        RouteRemoveApplicationProgress? Boundary)
    {
        internal static VerificationAttempt Stop(RouteRemoveApplicationProgress boundary)
            => new(Verification: null, boundary);
    }
}
