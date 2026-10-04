using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedSelectionViewportProcessTests
{
    private const string Controls = "up/down move  enter next  esc cancel";

    [Theory(DisplayName = "Published Extension wizard replaces bounded views and retains the plan before authority"), Trait("Feature", "cli-interaction"), Trait("Evidence", "EndToEnd")]
    [InlineData(80, 24, false, true)]
    [InlineData(40, 12, false, true)]
    [InlineData(80, 24, true, false)]
    public async Task TerminalSelectionViewport(short width, short height, bool resize, bool apply)
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("Selection viewport terminal evidence requires Windows ConPTY.");
        using var consumer = PublishedJourneyWorkspace.Create("e2e-selection-viewport-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-selection-viewport-catalogue");
        consumer.ExpectCoreInstall();
        consumer.ExpectFiles("docs/selected.md", ".agents/guidance/runtime.md", ".agents/open-forge.json");
        var installed = await consumer.RunAsync("install", "--automatic", "--format", "json");
        Assert.Equal(0, installed.ExitCode);
        for (var index = 1; index <= 25; index++) WritePackage(catalogue, index);
        var before = consumer.SnapshotState();
        var sourceBefore = catalogue.SnapshotState();
        var steps = new List<PublishedTerminalStep>();
        if (resize)
        {
            steps.Add(new(Controls, "", new(40, 12)));
            steps.Add(new(Controls, "", new(80, 24)));
        }
        steps.Add(new(Controls, " "));
        steps.Add(new(Controls, "\u001b[B"));
        steps.Add(new(Controls, " "));
        steps.Add(new(Controls, string.Concat(Enumerable.Repeat("\u001b[B", 23))));
        steps.Add(new("Choice 25/25", "\u001b[B\r"));
        for (var page = 0; page < 4; page++) steps.Add(new("pgup/pgdn review", "\u001b[6~"));
        steps.Add(new("pgup/pgdn review", "2"));
        steps.Add(new("Apply these changes? [y/N]", apply ? "y" : "n"));

        var terminal = await PublishedWindowsTerminal.RunAsync(consumer.Target, consumer.Path,
            ["extension", "install", "--source", catalogue.Path], consumer.ProcessEnvironment,
            new PublishedTerminalScenario { Size = new(width, height), TerminalName = "xterm", Steps = steps });

        Assert.Equal(apply ? 0 : 130, terminal.ExitCode);
        Assert.Contains("Choice 25/25", terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains("[+]", terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains("Writes outside .agents:", terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains("docs/selected.md", terminal.Transcript, StringComparison.Ordinal);
        var plan = terminal.Transcript.IndexOf("Would install", StringComparison.Ordinal);
        var confirmation = terminal.Transcript.IndexOf("Apply these changes? [y/N]", StringComparison.Ordinal);
        Assert.True(plan >= 0 && confirmation > plan, terminal.Transcript);
        var afterPlan = terminal.Transcript[plan..];
        Assert.DoesNotContain("\u001b[2J", afterPlan, StringComparison.Ordinal);
        Assert.Equal(sourceBefore, catalogue.SnapshotState());
        if (apply)
        {
            Assert.True(File.Exists(consumer.Combine("docs/selected.md")));
            Assert.True(File.Exists(consumer.Combine(".agents/guidance/runtime.md")));
            using var ownership = JsonDocument.Parse(File.ReadAllText(consumer.Combine(".agents/open-forge.lock.json")));
            Assert.Equal(2, ownership.RootElement.GetProperty("extensions").GetArrayLength());
            Assert.False(File.Exists(consumer.Combine(".agents/open-forge.json")));
        }
        else
        {
            Assert.Equal(before, consumer.SnapshotState());
            consumer.LockStore.AssertNoRecoveryArtifacts(consumer.Path);
        }
    }

    [Theory(DisplayName = "Published noninteractive and dry-run reports contain no terminal controls"), Trait("Feature", "cli-interaction"), Trait("Evidence", "EndToEnd")]
    [InlineData("automatic")]
    [InlineData("json")]
    [InlineData("dry-run")]
    [InlineData("redirected")]
    public async Task NoninteractiveReportsRemainPlain(string scenario)
    {
        using var consumer = PublishedJourneyWorkspace.Create("e2e-selection-plain-consumer");
        using var catalogue = PublishedJourneyWorkspace.Create("e2e-selection-plain-catalogue");
        consumer.ExpectCoreInstall();
        Assert.Equal(0, (await consumer.RunAsync("install", "--automatic", "--format", "json")).ExitCode);
        WritePackage(catalogue, 2);
        var before = consumer.SnapshotState();
        var arguments = new List<string> { "extension", "install", "package-02", "--source", catalogue.Path };
        if (scenario != "redirected") arguments.Add("--dry-run");
        if (scenario == "automatic") arguments.Add("--automatic");
        if (scenario == "json") arguments.AddRange(["--format", "json"]);

        var result = await consumer.RunAsync(arguments.ToArray());

        Assert.Equal(scenario == "redirected" ? 4 : 0, result.ExitCode);
        Assert.DoesNotContain('\u001b', result.StandardOutput);
        Assert.DoesNotContain('\u001b', result.StandardError);
        Assert.Equal(before, consumer.SnapshotState());
        if (scenario == "redirected")
        {
            Assert.Empty(result.StandardOutput);
            Assert.Contains("Extension install needs confirmation, and this session cannot ask.", result.StandardError, StringComparison.Ordinal);
        }
        if (scenario == "json")
        {
            using var document = JsonDocument.Parse(result.StandardOutput);
            Assert.Equal("extension install", document.RootElement.GetProperty("command").GetString());
            Assert.Empty(result.StandardError);
        }
    }

    private static void WritePackage(PublishedJourneyWorkspace catalogue, int index)
    {
        var id = $"package-{index:00}";
        if (index == 20) id += "-with-a-long-label-and-distinct-suffix";
        var dependencies = index == 1 ? "[\"package-02\"]" : "[]";
        catalogue.WriteText($"{id}/extension.json", $$"""
            {
              "id": "{{id}}",
              "name": "{{id}}",
              "description": "Long package description {{index}}. {{new string('d', 120)}}",
              "version": "1.0.0",
              "dependencies": {{dependencies}}
            }
            """
        );
        var path = index == 1 ? "docs/selected.md" : $".agents/guidance/package-{index:00}.md";
        if (index == 2) path = ".agents/guidance/runtime.md";
        catalogue.WriteText($"{id}/content/{path}", "---\nopen-forge:\n  description: Package fixture\n  tags: [Guidance]\n---\n# Package\n");
    }
}
