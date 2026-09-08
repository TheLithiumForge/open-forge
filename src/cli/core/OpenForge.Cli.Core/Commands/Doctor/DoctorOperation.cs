using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorOperation(OperationalContributorCatalogue contributors)
{
    private readonly DoctorDiagnosisReader _reader = new(contributors);

    internal async ValueTask<DoctorResult> ExecuteAsync(
        DoctorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var diagnosis = await _reader.ReadAsync(request, cancellationToken)
                .ConfigureAwait(false);
            return diagnosis.Result;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return DoctorResultBuilder.Event(
                request.Workspace,
                CliSemanticStatus.Interrupted,
                kind: null,
                "Doctor inspection was interrupted.");
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return DoctorResultBuilder.Event(
                request.Workspace,
                CliSemanticStatus.Failed,
                kind: null,
                $"Doctor inspection failed: {exception.GetType().Name}.");
        }
    }
}
