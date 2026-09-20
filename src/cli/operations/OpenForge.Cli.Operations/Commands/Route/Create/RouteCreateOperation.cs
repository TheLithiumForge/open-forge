using OpenForge.Cli.Core.Commands.Route.Create.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Create.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Create.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Route.Create;

internal sealed class RouteCreateOperation
{
    private readonly RouteCreatePlanBuilder _planBuilder;
    private readonly RouteCreateApplicationOperation _applicationOperation;
    private readonly RouteCreateResultBuilder _resultBuilder;

    internal RouteCreateOperation(
        RouteCreatePlanBuilder planBuilder,
        RouteCreateApplicationOperation applicationOperation,
        RouteCreateResultBuilder resultBuilder)
    {
        _planBuilder = planBuilder;
        _applicationOperation = applicationOperation;
        _resultBuilder = resultBuilder;
    }

    internal async ValueTask<RouteCreateResult> ExecuteAsync(
        RouteCreateRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var build = await _planBuilder.BuildAsync(request, cancellationToken)
            .ConfigureAwait(false);
        if (build.Plan is not { } plan || request.IsDryRun || plan.IsNoOp)
        {
            return _resultBuilder.Build(build.Formation);
        }

        var applied = await _applicationOperation.ExecuteAsync(plan, cancellationToken)
            .ConfigureAwait(false);
        return _resultBuilder.Build(applied);
    }
}
