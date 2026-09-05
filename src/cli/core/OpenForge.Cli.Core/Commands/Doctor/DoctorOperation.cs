using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorOperation(OperationalContributorCatalogue contributors)
{
    private readonly OperationalContributorCatalogue _contributors = contributors;

    internal async ValueTask<DoctorResult> ExecuteAsync(
        DoctorRequest request,
        CancellationToken cancellationToken)
    {
        var resultBuilder = new DoctorResultBuilder();
        try
        {
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
            return resultBuilder.Build(
                request,
                new DoctorObservation(workspace, recovery, routes, references, framework, extensions));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return resultBuilder.Event(
                request.Workspace,
                CliSemanticStatus.Interrupted,
                kind: null,
                "Doctor inspection was interrupted.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return resultBuilder.Event(
                request.Workspace,
                CliSemanticStatus.Failed,
                kind: null,
                $"Doctor inspection failed: {exception.GetType().Name}.");
        }
    }
}
