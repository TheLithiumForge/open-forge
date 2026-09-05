using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Result;

internal enum DoctorSubjectKind
{
    Workspace,
    Path,
    Route,
    GeneratedRegion,
    SourceOccurrence,
    Target,
    RecoveryItem,
    ManagedFile,
    Extension,
    Dependency,
}

internal sealed record DoctorSubject
{
    public required DoctorSubjectKind Kind { get; init; }

    public required string? Path { get; init; }

    public required string? Identifier { get; init; }

    public required SourceLocation? Location { get; init; }
}
