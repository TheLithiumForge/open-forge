using OpenForge.Cli.Core.Framework.Tools.Models.Launchers;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Channels;

internal enum ToolNpmClient { Npm, Pnpm, YarnClassic, YarnModern, Bun }
internal enum ToolManagerChoiceMode { Ask, Default }

internal sealed record ToolPackageManagerFacts
{
    public required ToolNpmClient Client { get; init; }
    public required string Version { get; init; }
    public required ToolCommandTarget Invocation { get; init; }
    public string? RuntimePath { get; init; }
    public string? RuntimeVersion { get; init; }
    public string? GlobalBinPath { get; init; }
    public required ToolPathReadiness GlobalBinReadiness { get; init; }
    public required bool SupportsGlobalInstall { get; init; }
}
