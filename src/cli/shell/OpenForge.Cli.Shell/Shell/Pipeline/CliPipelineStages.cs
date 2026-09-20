using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Presentation.Models;

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

internal static class CliCompletionStage
{
    internal static CliProcessCompletion Complete(CliOutputReceipt receipt)
    {
        ArgumentNullException.ThrowIfNull(receipt);
        CliPresentationDefinitions.ValidateOutputPolicy(receipt.Status, receipt.Format, receipt.PrimaryTarget);
        return CliProcessCompletionPolicy.Complete(receipt.Status, receipt.PrimaryTarget);
    }
}
