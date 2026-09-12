using OpenForge.Cli.Core.Framework.Filesystem.Models;
using OpenForge.Cli.Core.Framework.Recovery.Models.Catalogue;

namespace OpenForge.Cli.Core.UnitTests.Framework.Recovery.Models.Catalogue;

[Trait("Feature", "mutation-foundation"), Trait("Evidence", "Unit")]
public sealed class RecoveryReadClassificationContractTests
{
    [Theory(DisplayName = "Classified recovery reads retain each admitted state and supplied failure and cause")]
    [InlineData((int)RecoveryBundleReadState.Malformed), InlineData((int)RecoveryBundleReadState.Unsupported), InlineData((int)RecoveryBundleReadState.Unavailable)]
    public void ReadClassifiedRetainsAdmittedFacts(int state)
    {
        var failure = new FilesystemFailure(FilesystemFailureKind.InputOutput, "Filesystem failure.");
        const string cause = "  classified read cause  ";

        var result = RecoveryBundleReadResult.Classified((RecoveryBundleReadState)state, cause, failure);

        Assert.Equal((RecoveryBundleReadState)state, result.State);
        Assert.Null(result.Verified);
        Assert.Same(failure, result.Failure);
        Assert.Equal("  classified read cause  ", result.Cause);
        Assert.Same(cause, result.Cause);
    }

    [Theory(DisplayName = "Classified recovery reads reject reserved states before an invalid cause")]
    [InlineData((int)RecoveryBundleReadState.Valid), InlineData((int)RecoveryBundleReadState.Cancelled)]
    public void ReadClassifiedRejectsReservedStatesBeforeCause(int state)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecoveryBundleReadResult.Classified((RecoveryBundleReadState)state, cause: " "));

        Assert.Equal("state", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery reads reject an undefined state with otherwise valid facts")]
    public void ReadClassifiedRejectsUndefinedState()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecoveryBundleReadResult.Classified((RecoveryBundleReadState)999, cause: "Classified read."));

        Assert.Equal("state", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery reads reject an undefined state before an invalid cause")]
    public void ReadClassifiedChecksUndefinedStateBeforeCause()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            RecoveryBundleReadResult.Classified((RecoveryBundleReadState)999, cause: " "));

        Assert.Equal("state", exception.ParamName);
    }

    [Theory(DisplayName = "Classified recovery candidates retain each admitted kind and integrity with normalized path and supplied facts")]
    [InlineData((int)RecoveryBundleCandidateKind.Final, (int)RecoveryBundleIntegrity.Malformed)]
    [InlineData((int)RecoveryBundleCandidateKind.Final, (int)RecoveryBundleIntegrity.Unsupported)]
    [InlineData((int)RecoveryBundleCandidateKind.Final, (int)RecoveryBundleIntegrity.Unavailable)]
    [InlineData((int)RecoveryBundleCandidateKind.Draft, (int)RecoveryBundleIntegrity.Malformed)]
    [InlineData((int)RecoveryBundleCandidateKind.Draft, (int)RecoveryBundleIntegrity.Unsupported)]
    [InlineData((int)RecoveryBundleCandidateKind.Draft, (int)RecoveryBundleIntegrity.Unavailable)]
    public void CandidateClassifiedRetainsAdmittedFacts(int kind, int integrity)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-recovery-classification"));
        var path = Path.Combine(root, "nested", "..", "bundle.zip");
        var failure = new FilesystemFailure(FilesystemFailureKind.InputOutput, "Filesystem failure.");
        const string cause = "  classified candidate cause  ";

        var result = RecoveryBundleCandidateSnapshot.Classified(
            path: path,
            kind: (RecoveryBundleCandidateKind)kind,
            integrity: (RecoveryBundleIntegrity)integrity,
            cause: cause,
            failure: failure);

        Assert.Equal(Path.Combine(root, "bundle.zip"), result.Path);
        Assert.Equal((RecoveryBundleCandidateKind)kind, result.Kind);
        Assert.Equal((RecoveryBundleIntegrity)integrity, result.Integrity);
        Assert.Null(result.Verified);
        Assert.Same(failure, result.Failure);
        Assert.Equal("  classified candidate cause  ", result.Cause);
        Assert.Same(cause, result.Cause);
    }

    [Theory(DisplayName = "Classified recovery candidates reject reserved integrity before invalid kind cause and path")]
    [InlineData((int)RecoveryBundleIntegrity.Verified), InlineData((int)RecoveryBundleIntegrity.Incomplete)]
    public void CandidateClassifiedRejectsReservedIntegrityBeforeOtherFacts(int integrity)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: null!, kind: (RecoveryBundleCandidateKind)999, integrity: (RecoveryBundleIntegrity)integrity, cause: " "));

        Assert.Equal("integrity", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery candidates reject undefined integrity with otherwise valid facts")]
    public void CandidateClassifiedRejectsUndefinedIntegrity()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: "bundle.zip", kind: RecoveryBundleCandidateKind.Final, integrity: (RecoveryBundleIntegrity)999, cause: "Classified candidate."));

        Assert.Equal("integrity", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery candidates reject undefined integrity before invalid kind cause and path")]
    public void CandidateClassifiedChecksUndefinedIntegrityBeforeOtherFacts()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: null!, kind: (RecoveryBundleCandidateKind)999, integrity: (RecoveryBundleIntegrity)999, cause: " "));

        Assert.Equal("integrity", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery candidates reject undefined kind with otherwise valid facts")]
    public void CandidateClassifiedRejectsUndefinedKind()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: "bundle.zip", kind: (RecoveryBundleCandidateKind)999, integrity: RecoveryBundleIntegrity.Malformed, cause: "Classified candidate."));

        Assert.Equal("kind", exception.ParamName);
    }

    [Theory(DisplayName = "Classified recovery candidates reject undefined kind before cause validation and path normalization"), InlineData(" "), InlineData("Classified candidate.")]
    public void CandidateClassifiedChecksUndefinedKindBeforeCauseAndPath(string cause)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: null!, kind: (RecoveryBundleCandidateKind)999, integrity: RecoveryBundleIntegrity.Malformed, cause: cause));

        Assert.Equal("kind", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery candidates validate cause before normalizing the supplied path")]
    public void CandidateClassifiedChecksCauseBeforePath()
    {
        var exception = Assert.Throws<ArgumentException>(() => RecoveryBundleCandidateSnapshot.Classified(
            path: null!, kind: RecoveryBundleCandidateKind.Final, integrity: RecoveryBundleIntegrity.Malformed, cause: " "));

        Assert.Equal("cause", exception.ParamName);
    }

    [Fact(DisplayName = "Classified recovery reads and candidates preserve an absent optional filesystem failure")]
    public void ClassifiedFactoriesRetainAbsentFailure()
    {
        var read = RecoveryBundleReadResult.Classified(RecoveryBundleReadState.Malformed, "Read cause.");
        var candidate = RecoveryBundleCandidateSnapshot.Classified(
            path: "bundle.zip", kind: RecoveryBundleCandidateKind.Draft, integrity: RecoveryBundleIntegrity.Unsupported, cause: "Candidate cause.");

        Assert.Null(read.Failure);
        Assert.Null(candidate.Failure);
    }
}
