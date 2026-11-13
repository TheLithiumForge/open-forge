using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Commands.Tool.Run.Models;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;
using OpenForge.Cli.Core.Framework.Settings;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class DoctorToolFindingAppender
{
    internal static DoctorResult Append(DoctorResult result, IReadOnlyList<ToolRunFailure> missing)
    {
        if (missing.Count == 0) return result;
        var domains = result.Diagnosis.Domains.Select(domain => domain.Domain == DoctorDomainKind.WorkspaceEntry ? Append(domain, missing) : domain).ToArray();
        return result with
        {
            Status = DoctorResultPolicy.ReadStatus(result.Diagnosis.Coverage, domains),
            Diagnosis = result.Diagnosis with
            {
                Domains = domains,
                Counts = DoctorFindingAggregation.Count(domains.SelectMany(domain => domain.Findings)),
                Actions = DoctorFindingAggregation.Actions(domains.SelectMany(domain => domain.Findings)),
            },
        };
    }

    private static DoctorDomainReport Append(DoctorDomainReport domain, IReadOnlyList<ToolRunFailure> missing)
    {
        var ordered = DoctorFindingAggregation.Order(domain.Findings.Concat(missing.Select(Create)));
        return domain with
        {
            Findings = ordered,
            Counts = DoctorFindingAggregation.Count(ordered),
            Actions = DoctorFindingAggregation.Actions(ordered),
        };
    }

    private static DoctorFinding Create(ToolRunFailure failure)
    {
        var action = new DoctorNextAction
        {
            Kind = DoctorNextActionKind.AcceptedOperation,
            Operation = null,
            Command = ToolContractCodes.RestoreCommand,
            Reason = failure.Cause,
        };
        return DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(DoctorFindingKind.ToolDeclaredMissing, failure.Cause, DoctorResolutionLane.TargetedOperation, action),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Tool, identifier: failure.Id),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.Tools, WorkspaceSettingsDefinitions.RelativePath),
            [new DoctorStateEvidence(DoctorObservedState.Missing)]);
    }
}
