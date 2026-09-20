using OpenForge.Cli.Core.Framework.Distribution.Models.Content;
using OpenForge.Cli.Core.Framework.Distribution.Operational.Models;

namespace OpenForge.Cli.Core.UnitTests.Framework.Distribution.Operational.Models;

public sealed class FrameworkTargetSourceValidationTests
{
    [Trait("Boundary", "Processing")]
    [Fact(DisplayName = "Framework target source validation enforces coherent causes"), Trait("Feature", "status-command"), Trait("Evidence", "Unit")]
    public void TargetSourceValidationEnforcesCoherentCauses()
    {
        var valid = new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.Valid,
            cause: null);
        var mismatch = new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.SourceMismatch,
            "source mismatch");
        var blocked = new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.Blocked,
            "source blocked");

        Assert.Equal(FrameworkTargetSourceState.Valid, valid.State);
        Assert.Null(valid.Cause);
        Assert.Equal("source mismatch", mismatch.Cause);
        Assert.Equal("source blocked", blocked.Cause);
        Assert.Throws<ArgumentException>(() => new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.Valid,
            "unexpected"));
        Assert.Throws<ArgumentException>(() => new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.SourceMismatch,
            "  "));
        Assert.Throws<ArgumentException>(() => new FrameworkTargetSourceValidation(
            FrameworkTargetSourceState.Blocked,
            cause: null));
        Assert.Throws<ArgumentOutOfRangeException>(() => new FrameworkTargetSourceValidation(
            (FrameworkTargetSourceState)int.MaxValue,
            cause: null));
    }

}
