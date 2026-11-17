using OpenForge.Cli.Core.Framework.Tools.Shared.Paths;

namespace OpenForge.Cli.Core.UnitTests.Framework.Tools.Shared.Paths;

// The pure user Path edit. Values are synthetic entries joined with the owner's separator.
[Trait("Feature", "tool-path"), Trait("Evidence", "Unit")]
public sealed class ToolWindowsUserPathTests
{
    private const string Bin = @"C:\Users\synthetic\.open-forge\bin";
    private const string Expandable = @"%USERPROFILE%\synthetic-tools";
    private const string Other = @"C:\synthetic\other";

    private static string Join(params string[] entries) => string.Join(ToolWindowsUserPath.Separator, entries);

    [Fact(DisplayName = "Appending keeps unexpanded entries, order and duplicates and adds only the bin entry")]
    public void AppendKeepsEveryEntry()
    {
        var value = Join(Expandable, Other, Other);
        Assert.Equal(Join(Expandable, Other, Other, Bin), ToolWindowsUserPath.Append(value, Bin));
    }

    [Fact(DisplayName = "Appending after a trailing separator adds no empty entry")]
    public void AppendAfterTrailingSeparator()
        => Assert.Equal(Join(Expandable, Bin), ToolWindowsUserPath.Append(Join(Expandable, string.Empty), Bin));

    [Theory(DisplayName = "Appending to an empty or absent value writes only the bin entry")]
    [InlineData(null)]
    [InlineData("")]
    public void AppendToEmpty(string? value) => Assert.Equal(Bin, ToolWindowsUserPath.Append(value, Bin));

    [Fact(DisplayName = "Appending when the bin entry is present keeps the value unchanged")]
    public void AppendWhenPresent()
    {
        var value = Join(Bin, Expandable);
        Assert.Same(value, ToolWindowsUserPath.Append(value, Bin));
    }

    [Fact(DisplayName = "A value with a null character or beyond the variable limit is not edited")]
    public void UnsafeValuesAreRefused()
    {
        var oversized = new string('x', ToolWindowsUserPath.MaximumLength + 1);
        Assert.Null(ToolWindowsUserPath.Append(Join(Other, "\0"), Bin));
        Assert.Null(ToolWindowsUserPath.Append(oversized, Bin));
    }
}
