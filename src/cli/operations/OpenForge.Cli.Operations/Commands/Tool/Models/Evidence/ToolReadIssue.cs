using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Tool.Models.Evidence;

internal sealed record ToolReadIssue(ToolReadIssueCode Code, CliSemanticStatus Status, string Subject, string Cause)
{
    internal string MachineCode => ToolReadIssueCodes.Suffix(Code);
}
