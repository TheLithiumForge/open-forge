using OpenForge.Cli.Core.Commands.Tool.Shared.Writing.Planning;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Writing.Planning;

[Trait("Feature", "tool"), Trait("Evidence", "Unit"), Trait("Boundary", "Processing")]
public sealed class ToolWriteMinimumVersionTests
{
    [Theory(DisplayName = "Open Forge minimum versions compare stable and prerelease precedence")]
    [InlineData("0.9.0-beta.18", "0.9.0-beta.19", false)]
    [InlineData("0.9.0-beta.19", "0.9.0-beta.19", true)]
    [InlineData("0.9.0-beta.20", "0.9.0-beta.19", true)]
    [InlineData("0.9.0-beta.9", "0.9.0-beta.19", false)]
    [InlineData("0.9.0", "0.9.0-beta.19", true)]
    [InlineData("0.9.0-beta.19", "0.9.0", false)]
    [InlineData("0.10.0-alpha", "0.9.0", true)]
    [InlineData("1.0.0-alpha.1", "1.0.0-alpha", true)]
    [InlineData("1.0.0-alpha", "1.0.0-alpha.1", false)]
    [InlineData("1.0.0-beta", "1.0.0-alpha.99", true)]
    [InlineData("1.0.0-1", "1.0.0-alpha", false)]
    [InlineData("1.0.0-alpha", "1.0.0-999", true)]
    [InlineData("1.0.0-beta.99999999999999999999", "1.0.0-beta.9999999999999999999", true)]
    [InlineData("1.0.0+build.2", "1.0.0+build.99", true)]
    [InlineData("1.0.0-beta.19+local", "1.0.0-beta.19", true)]
    [InlineData("1.0", "1.0.0", false)]
    [InlineData("1.0.0", "invalid", false)]
    [InlineData("1.0.0.0", "1.0.0", false)]
    [InlineData("01.0.0", "1.0.0", false)]
    [InlineData("1.0.0-beta.01", "1.0.0", false)]
    [InlineData("1.0.0+", "1.0.0", false)]
    [InlineData("1.0.0\n", "1.0.0", false)]
    public void ComparesMinimum(string current, string minimum, bool expected)
        => Assert.Equal(expected, ToolWriteMinimumVersion.IsSatisfied(current, minimum));
}
