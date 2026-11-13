using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal sealed record ToolPackageManagerFacts
{
    public required string Id { get; init; }
    public required string Version { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public string? RuntimePath { get; init; }
    public string? RuntimeVersion { get; init; }
    public string? GlobalBinPath { get; init; }
}
