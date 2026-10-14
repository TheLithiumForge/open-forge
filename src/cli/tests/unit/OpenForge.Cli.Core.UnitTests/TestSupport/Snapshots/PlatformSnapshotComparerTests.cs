using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.TestSupport.Snapshots;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Unit")]
public sealed class PlatformSnapshotComparerTests
{
    [Theory(DisplayName = "Platform snapshots adapt raw link evidence without hiding nonportable result targets")]
    [Trait("Boundary", "Output"), InlineData("linux"), InlineData("macos")]
    public void PortableTargetsStayExactWhileExpectedStateEvidenceAdapts(string platform)
    {
        const string windows = """
            {
              "effects": [
                { "kind": "link", "target": "..\\team\\kind.md" },
                { "target": "..\\team\\kind.md" }
              ],
              "expectedTarget": "..\\team\\kind.md",
              "expectedStates": [
                { "kind": "relative-file-link", "target": "..\\team\\kind.md" }
              ]
            }
            """;
        using var adapted = System.Text.Json.JsonDocument.Parse(PlatformSnapshotComparer.AdaptExpected(windows, platform, false));
        var root = adapted.RootElement;
        Assert.All(root.GetProperty("effects").EnumerateArray(), effect => Assert.Equal("..\\team\\kind.md", effect.GetProperty("target").GetString()));
        Assert.Equal("..\\team\\kind.md", root.GetProperty("expectedTarget").GetString());
        Assert.Equal("../team/kind.md", root.GetProperty("expectedStates")[0].GetProperty("target").GetString());
    }

    [Theory, Trait("Boundary", "Output")]
    [InlineData("linux"), InlineData("macos")]
    public void LinkTargetExpectationsAdaptWhileLiteralContentStaysExact(string platform)
    {
        const string windows = """
            {
              "observedTarget": "..\\team\\kind.md",
              "rawRelativeTarget": "..\\team\\kind.md",
              "literal": "literal\\marker"
            }

            """;
        const string unix = """
            {
              "observedTarget": "../team/kind.md",
              "rawRelativeTarget": "../team/kind.md",
              "literal": "literal\\marker"
            }

            """;
        Assert.Equal(windows, PlatformSnapshotComparer.AdaptExpected(windows, "windows", false));
        Assert.Equal(unix, PlatformSnapshotComparer.AdaptExpected(windows, platform, false));
        Assert.Equal("Observed target: ../team/kind.md\nliteral\\marker\n",
            PlatformSnapshotComparer.AdaptExpected("Observed target: ..\\team\\kind.md\nliteral\\marker\n", platform, false));
    }

    [Fact, Trait("Boundary", "Output")]
    public void ManifestSharingAdaptsOnlyTypedAdviceOnNonWindows()
    {
        const string windows = "manifest.json could not be read because it is in use.\nClose the program holding the file, then retry.\nRaw OS evidence.";
        const string unix = "manifest.json could not be read because a filesystem operation failed.\nCheck that the file is accessible, then retry.\nRaw OS evidence.";

        Assert.Equal(windows, PlatformSnapshotComparer.AdaptExpected(windows, "windows", false, manifestSharing: true));
        foreach (var platform in new[] { "linux", "macos" })
        {
            Assert.Equal(windows, PlatformSnapshotComparer.AdaptExpected(windows, platform, false));
            Assert.Equal(unix, PlatformSnapshotComparer.AdaptExpected(windows, platform, false, manifestSharing: true));
        }
    }

    [Fact, Trait("Boundary", "Output")]
    public void ExpectedPathsAdaptWithoutRewritingLiteralContent()
    {
        const string windows = """{"path":"<workspace>\\shared\\note.md","literal":"literal\\marker"}""";
        const string unix = """{"path":"<workspace>/shared/note.md","literal":"literal\\marker"}""";
        Assert.Equal(windows, PlatformSnapshotComparer.AdaptExpected(windows, "windows", false));
        Assert.Equal(unix, PlatformSnapshotComparer.AdaptExpected(windows, "linux", false));
        Assert.Equal(unix, PlatformSnapshotComparer.AdaptExpected(windows, "macos", false));
        const string literal = """<workspace>literal\\marker""";
        Assert.Equal(literal, PlatformSnapshotComparer.AdaptExpected(literal, "linux", false));
    }

    [Fact, Trait("Boundary", "Output")]
    public void OnlyHeldLockCapturesAdaptTheExactSharingError()
    {
        const string windows = "IOException (0x80070020): The filesystem operation failed.";
        Assert.Equal(windows, PlatformSnapshotComparer.AdaptExpected(windows, "linux", false));
        Assert.Equal("IOException (0x0000000B): The filesystem operation failed.", PlatformSnapshotComparer.AdaptExpected(windows, "linux", true));
        Assert.Equal("IOException (0x00000023): The filesystem operation failed.", PlatformSnapshotComparer.AdaptExpected(windows, "macos", true));
        const string unrelated = "IOException (0x80070005): The filesystem operation failed.";
        Assert.Equal(unrelated, PlatformSnapshotComparer.AdaptExpected(unrelated, "linux", true));
        Assert.Throws<ArgumentOutOfRangeException>(() => PlatformSnapshotComparer.AdaptExpected(windows, "unknown", true));
    }
}
