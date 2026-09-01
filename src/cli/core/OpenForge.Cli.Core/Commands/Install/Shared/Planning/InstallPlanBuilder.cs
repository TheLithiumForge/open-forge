using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal sealed class InstallPlanBuilder
{
    private readonly InstallPlanningInspector _inspector;
    private readonly InstallPlanningBasisBuilder _basisBuilder;
    private readonly InstallManagedStateEvaluator _managedStateEvaluator;
    private readonly InstallEstablishmentPlanner _establishmentPlanner;
    private readonly InstallPlanResultProjector _resultProjector = new();

    internal InstallPlanBuilder(
        PhysicalPathResolver physicalPathResolver,
        LifecycleStore lifecycleStore,
        RecoveryBundleCatalogue recoveryCatalogue)
    {
        ArgumentNullException.ThrowIfNull(physicalPathResolver);
        ArgumentNullException.ThrowIfNull(lifecycleStore);
        ArgumentNullException.ThrowIfNull(recoveryCatalogue);
        _inspector = new InstallPlanningInspector(
            physicalPathResolver,
            lifecycleStore,
            recoveryCatalogue);
        _basisBuilder = new InstallPlanningBasisBuilder(physicalPathResolver);
        _managedStateEvaluator = new InstallManagedStateEvaluator(physicalPathResolver);
        _establishmentPlanner = new InstallEstablishmentPlanner(
            physicalPathResolver,
            lifecycleStore);
    }

    internal async ValueTask<InstallPlanBuild> BuildAsync(
        InstallRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var inspectionResult = await _inspector.InspectAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (inspectionResult is InstallInspectionStopped inspectionStopped)
        {
            return _resultProjector.ProjectBoundary(inspectionStopped.Boundary);
        }

        var inspection = ((InstallInspectionCompleted)inspectionResult).Facts;
        var basisResult = _basisBuilder.Build(inspection);
        if (basisResult is InstallPlanningBasisStopped basisStopped)
        {
            return _resultProjector.ProjectBoundary(basisStopped.Boundary);
        }

        var basis = ((InstallPlanningBasisCompleted)basisResult).Basis;
        var managedState = await _managedStateEvaluator.EvaluateAsync(
                basis,
                cancellationToken)
            .ConfigureAwait(false);
        return managedState switch
        {
            InstallManagedStateStopped stopped =>
                _resultProjector.ProjectBoundary(stopped.Boundary),
            InstallManagedStateTrustedExact trustedExact =>
                _resultProjector.ProjectTrustedExact(trustedExact.Context),
            InstallManagedStateEstablishment establishment =>
                _resultProjector.Project(_establishmentPlanner.Build(establishment.Input)),
            _ => throw new ArgumentOutOfRangeException(
                nameof(managedState),
                managedState,
                "The Install managed-state result is not defined."),
        };
    }
}
