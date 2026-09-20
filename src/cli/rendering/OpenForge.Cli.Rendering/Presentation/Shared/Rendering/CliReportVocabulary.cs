using System.Text.Json;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering;

internal static class CliReportVocabulary
{
    internal static string Name(Enum value)
    {
        if (!Enum.IsDefined(value.GetType(), value))
        {
            throw new ArgumentOutOfRangeException(nameof(value), value, "The value is not defined.");
        }

        return JsonNamingPolicy.KebabCaseLower.ConvertName(value.ToString());
    }

    internal static CliSeverity Severity(CliSemanticStatus status) => status switch
    {
        CliSemanticStatus.Complete => CliSeverity.Info,
        CliSemanticStatus.Attention or CliSemanticStatus.Incomplete => CliSeverity.Warning,
        CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted => CliSeverity.Error,
        _ => throw new ArgumentOutOfRangeException(nameof(status)),
    };
}
