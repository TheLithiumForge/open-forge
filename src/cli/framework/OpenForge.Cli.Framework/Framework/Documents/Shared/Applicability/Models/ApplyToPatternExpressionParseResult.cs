using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

internal sealed record ApplyToPatternExpressionParseResult
{
    private ApplyToPatternExpressionParseResult(ImmutableArray<ApplyToPattern> patterns, ApplyToPatternFailure? failure)
    {
        if (patterns.IsDefaultOrEmpty == (failure is null))
        {
            throw new ArgumentException("An expression parse result must contain either patterns or a failure.");
        }

        if (failure is { } failureValue && !Enum.IsDefined(failureValue))
        {
            throw new ArgumentOutOfRangeException(nameof(failure), failureValue, "The pattern failure is not defined.");
        }

        Patterns = patterns;
        Failure = failure;
    }

    internal ImmutableArray<ApplyToPattern> Patterns { get; }

    internal ApplyToPatternFailure? Failure { get; }

    internal static ApplyToPatternExpressionParseResult Succeeded(ImmutableArray<ApplyToPattern> patterns)
        => new(patterns, failure: null);

    internal static ApplyToPatternExpressionParseResult Failed(ApplyToPatternFailure failure)
        => new([], failure);
}
