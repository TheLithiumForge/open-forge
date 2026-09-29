using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Commands.Route.Inspect.Models.Result;

internal sealed class RouteInspectApplicability
{
    private RouteInspectApplicability(
        RouteInspectApplicabilityState state,
        IEnumerable<RouteInspectApplicabilityCondition> conditions,
        IEnumerable<string> matchingPaths)
    {
        if (!Enum.IsDefined(state))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The source applicability state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(matchingPaths);
        var materializedConditions = conditions.ToArray();
        var materializedPaths = matchingPaths.ToArray();
        if (materializedConditions.Any(condition => condition is null)
            || materializedPaths.Any(string.IsNullOrWhiteSpace)
            || materializedPaths.Distinct(StringComparer.Ordinal).Count() != materializedPaths.Length)
        {
            throw new ArgumentException("Route-inspect applicability facts must be valid and unique.");
        }

        State = state;
        Conditions = new ReadOnlyCollection<RouteInspectApplicabilityCondition>(materializedConditions);
        MatchingPaths = new ReadOnlyCollection<string>(materializedPaths);
    }

    internal RouteInspectApplicabilityState State { get; }

    internal IReadOnlyList<RouteInspectApplicabilityCondition> Conditions { get; }

    internal IReadOnlyList<string> MatchingPaths { get; }

    internal static RouteInspectApplicability From(SourceApplicabilityResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        var conditions = result.Conditions
            .Where(condition => condition.Metadata.State != ApplyToMetadataState.Absent)
            .Select(condition => new RouteInspectApplicabilityCondition(
                condition.CanonicalSourcePath,
                condition.Metadata.Patterns.Select(pattern => pattern.Text)));
        var state = result.State switch
        {
            SourceApplicabilityState.Unconditioned => RouteInspectApplicabilityState.Unconditioned,
            SourceApplicabilityState.Matched => RouteInspectApplicabilityState.Matched,
            SourceApplicabilityState.Unmatched => RouteInspectApplicabilityState.Unmatched,
            SourceApplicabilityState.Pending => RouteInspectApplicabilityState.Pending,
            SourceApplicabilityState.Invalid => RouteInspectApplicabilityState.Invalid,
            _ => throw new ArgumentOutOfRangeException(nameof(result), result.State, "The source applicability state is not defined."),
        };
        return new RouteInspectApplicability(state, conditions, result.MatchingPaths);
    }
}

internal sealed class RouteInspectApplicabilityCondition
{
    internal RouteInspectApplicabilityCondition(string source, IEnumerable<string> patterns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        ArgumentNullException.ThrowIfNull(patterns);
        var materializedPatterns = patterns.ToArray();
        if (materializedPatterns.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Applicability patterns cannot be blank.", nameof(patterns));
        }

        Source = source;
        Patterns = new ReadOnlyCollection<string>(materializedPatterns);
    }

    internal string Source { get; }

    internal IReadOnlyList<string> Patterns { get; }
}

internal enum RouteInspectApplicabilityState
{
    Unconditioned,
    Matched,
    Unmatched,
    Pending,
    Invalid,
}
