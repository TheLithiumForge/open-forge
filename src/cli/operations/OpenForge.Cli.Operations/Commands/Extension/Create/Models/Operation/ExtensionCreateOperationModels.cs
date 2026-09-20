using OpenForge.Cli.Core.Commands.Extension.Create.Models.Planning;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Result;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Models.Operation;

internal sealed record ExtensionCreatePlanningOutcome
{
    private ExtensionCreatePlanningOutcome(
        ExtensionCreatePlan? plan,
        ExtensionCreateResult? result)
    {
        if ((plan is null) == (result is null))
        {
            throw new ArgumentException("A planning outcome requires exactly one plan or terminal result.");
        }

        Plan = plan;
        Result = result;
    }

    internal ExtensionCreatePlan? Plan { get; }

    internal ExtensionCreateResult? Result { get; }

    internal static ExtensionCreatePlanningOutcome Planned(ExtensionCreatePlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        return new ExtensionCreatePlanningOutcome(plan, null);
    }

    internal static ExtensionCreatePlanningOutcome Terminal(ExtensionCreateResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return new ExtensionCreatePlanningOutcome(null, result);
    }
}

internal sealed record ExtensionCreateEffectApplication
{
    internal ExtensionCreateEffectApplication(
        bool applied,
        ExtensionCreateFinding? finding)
    {
        if (applied == (finding is not null))
        {
            throw new ArgumentException("An effect application requires either an applied receipt or one finding.");
        }

        Applied = applied;
        Finding = finding;
    }

    internal bool Applied { get; }

    internal ExtensionCreateFinding? Finding { get; }
}
