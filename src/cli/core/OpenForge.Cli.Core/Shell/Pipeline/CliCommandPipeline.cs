using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Presentation.Models;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal sealed class CliCommandPipeline<TRequest, TResult>
    where TResult : ICliCommandResult
{
    private readonly CliOperation<TRequest, TResult> _operation;
    private readonly CliRendererSet<TResult> _renderers;
    private readonly CliDiagnosticRenderer<TResult>? _diagnosticRenderer;

    internal CliCommandPipeline(
        CliOperation<TRequest, TResult> operation,
        CliRendererSet<TResult> renderers,
        CliDiagnosticRenderer<TResult>? diagnosticRenderer = null)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(renderers);
        _operation = operation;
        _renderers = renderers;
        _diagnosticRenderer = diagnosticRenderer;
    }

    internal async ValueTask<CliProcessCompletion> ExecuteAsync(
        TRequest request,
        CliPresentation presentation,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        var operation = await CliOperationStage
            .InvokeAsync(new CliOperationRequest<TRequest>(request), _operation, cancellationToken)
            .ConfigureAwait(false);
        return await PresentAsync(operation.Result, presentation, writers).ConfigureAwait(false);
    }

    internal async ValueTask<CliProcessCompletion> PresentAsync(
        TResult result,
        CliPresentation presentation,
        CliOutputWriters writers)
    {
        var presentationRequest = CliPresentationStage.Create(result, presentation);
        var rendered = CliRenderingStage.Render(presentationRequest, _renderers, _diagnosticRenderer);
        var receipt = await CliOutputStage
            .WriteAsync(rendered, writers, CancellationToken.None)
            .ConfigureAwait(false);
        return CliCompletionStage.Complete(receipt);
    }
}
