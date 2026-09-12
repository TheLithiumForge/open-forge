using System.Text.Json;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Manifest;
using OpenForge.Cli.Core.Commands.Extension.Create.Models.Request;
using OpenForge.Cli.Core.Framework.Extensions.Models.Serialization;
using OpenForge.Cli.Core.Framework.Extensions.Serialization;

namespace OpenForge.Cli.Core.Commands.Extension.Create.Shared.Manifest;

internal static class ExtensionCreateManifestBuilder
{
    internal static ExtensionCreateManifestPayload Build(
        ExtensionCreateRequest request,
        string stableId)
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentException.ThrowIfNullOrWhiteSpace(stableId);
        var orderedDependencies = request.Dependencies.Order(StringComparer.Ordinal).ToArray();
        var manifest = new ExtensionCreateManifest
        {
            Id = stableId,
            Name = request.Name ?? DeriveName(stableId),
            Description = request.Description ?? $"Open Forge Extension package {stableId}.",
            Version = request.PackageVersion ?? ExtensionCreateDefinitions.DefaultPackageVersion,
            Dependencies = orderedDependencies,
        };
        var document = new ExtensionManifestDocument
        {
            Id = manifest.Id,
            Name = manifest.Name,
            Description = manifest.Description,
            Version = manifest.Version,
            Dependencies = orderedDependencies,
        };
        return new ExtensionCreateManifestPayload(
            Manifest: manifest,
            Bytes: JsonSerializer.SerializeToUtf8Bytes(
                document,
                ExtensionPackageJsonContext.Default.ExtensionManifestDocument));
    }

    private static string DeriveName(string stableId)
        => string.Join(' ', stableId.Split('-').Select(UppercaseFirstAscii));

    private static string UppercaseFirstAscii(string segment)
    {
        if (segment.Length == 0 || segment[0] is not (>= 'a' and <= 'z'))
        {
            return segment;
        }

        return ((char)(segment[0] - ('a' - 'A'))).ToString() + segment[1..];
    }
}
