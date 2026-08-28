using OpenForge.Cli.Core.Framework.Filesystem;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

namespace OpenForge.Cli.Core.Framework.Mutation.Validation.Models;

internal enum FileExpectationValidationState
{
    Matched,
    Mismatched,
    Blocked,
    Failed,
    Cancelled,
}

internal sealed record FileExpectationValidationResult
{
    private const int MaximumCauseLength = 256;

    private FileExpectationValidationResult(
        FileExpectationValidationState state,
        FileExpectation expectation,
        FileStateSnapshot? actual,
        string? physicalPath,
        FilesystemFailure? failure,
        string? cause)
    {
        State = state;
        Expectation = expectation;
        Actual = actual;
        PhysicalPath = physicalPath;
        Failure = failure;
        Cause = cause;
    }

    internal FileExpectationValidationState State { get; }

    internal FileExpectation Expectation { get; }

    internal FileStateSnapshot? Actual { get; }

    internal string? PhysicalPath { get; }

    internal FilesystemFailure? Failure { get; }

    internal string? Cause { get; }

    internal static FileExpectationValidationResult Matched(
        FileExpectation expectation,
        FileStateSnapshot actual,
        string physicalPath)
    {
        Validate(expectation, actual);
        if (expectation != actual.Expectation)
        {
            throw new ArgumentException(
                "A matched file validation requires the exact expected state.",
                nameof(actual));
        }

        return Create(
            FileExpectationValidationState.Matched,
            expectation,
            actual,
            physicalPath: ValidatePhysicalPath(actual, physicalPath),
            failure: null,
            cause: null);
    }

    internal static FileExpectationValidationResult Mismatched(
        FileExpectation expectation,
        FileStateSnapshot actual,
        string physicalPath,
        string cause)
    {
        Validate(expectation, actual);
        if (expectation == actual.Expectation)
        {
            throw new ArgumentException(
                "A mismatched file validation cannot carry the expected state as actual.",
                nameof(actual));
        }

        return Create(
            FileExpectationValidationState.Mismatched,
            expectation,
            actual,
            physicalPath: ValidatePhysicalPath(actual, physicalPath),
            failure: null,
            cause: ValidateCause(cause));
    }

    internal static FileExpectationValidationResult Blocked(
        FileExpectation expectation,
        string cause)
        => Create(
            FileExpectationValidationState.Blocked,
            expectation,
            actual: null,
            physicalPath: null,
            failure: null,
            cause: ValidateCause(cause));

    internal static FileExpectationValidationResult Failed(
        FileExpectation expectation,
        FilesystemFailure failure)
    {
        ArgumentNullException.ThrowIfNull(failure);
        return Create(
            FileExpectationValidationState.Failed,
            expectation,
            actual: null,
            physicalPath: null,
            failure: failure,
            cause: failure.DirectCause);
    }

    internal static FileExpectationValidationResult Cancelled(FileExpectation expectation)
        => Create(
            FileExpectationValidationState.Cancelled,
            expectation,
            actual: null,
            physicalPath: null,
            failure: null,
            cause: null);

    private static FileExpectationValidationResult Create(
        FileExpectationValidationState state,
        FileExpectation expectation,
        FileStateSnapshot? actual,
        string? physicalPath,
        FilesystemFailure? failure,
        string? cause)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        return new FileExpectationValidationResult(
            state: state,
            expectation: expectation,
            actual: actual,
            physicalPath: physicalPath,
            failure: failure,
            cause: cause);
    }

    private static void Validate(
        FileExpectation expectation,
        FileStateSnapshot actual)
    {
        ArgumentNullException.ThrowIfNull(expectation);
        ArgumentNullException.ThrowIfNull(actual);
        if (!string.Equals(expectation.LogicalPath, actual.LogicalPath, PathComparison()))
        {
            throw new ArgumentException(
                "File validation facts must describe one logical path.",
                nameof(actual));
        }
    }

    private static string ValidateCause(string cause)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(cause);
        return cause.Length <= MaximumCauseLength
            ? cause
            : cause[..MaximumCauseLength];
    }

    private static string ValidatePhysicalPath(
        FileStateSnapshot actual,
        string physicalPath)
    {
        var normalized = FileExpectation.NormalizeAbsolutePath(
            physicalPath,
            nameof(physicalPath));
        if (actual.PhysicalPath is { } actualPhysicalPath
            && !string.Equals(actualPhysicalPath, normalized, PathComparison()))
        {
            throw new ArgumentException(
                "File validation physical path must match the observed present state.",
                nameof(physicalPath));
        }

        return normalized;
    }

    private static StringComparison PathComparison()
        => OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
}
