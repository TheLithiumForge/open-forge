using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemoveApplicationCompletion(
    RouteRemoveAppliedVerifier verifier,
    RouteRemoveOwnershipPublisher ownershipPublisher)
{
    private readonly RouteRemoveAppliedVerifier _verifier = verifier;
    private readonly RouteRemoveOwnershipPublisher _ownershipPublisher = ownershipPublisher;

    internal async ValueTask<RouteRemoveApplicationProgress> CompleteAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveRecoveryPreparationResult preparation,
        RouteRemoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        var contentVerification = await VerifyContentAsync(held, application, cancellationToken)
            .ConfigureAwait(false);
        if (contentVerification.Boundary is { } contentBoundary)
        {
            return contentBoundary;
        }

        var contentResult = contentVerification.Verification
            ?? throw new InvalidOperationException(
                "Successful Route Remove content verification requires its result.");
        if (contentResult.State != RouteRemoveAppliedVerificationState.Verified)
        {
            return RouteRemoveApplicationProgressProjector.VerificationBoundary(
                held.Plan,
                application,
                contentResult);
        }

        var prepared = preparation.Preparation
            ?? throw new InvalidOperationException(
                "Prepared Route Remove recovery requires its verified bundle identity.");
        var published = await PublishOwnershipAsync(
            held,
            prepared,
            application,
            cancellationToken).ConfigureAwait(false);
        if (!published.Findings.IsEmpty)
        {
            return published;
        }

        var verification = await VerifyAsync(held, published, cancellationToken)
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
                published,
                result);
        }

        return await DeleteRecoveryAsync(
            held,
            preparation,
            published with { Verification = RouteRemoveVerificationState.Verified },
            cancellationToken).ConfigureAwait(false);
    }

    private async ValueTask<VerificationAttempt> VerifyContentAsync(
        RouteRemoveHeldApplication held,
        RouteRemoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _verifier.VerifyContentEffectsAsync(
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
                "Route Remove content verification was interrupted unexpectedly."));
        }
        catch (Exception)
        {
            return VerificationAttempt.Stop(Unexpected(
                held,
                application,
                interrupted: false,
                "Route Remove content verification failed unexpectedly."));
        }
    }

    private async ValueTask<RouteRemoveApplicationProgress> PublishOwnershipAsync(
        RouteRemoveHeldApplication held,
        RecoveryBundlePreparation prepared,
        RouteRemoveApplicationProgress application,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _ownershipPublisher.PublishAsync(
                held.Plan,
                held.Lease,
                prepared,
                application,
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return RouteRemoveApplicationProgressProjector.UnexpectedAfterApplication(
                held.Plan,
                application with { OwnershipOutcome = RouteRemovePersistenceOutcome.NotStarted },
                interrupted: true,
                "Route Remove ownership publication was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return RouteRemoveApplicationProgressProjector.UnexpectedAfterApplication(
                held.Plan,
                application with { OwnershipOutcome = RouteRemovePersistenceOutcome.Unknown },
                interrupted: false,
                "Route Remove ownership publication failed unexpectedly.");
        }
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
