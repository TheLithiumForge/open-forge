using System.Text.Json.Serialization;

namespace OpenForge.Cli.Core.Framework.Tools.Shared.Serialization.Models;

internal sealed record ToolCbmNativePlan
{
    public required string Type { get; init; }
    public required string[] AgentsDetected { get; init; }
    public required string[] ConfigFilesPlanned { get; init; }
    public required string[] InstructionFilesPlanned { get; init; }
    public required string[] SkillFilesPlanned { get; init; }
    public required string[] AgentFilesPlanned { get; init; }
    public required string[] PromptFilesPlanned { get; init; }
    public required ToolCbmNativeHook[] HooksPlanned { get; init; }
    public required bool WritesStarted { get; init; }
    public required bool NetworkAfterInstall { get; init; }
    public required string NextSafeCommand { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
internal sealed record ToolCbmNativeHook
{
    public required string Agent { get; init; }
    public required string Path { get; init; }
}
