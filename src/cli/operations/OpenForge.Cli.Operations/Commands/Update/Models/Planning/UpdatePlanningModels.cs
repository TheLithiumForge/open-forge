using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Commands.Update.Models.Comparison;
using OpenForge.Cli.Core.Commands.Update.Models.Result;

namespace OpenForge.Cli.Core.Commands.Update.Models.Planning;

internal sealed record UpdatePlanningAuthority
{
    internal UpdatePlanningAuthority(bool force, bool prune)
    {
        Force = force;
        Prune = prune;
    }

    internal bool Force { get; }

    internal bool Prune { get; }
}

internal enum UpdatePlanningDisposition
{
    NoOp,
    Create,
    Replace,
    Restore,
    Delete,
    Preserve,
    Blocked,
}

internal sealed record UpdatePlanningDecision
{
    internal UpdatePlanningDecision(
        UpdateComparison comparison,
        UpdatePlanningDisposition disposition)
    {
        ArgumentNullException.ThrowIfNull(comparison);
        if (!Enum.IsDefined(disposition))
        {
            throw new ArgumentOutOfRangeException(
                nameof(disposition),
                disposition,
                "The Update planning disposition is not defined.");
        }

        Comparison = comparison;
        Disposition = disposition;
    }

    internal UpdateComparison Comparison { get; }

    internal UpdatePlanningDisposition Disposition { get; }
}

internal sealed record UpdatePlanningPlan
{
    internal UpdatePlanningPlan(
        UpdatePlanningAuthority authority,
        IEnumerable<UpdatePlanningDecision> decisions)
    {
        ArgumentNullException.ThrowIfNull(authority);
        ArgumentNullException.ThrowIfNull(decisions);
        var materialized = decisions
            .Select(decision => decision ?? throw new ArgumentException(
                "Update planning decisions cannot contain null members.",
                nameof(decisions)))
            .ToArray();
        Authority = authority;
        Decisions = new ReadOnlyCollection<UpdatePlanningDecision>(materialized);
    }

    internal UpdatePlanningAuthority Authority { get; }

    internal IReadOnlyList<UpdatePlanningDecision> Decisions { get; }

    internal bool IsNoOp => Decisions.All(
        decision => decision.Disposition == UpdatePlanningDisposition.NoOp);

    internal bool IsEffectFree => Decisions.All(
        decision => decision.Disposition is UpdatePlanningDisposition.NoOp
            or UpdatePlanningDisposition.Preserve);

    internal bool IsBlocked => Decisions.Any(
        decision => decision.Disposition == UpdatePlanningDisposition.Blocked);
}

internal sealed record UpdatePlanBuild
{
    internal UpdatePlanBuild(UpdatePlanningPlan? plan, UpdateResult preview)
    {
        ArgumentNullException.ThrowIfNull(preview);
        Plan = plan;
        Preview = preview;
    }

    internal UpdatePlanningPlan? Plan { get; }

    internal UpdateResult Preview { get; }
}
