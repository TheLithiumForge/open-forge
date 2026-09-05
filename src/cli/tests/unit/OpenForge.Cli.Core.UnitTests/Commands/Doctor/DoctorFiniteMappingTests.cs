using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Rendering;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorFiniteMappingTests
{
    [Fact(DisplayName = "Doctor finite finding coordinates map exhaustively and reject undefined values")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void FindingCoordinatesMapExhaustivelyAndFailClosed()
    {
        AssertMappings(
            [
                (DoctorFindingSeverity.Information, "information"),
                (DoctorFindingSeverity.Warning, "warning"),
                (DoctorFindingSeverity.Error, "error"),
            ],
            DoctorFindingWireVocabulary.Severity);
        AssertMappings(
            [
                (DoctorCandidateCardinality.None, "none"),
                (DoctorCandidateCardinality.One, "one"),
                (DoctorCandidateCardinality.Several, "several"),
            ],
            DoctorFindingWireVocabulary.Cardinality);
        AssertMappings(
            [(DoctorProposalKind.ReferenceCanonicalization, "reference-canonicalization")],
            DoctorFindingWireVocabulary.Proposal);
        AssertMappings(
            [
                (DoctorProposalVerificationKind.SameTargetIdentity, "same-target-identity"),
                (DoctorProposalVerificationKind.ResultingBytes, "resulting-bytes"),
            ],
            DoctorFindingWireVocabulary.Verification);
        AssertMappings(
            [
                (DoctorProposalRecoveryKind.NoPersistentState, "no-persistent-state"),
                (DoctorProposalRecoveryKind.RepairReceiptRequired, "repair-receipt-required"),
            ],
            DoctorFindingWireVocabulary.Recovery);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorFindingWireVocabulary.Severity((DoctorFindingSeverity)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorFindingWireVocabulary.Cardinality((DoctorCandidateCardinality)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorFindingWireVocabulary.Proposal((DoctorProposalKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorFindingWireVocabulary.Verification((DoctorProposalVerificationKind)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorFindingWireVocabulary.Recovery((DoctorProposalRecoveryKind)int.MaxValue));
    }

    [Fact(DisplayName = "Doctor coverage merge is exhaustive for both operands and rejects undefined values")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void CoverageMergeIsExhaustiveAndFailsClosed()
    {
        var expected = new (DoctorCoverageState First, DoctorCoverageState Second, DoctorCoverageState Result)[]
        {
            (DoctorCoverageState.Complete, DoctorCoverageState.Complete, DoctorCoverageState.Complete),
            (DoctorCoverageState.Complete, DoctorCoverageState.Incomplete, DoctorCoverageState.Incomplete),
            (DoctorCoverageState.Complete, DoctorCoverageState.Blocked, DoctorCoverageState.Blocked),
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Complete, DoctorCoverageState.Incomplete),
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Incomplete, DoctorCoverageState.Incomplete),
            (DoctorCoverageState.Incomplete, DoctorCoverageState.Blocked, DoctorCoverageState.Blocked),
            (DoctorCoverageState.Blocked, DoctorCoverageState.Complete, DoctorCoverageState.Blocked),
            (DoctorCoverageState.Blocked, DoctorCoverageState.Incomplete, DoctorCoverageState.Blocked),
            (DoctorCoverageState.Blocked, DoctorCoverageState.Blocked, DoctorCoverageState.Blocked),
        };
        foreach (var (first, second, result) in expected)
        {
            Assert.Equal(result, DoctorDomainSupport.Combine(first, second));
        }

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorDomainSupport.Combine((DoctorCoverageState)int.MaxValue, DoctorCoverageState.Complete));
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorDomainSupport.Combine(DoctorCoverageState.Complete, (DoctorCoverageState)int.MaxValue));
    }

    [Fact(DisplayName = "Extension Doctor assessment rejects every undefined finite coordinate")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void ExtensionAssessmentRejectsUndefinedFiniteCoordinates()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAssessment(
            state: (OperationalViewState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAssessment(
            section: (ExtensionLifecycleSectionState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAssessment(
            managedSet: (ExtensionManagedSetState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAssessment(
            lifecycle: (OperationalLifecycleState)int.MaxValue));
        Assert.Throws<ArgumentOutOfRangeException>(() => CreateAssessment(
            sourceAvailability: (OperationalSourceAvailability)int.MaxValue));
    }

    private static void AssertMappings<T>(
        IReadOnlyList<(T Value, string Name)> expected,
        Func<T, string> read)
        where T : struct, Enum
    {
        Assert.Equal(Enum.GetValues<T>(), expected.Select(item => item.Value));
        foreach (var (value, name) in expected)
        {
            Assert.Equal(name, read(value));
        }
    }

    private static ExtensionLifecycleDoctorAssessment CreateAssessment(
        OperationalViewState state = OperationalViewState.Complete,
        ExtensionLifecycleSectionState section = ExtensionLifecycleSectionState.Present,
        ExtensionManagedSetState managedSet = ExtensionManagedSetState.Empty,
        OperationalLifecycleState lifecycle = OperationalLifecycleState.Trusted,
        OperationalSourceAvailability sourceAvailability = OperationalSourceAvailability.Available)
        => ExtensionLifecycleDoctorAssessment.Create(
            state,
            section,
            managedSet,
            lifecycle,
            sourceAvailability);
}
