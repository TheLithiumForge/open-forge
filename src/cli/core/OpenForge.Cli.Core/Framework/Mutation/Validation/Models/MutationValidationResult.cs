using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

internal enum MutationValidationState
{
    Valid,
    Mismatched,
    Blocked,
    Failed,
    Cancelled,
}

internal sealed record MutationValidationResult
{
    private const int MaximumCauseLength = 256;

    private MutationValidationResult(
        MutationValidationState state,
        IReadOnlyList<FileExpectationValidationResult> checks,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Checks = checks;
        Failure = failure;
        Cause = cause;
    }

    internal MutationValidationState State { get; }

    internal IReadOnlyList<FileExpectationValidationResult> Checks { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static MutationValidationResult Valid()
        => new(
            state: MutationValidationState.Valid,
            checks: Array.Empty<FileExpectationValidationResult>(),
            failure: null,
            cause: null);

    internal static MutationValidationResult FromChecks(
        IEnumerable<FileExpectationValidationResult> checks)
    {
        ArgumentNullException.ThrowIfNull(checks);
        var values = checks
            .Select(value => value
                ?? throw new ArgumentException(
                    "Mutation validation checks cannot contain null members.",
                    nameof(checks)))
            .ToArray();
        if (values.Length == 0)
        {
            throw new ArgumentException(
                "A checked mutation validation result cannot be empty.",
                nameof(checks));
        }

        var decisive = values.FirstOrDefault(value => value.State != FileExpectationValidationState.Matched);
        var state = decisive?.State switch
        {
            null => MutationValidationState.Valid,
            FileExpectationValidationState.Mismatched => MutationValidationState.Mismatched,
            FileExpectationValidationState.Blocked => MutationValidationState.Blocked,
            FileExpectationValidationState.Failed => MutationValidationState.Failed,
            FileExpectationValidationState.Cancelled => MutationValidationState.Cancelled,
            _ => throw new ArgumentOutOfRangeException(
                nameof(checks),
                decisive?.State,
                "The file validation state is not defined."),
        };
        return new MutationValidationResult(
            state: state,
            checks: new ReadOnlyCollection<FileExpectationValidationResult>(values),
            failure: decisive?.Failure,
            cause: decisive?.Cause);
    }

    internal static MutationValidationResult Blocked(string cause)
        => Blocked(cause, []);

    internal static MutationValidationResult Blocked(
        string cause,
        IEnumerable<FileExpectationValidationResult> checks)
    {
        ArgumentNullException.ThrowIfNull(checks);
        var values = checks
            .Select(value => value
                ?? throw new ArgumentException(
                    "Mutation validation checks cannot contain null members.",
                    nameof(checks)))
            .ToArray();
        return new MutationValidationResult(
            state: MutationValidationState.Blocked,
            checks: new ReadOnlyCollection<FileExpectationValidationResult>(values),
            failure: null,
            cause: ValidateCause(cause));
    }

    internal static MutationValidationResult Cancelled()
        => new(
            state: MutationValidationState.Cancelled,
            checks: Array.Empty<FileExpectationValidationResult>(),
            failure: null,
            cause: null);

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }
}
