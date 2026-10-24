using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Status;

internal sealed class StatusOperation(
    OperationalContributorCatalogue contributors,
    PhysicalPathResolver physicalPathResolver)
{
    internal async ValueTask<StatusResult> ExecuteAsync(
        StatusRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var observation = await new WorkspaceLayoutReader(physicalPathResolver)
                .ReadAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var layout = observation.Layout;
            var ownership = observation.Ownership;
            var workspaceEntry = await contributors.WorkspaceEntry
                .ReadStatusAsync(request.Workspace, observation, cancellationToken)
                .ConfigureAwait(false);
            var recoveryResiduals = await contributors.RecoveryResiduals
                .ReadStatusAsync(request.Workspace, cancellationToken)
                .ConfigureAwait(false);
            var routes = await contributors.Routes
                .ReadStatusAsync(observation, NavigationSharingReader.FromOwnership(ownership).Sharing, cancellationToken)
                .ConfigureAwait(false);
            var frameworkLifecycle = await contributors.FrameworkLifecycle
                .ReadStatusAsync(observation, cancellationToken)
                .ConfigureAwait(false);
            var extensionLifecycle = await contributors.ExtensionLifecycle
                .ReadStatusAsync(observation, cancellationToken)
                .ConfigureAwait(false);
            var libraries = await contributors.Libraries.ReadStatusAsync(request.Workspace, ownership, cancellationToken).ConfigureAwait(false);
            var observations = new StatusObservationSet(
                layout,
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
