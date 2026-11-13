using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Shared;
using OpenForge.Cli.Core.Framework.Tools.Models.Backups;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class ToolBackupDoctorInspector
{
    internal static DoctorDomainReport Append(CliWorkspace selectedWorkspace, DoctorDomainReport workspace, IReadOnlyList<ToolBackupCandidate> backups)
    {
        if (backups.Count == 0) return workspace;
        var findings = workspace.Findings.ToList();
        foreach (var backup in backups) findings.Add(Create(selectedWorkspace, backup));
        var unavailable = backups.Where(backup => !backup.SafePaths).ToArray();
        return workspace with
        {
            Coverage = unavailable.Length == 0 ? workspace.Coverage : DoctorDomainSupport.Combine(workspace.Coverage, DoctorCoverageState.Incomplete),
            Findings = DoctorFindingAggregation.Order(findings),
            Counts = DoctorFindingAggregation.Count(findings),
            Actions = DoctorFindingAggregation.Actions(findings),
            Limitations = [.. workspace.Limitations, .. unavailable.Select(backup =>
                DoctorDomainSupport.Limitation(DoctorCoverageState.Incomplete, backup.Cause ?? "The Tool backup path is unavailable."))],
        };
    }

    private static DoctorFinding Create(CliWorkspace workspace, ToolBackupCandidate backup)
    {
        var action = new DoctorNextAction
        {
            Kind = DoctorNextActionKind.RepairPreview,
            Operation = DoctorNextOperation.Repair,
            Command = CliCommandText.InWorkspace(CommandLines.RepairAutomaticPreview, workspace.LexicalRoot),
            Reason = "Preview restoration of the sibling backup before changing its target.",
        };
        var descriptor = backup.SafePaths
            ? DoctorDomainSupport.Warning(DoctorFindingKind.ToolBackupPresent, "A sibling backup is present at a Tool control path.", DoctorResolutionLane.TargetedOperation, action)
            : DoctorDomainSupport.Error(DoctorFindingKind.ToolBackupUnavailable, backup.Cause ?? "The Tool backup path is unavailable.");
        return DoctorDomainSupport.Create(descriptor,
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, Path.GetRelativePath(workspace.LexicalRoot, backup.Backup).Replace(Path.DirectorySeparatorChar, '/')),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.ToolBackups,
                Path.GetRelativePath(workspace.LexicalRoot, backup.Backup).Replace(Path.DirectorySeparatorChar, '/')),
            [new DoctorStateEvidence(backup.SafePaths ? DoctorObservedState.Present : DoctorObservedState.Unavailable)]);
    }
}
