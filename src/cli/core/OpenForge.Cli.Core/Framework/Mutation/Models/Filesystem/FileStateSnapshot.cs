using System.Collections.Immutable;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;

internal sealed record FileStateSnapshot
{
    private FileStateSnapshot(
        FileExpectation expectation,
        ImmutableArray<byte> bytes,
        bool hasBytes)
    {
        Expectation = expectation;
        Bytes = bytes;
        HasBytes = hasBytes;
    }

    internal FileExpectation Expectation { get; }

    internal FileExpectationKind Kind => Expectation.Kind;

    internal string LogicalPath => Expectation.LogicalPath;

    internal string? PhysicalPath => Expectation.PhysicalPath;

    internal string? ContentHash => Expectation.ContentHash;

    internal ImmutableArray<byte> Bytes { get; }

    internal bool HasBytes { get; }

    internal static FileStateSnapshot Missing(string logicalPath)
        => new(FileExpectation.Missing(logicalPath), [], hasBytes: false);

    internal static FileStateSnapshot Directory(
        string logicalPath,
        string physicalPath)
        => new(FileExpectation.Directory(logicalPath, physicalPath), [], hasBytes: false);

    internal static FileStateSnapshot File(
        string logicalPath,
        string physicalPath,
        ReadOnlySpan<byte> bytes)
    {
        var values = ImmutableArray.CreateRange(bytes.ToArray());
        return new FileStateSnapshot(
            FileExpectation.File(logicalPath, physicalPath, FileExpectation.Hash(values.AsSpan())),
            values,
            hasBytes: true);
    }
}
