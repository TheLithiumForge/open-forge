namespace OpenForge.Cli.Core.Commands.Doctor.Models.Presentation;

internal sealed class DoctorJsonDomain
{
    [System.Text.Json.Serialization.JsonIgnore]
    public DoctorJsonLibrary? Libraries { get; init; }

    public required string Domain { get; init; }

    public required DoctorJsonBoundary Boundary { get; init; }

    public required string Coverage { get; init; }

    public required string? Lifecycle { get; init; }

    public required string? SourceAvailability { get; init; }

    public required DoctorJsonLimitation[] Limitations { get; init; }

    public required DoctorJsonCounts Counts { get; init; }

    public required DoctorJsonFinding[] Findings { get; init; }

    public required DoctorJsonAction[] Actions { get; init; }
}

internal sealed class DoctorJsonBoundary
{
    public required string Kind { get; init; }

    public required string? Path { get; init; }
}

internal sealed class DoctorJsonLimitation
{
    public required string Kind { get; init; }

    public required string Message { get; init; }
}
