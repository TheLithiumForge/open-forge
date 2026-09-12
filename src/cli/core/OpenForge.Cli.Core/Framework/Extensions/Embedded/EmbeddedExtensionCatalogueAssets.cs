using System.Collections.ObjectModel;
using OpenForge.Cli.Core.Framework.Filesystem.Shared.Paths;

namespace OpenForge.Cli.Core.Framework.Extensions.Embedded;

internal static class EmbeddedExtensionCatalogueAssets
{
    private const string ResourcePrefix = "OpenForge.Extensions.Payload/";

    internal static IReadOnlyDictionary<string, ReadOnlyMemory<byte>> All
    {
        get
        {
            var assembly = typeof(EmbeddedExtensionCatalogueAssets).Assembly;
            var names = assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
                .Order(StringComparer.Ordinal);
            var assets = new Dictionary<string, ReadOnlyMemory<byte>>(StringComparer.Ordinal);
            foreach (var name in names)
            {
                var path = name[ResourcePrefix.Length..].Replace('\\', '/');
                if (!PortableWorkspacePath.TryNormalize(path, out var normalized)
                    || !string.Equals(path, normalized, StringComparison.Ordinal)
                    || !path.Contains('/'))
                {
                    throw new InvalidDataException($"Embedded Extension asset '{path}' has an invalid package path.");
                }

                using var stream = assembly.GetManifestResourceStream(name)
                    ?? throw new InvalidDataException($"Embedded Extension resource '{name}' is unavailable.");
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
                if (!assets.TryAdd(path, buffer.ToArray()))
                {
                    throw new InvalidDataException($"Embedded Extension asset '{path}' occurs more than once.");
                }
            }

            if (assets.Count == 0)
            {
                throw new InvalidDataException("No embedded Extension package resources were found.");
            }

            return new ReadOnlyDictionary<string, ReadOnlyMemory<byte>>(assets);
        }
    }
}
