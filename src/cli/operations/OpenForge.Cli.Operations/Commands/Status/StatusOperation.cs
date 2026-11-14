using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Commands.Status.Models.Operation;
using OpenForge.Cli.Core.Commands.Status.Models.Request;
using OpenForge.Cli.Core.Commands.Status.Models.Result;
using OpenForge.Cli.Core.Commands.Status.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Observation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;

namespace OpenForge.Cli.Core.Commands.Status;

internal sealed class StatusOperation(
    OperationalContributorCatalogue contributors,
    PhysicalPathResolver physicalPathResolver,
    Func<ToolWorkspaceObservationReader>? tools = null)
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
            var outside = observation.Layout is null || observation.OutsideRoutes.Length > 0
                ? await contributors.Routes.ReadOutsideAsync(observation, cancellationToken).ConfigureAwait(false) : null;
            var sharing = NavigationSharingReader.FromOwnership(ownership).Sharing;
            var routes = outside is null
                ? await contributors.Routes.ReadStatusAsync(observation, sharing, cancellationToken).ConfigureAwait(false)
                : await contributors.Routes.ReadStatusAsync(observation, sharing, cancellationToken, outside).ConfigureAwait(false);
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
                libraries)
            {
                LayoutRead = observation,
                OutsideRoutes = outside,
            };
            var result = StatusResultBuilder.Build(request, observations);
            if (tools is null) return result;
            try
            {
                return StatusToolFindingAppender.Append(result, await tools().ReadAsync(request.Workspace.PhysicalRoot,
                    layout?.MainRoot ?? WorkspaceLayout.DefaultMainRoot, cancellationToken).ConfigureAwait(false));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            { return StatusToolFindingAppender.Append(result, [], exception.Message); }
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
