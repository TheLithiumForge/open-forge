using OpenForge.Cli.Core.Commands.Tool.Shared.Models.Evidence;
using OpenForge.Cli.Core.Commands.Tool.Shared.Reporting;

namespace OpenForge.Cli.Core.UnitTests.Commands.Tool.Shared.Reporting;

[Trait("Feature", "tool-read"), Trait("Evidence", "Unit")]
public sealed class ToolReadIssueCodesTests
{
    [Fact(DisplayName = "Every Tool read situation has one distinct code and undefined situations are rejected")]
    public void CodesAreDistinctAndClosed()
    {
        var codes = Enum.GetValues<ToolReadIssueCode>();
        var suffixes = codes.Select(ToolReadIssueCodes.Suffix).ToArray();
        Assert.Equal(codes.Length, suffixes.Distinct(StringComparer.Ordinal).Count());
        Assert.All(suffixes, suffix => Assert.False(string.IsNullOrWhiteSpace(suffix)));
        Assert.Throws<ArgumentOutOfRangeException>(() => ToolReadIssueCodes.Suffix((ToolReadIssueCode)(-1)));
    }
}
