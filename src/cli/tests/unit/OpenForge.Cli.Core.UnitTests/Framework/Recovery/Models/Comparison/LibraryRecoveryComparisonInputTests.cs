using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models.Comparison;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRecoveryComparisonInputTests
{
    [Fact(DisplayName = "Recovery comparison requires entry leaf and independent ordinary-content target agreement")]
    public void RejectsDisagreementAndAbsentOrdinaryReadFacts()
    {
        var root = Path.GetFullPath("comparison-workspace");
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var identity = RecoveryContentIdentity.FromBytes("record"u8);
        var entry = RecoveryEntry.Create(ordinal: 0, logicalPath: CanonicalRelativePath.Create(".agents/open-forge.libraries.json"),
            kind: RecoveryEntryKind.OrdinaryCreate, prior: RecoveryEntryState.Missing, intended: RecoveryEntryState.Ordinary(identity));
        var context = new RecoveryEntryComparisonContext(workspace, entry);
        var otherPath = Path.Combine(root, "other.json");
        var ordinary = NoFollowLeafObservation.OrdinaryFile(context.LogicalPath);
        var otherRead = new RecoveryOrdinaryContentObservation(otherPath, identity, failure: null);
        var read = new RecoveryOrdinaryContentObservation(context.LogicalPath, identity, failure: null);

        Assert.Equal(Path.Combine(root, ".agents", "open-forge.libraries.json"), context.LogicalPath);
        Assert.Throws<ArgumentException>(() => new RecoveryEntryComparisonInput(context, NoFollowLeafObservation.Missing(otherPath), ordinaryContent: null));
        Assert.Throws<ArgumentException>(() => new RecoveryEntryComparisonInput(context, ordinary, ordinaryContent: null));
        Assert.Throws<ArgumentException>(() => new RecoveryEntryComparisonInput(context, ordinary, otherRead));
        Assert.Throws<ArgumentException>(() => new RecoveryEntryComparisonInput(context, NoFollowLeafObservation.Missing(context.LogicalPath), read));
        var link = NoFollowLeafObservation.CreateRelativeFileLink(context.LogicalPath,
            RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../source.json"));
        Assert.Throws<ArgumentException>(() => new RecoveryEntryComparisonInput(context, link, read));
        var accepted = new RecoveryEntryComparisonInput(context, ordinary, read);
        Assert.Same(read, accepted.OrdinaryContent);
    }

    [Fact(DisplayName = "Ordinary recovery content observations require exactly one identity or failure and an absolute path")]
    public void RejectsContradictoryContentFacts()
    {
        var path = Path.GetFullPath("record.json");
        var identity = RecoveryContentIdentity.FromBytes("record"u8);
        var failure = new FilesystemFailure(FilesystemFailureKind.AccessDenied, "cannot read");

        Assert.Throws<ArgumentException>(() => new RecoveryOrdinaryContentObservation(path, identity: null, failure: null));
        Assert.Throws<ArgumentException>(() => new RecoveryOrdinaryContentObservation(path, identity, failure));
        Assert.Throws<ArgumentException>(() => new RecoveryOrdinaryContentObservation("record.json", identity, failure: null));
        Assert.Same(failure, new RecoveryOrdinaryContentObservation(path, identity: null, failure).Failure);
    }

    [Fact(DisplayName = "Neutral Library entry comparison does not widen Framework-only bundle comparison admission")]
    public void RetainsFrameworkProducerBoundary()
    {
        var root = Path.GetFullPath("comparison-workspace");
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var library = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, workspace);
        var framework = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Framework, RecoveryBundleOperation.Update, workspace);
        var identity = RecoveryContentIdentity.FromBytes("ordinary"u8);
        var prior = RecoveryBundleTargetComparison.Prior(".agents/a.md", identity);
        var intended = RecoveryBundleTargetComparison.Intended(".agents/b.md", identity);
        var path = Path.GetFullPath("operation.zip");

        Assert.Throws<ArgumentException>(() => RecoveryBundleComparison.Create(path, library, [prior]));
        var comparison = RecoveryBundleComparison.Create(path, framework, [prior, intended]);
        Assert.True(comparison.IsPartial);
        Assert.Equal(framework, comparison.Attribution);
        Assert.Equal([prior, intended], comparison.Targets);
    }
}
