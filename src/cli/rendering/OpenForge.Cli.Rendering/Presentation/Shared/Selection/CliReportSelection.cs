using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Selection;

internal static class CliReportSelection
{
    internal static CliSelectedReport<TData> Select<TResult, TData>(TResult result, CliSelection selection,
        CliReportRendering<TResult, TData> rendering) where TResult : ICliCommandResult where TData : class
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(rendering);
        var report = rendering.Selector(result, selection);
        if (report.Status != result.Status || report.Command != result.Command)
            throw new InvalidOperationException("Presentation cannot change the operation identity or status.");
        var selected = CliReportTrimmer.Trim(report, selection, rendering.Shape);
        return rendering.SelectText is { } selectText ? selectText(selected) : selected;
    }
}
