using OpenForge.Cli.Core.Commands.Remove.Models.Result;

namespace OpenForge.Cli.Core.Commands.Remove.Models.Planning;

internal abstract record RemovePathPlanningOutcome
{
    private RemovePathPlanningOutcome()
    {
    }

    internal sealed record Planned(RemovePathPlan Plan) : RemovePathPlanningOutcome;

    internal sealed record Stopped(RemoveResult Result) : RemovePathPlanningOutcome;
}
