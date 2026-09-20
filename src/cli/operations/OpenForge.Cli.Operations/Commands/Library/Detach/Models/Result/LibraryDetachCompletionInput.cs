using OpenForge.Cli.Core.Commands.Library.Detach.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Detach.Models.Planning;
using OpenForge.Cli.Core.Commands.Library.Models.Application;

namespace OpenForge.Cli.Core.Commands.Library.Detach.Models.Result;

internal sealed record LibraryDetachCompletionInput
{
    public required LibraryDetachRequest Request { get; init; }
    public required LibraryDetachPlan? Plan { get; init; }
    public required LibraryDetachPlanningInput? Observations { get; init; }
    public required LibraryExecutionEvidence Execution { get; init; }

    internal void Validate()
    {
        ArgumentNullException.ThrowIfNull(Request);
        ArgumentNullException.ThrowIfNull(Execution);
        if (!Enum.IsDefined(Request.Mode))
        {
            throw new ArgumentOutOfRangeException(nameof(Request), Request.Mode, "The Library execution mode is not defined.");
        }

        if (Plan is { } plan && plan.Input.Request != Request)
        {
            throw new ArgumentException("The completion plan must belong to its request.", nameof(Plan));
        }

        if (Observations is { } observations && observations.Request != Request)
        {
            throw new ArgumentException("Completion observations must belong to their request.", nameof(Observations));
        }

        if (Execution.RecoveryPreparation is { } preparation && !preparation.MatchesWorkspace(Request.Workspace))
        {
            throw new ArgumentException("Recovery evidence must belong to the request's workspace.", nameof(Execution));
        }

        Execution.Validate();
    }
}
