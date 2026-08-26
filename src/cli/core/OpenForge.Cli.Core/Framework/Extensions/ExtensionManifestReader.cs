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

    internal static ExtensionPackageFact Read(ReadOnlySpan<byte> bytes, int payloadFileCount)
    {
        _ = StrictUtf8.GetString(bytes);
        ExtensionJsonSyntaxValidator.ValidateNoDuplicateProperties(bytes);
        var manifest = JsonSerializer.Deserialize(
            bytes,
            ExtensionPackageJsonContext.Default.ExtensionManifestDocument)
            ?? throw new JsonException("An Extension manifest cannot be null.");
        Validate(manifest);
        return new ExtensionPackageFact(
            id: manifest.Id,
            name: manifest.Name,
            description: manifest.Description,
            version: manifest.Version,
            dependencies: manifest.Dependencies,
            payloadFileCount: payloadFileCount);
    }

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
        if (!ordered.SequenceEqual(manifest.Dependencies, StringComparer.Ordinal)
            || ordered.Distinct(StringComparer.Ordinal).Count() != ordered.Length
            || manifest.Dependencies.Contains(manifest.Id, StringComparer.Ordinal))
        {
            throw new JsonException("Extension dependencies must be distinct, ordered stable IDs and cannot include the package itself.");
        }
    }
}
