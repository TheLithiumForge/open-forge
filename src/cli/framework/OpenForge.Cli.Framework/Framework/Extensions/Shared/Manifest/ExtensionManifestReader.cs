using System.Text;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Extensions.Identity;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Serialization;
using OpenForge.Cli.Core.Framework.Extensions.Serialization;

namespace OpenForge.Cli.Core.Framework.Extensions.Shared.Manifest;

internal static class ExtensionManifestReader
{
    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    private static readonly string[] AcceptedKeys =
        ["id", "name", "description", "version", "dependencies"];

    internal static ExtensionPackageFact Read(
        ReadOnlySpan<byte> bytes,
        string manifestPath,
        IEnumerable<ExtensionPackageFileFact> payload)
    {
        _ = StrictUtf8.GetCharCount(bytes);
        JsonDuplicatePropertyValidator.ValidateNoDuplicateProperties(bytes);
        ValidateAcceptedKeys(bytes);
        var manifest = JsonSerializer.Deserialize(
            bytes,
            ExtensionPackageJsonContext.Default.ExtensionManifestDocument)
            ?? throw new JsonException("An Extension manifest cannot be null.");
        Validate(manifest);
        return CreatePackage(manifest, manifestPath, payload);
    }

    /// <summary>
    /// Names the keys an Extension manifest accepts, so an unaccepted key reports the
    /// manifest contract rather than the serializer's unmapped-member message.
    /// </summary>
    private static void ValidateAcceptedKeys(ReadOnlySpan<byte> bytes)
    {
        var reader = new Utf8JsonReader(
            bytes,
            new JsonReaderOptions
            {
                AllowTrailingCommas = false,
                CommentHandling = JsonCommentHandling.Disallow,
            });
        if (!reader.Read() || reader.TokenType != JsonTokenType.StartObject)
        {
            return;
        }

        while (reader.Read() && reader.TokenType == JsonTokenType.PropertyName)
        {
            var property = reader.GetString()
                ?? throw new JsonException("A JSON property name cannot be null.");
            if (!AcceptedKeys.Contains(property, StringComparer.Ordinal))
            {
                throw new JsonException(
                    $"The manifest key '{property}' is not accepted. Accepted keys: {string.Join(", ", AcceptedKeys)}.");
            }

            if (!reader.Read())
            {
                throw new JsonException($"The JSON property '{property}' has no value.");
            }

            if (reader.TokenType is JsonTokenType.StartObject or JsonTokenType.StartArray)
            {
                reader.Skip();
            }
        }
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
                Payload = [.. payload],
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
