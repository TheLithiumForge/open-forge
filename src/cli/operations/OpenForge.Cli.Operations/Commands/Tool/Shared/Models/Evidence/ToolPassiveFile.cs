namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

internal sealed record ToolPassiveFile(ToolEvidenceState State, byte[]? Bytes, string? Cause);
