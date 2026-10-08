namespace OpenForge.Cli.Core.Commands.Extension.Update.Models.Planning.Topology;

internal sealed class ExtensionUpdatePayloadRenderingException(string path, string cause)
    : Exception(cause)
{
    internal string Path { get; } = path;
}
