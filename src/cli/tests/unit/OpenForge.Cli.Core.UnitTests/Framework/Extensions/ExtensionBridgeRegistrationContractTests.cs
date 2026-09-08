using OpenForge.Cli.Core.Framework.Extensions.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Extensions;

public sealed class ExtensionBridgeRegistrationContractTests
{
    [Fact(DisplayName = "Extension bridge-registration observations preserve exact ownership and four observed states"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void ObservationsPreserveExactOwnershipAndStates()
    {
        var owners = new[] { "toolkit", "base" };
        var current = Observation(
            ExtensionBridgeRegistrationState.Current,
            actualEntry: "- [Toolkit](toolkit/_toolkit.md)",
            cause: null);
        var missing = Observation(
            ExtensionBridgeRegistrationState.Missing,
            actualEntry: null,
            cause: null);
        var unreadable = Observation(
            ExtensionBridgeRegistrationState.Unreadable,
            actualEntry: null,
            cause: "The parent Entries document is unreadable.");
        var inconsistent = Observation(
            ExtensionBridgeRegistrationState.Inconsistent,
            actualEntry: "- [Other](other/_other.md)",
            cause: null);
        owners[0] = "changed-after-construction";

        Assert.Equal(["toolkit", "base"], current.Owners);
        Assert.Equal(ExtensionBridgeRegistrationState.Current, current.State);
        Assert.Equal(current.ExpectedEntry, current.ActualEntry);
        Assert.Equal(ExtensionBridgeRegistrationState.Missing, missing.State);
        Assert.Null(missing.ActualEntry);
        Assert.Equal(ExtensionBridgeRegistrationState.Unreadable, unreadable.State);
        Assert.Null(unreadable.ActualEntry);
        Assert.NotNull(unreadable.Cause);
        Assert.Equal(ExtensionBridgeRegistrationState.Inconsistent, inconsistent.State);
        Assert.NotEqual(inconsistent.ExpectedEntry, inconsistent.ActualEntry);
        Assert.Null(inconsistent.Cause);
    }

    [Fact(DisplayName = "Extension bridge-registration facts retain set-valued observations and bounded coverage causes"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void FactsRetainSetValuedObservationsAndCoverageCauses()
    {
        var observations = new[]
        {
            Observation(
                ExtensionBridgeRegistrationState.Current,
                "- [Toolkit](toolkit/_toolkit.md)",
                null),
            Observation(ExtensionBridgeRegistrationState.Missing, null, null),
        };
        var complete = ExtensionBridgeRegistrationFacts.Complete(observations);
        var incomplete = ExtensionBridgeRegistrationFacts.Incomplete("The selected source is unavailable.");
        var blocked = ExtensionBridgeRegistrationFacts.Blocked("The registration mapping is ambiguous.");
        observations[0] = Observation(
            ExtensionBridgeRegistrationState.Inconsistent,
            "- [Other](other/_other.md)",
            null);

        Assert.Equal(ExtensionBridgeRegistrationCoverage.Complete, complete.Coverage);
        Assert.Equal(2, complete.Observations.Count);
        Assert.Equal(ExtensionBridgeRegistrationState.Current, complete.Observations[0].State);
        Assert.Null(complete.Cause);
        Assert.Equal(ExtensionBridgeRegistrationCoverage.Incomplete, incomplete.Coverage);
        Assert.Empty(incomplete.Observations);
        Assert.Equal("The selected source is unavailable.", incomplete.Cause);
        Assert.Equal(ExtensionBridgeRegistrationCoverage.Blocked, blocked.Coverage);
        Assert.Empty(blocked.Observations);
        Assert.Equal("The registration mapping is ambiguous.", blocked.Cause);
    }

    [Fact(DisplayName = "Extension bridge-registration observations reject state and ownership mismatches"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void ObservationsRejectStateAndOwnershipMismatches()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionBridgeRegistrationObservation(
            ".agents/toolkit/_toolkit.md",
            ["toolkit"],
            "toolkit",
            "catalogue",
            "toolkit/content/.agents/toolkit/_toolkit.md",
            ".agents/_toolkit.md",
            "- [Toolkit](toolkit/_toolkit.md)",
            null,
            (ExtensionBridgeRegistrationState)int.MaxValue,
            null));
        Assert.Throws<ArgumentException>(() => new ExtensionBridgeRegistrationObservation(
            ".agents/toolkit/_toolkit.md",
            [],
            "toolkit",
            "catalogue",
            "toolkit/content/.agents/toolkit/_toolkit.md",
            ".agents/_toolkit.md",
            "- [Toolkit](toolkit/_toolkit.md)",
            "- [Toolkit](toolkit/_toolkit.md)",
            ExtensionBridgeRegistrationState.Current,
            null));
        Assert.Throws<ArgumentException>(() => Observation(
            ExtensionBridgeRegistrationState.Current,
            actualEntry: "- [Other](other/_other.md)",
            cause: null));
        Assert.Throws<ArgumentException>(() => Observation(
            ExtensionBridgeRegistrationState.Unreadable,
            actualEntry: null,
            cause: null));
        Assert.Throws<ArgumentException>(() => new ExtensionBridgeRegistrationObservation(
            ".agents/toolkit/_toolkit.md",
            ["toolkit"],
            "toolkit",
            "catalogue",
            "toolkit/content/.agents/toolkit/_toolkit.md",
            ".agents/_toolkit.md",
            "- [Toolkit](toolkit/_toolkit.md)",
            null,
            ExtensionBridgeRegistrationState.Current,
            "unexpected cause"));
    }

    private static ExtensionBridgeRegistrationObservation Observation(
        ExtensionBridgeRegistrationState state,
        string? actualEntry,
        string? cause)
        => new(
            ".agents/toolkit/_toolkit.md",
            ["toolkit", "base"],
            "toolkit",
            "catalogue",
            "toolkit/content/.agents/toolkit/_toolkit.md",
            ".agents/_toolkit.md",
            "- [Toolkit](toolkit/_toolkit.md)",
            actualEntry,
            state,
            cause);
}
