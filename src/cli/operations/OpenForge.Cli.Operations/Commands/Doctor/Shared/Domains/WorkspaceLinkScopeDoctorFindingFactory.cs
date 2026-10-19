using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class WorkspaceLinkScopeDoctorFindingFactory
{
    internal static DoctorFinding Create((string Key, string Path) entry)
    {
        var action = $"Remove or update {entry.Key} entry {entry.Path} in {WorkspaceSettingsDefinitions.RelativePath}.";
        return DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.WorkspaceLinkIgnorePathMissing,
                $"{entry.Key} entry {entry.Path} no longer exists.", DoctorResolutionLane.ManualDecision,
                new DoctorNextAction
                {
                    Kind = DoctorNextActionKind.ManualDecision,
                    Operation = null,
                    Command = action,
                    Reason = action,
                }),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Path, entry.Path, entry.Key),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, WorkspaceSettingsDefinitions.RelativePath),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);
    }
}
