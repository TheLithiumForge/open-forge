using OpenForge.Cli.Composition.Shared.Tools;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Composition.Shared.Tools;

[Trait("Feature", "tool-target"), Trait("Evidence", "Integration")]
public sealed class ToolTargetObserverTests
{
    [Theory(DisplayName = "Native version output keeps only its leading numeric release")]
    [InlineData("6.1.0-26-amd64", "6.1.0")]
    [InlineData("13.5", "13.5")]
    [InlineData("2.28 (vendor)", "2.28")]
    [InlineData("10.0.26100", "10.0.26100")]
    [InlineData("", null)]
    [InlineData("unknown", null)]
    [InlineData(null, null)]
    public void ReadsNumericVersion(string? value, string? expected)
        => Assert.Equal(expected, ToolTargetObserver.NumericVersion(value));

    [Fact(DisplayName = "An unselected newer library file doesn't satisfy the active C++ runtime floor")]
    public void IgnoresUnselectedLibrary()
    {
        using var temporary = TemporaryWorkspace.Create("tool-target");
        temporary.CreateFile($"{ToolTargetObserver.LibStdCppName}.0.99", "synthetic unselected library");
        Assert.Null(ToolTargetObserver.LibStdCpp([temporary.Path]));
    }
}
