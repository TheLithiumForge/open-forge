using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;
using OpenForge.Cli.Core.Commands.Route.Remove.Shared.References;
using OpenForge.Cli.Core.Commands.Route.Shared.Models.References;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Planning;

internal sealed class RouteRemovePlanBuilder(
    RouteRemoveSubjectResolver subjectResolver,
    RouteRemoveCategoryInventoryReader inventoryReader,
    RouteRemoveReferencePlanner referencePlanner,
    RouteRemoveNavigationPlanner navigationPlanner,
    RouteRemoveCategoryAbsencePlanner absencePlanner)
{
    private readonly RouteRemoveSubjectResolver _subjectResolver = subjectResolver;
    private readonly RouteRemoveCategoryInventoryReader _inventoryReader = inventoryReader;
    private readonly RouteRemoveReferencePlanner _referencePlanner = referencePlanner;
    private readonly RouteRemoveNavigationPlanner _navigationPlanner = navigationPlanner;
    private readonly RouteRemoveCategoryAbsencePlanner _absencePlanner = absencePlanner;

    internal async ValueTask<RouteRemovePlanBuild> BuildAsync(
        RouteRemoveRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        var subject = await _subjectResolver.ResolveAsync(
            request,
            cancellationToken).ConfigureAwait(false);
        if (subject.Boundary is { } subjectBoundary)
        {
            if (subjectBoundary.Findings.Any(finding =>
                    finding.Code == RouteRemoveFindingCode.SourceNotFound)
                && RouteRemoveAbsenceScope.TryCreate(request) is { } scope)
            {
                return await _absencePlanner.BuildAsync(scope, cancellationToken)
                    .ConfigureAwait(false);
            }

            return new RouteRemovePlanBuild(plan: null, subjectBoundary);
        }

        var resolved = subject.Subject
            ?? throw new InvalidOperationException("Successful Route Remove resolution requires its subject.");
        var inventory = await _inventoryReader.ReadAsync(
            resolved,
            cancellationToken).ConfigureAwait(false);
        if (inventory.Boundary is { } inventoryBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, inventoryBoundary);
        }

        var facts = inventory.Inventory
            ?? throw new InvalidOperationException("Successful Route Remove inventory requires its facts.");
        var references = await _referencePlanner.BuildAsync(
            new RouteRemoveReferencePlanningRequest
            {
                Subject = resolved,
                CatalogueRequest = new RouteMarkdownCatalogueRequest(
                    request.Workspace,
                    ["."],
                    [],
                    new RouteMarkdownCatalogueFilters([".md"])),
            },
            facts,
            cancellationToken).ConfigureAwait(false);
        if (references.Boundary is { } referenceBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, referenceBoundary with
            {
                Ownership = RouteRemoveOwnershipProjector.Project(facts.Ownership, resolved),
            });
        }

        var referencePlan = references.Plan
            ?? throw new InvalidOperationException("Successful Route Remove reference planning requires its plan.");
        var navigation = _navigationPlanner.Build(resolved);
        if (navigation.Boundary is { } navigationBoundary)
        {
            return new RouteRemovePlanBuild(plan: null, navigationBoundary with
            {
                Ownership = RouteRemoveOwnershipProjector.Project(facts.Ownership, resolved),
                References = referencePlan.References,
            });
        }

        var projection = RouteRemoveEffectPlanner.Build(
            facts,
            referencePlan,
            navigation.Plan
                ?? throw new InvalidOperationException("Successful Route Remove navigation planning requires its plan."));
        var plan = RouteRemovePlanProjector.Build(projection, facts);
        return new RouteRemovePlanBuild(plan, plan.Preview);
    }

    internal ValueTask<RouteRemovePlanBuild> BuildAbsenceAsync(
        RouteRemovePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return _absencePlanner.BuildPostRemoveAsync(
            RouteRemoveAbsenceScope.FromPlan(plan),
            plan,
            cancellationToken);
    }
}
