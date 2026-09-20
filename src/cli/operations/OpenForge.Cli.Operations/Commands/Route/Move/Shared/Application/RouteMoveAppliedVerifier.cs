using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Framework.Mutation.Validation;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMoveAppliedVerifier(
    RouteMovePostMoveObserver postMoveObserver,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteMovePostMoveObserver _postMoveObserver = postMoveObserver;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteMoveAppliedVerification> VerifyAsync(
        RouteMoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(input);
        if (!input.Lease.IsHeldFor(input.Plan.Request.Workspace))
        {
            return Failed("Final Route Move verification does not hold the selected workspace lease.");
        }

        RouteMoveAppliedVerification? receiptBoundary;
        try
        {
            receiptBoundary = await VerifyReceiptsAsync(input, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Interrupted();
        }
        catch (Exception)
        {
            return Failed("Exact Route Move receipt verification failed unexpectedly.");
        }

        if (receiptBoundary is not null)
        {
            return receiptBoundary;
        }

        var postMove = await _postMoveObserver.VerifyAsync(input.Plan, cancellationToken)
            .ConfigureAwait(false);
        return postMove.State switch
        {
            RouteMovePostMoveVerificationState.Verified => Verified(),
            RouteMovePostMoveVerificationState.Interrupted => Interrupted(),
            RouteMovePostMoveVerificationState.Failed => Failed(
                postMove.Cause ?? "The final Route Move semantic state did not match its accepted plan."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(postMove), postMove.State, "The post-move verification state is not defined."),
        };
    }

    private static RouteMoveAppliedVerification Verified()
        => new(RouteMoveAppliedVerificationState.Verified, cause: null);

    private static RouteMoveAppliedVerification Interrupted()
        => new(
            RouteMoveAppliedVerificationState.Interrupted,
            "Final Route Move verification was interrupted.");

    private static RouteMoveAppliedVerification Failed(string cause)
        => new(RouteMoveAppliedVerificationState.Failed, cause);
}
