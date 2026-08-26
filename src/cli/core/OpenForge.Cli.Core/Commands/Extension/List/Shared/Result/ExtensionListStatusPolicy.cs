using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;

internal static class ExtensionListStatusPolicy
{
    private static ReadOnlySpan<CliSemanticStatus> Precedence =>
    [
        CliSemanticStatus.Interrupted,
        CliSemanticStatus.Failed,
        CliSemanticStatus.Invalid,
        CliSemanticStatus.Blocked,
        CliSemanticStatus.Incomplete,
        CliSemanticStatus.Attention,
    ];

    internal static CliSemanticStatus ReadStatus(IEnumerable<ExtensionListFinding> findings)
    {
        var statuses = findings.Select(finding => finding.Status).ToHashSet();
        foreach (var status in Precedence)
        {
            if (statuses.Contains(status))
            {
                return status;
            }
        }

        return CliSemanticStatus.Complete;
    }

    internal static CliNextAction? ReadNext(CliSemanticStatus status)
        => status switch
        {
            CliSemanticStatus.Complete or CliSemanticStatus.Attention => null,
            CliSemanticStatus.Incomplete => ExtensionListDefinitions.IncompleteNext,
            CliSemanticStatus.Invalid => ExtensionListDefinitions.InvalidNext,
            CliSemanticStatus.Blocked => ExtensionListDefinitions.BlockedNext,
            CliSemanticStatus.Failed => ExtensionListDefinitions.FailedNext,
            CliSemanticStatus.Interrupted => ExtensionListDefinitions.InterruptedNext,
            _ => throw new ArgumentOutOfRangeException(nameof(status), status, "The Extension List status is not defined."),
        };
}
