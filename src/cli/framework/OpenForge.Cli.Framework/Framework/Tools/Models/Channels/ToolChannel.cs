namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal enum ToolChannelKind { Npm, GitHubRelease }

internal abstract record ToolChannel
{
    public required string Id { get; init; }
    public required ToolChannelKind Kind { get; init; }
    public required ToolChannelRequirements Requirements { get; init; }
}

internal sealed record ToolNpmChannel : ToolChannel
{
    public required string Package { get; init; }
}

internal sealed record ToolGitHubReleaseChannel : ToolChannel
{
    public required string Repository { get; init; }
    public required IReadOnlyList<ToolReleaseAsset> Assets { get; init; }
}
