using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Lifecycle;
using OpenForge.Cli.Core.Framework.OperationalContributors;

namespace OpenForge.Cli.Core.Commands.Status;

internal sealed class StatusOperation(
    OperationalContributorCatalogue contributors,
    LifecycleDocumentSnapshotReader lifecycleSnapshotReader)
{
    internal async ValueTask<StatusResult> ExecuteAsync(
        StatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var lifecycleSnapshot = await lifecycleSnapshotReader
                .ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var workspaceEntry = await contributors.WorkspaceEntry
                .ReadStatusAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var recoveryResiduals = await contributors.RecoveryResiduals
                .ReadStatusAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var routes = await contributors.Routes
                .ReadStatusAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var frameworkLifecycle = await contributors.FrameworkLifecycle
                .ReadStatusAsync(lifecycleSnapshot, cancellationToken)
                .ConfigureAwait(false);
            var extensionLifecycle = await contributors.ExtensionLifecycle
                .ReadStatusAsync(lifecycleSnapshot, cancellationToken)
                .ConfigureAwait(false);
            var libraries = await contributors.Libraries.ReadStatusAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
            var observations = new StatusObservationSet(
                workspaceEntry,
                recoveryResiduals,
                routes,
                frameworkLifecycle,
                extensionLifecycle,
                libraries);
            return StatusResultBuilder.Build(request, observations);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return StatusResultBuilder.Event(
                request.Workspace,
                StatusFindingCode.Interrupted,
                null,
                "The Status observation was interrupted.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return StatusResultBuilder.Event(
                request.Workspace,
                StatusFindingCode.OperationFailed,
                null,
                $"The Status observation failed: {exception.GetType().Name}.");
        }
    }
}
