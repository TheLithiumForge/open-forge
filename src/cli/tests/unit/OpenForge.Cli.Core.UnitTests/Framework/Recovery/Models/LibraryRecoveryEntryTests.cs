using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Recovery.Models.Application;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.Framework.Recovery.Models.Preparation;
using OpenForge.Cli.Core.Framework.Workspace.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models;

[Trait("Feature", "library-foundation"), Trait("Evidence", "Unit")]
public sealed class LibraryRecoveryEntryTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Prior-missing Library record create retains intended bytes identity without prior payload")]
    public void ReversibleRecordCreateHasNoPriorPayload()
    {
        var root = Path.GetFullPath("workspace");
        var path = Path.Combine(root, ".agents", "open-forge.libraries.json");
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var change = PlannedFileChange.Create(FileExpectation.Missing(path), "record"u8);
        var target = RecoveryBundleTarget.CreateReversible(change, FileStateSnapshot.Missing(path));
        var input = RecoveryBundleInput.Create(workspace, "library attach",
            RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Attach, workspace), Guid.NewGuid(), [target]);

        var entry = RecoveryEntry.FromTarget(input, target, ordinal: 0);

        Assert.True(target.RequiresRecovery);
        Assert.Single(input.RecoveryTargets);
        Assert.Equal(RecoveryEntryKind.OrdinaryCreate, entry.Kind);
        Assert.Equal(".agents/open-forge.libraries.json", entry.TargetPath);
        Assert.Equal(RecoveryEntryStateKind.Missing, entry.Prior.Kind);
        Assert.Null(entry.PriorPayload);
        Assert.True(Assert.IsType<RecoveryContentIdentity>(entry.Intended.OrdinaryFile).Matches("record"u8));
        Assert.Null(entry.Prior.OrdinaryFile);
        Assert.Null(entry.Intended.RelativeFileLink);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Link recovery entries preserve exact raw target and reject target-byte payloads")]
    [InlineData(false), InlineData(true)]
    public void LinksHaveOnlyTypedObjectIdentity(bool delete)
    {
        var link = RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/.agents/a.md");
        var prior = delete ? RecoveryEntryState.RelativeLink(link) : RecoveryEntryState.Missing;
        var intended = delete ? RecoveryEntryState.Missing : RecoveryEntryState.RelativeLink(link);
        var kind = delete ? RecoveryEntryKind.RelativeFileLinkDelete : RecoveryEntryKind.RelativeFileLinkCreate;
        var path = CanonicalRelativePath.Create(".agents/a.md");

        var entry = RecoveryEntry.Create(ordinal: 0, logicalPath: path, kind: kind, prior: prior, intended: intended);

        Assert.Null(entry.PriorPayload);
        Assert.Null(entry.Prior.OrdinaryFile);
        Assert.Null(entry.Intended.OrdinaryFile);
        Assert.Equal(link, delete ? entry.Prior.RelativeFileLink : entry.Intended.RelativeFileLink);
        Assert.Throws<ArgumentException>(() => RecoveryEntry.Create(ordinal: 0, logicalPath: path, kind: kind,
            prior: prior, intended: intended, priorPayload: "payload/000000.bin"));
        Assert.Throws<ArgumentException>(() => RecoveryEntry.Create(ordinal: 0, logicalPath: path, kind: kind,
            prior: intended, intended: prior));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Recovery artifact dispositions distinguish positively retained paths from unknown state")]
    public void RetainedAndUnknownDoNotClaimRemoval()
    {
        var path = Path.GetFullPath("retained.zip");
        var retained = RecoveryBundleDeletionResult.BlockedRetained(path, "changed object");
        var unknown = RecoveryBundleDeletionResult.BlockedUnknown("unavailable observation");

        Assert.Equal(RecoveryBundleDisposition.Retained, retained.Disposition);
        Assert.Equal(path, retained.ResidualPath);
        Assert.Equal(RecoveryBundleDisposition.Unknown, unknown.Disposition);
        Assert.Null(unknown.ResidualPath);
    }
}
