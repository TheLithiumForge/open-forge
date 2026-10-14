using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "nested-scope-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F05")]
public sealed class F05NestedScopeJourneyTests
{
    private const string Id = "guidance/team/new-topic/notes";
    private const string Leaf = ".agents/guidance/team/new-topic/notes.md";
    private const string Team = ".agents/guidance/team/_team.md";
    private const string Topic = ".agents/guidance/team/new-topic/_new-topic.md";
    private const string Body = "# Team notes\n\nKeep this authored body exactly.\n";
    private const string Patterns = ".agents/patterns/_patterns.md";
    private const string Readme = "# Outside\n\nThis authored README must survive route creation.\n";
    private const string ParentPrefix = "# Guidance\n\nKeep this authored parent prefix.\n\n## Axioms\n\n- Preserve team rules.\n\n## Entries\n\n";
    private const string ParentSuffix = "\n## Maintainer notes\n\nKeep this authored suffix, including café 👩‍💻.\n";

    [Fact(DisplayName = "F05 indexes a manually nested note then deliberately refines its tags")]
    [Trait("Scenarios", "X34,C05-14,C15-02,C02-14")]
    public async Task ManualScope()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f05-manual");
        journey.Expect(Team, Topic);
        journey.Write(Leaf, Body);
        var preview = await journey.TextAsync(0, true, "index", "--dry-run");
        AssertScopeText(preview.StandardOutput, true);
        using (var json = await journey.JsonAsync(0, true, "index", "--dry-run"))
        {
            Assert.Equal(2, json.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
            Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
            Assert.Equal([Team, Topic], json.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray().Select(row => row.GetProperty("path").GetString()).Order(StringComparer.Ordinal));
        }
        var applied = await journey.TextAsync(0, false, "index");
        AssertScopeText(applied.StandardOutput, false);
        journey.AssertBytes(Leaf, journey.Document("Team notes", ["Guidance"], Body));
        journey.AssertBytes(Team, journey.Scaffold("guidance/team", "Team", ["Guidance"], "- [Notes in guidance/team/new-topic](new-topic/_new-topic.md) - #Guidance"));
        journey.AssertBytes(Topic, journey.Scaffold("guidance/team/new-topic", "New topic", ["Guidance"], "- [Team notes](notes.md) - #Guidance"));
        await journey.TextAsync(0, false, "route", "update", Id, "--tag", "Guidance", "--tag", "Team");
        journey.AssertBytes(Leaf, journey.Document("Team notes", ["Guidance", "Team"], Body));
        var inspect = await journey.TextAsync(0, true, "route", "inspect", Id);
        Assert.Contains(Id, inspect.StandardOutput, StringComparison.Ordinal);
        var context = await journey.TextAsync(0, true, "context", Id);
        Assert.Contains(Body, context.StandardOutput, StringComparison.Ordinal);
        await journey.NoOpAsync("index");
        await journey.CleanDoctorAsync();
    }

