namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal enum ContextOptionalCoverageState
{
    NotRequested,
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed record ContextCoverage
{
    internal ContextCoverage(
        ContextCoverageState state,
        ContextCoverageState selection,
        ContextOptionalCoverageState links,
        ContextCoverageState projection)
    {
        if (!Enum.IsDefined(state) || !Enum.IsDefined(selection) || !Enum.IsDefined(links) || !Enum.IsDefined(projection))
        {
            throw new ArgumentException("Every Context coverage value must be defined.");
        }

        State = state;
        Selection = selection;
        Links = links;
        Projection = projection;
    }

    internal ContextCoverageState State { get; }

    internal ContextCoverageState Selection { get; }

    internal ContextOptionalCoverageState Links { get; }

    internal ContextCoverageState Projection { get; }
}
