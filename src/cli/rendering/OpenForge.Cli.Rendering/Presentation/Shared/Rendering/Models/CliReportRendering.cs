using System.Text.Json.Serialization.Metadata;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;

internal sealed record CliReportRendering<TResult, TData> where TResult : ICliCommandResult where TData : class
{
    public required CliReportSelector<TResult, TData> Selector { get; init; }
    public required CliDataTextRenderer<TData> DataTextRenderer { get; init; }
    public required JsonTypeInfo<TData> DataJsonTypeInfo { get; init; }
    public required CliCommandShape Shape { get; init; }
    public Func<CliSelectedReport<TData>, CliSelectedReport<TData>>? SelectText { get; init; }
}
