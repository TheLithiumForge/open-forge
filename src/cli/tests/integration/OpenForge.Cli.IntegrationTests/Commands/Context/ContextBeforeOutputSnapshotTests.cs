using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ContextBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Context output preserves selected content and leaves source bytes unchanged")]
    public async Task Selection()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in Scenarios())
        {
            using var workspace = new RoutedOutputWorkspace();
            string[] selection = situation switch
            {
                "startup" => [],
                "one-source" => ["docs/guide"],
                "additions-only" => ["docs/guide", "--additions-only"],
                "paths" => ["docs/guide", "--content", "paths"],
                "headings" => ["docs/guide", "--content", "headings"],
                "section" => ["docs/guide", "--content", "section:Rules"],
                "section-missing" => ["docs/guide", "--content", "section:Absent"],
                "follow-links" or "broken-followed-link" => ["docs/guide", "--follow-links", "1"],
                "unknown-source" => ["missing/source"],
                "unreadable-source" => ["docs/guide"],
                "invalid-content" => ["--content", "invalid"],
                "ambiguous-source" => ["docs/guide"],
                "frontmatter-missing-host" => ["--content", "frontmatter"],
                _ => throw new ArgumentOutOfRangeException(nameof(situation)),
            };
            if (situation == "ambiguous-source") workspace.CollidingGuideId();
            if (situation == "unreadable-source")
            {
                workspace.ReplaceBytes(".agents/docs/guide.md", [0xff, 0xfe, 0xfd]);
            }

            if (situation == "broken-followed-link")
            {
                workspace.Replace(".agents/docs/guide.md", "---\nopen-forge:\n  description: Guide\n  tags: [Docs, Guide]\n---\n# Guide\n\n[Missing](missing.md).\n");
            }

            var before = workspace.Snapshot();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = ["context", .. selection],
                ExitCode = exitCode,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.Snapshot());
        }

        Assert.Equal(
            14 * 8,
            snapshots.Keys.Count(key => !key.EndsWith(".diagnostics", StringComparison.Ordinal)));
        Assert.Contains(
            snapshots.Keys,
            key => key.EndsWith(".diagnostics", StringComparison.Ordinal));
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    private static IEnumerable<(string Situation, int ExitCode)> Scenarios()
        =>
        [
            ("startup", 0),
            ("one-source", 0),
            ("additions-only", 0),
            ("paths", 0),
            ("headings", 0),
            ("section", 2),
            ("section-missing", 2),
            ("follow-links", 0),
            ("broken-followed-link", 3),
            ("unknown-source", 4),
            ("unreadable-source", 3),
            ("invalid-content", 4),
            ("ambiguous-source", 5),
            ("frontmatter-missing-host", 0),
        ];
}
