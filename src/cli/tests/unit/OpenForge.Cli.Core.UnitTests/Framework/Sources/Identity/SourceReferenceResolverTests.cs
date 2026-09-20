using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;
using OpenForge.Cli.Core.UnitTests.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.Core.UnitTests.Framework.Sources.Identity;

public sealed class SourceReferenceResolverTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Source references resolve IDs, exact base and overwrite paths, and collisions without guessing")]
    [Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void ReferencesResolveLogicalIdentityAndCollisionFacts()
    {
        var guide = SourceInventoryTestData.Source(
            ".agents/docs/guide.md",
            withOverwrite: true);
        var collisionFile = SourceInventoryTestData.Source(
            ".agents/collision.md",
            "collision");
        var collisionEntrypoint = SourceInventoryTestData.Source(
            ".agents/collision/_collision.md",
            "collision",
            SourceDocumentForm.CanonicalEntrypoint);
        var catalogue = Catalogue(guide, collisionFile, collisionEntrypoint);
        var physicalCalls = 0;
        var resolver = new SourceReferenceResolver((_, path) =>
        {
            physicalCalls++;
            return PhysicalPathResolution.Classified(PhysicalPathState.Missing, path);
        });

        Assert.Same(guide, resolver.Resolve("docs/guide", catalogue).Source);
        Assert.Same(guide, resolver.Resolve(".agents/docs/guide.md", catalogue).Source);
        Assert.Same(guide, resolver.Resolve("./.agents/docs/guide.overwrite.md", catalogue).Source);

        var ambiguous = resolver.Resolve("collision", catalogue);
        Assert.Equal(SourceReferenceResolutionState.Ambiguous, ambiguous.State);
        Assert.Equal(
            [".agents/collision.md", ".agents/collision/_collision.md"],
            ambiguous.Candidates.Select(candidate => candidate.Identity.CanonicalBasePath));

        var unknown = resolver.Resolve("missing", catalogue);
        Assert.Equal(SourceReferenceKind.SourceId, unknown.Form);
        Assert.Equal(SourceReferenceResolutionState.Unknown, unknown.State);
        Assert.Equal(0, physicalCalls);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Exact source paths retain invalid, unsupported, missing, and unsafe physical outcomes")]
    [Trait("Feature", "source-selection"), Trait("Evidence", "Unit")]
    public void ExactPathsRetainTypedUnresolvedOutcomes()
    {
        var catalogue = Catalogue(SourceInventoryTestData.Source(".agents/docs/guide.md"));
        var resolver = new SourceReferenceResolver((workspace, path) =>
        {
            if (path.EndsWith("unsafe.md", StringComparison.Ordinal))
            {
                var outside = Path.GetFullPath(Path.Combine(workspace.PhysicalRoot, "..", "unsafe.md"));
                return PhysicalPathResolution.Classified(PhysicalPathState.External, path, outside);
            }

            if (path.EndsWith("unsupported.md", StringComparison.Ordinal))
            {
                return PhysicalPathResolution.Contained(path, SourceInventoryTestData.Physical(path));
            }

            return PhysicalPathResolution.Classified(PhysicalPathState.Missing, path);
        });

        Assert.Equal(
            SourceReferenceResolutionState.Invalid,
            resolver.Resolve(".agents/docs/../bad.md", catalogue).State);
        Assert.Equal(
            SourceReferenceResolutionState.Unsupported,
            resolver.Resolve(".agents/docs/unsupported.md", catalogue).State);
        Assert.Equal(
            SourceReferenceResolutionState.Unknown,
            resolver.Resolve(".agents/docs/missing.md", catalogue).State);
        Assert.Equal(
            SourceReferenceResolutionState.Unsafe,
            resolver.Resolve(".agents/docs/unsafe.md", catalogue).State);
    }

    private static SourceCatalogue Catalogue(params SourceLogicalSource[] sources)
    {
        var candidates = sources.SelectMany(source =>
        {
            var values = new List<SourceCandidate>
            {
                Candidate(source.Base, source.Identity.AutomaticId),
            };
            if (source.Overwrite is { } overwrite)
            {
                values.Add(Candidate(overwrite, source.Identity.AutomaticId));
            }

            return values;
        });
        return new SourceCatalogue(
            SourceInventoryTestData.Workspace(),
            candidates,
            sources,
            [],
            false);
    }

    private static SourceCandidate Candidate(SourceLayer layer, string id)
        => SourceInventoryTestData.Candidate(
            layer.CanonicalPath,
            layer.Form,
            id,
            PhysicalPathState.Contained,
            layer.PhysicalPath,
            Path.GetDirectoryName(layer.PhysicalPath));
}
