using TheLithium.Imprint;

namespace OpenForge.Cli.IntegrationTests.Commands.Extension.Create;

// Expand the owned coordinate before applying the diagnostic's promised bound.
// Normalizing after rendering cannot recover a path cut off midway by that bound.
// Received output remains untouched, including the exact truncation point.
internal sealed class ExtensionCreateDiagnosticSnapshotComparer(string cataloguePath) : ISnapshotComparer
{
    public SnapshotComparisonResult Compare(
        string expected, string received, SnapshotFormat format, ResolvedSnapshotComparison options)
    {
        var expanded = expected.Replace("<extension-source>", cataloguePath, StringComparison.Ordinal)
            .ReplaceLineEndings("\n");
        var bounded = string.Join('\n', expanded.Split('\n').Select(line =>
            line.Length > 240 ? line[..237] + "..." : line));
        return bounded == received.ReplaceLineEndings("\n")
            ? SnapshotComparisonResult.Match
            : new SnapshotComparisonResult(false,
                "Diagnostic differs from the fixture-expanded, 240-character-bounded expectation; see the unchanged received artifact.");
    }
}
