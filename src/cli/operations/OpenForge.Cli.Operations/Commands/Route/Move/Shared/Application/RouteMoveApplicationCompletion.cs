using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed class RouteMoveApplicationCompletion(
    RouteMoveAppliedVerifier verifier)
{
    private readonly RouteMoveAppliedVerifier _verifier = verifier;

    internal async ValueTask<RouteMoveApplicationProgress> CompleteAsync(
        RouteMoveHeldApplication held,
        RouteMoveRecoveryPreparationResult preparation,
        RouteMoveApplicationProgress application,
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
                "Successful Route Move completion verification requires its result.");
        if (result.State != RouteMoveAppliedVerificationState.Verified)
        {
            return RouteMoveApplicationProgressProjector.VerificationBoundary(
                held.Plan,
                application,
                result);
        }

        return await DeleteRecoveryAsync(
            held,
            preparation,
            application with { Verification = RouteMoveVerificationState.Verified },
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<VerificationAttempt> VerifyAsync(
        RouteMoveHeldApplication held,
        RouteMoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _verifier.VerifyAsync(
                new RouteMoveAppliedVerificationInput
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
                "Final Route Move verification was interrupted unexpectedly."));
        }
        catch (Exception)
        {
            return VerificationAttempt.Stop(Unexpected(
                held,
                application,
                interrupted: false,
                "Final Route Move verification failed unexpectedly."));
        }
    }

    private static async ValueTask<RouteMoveApplicationProgress> DeleteRecoveryAsync(
        RouteMoveHeldApplication held,
        RouteMoveRecoveryPreparationResult preparation,
        RouteMoveApplicationProgress verified,
        CancellationToken cancellationToken)
    {
        try
        {
            var prepared = preparation.Preparation
                ?? throw new InvalidOperationException(
                    "Prepared Route Move recovery requires its verified bundle identity.");
            var deletion = await RouteMoveRecoveryLifecycle.DeleteExactAsync(
                new RouteMoveRecoveryDeletionInput
                {
                    Held = held,
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
                "Route Move recovery deletion was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return Unexpected(
                held,
                verified,
                interrupted: false,
                "Route Move recovery deletion failed unexpectedly.");
        }
    }

    private static RouteMoveApplicationProgress Unexpected(
        RouteMoveHeldApplication held,
        RouteMoveApplicationProgress progress,
        bool interrupted,
        string cause)
        => RouteMoveApplicationProgressProjector.UnexpectedAfterApplication(
            held.Plan,
            progress,
            interrupted,
            cause);

    private sealed record VerificationAttempt(
        RouteMoveAppliedVerification? Verification,
        RouteMoveApplicationProgress? Boundary)
    {
        internal static VerificationAttempt Stop(RouteMoveApplicationProgress boundary)
            => new(Verification: null, boundary);
    }
}