    [Theory(DisplayName = "F05 retains explicit Route Create and Init alternatives")]
    [InlineData(false), InlineData(true)]
    [Trait("Scenarios", "C14-09,C13-01,C15-02,C15-06")]
    public async Task ExplicitCreation(bool initialize)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f05-cli");
        journey.Expect(Team, Topic, Leaf);
        journey.Write("README.md", Readme);
        var patterns = journey.Document("Patterns", ["Pattern"], $"# Patterns\n\n## Axioms\n\n{PublishedMaintenanceJourney.Inherited}\n\n## Entries\n\n- none - No entries - #Empty\n");
        journey.Write(Patterns, patterns);
        journey.Write(".agents/loader.md", "# Open Forge Loader\n\n## Axioms\n\n- Preserve authored content.\n\n## Entries\n\n"
            + "- [Guidance](guidance/_guidance.md) - #Guidance\n- [Memory](memory/_memory.md) - #Memory\n- [Patterns](patterns/_patterns.md) - #Pattern\n");
        journey.Write(PublishedMaintenanceJourney.Guidance, journey.Document("Guidance", ["Guidance"], ParentPrefix + "- none - No entries - #Empty\n" + ParentSuffix));
        var before = PublishedWorkspaceTreeSnapshot.Capture(journey.Path);
        string[] allowedChanges = [PublishedMaintenanceJourney.Guidance, ".agents/guidance/team", Team, ".agents/guidance/team/new-topic", Topic, Leaf];
        if (initialize) await journey.TextAsync(0, false, "route", "init", "guidance/team/new-topic");
        using (var preview = await journey.JsonAsync(0, true, "route", "create", Id, "--description", "Team topic notes", "--tag", "Guidance", "--dry-run"))
        {
            Assert.Equal("completed", preview.RootElement.GetProperty("status").GetString());
            Assert.Equal("dry-run", preview.RootElement.GetProperty("data").GetProperty("mode").GetString());
            Assert.Equal(Leaf, preview.RootElement.GetProperty("data").GetProperty("target").GetProperty("path").GetString());
            Assert.Equal(Topic, preview.RootElement.GetProperty("data").GetProperty("listedIn").GetString());
        }
        var create = await journey.TextAsync(0, false, "route", "create", Id, "--description", "Team topic notes", "--tag", "Guidance");
        Assert.Contains($"Created {Leaf}", create.StandardOutput, StringComparison.Ordinal);
        AssertExplicitBytes(journey, initialize, ["Guidance"]);
        PublishedJourneyAssertions.AssertOnlyFileMutations(before, PublishedWorkspaceTreeSnapshot.Capture(journey.Path), allowedChanges);
        await journey.TextAsync(0, false, "route", "update", Id, "--tag", "Guidance", "--tag", "Team");
        AssertExplicitBytes(journey, initialize, ["Guidance", "Team"]);
        var repeat = await journey.TextAsync(0, true, "route", "update", Id, "--tag", "Guidance", "--tag", "Team");
        Assert.Contains($"{Id} already has these values. Nothing to do.", repeat.StandardOutput, StringComparison.Ordinal);
        using (var repeated = await journey.JsonAsync(0, true, "route", "update", Id, "--tag", "Guidance", "--tag", "Team"))
        {
            Assert.Equal("completed", repeated.RootElement.GetProperty("status").GetString());
            Assert.Empty(repeated.RootElement.GetProperty("effects").EnumerateArray());
        }
        journey.AssertBytes("README.md", Readme);
        journey.AssertBytes(Patterns, patterns);
        Assert.Equal(before[Patterns], PublishedWorkspaceTreeSnapshot.Capture(journey.Path)[Patterns]);
        Assert.Equal(before["README.md"], PublishedWorkspaceTreeSnapshot.Capture(journey.Path)["README.md"]);
        PublishedJourneyAssertions.AssertOnlyFileMutations(before, PublishedWorkspaceTreeSnapshot.Capture(journey.Path), allowedChanges);
        await journey.TextAsync(0, false, "index");
        await journey.TextAsync(0, true, "context", Id);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    [Fact(DisplayName = "F05 refuses an unknown root without changing workspace or stores"), Trait("Scenarios", "C14-08")]
    public async Task UnknownRootCreateIsInvalidAndReadOnly()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f05-unknown-root");
        journey.Write("README.md", Readme);
        var result = await journey.TextAsync(4, true, "route", "create", "nonsense/team/notes", "--description", "Team topic notes", "--tag", "Guidance");
        Assert.DoesNotContain("route init", result.StandardError, StringComparison.Ordinal);
        Assert.False(Directory.Exists(journey.Combine(".agents/nonsense")));
        journey.AssertBytes("README.md", Readme);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    private static void AssertExplicitBytes(PublishedMaintenanceJourney journey, bool initialized, string[] tags)
    {
        string teamDescription = "guidance/team";
        string topicDescription = "guidance/team/new-topic";
        string categoryTags = "";
        if (initialized)
        {
            teamDescription = "Draft route for guidance/team; replace this description before relying on it for selection";
            topicDescription = "Draft route for guidance/team/new-topic; replace this description before relying on it for selection";
            categoryTags = " - #NeedsAuthoring";
        }
        journey.AssertBytes(PublishedMaintenanceJourney.Guidance,
            journey.Document("Guidance", ["Guidance"], ParentPrefix + $"- [{teamDescription}](team/_team.md){categoryTags}\n" + ParentSuffix));
        var teamBody = $"# guidance/team\n\n## Axioms\n\n{PublishedMaintenanceJourney.Inherited}\n\n## Entries\n\n- [{topicDescription}](new-topic/_new-topic.md){categoryTags}\n";
        var topicBody = $"# guidance/team/new-topic\n\n## Axioms\n\n{PublishedMaintenanceJourney.Inherited}\n\n## Entries\n\n- [Team topic notes](notes.md) - {string.Join(" ", tags.Select(tag => "#" + tag))}\n";
        if (initialized)
        {
            teamBody = journey.Document(teamDescription, ["NeedsAuthoring"], teamBody.Replace("# guidance/team\n\n", $"# team\n\n{teamDescription}.\n\n", StringComparison.Ordinal));
            topicBody = journey.Document(topicDescription, ["NeedsAuthoring"], topicBody.Replace("# guidance/team/new-topic\n\n", $"# new-topic\n\n{topicDescription}.\n\n", StringComparison.Ordinal));
        }
        else
        {
            teamBody = "---\nopen-forge: {}\n---\n\n" + teamBody;
            topicBody = "---\nopen-forge: {}\n---\n\n" + topicBody;
        }
        journey.AssertBytes(Team, teamBody);
        journey.AssertBytes(Topic, topicBody);
        journey.AssertBytes(Leaf, PublishedMaintenanceJourney.Header(journey.Form, "Team topic notes", tags).TrimEnd('\n') + "\n");
    }

    private static void AssertScopeText(string text, bool preview)
    {
        Assert.Contains(preview ? "Would create 2 entrypoints and complete metadata in 1 file." : "Created 2 entrypoints and completed metadata in 1 file.", text, StringComparison.Ordinal);
        Assert.Contains(preview ? "entrypoint would be created" : "entrypoint created", text, StringComparison.Ordinal);
    }
}
