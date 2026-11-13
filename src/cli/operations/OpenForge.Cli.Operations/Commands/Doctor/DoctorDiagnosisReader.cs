using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Framework.Settings.Shared.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Shared.NavigationSharing;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Maintenance;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Tools.Shared.Backups;
using OpenForge.Cli.Core.Commands.Tool.Run;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorDiagnosisReader(OperationalContributorCatalogue contributors, RenameEvidenceReader? renames = null,
    Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>>? readLayout = null, ToolRunMissingReader? missingTools = null)
{
    private readonly ToolRunMissingReader _missingTools = missingTools ?? new(new());
    private readonly OperationalContributorCatalogue _contributors = contributors;
    private readonly DoctorMaintenanceReader _maintenance = new(renames ?? new RenameEvidenceReader(new PhysicalPathResolver()));

    internal async ValueTask<DoctorDiagnosisRead> ReadAsync(
        DoctorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var observation = await (readLayout ?? new WorkspaceLayoutReader(new PhysicalPathResolver()).ReadAsync)(request.Workspace, cancellationToken).ConfigureAwait(false);
        var workspace = await _contributors.WorkspaceEntry
            .ReadDoctorAsync(request.Workspace, observation, cancellationToken)
            .ConfigureAwait(false);
        var recovery = await _contributors.RecoveryResiduals
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var sharing = NavigationSharingReader.FromOwnership(observation.Ownership);
        var outside = observation.Layout is null || observation.OutsideRoutes.Length > 0
            ? await _contributors.Routes.ReadOutsideAsync(observation, cancellationToken).ConfigureAwait(false) : null;
        var routes = outside is null
            ? await _contributors.Routes.ReadDoctorAsync(observation, sharing.Sharing, cancellationToken).ConfigureAwait(false)
            : await _contributors.Routes.ReadDoctorAsync(observation, sharing.Sharing, cancellationToken, outside).ConfigureAwait(false);
        var referenceRead = _contributors.LocalReferences
            .ReadDoctorAsync(observation, cancellationToken, routes)
            .AsTask();
        var maintenanceRead = _maintenance.ReadAsync(routes, referenceRead, sharing, observation, cancellationToken).AsTask();
        await Task.WhenAll(referenceRead, maintenanceRead).ConfigureAwait(false);
        var references = await referenceRead.ConfigureAwait(false);
        var framework = await _contributors.FrameworkLifecycle
            .ReadDoctorAsync(observation, cancellationToken)
            .ConfigureAwait(false);
        var extensions = await _contributors.ExtensionLifecycle
            .ReadDoctorAsync(observation, cancellationToken)
            .ConfigureAwait(false);
        var libraries = await _contributors.Libraries.ReadDoctorAsync(request.Workspace, observation.Ownership, cancellationToken).ConfigureAwait(false);
        var libraryResiduals = await LibraryResidualAttributionReader.ReadAsync(
            request.Workspace, libraries, recovery, cancellationToken).ConfigureAwait(false);
        var maintenance = await maintenanceRead.ConfigureAwait(false);
        routes = DoctorMaintenanceRouteProjection.Read(routes, maintenance, sharing.Sharing);
        cancellationToken.ThrowIfCancellationRequested();
        var resultObservation = new DoctorObservation(
            workspace,
            recovery,
            routes,
            references,
            framework,
            extensions,
            libraries,
            libraryResiduals)
        {
            Maintenance = maintenance,
            LayoutRead = observation,
            OutsideRoutes = outside,
            ToolBackups = ToolBackupReader.Read(request.Workspace, ToolBackupPaths.WorkspaceTargets, cancellationToken),
        };
        var result = DoctorToolFindingAppender.Append(DoctorResultBuilder.Build(request, resultObservation),
            await _missingTools.ReadAsync(request.Workspace, new(observation.Settings, observation.Ownership), cancellationToken).ConfigureAwait(false));
        return new DoctorDiagnosisRead(resultObservation, result);
    }
}
