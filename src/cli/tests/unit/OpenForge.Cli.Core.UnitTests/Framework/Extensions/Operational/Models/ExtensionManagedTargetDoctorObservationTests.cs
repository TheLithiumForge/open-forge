using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;
using OpenForge.Cli.Core.Framework.Filesystem.Models.Reading;
using OpenForge.Cli.Core.Framework.OperationalContributors.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Extensions.Operational.Models;

public sealed class ExtensionManagedTargetDoctorObservationTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Extension target unavailable boundaries retain their typed reason"),
     Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Unit")]
    [InlineData((int)ExtensionManagedTargetUnavailableReason.IntendedComparisonUnavailable)]
    [InlineData((int)ExtensionManagedTargetUnavailableReason.TargetReadUnavailable)]
    public void UnavailableBoundaryRetainsReason(int reasonValue)
    {
        var reason = (ExtensionManagedTargetUnavailableReason)reasonValue;
        var observation = ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Unavailable),
            ManagedTargetReadState.Unavailable,
            "boundary cause",
            reason);

        Assert.Equal(ManagedTargetReadState.Unavailable, observation.ReadState);
        Assert.Equal(OperationalTargetState.Unavailable, observation.Target.State);
        Assert.Equal("boundary cause", observation.Cause);
        Assert.Equal(reason, observation.UnavailableReason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension target observed and non-unavailable boundaries have no unavailable reason"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Unit")]
    public void NonUnavailableObservationsHaveNoReason()
    {
        var observed = ExtensionManagedTargetDoctorObservation.Observed(
            Target(OperationalTargetState.Current),
            "current fingerprint");
        var missing = ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Missing),
            ManagedTargetReadState.Missing,
            cause: null,
            unavailableReason: null);
        var blocked = ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Blocked),
            ManagedTargetReadState.Blocked,
            "blocked cause",
            unavailableReason: null);

        Assert.Null(observed.UnavailableReason);
        Assert.Null(missing.UnavailableReason);
        Assert.Null(blocked.UnavailableReason);
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension target unavailable reason is required only for unavailable boundaries"), Trait("Feature", "extension-lifecycle-observation"), Trait("Evidence", "Unit")]
    public void ReasonMustMatchUnavailableBoundary()
    {
        Assert.Throws<ArgumentException>(() => ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Unavailable),
            ManagedTargetReadState.Unavailable,
            "boundary cause",
            unavailableReason: null));
        Assert.Throws<ArgumentException>(() => ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Missing),
            ManagedTargetReadState.Missing,
            cause: null,
            unavailableReason: ExtensionManagedTargetUnavailableReason.TargetReadUnavailable));
        Assert.Throws<ArgumentOutOfRangeException>(() => ExtensionManagedTargetDoctorObservation.Boundary(
            Target(OperationalTargetState.Unavailable),
            ManagedTargetReadState.Unavailable,
            "boundary cause",
            (ExtensionManagedTargetUnavailableReason)int.MaxValue));
    }

    private static ExtensionManagedTargetObservation Target(OperationalTargetState state)
        => new()
        {
            Path = "managed.md",
            Owners = ["extension-id"],
            IntendedFingerprint = null,
            State = state,
        };
}
