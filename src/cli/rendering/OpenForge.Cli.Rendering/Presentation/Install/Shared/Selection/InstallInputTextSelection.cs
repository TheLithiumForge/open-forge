using OpenForge.Cli.Core.Presentation.Install.Models;
using OpenForge.Cli.Core.Presentation.Shared.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;

namespace OpenForge.Cli.Core.Presentation.Install.Shared.Selection;

internal static class InstallInputTextSelection
{
    internal static CliSelectedReport<InstallData> Select(CliSelectedReport<InstallData> selected)
    {
        var data = selected.Report.Data;
        string headline;
        if (data.InvalidRoute is { } route)
            headline = global::OpenForge.Cli.OutputText.Install.InstallInputText.InvalidRoute(route.Value);
        else if (data.InvalidOption is { } option)
            headline = InstallReportSelector.InvalidOptionMessage(option);
        else return selected;
        return selected with
        {
            TextHeadline = new CliHeadline(headline, CliHeadlineKind.CannotStart),
            TextFindings = selected.TextFindings.Where(finding => finding.Code != "install.invalid-input").ToArray(),
        };
    }
}
