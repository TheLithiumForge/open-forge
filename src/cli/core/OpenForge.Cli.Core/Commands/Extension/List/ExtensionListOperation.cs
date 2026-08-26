using OpenForge.Cli.Core.Commands.Extension.List.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.List.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.List.Shared.Result;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.List;

internal sealed class ExtensionListOperation(
    ExtensionSourceReader sourceReader,
    LifecycleDocumentReader lifecycleReader)
{
    private readonly ExtensionSourceReader _sourceReader = sourceReader;
    private readonly LifecycleDocumentReader _lifecycleReader = lifecycleReader;

    internal async ValueTask<ExtensionListResult> ExecuteAsync(
        ExtensionListRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var source = await _sourceReader
                .ReadAsync(request.Workspace, request.ExplicitSource, cancellationToken)
                .ConfigureAwait(false);
            var lifecycle = request.Selection.Installed
                ? await _lifecycleReader.ReadExtensionsAsync(request.Workspace, cancellationToken).ConfigureAwait(false)
                : null;
            return ExtensionListResultBuilder.Build(request, source, lifecycle);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return ExtensionListResultBuilder.Event(
                request,
                CliSemanticStatus.Interrupted,
                ExtensionListFindingCode.Interrupted,
                "Extension List was interrupted.");
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            return ExtensionListResultBuilder.Event(
                request,
                CliSemanticStatus.Failed,
                ExtensionListFindingCode.OperationFailed,
                exception.Message);
        }
    }
}
