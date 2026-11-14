using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Framework.Settings.Models.Observation;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

namespace OpenForge.Cli.Core.Commands.Doctor;

internal sealed class DoctorOperation(OperationalContributorCatalogue contributors, RenameEvidenceReader? renames = null,
    Func<CliWorkspace, CancellationToken, ValueTask<WorkspaceLayoutRead>>? readLayout = null,
    Func<ToolWorkspaceObservationReader>? tools = null)
{
    private readonly DoctorDiagnosisReader _reader = new(contributors, renames, readLayout);

    internal async ValueTask<DoctorResult> ExecuteAsync(
        DoctorRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var diagnosis = await _reader.ReadAsync(request, cancellationToken)
                .ConfigureAwait(false);
            if (tools is null) return diagnosis.Result;
            try
            {
                return DoctorToolFindingAppender.Append(diagnosis.Result, await tools().ReadAsync(request.Workspace.PhysicalRoot,
                    diagnosis.Observation.LayoutRead?.Layout?.MainRoot ?? WorkspaceLayout.DefaultMainRoot, cancellationToken).ConfigureAwait(false));
            }
            catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or ArgumentException)
            { return DoctorToolFindingAppender.Append(diagnosis.Result, [], exception.Message); }
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
