using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Distribution.Shared.Content;
using OpenForge.Cli.Core.Framework.Documents.Metadata.Models;

namespace OpenForge.Cli.Core.Commands.Install.Shared.Planning;

internal static class InstallPayloadRendering
{
    internal static IReadOnlyDictionary<string, byte[]> Render(FrameworkPayload payload, FrontmatterForm form)
        => payload.Assets.ToDictionary(asset => asset.Path,
            asset => Render(asset.Path, asset.Bytes.AsMemory(), form), StringComparer.Ordinal);

    internal static byte[] Render(string path, ReadOnlyMemory<byte> bytes, FrontmatterForm form)
    {
        var rendered = WorkspacePayloadRenderer.Render(path, bytes, form);
        return rendered.Bytes?.ToArray()
            ?? throw new InvalidDataException($"Payload '{path}' cannot be delivered: {rendered.Cause}");
    }
}
