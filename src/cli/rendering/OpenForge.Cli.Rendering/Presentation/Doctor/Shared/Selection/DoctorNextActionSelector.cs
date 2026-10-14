using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Presentation.Doctor.Shared.Wording;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Operation;

namespace OpenForge.Cli.Core.Presentation.Doctor.Shared.Selection;

internal static class DoctorNextActionSelector
{
    internal static CliNextAction? Select(CliSemanticStatus status, IReadOnlyList<DoctorFinding> findings)
    {
        if (status is CliSemanticStatus.Invalid or CliSemanticStatus.Blocked or CliSemanticStatus.Failed or CliSemanticStatus.Interrupted) return null;
        if (!findings.Any(finding => finding.Severity is DoctorFindingSeverity.Warning or DoctorFindingSeverity.Error)) return null;
        var ordered = findings.OrderBy(finding => finding.Provenance.Domain).ThenBy(finding => finding.Kind)
            .ThenBy(finding => finding.Subject.Path, StringComparer.Ordinal).ThenBy(finding => finding.Subject.LocationView?.Line).ToArray();
        var boundary = ordered.FirstOrDefault(finding => finding.Resolution == DoctorResolutionLane.BlockedRepair
            || finding.Maintenance?.Condition == DoctorMaintenanceCondition.RestorationBoundary
            || finding.Provenance.Domain == DoctorDomainKind.RecoveryResiduals && DoctorWording.Actions(finding).Count > 0);
        if (boundary is not null)
            return DoctorWording.Actions(boundary).FirstOrDefault() ?? ManualBoundary(boundary);
        var identity = ordered.FirstOrDefault(finding => finding.Resolution == DoctorResolutionLane.GuidedChoice
            && finding.Maintenance?.PreserveRenameIdentity == true);
        if (identity is not null) return DoctorWording.Actions(identity).FirstOrDefault();
        if (ordered.Any(finding => finding.Resolution == DoctorResolutionLane.SafeExact))
            return new("open-forge repair --automatic", global::OpenForge.Cli.OutputText.Doctor.DoctorMaintenanceText.RepairReason());
        var index = ordered.Where(finding => finding.Resolution == DoctorResolutionLane.TargetedOperation
            && finding.Actions.Any(action => action.Operation == DoctorNextOperation.Index)).ToArray();
        var indexAction = index.SelectMany(DoctorWording.Actions).FirstOrDefault(action => action.Kind == CliNextActionKind.Command);
        if (indexAction is not null) return indexAction;
        if (index.FirstOrDefault() is { } indexFinding) return DoctorWording.Actions(indexFinding).FirstOrDefault();
        var targeted = ordered.FirstOrDefault(finding => finding.Resolution == DoctorResolutionLane.TargetedOperation);
        if (targeted is not null && DoctorWording.Actions(targeted).FirstOrDefault() is { } targetedAction) return targetedAction;
        if (ordered.Any(finding => finding.Resolution == DoctorResolutionLane.GuidedChoice))
            return new("open-forge repair", global::OpenForge.Cli.OutputText.Doctor.DoctorText.MessageReviewTheFindingsAndChooseARepair());
        var manual = ordered.FirstOrDefault(finding => finding.Resolution == DoctorResolutionLane.ManualDecision);
        if (manual is not null) return DoctorWording.Actions(manual).FirstOrDefault() ?? ManualBoundary(manual);
        return null;
    }

    private static CliNextAction ManualBoundary(DoctorFinding finding)
    {
        var sentence = global::OpenForge.Cli.OutputText.Doctor.DoctorMaintenanceText.ReviewBoundary(finding.Subject.Path ?? finding.Subject.Identifier ?? "the reported file");
        return new(sentence, sentence) { Kind = CliNextActionKind.Sentence };
    }
}
