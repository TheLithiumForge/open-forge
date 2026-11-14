namespace OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

internal sealed record ToolCoverage(string Store, ToolEvidenceState State, string? Cause);
