using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.References;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;

internal sealed partial class RouteMovePostMoveObserver(
    RouteMoveSubjectResolver subjectResolver,
    RouteMoveCategoryInventoryReader inventoryReader,
    RouteMoveReferencePlanner referencePlanner,
    RouteMoveNavigationPlanner navigationPlanner,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteMoveSubjectResolver _subjectResolver = subjectResolver;
    private readonly RouteMoveCategoryInventoryReader _inventoryReader = inventoryReader;
    private readonly RouteMoveReferencePlanner _referencePlanner = referencePlanner;
    private readonly RouteMoveNavigationPlanner _navigationPlanner = navigationPlanner;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteMovePostMoveVerification> VerifyAsync(
        RouteMovePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var expectationBoundary = await VerifyExpectationsAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        if (expectationBoundary is not null)
        {
            return expectationBoundary;
        }

        var observed = await ResolveDestinationAsync(plan, cancellationToken).ConfigureAwait(false);
        if (observed.Verification is { } destinationBoundary)
        {
            return destinationBoundary;
        }

        var inventory = observed.Inventory
            ?? throw new InvalidOperationException(
                "A verified post-move destination requires its inventory.");
        var reference = await _referencePlanner.ObserveAsync(
            plan,
            inventory.Subject.Catalogue,
            cancellationToken).ConfigureAwait(false);
        if (reference.State != RouteMoveReferencePostMoveState.Verified)
        {
            return FromReference(reference);
        }

        var navigation = _navigationPlanner.Observe(plan, inventory.Subject);
        return navigation.State switch
        {
            RouteMoveNavigationPostMoveState.Verified => Verified(),
            RouteMoveNavigationPostMoveState.Interrupted => Interrupted(),
            RouteMoveNavigationPostMoveState.Failed => Failed(
                navigation.Cause ?? "Final Route Move navigation observation failed."),
            _ => throw new ArgumentOutOfRangeException(
                nameof(navigation), navigation.State, "The navigation observation state is not defined."),
        };
    }

    private async ValueTask<RouteMovePostMoveVerification?> VerifyExpectationsAsync(
        RouteMovePlan plan,
        CancellationToken cancellationToken)
    {
        RouteMovePostMoveExpectationProjection projection;
        try
        {
            projection = RouteMovePostMoveExpectationProjection.Build(plan);
        }
        catch (Exception)
        {
            return Failed("The accepted Route Move post-move expectation is invalid.");
        }

        foreach (var observation in projection.All())
        {
            var result = await _expectationValidator.ValidateAsync(
                plan.Request.Workspace,
                observation.Expectation,
                cancellationToken).ConfigureAwait(false);
            if (result.State == FileExpectationValidationState.Cancelled)
            {
                return Interrupted();
            }

            if (result.State != FileExpectationValidationState.Matched)
            {
                return Failed(result.Cause
                    ?? $"The final Route Move {observation.Meaning} observation changed.");
            }
        }

        return null;
    }

    private static RouteMovePostMoveVerification FromReference(
        RouteMoveReferencePostMoveResult reference)
        => reference.State == RouteMoveReferencePostMoveState.Interrupted
            ? Interrupted()
            : Failed(reference.Cause ?? "Final Route Move reference observation failed.");

    private static RouteMovePostMoveVerification ReadBoundary(CancellationToken cancellationToken)
        => cancellationToken.IsCancellationRequested
            ? Interrupted()
            : Failed("The final Route Move destination could not be resolved safely.");

    private static RouteMovePostMoveVerification Verified()
        => new(RouteMovePostMoveVerificationState.Verified, Cause: null);

    private static RouteMovePostMoveVerification Failed(string cause)
        => new(RouteMovePostMoveVerificationState.Failed, cause);

    private static RouteMovePostMoveVerification Interrupted()
        => new(
            RouteMovePostMoveVerificationState.Interrupted,
            "Final Route Move semantic verification was interrupted.");

}
