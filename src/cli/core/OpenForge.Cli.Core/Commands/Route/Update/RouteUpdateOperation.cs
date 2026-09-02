using OpenForge.Cli.Core.Commands.Route.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Update.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Planning;
using OpenForge.Cli.Core.Commands.Route.Update.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Route.Update;

internal sealed class RouteUpdateOperation
{
    private readonly RouteUpdatePlanBuilder _planBuilder;
    private readonly RouteUpdateApplicationOperation _applicationOperation;
    private readonly RouteUpdateResultBuilder _resultBuilder;

    internal RouteUpdateOperation(
        RouteUpdatePlanBuilder planBuilder,
        RouteUpdateApplicationOperation applicationOperation,
        RouteUpdateResultBuilder resultBuilder)
    {
        _planBuilder = planBuilder;
        _applicationOperation = applicationOperation;
        _resultBuilder = resultBuilder;
    }

    internal async ValueTask<RouteUpdateResult> ExecuteAsync(
        RouteUpdateRequest request,
        CancellationToken cancellationToken)
    {
        RouteUpdatePlanBuild build;
        try
        {
            build = await _planBuilder.BuildAsync(request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteUpdateFindingCode.Interrupted,
                "Route Update planning was interrupted."));
        }
        catch (Exception)
        {
            return _resultBuilder.Build(InitialBoundary(
                request,
                RouteUpdateFindingCode.OperationFailed,
                "Route Update planning failed unexpectedly."));
        }

        if (build.Plan is not { } plan)
        {
            var formation = build.Formation;
            if (formation.Findings.Any(finding =>
                    finding.Code == RouteUpdateFindingCode.Interrupted))
            {
                formation = formation with
                {
                    Recovery = RouteUpdateRecovery.NotCreated(),
                };
            }

            return _resultBuilder.Build(formation);
        }

        if (request.IsDryRun)
        {
            return _resultBuilder.Build(plan.Preview);
        }

        if (plan.IsNoOp)
        {
            return _resultBuilder.Build(plan.Preview with
            {
                Recovery = RouteUpdateRecovery.NotRequired(),
                Verification = RouteUpdateVerificationState.Verified,
            });
        }

        return await _applicationOperation.ExecuteAsync(plan, cancellationToken)
            .ConfigureAwait(false);
    }

    private static RouteUpdateResultFormation InitialBoundary(
        RouteUpdateRequest request,
        RouteUpdateFindingCode code,
        string cause)
        => new()
        {
            Workspace = request.Workspace,
            Mode = request.Mode,
            Target = RouteUpdateTarget.Unresolved(request.SourceReference),
            Patch = RouteUpdatePatch.Unresolved(request.Patch),
            Template = request.TemplateReference is { } template
                ? RouteUpdateTemplate.Unresolved(template)
                : null,
            Plan = new RouteUpdatePlanFacts
            {
                Completeness = RouteUpdatePlanCompleteness.Incomplete,
                Safety = RouteUpdatePlanSafety.NotEstablished,
                Body = RouteUpdateBodyState.NotEstablished,
            },
            Effects = [],
            UnchangedPaths = [],
            Recovery = RouteUpdateRecovery.NotCreated(),
            Verification = RouteUpdateVerificationState.NotRequested,
            Findings = [new RouteUpdateFinding(code, cause, request.SourceReference)],
        };
}
