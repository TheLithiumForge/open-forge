using OpenForge.Cli.Core.Commands.Route.Remove.Models.Operation;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Planning;
using OpenForge.Cli.Core.Commands.Route.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Route.Remove.Shared.Application;

internal sealed class RouteRemovePostRemoveVerifier
{
    internal RouteRemovePostRemoveVerification Verify(
        RouteRemovePlan plan,
        RouteRemovePlanBuild observation)
        => VerifyCore(plan, observation, allowPlannedClaims: false);

    internal RouteRemovePostRemoveVerification VerifyContentAbsence(
        RouteRemovePlan plan,
        RouteRemovePlanBuild observation)
        => VerifyCore(plan, observation, allowPlannedClaims: true);

    private static RouteRemovePostRemoveVerification VerifyCore(
        RouteRemovePlan plan,
        RouteRemovePlanBuild observation,
        bool allowPlannedClaims)
    {
        ArgumentNullException.ThrowIfNull(plan);
        ArgumentNullException.ThrowIfNull(observation);
        var formation = observation.Formation;
        if (formation.Findings.FirstOrDefault(finding =>
                finding.Code == RouteRemoveFindingCode.Interrupted) is { } interrupted)
        {
            return Interrupted(interrupted.Cause);
        }

        if (!formation.Findings.IsEmpty)
        {
            return Failed(
                formation.Findings[0].Cause);
        }

        if (observation.Plan is not null
            || formation.Workspace != plan.Request.Workspace
            || formation.Mode != plan.Request.Mode
            || formation.Source != plan.Preview.Source
            || formation.Subject.Kind != plan.Preview.Subject.Kind
            || !formation.Subject.Layers.IsEmpty
            || !formation.Subject.Items.IsEmpty)
        {
            return Failed("The final Route Remove absence observation changed its accepted subject identity.");
        }

        var plannedClaims = plan.Preview.Persistence.Ownership.Claims;
        var expectedOwnershipState = allowPlannedClaims && !plannedClaims.IsEmpty
            ? RouteRemoveOwnershipState.Claimed
            : RouteRemoveOwnershipState.Unmanaged;
        if (formation.Ownership.State != expectedOwnershipState
            || formation.Ownership.Framework != RouteRemoveOwnershipTrust.Trusted
            || formation.Ownership.Extensions != RouteRemoveOwnershipTrust.Trusted
            || allowPlannedClaims && !formation.Ownership.Claims.SequenceEqual(plannedClaims)
            || !allowPlannedClaims && !formation.Ownership.Claims.IsEmpty)
        {
            return Failed(allowPlannedClaims
                ? "The pre-publication Route Remove absence observation changed the accepted ownership claims."
                : "The final Route Remove absence observation did not establish unclaimed ownership.");
        }

        var expectedOccurrenceCount = plan.Projection.References.References.OccurrenceCount
            - plan.Projection.References.References.Detachments.Length;
        if (formation.References.Coverage != RouteRemoveCoverage.Complete
            || formation.References.ScannedSourceCount
                != formation.References.InspectedSourceCount
            || formation.References.OccurrenceCount != expectedOccurrenceCount
            || !formation.References.Detachments.IsEmpty)
        {
            return Failed("The final Route Remove absence observation did not establish complete reference absence and coverage.");
        }

        if (formation.GeneratedNavigation.Coverage != RouteRemoveCoverage.Complete
            || !formation.GeneratedNavigation.Regions.IsEmpty)
        {
            return Failed("The final Route Remove absence observation retained generated navigation exposure.");
        }

        if (formation.Plan.Completeness != RouteRemovePlanCompleteness.Complete
            || formation.Plan.Safety != RouteRemovePlanSafety.Safe
            || formation.Verification != RouteRemoveVerificationState.Verified
            || !formation.Effects.IsEmpty
            || !formation.UnchangedPaths.IsEmpty)
        {
            return Failed("The final Route Remove absence observation was not complete, safe, and verified.");
        }

        return new RouteRemovePostRemoveVerification(
            RouteRemovePostRemoveVerificationState.Verified,
            Cause: null);
    }

    private static RouteRemovePostRemoveVerification Failed(string cause)
        => new(RouteRemovePostRemoveVerificationState.Failed, cause);

    private static RouteRemovePostRemoveVerification Interrupted(string cause)
        => new(
            RouteRemovePostRemoveVerificationState.Interrupted,
            cause);
}
