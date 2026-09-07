using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairBoundaryDiagnosisReader(DoctorDiagnosisReader diagnosisReader)
{
    internal async ValueTask<RepairPostDiagnosis> ReadAsync(RepairRequest request)
    {
        try
        {
            var read = await diagnosisReader.ReadAsync(new DoctorRequest(request.Workspace), CancellationToken.None)
                .ConfigureAwait(false);
            var coverage = RepairCoverageMapper.Read(read.Observation);
            return new RepairPostDiagnosis(RepairCoverageMapper.ReadPostDiagnosisState(coverage), coverage, RepairRemainingFindingReader.Read(read.Observation.LocalReferences));
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new RepairPostDiagnosis(RepairPostDiagnosisState.Failed, RepairPostDiagnosis.NotRequested.Coverage,
                [new RepairFinding(RepairFindingCode.OperationFailed, "Fresh diagnosis at the handled Repair boundary failed.")]);
        }
    }
}
