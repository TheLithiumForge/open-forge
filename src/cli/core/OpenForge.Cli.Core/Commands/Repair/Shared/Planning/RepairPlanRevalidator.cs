using OpenForge.Cli.Core.Commands.Doctor;
using OpenForge.Cli.Core.Commands.Doctor.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Planning;
using OpenForge.Cli.Core.Commands.Repair.Models.Request;
using OpenForge.Cli.Core.Commands.Repair.Models.Result;
using OpenForge.Cli.Core.Commands.Repair.Shared.Diagnosis;

namespace OpenForge.Cli.Core.Commands.Repair.Shared.Planning;

internal sealed class RepairPlanRevalidator(
    DoctorDiagnosisReader diagnosisReader,
    RepairCatalogueReader catalogueReader)
{
    internal async ValueTask<bool> ValidateAsync(RepairPlan plan, CancellationToken cancellationToken)
    {
        var diagnosis = await diagnosisReader.ReadAsync(new DoctorRequest(plan.Request.Workspace), cancellationToken)
            .ConfigureAwait(false);
        if (RepairCoverageMapper.Read(diagnosis.Observation).SelectedScope != RepairCoverageState.Complete)
        {
            return false;
        }

        var catalogue = await catalogueReader.ReadAsync(plan.Request.Workspace, diagnosis.Observation.LocalReferences, cancellationToken, plan.Request.Relinks)
            .ConfigureAwait(false);
        var choices = plan.Selection.Selected.Select(value => new RepairRelinkRequest(
            new RepairSourceLocation(value.Proposal.SourceCanonicalPath, value.Proposal.Occurrence.Line, value.Proposal.Occurrence.Column),
            value.Proposal.ExpectedDestination, value.Resolution.Target)).ToArray();
        var current = plan.Request.SelectionMode == RepairSelectionMode.InteractiveWizard
            ? RepairPlanner.Build(plan.Request, catalogue.Proposals, choices)
            : RepairPlanner.Build(plan.Request, catalogue.Proposals);
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
