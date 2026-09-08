namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonCandidates
{
    public required string Cardinality { get; init; }

    public required DoctorJsonCandidate[] Items { get; init; }
}

internal sealed class DoctorJsonCandidate
{
    public required DoctorJsonSubject Subject { get; init; }

    public required DoctorJsonCandidateBasis[] Evidence { get; init; }

    public required DoctorJsonProvenance Provenance { get; init; }
}

internal sealed class DoctorJsonCandidateBasis
{
    public required string Kind { get; init; }

    public required string? Value { get; init; }

    public required DoctorJsonLocation? Location { get; init; }
}

internal sealed class DoctorJsonProposal
{
    public required string Kind { get; init; }

    public required DoctorJsonSubject Subject { get; init; }

    public required string? Expected { get; init; }

    public required string? Intended { get; init; }

    public required DoctorJsonBoundary Boundary { get; init; }

    public required string Verification { get; init; }

    public required string Recovery { get; init; }

    [System.Text.Json.Serialization.JsonIgnore]
    public required DoctorJsonLibraryRecovery? LibraryRecovery { get; init; }
}
