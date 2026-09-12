using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

internal enum PlannedFileChangeKind
{
    Create,
    Replace,
    Delete,
    ReplaceGeneratedRegion,
}

internal sealed record PlannedFileChange
{
    private PlannedFileChange(
        PlannedFileChangeKind kind,
        FileExpectation expectation,
        ReadOnlySpan<byte> intendedBytes)
    {
        Kind = kind;
        Expectation = expectation;
        IntendedBytes = ImmutableArray.Create(intendedBytes);
    }

    internal PlannedFileChangeKind Kind { get; }

    internal FileExpectation Expectation { get; }

    internal string LogicalPath => Expectation.LogicalPath;

    internal ImmutableArray<byte> IntendedBytes { get; }

    internal bool HasIntendedBytes => Kind != PlannedFileChangeKind.Delete;

    internal static PlannedFileChange Create(
        FileExpectation expectation,
        ReadOnlySpan<byte> intendedBytes)
    {
        ValidateExpectation(expectation, FileExpectationKind.Missing, nameof(expectation));
        return new PlannedFileChange(PlannedFileChangeKind.Create, expectation, intendedBytes);
    }

    internal static PlannedFileChange Replace(
        FileExpectation expectation,
        ReadOnlySpan<byte> intendedBytes)
    {
        ValidateExpectation(expectation, FileExpectationKind.File, nameof(expectation));
        ValidateChangedBytes(expectation, intendedBytes);
        return new PlannedFileChange(PlannedFileChangeKind.Replace, expectation, intendedBytes);
    }

    internal static PlannedFileChange Delete(FileExpectation expectation)
    {
        ValidateExpectation(expectation, FileExpectationKind.File, nameof(expectation));
        return new PlannedFileChange(PlannedFileChangeKind.Delete, expectation, []);
    }

    internal static PlannedFileChange ReplaceGeneratedRegion(
        FileExpectation expectation,
        ReadOnlySpan<byte> intendedDocumentBytes)
    {
        ValidateExpectation(expectation, FileExpectationKind.File, nameof(expectation));
        ValidateChangedBytes(expectation, intendedDocumentBytes);
        return new PlannedFileChange(
            PlannedFileChangeKind.ReplaceGeneratedRegion,
            expectation,
            intendedDocumentBytes);
    }

    private static void ValidateChangedBytes(
        FileExpectation expectation,
        ReadOnlySpan<byte> intendedBytes)
    {
        if (string.Equals(
            expectation.ContentHash,
            FileExpectation.Hash(intendedBytes),
            StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "A planned replacement must change the expected exact bytes.",
                nameof(intendedBytes));
        }
    }

    private static void ValidateExpectation(
        FileExpectation expectation,
        FileExpectationKind expectedKind,
        string parameterName)
    {
        ArgumentNullException.ThrowIfNull(expectation, parameterName);
        if (expectation.Kind != expectedKind)
        {
            throw new ArgumentException(
                $"A {expectedKind} expectation is required for this planned file change.",
                parameterName);
        }
    }
}
