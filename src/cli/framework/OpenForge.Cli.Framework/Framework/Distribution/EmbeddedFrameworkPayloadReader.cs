using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.Framework.Distribution;

internal static class EmbeddedFrameworkPayloadReader
{
    private const string ResourcePrefix = "OpenForge.Framework.Payload/";
    private const string UnavailableCause = "No embedded Framework payload resources were found.";
    private const string InvalidCausePrefix = "The embedded Framework payload is invalid";

    internal static FrameworkPayloadReadResult Read()
    {
        var assembly = typeof(EmbeddedFrameworkPayloadReader).Assembly;
        var resourceNames = assembly.GetManifestResourceNames()
            .Where(name => name.StartsWith(ResourcePrefix, StringComparison.Ordinal))
            .Order(StringComparer.Ordinal)
            .ToArray();
        if (resourceNames.Length == 0)
        {
            return FrameworkPayloadReadResult.Unavailable(UnavailableCause);
        }

        try
        {
            var assets = new List<FrameworkPayloadAsset>(resourceNames.Length);
            foreach (var resourceName in resourceNames)
            {
                var path = resourceName[ResourcePrefix.Length..].Replace('\\', '/');
                using var stream = assembly.GetManifestResourceStream(resourceName)
                    ?? throw new InvalidDataException($"The manifest resource '{resourceName}' is unavailable.");
                using var buffer = new MemoryStream();
                stream.CopyTo(buffer);
                assets.Add(FrameworkPayloadAsset.Create(path, buffer.GetBuffer().AsSpan(0, checked((int)buffer.Length))));
            }

            return FrameworkPayloadReadResult.Available(FrameworkPayload.Create(assets));
        }
        catch (Exception exception) when (exception is ArgumentException or IOException)
        {
            return FrameworkPayloadReadResult.Invalid($"{InvalidCausePrefix}: {exception.Message}");
        }
    }
}
