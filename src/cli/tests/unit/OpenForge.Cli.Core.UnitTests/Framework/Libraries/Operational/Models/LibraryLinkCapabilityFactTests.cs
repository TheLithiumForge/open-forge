using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Libraries.Operational.Models;

[Trait("Feature", "library-read"), Trait("Evidence", "Unit")]
public sealed class LibraryLinkCapabilityFactTests
{
    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Library link capability retains independent positive and negative capability evidence")]
    [InlineData((int)LibraryLinkCapabilityState.Supported), InlineData((int)LibraryLinkCapabilityState.Unsupported)]
    public static void RetainsCapabilityEvidence(int stateValue)
    {
        var state = (LibraryLinkCapabilityState)stateValue;
        var fact = new LibraryLinkCapabilityFact(state, "independently observed platform capability");

        Assert.Equal(state, fact.State);
        Assert.Equal("independently observed platform capability", fact.Evidence);
    }

    [Trait("Boundary", "Processing")]
    [Theory(DisplayName = "Library link capability cannot be asserted without observation evidence")]
    [InlineData(""), InlineData(" "), InlineData("\t")]
    public static void RejectsAbsentEvidence(string evidence)
        => Assert.Throws<ArgumentException>(() => new LibraryLinkCapabilityFact(LibraryLinkCapabilityState.Supported, evidence));

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Library capability rejects an undefined state instead of assuming unsupported")]
    public static void RejectsUndefinedState()
        => Assert.Throws<ArgumentOutOfRangeException>(() => new LibraryLinkCapabilityFact((LibraryLinkCapabilityState)999, "observation"));
}
