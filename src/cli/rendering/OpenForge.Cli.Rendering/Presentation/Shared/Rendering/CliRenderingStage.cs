using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal static class CliRenderingStage
{
    internal static CliRenderedOutput Render<TResult, TData>(
        CliPresentationRequest<TResult> presentation,
        CliReportRendering<TResult, TData> rendering)
        where TResult : ICliCommandResult where TData : class
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(rendering);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var options = presentation.Presentation;
        var selection = new CliSelection(options.Detail, options.Filter);
        var selected = CliReportSelection.Select(presentation.Result, selection, rendering);
        var report = selected.Report;
        var target = options.Format == CliFormat.Json ? CliOutputTarget.StandardOutput
            : CliStatusDefinitions.Read(report.Status).Disposition.HumanOutputTarget;
        var document = options.Format == CliFormat.Text
            ? CliTextRenderer.Render(selected, CliTextStyle.For(report.Status, options.Colors), rendering.DataTextRenderer) : null;
        var content = document?.Content ?? CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo);
        var diagnostics = OpenForge.Cli.Core.Presentation.Shared.Rendering.CliDiagnosticRenderer.Render(selected.Report.Diagnostics, selection.Detail);
        return new CliRenderedOutput(report.Status, options.Format, target, content, diagnostics) { TextDocument = document };
    }


}

