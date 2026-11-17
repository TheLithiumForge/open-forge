namespace OpenForge.Cli.Core.Commands.Tool.Models.Writing.Selection;

internal enum ToolWriteSelectionIssueCode { NamedSpecRequired, UnknownId, UnknownChannel, UpdateRequiresRecord }

internal sealed record ToolWriteSelectionIssue(ToolWriteSelectionIssueCode Code, string? ToolId);

internal sealed record ToolWriteSelectionResult(
    IReadOnlyList<ToolWriteSelection> Tools,
    IReadOnlyList<ToolWriteSelectionIssue> Issues);
