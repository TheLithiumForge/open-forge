using OpenForge.Cli.Core.Commands.Route.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;

internal sealed class RouteUpdateApplicationPipeline(
    RouteUpdateApplicationPreparer preparer,
    RouteUpdateEffectApplication effectApplication,
    RouteUpdateAppliedVerifier verifier)
{
    private readonly RouteUpdateApplicationPreparer _preparer = preparer;
    private readonly RouteUpdateEffectApplication _effectApplication = effectApplication;
    private readonly RouteUpdateAppliedVerifier _verifier = verifier;

    internal async ValueTask<RouteUpdateApplicationProgress> ExecuteAsync(
        RouteUpdateApplicationPipelineInput input,
        CancellationToken cancellationToken)
    {
        var plan = input.Plan;
        RouteUpdateApplicationPreparation prepared;
        try
        {
            prepared = await _preparer.PrepareAsync(
                    new RouteUpdateApplicationPreparationInput
                    {
                        Plan = plan,
                        Lease = input.Lease,
                        OperationId = input.OperationId,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return BeforeApplication(
                Finding(
                    plan,
                    RouteUpdateFindingCode.Interrupted,
                    "Route Update application preparation was interrupted unexpectedly."),
                RouteUpdateRecovery.Unknown(residualPath: null));
        }
        catch (Exception)
        {
            return BeforeApplication(
                Finding(
                    plan,
                    RouteUpdateFindingCode.OperationFailed,
                    "Route Update application preparation failed unexpectedly."),
                RouteUpdateRecovery.Unknown(residualPath: null));
        }

        if (prepared.State != RouteUpdateApplicationPreparationState.Ready)
        {
            return BeforeApplication(
                prepared.Finding
                    ?? new RouteUpdateFinding(
                        RouteUpdateFindingCode.OperationFailed,
                        "Route Update application preparation returned no decisive finding.",
                        plan.Preview.Target.Path),
                prepared.Recovery);
        }

        var recoveryPreparation = prepared.RecoveryPreparation
            ?? throw new InvalidOperationException(
                "Ready Route Update application requires one verified recovery final.");
        var validation = prepared.Validation
            ?? throw new InvalidOperationException(
                "Ready Route Update application requires exact whole-plan checks.");
        var application = await _effectApplication.ApplyAsync(
                new RouteUpdateEffectApplicationInput
                {
                    Plan = plan,
                    Preparation = recoveryPreparation,
                    Lease = input.Lease,
                    Validation = validation,
                },
                cancellationToken)
            .ConfigureAwait(false);

        if (!application.Findings.IsEmpty)
        {
            return application;
        }

        RouteUpdateAppliedVerification verification;
        try
        {
            verification = await _verifier.VerifyAsync(
                    new RouteUpdateAppliedVerificationInput
                    {
                        Plan = plan,
                        Progress = application,
                        Lease = input.Lease,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return AfterApplicationBoundary(
                application,
                Finding(
                    plan,
                    RouteUpdateFindingCode.Interrupted,
                    "Final Route Update verification was interrupted unexpectedly."));
        }
        catch (Exception)
        {
            return AfterApplicationBoundary(
                application,
                Finding(
                    plan,
                    RouteUpdateFindingCode.OperationFailed,
                    "Final Route Update verification failed unexpectedly."));
        }

        if (verification.State != RouteUpdateAppliedVerificationState.Verified)
        {
            var cancelled = verification.State == RouteUpdateAppliedVerificationState.Cancelled;
            return application with
            {
                Verification = cancelled
                    ? RouteUpdateVerificationState.Unknown
                    : RouteUpdateVerificationState.Failed,
                Findings =
                [
                    new RouteUpdateFinding(
                        cancelled
                            ? RouteUpdateFindingCode.Interrupted
                            : RouteUpdateFindingCode.VerificationFailed,
                        verification.Cause
                            ?? "Final Route Update verification did not complete.",
                        plan.Preview.Target.Path),
                ],
            };
        }

        RouteUpdateRecoveryCompletionResult completion;
        try
        {
            completion = await RouteUpdateRecoveryCompleter.CompleteAsync(
                    new RouteUpdateRecoveryCompletionInput
                    {
                        Plan = plan,
                        Preparation = recoveryPreparation,
                        Lease = input.Lease,
                    },
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return AfterRecoveryBoundary(
                plan,
                application,
                RouteUpdateFindingCode.Interrupted,
                "Route Update recovery cleanup was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return AfterRecoveryBoundary(
                plan,
                application,
                RouteUpdateFindingCode.RecoveryFailed,
                "Route Update recovery cleanup failed unexpectedly.");
        }

        return application with
        {
            Recovery = completion.Recovery,
            Verification = RouteUpdateVerificationState.Verified,
            Findings = completion.FindingCode is { } code
                ?
                [
                    new RouteUpdateFinding(
                        code,
                        completion.Cause
                            ?? "Route Update recovery cleanup did not complete.",
                        completion.Recovery.ResidualPath),
                ]
                : [],
        };
    }

    private static RouteUpdateApplicationProgress AfterApplicationBoundary(
        RouteUpdateApplicationProgress application,
        RouteUpdateFinding finding)
        => application with
        {
            Verification = RouteUpdateVerificationState.Unknown,
            Findings = [finding],
        };

    private static RouteUpdateApplicationProgress AfterRecoveryBoundary(
        RouteUpdatePlan plan,
        RouteUpdateApplicationProgress application,
        RouteUpdateFindingCode code,
        string cause)
        => application with
        {
            Recovery = RouteUpdateRecovery.Unknown(residualPath: null),
            Verification = RouteUpdateVerificationState.Verified,
            Findings = [Finding(plan, code, cause)],
        };

    private static RouteUpdateFinding Finding(
        RouteUpdatePlan plan,
        RouteUpdateFindingCode code,
        string cause)
        => new(code, cause, plan.Preview.Target.Path);

    private static RouteUpdateApplicationProgress BeforeApplication(
        RouteUpdateFinding finding,
        RouteUpdateRecovery recovery)
        => new()
        {
            Receipts = [],
            UncertainAttempt = null,
            Recovery = recovery,
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings = [finding],
        };
}
