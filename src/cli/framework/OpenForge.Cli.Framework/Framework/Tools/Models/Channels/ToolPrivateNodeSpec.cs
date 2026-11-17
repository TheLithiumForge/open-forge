namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal sealed record ToolPrivateNodeSpec
{
    public required string Version { get; init; }
    public required ToolReleaseAsset Archive { get; init; }
    public required Uri SignedChecksums { get; init; }
    public required string VerifiedSigner { get; init; }
    public required string NpmRelativePath { get; init; }
}
