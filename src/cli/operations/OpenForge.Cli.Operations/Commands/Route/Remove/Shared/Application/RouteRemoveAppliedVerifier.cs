using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Framework.Mutation.Validation;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed partial class RouteRemoveAppliedVerifier(
    RouteRemovePostRemoveObserver postRemoveObserver,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteRemovePostRemoveObserver _postRemoveObserver = postRemoveObserver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

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
