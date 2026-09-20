using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Shared.Identity;

namespace OpenForge.Cli.Core.Framework.Recovery.Models.Identity;

internal sealed record RecoveryContentIdentity
{
    private RecoveryContentIdentity(long length, string sha256)
    {
        Length = length;
        Sha256 = sha256;
    }

    internal long Length { get; }

    internal string Sha256 { get; }

    internal static RecoveryContentIdentity Create(long length, string sha256)
    {
        if (length < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(length));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(sha256);
        if (sha256.Length != RecoveryBundleFormatV1.Sha256HexLength
            || sha256.Any(character => character is not (>= '0' and <= '9' or >= 'a' and <= 'f')))
        {
            throw new ArgumentException(
                "A recovery fingerprint must be a lowercase SHA-256 value.",
                nameof(sha256));
        }

        return new RecoveryContentIdentity(length, sha256);
    }

    internal static RecoveryContentIdentity FromBytes(ReadOnlySpan<byte> bytes)
        => Create(bytes.Length, FileExpectation.Hash(bytes));

    internal bool Matches(ReadOnlySpan<byte> bytes)
        => Length == bytes.Length
            && string.Equals(Sha256, FileExpectation.Hash(bytes), StringComparison.Ordinal);

    internal bool Matches(string? sha256, long length)
        => Length == length && string.Equals(Sha256, sha256, StringComparison.Ordinal);
}
