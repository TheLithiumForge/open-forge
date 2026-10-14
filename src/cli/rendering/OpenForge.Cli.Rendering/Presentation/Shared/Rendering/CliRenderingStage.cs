using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal static class CliRenderingStage
{
    internal static CliRenderedOutput Render<TResult, TData>(
        CliPresentationRequest<TResult> presentation,
        CliReportRendering<TResult, TData> rendering, CliRequestReplay? replay = null)
        where TResult : ICliCommandResult where TData : class
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(rendering);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
        var options = presentation.Presentation;
        var selection = new CliSelection(options.Detail, options.Filter);
        var selected = CliReportSelection.Select(presentation.Result, selection, rendering, replay);
        var report = selected.Report;
        var target = options.Format == CliFormat.Json ? CliOutputTarget.StandardOutput
            : CliStatusDefinitions.Read(report.Status).Disposition.HumanOutputTarget;
        var document = options.Format == CliFormat.Text
            ? CliTextRenderer.Render(selected, CliTextStyle.For(report.Status, options.Colors).WithWidth(options.Width), rendering.DataTextRenderer) : null;
        var content = document?.Content ?? CliJsonRenderer.Render(selected, rendering.DataJsonTypeInfo);
        var diagnostics = OpenForge.Cli.Core.Presentation.Shared.Rendering.CliDiagnosticRenderer.Render(
            options.Format == CliFormat.Json ? report.Diagnostics : selected.TextDiagnostics, selection.Detail);
        return new CliRenderedOutput(report.Status, options.Format, target, content, diagnostics) { TextDocument = document };
    }


}
