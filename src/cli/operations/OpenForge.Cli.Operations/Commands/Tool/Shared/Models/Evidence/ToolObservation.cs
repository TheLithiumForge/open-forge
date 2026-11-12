namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

internal sealed record ToolObservation(ToolEvidenceState State, string? Cause = null);
