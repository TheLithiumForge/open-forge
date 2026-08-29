using OpenForge.Cli.Core.Commands.Index.Models.Selection;

namespace OpenForge.Cli.Core.Commands.Index.Models.Planning;

internal enum IndexRegionAction
{
    NotEstablished,
    Unchanged,
    Update,
}

internal enum IndexRegionOutcome
{
    NotEstablished,
    AlreadyCurrent,
    NotRequested,
    NotStarted,
    Applied,
    Verified,
    Unknown,
}

internal sealed record IndexChange
{
    internal IndexChange(string beforeBody, string expectedBody)
    {
        ArgumentNullException.ThrowIfNull(beforeBody);
        ArgumentNullException.ThrowIfNull(expectedBody);
        if (string.Equals(beforeBody, expectedBody, StringComparison.Ordinal))
        {
            throw new ArgumentException("An Index change requires distinct before and expected bodies.", nameof(expectedBody));
        }

        BeforeBody = beforeBody;
        ExpectedBody = expectedBody;
    }

    internal string BeforeBody { get; }

    internal string ExpectedBody { get; }
}

internal sealed record IndexRegion
{
    private readonly IndexRegionState _state;

    private IndexRegion(
        IndexLogicalSource source,
        IndexRegionAction action,
        IndexRegionState state)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(state);
        _ = state.Outcome switch
        {
            IndexRegionOutcome.NotEstablished
                or IndexRegionOutcome.AlreadyCurrent
                or IndexRegionOutcome.NotRequested
                or IndexRegionOutcome.NotStarted
                or IndexRegionOutcome.Applied
                or IndexRegionOutcome.Verified
                or IndexRegionOutcome.Unknown => true,
            _ => throw new ArgumentOutOfRangeException(
                nameof(state),
                state.Outcome,
                "The Index region outcome is not defined."),
        };

        if (state.BeforeEntryCount is < 0 || state.ExpectedEntryCount is < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(state), "Index entry counts cannot be negative.");
        }

        ValidateCoherence(action, state);
        Source = source;
        Action = action;
        _state = state;
    }

    internal IndexLogicalSource Source { get; }

    internal IndexRegionAction Action { get; }

    internal int? BeforeEntryCount => _state.BeforeEntryCount;

    internal int? ExpectedEntryCount => _state.ExpectedEntryCount;

    internal IndexChange? Change => _state.Change;

    internal IndexRegionOutcome Outcome => _state.Outcome;

    internal IndexRegion WithOutcome(IndexRegionOutcome outcome)
        => new(Source, Action, _state.WithOutcome(outcome));

    internal static IndexRegion NotEstablished(IndexLogicalSource source)
        => new(
            source,
            IndexRegionAction.NotEstablished,
            new IndexRegionState
            {
                BeforeEntryCount = null,
                ExpectedEntryCount = null,
                Change = null,
                Outcome = IndexRegionOutcome.NotEstablished,
            });

    internal static IndexRegion Unchanged(
        IndexLogicalSource source,
        int beforeEntryCount,
        int expectedEntryCount)
        => new(
            source,
            IndexRegionAction.Unchanged,
            new IndexRegionState
            {
                BeforeEntryCount = beforeEntryCount,
                ExpectedEntryCount = expectedEntryCount,
                Change = null,
                Outcome = IndexRegionOutcome.AlreadyCurrent,
            });

    internal static IndexRegion Update(
        IndexLogicalSource source,
        IndexRegionUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);
        return new(
            source,
            IndexRegionAction.Update,
            new IndexRegionState
            {
                BeforeEntryCount = update.BeforeEntryCount,
                ExpectedEntryCount = update.ExpectedEntryCount,
                Change = update.Change,
                Outcome = update.Outcome,
            });
    }

    private static void ValidateCoherence(
        IndexRegionAction action,
        IndexRegionState state)
    {
        var valid = action switch
        {
            IndexRegionAction.NotEstablished => state.BeforeEntryCount is null
                && state.ExpectedEntryCount is null
                && state.Change is null
                && state.Outcome == IndexRegionOutcome.NotEstablished,
            IndexRegionAction.Unchanged => state.BeforeEntryCount is not null
                && state.ExpectedEntryCount is not null
                && state.Change is null
                && state.Outcome == IndexRegionOutcome.AlreadyCurrent,
            IndexRegionAction.Update => state.ExpectedEntryCount is not null
                && state.Change is not null
                && state.Outcome is IndexRegionOutcome.NotRequested
                    or IndexRegionOutcome.NotStarted
                    or IndexRegionOutcome.Applied
                    or IndexRegionOutcome.Verified
                    or IndexRegionOutcome.Unknown,
            _ => throw new ArgumentOutOfRangeException(
                nameof(action),
                action,
                "The Index region action is not defined."),
        };
        if (!valid)
        {
            throw new ArgumentException("The Index region facts are not coherent with its action and outcome.");
        }
    }

    private sealed record IndexRegionState
    {
        public required int? BeforeEntryCount { get; init; }

        public required int? ExpectedEntryCount { get; init; }

        public required IndexChange? Change { get; init; }

        public required IndexRegionOutcome Outcome { get; init; }

        internal IndexRegionState WithOutcome(IndexRegionOutcome outcome)
            => this with { Outcome = outcome };
    }
}

internal sealed record IndexRegionUpdate
{
    public required int? BeforeEntryCount { get; init; }

    public required int ExpectedEntryCount { get; init; }

    public required IndexChange Change { get; init; }

    public required IndexRegionOutcome Outcome { get; init; }
}
