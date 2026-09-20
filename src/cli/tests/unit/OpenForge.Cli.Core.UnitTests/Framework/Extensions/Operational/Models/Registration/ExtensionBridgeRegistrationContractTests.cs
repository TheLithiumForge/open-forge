using OpenForge.Cli.Core.Framework.Extensions.Operational.Models.Registration;

namespace OpenForge.Cli.Core.UnitTests.Framework.Extensions.Operational.Models.Registration;

public sealed class ExtensionBridgeRegistrationContractTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension bridge-registration observations preserve exact ownership and four observed states"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void ObservationsPreserveExactOwnershipAndStates()
    {
        var owners = new[] { "toolkit", "base" };
        var current = new ExtensionBridgeRegistrationObservation(
            new BridgeCandidate(
                ".agents/toolkit/_toolkit.md",
                owners,
                "toolkit",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = "- [Toolkit](toolkit/_toolkit.md)",
                State = ExtensionBridgeRegistrationState.Current,
                Cause = null,
            });
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
        Assert.Equal(".agents/toolkit/_toolkit.md", current.TargetPath);
        Assert.Equal("toolkit", current.PackageId);
        Assert.Equal("catalogue", current.ReviewedSourceIdentity);
        Assert.Equal("toolkit/content/.agents/toolkit/_toolkit.md", current.SourceAssetPath);
        Assert.Equal(".agents/_toolkit.md", current.ParentPath);
        Assert.Equal("- [Toolkit](toolkit/_toolkit.md)", current.ExpectedEntry);
        Assert.Null(current.Cause);
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

    [Trait("Boundary", "Processing")]
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

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Extension bridge-registration observations reject state and ownership mismatches"), Trait("Feature", "extension-update"), Trait("Evidence", "Unit")]
    public void ObservationsRejectStateAndOwnershipMismatches()
    {
        var undefined = Assert.Throws<ArgumentOutOfRangeException>(() => new ExtensionBridgeRegistrationObservation(
            new BridgeCandidate(
                ".agents/toolkit/_toolkit.md",
                ["toolkit"],
                "toolkit",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = null,
                State = (ExtensionBridgeRegistrationState)int.MaxValue,
                Cause = null,
            }));
        Assert.Equal("state", undefined.ParamName);
        Assert.Equal((ExtensionBridgeRegistrationState)int.MaxValue, undefined.ActualValue);
        Assert.Equal("The Extension bridge-registration state is not defined. (Parameter 'state')"
            + Environment.NewLine + "Actual value was 2147483647.", undefined.Message);
        var emptyOwners = Assert.Throws<ArgumentException>(() => new ExtensionBridgeRegistrationObservation(
            new BridgeCandidate(
                ".agents/toolkit/_toolkit.md",
                [],
                "toolkit",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = "- [Toolkit](toolkit/_toolkit.md)",
                State = ExtensionBridgeRegistrationState.Current,
                Cause = null,
            }));
        Assert.Equal("owners", emptyOwners.ParamName);
        Assert.Equal("Extension bridge-registration observations require unique non-empty owners. (Parameter 'owners')", emptyOwners.Message);
        var identity = Assert.Throws<ArgumentException>(() => new ExtensionBridgeRegistrationObservation(
            new BridgeCandidate(
                " ",
                [],
                " ",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = null,
                State = (ExtensionBridgeRegistrationState)int.MaxValue,
                Cause = null,
            }));
        Assert.Equal("targetPath", identity.ParamName);
        Assert.Equal("The value cannot be an empty string or composed entirely of whitespace. (Parameter 'targetPath')", identity.Message);
        Assert.Throws<ArgumentException>(() => Observation(
            ExtensionBridgeRegistrationState.Current,
            actualEntry: "- [Other](other/_other.md)",
            cause: null));
        Assert.Throws<ArgumentException>(() => Observation(
            ExtensionBridgeRegistrationState.Unreadable,
            actualEntry: null,
            cause: null));
        Assert.Throws<ArgumentException>(() => new ExtensionBridgeRegistrationObservation(
            new BridgeCandidate(
                ".agents/toolkit/_toolkit.md",
                ["toolkit"],
                "toolkit",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = null,
                State = ExtensionBridgeRegistrationState.Current,
                Cause = "unexpected cause",
            }));
    }

    private static ExtensionBridgeRegistrationObservation Observation(
        ExtensionBridgeRegistrationState state,
        string? actualEntry,
        string? cause)
        => new(
            new BridgeCandidate(
                ".agents/toolkit/_toolkit.md",
                ["toolkit", "base"],
                "toolkit",
                "catalogue",
                "toolkit/content/.agents/toolkit/_toolkit.md",
                []),
            new BridgeObservationComparison
            {
                ParentPath = ".agents/_toolkit.md",
                ExpectedEntry = "- [Toolkit](toolkit/_toolkit.md)",
                ActualEntry = actualEntry,
                State = state,
                Cause = cause,
            });
}
