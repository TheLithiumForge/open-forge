using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal static class RenameWeakCandidateFilter
{
    internal static ImmutableArray<LocalReferenceCandidateObservation> Read(
        ImmutableArray<RenameWeakCandidate> weak,
        ImmutableArray<RenameCurrentCandidate> candidates,
        ImmutableArray<RenameCurrentCandidate> aliases,
        string oldPath)
    {
        var admitted = ImmutableArray.CreateBuilder<LocalReferenceCandidateObservation>();
        foreach (var observation in weak.Where(weak => weak.OldPath == oldPath).Select(weak => weak.Candidate))
        {
            if (!observation.Bases.Any(basis => basis.Kind != LocalReferenceCandidateBasisKind.RouteNeighborhood)) continue;
            var alias = aliases.FirstOrDefault(candidate => candidate.Path == observation.Candidate.Path);
            if (alias is null) continue;
            var candidate = candidates.First(current => PhysicalIdentityTracker.PathComparer.Equals(current.Snapshot.PhysicalPath, alias.Snapshot.PhysicalPath));
            var sourcePath = observation.SourcePath;
            if (SourceOverwritePath.HasSuffix(sourcePath)) sourcePath = SourceOverwritePath.ReadBasePath(sourcePath);
            var source = aliases.FirstOrDefault(current => current.Path == sourcePath);
            if (sourcePath == candidate.Path || sourcePath == alias.Path
                || source is not null && PhysicalIdentityTracker.PathComparer.Equals(source.Snapshot.PhysicalPath, candidate.Snapshot.PhysicalPath)) continue;
            admitted.Add(LocalReferenceCandidateObservation.Create(observation.SourcePath, observation.Destination,
                new SourceLinkIdentity { Id = observation.Candidate.Id, Path = candidate.Path }, observation.Bases));
        }

        return admitted.DistinctBy(observation => (observation.SourcePath, observation.Destination, observation.Candidate.Path))
            .OrderBy(observation => observation.Candidate.Path, StringComparer.Ordinal).ToImmutableArray();
    }
}
