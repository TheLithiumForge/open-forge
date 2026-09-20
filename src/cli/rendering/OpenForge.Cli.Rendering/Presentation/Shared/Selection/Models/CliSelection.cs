using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Presentation.Shared.Models;

namespace OpenForge.Cli.Core.Presentation.Shared.Selection.Models;

internal sealed record CliSelection(CliDetail Detail, IReadOnlySet<CliSeverity>? Filter = null);

internal sealed record CliSelectedReport<TData>(CliReport<TData> Report, CliSelection Selection, bool ShowWorkspace)
    where TData : class
{
    internal bool ShowHeadline { get; init; } = true;
    internal bool ShowNext { get; init; } = true;
    internal IReadOnlyList<CliFinding> TextFindings { get; init; } = Report.Findings;
    internal IReadOnlyList<CliEffect> TextEffects { get; init; } = Report.Effects;
    internal IReadOnlyList<CliCount> TextCounts { get; init; } = Report.Counts;
    internal IReadOnlyList<CliLimitation> TextLimitations { get; init; } = Report.Limitations;
}

internal delegate CliReport<TData> CliReportSelector<in TResult, TData>(TResult result, CliSelection selection)
    where TResult : ICliCommandResult where TData : class;
