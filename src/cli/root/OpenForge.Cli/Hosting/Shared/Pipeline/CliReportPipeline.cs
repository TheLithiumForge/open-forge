using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Presentation.Shared.Rendering.Models;
using OpenForge.Cli.Core.Presentation.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Selection.Models;
using OpenForge.Cli.Core.Presentation.Shared.Text;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal sealed class CliReportPipeline<TRequest, TResult, TData>(
    CliOperation<TRequest, TResult> operation,
    CliReportRendering<TResult, TData> rendering)
    where TResult : ICliCommandResult where TData : class
{
    internal async ValueTask<CliProcessCompletion> ExecuteAsync(
        TRequest request, CliPresentation presentation, CliOutputWriters writers, CancellationToken cancellationToken)
    {
        var result = await CliOperationStage.InvokeAsync(new CliOperationRequest<TRequest>(request), operation, cancellationToken).ConfigureAwait(false);
        return await PresentAsync(result.Result, presentation, writers).ConfigureAwait(false);
    }

    internal async ValueTask<CliProcessCompletion> PresentAsync(TResult result, CliPresentation presentation, CliOutputWriters writers)
    {
        var request = CliPresentationStage.Create(result, presentation with { Colors = writers.Colors });
        var output = CliRenderingStage.Render(request, rendering);
        var receipt = await CliOutputStage.WriteAsync(output, writers, CancellationToken.None).ConfigureAwait(false);
        return CliCompletionStage.Complete(receipt);
    }
}
