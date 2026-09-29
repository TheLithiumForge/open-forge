namespace OpenForge.Cli.Core.Framework.Documents.Shared.Applicability.Models;

internal sealed record ApplyToPatternParseResult
{
    private ApplyToPatternParseResult(ApplyToPattern? pattern, ApplyToPatternFailure? failure)
    {
        if ((pattern is null) == (failure is null))
        {
            throw new ArgumentException("A pattern parse result must contain either a pattern or a failure.");
        }

        if (failure is { } failureValue && !Enum.IsDefined(failureValue))
        {
            throw new ArgumentOutOfRangeException(nameof(failure), failureValue, "The pattern failure is not defined.");
        }

        Pattern = pattern;
        Failure = failure;
    }

    internal ApplyToPattern? Pattern { get; }

    internal ApplyToPatternFailure? Failure { get; }

    internal static ApplyToPatternParseResult Succeeded(ApplyToPattern pattern)
    {
        ArgumentNullException.ThrowIfNull(pattern);
        return new ApplyToPatternParseResult(pattern, failure: null);
    }

    internal static ApplyToPatternParseResult Failed(ApplyToPatternFailure failure)
        => new(pattern: null, failure);
}
