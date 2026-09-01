using OpenForge.Cli.Core.Commands.Route.Init.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Init.Models.Result;
using OpenForge.Cli.Core.Framework.Recovery;

namespace OpenForge.Cli.Core.Commands.Route.Init.Shared.Planning;

internal sealed class RouteInitPlanBuilder
{
    private readonly RouteInitPlanningInspector _inspector = new();
    private readonly RouteInitIntendedChainBuilder _intendedChainBuilder = new();
    private readonly RouteInitProspectivePlanBuilder _prospectivePlanBuilder = new();
    private readonly RouteInitPlanResultProjector _resultProjector = new();
    private readonly RouteInitPlanFinalizer _finalizer;

    internal RouteInitPlanBuilder()
        : this(new RecoveryBundleCatalogue(new RecoveryBundleReader()))
    {
    }

    internal RouteInitPlanBuilder(RecoveryBundleCatalogue recoveryCatalogue)
    {
        ArgumentNullException.ThrowIfNull(recoveryCatalogue);
        _finalizer = new RouteInitPlanFinalizer(recoveryCatalogue);
    }

    internal async ValueTask<RouteInitPlanBuild> BuildAsync(
        RouteInitRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var inspectionResult = await _inspector.InspectAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (inspectionResult is RouteInitInspectionStopped inspectionStopped)
        {
            return _resultProjector.ProjectStopped(request, inspectionStopped.Boundary);
        }

        var inspection = ((RouteInitInspectionCompleted)inspectionResult).Facts;
        var intendedResult = await _intendedChainBuilder.BuildAsync(
                request,
                inspection,
                cancellationToken)
            .ConfigureAwait(false);
        if (intendedResult is RouteInitIntendedChainStopped intendedStopped)
        {
            return _resultProjector.ProjectStopped(request, intendedStopped.Boundary);
        }

        var intended = ((RouteInitIntendedChainCompleted)intendedResult).Chain;
        var prospectiveResult = await _prospectivePlanBuilder.BuildAsync(
                request,
                inspection,
                intended,
                cancellationToken)
            .ConfigureAwait(false);
        if (prospectiveResult is RouteInitProspectivePlanStopped prospectiveStopped)
        {
            return _resultProjector.ProjectStopped(request, prospectiveStopped.Boundary);
        }

        var prospective = ((RouteInitProspectivePlanCompleted)prospectiveResult).Facts;
        var finalizationResult = await _finalizer.FinalizeAsync(
                request,
                inspection,
                prospective,
                cancellationToken)
            .ConfigureAwait(false);
        if (finalizationResult is RouteInitPlanFinalizationStopped finalizationStopped)
        {
            return _resultProjector.ProjectStopped(request, finalizationStopped.Boundary);
        }

        var finalization = ((RouteInitPlanFinalizationCompleted)finalizationResult).Facts;
        return _resultProjector.ProjectCompleted(
            request,
            inspection,
            intended,
            prospective,
            finalization);
    }
}
