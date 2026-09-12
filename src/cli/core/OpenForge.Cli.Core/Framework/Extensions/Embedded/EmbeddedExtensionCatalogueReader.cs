using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Extensions.Embedded;

internal static class EmbeddedExtensionCatalogueReader
{
    internal static ExtensionSourceReadResult Read()
    {
        try
        {
            var assets = EmbeddedExtensionCatalogueAssets.All;
            var packages = assets.GroupBy(asset => asset.Key[..asset.Key.IndexOf('/')], StringComparer.Ordinal)
                .Select(ReadPackage)
                .OrderBy(package => package.Id, StringComparer.Ordinal)
                .ToList();
            ValidateClosure(packages);
            return new(
                state: ExtensionSourceReadState.Complete,
                kind: ExtensionSourceKind.EmbeddedCatalogue,
                identity: "embedded catalogue",
                packages: packages,
                cause: null);
        }
        catch (Exception exception) when (exception is JsonException or IOException or DecoderFallbackException)
        {
            return new(
                state: ExtensionSourceReadState.Invalid,
                kind: ExtensionSourceKind.EmbeddedCatalogue,
                identity: "embedded catalogue",
                packages: [],
                cause: $"The embedded Extension catalogue is invalid: {exception.Message}");
        }
    }

    private static ExtensionPackageFact ReadPackage(
        IGrouping<string, KeyValuePair<string, ReadOnlyMemory<byte>>> assets)
    {
        ReadOnlyMemory<byte>? manifest = null;
        var payload = new List<ExtensionPackageFileFact>();
        foreach (var asset in assets.OrderBy(asset => asset.Key, StringComparer.Ordinal))
        {
            var path = asset.Key[(assets.Key.Length + 1)..];
            if (path == ExtensionPackageLayout.ManifestFileName)
            {
                manifest = asset.Value;
            }
            else if (path.StartsWith(ExtensionPackageLayout.ContentPathPrefix, StringComparison.Ordinal))
            {
                var target = path[ExtensionPackageLayout.ContentPathPrefix.Length..];
                if (!PortableWorkspacePath.TryNormalize(target, out var normalizedTarget))
                {
                    throw new InvalidDataException($"Embedded Extension payload '{asset.Key}' has an invalid target path.");
                }

                payload.Add(ExtensionPackageFileFact.Create(new ExtensionPackageFileSnapshot
                {
                    Path = path,
                    TargetPath = normalizedTarget,
                    State = ExtensionPackageFileReadState.Available,
                    ByteLength = asset.Value.Length,
                    Sha256 = Convert.ToHexStringLower(SHA256.HashData(asset.Value.Span)),
                    Bytes = asset.Value,
                }));
            }
        }

        if (manifest is null)
        {
            throw new InvalidDataException($"Embedded Extension directory '{assets.Key}' has no manifest.");
        }

        return ExtensionManifestReader.Read(manifest.Value.Span, ExtensionPackageLayout.ManifestFileName, payload);
    }

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
