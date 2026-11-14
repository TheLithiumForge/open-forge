using OpenForge.Cli.Core.Commands.Tool.Models.Writing.Result;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Planning;

internal sealed record ToolWriteToolPlanningResult(
    ToolWriteToolPlan? Tool, ToolWriteManagerQuestion? Manager, IReadOnlyList<ToolWriteFinding> Findings, bool UsedNetwork);
