using OpenForge.Cli.Core.Framework.Tools.Shared.Versions;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Versions;

[Trait("Feature", "tool-runtime-ranges"), Trait("Evidence", "Unit")]
public sealed class ToolRuntimeRangeTests
{
    [Theory(DisplayName = "Runtime ranges enforce stable exact versions and bounded minimum and ceiling grammar")]
    [InlineData("20.0.0", ">=20", true)]
    [InlineData("19.9.9", ">=20", false)]
    [InlineData("20.1.0", ">=20.1", true)]
    [InlineData("20.0.9", ">=20.1", false)]
    [InlineData("22.5.0", ">=20.1.3 <24", true)]
    [InlineData("24.0.0", ">=20 <24", false)]
    [InlineData("23.9.9", ">=20 <24.0", true)]
    [InlineData("20.1.0", "20.1.0", true)]
    [InlineData("20.1.1", "20.1.0", false)]
    [InlineData("20.1.0-rc.1", ">=20", false)]
    [InlineData("24.0.0", ">=24 <22", false)]
    [InlineData("24.0.0", ">=24 <24", false)]
    [InlineData("20.1.0", "^20", false)]
    [InlineData("20.1.0", "20.x", false)]
    [InlineData("20.1.0", ">=20 || >=22", false)]
    [InlineData("20.1.0", ">=020", false)]
    public void MatchesRuntime(string version, string range, bool matches) => Assert.Equal(matches, ToolRuntimeRange.Satisfies(version, range));
}
