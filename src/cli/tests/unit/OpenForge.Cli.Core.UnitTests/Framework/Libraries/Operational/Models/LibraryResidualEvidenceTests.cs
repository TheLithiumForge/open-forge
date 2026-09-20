using OpenForge.Cli.Core.Framework.Filesystem.LogicalPaths.Models;
using OpenForge.Cli.Core.Framework.Libraries.Models.Identity;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Entries;
using OpenForge.Cli.Core.Framework.Recovery.Models.Identity;
using OpenForge.Cli.Core.UnitTests.Framework.Libraries.Operational.Shared.Attribution;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Operational.Models;

[Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
public sealed class LibraryResidualEvidenceTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Library residual evidence retains exact current membership or independently verified prior membership")]
    [InlineData(false), InlineData(true)]
    public static void MembershipRetainsExactCandidateEntryAndRecord(bool currentPresent)
    {
        var facts = new LibraryResidualFacts();
        var current = facts.Current(currentPresent);
        var prior = currentPresent ? null : facts.Prior;

        var evidence = new LibraryResidualEvidence(LibraryId.Create("team"), current, prior, facts.Residual, facts.LinkComparison);

        Assert.Same(current, evidence.CurrentRecord);
        Assert.Same(prior, evidence.VerifiedPriorRecord);
        Assert.Same(facts.Residual, evidence.Residual);
        Assert.Same(facts.LinkComparison, evidence.Entry);
        Assert.Equal("team", evidence.LibraryId.Value);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Library residual attribution cannot be established by matching destination path alone")]
    public static void MissingMembershipRejectsPathOnlyAuthority()
    {
        var facts = new LibraryResidualFacts();

        Assert.Throws<ArgumentException>(() => new LibraryResidualEvidence(
            LibraryId.Create("team"), facts.Current(false), null, facts.Residual, facts.LinkComparison));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Library residual attribution rejects a supplied Library ID outside current and prior record membership")]
    [InlineData(false), InlineData(true)]
    public static void UnknownIdCannotBorrowMembership(bool currentPresent)
    {
        var facts = new LibraryResidualFacts();

        Assert.Throws<ArgumentException>(() => new LibraryResidualEvidence(
            LibraryId.Create("unknown"), facts.Current(currentPresent), facts.Prior, facts.Residual, facts.LinkComparison));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Library residual evidence rejects a comparison not belonging to its exact residual entry set")]
    public static void RejectsForeignComparison()
    {
        var facts = new LibraryResidualFacts();
        var foreign = facts.Comparison(facts.PriorEntry(".agents/foreign.json"));

        Assert.Throws<ArgumentException>(() => new LibraryResidualEvidence(
            LibraryId.Create("team"), facts.Current(true), null, facts.Residual, foreign));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Library prior-record membership is bound to the exact entry present in the verified candidate")]
    public static void RejectsPriorRecordFromAnotherCandidate()
    {
        var facts = new LibraryResidualFacts();
        var otherIdentity = RecoveryContentIdentity.FromBytes("other record bytes"u8);
        var otherEntry = RecoveryEntry.Create(0, CanonicalRelativePath.Create(".agents/open-forge.lock.json"),
            RecoveryEntryKind.OrdinaryDelete, RecoveryEntryState.Ordinary(otherIdentity), RecoveryEntryState.Missing, "payloads/00000000.bin");
        var otherPrior = new LibraryRecoveryPriorRecord(facts.Record, otherEntry, otherIdentity);

        Assert.Throws<ArgumentException>(() => new LibraryResidualEvidence(
            LibraryId.Create("team"), facts.Current(false), otherPrior, facts.Residual, facts.LinkComparison));
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Library prior-record evidence rejects another path or mismatched verified payload identity")]
    [InlineData("path"), InlineData("hash"), InlineData("length"), InlineData("missing-payload")]
    public static void RejectsUnboundPriorPayload(string mismatch)
    {
        var facts = new LibraryResidualFacts();
        var entry = mismatch switch
        {
            "path" => facts.PriorEntry(".agents/other.json"),
            "missing-payload" => RecoveryEntry.Create(0, CanonicalRelativePath.Create(".agents/open-forge.lock.json"),
                RecoveryEntryKind.OrdinaryCreate, RecoveryEntryState.Missing, RecoveryEntryState.Ordinary(facts.PayloadIdentity)),
            _ => facts.RecordEntry,
        };
        var identity = mismatch switch
        {
            "hash" => RecoveryContentIdentity.Create(facts.PayloadIdentity.Length, new string('0', 64)),
            "length" => RecoveryContentIdentity.Create(facts.PayloadIdentity.Length + 1, facts.PayloadIdentity.Sha256),
            _ => facts.PayloadIdentity,
        };

        Assert.Throws<ArgumentException>(() => new LibraryRecoveryPriorRecord(facts.Record, entry, identity));
    }
}
