using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Extensions.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions;

internal static class ExtensionManifestReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    internal static ExtensionPackageFact ReadManifestOnly(ReadOnlySpan<byte> bytes)
    {
        _ = StrictUtf8.GetString(bytes);
        ExtensionJsonSyntaxValidator.ValidateNoDuplicateProperties(bytes);
        var manifest = JsonSerializer.Deserialize(
            bytes,
            ExtensionPackageJsonContext.Default.ExtensionManifestDocument)
            ?? throw new JsonException("An Extension manifest cannot be null.");
        Validate(manifest);
        return CreatePackage(manifest, "extension.json", []);
    }

    internal static ExtensionPackageFact Read(
        ReadOnlySpan<byte> bytes,
        string manifestPath,
        IEnumerable<ExtensionPackageFileFact> payload)
    {
        _ = StrictUtf8.GetString(bytes);
        ExtensionJsonSyntaxValidator.ValidateNoDuplicateProperties(bytes);
        var manifest = JsonSerializer.Deserialize(
            bytes,
            ExtensionPackageJsonContext.Default.ExtensionManifestDocument)
            ?? throw new JsonException("An Extension manifest cannot be null.");
        Validate(manifest);
        return CreatePackage(manifest, manifestPath, payload);
    }

    private static ExtensionPackageFact CreatePackage(
        ExtensionManifestDocument manifest,
        string manifestPath,
        IEnumerable<ExtensionPackageFileFact> payload)
        => ExtensionPackageFact.Create(
            new ExtensionPackageManifestFact
            {
                Id = manifest.Id,
                Name = manifest.Name,
                Description = manifest.Description,
                Version = manifest.Version,
                Dependencies = manifest.Dependencies,
            },
            new ExtensionPackageContentsFact
            {
                ManifestPath = manifestPath,
                Payload = payload.ToArray(),
            });

    private static void Validate(ExtensionManifestDocument manifest)
    {
        if (!ExtensionIdentity.IsValidStableId(manifest.Id)
            || string.IsNullOrWhiteSpace(manifest.Name)
            || string.IsNullOrWhiteSpace(manifest.Description)
            || string.IsNullOrWhiteSpace(manifest.Version)
            || manifest.Dependencies is null
            || manifest.Dependencies.Any(dependency => !ExtensionIdentity.IsValidStableId(dependency)))
        {
            throw new JsonException("The Extension manifest has an invalid identity or required value.");
        }

        var ordered = manifest.Dependencies.Order(StringComparer.Ordinal).ToArray();
        if (ordered.Distinct(StringComparer.Ordinal).Count() != ordered.Length)
        {
            throw new ExtensionManifestDependencyConflictException(
                "Extension dependencies cannot contain duplicate stable IDs.");
        }

        if (!ordered.SequenceEqual(manifest.Dependencies, StringComparer.Ordinal)
            || manifest.Dependencies.Contains(manifest.Id, StringComparer.Ordinal))
        {
            throw new JsonException("Extension dependencies must be distinct, ordered stable IDs and cannot include the package itself.");
        }
    }
}

internal sealed class ExtensionManifestDependencyConflictException(string message) : JsonException(message);
