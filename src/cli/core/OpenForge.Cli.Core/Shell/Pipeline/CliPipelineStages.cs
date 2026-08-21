using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Shell.Pipeline;

internal static class CliOperationStage
{
    internal static async ValueTask<CliOperationResult<TResult>> InvokeAsync<TRequest, TResult>(
        CliOperationRequest<TRequest> request,
        CliOperation<TRequest, TResult> operation,
        CancellationToken cancellationToken)
        where TResult : ICliCommandResult
    {
        ArgumentNullException.ThrowIfNull(request);
        ArgumentNullException.ThrowIfNull(request.Request);
        ArgumentNullException.ThrowIfNull(operation);
        cancellationToken.ThrowIfCancellationRequested();

        var result = await operation(request.Request, cancellationToken).ConfigureAwait(false);
        ArgumentNullException.ThrowIfNull(result);
        ValidateResult(result);
        return new CliOperationResult<TResult>(result);
    }

    internal static void ValidateResult(ICliCommandResult result)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentException.ThrowIfNullOrWhiteSpace(result.Command);
        _ = CliStatusDefinitions.Read(result.Status);
    }
}

internal static class CliPresentationStage
{
    internal static CliPresentationRequest<TResult> Create<TResult>(
        TResult result,
        CliPresentation presentation)
        where TResult : ICliCommandResult
    {
        CliOperationStage.ValidateResult(result);
        CliPresentationDefinitions.Validate(presentation);
        return new CliPresentationRequest<TResult>(result, presentation);
    }
}

internal static class CliRenderingStage
{
    internal const int MaximumDiagnosticLength = 4096;

    internal static CliRenderedOutput Render<TResult>(
        CliPresentationRequest<TResult> presentation,
        CliRendererSet<TResult> renderers,
        CliDiagnosticRenderer<TResult>? diagnosticRenderer)
        where TResult : ICliCommandResult
    {
        Validate(presentation, renderers);
        var renderer = renderers.Read(presentation.Presentation.Format);
        var primaryContent = renderer(presentation);
        ArgumentNullException.ThrowIfNull(primaryContent);

        string? diagnosticContent = null;
        if (presentation.Presentation.Verbosity == CliVerbosity.Verbose
            && diagnosticRenderer is not null)
        {
            diagnosticContent = diagnosticRenderer(presentation);
            if (diagnosticContent is { Length: > MaximumDiagnosticLength })
            {
                throw new InvalidOperationException(
                    $"Diagnostic content exceeds the {MaximumDiagnosticLength}-character limit.");
            }
        }

        var target = presentation.Presentation.Format switch
        {
            CliOutputFormat.Human => CliStatusDefinitions.Read(presentation.Result.Status).Disposition.HumanOutputTarget,
            CliOutputFormat.Json => CliOutputTarget.StandardOutput,
            _ => throw new ArgumentOutOfRangeException(
                nameof(presentation),
                presentation.Presentation.Format,
                "The output format is not defined."),
        };

        return new CliRenderedOutput(
            presentation.Result.Status,
            presentation.Presentation.Format,
            target,
            primaryContent,
            diagnosticContent);
    }

    private static void Validate<TResult>(
        CliPresentationRequest<TResult> presentation,
        CliRendererSet<TResult> renderers)
        where TResult : ICliCommandResult
    {
        ArgumentNullException.ThrowIfNull(presentation);
        ArgumentNullException.ThrowIfNull(renderers);
        CliOperationStage.ValidateResult(presentation.Result);
        CliPresentationDefinitions.Validate(presentation.Presentation);
    }
}

internal static class CliOutputStage
{
    internal static async ValueTask<CliOutputReceipt> WriteAsync(
        CliRenderedOutput output,
        CliOutputWriters writers,
        CancellationToken cancellationToken)
    {
        Validate(output, writers);
        cancellationToken.ThrowIfCancellationRequested();

        var primaryWriter = output.PrimaryTarget switch
        {
            CliOutputTarget.StandardOutput => writers.StandardOutput,
            CliOutputTarget.StandardError => writers.StandardError,
            _ => throw new ArgumentOutOfRangeException(
                nameof(output),
                output.PrimaryTarget,
                "The output target is not defined."),
        };
        await primaryWriter.WriteLineAsync(output.PrimaryContent.AsMemory(), cancellationToken).ConfigureAwait(false);

        var diagnosticWritten = output.DiagnosticContent is not null;
        if (diagnosticWritten)
        {
            await writers.StandardError
                .WriteLineAsync(output.DiagnosticContent.AsMemory(), cancellationToken)
                .ConfigureAwait(false);
        }

        return new CliOutputReceipt(output.Status, output.Format, output.PrimaryTarget, diagnosticWritten);
    }

    private static void Validate(CliRenderedOutput output, CliOutputWriters writers)
    {
        ArgumentNullException.ThrowIfNull(output);
        ArgumentNullException.ThrowIfNull(writers);
        ArgumentNullException.ThrowIfNull(output.PrimaryContent);
        _ = CliStatusDefinitions.Read(output.Status);
        CliPresentationDefinitions.ValidateOutputPolicy(output.Status, output.Format, output.PrimaryTarget);
        if (output.DiagnosticContent is { Length: > CliRenderingStage.MaximumDiagnosticLength })
        {
            throw new InvalidOperationException("Diagnostic content is not bounded.");
        }
    }
}

internal static class CliCompletionStage
{
    internal static CliProcessCompletion Complete(CliOutputReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        CliPresentationDefinitions.ValidateOutputPolicy(receipt.Status, receipt.Format, receipt.PrimaryTarget);
        return CliProcessCompletionPolicy.Complete(receipt.Status, receipt.PrimaryTarget);
    }
}
