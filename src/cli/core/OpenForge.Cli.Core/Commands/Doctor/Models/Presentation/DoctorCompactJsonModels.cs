namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorCompactJsonResult
{
    public required bool ReadOnly { get; init; }

    public required bool ChangesMade { get; init; }

    public required string Coverage { get; init; }

    public required DoctorJsonCounts Counts { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }

    public required DoctorCompactJsonDomain[] Domains { get; init; }
    public required DoctorCompactJsonCandidateSet[] CandidateSets { get; init; }
}

internal sealed class DoctorCompactJsonDomain
{
    [System.Text.Json.Serialization.JsonIgnore]
    public required DoctorJsonLibrary? Libraries { get; init; }

    public required string Domain { get; init; }

    public required DoctorJsonBoundary Boundary { get; init; }

    public required string Coverage { get; init; }

    public required string? Lifecycle { get; init; }

    public required string? SourceAvailability { get; init; }

    public required DoctorJsonLimitation[] Limitations { get; init; }

    public required DoctorJsonCounts Counts { get; init; }

    public required DoctorCompactJsonFinding[] Findings { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }
}

internal sealed class DoctorCompactJsonFinding
{
    public required string Kind { get; init; }

    public required string Severity { get; init; }

    public required string Message { get; init; }

    public required DoctorJsonSubject Subject { get; init; }

    public required DoctorJsonEvidence[] Evidence { get; init; }


    public required string Resolution { get; init; }

    public required int? CandidateSet { get; init; }

    public required DoctorJsonProposal? Proposal { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }
}

internal sealed class DoctorCompactJsonCandidateSet
{
    public required int Id { get; init; }
    public required string Cardinality { get; init; }
    public required DoctorJsonCandidate[] Items { get; init; }
}
