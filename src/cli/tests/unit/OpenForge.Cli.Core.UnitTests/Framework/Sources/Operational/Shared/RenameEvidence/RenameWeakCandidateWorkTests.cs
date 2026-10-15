using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Sources.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Models.References;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence;
using OpenForge.Cli.Core.Framework.Sources.Operational.Shared.RenameEvidence.Models.Observations;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Operational.Shared.RenameEvidence;

[Trait("Feature", "rename-evidence"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class RenameWeakCandidateWorkTests
{
    [Fact(DisplayName = "Repeated missing-reference hints index captured candidates once instead of scanning them per problem")]
    public void CandidateSearchWorkIsBoundedByTheObservedUniverse()
    {
        var candidates = Enumerable.Range(0, 64).Select(index => RenameEvidenceTestFacts.Candidate($".agents/guidance/target-{index}.md")).ToImmutableArray();
        var observation = LocalReferenceCandidateObservation.Create(".agents/guidance/source.md", "missing.md",
            new SourceLinkIdentity { Id = null, Path = candidates[^1].Path },
            [LocalReferenceCandidateBasis.Create(LocalReferenceCandidateBasisKind.Title, "Target", null)]);
        ImmutableArray<RenameWeakCandidate> weak = [new(RenameEvidenceTestFacts.OldPath, observation)];
        var examined = 0;
        var filter = new RenameWeakCandidateFilter(candidates, candidates, () => examined++);

        for (var index = 0; index < 50; index++)
            Assert.Equal(candidates[^1].Path, Assert.Single(filter.Read(weak, RenameEvidenceTestFacts.OldPath)).Candidate.Path);

        Assert.Equal(2 * candidates.Length, examined);
    }
}
