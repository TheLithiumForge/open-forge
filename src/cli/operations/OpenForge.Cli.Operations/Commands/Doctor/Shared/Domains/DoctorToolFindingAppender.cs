using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Aggregation;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Tools.Shared.Observation.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class DoctorToolFindingAppender
{
    internal static DoctorResult Append(DoctorResult result, IReadOnlyList<ToolObservation> tools, string? failure = null)
    {
        var added = tools.Where(tool => tool.State != ToolObservationState.Current).Select(Create).ToList();
        if (failure is not null) added.Add(Finding(DoctorFindingKind.ToolObservationUnavailable, null, failure, DoctorObservedState.Unavailable, "open-forge tool status"));
        if (added.Count == 0) return result;
        var domains = result.Diagnosis.Domains.Select(domain =>
        {
            if (domain.Domain != DoctorDomainKind.WorkspaceEntry) return domain;
            var findings = DoctorFindingAggregation.Order(domain.Findings.Concat(added));
            return domain with
            {
                Coverage = added.Any(finding => finding.Kind == DoctorFindingKind.ToolObservationUnavailable) ? DoctorCoverageState.Incomplete : domain.Coverage,
                Findings = findings,
                Counts = DoctorFindingAggregation.Count(findings),
                Actions = DoctorFindingAggregation.Actions(findings),
            };
        }).ToArray();
        var coverage = domains.Max(domain => domain.Coverage);
        return result with
        {
            Status = DoctorResultPolicy.ReadStatus(coverage, domains),
            Diagnosis = result.Diagnosis with
            {
                Coverage = coverage,
                Domains = domains,
                Counts = DoctorFindingAggregation.Count(domains.SelectMany(domain => domain.Findings)),
                Actions = DoctorFindingAggregation.Actions(domains.SelectMany(domain => domain.Findings)),
            },
        };
    }

    private static DoctorFinding Create(ToolObservation tool)
    {
        var (kind, state) = tool.State switch
        {
            ToolObservationState.Missing => (DoctorFindingKind.ToolDeclaredMissing, DoctorObservedState.Missing),
            ToolObservationState.Changed => (DoctorFindingKind.ToolDrift, DoctorObservedState.Changed),
            ToolObservationState.Unsupported => (DoctorFindingKind.ToolUnsupported, DoctorObservedState.Unsupported),
            ToolObservationState.Unavailable => (DoctorFindingKind.ToolObservationUnavailable, DoctorObservedState.Unavailable),
            _ => throw new ArgumentOutOfRangeException(nameof(tool)),
        };
        var command = tool.State switch
        {
            ToolObservationState.Missing => "open-forge tool install",
            ToolObservationState.Changed => $"open-forge tool install {tool.Entry.Identity.Id}",
            _ => $"open-forge tool inspect {tool.Entry.Identity.Id}",
        };
        return Finding(kind, tool.Entry.Identity.Id,
            $"{tool.Entry.Identity.Name}: {tool.State}. Locked version: {tool.Locked?.Version ?? "missing"}. Detected version: {tool.Detection.Version ?? "unavailable"}.", state, command);
    }

    private static DoctorFinding Finding(DoctorFindingKind kind, string? id, string cause, DoctorObservedState state, string command)
        => DoctorDomainSupport.Create(
            DoctorDomainSupport.Warning(kind, cause, DoctorResolutionLane.TargetedOperation,
                new DoctorNextAction { Kind = DoctorNextActionKind.AcceptedOperation, Operation = null, Command = command, Reason = cause }),
            DoctorDomainSupport.Subject(DoctorSubjectKind.Tool, identifier: id),
            DoctorDomainSupport.Provenance(DoctorDomainKind.WorkspaceEntry, DoctorProvenanceSource.WorkspaceEntry, WorkspaceSettingsDefinitions.RelativePath),
            [new DoctorStateEvidence(state)]);
}
