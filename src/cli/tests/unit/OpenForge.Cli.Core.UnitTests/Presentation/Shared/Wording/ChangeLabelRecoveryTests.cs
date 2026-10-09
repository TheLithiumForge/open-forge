using OpenForge.Cli.Core.Presentation.Shared.Wording;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Shared.Wording;

public sealed class ChangeLabelRecoveryTests
{
    [Theory(DisplayName = "Replacement labels promise recovery content only when the bundle is kept")]
    [InlineData(true, true, "would be replaced (your previous file would be kept in a recovery bundle)")]
    [InlineData(true, false, "would be replaced")]
    [InlineData(false, true, "replaced (your previous file is in the recovery bundle)")]
    [InlineData(false, false, "replaced")]
    [Trait("Feature", "cli-change-labels"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public void ReplacementNamesActualRecovery(bool preview, bool recoveryKept, string expected)
        => Assert.Equal(expected, CliChangeWording.Replaced(preview, recoveryKept));
}
