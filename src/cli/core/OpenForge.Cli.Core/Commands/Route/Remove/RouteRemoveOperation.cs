using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Route.Remove;

internal sealed class RouteRemoveOperation(
    RouteRemovePlanBuilder planBuilder,
    RouteRemoveApplicationOperation applicationOperation,
    RouteRemoveResultBuilder resultBuilder)
{
    private readonly RouteRemovePlanBuilder _planBuilder = planBuilder;
    private readonly RouteRemoveApplicationOperation _applicationOperation = applicationOperation;
    private readonly RouteRemoveResultBuilder _resultBuilder = resultBuilder;

    internal RouteRemoveOperation()
        : this(RouteRemoveOperationFactory.Create())
    {
    }

    private RouteRemoveOperation(RouteRemoveOperation operation)
        : this(operation._planBuilder, operation._applicationOperation, operation._resultBuilder)
    {
    }

    internal async ValueTask<RouteRemoveResult> ExecuteAsync(
        RouteRemoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        RouteRemovePlanBuild build;
        try
        {
            build = await _planBuilder.BuildAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Remove planning was interrupted."));
        }
        catch (Exception)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteRemoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed,
                "Route Remove planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan || request.IsDryRun)
        {
            return _resultBuilder.Build(build.Formation);
        }

        if (plan.IsNoOp)
        {
            return _resultBuilder.Build(plan.Preview);
        }

        var progress = await ApplyAsync(plan, cancellationToken).ConfigureAwait(false);
        return _resultBuilder.Build(RouteRemoveApplicationResultProjector.Project(plan, progress));
    }

    private async ValueTask<RouteRemoveApplicationProgress> ApplyAsync(
        RouteRemovePlan plan,
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
                RouteRemoveFindingCode.Interrupted,
                CliSemanticStatus.Interrupted,
                "Route Remove application was interrupted unexpectedly.");
        }
        catch (Exception)
        {
            return UnknownProgress(
                plan,
                RouteRemoveFindingCode.OperationFailed,
                CliSemanticStatus.Failed,
                "Route Remove application failed unexpectedly.");
        }
    }

    private static RouteRemoveApplicationProgress UnknownProgress(
        RouteRemovePlan plan,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => new()
        {
            Recovery = new RouteRemoveRecovery
            {
                State = RouteRemoveRecoveryState.Unknown,
                ProtectedPaths = plan.Preview.Recovery.ProtectedPaths,
            },
            Verification = RouteRemoveVerificationState.Unknown,
            Findings = [new RouteRemoveFinding(code, status, plan.Preview.Source.Path, cause)],
        };

    private static RouteRemoveResultFormation InitialBoundary(
        RouteRemoveRequest request,
        RouteRemoveFindingCode code,
        CliSemanticStatus status,
        string cause)
        => RouteRemoveBoundary.Stop(
            RouteRemoveBoundary.Start(request),
            code,
            status,
            request.SourceReference,
            cause);
}
