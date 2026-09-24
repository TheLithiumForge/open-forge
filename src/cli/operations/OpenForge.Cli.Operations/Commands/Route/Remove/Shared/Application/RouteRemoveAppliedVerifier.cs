using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveAppliedVerifier(
    RouteRemovePostRemoveObserver postRemoveObserver,
    FileExpectationValidator expectationValidator,
    PhysicalPathResolver physicalPathResolver)
{
    private readonly RouteRemovePostRemoveObserver _postRemoveObserver = postRemoveObserver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;

    internal async ValueTask<RouteRemoveAppliedVerification> VerifyContentEffectsAsync(
        RouteRemoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Lease.IsHeldFor(input.Plan.Request.Workspace))
        {
            return Failed("Route Remove content verification does not hold the selected workspace lease.");
        }

        try
        {
            var receipts = await VerifyReceiptsAsync(input, cancellationToken).ConfigureAwait(false);
            if (receipts is not null)
            {
                return receipts;
            }

            var settings = await VerifySettingsAsync(input.Plan, input.Progress, cancellationToken)
                .ConfigureAwait(false);
            if (settings is not null)
            {
                return Failed(settings);
            }

            var absence = await _postRemoveObserver.VerifyContentAbsenceAsync(input.Plan, cancellationToken)
                .ConfigureAwait(false);
            return absence.State switch
            {
                RouteRemovePostRemoveVerificationState.Verified => Verified(),
                RouteRemovePostRemoveVerificationState.Interrupted => Interrupted(),
                RouteRemovePostRemoveVerificationState.Failed => Failed(
                    absence.Cause ?? "The pre-publication Route Remove absence check failed."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(absence), absence.State, "The pre-publication absence state is not defined."),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("Route Remove content verification failed unexpectedly.");
        }
    }

    internal async ValueTask<RouteRemoveAppliedVerification> VerifyAsync(
        RouteRemoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Lease.IsHeldFor(input.Plan.Request.Workspace))
        {
            return Failed("Final Route Remove verification does not hold the selected workspace lease.");
        }

        try
        {
            var receiptBoundary = await VerifyReceiptsAsync(input, cancellationToken)
                .ConfigureAwait(false);
            if (receiptBoundary is not null)
            {
                return receiptBoundary;
            }

            var persistenceBoundary = await VerifyPersistenceAsync(input, cancellationToken)
                .ConfigureAwait(false);
            if (persistenceBoundary is not null)
            {
                return persistenceBoundary;
            }

            var postRemove = await _postRemoveObserver.VerifyAsync(input.Plan, cancellationToken)
                .ConfigureAwait(false);
            return postRemove.State switch
            {
                RouteRemovePostRemoveVerificationState.Verified => Verified(),
                RouteRemovePostRemoveVerificationState.Interrupted => Interrupted(),
                RouteRemovePostRemoveVerificationState.Failed => Failed(
                    postRemove.Cause ?? "The final Route Remove semantic state did not match its accepted plan."),
                _ => throw new ArgumentOutOfRangeException(
                    nameof(postRemove), postRemove.State, "The post-remove verification state is not defined."),
            };
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("Final Route Remove verification failed unexpectedly.");
        }
    }

    private static RouteRemoveAppliedVerification Verified()
        => new(RouteRemoveAppliedVerificationState.Verified, cause: null);

    private static RouteRemoveAppliedVerification Interrupted()
        => new(
            RouteRemoveAppliedVerificationState.Interrupted,
            "Final Route Remove verification was interrupted.");

    private static RouteRemoveAppliedVerification Failed(string cause)
        => new(RouteRemoveAppliedVerificationState.Failed, cause);
}
