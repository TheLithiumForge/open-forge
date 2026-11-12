using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;

internal sealed record ToolReadIssue(ToolReadIssueCode Code, CliSemanticStatus Status, string Subject, string Cause);
