namespace OpenForge.Cli.Core.Framework.Tools.Models.Catalogue;

internal sealed record ToolAgentGuidance
{
    public required string SkillResource { get; init; }
    public required string DirectCommand { get; init; }
    public required string ManagedBlockLine { get; init; }
}
