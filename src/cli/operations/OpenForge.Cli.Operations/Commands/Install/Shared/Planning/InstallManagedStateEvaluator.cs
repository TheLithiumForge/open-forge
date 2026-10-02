using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Result;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallManagedStateEvaluator
{
    private readonly InstallContentIdentity _contentIdentity = new();
    internal InstallManagedStateResult Evaluate(
        InstallPlanningBasis basis,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(basis);
        cancellationToken.ThrowIfCancellationRequested();

        var intended = basis.Context.IntendedState;
        var ownsBase = basis.CurrentFramework is { } framework
            && (framework.Paths.Any(intended.TargetBytes.ContainsKey)
                || framework.Regions.Any(region =>
                    (region.Region == "open-forge" && intended.ManagedBlockBytes.ContainsKey(region.Path))
                    || (region.Region == "entries" && intended.GeneratedRegionPaths.Contains(region.Path))));
        if (!ownsBase)
        {
            return new InstallManagedStateEstablishment(new InstallEstablishmentPlanInput
            {
                Context = basis.Context,
                Ownership = basis.Ownership,
                CurrentTargets = basis.CurrentTargets,
            });
        }

        bool managedBaseAdmissible;
        try
        {
            managedBaseAdmissible = _contentIdentity.IsManagedBaseAdmissible(
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

        if (!managedBaseAdmissible)
        {
            return Stopped(
                basis.Context,
                InstallManagementState.ManagedDivergence,
                InstallFindingCode.ManagedDivergence,
                "The owned Framework targets differ from the current embedded payload.");
        }

        bool exact;
        try
        {
            exact = _contentIdentity.IsCurrentBaseExact(
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

        if (exact)
        {
            return new InstallManagedStateTrustedExact(basis.Context);
        }

        if (intended.Migrations.Count == 0)
        {
            return Stopped(
                basis.Context,
                InstallManagementState.ManagedDivergence,
                InstallFindingCode.ManagedDivergence,
                "The owned Framework targets differ from the current embedded payload without a planned workspace adoption.");
        }

        var verifiedManagedTargetPaths = intended.TargetBytes.Keys
            .Concat(intended.ManagedBlockBytes.Keys)
            .Where(path => !intended.UserOwnedPaths.Contains(path))
            .ToHashSet(StringComparer.Ordinal);
        return new InstallManagedStateAdoption(new InstallEstablishmentPlanInput
        {
            Context = basis.Context,
            Ownership = basis.Ownership,
            CurrentTargets = basis.CurrentTargets,
            VerifiedManagedTargetPaths = verifiedManagedTargetPaths,
        });
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
