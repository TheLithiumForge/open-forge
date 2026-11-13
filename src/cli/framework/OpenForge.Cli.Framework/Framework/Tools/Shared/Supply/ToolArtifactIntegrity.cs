using System.Security.Cryptography;
using OpenForge.Cli.Core.Framework.Tools.Shared.Validation;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArtifactIntegrity
{
    internal const string Sha256Prefix = "sha256:";
    internal const string Sha512Prefix = "sha512-";
    private static readonly int Sha512IntegrityLength = Sha512Prefix.Length + (SHA512.HashSizeInBytes + 2) / 3 * 4;

    internal static bool IsValid(string value)
    {
        if (value.StartsWith(Sha256Prefix, StringComparison.Ordinal)) return ToolIdentity.IsSha256(value[Sha256Prefix.Length..]);
        if (!value.StartsWith(Sha512Prefix, StringComparison.Ordinal) || value.Length != Sha512IntegrityLength || value.Any(char.IsWhiteSpace)) return false;
        Span<byte> decoded = stackalloc byte[SHA512.HashSizeInBytes];
        return Convert.TryFromBase64String(value[Sha512Prefix.Length..], decoded, out var count) && count == decoded.Length;
    }

    internal static string Key(string integrity)
    {
        if (!IsValid(integrity)) throw new ArgumentException("The artifact integrity is invalid.", nameof(integrity));
        return integrity.StartsWith(Sha256Prefix, StringComparison.Ordinal) ? integrity[Sha256Prefix.Length..]
            : Convert.ToHexStringLower(Convert.FromBase64String(integrity[Sha512Prefix.Length..]));
    }

    internal static async ValueTask VerifyAsync(string path, long bytes, string integrity, CancellationToken token)
    {
        if (!IsValid(integrity)) throw new ArgumentException("The artifact integrity is invalid.", nameof(integrity));
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length != bytes) throw new InvalidDataException("The artifact length differs from the reviewed descriptor.");
        var sha256 = integrity.StartsWith(Sha256Prefix, StringComparison.Ordinal);
        var expected = sha256 ? Convert.FromHexString(integrity[Sha256Prefix.Length..]) : Convert.FromBase64String(integrity[Sha512Prefix.Length..]);
        var actual = sha256 ? await SHA256.HashDataAsync(stream, token).ConfigureAwait(false) : await SHA512.HashDataAsync(stream, token).ConfigureAwait(false);
        if (!CryptographicOperations.FixedTimeEquals(actual, expected)) throw new InvalidDataException("The artifact integrity differs from the reviewed descriptor.");
    }

    internal static async ValueTask<string> FileHashAsync(string path, CancellationToken token)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, token).ConfigureAwait(false));
    }
}
