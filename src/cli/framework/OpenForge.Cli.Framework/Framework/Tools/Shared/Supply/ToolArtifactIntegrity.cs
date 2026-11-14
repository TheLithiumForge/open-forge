using System.Security.Cryptography;
using System.Diagnostics.CodeAnalysis;
using OpenForge.Cli.Core.Framework.Tools.Models.Channels;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Supply;

internal static class ToolArtifactIntegrity
{
    internal static bool IsSha256([NotNullWhen(true)] string? value)
        => value is { Length: SHA256.HashSizeInBytes * 2 } && value.All(char.IsAsciiHexDigit);

    internal static async ValueTask VerifyAsync(string path, ToolReleaseAsset asset, CancellationToken token)
    {
        if (!IsSha256(asset.Sha256)) throw new InvalidDataException("The artifact needs a pinned SHA-256.");
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        if (stream.Length <= 0 || stream.Length > asset.MaximumBytes) throw new InvalidDataException("The artifact exceeds its declared size limit.");
        var actual = await SHA256.HashDataAsync(stream, token).ConfigureAwait(false);
        if (!CryptographicOperations.FixedTimeEquals(actual, Convert.FromHexString(asset.Sha256)))
            throw new InvalidDataException("The artifact SHA-256 differs from its pin.");
    }

    internal static async ValueTask<string> FileHashAsync(string path, CancellationToken token)
    {
        await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
        return Convert.ToHexStringLower(await SHA256.HashDataAsync(stream, token).ConfigureAwait(false));
    }
}
