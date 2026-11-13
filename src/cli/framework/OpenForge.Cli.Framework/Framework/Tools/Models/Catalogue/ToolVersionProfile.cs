using OpenForge.Cli.Core.Framework.Tools.Models.Configuration;

namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal sealed record ToolVersionProfile
{
    public required string Id { get; init; }
    public required ToolVersionRange Range { get; init; }
    public required IReadOnlyList<string> TestedVersions { get; init; }
    public required string MinimumOpenForgeVersion { get; init; }
    public ToolSetupSpec? Setup { get; init; }
    public required IReadOnlyList<ToolConfigChange> Changes { get; init; }
    public required ToolAgentGuidance Guidance { get; init; }
}
