using OpenForge.Cli.Core.Commands.Shared.RouteScaffolding.Models.Rename;
using OpenForge.Cli.Core.Framework.GeneratedNavigation.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;

namespace OpenForge.Cli.Core.Commands.Shared.RouteScaffolding;

/// <summary>
/// Implements the folder-rename decision in Forgiving Maintenance. The caller
/// supplies unique verified identity evidence. This decision acquires no proof
/// or mutation authority and preserves prior bytes for normal recovery.
/// </summary>
internal sealed class RouteScaffoldRenamePlanner
{
    internal RouteScaffoldEntrypointIntent Decide(RouteScaffoldRenameCandidate candidate, RouteScaffoldRenameProof? proof)
    {
        if (candidate.Destination.Base.Form != SourceDocumentForm.CanonicalEntrypoint
            || candidate.DestinationBefore.Kind != FileExpectationKind.Missing)
            throw new ArgumentException("A scaffold intent requires an absent canonical destination.", nameof(candidate));

        var old = candidate.OldSource;
        var before = candidate.OldSnapshot;
        var looksCanonical = old is not null
            && old.Base.Form == SourceDocumentForm.Markdown
            && SourceLogicalPath.ReadParent(old.Identity.CanonicalBasePath) == SourceLogicalPath.ReadParent(candidate.Destination.Identity.CanonicalBasePath)
            && SourceLogicalPath.ReadFileName(old.Identity.CanonicalBasePath).StartsWith('_');
        var admittedProof = proof is not null && Admits(proof);
        var proven = looksCanonical && admittedProof && proof is not null && old is not null
            && before is { Kind: FileExpectationKind.File, HasBytes: true }
            && proof.CurrentCanonicalPath == old.Identity.CanonicalBasePath
            && proof.CurrentContentHash == before.ContentHash
            && SourceFormClassifier.TryClassify(proof.OldCanonicalPath, out var form)
            && form == SourceDocumentForm.CanonicalEntrypoint
            && SourceLogicalPath.ReadFileName(proof.OldCanonicalPath) == SourceLogicalPath.ReadFileName(old.Identity.CanonicalBasePath);

        return new RouteScaffoldEntrypointIntent
        {
            Source = candidate.Destination,
            Before = candidate.DestinationBefore,
            IntendedBytes = proven && before is not null ? before.Bytes : candidate.ScaffoldBytes,
            DeleteBefore = proven ? before : null,
            OldNavigation = !proven && looksCanonical && old is not null
                ? new(old.Identity.CanonicalBasePath, SourceLogicalPath.ReadFileName(candidate.Destination.Identity.CanonicalBasePath))
                : null,
        };
    }

    internal RouteScaffoldEntrypointPlan Complete(RouteScaffoldEntrypointIntent intent, GeneratedNavigationRegion region)
    {
        if (!ReferenceEquals(intent.Source, region.Source)
            || region.State != GeneratedNavigationRegionState.Available
            || region.Change is not { } change
            || !change.BeforeDocumentBytes.AsSpan().SequenceEqual(intent.IntendedBytes.AsSpan()))
            throw new ArgumentException("A scaffold Create requires its exact complete projected document.", nameof(region));
        var create = PlannedFileChange.Create(intent.Before.Expectation, change.ExpectedDocumentBytes.AsSpan());
        if (intent.DeleteBefore is not { } before)
            return new([create], []);
        var delete = PlannedFileChange.Delete(before.Expectation);
        return new([create, delete], [RecoveryBundleTarget.Create(delete, before)]);
    }

    private static bool Admits(RouteScaffoldRenameProof proof)
        => proof.Basis switch
        {
            RouteScaffoldRenameBasis.CommittedContent => proof.Similarity is null,
            RouteScaffoldRenameBasis.GitRemoval => proof.Similarity is >= 90 and <= 100,
            _ => throw new ArgumentOutOfRangeException(nameof(proof), proof.Basis, "The scaffold rename basis is not defined."),
        };
}
