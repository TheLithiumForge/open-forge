using OpenForge.Cli.Core.Framework.OperationalContributors.Models;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;
using OpenForge.Cli.Core.Framework.Sources.Models.References;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Models;

internal enum LocalReferenceKind
{
    Link,
    Image,
}

internal sealed record LocalReferenceObservation
{
    public required string SourcePath { get; init; }

    public required LocalReferenceKind Kind { get; init; }

    public required string Destination { get; init; }

    public required SourceLocation? Location { get; init; }

    public required SourceLinkDestinationFacts Facts { get; init; }
}

internal sealed record LocalReferenceDoctorView(
    OperationalViewState State,
    IReadOnlyList<LocalReferenceObservation> References);
