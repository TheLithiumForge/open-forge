using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallManagedStateEvaluator
{
    private readonly InstallContentIdentity _contentIdentity = new();
    private readonly InstallPreservationVerifier _preservationVerifier;

    internal InstallManagedStateEvaluator(PhysicalPathResolver physicalPathResolver)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        _preservationVerifier = new InstallPreservationVerifier(physicalPathResolver);
    }

    internal async ValueTask<InstallManagedStateResult> EvaluateAsync(
        InstallPlanningBasis basis,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(basis);

        if (basis.CurrentFramework is null)
        {
            return new InstallManagedStateEstablishment(new InstallEstablishmentPlanInput
            {
                Context = basis.Context,
                Lifecycle = basis.Lifecycle,
                CurrentTargets = basis.CurrentTargets,
            });
        }

        bool exact;
        try
        {
            exact = _contentIdentity.IsCurrentBaseExact(
                basis.CurrentFramework,
                basis.Context.IntendedLifecycle,
                basis.CurrentTargets,
                basis.Context.IntendedState);
        }
        catch (Exception exception) when (exception is ArgumentException
            or InvalidDataException
            or InvalidOperationException)
        {
            return Stopped(
                basis.Context,
                InstallManagementState.Blocked,
                InstallFindingCode.LifecycleBlocked,
                $"The managed Framework state cannot be verified safely: {exception.Message}");
        }

        if (!exact)
        {
            return Stopped(
                basis.Context,
                InstallManagementState.ManagedDivergence,
                InstallFindingCode.ManagedDivergence,
                "The trusted managed Framework state differs from its embedded current baseline.");
        }

        var preservation = await _preservationVerifier.VerifyAsync(
                basis.CurrentFramework,
                basis.Context.IntendedState,
                basis.Context.Request.Workspace,
                cancellationToken)
            .ConfigureAwait(false);
        if (ReadPreservationBoundary(basis.Context, preservation) is { } boundary)
        {
            return new InstallManagedStateStopped(boundary);
        }

        return new InstallManagedStateTrustedExact(basis.Context);
    }

    private static InstallPlanningBoundary? ReadPreservationBoundary(
        InstallPlanContext context,
        InstallPreservationVerification preservation)
    {
        return preservation.State switch
        {
            InstallPreservationVerificationState.Verified => null,
            InstallPreservationVerificationState.Changed
                or InstallPreservationVerificationState.Blocked => Boundary(
                    context,
                    InstallManagementState.Blocked,
                    InstallFindingCode.LifecycleBlocked,
                    preservation.Cause
                        ?? "A preserved scoped Framework target is changed or unsafe.",
                    preservation.Subject),
            InstallPreservationVerificationState.Incomplete => Boundary(
                    context,
                    InstallManagementState.Incomplete,
                    InstallFindingCode.LifecycleUnavailable,
                    preservation.Cause
                        ?? "A preserved scoped Framework target is unavailable.",
                    preservation.Subject),
            InstallPreservationVerificationState.Cancelled => Boundary(
                    context,
                    InstallManagementState.Interrupted,
                    InstallFindingCode.Interrupted,
                    preservation.Cause
                        ?? "Scoped Framework preservation verification was interrupted.",
                    preservation.Subject),
            _ => throw new ArgumentOutOfRangeException(
                nameof(preservation),
                preservation.State,
                "The scoped Framework preservation state is not defined."),
        };
    }

    private static InstallManagedStateStopped Stopped(
        InstallPlanContext context,
        InstallManagementState state,
        InstallFindingCode code,
        string cause)
        => new(Boundary(context, state, code, cause));

    private static InstallPlanningBoundary Boundary(
        InstallPlanContext context,
        InstallManagementState state,
        InstallFindingCode code,
        string cause,
        string? subject = null)
        => new(
            state,
            code,
            cause,
            subject,
            context.Payload,
            context.IntendedState);
}
