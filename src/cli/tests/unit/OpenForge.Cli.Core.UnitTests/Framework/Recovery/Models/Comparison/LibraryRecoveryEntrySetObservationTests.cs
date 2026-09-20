using System.Collections.Immutable;
using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Operational.Models;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models.Comparison;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRecoveryEntrySetObservationTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery entry sets preserve every finite comparison state without reducing incomplete facts to a safe subset")]
    [InlineData("Prior"), InlineData("Intended"), InlineData("Third"), InlineData("Unavailable"), InlineData("Blocked")]
    public static void RetainsCompleteOrderedFacts(string state)
    {
        var (workspace, candidate, entries) = Facts();
        var classification = state switch
        {
            "Prior" => RecoveryBundleTargetComparisonState.Prior,
            "Intended" => RecoveryBundleTargetComparisonState.Intended,
            "Third" => RecoveryBundleTargetComparisonState.Third,
            "Unavailable" => RecoveryBundleTargetComparisonState.Unavailable,
            "Blocked" => RecoveryBundleTargetComparisonState.Blocked,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        entries = entries.SetItem(1, Comparison(entries[1].Input.Context, classification));

        var result = new RecoveryEntrySetObservation(workspace, candidate, entries);

        Assert.Same(workspace, result.Workspace);
        Assert.Same(candidate, result.Candidate);
        Assert.Equal(entries, result.Entries);
        Assert.Equal(classification, result.Entries[1].State);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery entry sets reject incomplete reordered duplicate and foreign entry or workspace facts")]
    [InlineData("default"), InlineData("empty"), InlineData("missing"), InlineData("reordered"), InlineData("duplicate"), InlineData("extra")]
    [InlineData("changed-entry"), InlineData("foreign-context")]
    public static void RejectsNonExactSet(string defect)
    {
        var (workspace, candidate, entries) = Facts();
        var changedEntry = entries[0].Input.Context.Entry;
        if (defect == "changed-entry")
        {
            changedEntry = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/other.md"),
                kind: RecoveryEntryKind.OrdinaryCreate, prior: RecoveryEntryState.Missing,
                intended: RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes("record"u8)));
        }
        var contextWorkspace = defect == "foreign-context"
            ? new CliWorkspace(Path.GetFullPath("foreign"), Path.GetFullPath("foreign"), CliWorkspaceSelectionMethod.ExplicitWorkspace)
            : workspace;
        var changed = Comparison(new RecoveryEntryComparisonContext(contextWorkspace, changedEntry));
        ImmutableArray<RecoveryEntryComparison> invalid = defect switch
        {
            "default" => default,
            "empty" => [],
            "missing" => [entries[0]],
            "reordered" => [entries[1], entries[0]],
            "duplicate" => [entries[0], entries[0]],
            "extra" => entries.Add(entries[0]),
            "changed-entry" or "foreign-context" => entries.SetItem(0, changed),
            _ => throw new ArgumentOutOfRangeException(nameof(defect)),
        };

        Assert.Throws<ArgumentException>(() => new RecoveryEntrySetObservation(workspace, candidate, invalid));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Recovery entry sets require exact verified final and physical workspace identity")]
    [InlineData("draft"), InlineData("malformed"), InlineData("unsupported"), InlineData("unavailable")]
    [InlineData("workspace-key"), InlineData("workspace-path")]
    public static void RejectsUntrustedCandidate(string defect)
    {
        var (workspace, candidate, entries) = Facts();
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(candidate.Verified);
        var invalid = defect switch
        {
            "draft" => RecoveryBundleCandidateSnapshot.IncompleteDraft(candidate.Path),
            "malformed" => RecoveryBundleCandidateSnapshot.Classified(candidate.Path,
                RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Malformed, "malformed"),
            "unsupported" => RecoveryBundleCandidateSnapshot.Classified(candidate.Path,
                RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Unsupported, "unsupported"),
            "unavailable" => RecoveryBundleCandidateSnapshot.Classified(candidate.Path,
                RecoveryBundleCandidateKind.Final, RecoveryBundleIntegrity.Unavailable, "unavailable"),
            "workspace-key" => RecoveryBundleCandidateSnapshot.VerifiedFinal(verified with { WorkspaceKey = new string('0', 64) }),
            "workspace-path" => RecoveryBundleCandidateSnapshot.VerifiedFinal(verified with { WorkspacePhysicalPath = Path.GetFullPath("other") }),
            _ => throw new ArgumentOutOfRangeException(nameof(defect)),
        };

        Assert.Throws<ArgumentException>(() => new RecoveryEntrySetObservation(workspace, invalid, entries));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Library residual domain observations require the exact neutral set and never acquire Framework comparison authority")]
    public static void BindsLibraryResidualToItsExactCandidate()
    {
        var (workspace, candidate, entries) = Facts();
        var set = new RecoveryEntrySetObservation(workspace, candidate, entries);
        var verified = Assert.IsType<RecoveryBundleVerifiedRead>(candidate.Verified);
        var other = RecoveryBundleCandidateSnapshot.VerifiedFinal(verified with { BundlePath = Path.GetFullPath("other.zip") });

        Assert.Throws<ArgumentException>(() => RecoveryDoctorCandidateObservation.Create(candidate, comparison: null));
        Assert.Throws<ArgumentException>(() => RecoveryDoctorCandidateObservation.Create(other, comparison: null, entryComparisons: set));
        var result = RecoveryDoctorCandidateObservation.Create(candidate, comparison: null, entryComparisons: set);

        Assert.Same(set, result.EntryComparisons);
        Assert.Null(result.Comparison);
        Assert.Equal(RecoveryBundleProducer.Library, result.Attribution?.Producer);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Recovery entry-set agreement uses immutable workspace and entry value identity")]
    public static void AcceptsEquivalentImmutableIdentities()
    {
        var (workspace, candidate, entries) = Facts();
        var equivalentWorkspace = workspace with { };
        entries = [.. entries.Select(entry => Comparison(
            new RecoveryEntryComparisonContext(equivalentWorkspace, entry.Input.Context.Entry with { })))];

        var result = new RecoveryEntrySetObservation(workspace, candidate, entries);

        Assert.Equal(entries, result.Entries);
    }

    private static (CliWorkspace Workspace, RecoveryBundleCandidateSnapshot Candidate, ImmutableArray<RecoveryEntryComparison> Entries) Facts()
    {
        var root = Path.GetFullPath("entry-set-workspace");
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var first = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/record.json"),
            kind: RecoveryEntryKind.OrdinaryCreate, prior: RecoveryEntryState.Missing,
            intended: RecoveryEntryState.Ordinary(RecoveryContentIdentity.FromBytes("record"u8)));
        var second = RecoveryEntry.Create(ordinal: 1, logicalPath: CanonicalRelativePath.Create(".agents/source.md"),
            kind: RecoveryEntryKind.RelativeFileLinkCreate, prior: RecoveryEntryState.Missing,
            intended: RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/source.md")));
        var verified = new RecoveryBundleVerifiedRead
        {
            BundlePath = Path.GetFullPath("operation.zip"),
            WorkspacePhysicalPath = root,
            WorkspaceKey = WorkspaceIdentity.Key(root),
            Command = "library attach",
            Attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, workspace),
            OperationId = Guid.NewGuid(),
            Entries = [first, second],
        };
        return (workspace, RecoveryBundleCandidateSnapshot.VerifiedFinal(verified),
            [Comparison(new RecoveryEntryComparisonContext(workspace, first)), Comparison(new RecoveryEntryComparisonContext(workspace, second))]);
    }

    private static RecoveryEntryComparison Comparison(
        RecoveryEntryComparisonContext context,
        RecoveryBundleTargetComparisonState state = RecoveryBundleTargetComparisonState.Prior)
    {
        var differentLink = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/different.md");
        var leaf = state switch
        {
            RecoveryBundleTargetComparisonState.Prior => NoFollowLeafObservation.Missing(context.LogicalPath),
            RecoveryBundleTargetComparisonState.Intended => NoFollowLeafObservation.CreateRelativeFileLink(context.LogicalPath,
                context.Entry.Intended.RelativeFileLink ?? throw new InvalidOperationException("This intended fixture requires a link entry.")),
            RecoveryBundleTargetComparisonState.Third => NoFollowLeafObservation.CreateRelativeFileLink(context.LogicalPath, differentLink),
            RecoveryBundleTargetComparisonState.Unavailable => NoFollowLeafObservation.Classified(context.LogicalPath, NoFollowLeafState.Inaccessible,
                new FilesystemFailure(FilesystemFailureKind.AccessDenied, "unavailable")),
            RecoveryBundleTargetComparisonState.Blocked => NoFollowLeafObservation.Directory(context.LogicalPath),
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        var observed = state switch
        {
            RecoveryBundleTargetComparisonState.Prior => RecoveryEntryState.Missing,
            RecoveryBundleTargetComparisonState.Intended => context.Entry.Intended,
            RecoveryBundleTargetComparisonState.Third => RecoveryEntryState.RelativeLink(differentLink),
            RecoveryBundleTargetComparisonState.Unavailable or RecoveryBundleTargetComparisonState.Blocked => null,
            _ => throw new ArgumentOutOfRangeException(nameof(state)),
        };
        return new RecoveryEntryComparison
        {
            Input = new RecoveryEntryComparisonInput(context, leaf, ordinaryContent: null),
            State = state,
            Observed = observed,
            Cause = state is RecoveryBundleTargetComparisonState.Unavailable or RecoveryBundleTargetComparisonState.Blocked ? "unsafe or unavailable" : null,
        };
    }
}
