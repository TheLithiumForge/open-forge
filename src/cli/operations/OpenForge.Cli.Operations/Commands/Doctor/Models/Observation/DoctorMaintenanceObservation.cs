using System.Collections.Immutable;
using OpenForge.Cli.Core.Commands.Shared.RoutedMaintenance.Metadata.Models;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;
using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Models.Locations;

namespace OpenForge.Cli.Core.Commands.Doctor.Models.Observation;

internal sealed record DoctorMaintenanceObservation
{
    public required ImmutableDictionary<string, RoutedMetadataEligibility> Eligibility { get; init; }
    public required ImmutableHashSet<string> IndexablePaths { get; init; }
    public required ImmutableHashSet<string> ChainGapPaths { get; init; }
    public required ImmutableDictionary<string, string> OldNavigationPaths { get; init; }
    public ImmutableHashSet<string> ExistingCanonicalNavigationPaths { get; init; } = [];
    public required RenameEvidenceResult? Renames { get; init; }
    public RoutedMetadataPolicyContext? ReferenceProtectionPolicy { get; init; }
    public ImmutableDictionary<string, DoctorNextAction> ManagingUpdates { get; init; } = ImmutableDictionary<string, DoctorNextAction>.Empty;
    public ImmutableArray<DoctorSourceMoveObservation> SourceMoves { get; init; } = [];
    public ImmutableDictionary<string, string> MalformedMetadata { get; init; } = ImmutableDictionary<string, string>.Empty;
    public ImmutableDictionary<string, SourceLocation> GeneratedEntries { get; init; } = ImmutableDictionary<string, SourceLocation>.Empty;
}

internal sealed record DoctorSourceMoveObservation(LocalReferenceObservation Reference, RenameSourceMoveTarget Target);
