using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors;

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
        cancellationToken.ThrowIfCancellationRequested();
        var observation = new DoctorObservation(
            workspace,
            recovery,
            routes,
            references,
            framework,
            extensions);
        var result = new DoctorResultBuilder().Build(request, observation);
        return new DoctorDiagnosisRead(observation, result);
    }
}
