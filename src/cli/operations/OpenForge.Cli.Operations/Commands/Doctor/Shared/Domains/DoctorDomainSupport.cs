using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;

internal static class DoctorDomainSupport
{
    internal static DoctorCoverageState Coverage(OperationalViewState state)
        => state switch
        {
            OperationalViewState.Complete => DoctorCoverageState.Complete,
            OperationalViewState.Incomplete or OperationalViewState.Interrupted => DoctorCoverageState.Incomplete,
            OperationalViewState.Blocked => DoctorCoverageState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state), state, "The operational view state is not defined."),
        };

    internal static DoctorCoverageState Combine(
        DoctorCoverageState first,
        DoctorCoverageState second)
        => (first, second) switch
        {
            (DoctorCoverageState.Complete, DoctorCoverageState.Complete) => DoctorCoverageState.Complete,
            (DoctorCoverageState.Complete, DoctorCoverageState.Incomplete) => DoctorCoverageState.Incomplete,
            (DoctorCoverageState.Complete, DoctorCoverageState.Blocked) => DoctorCoverageState.Blocked,
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Complete) => DoctorCoverageState.Incomplete,
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Incomplete) => DoctorCoverageState.Incomplete,
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Blocked) => DoctorCoverageState.Blocked,
            (DoctorCoverageState.Blocked, DoctorCoverageState.Complete) => DoctorCoverageState.Blocked,
            (DoctorCoverageState.Blocked, DoctorCoverageState.Incomplete) => DoctorCoverageState.Blocked,
            (DoctorCoverageState.Blocked, DoctorCoverageState.Blocked) => DoctorCoverageState.Blocked,
            _ => throw new ArgumentOutOfRangeException(
                nameof(first),
                (first, second),
                "A Doctor coverage state is not defined."),
        };

    internal static DoctorLimitation Limitation(DoctorCoverageState coverage, string message)
        => new()
        {
            Kind = coverage switch
            {
                DoctorCoverageState.Complete => DoctorLimitationKind.Unavailable,
                DoctorCoverageState.Incomplete => DoctorLimitationKind.Incomplete,
                DoctorCoverageState.Blocked => DoctorLimitationKind.Blocked,
                _ => throw new ArgumentOutOfRangeException(nameof(coverage), coverage, "The Doctor coverage state is not defined."),
            },
            Message = message,
        };

    internal static DoctorFinding Create(
        DoctorFindingDescriptor descriptor,
        DoctorSubject subject,
        DoctorProvenance provenance,
        IReadOnlyList<DoctorEvidence> evidence)
    {
        ArgumentNullException.ThrowIfNull(evidence);
        if (evidence.Count == 0)
        {
            throw new ArgumentException(
                "Every Doctor finding requires typed evidence.",
                nameof(evidence));
        }

        return new DoctorFinding
        {
            Kind = descriptor.Kind,
            Severity = descriptor.Severity,
            Message = descriptor.Message,
            Subject = subject,
            Evidence = evidence,
            Provenance = provenance,
            Resolution = descriptor.Resolution,
            Candidates = null,
            Proposal = null,
            Actions = descriptor.Action is null ? [] : [descriptor.Action],
        };
    }

    internal static DoctorFinding CreateWithProposal(
        DoctorFindingDescriptor descriptor,
        DoctorSubject subject,
        DoctorProvenance provenance,
        IReadOnlyList<DoctorEvidence> evidence,
        DoctorExactProposal proposal)
        => Create(descriptor, subject, provenance, evidence) with
        {
            Proposal = proposal,
        };

    internal static DoctorSubject Subject(
        DoctorSubjectKind kind,
        string? path = null,
        string? identifier = null)
        => new()
        {
            Kind = kind,
            Path = path,
            Identifier = identifier,
            Location = null,
        };

    internal static DoctorProvenance Provenance(
        DoctorDomainKind domain,
        DoctorProvenanceSource source,
        string? path = null)
        => new()
        {
            Domain = domain,
            Source = source,
            Path = path,
            Location = null,
        };

    internal static DoctorFindingDescriptor Information(
        DoctorFindingKind kind,
        string message,
        DoctorNextAction? action = null)
        => new(kind, DoctorFindingSeverity.Information, message, DoctorResolutionLane.Informational, action);

    internal static DoctorFindingDescriptor Information(
        DoctorFindingKind kind,
        string message,
        DoctorResolutionLane resolution,
        DoctorNextAction? action = null)
        => new(kind, DoctorFindingSeverity.Information, message, resolution, action);

    internal static DoctorFindingDescriptor Warning(
        DoctorFindingKind kind,
        string message,
        DoctorResolutionLane resolution,
        DoctorNextAction? action = null)
        => new(kind, DoctorFindingSeverity.Warning, message, resolution, action);

    internal static DoctorFindingDescriptor Error(
        DoctorFindingKind kind,
        string message,
        DoctorResolutionLane resolution = DoctorResolutionLane.BlockedRepair,
        DoctorNextAction? action = null)
        => new(kind, DoctorFindingSeverity.Error, message, resolution, action);
}

internal sealed record DoctorFindingDescriptor(
    DoctorFindingKind Kind,
    DoctorFindingSeverity Severity,
    string Message,
    DoctorResolutionLane Resolution,
    DoctorNextAction? Action);
