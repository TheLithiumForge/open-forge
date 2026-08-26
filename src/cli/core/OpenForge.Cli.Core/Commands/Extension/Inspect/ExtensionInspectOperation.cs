using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.Lifecycle.Models;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal sealed class ExtensionInspectOperation(
    ExtensionSourceReader sourceReader,
    LifecycleDocumentReader lifecycleReader,
    ExtensionInspectCurrentPathReader currentPathReader,
    ExtensionInspectResultBuilder resultBuilder)
{
    private readonly ExtensionSourceReader _sourceReader = sourceReader;
    private readonly LifecycleDocumentReader _lifecycleReader = lifecycleReader;
    private readonly ExtensionInspectCurrentPathReader _currentPathReader = currentPathReader;
    private readonly ExtensionInspectResultBuilder _resultBuilder = resultBuilder;

    internal async ValueTask<ExtensionInspectResult> ExecuteAsync(
        ExtensionInspectRequest request,
        CancellationToken cancellationToken)
    {
        ExtensionSourceReadResult? source = null;
        LifecycleReadResult? lifecycle = null;
        IReadOnlyList<ExtensionInspectCurrentPath>? currentPaths = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            source = await _sourceReader
                .ReadAsync(request.Workspace, request.ExplicitSource, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            lifecycle = await _lifecycleReader
                .ReadExtensionsAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            currentPaths = await _currentPathReader
                .ReadAsync(request.Workspace, lifecycle.Packages, request.StableId, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return _resultBuilder.Build(request, source, lifecycle, currentPaths);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return _resultBuilder.Event(new ExtensionInspectEventInput
            {
                Request = request,
                Source = source,
                Lifecycle = lifecycle,
                CurrentPaths = currentPaths,
                Status = CliSemanticStatus.Interrupted,
                Code = ExtensionInspectFindingCode.Interrupted,
                Cause = "Extension Inspect was interrupted.",
            });
        }
        catch (Exception exception) when (exception is IOException or InvalidOperationException or UnauthorizedAccessException)
        {
            return _resultBuilder.Event(new ExtensionInspectEventInput
            {
                Request = request,
                Source = source,
                Lifecycle = lifecycle,
                CurrentPaths = currentPaths,
                Status = CliSemanticStatus.Failed,
                Code = ExtensionInspectFindingCode.OperationFailed,
                Cause = exception.Message,
            });
        }
    }
}
