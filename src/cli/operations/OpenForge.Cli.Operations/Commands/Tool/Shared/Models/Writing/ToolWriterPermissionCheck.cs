namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed record ToolWriterPermissionCheck(ToolDestinationDecision[] Decisions, string[] Grants, ToolWriteIssue[] Issues);
