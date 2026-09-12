using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Receipts;
using OpenForge.Cli.Core.Framework.Mutation.Validation;
using OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed partial class RouteUpdateAppliedVerifier(
    RouteUpdatePlanBuilder planBuilder,
    FileExpectationValidator expectationValidator)
{
    private readonly RouteUpdatePlanBuilder _planBuilder = planBuilder;
    private readonly FileExpectationValidator _expectationValidator = expectationValidator;

    internal async ValueTask<RouteUpdateAppliedVerification> VerifyAsync(
        RouteUpdateAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        if (!input.Lease.IsHeldFor(input.Plan.Request.Workspace))
        {
            return Failed("Final Route Update verification does not hold the selected workspace lease.");
        }

        RouteUpdateAppliedVerification? receiptBoundary;
        try
        {
            receiptBoundary = await VerifyReceiptsAsync(input, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed("Exact Route Update receipt verification failed unexpectedly.");
        }

        if (receiptBoundary is not null)
        {
            return receiptBoundary;
        }

        RouteUpdatePlanBuild fresh;
        try
        {
            fresh = await _planBuilder.BuildAsync(input.Plan.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Cancelled();
        }
        catch (Exception)
        {
            return Failed("The final Route Update state could not be rebuilt after application.");
        }

        if (fresh.Formation.Findings.Any(finding =>
                finding.Code == Models.Result.RouteUpdateFindingCode.Interrupted))
        {
            return Cancelled();
        }

        if (fresh.Plan is not { } rebuilt
            || !rebuilt.IsNoOp
            || fresh.Formation.Plan is not
            {
                Completeness: Models.Result.RouteUpdatePlanCompleteness.Complete,
                Safety: Models.Result.RouteUpdatePlanSafety.Safe,
            }
            || !fresh.Formation.Effects.IsEmpty
            || !MatchesPostcondition(input.Plan, rebuilt))
        {
            return Failed(
                fresh.Formation.Findings.FirstOrDefault()?.Cause
                    ?? "The final Route Update state did not rebuild as the exact complete safe no-op.");
        }

        return new RouteUpdateAppliedVerification
        {
            State = RouteUpdateAppliedVerificationState.Verified,
            Cause = null,
        };
    }

    private async ValueTask<RouteUpdateAppliedVerification?> VerifyReceiptsAsync(
        RouteUpdateAppliedVerificationInput input,
        CancellationToken cancellationToken)
    {
        if (!input.Progress.Findings.IsEmpty
            || input.Progress.Receipts.Length != input.Plan.FileChanges.Length)
        {
            return Failed("Final Route Update verification requires one clean receipt per effect.");
        }

        for (var index = 0; index < input.Progress.Receipts.Length; index++)
        {
            var receipt = input.Progress.Receipts[index];
            var change = input.Plan.FileChanges[index];
            if (!MatchesReceipt(change, receipt)
                || receipt.After is not { } after)
            {
                return Failed("A planned Route Update effect has no exact verified receipt.");
            }

            var validation = await _expectationValidator.ValidateAsync(
                    input.Plan.Request.Workspace,
                    after.Expectation,
                    cancellationToken)
                .ConfigureAwait(false);
            if (validation.State == FileExpectationValidationState.Cancelled)
            {
                return Cancelled();
            }

            if (validation.State != FileExpectationValidationState.Matched)
            {
                return Failed(
                    validation.Cause
                        ?? "An applied Route Update effect changed before final verification.");
            }
        }

        return null;
    }

    private static bool MatchesReceipt(
        PlannedFileChange change,
        FileChangeReceipt receipt)
        => receipt.Change.Kind == change.Kind
            && receipt.Change.Expectation == change.Expectation
            && receipt.Change.IntendedBytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan())
            && receipt.EffectState == FilesystemEffectState.Applied
            && receipt.VerificationState == FilesystemVerificationState.Verified
            && receipt.After is { } after
            && after.Kind == FileExpectationKind.File
            && after.HasBytes
            && after.Bytes.AsSpan().SequenceEqual(change.IntendedBytes.AsSpan());

    private static RouteUpdateAppliedVerification Cancelled()
        => new()
        {
            State = RouteUpdateAppliedVerificationState.Cancelled,
            Cause = "Final Route Update verification was interrupted.",
        };

    private static RouteUpdateAppliedVerification Failed(string cause)
        => new()
        {
            State = RouteUpdateAppliedVerificationState.Failed,
            Cause = cause,
        };
}
