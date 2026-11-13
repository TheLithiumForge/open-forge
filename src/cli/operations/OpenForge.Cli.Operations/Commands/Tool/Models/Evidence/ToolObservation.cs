namespace OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

internal sealed record ToolObservation(ToolEvidenceState State, string? Cause = null);
