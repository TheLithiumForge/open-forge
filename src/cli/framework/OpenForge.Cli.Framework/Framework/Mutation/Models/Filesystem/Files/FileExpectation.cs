using System.Security.Cryptography;

namespace OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;

internal enum FileExpectationKind
{
    Missing,
    File,
    Directory,
}

internal sealed record FileExpectation
{
    private FileExpectation(
        FileExpectationKind kind,
        string logicalPath,
        string? physicalPath,
        string? contentHash)
    {
        Kind = kind;
        LogicalPath = logicalPath;
        PhysicalPath = physicalPath;
        ContentHash = contentHash;
    }

    internal FileExpectationKind Kind { get; }

    internal string LogicalPath { get; }

    internal string? PhysicalPath { get; }

    internal string? ContentHash { get; }

    internal bool Exists => Kind != FileExpectationKind.Missing;

    internal static FileExpectation Missing(string logicalPath)
        => new(
            kind: FileExpectationKind.Missing,
            logicalPath: NormalizeAbsolutePath(logicalPath, nameof(logicalPath)),
            physicalPath: null,
            contentHash: null);

    internal static FileExpectation File(
        string logicalPath,
        string physicalPath,
        string contentHash)
        => new(
            kind: FileExpectationKind.File,
            logicalPath: NormalizeAbsolutePath(logicalPath, nameof(logicalPath)),
            physicalPath: NormalizeAbsolutePath(physicalPath, nameof(physicalPath)),
            contentHash: ValidateHash(contentHash));

    internal static FileExpectation Directory(
        string logicalPath,
        string physicalPath)
        => new(
            kind: FileExpectationKind.Directory,
            logicalPath: NormalizeAbsolutePath(logicalPath, nameof(logicalPath)),
            physicalPath: NormalizeAbsolutePath(physicalPath, nameof(physicalPath)),
            contentHash: null);

    internal static string Hash(ReadOnlySpan<byte> bytes)
        => Convert.ToHexStringLower(SHA256.HashData(bytes));

    internal static string NormalizeAbsolutePath(string path, string parameterName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(path, parameterName);
        if (!Path.IsPathFullyQualified(path))
        {
            throw new ArgumentException("A mutation path must be fully qualified.", parameterName);
        }

        return Path.GetFullPath(path);
    }

    private static string ValidateHash(string contentHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(contentHash);
        if (contentHash.Length != SHA256.HashSizeInBytes * 2
            || contentHash.Any(character => character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A file expectation hash must be a lowercase SHA-256 value.",
                nameof(contentHash));
        }

        return contentHash;
    }
}
