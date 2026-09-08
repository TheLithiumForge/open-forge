using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Inventory;
using OpenForge.Cli.Core.Framework.Libraries.Models.Observation;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
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
    Library,
    LibrarySourceRoot,
    LibraryMapping,
    LibraryProjection,
    LibraryResidual,
}

internal sealed record DoctorSubject
{
    public DoctorLibrarySubject? Library { get; init; }

    public required DoctorSubjectKind Kind { get; init; }

    public required string? Path { get; init; }

    public required string? Identifier { get; init; }

    public required SourceLocation? Location { get; init; }
}

internal sealed record DoctorLibrarySubject
{
    internal DoctorLibrarySubject(
        DoctorSubjectKind kind,
        LibraryId libraryId,
        LibraryRecord? registration,
        LibrarySourceRootObservation? source,
        LibraryMapping? mapping,
        LibraryMappingObservation? projection,
        LibraryResidualEvidence? residual)
    {
        ArgumentNullException.ThrowIfNull(libraryId);
        var present = (registration is null ? 0 : 1) + (source is null ? 0 : 1) + (mapping is null ? 0 : 1)
            + (projection is null ? 0 : 1) + (residual is null ? 0 : 1);
        var coherent = kind switch
        {
            DoctorSubjectKind.Library => registration is not null && registration.Id == libraryId,
            DoctorSubjectKind.LibrarySourceRoot => source is not null,
            DoctorSubjectKind.LibraryMapping => mapping is not null,
            DoctorSubjectKind.LibraryProjection => projection is not null,
            DoctorSubjectKind.LibraryResidual => residual is not null && residual.LibraryId == libraryId,
            _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, "A Library subject requires a defined Library subject kind."),
        };
        if (present != 1 || !coherent)
        {
            throw new ArgumentException("A Library subject requires exactly the typed fact identified by its kind.", nameof(kind));
        }

        Kind = kind;
        LibraryId = libraryId;
        Registration = registration;
        Source = source;
        Mapping = mapping;
        Projection = projection;
        Residual = residual;
    }

    internal DoctorSubjectKind Kind { get; }
    internal LibraryId LibraryId { get; }
    internal LibraryRecord? Registration { get; }
    internal LibrarySourceRootObservation? Source { get; }
    internal LibraryMapping? Mapping { get; }
    internal LibraryMappingObservation? Projection { get; }
    internal LibraryResidualEvidence? Residual { get; }
}
