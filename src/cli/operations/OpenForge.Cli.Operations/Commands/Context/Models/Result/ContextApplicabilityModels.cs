using OpenForge.Cli.Core.Commands.Context.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Context.Models.Result;

internal enum ContextApplicabilityState
{
    Unconditioned,
    Matched,
    Unmatched,
    Pending,
    Invalid,
}

internal sealed record ContextApplicabilityCondition
{
    internal ContextApplicabilityCondition(string source, IEnumerable<string> patterns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(patterns);
        Source = source;
        Patterns = ContextResultCollections.Snapshot(patterns, nameof(patterns));
    }

    internal string Source { get; }

    internal IReadOnlyList<string> Patterns { get; }
}

internal sealed record ContextApplicability
{
    internal ContextApplicability(
        ContextApplicabilityState state,
        IEnumerable<ContextApplicabilityCondition> conditions,
        IEnumerable<string> matchingPaths)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Context applicability state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(matchingPaths);
        State = state;
        Conditions = ContextResultCollections.Snapshot(conditions, nameof(conditions));
        MatchingPaths = ContextResultCollections.Snapshot(matchingPaths, nameof(matchingPaths));
    }

    internal ContextApplicabilityState State { get; }

    internal IReadOnlyList<ContextApplicabilityCondition> Conditions { get; }

    internal IReadOnlyList<string> MatchingPaths { get; }
}

internal sealed record ContextPendingCondition
{
    internal ContextPendingCondition(string source, IEnumerable<string> patterns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(patterns);
        Source = source;
        Patterns = ContextResultCollections.Snapshot(patterns, nameof(patterns));
    }

    internal string Source { get; }

    internal IReadOnlyList<string> Patterns { get; }
}
