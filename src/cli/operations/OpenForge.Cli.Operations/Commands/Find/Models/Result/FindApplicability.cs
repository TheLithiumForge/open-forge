using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Sources.Identity;

namespace OpenForge.Cli.Core.Commands.Find.Models.Result;

internal sealed record FindApplicabilityCondition
{
    internal FindApplicabilityCondition(string source, IEnumerable<string> patterns)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(source);
        if (!SourceLogicalPath.IsCanonicalSource(source))
        {
            throw new ArgumentException("Find applicability condition sources must be canonical .agents paths.", nameof(source));
        }

        ArgumentNullException.ThrowIfNull(patterns);
        var materialized = patterns.ToArray();
        if (materialized.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Find applicability patterns cannot be blank.", nameof(patterns));
        }

        Source = source;
        Patterns = Array.AsReadOnly(materialized);
    }

    internal string Source { get; }

    internal IReadOnlyList<string> Patterns { get; }
}

internal sealed record FindMatchApplicability
{
    internal FindMatchApplicability(
        string state,
        IEnumerable<FindApplicabilityCondition> conditions,
        IEnumerable<string> matchingPaths)
    {
        if (state is not ("unconditioned" or "matched" or "unmatched" or "pending" or "invalid"))
        {
            throw new ArgumentOutOfRangeException(nameof(state), state, "The Find applicability state is not defined.");
        }

        ArgumentNullException.ThrowIfNull(conditions);
        ArgumentNullException.ThrowIfNull(matchingPaths);
        var materializedConditions = conditions.ToArray();
        var materializedPaths = matchingPaths.ToArray();
        if (materializedConditions.Any(condition => condition is null)
            || materializedPaths.Any(string.IsNullOrWhiteSpace))
        {
            throw new ArgumentException("Find applicability collections cannot contain null or blank values.");
        }

        State = state;
        Conditions = Array.AsReadOnly(materializedConditions);
        MatchingPaths = Array.AsReadOnly(materializedPaths);
    }

    internal string State { get; }

    internal IReadOnlyList<FindApplicabilityCondition> Conditions { get; }

    internal IReadOnlyList<string> MatchingPaths { get; }
}

internal sealed record FindApplicabilityEvaluation
{
    internal FindApplicabilityEvaluation(
        IEnumerable<FindMatch> matches,
        IEnumerable<FindFinding> findings)
    {
        ArgumentNullException.ThrowIfNull(matches);
        ArgumentNullException.ThrowIfNull(findings);
        var materializedMatches = matches.ToArray();
        var materializedFindings = findings.ToArray();
        if (materializedMatches.Any(match => match is null)
            || materializedFindings.Any(finding => finding is null))
        {
            throw new ArgumentException("Find applicability results cannot contain null values.");
        }

        Matches = Array.AsReadOnly(materializedMatches);
        Findings = Array.AsReadOnly(materializedFindings);
    }

    internal IReadOnlyList<FindMatch> Matches { get; }

    internal IReadOnlyList<FindFinding> Findings { get; }
}
