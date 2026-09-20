using System.Collections.ObjectModel;

namespace OpenForge.Cli.Core.Commands.Route.List;

internal enum RouteListCoverageState
{
    NotStarted,
    Complete,
    Incomplete,
    Blocked,
    Failed,
    Interrupted,
}

internal sealed class RouteListCoverage
{
    private RouteListCoverage(
        RouteListCoverageState state,
        RouteListDepth? requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IReadOnlyList<string> evidence,
        IReadOnlyList<string> unresolvedBoundaries)
    {
        State = state;
        RequestedDepth = requestedDepth;
        EffectiveDepth = effectiveDepth;
        SelectedRootCount = selectedRootCount;
        ConfirmedRowCount = confirmedRowCount;
        Evidence = evidence;
        UnresolvedBoundaries = unresolvedBoundaries;
    }

    internal RouteListCoverageState State { get; }

    internal RouteListDepth? RequestedDepth { get; }

    internal RouteListDepth? EffectiveDepth { get; }

    internal int SelectedRootCount { get; }

    internal int ConfirmedRowCount { get; }

    internal IReadOnlyList<string> Evidence { get; }

    internal IReadOnlyList<string> UnresolvedBoundaries { get; }

    internal static RouteListCoverage NotStarted(RouteListDepth? requestedDepth)
    {
        return new RouteListCoverage(
            RouteListCoverageState.NotStarted,
            requestedDepth,
            null,
            0,
            0,
            Array.AsReadOnly(Array.Empty<string>()),
            Array.AsReadOnly(Array.Empty<string>()));
    }

    internal static RouteListCoverage Complete(
        RouteListDepth requestedDepth,
        RouteListDepth effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence)
    {
        return Create(
            RouteListCoverageState.Complete,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            evidence,
            []);
    }

    internal static RouteListCoverage Incomplete(
        RouteListDepth requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        return Create(
            RouteListCoverageState.Incomplete,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            evidence,
            unresolvedBoundaries);
    }

    internal static RouteListCoverage Blocked(
        RouteListDepth? requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        return Create(
            RouteListCoverageState.Blocked,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            evidence,
            unresolvedBoundaries);
    }

    internal static RouteListCoverage Failed(
        RouteListDepth? requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        return Create(
            RouteListCoverageState.Failed,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            evidence,
            unresolvedBoundaries);
    }

    internal static RouteListCoverage Interrupted(
        RouteListDepth requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        return Create(
            RouteListCoverageState.Interrupted,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            evidence,
            unresolvedBoundaries);
    }

    private static RouteListCoverage Create(
        RouteListCoverageState state,
        RouteListDepth? requestedDepth,
        RouteListDepth? effectiveDepth,
        int selectedRootCount,
        int confirmedRowCount,
        IEnumerable<string> evidence,
        IEnumerable<string> unresolvedBoundaries)
    {
        if (!Enum.IsDefined(state) || state == RouteListCoverageState.NotStarted)
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The coverage state requires another factory.");
        }

        if (state is RouteListCoverageState.Complete or RouteListCoverageState.Incomplete or RouteListCoverageState.Interrupted)
        {
            ArgumentNullException.ThrowIfNull(requestedDepth);
        }

        if (state == RouteListCoverageState.Complete)
        {
            ArgumentNullException.ThrowIfNull(effectiveDepth);
        }

        if (selectedRootCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(selectedRootCount), selectedRootCount, "Selected root count cannot be negative.");
        }

        if (confirmedRowCount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(confirmedRowCount), confirmedRowCount, "Confirmed row count cannot be negative.");
        }

        var materializedEvidence = Materialize(evidence, nameof(evidence));
        var materializedBoundaries = Materialize(unresolvedBoundaries, nameof(unresolvedBoundaries));
        if (state == RouteListCoverageState.Complete && materializedBoundaries.Count != 0)
        {
            throw new ArgumentException("Complete coverage cannot contain an unresolved boundary.", nameof(unresolvedBoundaries));
        }

        if (state != RouteListCoverageState.Complete && materializedBoundaries.Count == 0)
        {
            throw new ArgumentException("Non-complete coverage requires an unresolved boundary.", nameof(unresolvedBoundaries));
        }

        return new RouteListCoverage(
            state,
            requestedDepth,
            effectiveDepth,
            selectedRootCount,
            confirmedRowCount,
            materializedEvidence,
            materializedBoundaries);
    }

    private static IReadOnlyList<string> Materialize(IEnumerable<string> values, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(values, parameterName);
        var materialized = values.ToArray();
        if (materialized.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Coverage facts cannot contain an empty value.", parameterName);
        }

        return new ReadOnlyCollection<string>(materialized);
    }
}
