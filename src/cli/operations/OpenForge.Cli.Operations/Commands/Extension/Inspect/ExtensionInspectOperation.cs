using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Request;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Models.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Result;
using OpenForge.Cli.Core.Commands.Extension.Inspect.Shared.Reading;
using OpenForge.Cli.Core.Framework.Extensions;
using OpenForge.Cli.Core.Framework.Extensions.Models;
using OpenForge.Cli.Core.Framework.Ownership.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Extension.Inspect;

internal sealed class ExtensionInspectOperation(
    ExtensionSourceReader sourceReader,
    PhysicalPathResolver physicalPathResolver,
    ExtensionInspectCurrentPathReader currentPathReader,
    ExtensionInspectResultBuilder resultBuilder)
{
    private readonly ExtensionSourceReader _sourceReader = sourceReader;
    private readonly PhysicalPathResolver _physicalPathResolver = physicalPathResolver;
    private readonly ExtensionInspectCurrentPathReader _currentPathReader = currentPathReader;
    private readonly ExtensionInspectResultBuilder _resultBuilder = resultBuilder;

    internal async ValueTask<ExtensionInspectResult> ExecuteAsync(
        ExtensionInspectRequest request,
        CancellationToken cancellationToken)
    {
        ExtensionSourceReadResult? source = null;
        WorkspaceOwnershipRead? lifecycle = null;
        IReadOnlyList<ExtensionInspectCurrentPath>? currentPaths = null;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            source = await _sourceReader
                .ReadAsync(request.Workspace, request.ExplicitSource, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            var observation = await new WorkspaceLayoutReader(_physicalPathResolver)
                .ReadAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
            lifecycle = ExtensionInspectInstalledClosureReader.ReadOwnership(observation.Ownership);
            cancellationToken.ThrowIfCancellationRequested();
            currentPaths = await _currentPathReader
                .ReadAsync(request.Workspace, lifecycle.Document.Extensions, request.StableId, cancellationToken)
                .ConfigureAwait(false);
            cancellationToken.ThrowIfCancellationRequested();
            return _resultBuilder.Build(request, source, observation, currentPaths);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return _resultBuilder.Event(new ExtensionInspectEventInput
            {
                Request = request,
                Source = source,
                Ownership = lifecycle,
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
                Ownership = lifecycle,
                CurrentPaths = currentPaths,
                Status = CliSemanticStatus.Failed,
                Code = ExtensionInspectFindingCode.OperationFailed,
                Cause = exception.Message,
            });
        }
    }
}
