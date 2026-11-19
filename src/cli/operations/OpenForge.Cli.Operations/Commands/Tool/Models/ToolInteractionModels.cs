using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.Core.Commands.Tool.Models;

/// <summary>The plan question. The preview result carries every fact the question shows.</summary>
internal sealed record ToolPlanQuestion;

/// <summary>Whether to run a tool's own uninstall after Open Forge's changes are removed.</summary>
internal sealed record ToolUninstallQuestion(ToolWriteTool Tool);

internal sealed record ToolInteraction
{
    public required bool CanPrompt { get; init; }
    public required CliPlanConfirmation<ToolWriteResult, ToolPlanQuestion> ConfirmPlan { get; init; }
    public required CliPrompt<ToolUninstallQuestion, bool> ConfirmUninstall { get; init; }
}
