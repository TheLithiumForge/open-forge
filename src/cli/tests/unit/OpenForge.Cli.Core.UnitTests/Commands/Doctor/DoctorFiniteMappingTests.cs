using OpenForge.Cli.Core.Commands.Doctor.Models.Result;
using OpenForge.Cli.Core.Commands.Doctor.Shared.Domains;
using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.UnitTests.Commands.Doctor;

public sealed class DoctorFiniteMappingTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Doctor coverage vocabulary rejects undefined values")]
    [Trait("Feature", "doctor-command"), Trait("Evidence", "Unit")]
    public void CoverageVocabularyRejectsUndefinedValues()
    {
        Assert.Equal(
            Enum.GetValues<DoctorCoverageState>(),
            [DoctorCoverageState.Complete, DoctorCoverageState.Incomplete, DoctorCoverageState.Blocked]);
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DoctorDomainSupport.Combine((DoctorCoverageState)int.MaxValue, DoctorCoverageState.Complete));
    }

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
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
