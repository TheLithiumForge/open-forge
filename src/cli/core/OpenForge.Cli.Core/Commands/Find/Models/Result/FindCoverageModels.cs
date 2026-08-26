namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal enum FindCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum FindProjectionCoverageState
{
    NotRequested,
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record FindCoverage
{
    internal FindCoverage(
        FindCoverageState state,
        FindCoverageState matching,
        FindProjectionCoverageState projection)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Find coverage state is not defined.");
        }

        if (!Enum.IsDefined(matching))
        {
            throw new ArgumentOutOfRangeException(nameof(matching), matching, "The Find matching coverage state is not defined.");
        }

        if (!Enum.IsDefined(projection))
        {
            throw new ArgumentOutOfRangeException(nameof(projection), projection, "The Find projection coverage state is not defined.");
        }

        State = state;
        Matching = matching;
        Projection = projection;
    }

    internal FindCoverageState State { get; }

    internal FindCoverageState Matching { get; }

    internal FindProjectionCoverageState Projection { get; }
}
