using OpenForge.Cli.Core.Commands.Route.Move.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Move.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Move.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Move;

internal sealed class RouteMoveOperation(
    RouteMovePlanBuilder planBuilder,
    RouteMoveApplicationOperation applicationOperation,
    RouteMoveResultBuilder resultBuilder)
{
    private readonly RouteMovePlanBuilder _planBuilder = planBuilder;
    private readonly RouteMoveApplicationOperation _applicationOperation = applicationOperation;
    private readonly RouteMoveResultBuilder _resultBuilder = resultBuilder;

    internal async ValueTask<RouteMoveResult> ExecuteAsync(
        RouteMoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        RouteMovePlanBuild build;
        try
        {
            build = await _planBuilder.BuildAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Move planning was interrupted."));
        }
        catch (Exception)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteMoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed,
                "Route Move planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan || request.IsDryRun)
        {
            return _resultBuilder.Build(build.Formation);
        }

        if (plan.IsNoOp)
        {
            return _resultBuilder.Build(RouteMoveBoundary.Stop(
                plan.Preview,
                RouteMoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed,
                plan.Preview.Source.Path,
                "An ordinary Route Move cannot form an empty application plan."));
        }

        var progress = await ApplyAsync(plan, cancellationToken).ConfigureAwait(false);
        return _resultBuilder.Build(RouteMoveApplicationResultProjector.Project(plan, progress));
    }

    private async ValueTask<RouteMoveApplicationProgress> ApplyAsync(
        RouteMovePlan plan,
        CancellationToken cancellationToken)
    {
        try
        {
            return await _applicationOperation.ExecuteAsync(
                plan,
                Guid.NewGuid(),
                cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return UnknownProgress(
                plan,
                RouteMoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Move application was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return UnknownProgress(
                plan,
                RouteMoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed,
                "Route Move application failed unexpectedly.");
        }
    }

    private static RouteMoveApplicationProgress UnknownProgress(
        RouteMovePlan plan,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new()
        {
            Recovery = new RouteMoveRecovery
            {
                State = RouteMoveRecoveryState.Unknown,
                ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
            },
            Verification = RouteMoveVerificationState.Unknown,
            Findings = [new RouteMoveFinding(code, status, plan.Preview.Destination.Path, cause)],
        };

    private static RouteMoveResultFormation InitialBoundary(
        RouteMoveRequest request,
        RouteMoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => RouteMoveBoundary.Stop(
            RouteMoveBoundary.Start(request),
            code,
            status,
            request.SourceReference,
            cause);
}
