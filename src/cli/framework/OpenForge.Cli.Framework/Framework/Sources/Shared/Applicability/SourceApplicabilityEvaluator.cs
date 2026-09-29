using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Shared.Applicability.Models;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Framework.Sources.Shared.Applicability.Models;

namespace OpenForge.Cli.Core.Framework.Sources.Shared.Applicability;

internal static class SourceApplicabilityEvaluator
{
    internal static SourceApplicabilityResult Evaluate(
        IReadOnlyList<SourceApplyToCondition> chain,
        IReadOnlyList<string> workspaceRelativePaths)
    {
        ArgumentNullException.ThrowIfNull(chain);
        ArgumentNullException.ThrowIfNull(workspaceRelativePaths);

        var conditions = chain.ToImmutableArray();
        var hasValidClause = false;
        foreach (var condition in conditions)
        {
            ArgumentNullException.ThrowIfNull(condition);
            ArgumentNullException.ThrowIfNull(condition.Metadata);
            switch (condition.Metadata.State)
            {
                case ApplyToMetadataState.Absent:
                    break;
                case ApplyToMetadataState.Valid:
                    hasValidClause = true;
                    break;
                case ApplyToMetadataState.Invalid:
                    return CreateResult(SourceApplicabilityState.Invalid, conditions, []);
                default:
                    throw new ArgumentOutOfRangeException(
                        nameof(chain),
                        condition.Metadata.State,
                        "The applyTo metadata state is not defined.");
            }
        }

        if (!hasValidClause)
        {
            return CreateResult(SourceApplicabilityState.Unconditioned, conditions, []);
        }

        if (workspaceRelativePaths.Count == 0)
        {
            return CreateResult(SourceApplicabilityState.Pending, conditions, []);
        }

        var matchingPaths = ImmutableArray.CreateBuilder<string>();
        var seenPaths = new HashSet<string>(StringComparer.Ordinal);
        foreach (var path in workspaceRelativePaths)
        {
            ArgumentNullException.ThrowIfNull(path);
            if (!seenPaths.Add(path) || !MatchesEveryCondition(conditions, path))
            {
                continue;
            }

            matchingPaths.Add(path);
        }

        var state = matchingPaths.Count > 0
            ? SourceApplicabilityState.Matched
            : SourceApplicabilityState.Unmatched;
        return CreateResult(state, conditions, matchingPaths.ToImmutable());
    }

    private static bool MatchesEveryCondition(
        ImmutableArray<SourceApplyToCondition> conditions,
        string workspaceRelativePath)
    {
        foreach (var condition in conditions)
        {
            if (condition.Metadata.State == ApplyToMetadataState.Absent)
            {
                continue;
            }

            if (!condition.Metadata.Patterns.Any(pattern =>
                    ApplyToPatternMatcher.IsMatch(pattern, workspaceRelativePath)))
            {
                return false;
            }
        }

        return true;
    }

    private static SourceApplicabilityResult CreateResult(
        SourceApplicabilityState state,
        ImmutableArray<SourceApplyToCondition> conditions,
        ImmutableArray<string> matchingPaths)
        => new(state, conditions, matchingPaths);
}
