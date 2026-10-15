using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;

internal sealed class RenameWeakCandidateFilter
{
    // Index immutable evidence already acquired by the reader; this never resolves or reads a filesystem path.
    private readonly Dictionary<string, RenameCurrentCandidate> _aliases = new(StringComparer.Ordinal);
    private readonly HashSet<string> _paths = new(StringComparer.Ordinal);
    private readonly ILookup<string?, RenameCurrentCandidate> _physicalCandidates;

    internal RenameWeakCandidateFilter(ImmutableArray<RenameCurrentCandidate> candidates,
        ImmutableArray<RenameCurrentCandidate> aliases, Action? candidateExamined = null)
    {
        _physicalCandidates = candidates.ToLookup(candidate =>
        {
            candidateExamined?.Invoke();
            _paths.Add(candidate.Path);
            return candidate.Snapshot.PhysicalPath;
        }, PhysicalIdentityTracker.PathComparer);
        foreach (var alias in aliases)
        {
            candidateExamined?.Invoke();
            _aliases.TryAdd(alias.Path, alias);
        }
    }

    internal bool ContainsCandidate(string path) => _paths.Contains(path);

    internal ImmutableArray<LocalReferenceCandidateObservation> Read(ImmutableArray<RenameWeakCandidate> weak, string oldPath)
    {
        var admitted = ImmutableArray.CreateBuilder<LocalReferenceCandidateObservation>();
        foreach (var observation in weak.Where(weak => weak.OldPath == oldPath).Select(weak => weak.Candidate))
        {
            if (!observation.Bases.Any(basis => basis.Kind != LocalReferenceCandidateBasisKind.RouteNeighborhood)) continue;
            if (!_aliases.TryGetValue(observation.Candidate.Path, out var alias)) continue;
            var candidate = _physicalCandidates[alias.Snapshot.PhysicalPath].First();
            var sourcePath = observation.SourcePath;
            if (SourceOverwritePath.HasSuffix(sourcePath)) sourcePath = SourceOverwritePath.ReadBasePath(sourcePath);
            _aliases.TryGetValue(sourcePath, out var source);
            if (sourcePath == candidate.Path || sourcePath == alias.Path
                || source is not null && PhysicalIdentityTracker.PathComparer.Equals(source.Snapshot.PhysicalPath, candidate.Snapshot.PhysicalPath)) continue;
            admitted.Add(LocalReferenceCandidateObservation.Create(observation.SourcePath, observation.Destination,
                new SourceLinkIdentity { Id = observation.Candidate.Id, Path = candidate.Path }, observation.Bases));
        }

        return admitted.DistinctBy(observation => (observation.SourcePath, observation.Destination, observation.Candidate.Path))
            .OrderBy(observation => observation.Candidate.Path, StringComparer.Ordinal).ToImmutableArray();
    }
}
