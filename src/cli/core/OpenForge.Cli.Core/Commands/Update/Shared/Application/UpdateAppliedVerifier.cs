using OpenForge.Cli.Core.Commands.Update.Models.Operation;
using OpenForge.Cli.Core.Commands.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Mutation.Locking.Models;

namespace OpenForge.Cli.Core.Commands.Update.Shared.Application;

internal sealed class UpdateAppliedVerifier(UpdatePlanBuilder planBuilder)
{
    private readonly UpdatePlanBuilder _planBuilder = planBuilder;

    internal async ValueTask<UpdateAppliedVerification> VerifyAsync(
        UpdatePlanExecution applied,
        WorkspaceLockLease lease,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(applied);
        ArgumentNullException.ThrowIfNull(lease);
        if (!lease.IsHeldFor(applied.Request.Workspace))
        {
            return Failed("Final Update verification does not hold the selected workspace lease.");
        }

        UpdatePlanResolution current;
        try
        {
            current = await _planBuilder
                .BuildExecutionAsync(applied.Request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return new UpdateAppliedVerification(
                UpdateVerificationState.Unknown,
                new UpdateFinding(
                    UpdateFindingCode.Interrupted,
                    target: null,
                    "Final Update verification was interrupted."));
        }
        catch (Exception)
        {
            return new UpdateAppliedVerification(
                UpdateVerificationState.Unknown,
                new UpdateFinding(
                    UpdateFindingCode.OperationFailed,
                    target: null,
                    "Final Update verification failed unexpectedly."));
        }

        if (current.Execution is not { } verified
            || verified.Build.Plan is not { IsNoOp: true }
            || verified.Effects.Count != 0
            || verified.LifecycleChange is not null
            || verified.Build.Preview.Findings.Count != 0)
        {
            return Failed(
                current.Build.Preview.Findings.FirstOrDefault()?.Cause
                    ?? "The applied Update workspace does not match the complete current intended state.");
        }

        return new UpdateAppliedVerification(UpdateVerificationState.Verified, Finding: null);
    }

    private static UpdateAppliedVerification Failed(string cause)
        => new(
            UpdateVerificationState.Failed,
            new UpdateFinding(
                UpdateFindingCode.VerificationFailed,
                target: null,
                cause));
}
