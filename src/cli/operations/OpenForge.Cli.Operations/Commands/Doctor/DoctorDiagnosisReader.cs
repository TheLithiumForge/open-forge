using OpenForge.Cli.Core.Framework.Libraries.Operational;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorDiagnosisReader(OperationalContributorCatalogue contributors)
{
    private readonly OperationalContributorCatalogue _contributors = contributors;

    internal async ValueTask<DoctorDiagnosisRead> ReadAsync(
        DoctorRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();
        var workspace = await _contributors.WorkspaceEntry
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var recovery = await _contributors.RecoveryResiduals
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var routes = await _contributors.Routes
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var references = await _contributors.LocalReferences
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var framework = await _contributors.FrameworkLifecycle
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var extensions = await _contributors.ExtensionLifecycle
            .ReadDoctorAsync(request.Workspace, cancellationToken)
            .ConfigureAwait(false);
        var libraries = await _contributors.Libraries.ReadDoctorAsync(request.Workspace, cancellationToken).ConfigureAwait(false);
        var libraryResiduals = await LibraryResidualAttributionReader.ReadAsync(
            request.Workspace, libraries, recovery, cancellationToken).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();
        var observation = new DoctorObservation(
            workspace,
            recovery,
            routes,
            references,
            framework,
            extensions,
            libraries,
            libraryResiduals);
        var result = DoctorResultBuilder.Build(request, observation);
        return new DoctorDiagnosisRead(observation, result);
    }
}
