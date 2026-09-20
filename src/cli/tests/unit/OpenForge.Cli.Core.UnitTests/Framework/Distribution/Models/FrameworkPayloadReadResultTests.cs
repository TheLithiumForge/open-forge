using OpenForge.Cli.Core.Framework.Distribution.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Models;

public sealed class FrameworkPayloadReadResultTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload read factories preserve complete state invariants"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void ReadFactoriesPreserveStateInvariants()
    {
        var payload = FrameworkPayload.Create(
        [
            FrameworkPayloadAsset.Create("AGENTS.md", "agents"u8),
            FrameworkPayloadAsset.Create("CLAUDE.md", "claude"u8),
            FrameworkPayloadAsset.Create(".agents/loader.md", "loader"u8),
        ]);

        var available = FrameworkPayloadReadResult.Available(payload);
        var unavailable = FrameworkPayloadReadResult.Unavailable("No embedded Framework payload resources were found.");
        var invalid = FrameworkPayloadReadResult.Invalid("The embedded Framework payload is invalid.");

        Assert.Equal(FrameworkPayloadReadState.Available, available.State);
        Assert.Same(payload, available.Payload);
        Assert.Null(available.Cause);
        Assert.Equal(FrameworkPayloadReadState.Unavailable, unavailable.State);
        Assert.Null(unavailable.Payload);
        Assert.False(string.IsNullOrEmpty(unavailable.Cause));
        Assert.Equal(FrameworkPayloadReadState.Invalid, invalid.State);
        Assert.Null(invalid.Payload);
        Assert.False(string.IsNullOrEmpty(invalid.Cause));
    }

    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework payload read factories reject incomplete state combinations"), Trait("Feature", "framework-payload"), Trait("Evidence", "Unit")]
    public void ReadFactoriesRejectIncompleteStates()
    {
        Assert.Throws<ArgumentNullException>(() => FrameworkPayloadReadResult.Available(null!));
        Assert.Throws<ArgumentException>(() => FrameworkPayloadReadResult.Unavailable(""));
        Assert.Throws<ArgumentException>(() => FrameworkPayloadReadResult.Invalid(""));
    }
}
