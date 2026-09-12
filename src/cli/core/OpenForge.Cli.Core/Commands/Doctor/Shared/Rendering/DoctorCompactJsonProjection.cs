using OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;

namespace OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;

internal sealed class DoctorCompactJsonProjection
{
    private readonly Dictionary<DoctorCandidateSet, int> _candidateIds = new(DoctorCandidateSetComparer.Instance);
    private readonly List<DoctorCompactJsonCandidateSet> _candidateSets = [];

    internal static DoctorCompactJsonResult Create(DoctorResult result)
        => new DoctorCompactJsonProjection().Project(result);

    private DoctorCompactJsonResult Project(DoctorResult result)
    {
        var domains = result.Diagnosis.Domains.Select(Domain).ToArray();
        return new DoctorCompactJsonResult
        {
            ReadOnly = DoctorDiagnosis.ReadOnly,
            ChangesMade = DoctorDiagnosis.ChangesMade,
            Coverage = DoctorWireVocabulary.Coverage(result.Diagnosis.Coverage),
            Counts = DoctorJsonProjection.Counts(result.Diagnosis.Counts),
            Actions = result.Diagnosis.Actions.Select(DoctorJsonProjection.Action).ToArray(),
            Domains = domains,
            CandidateSets = _candidateSets.ToArray(),
        };
    }

    private DoctorCompactJsonDomain Domain(DoctorDomainReport domain)
        => new()
        {
            Libraries = domain.Libraries is { } libraries ? DoctorLibraryPresentation.Project(libraries) : null,
            Domain = DoctorWireVocabulary.Domain(domain.Domain),
            Boundary = DoctorJsonProjection.Boundary(domain.Boundary),
            Coverage = DoctorWireVocabulary.Coverage(domain.Coverage),
            Lifecycle = domain.Lifecycle is { } lifecycle ? DoctorWireVocabulary.Lifecycle(lifecycle) : null,
            SourceAvailability = domain.SourceAvailability is { } availability ? DoctorWireVocabulary.SourceAvailability(availability) : null,
            Limitations = domain.Limitations.Select(limitation => new DoctorJsonLimitation
            {
                Kind = DoctorWireVocabulary.Limitation(limitation.Kind),
                Message = limitation.Message,
            }).ToArray(),
            Counts = DoctorJsonProjection.Counts(domain.Counts),
            Findings = domain.Findings.Select(Finding).ToArray(),
            Actions = domain.Actions.Select(DoctorJsonProjection.Action).ToArray(),
        };

    private DoctorCompactJsonFinding Finding(DoctorFinding finding)
        => new()
        {
            Kind = DoctorDefinitions.ReadFindingKind(finding.Kind),
            Severity = DoctorFindingWireVocabulary.Severity(finding.Severity),
            Message = finding.Message,
            Subject = DoctorJsonFindingProjection.Subject(finding.Subject),
            Evidence = finding.Evidence.Select(DoctorJsonEvidenceProjection.Create).ToArray(),
            Resolution = DoctorFindingWireVocabulary.Resolution(finding.Resolution),
            CandidateSet = CandidateSet(finding.Candidates),
            Proposal = finding.Proposal is { } proposal ? DoctorJsonFindingProjection.Proposal(proposal) : null,
            Actions = finding.Actions.Select(DoctorJsonProjection.Action).ToArray(),
        };

    private int? CandidateSet(DoctorCandidateSet? candidates)
    {
        if (candidates is null)
        {
            return null;
        }

        if (_candidateIds.TryGetValue(candidates, out var existing))
        {
            return existing;
        }

        var id = _candidateSets.Count + 1;
        var projected = DoctorJsonFindingProjection.Candidates(candidates);
        _candidateIds.Add(candidates, id);
        _candidateSets.Add(new DoctorCompactJsonCandidateSet
        {
            Id = id,
            Cardinality = projected.Cardinality,
            Items = projected.Items,
        });
        return id;
    }
}
