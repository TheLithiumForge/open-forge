namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Writing;

internal sealed class ToolWriterFailure(ToolWriteIssue issue) : Exception(issue.Cause)
{
    internal ToolWriteIssue Issue { get; } = issue;
}
