using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;
using TheLithium.Imprint;

namespace OpenForge.Cli.IntegrationTests.TestSupport.Snapshots;

[Trait("Feature", "snapshot-comparison"), Trait("Evidence", "Integration")]
public sealed class CommandOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshots reject a missing baseline during ordinary verification")]
    public void MissingBaselineFailsWithoutCreatingIt()
    {
        Assert.NotEqual("1", Environment.GetEnvironmentVariable(CommandOutputSnapshot.UpdateVariable));
        using var workspace = TemporaryWorkspace.Create("snapshot-missing");
        var source = workspace.CreateFile("Capture.cs", "// Owned snapshot location.\n");
        try
        {
            var failure = Assert.Throws<SnapshotMismatchException>(() =>
                CommandOutputSnapshot.MatchSnapshot("Actual output\n", "missing", source, "Capture"));
            Assert.Contains("missing", failure.Message, StringComparison.OrdinalIgnoreCase);
            Assert.Empty(Directory.GetFiles(workspace.Path, "missing.txt", SearchOption.AllDirectories));
        }
        finally
        {
            DeleteSnapshotArtifacts(workspace);
        }
    }

    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Command snapshot mismatch shows the changed lines and preserves the expected file")]
    public void ChangedExpectedOutputFailsWithReadableDifference()
    {
        Assert.NotEqual("1", Environment.GetEnvironmentVariable(CommandOutputSnapshot.UpdateVariable));
        using var workspace = TemporaryWorkspace.Create("snapshot-mismatch");
        var source = workspace.CreateFile("Capture.cs", "// Owned snapshot location.\n");
        var baselineDirectory = workspace.Combine("__snapshots__", "Capture", "changed");
        Directory.CreateDirectory(baselineDirectory);
        var baseline = Path.Combine(baselineDirectory, "changed.txt");
        const string expected = "First line\nExpected output\n";
        File.WriteAllText(baseline, expected);
        try
        {
            var failure = Assert.Throws<SnapshotMismatchException>(() =>
                CommandOutputSnapshot.MatchSnapshot("First line\nChanged output\n", "changed", source, "Capture"));
            Assert.Contains("Expected output", failure.Message, StringComparison.Ordinal);
            Assert.Contains("Changed output", failure.Message, StringComparison.Ordinal);
            Assert.Equal(expected, File.ReadAllText(baseline));
        }
        finally
        {
            DeleteSnapshotArtifacts(workspace);
        }
    }

    private static void DeleteSnapshotArtifacts(TemporaryWorkspace workspace)
    {
        foreach (var relative in new[] { "__snapshots__", "artifacts" })
        {
            var path = workspace.Combine(relative);
            if (Directory.Exists(path))
            {
                Directory.Delete(path, recursive: true);
            }
        }
    }
}
