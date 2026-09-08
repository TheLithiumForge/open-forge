using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Models.Record;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem;
using OpenForge.Cli.Core.Framework.Recovery.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Comparison;
using OpenForge.Cli.Core.Framework.Workspace;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Operational.Shared.Attribution;

internal sealed class LibraryResidualFacts
{
    internal LibraryResidualFacts()
    {
        var root = Path.Combine(Path.GetTempPath(), "library-residual-facts");
        Workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        Record = LibrariesRecord.Create([
            LibraryRecord.Create(LibraryId.Create("team"), WorkspaceRelativeDirectory.Create("shared/team"),
                [SourceRelativeEligiblePath.Create(".agents/a.md")]),
        ]);
        PayloadIdentity = RecoveryContentIdentity.FromBytes("{\"schemaVersion\":1,\"libraries\":[{\"id\":\"team\",\"sourceRoot\":\"shared/team\",\"paths\":[\".agents/a.md\"]}]}"u8);
        RecordEntry = PriorEntry(".agents/open-forge.libraries.json");
        LinkEntry = RecoveryEntry.Create(1, CanonicalRelativePath.Create(".agents/a.md"), RecoveryEntryKind.RelativeFileLinkDelete,
            RecoveryEntryState.RelativeLink(RelativeFileLinkIdentity.Create(NoFollowLinkKind.SymbolicLink, "../shared/team/.agents/a.md")),
            RecoveryEntryState.Missing);
        var key = WorkspaceIdentity.Key(Workspace.PhysicalRoot);
        var candidate = RecoveryBundleCandidateSnapshot.VerifiedFinal(new RecoveryBundleVerifiedRead
        {
            BundlePath = Path.Combine(Path.GetTempPath(), "operation-123456781234123412341234567890ab.zip"),
            WorkspacePhysicalPath = WorkspaceIdentity.NormalizePhysicalPath(Workspace.PhysicalRoot),
            WorkspaceKey = key,
            Command = "library detach",
            Attribution = RecoveryBundleAttribution.Create(RecoveryBundleProducer.Library, RecoveryBundleOperation.Detach, Workspace),
            OperationId = Guid.Parse("12345678-1234-1234-1234-1234567890ab"),
            Entries = [RecordEntry, LinkEntry],
        });
        LinkComparison = Comparison(LinkEntry);
        Residual = new RecoveryEntrySetObservation(Workspace, candidate, [Comparison(RecordEntry), LinkComparison]);
    }

    internal CliWorkspace Workspace { get; }
    internal LibrariesRecord Record { get; }
    internal RecoveryContentIdentity PayloadIdentity { get; }
    internal RecoveryEntry RecordEntry { get; }
    internal RecoveryEntry LinkEntry { get; }
    internal RecoveryEntryComparison LinkComparison { get; }
    internal RecoveryEntrySetObservation Residual { get; }
    internal LibraryRecoveryPriorRecord Prior => new(Record, RecordEntry, PayloadIdentity);

    internal LibrariesRecordRead Current(bool present)
        => new()
        {
            State = present ? LibrariesRecordReadState.Complete : LibrariesRecordReadState.Missing,
            Record = present ? Record : null,
            Snapshot = present
                ? FileStateSnapshot.File(RecordPath, RecordPath,
                    "{\"schemaVersion\":1,\"libraries\":[{\"id\":\"team\",\"sourceRoot\":\"shared/team\",\"paths\":[\".agents/a.md\"]}]}"u8)
                : FileStateSnapshot.Missing(RecordPath),
            Cause = null,
        };

    private string RecordPath => Path.Combine(Workspace.LexicalRoot, ".agents", "open-forge.libraries.json");

    internal RecoveryEntry PriorEntry(string path)
        => RecoveryEntry.Create(0, CanonicalRelativePath.Create(path), RecoveryEntryKind.OrdinaryDelete,
            RecoveryEntryState.Ordinary(PayloadIdentity), RecoveryEntryState.Missing, "payloads/00000000.bin");

    internal RecoveryEntryComparison Comparison(RecoveryEntry entry)
    {
        var context = new RecoveryEntryComparisonContext(Workspace, entry);
        return new RecoveryEntryComparison
        {
            Input = new RecoveryEntryComparisonInput(context, NoFollowLeafObservation.Missing(context.LogicalPath), null),
            State = RecoveryBundleTargetComparisonState.Intended,
            Observed = RecoveryEntryState.Missing,
            Cause = null,
        };
    }
}
