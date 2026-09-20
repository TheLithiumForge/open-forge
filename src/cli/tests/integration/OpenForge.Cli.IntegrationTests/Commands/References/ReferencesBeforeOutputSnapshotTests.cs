using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.References;

[Trait("Feature", "command-output-snapshots"), Trait("Evidence", "Integration")]
public sealed class ReferencesBeforeOutputSnapshotTests
{
    [Trait("Boundary", "Output")]
    [Theory(DisplayName = "References output preserves each direction and leaves source bytes unchanged")]
    [InlineData("links-both", 0)]
    [InlineData("no-authored-links", 0)]
    [InlineData("out-only", 0)]
    [InlineData("in-only-with-include", 0)]
    [InlineData("broken-outgoing", 2)]
    [InlineData("external-outgoing", 0)]
    [InlineData("unknown-source", 4)]
    [InlineData("ambiguous-source", 5)]
    [InlineData("invalid-direction", 4)]
    [InlineData("include-with-out-only", 4)]
    [InlineData("unreadable-source", 3)]
    public async Task Direction(string situation, int exitCode)
    {
        using var workspace = new RoutedOutputWorkspace();
        string[] selection = situation switch
        {
            "links-both" or "ambiguous-source" => ["docs/guide"],
            "no-authored-links" => ["docs/reference", "--direction", "out"],
            "out-only" => ["docs/guide", "--direction", "out"],
            "in-only-with-include" => ["docs/reference", "--direction", "in", "--include", "docs/guide"],
            "broken-outgoing" or "external-outgoing" or "unreadable-source" => ["docs/guide"],
            "unknown-source" => ["missing/source"],
            "invalid-direction" => ["docs/guide", "--direction", "invalid"],
            "include-with-out-only" => ["docs/guide", "--direction", "out", "--include", "docs/reference"],
            _ => throw new ArgumentOutOfRangeException(nameof(situation)),
        };
        if (situation == "ambiguous-source") workspace.CollidingGuideId();
        if (situation is "broken-outgoing" or "external-outgoing")
        {
            var target = situation == "broken-outgoing" ? "missing.md" : "https://example.com/guide";
            workspace.Replace(".agents/docs/guide.md", $"---\nopen-forge:\n  description: Guide\n  tags: [Docs, Guide]\n---\n# Guide\n\n[Linked]({target}).\n");
        }

        if (situation == "unreadable-source")
        {
            workspace.ReplaceBytes(".agents/docs/guide.md", [0xff, 0xfe, 0xfd]);
        }

        var before = workspace.Snapshot();
        await new ReadCommandOutputCapture(workspace.Path).MatchDetailsAsync(new ReadOutputScenario
        {
            Situation = situation,
            Arguments = ["references", .. selection],
            ExitCode = exitCode,
        }, testName: $"{nameof(Direction)}_{situation}");
        Assert.Equal(before, workspace.Snapshot());
    }
}
