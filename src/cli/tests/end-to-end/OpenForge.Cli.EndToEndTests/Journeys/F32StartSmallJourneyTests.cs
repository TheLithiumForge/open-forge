using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F32StartSmallJourneyTests
{
    private const string LoaderPath = ".agents/loader.md";
    private const string EmptyEntries = "- none - No entries - #Empty";
    private const string ContributingLink = "- [Contribution rules for every change](../CONTRIBUTING.md) - #LoadNow";
    private const string CSharpLink = "- [C# conventions to follow](../docs/csharp.md) - #LoadNow - applies to `**/*.cs`";

    [Fact(DisplayName = "F32 an existing repository starts with Minimal, links its own documents, grows, and keeps what it turned off"),
     Trait("Feature", "minimal-install"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F32")]
    public async Task ExistingRepositoryStartsSmallAndKeepsItsChoices()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f32-start-small");
        workspace.WriteText("README.md", "# Existing project\n\nKeep this README exactly.\n");
        workspace.WriteText("CONTRIBUTING.md", "# Contributing\n\nRun the tests before every commit.\n");
        workspace.WriteText("docs/csharp.md", "# C# conventions\n\nUse file-scoped namespaces.\n");
        var documents = new[] { "README.md", "CONTRIBUTING.md", "docs/csharp.md" }
            .ToDictionary(path => path, path => File.ReadAllBytes(workspace.Combine(path)));
        workspace.ExpectFiles("AGENTS.md", "CLAUDE.md", LoaderPath, ".agents/README.md", ".agents/open-forge.json", ".agents/open-forge.lock.json");

        var install = await workspace.RunAsync("install", "--preset", "minimal", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(install);
        Assert.Contains("Setup: Minimal", install.StandardOutput, StringComparison.Ordinal);
        Assert.False(Directory.Exists(workspace.Combine(".agents/memory")));
        Assert.False(Directory.Exists(workspace.Combine(".agents/directives")));
        Assert.Equal([EmptyEntries], JourneyPayloadAssertions.Entries(ReadLoader(workspace)));

        var loader = ReadLoader(workspace).Replace(EmptyEntries, $"{ContributingLink}\n{CSharpLink}", StringComparison.Ordinal);
        workspace.WriteText(LoaderPath, loader);
        foreach (var command in new[] { "index", "update" })
        {
            var maintenance = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, command);
            JourneyPayloadAssertions.AssertCompleted(maintenance);
        }

        var pending = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "context");
        Assert.Contains("CONTRIBUTING.md", pending.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("Use file-scoped namespaces.", pending.StandardOutput, StringComparison.Ordinal);
        var forCode = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "context", "--for", "src/App.cs");
        JourneyPayloadAssertions.AssertCompleted(forCode);
        Assert.Contains("Run the tests before every commit.", forCode.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Use file-scoped namespaces.", forCode.StandardOutput, StringComparison.Ordinal);

        workspace.ExpectFiles(".agents/directives/_directives.md", ".agents/memory/_memory.md", ".agents/memory/crystallized/_crystallized.md");
        var grow = await workspace.RunAsync("install", "--configure", "--preset", "custom",
            "--route", "directives=add", "--route", "memory/crystallized=add", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(grow);
        var grown = JourneyPayloadAssertions.Entries(ReadLoader(workspace));
        Assert.Equal(4, grown.Length);
        Assert.EndsWith("](directives/_directives.md) - #LoadNow #Core #Directive", grown[0], StringComparison.Ordinal);
        Assert.EndsWith("](memory/_memory.md) - #LoadNow #Memory", grown[1], StringComparison.Ordinal);
        Assert.Equal([ContributingLink, CSharpLink], grown[2..]);

        workspace.WriteText(LoaderPath, ReadLoader(workspace).Replace(grown[0] + "\n", string.Empty, StringComparison.Ordinal));
        var turnedOff = File.ReadAllBytes(workspace.Combine(LoaderPath));
        foreach (var command in new[] { new[] { "update" }, ["index"], ["install", "--automatic"] })
        {
            var maintenance = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, command);
            JourneyPayloadAssertions.AssertCompleted(maintenance);
            Assert.Equal(turnedOff, File.ReadAllBytes(workspace.Combine(LoaderPath)));
        }

        var doctor = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "doctor");
        JourneyPayloadAssertions.AssertCompleted(doctor);
        Assert.Contains("No problems found.", doctor.StandardOutput, StringComparison.Ordinal);
        foreach (var (path, bytes) in documents)
        {
            Assert.Equal(bytes, File.ReadAllBytes(workspace.Combine(path)));
        }
    }

    private static string ReadLoader(PublishedJourneyWorkspace workspace)
        => File.ReadAllText(workspace.Combine(LoaderPath), Encoding.UTF8);
}
