using System.Net;

namespace OpenForge.Cli.Composition.Shared.Tools.Npm.Models;

internal sealed record ToolNpmMetadataEnvironment
{
    public required string UserHome { get; init; }
    public required IReadOnlyDictionary<string, string?> Variables { get; init; }
    public IWebProxy? DefaultProxy { get; init; }
    public IReadOnlyList<string> AdditionalConfigPaths { get; init; } = [];
}
