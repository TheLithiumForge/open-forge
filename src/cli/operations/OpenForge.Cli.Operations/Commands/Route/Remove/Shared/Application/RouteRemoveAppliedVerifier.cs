using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

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
            if (await VerifyOutsideInputsAsync(input, cancellationToken).ConfigureAwait(false) is { } outsideCause)
                return Failed(outsideCause);
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
            if (await VerifyOutsideInputsAsync(input, cancellationToken).ConfigureAwait(false) is { } outsideCause)
                return Failed(outsideCause);
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

    private async ValueTask<string?> VerifyOutsideInputsAsync(RouteRemoveAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        var plan = input.Plan;
        if (plan.Projection.Subject.OutsideRoute is not { } outside) return null;
        if (RouteRemoveOutsideBoundaryInspector.ValidateCurrentAnchors(plan, cancellationToken) is { } rootCause)
            return rootCause;
        foreach (var logicalPath in plan.Projection.FileChanges.Select(change => change.LogicalPath)
            .Concat(plan.Projection.DirectoryDeletions.Select(deletion => deletion.LogicalPath)))
        {
            var path = Path.GetRelativePath(plan.Request.Workspace.LexicalRoot, logicalPath).Replace('\\', '/');
            if (!RouteRemoveOutsideBoundaryInspector.Contains(outside.DirectoryPath, path))
                return "A final content coordinate leaves the selected outside registration.";
            if (RouteRemoveOutsideBoundaryInspector.Validate(plan.LayoutRead, path, cancellationToken, allowMissing: true) is { } cause)
                return cause;
        }
        foreach (var snapshot in plan.Projection.References.Documents.Select(document => document.Snapshot)
            .Concat(plan.Projection.Navigation.ReadonlyInputs)
            .Where(snapshot => plan.Projection.FileChanges.All(change => change.LogicalPath != snapshot.LogicalPath))
            .DistinctBy(snapshot => snapshot.LogicalPath))
        {
            var result = await _expectationValidator.ValidateAsync(plan.Request.Workspace, snapshot.Expectation, cancellationToken).ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            if (result.State != FileExpectationValidationState.Matched)
                return "A retained readonly outside removal input changed before final verification.";
        }
        return null;
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
