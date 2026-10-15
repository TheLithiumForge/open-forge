using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class RepairPlanRevalidator(
    DoctorDiagnosisReader diagnosisReader,
    RepairCatalogueReader catalogueReader,
    RepairMaintenancePlanner? maintenance = null)
{
    internal async ValueTask<bool> ValidateAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        var diagnosis = await diagnosisReader.ReadAsync(new DoctorRequest(plan.Request.Workspace), cancellationToken)
            .ConfigureAwait(false);
        var libraryOnly = plan.Effects.Count == 0 && !plan.LibrarySteps.IsEmpty;
        var dependencyComplete = libraryOnly
            ? diagnosis.Observation.WorkspaceEntry.State == OperationalViewState.Complete
                && diagnosis.Observation.Libraries.State == OperationalViewState.Complete
            : RepairCoverageMapper.Read(diagnosis).SelectedScope == RepairCoverageState.Complete;
        if (!dependencyComplete)
        {
            return false;
        }

        var catalogue = await catalogueReader.ReadAsync(plan.Request.Workspace, diagnosis.Observation.LocalReferences, cancellationToken,
            plan.Request.Relinks, diagnosis.Observation.Maintenance)
            .ConfigureAwait(false);
        foreach (var proof in plan.Selection.Selected.SelectMany(selected => new[] { selected.Proposal.RenameProof, selected.Proposal.SourceMoveProof }).OfType<OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations.RenameEvidenceObservation>())
            if (plan.RenameEvidence is null || catalogue.RenameEvidence is null
                || !RenameEvidenceRevalidator.IsCurrent(plan.RenameEvidence, catalogue.RenameEvidence, proof.OldPath)) return false;
        var choices = plan.Selection.Selected.Select(value => new RepairRelinkRequest(
            new RepairSourceLocation(value.Proposal.SourceCanonicalPath, value.Proposal.Occurrence.Line, value.Proposal.Occurrence.Column),
            value.Proposal.ExpectedDestination, value.Resolution.Target)).ToArray();
        var libraries = RepairLibraryCatalogueReader.Read(diagnosis.Result.Diagnosis);
        if (!plan.Selection.Libraries.Selected.IsEmpty || !plan.Selection.Libraries.Unselected.IsEmpty || !libraries.IsEmpty)
        {
            if (!RepairLibraryRecoveryPlanner.Revalidate(plan, new RepairLibraryPlanningInput
            {
                Request = plan.Request,
                References = [.. catalogue.Proposals],
                Libraries = libraries,
                PromptRelinks = [.. choices],
                PromptLibraries = plan.Request.SelectionMode == RepairSelectionMode.InteractivePrompt
                    ? [.. plan.Selection.Libraries.Selected.Select(selected => selected.Proposal)]
                    : null,
            })) return false;
        }

        var current = plan.Request.SelectionMode == RepairSelectionMode.InteractivePrompt
            ? RepairPlanner.Build(plan.Request, catalogue.Proposals, choices)
            : RepairPlanner.Build(plan.Request, catalogue.Proposals);
        if (maintenance is not null)
            current = await maintenance.CompleteAsync(current, catalogue, cancellationToken).ConfigureAwait(false);
        if (plan.Maintenance is not null)
        {
            if (current.Maintenance is null || maintenance is null || !await maintenance.VerifyAsync(plan, cancellationToken).ConfigureAwait(false)
                || !plan.Maintenance.PolicyExpectations.SequenceEqual(current.Maintenance.PolicyExpectations)
                || plan.FileChanges.Count != current.FileChanges.Count
                || !plan.FileChanges.Zip(current.FileChanges).All(pair => pair.First.Kind == pair.Second.Kind && pair.First.Expectation == pair.Second.Expectation
                    && pair.First.IntendedBytes.AsSpan().SequenceEqual(pair.Second.IntendedBytes.AsSpan()))) return false;
        }
        return !current.IsBlocked
            && current.Selection.Selected.Count == plan.Selection.Selected.Count
            && current.Effects.Count == plan.Effects.Count
            && current.NoOps.Count == plan.NoOps.Count
            && current.Effects.Zip(plan.Effects).All(pair =>
                pair.First.ExpectedState.Expectation == pair.Second.ExpectedState.Expectation
                && pair.First.IntendedState.Expectation == pair.Second.IntendedState.Expectation)
            && current.Selection.Selected.Zip(plan.Selection.Selected).All(pair =>
                pair.First.Proposal.SourceCanonicalPath == pair.Second.Proposal.SourceCanonicalPath
                && pair.First.Proposal.Occurrence == pair.Second.Proposal.Occurrence
                && pair.First.Proposal.ExpectedDestination == pair.Second.Proposal.ExpectedDestination
                && pair.First.Resolution.IntendedDestination == pair.Second.Resolution.IntendedDestination
                && pair.First.Resolution.Target.CanonicalTargetPath == pair.Second.Resolution.Target.CanonicalTargetPath
                && pair.First.Resolution.Target.TargetFragment == pair.Second.Resolution.Target.TargetFragment);
    }
}
