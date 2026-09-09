using OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Extensions.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions.Embedded;

internal static class EmbeddedExtensionCatalogueReader
{
    private const string InventoryAsset = "inventory.json";

    internal static ExtensionSourceReadResult Read()
    {
        try
        {
            var inventoryBytes = EmbeddedExtensionCatalogueAssets.Read(InventoryAsset);
            JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(inventoryBytes.Span);
            var inventory = JsonSerializer.Deserialize(
                inventoryBytes.Span,
                ExtensionPackageJsonContext.Default.EmbeddedExtensionInventoryDocument)
                ?? throw new JsonException("The embedded Extension inventory cannot be null.");
            if (inventory.SchemaVersion != 1 || inventory.Packages is null)
            {
                throw new JsonException("The embedded Extension inventory schema is invalid.");
            }

            var packages = new List<ExtensionPackageFact>();
            var expectedAssets = new HashSet<string>(StringComparer.Ordinal) { InventoryAsset };
            string? previousPackage = null;
            foreach (var package in inventory.Packages)
            {
                if (package is null
                    || !ExtensionIdentity.IsValidStableId(package.Id)
                    || package.Assets is null
                    || package.Assets.Length == 0
                    || previousPackage is not null && string.CompareOrdinal(previousPackage, package.Id) >= 0)
                {
                    throw new JsonException("The embedded Extension package inventory is invalid or not ordered.");
                }

                var manifest = ValidateAssets(package, expectedAssets);
                packages.Add(manifest);
                previousPackage = package.Id;
            }

            if (!expectedAssets.SetEquals(EmbeddedExtensionCatalogueAssets.All.Keys))
            {
                throw new InvalidDataException("The embedded Extension asset inventory is incomplete or contains undeclared assets.");
            }

            ValidateClosure(packages);
            return new(
                state: ExtensionSourceReadState.Complete,
                kind: ExtensionSourceKind.EmbeddedCatalogue,
                identity: "embedded catalogue",
                packages: packages,
                cause: null);
        }
        catch (Exception exception) when (exception is JsonException or InvalidDataException or DecoderFallbackException)
        {
            return new(
                state: ExtensionSourceReadState.Invalid,
                kind: ExtensionSourceKind.EmbeddedCatalogue,
                identity: "embedded catalogue",
                packages: [],
                cause: $"The embedded Extension catalogue is invalid: {exception.Message}");
        }
    }

    private static ExtensionPackageFact ValidateAssets(
        EmbeddedExtensionInventoryPackage package,
        HashSet<string> expectedAssets)
    {
        string? previousPath = null;
        byte[]? manifestBytes = null;
        var payload = new List<ExtensionPackageFileFact>();
        foreach (var asset in package.Assets)
        {
            if (asset is null
                || !PortableWorkspacePath.TryNormalize(asset.Path, out var path)
                || !IsLowerSha256(asset.Sha256)
                || previousPath is not null && string.CompareOrdinal(previousPath, path) >= 0)
            {
                throw new JsonException("An embedded Extension asset identity is invalid or not ordered.");
            }

            var assetName = $"{package.Id}/{path}";
            if (!expectedAssets.Add(assetName))
            {
                throw new JsonException("The embedded Extension asset inventory contains a duplicate identity.");
            }

            var bytes = EmbeddedExtensionCatalogueAssets.Read(assetName);
            var digest = Convert.ToHexStringLower(SHA256.HashData(bytes.Span));
            if (!string.Equals(digest, asset.Sha256, StringComparison.Ordinal))
            {
                throw new InvalidDataException($"Embedded Extension asset hash mismatch for '{path}'.");
            }

            if (path == ExtensionPackageLayout.ManifestFileName)
            {
                manifestBytes = bytes.ToArray();
            }
            else if (path.StartsWith(ExtensionPackageLayout.ContentPathPrefix, StringComparison.Ordinal))
            {
                var target = path[ExtensionPackageLayout.ContentPathPrefix.Length..];
                if (!PortableWorkspacePath.TryNormalize(target, out var normalizedTarget))
                {
                    throw new JsonException("An embedded Extension payload target path is invalid.");
                }

                payload.Add(ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                {
                    Path = path,
                    TargetPath = normalizedTarget,
                    State = ExtensionPackageFileReadState.Available,
                    ByteLength = bytes.Length,
                    Sha256 = asset.Sha256,
                    Bytes = bytes,
                }));
            }

            previousPath = path;
        }

        if (manifestBytes is null)
        {
            throw new InvalidDataException($"Embedded Extension package '{package.Id}' has no manifest.");
        }

        var manifest = ExtensionManifestReader.Read(manifestBytes, ExtensionPackageLayout.ManifestFileName, payload);
        if (!string.Equals(manifest.Id, package.Id, StringComparison.Ordinal))
        {
            throw new InvalidDataException($"Embedded Extension package '{package.Id}' has a conflicting manifest ID.");
        }

        return manifest;
    }

    private static bool IsLowerSha256(string? value)
        => value is { Length: SHA256.HashSizeInBytes * 2 }
            && value.All(character => character is >= '0' and <= '9' or >= 'a' and <= 'f');

    private static void ValidateClosure(List<ExtensionPackageFact> packages)
    {
        var ids = packages.Select(package => package.Id).ToHashSet(StringComparer.Ordinal);
        if (ids.Count != packages.Count
            || packages.Any(package => package.Dependencies.Any(dependency => !ids.Contains(dependency))))
        {
            throw new InvalidDataException("The embedded Extension dependency closure is incomplete or ambiguous.");
        }

        ExtensionDependencyValidator.ValidateAcyclic(packages);
    }
}
