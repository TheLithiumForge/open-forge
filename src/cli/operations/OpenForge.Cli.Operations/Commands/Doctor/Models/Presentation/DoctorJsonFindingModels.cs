namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonFinding
{
    public required string Kind { get; init; }

    public required string Severity { get; init; }

    public required string Message { get; init; }

    public required DoctorJsonSubject Subject { get; init; }

    public required DoctorJsonEvidence[] Evidence { get; init; }

    public required DoctorJsonProvenance Provenance { get; init; }

    public required string Resolution { get; init; }

    public required DoctorJsonCandidates? Candidates { get; init; }

    public required DoctorJsonProposal? Proposal { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }
}

internal sealed class DoctorJsonSubject
{
    [System.Text.Json.Serialization.JsonIgnore]
    public DoctorJsonLibrarySubject? Library { get; init; }

    public required string Kind { get; init; }

    public required string? Path { get; init; }

    public required string? Id { get; init; }

    public required DoctorJsonLocation? Location { get; init; }
}

internal sealed class DoctorJsonEvidence
{
    public required string Kind { get; init; }

    public required string? Basis { get; init; }

    public required string? State { get; init; }

    public required string? Expected { get; init; }

    public required string? Actual { get; init; }

    public required string? Value { get; init; }

    public required string? Path { get; init; }

    public required DoctorJsonLocation? Location { get; init; }
}

internal sealed class DoctorJsonProvenance
{
    public required string Domain { get; init; }

    public required string Source { get; init; }

    public required string? Path { get; init; }

    public required DoctorJsonLocation? Location { get; init; }
}
