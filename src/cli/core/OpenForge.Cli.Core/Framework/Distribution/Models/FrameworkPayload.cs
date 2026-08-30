using System.Collections.Immutable;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace OpenForge.Cli.Core.Framework.Distribution.Models;

internal sealed class FrameworkPayload
{
    private FrameworkPayload(
        ImmutableArray<FrameworkPayloadAsset> assets,
        string inventoryFingerprint)
    {
        Assets = assets;
        InventoryFingerprint = inventoryFingerprint;
    }

    internal ImmutableArray<FrameworkPayloadAsset> Assets { get; }

    internal string InventoryFingerprint { get; }

    internal static FrameworkPayload Create(IEnumerable<FrameworkPayloadAsset> assets)
    {
        ArgumentNullException.ThrowIfNull(assets);
        var ordered = assets
            .OrderBy(asset => asset.Path, StringComparer.Ordinal)
            .ToImmutableArray();
        ValidateInventory(ordered);
        return new FrameworkPayload(ordered, CalculateInventoryFingerprint(ordered));
    }

    internal FrameworkPayloadAsset? Find(string path)
    {
        foreach (var asset in Assets)
        {
            if (string.Equals(asset.Path, path, StringComparison.Ordinal))
            {
                return asset;
            }
        }

        return null;
    }

    private static void ValidateInventory(ImmutableArray<FrameworkPayloadAsset> assets)
    {
        string? previousPath = null;
        var hasRootAgent = false;
        var hasRootClaude = false;
        var hasLoader = false;
        foreach (var asset in assets)
        {
            if (previousPath is not null && string.Equals(previousPath, asset.Path, StringComparison.Ordinal))
            {
                throw new ArgumentException("A Framework payload cannot contain duplicate canonical paths.", nameof(assets));
            }

            hasRootAgent |= asset.Path == FrameworkPayloadAsset.RootAgentPath;
            hasRootClaude |= asset.Path == FrameworkPayloadAsset.RootClaudePath;
            hasLoader |= asset.Path == FrameworkPayloadAsset.LoaderPath;
            previousPath = asset.Path;
        }

        if (!hasRootAgent || !hasRootClaude || !hasLoader)
        {
            throw new ArgumentException("A Framework payload must contain every required anchor.", nameof(assets));
        }
    }

    private static string CalculateInventoryFingerprint(ImmutableArray<FrameworkPayloadAsset> assets)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        foreach (var asset in assets)
        {
            var line = string.Create(
                CultureInfo.InvariantCulture,
                $"{asset.Path}\t{asset.ByteLength}\t{asset.Sha256}\n");
            hash.AppendData(Encoding.UTF8.GetBytes(line));
        }

        return Convert.ToHexStringLower(hash.GetHashAndReset());
    }
}
