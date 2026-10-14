using OpenForge.Cli.Core.Shell.Definitions.Shared.Commands;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Shell.Definitions.Shared.Commands;

public sealed class CliCommandTextTests
{
    [Theory(DisplayName = "Route command quoting keeps shell metacharacters literal")]
    [InlineData("it's a $value", "apostrophe", true), InlineData("it's a $value", "apostrophe", false)]
    [InlineData("a`b\"c", "quotes", true), InlineData("a`b\"c", "quotes", false)]
    [InlineData("line\none", "newline", true), InlineData("line\none", "newline", false)]
    [InlineData("line\r\n$one`'\"\t\\", "controls", true), InlineData("line\r\n$one`'\"\t\\", "controls", false)]
    [Trait("Boundary", "Output"), Trait("Feature", "route-mutation"), Trait("Evidence", "Unit")]
    public void QuotingPreservesLiteralValues(string value, string name, bool windowsShell)
    {
        var quoted = CliCommandText.Quote(value, windowsShell);
        Assert.DoesNotContain('\r', quoted);
        Assert.DoesNotContain('\n', quoted);
        Assert.DoesNotContain('\t', quoted);
        CommandOutputSnapshot.MatchSnapshot(quoted, $"{(windowsShell ? "powershell" : "shell")}.{name}");
    }
}
