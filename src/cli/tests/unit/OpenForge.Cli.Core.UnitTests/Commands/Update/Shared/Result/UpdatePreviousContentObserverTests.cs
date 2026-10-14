using OpenForge.Cli.Core.Commands.Update.Models.Result;
using OpenForge.Cli.Core.Commands.Update.Shared.Result;

namespace OpenForge.Cli.Core.UnitTests.Commands.Update.Shared.Result;

public sealed class UpdatePreviousContentObserverTests
{
    [Theory(DisplayName = "Update previous content is available only from a retained bundle")]
    [InlineData((int)UpdateRecoveryState.Retained, true, true)]
    [InlineData((int)UpdateRecoveryState.Retained, false, false)]
    [InlineData((int)UpdateRecoveryState.NotCreated, false, false)]
    [InlineData((int)UpdateRecoveryState.Unknown, true, false)]
    [Trait("Boundary", "Processing"), Trait("Feature", "update"), Trait("Evidence", "Unit")]
    public void AvailabilityUsesRecoveryFacts(int state, bool hasPath, bool expected)
    {
        var recovery = new UpdateRecovery
        {
            State = (UpdateRecoveryState)state,
            ProtectedPaths = [".agents/guidance/note.md"],
            ResidualPath = hasPath ? Path.Combine(AppContext.BaseDirectory, "retained-bundle.zip") : null,
        };
        Assert.Equal(expected, UpdatePreviousContentObserver.Read(recovery));
    }
}
