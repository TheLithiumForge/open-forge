using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

internal sealed class RepairBoundaryDiagnosisReader(DoctorDiagnosisReader diagnosisReader, RepairCatalogueReader? catalogueReader = null)
{
    internal async ValueTask<RepairPostDiagnosis> ReadAsync(RepairRequest request)
    {
        try
        {
            var read = await diagnosisReader.ReadAsync(new DoctorRequest(request.Workspace), CancellationToken.None)
                .ConfigureAwait(false);
            var coverage = RepairCoverageMapper.Read(read);
            var scope = RepairAuthoredReferenceBoundary.Filter(
                read.Observation.LocalReferences.References,
                RepairAuthoredReferenceBoundary.ReadRouteDocuments(read.Observation.Routes.Sources));
            var findings = new List<RepairFinding>();
            var catalogue = catalogueReader is null ? null : await catalogueReader.ReadAsync(request.Workspace,
                read.Observation.LocalReferences, CancellationToken.None, maintenance: read.Observation.Maintenance).ConfigureAwait(false);
            findings.AddRange(catalogue is null ? RepairRemainingFindingReader.Read(scope.References) : RepairRemainingFindingReader.ReadCatalogue(catalogue));
            findings.AddRange(RepairAuthoredReferenceBoundary.IncompleteFindings(scope.IncompleteSources));
            findings.AddRange(RepairRemainingFindingReader.ReadLibrary(read.Result.Diagnosis));
            return new RepairPostDiagnosis(
                RepairCoverageMapper.ReadPostDiagnosisState(coverage),
                coverage,
                findings)
            {
                Proposals = catalogue?.Proposals.Select(input => input.Proposal).ToArray() ?? [],
                NeedsIndex = await RepairContinuationReader.NeedsIndexAsync(request.Workspace, read.Observation.Routes, CancellationToken.None).ConfigureAwait(false),
            };
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return new RepairPostDiagnosis(RepairPostDiagnosisState.Failed, RepairPostDiagnosis.NotRequested.Coverage,
                [new RepairFinding(RepairFindingCode.OperationFailed, "Fresh diagnosis at the handled Repair boundary failed.")]);
        }
    }
}
