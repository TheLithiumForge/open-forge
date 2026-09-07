using OpenForge.Cli.Core.Commands.Repair.Shared.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Shared.Interaction;
using OpenForge.Cli.Core.Commands.Repair.Models.Application;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Application;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Commands.Repair.Shared.Result;

namespace OpenForge.Cli.Core.Commands.Repair;

internal sealed class RepairOperation
{
    private readonly RepairOperationComponents _components;

    internal RepairOperation()
        : this(RepairOperationFactory.CreateDefaultComponents())
    {
    }

    internal RepairOperation(RepairOperationComponents components)
    {
        ArgumentNullException.ThrowIfNull(components);
        _components = components;
    }

    internal async ValueTask<RepairResult> ExecuteAsync(
        RepairRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        if (!request.HasSelectionAuthority)
        {
            return RepairResultBuilder.SelectionRequired(request);
        }

        IReadOnlyList<RepairFinding> initialFindings = [];
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var diagnosis = await _components.DiagnosisReader.ReadAsync(
                new DoctorRequest(request.Workspace),
                cancellationToken).ConfigureAwait(false);
            var coverage = RepairCoverageMapper.Read(diagnosis.Observation);
            initialFindings = [.. RepairRemainingFindingReader.Read(diagnosis.Observation.LocalReferences)];
            var catalogue = await _components.CatalogueReader.ReadAsync(
                request.Workspace,
                diagnosis.Observation.LocalReferences,
                cancellationToken, request.Relinks).ConfigureAwait(false);
            RepairPlan plan;
            var wizard = new RepairWizard(_components.InteractiveSession);
            if (request.SelectionMode == RepairSelectionMode.InteractiveWizard)
            {
                if (!_components.InteractiveSession.CanPrompt)
                {
                    return RepairResultBuilder.SelectionRequired(request);
                }

                var selection = await wizard.SelectAsync([.. catalogue.Proposals.Select(value => value.Proposal).OrderByDescending(value => value.IsSafeExact)], cancellationToken)
                    .ConfigureAwait(false);
                if (selection.Cancelled)
                {
                    return await BoundaryAsync(request, new RepairFinding(RepairFindingCode.Interrupted,
                        "Repair was interrupted before application."), initialFindings).ConfigureAwait(false);
                }

                plan = RepairPlanner.Build(request, catalogue.Proposals, selection.Relinks);
            }
            else
            {
                plan = RepairPlanner.Build(request, catalogue.Proposals);
            }
            var findings = catalogue.Findings
                .Concat(ReadCoverageFindings(coverage))
                .Concat(plan.Conflicts.Select(conflict =>
                    RepairResultBuilder.PlanConflict(conflict.Cause)))
                .ToArray();
            var outcome = BeforeApplication();
            if (!plan.IsBlocked && coverage.SelectedScope == RepairCoverageState.Complete
                && !catalogue.Findings.Any(finding => finding.Code == RepairFindingCode.DiagnosisIncomplete))
            {
                var preflight = await _components.Application.PreflightAsync(plan, cancellationToken).ConfigureAwait(false);
                outcome = preflight.Outcome;
                if (outcome.Preflight.State == RepairPreflightState.Ready && request.Mode == RepairMode.Apply)
                {
                    if (request.SelectionMode == RepairSelectionMode.InteractiveWizard && plan.Effects.Count != 0
                        && !await wizard.ConfirmAsync(plan, cancellationToken).ConfigureAwait(false))
                    {
                        return await BoundaryAsync(request, new RepairFinding(RepairFindingCode.Interrupted,
                        "Repair was interrupted before application."), initialFindings).ConfigureAwait(false);
                    }

                    outcome = await _components.Application.ExecuteAsync(plan, preflight.Targets, cancellationToken).ConfigureAwait(false);
                }
            }
            if (request.Mode == RepairMode.Apply && outcome.PostDiagnosis.State == RepairPostDiagnosisState.NotRequested)
            {
                outcome = outcome with
                {
                    PostDiagnosis = await new RepairBoundaryDiagnosisReader(_components.DiagnosisReader).ReadAsync(request)
                        .ConfigureAwait(false),
                };
            }

            return RepairResultBuilder.Build(new RepairResultInput
            {
                Request = request,
                Diagnosis = coverage,
                Plan = plan,
                Findings = [.. findings, .. outcome.Findings],
                InitialFindings = initialFindings,
                Preflight = outcome.Preflight,
                Application = outcome.Application,
                Verification = outcome.Verification,
                Recovery = outcome.Recovery,
                PostDiagnosis = outcome.PostDiagnosis,
            });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return await BoundaryAsync(request, new RepairFinding(RepairFindingCode.Interrupted, "Repair was interrupted."), initialFindings)
                .ConfigureAwait(false);
        }
        catch (Exception exception) when (exception is not OutOfMemoryException)
        {
            return await BoundaryAsync(request, new RepairFinding(RepairFindingCode.OperationFailed,
                $"Repair failed unexpectedly: {exception.GetType().Name}."), initialFindings).ConfigureAwait(false);
        }
    }

    private async ValueTask<RepairResult> BoundaryAsync(
        RepairRequest request, RepairFinding finding, IReadOnlyList<RepairFinding> initialFindings)
    {
        var diagnosis = await new RepairBoundaryDiagnosisReader(_components.DiagnosisReader).ReadAsync(request).ConfigureAwait(false);
        return RepairResultBuilder.Boundary(request, finding, diagnosis, initialFindings);
    }

    private static IEnumerable<RepairFinding> ReadCoverageFindings(RepairDiagnosisCoverage coverage)
    {
        if (coverage.SelectedScope == RepairCoverageState.Blocked)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisBlocked,
                "A required Repair diagnosis domain is blocked.");
        }
        else if (coverage.SelectedScope != RepairCoverageState.Complete)
        {
            yield return new RepairFinding(
                RepairFindingCode.DiagnosisIncomplete,
                "A required Repair diagnosis domain is incomplete.");
        }
    }

    private static RepairApplicationOutcome BeforeApplication()
        => new(
            RepairPreflight.NotRequested,
            RepairApplication.NotRequested,
            RepairVerification.NotRequested,
            RepairRecovery.NotRequired,
            RepairPostDiagnosis.NotRequested,
            []);
}
