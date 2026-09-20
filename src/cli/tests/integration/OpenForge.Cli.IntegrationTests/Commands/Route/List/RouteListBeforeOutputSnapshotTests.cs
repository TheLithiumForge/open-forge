using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class RouteListBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Fact(DisplayName = "Route list output preserves selected depth and observed route conditions")]
    public async Task Routes()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var (situation, exitCode) in new[]
        {
            ("roots-depth-1", 0),
            ("subtree", 0),
            ("depth-all", 0),
            ("depth-0", 0),
            ("empty-subtree", 0),
            ("unknown-source", 4),
            ("invalid-depth", 4),
            ("ambiguous-source", 5),
            ("metadata-missing", 2),
            ("unreadable-entrypoint", 3),
            ("loader-malformed", 5),
        })
        {
            using var workspace = new RoutedOutputWorkspace();
            string[] selection = situation switch
            {
                "roots-depth-1" => [],
                "subtree" => ["docs"],
                "depth-all" => ["--depth", "all"],
                "depth-0" => ["--depth", "0"],
                "empty-subtree" => ["docs/reference"],
                "unknown-source" => ["missing/source"],
                "invalid-depth" => ["--depth", "-1"],
                "ambiguous-source" => ["docs"],
                "metadata-missing" => ["--depth", "all"],
                "unreadable-entrypoint" or "loader-malformed" => [],
                _ => throw new ArgumentOutOfRangeException(nameof(situation)),
            };
            switch (situation)
            {
                case "ambiguous-source":
                    workspace.AmbiguousRoot();
                    break;
                case "metadata-missing":
                    workspace.Replace(".agents/docs/guide.md", "# Guide without metadata\n");
                    break;
                case "unreadable-entrypoint":
                    workspace.ReplaceBytes(".agents/docs/_docs.md", [0xff, 0xfe, 0xfd]);
                    break;
                case "loader-malformed":
                    workspace.Replace(".agents/loader.md", "# Loader\n\n## Entries\n\n## Entries\n");
                    break;
            }

            var before = workspace.Snapshot();
            await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
            {
                Situation = situation,
                Arguments = ["route", "list", .. selection],
                ExitCode = exitCode,
            }, snapshotCollector: snapshots);
            Assert.Equal(before, workspace.Snapshot());
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }
}
