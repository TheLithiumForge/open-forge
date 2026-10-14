using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "plain-note-indexing-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F03")]
public sealed class F03PlainNoteJourneyTests
{
    private const string Note = ".agents/guidance/my-note.md";
    private const string Body = "# Team notes\r\n\r\nKeep café and 👩‍💻 exactly.\r\n\r\n- Check the result.\r\n";

    [Theory(DisplayName = "F03 previews and completes plain notes then preserves a hand-edited description")]
    [InlineData(JourneyFrontmatterForm.Root), InlineData(JourneyFrontmatterForm.Scoped)]
    [Trait("Scenarios", "X02,X04,C02-13,C05-13")]
    public async Task PlainNote(JourneyFrontmatterForm form)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f03-plain", form);
        journey.Write(Note, Body);
        var expected = PublishedMaintenanceJourney.Header(form, "Team notes", ["Guidance"], "\r\n") + Body;
        var preview = await journey.TextAsync(0, true, "index", "--dry-run");
        AssertCompletionText(preview.StandardOutput, preview: true);
        using (var json = await journey.JsonAsync(0, true, "index", "--dry-run")) AssertCompletion(json, "planned", ["description", "tags"], "Team notes", ["Guidance"]);
        journey.AssertBytes(Note, Body);

        using (var applied = await journey.JsonAsync(0, false, "index"))
        {
            AssertCompletion(applied, "done", ["description", "tags"], "Team notes", ["Guidance"]);
            Assert.Equal($"open-forge doctor {journey.WorkspaceFlag}", PublishedMaintenanceJourney.Next(applied));
        }
        journey.AssertBytes(Note, expected);
        Assert.Contains("- [Team notes](my-note.md) - #Guidance\n", File.ReadAllText(journey.Combine(PublishedMaintenanceJourney.Guidance)), StringComparison.Ordinal);
        var context = await journey.TextAsync(0, true, "context", "guidance/my-note");
        Assert.Contains(Body, context.StandardOutput, StringComparison.Ordinal);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");

        var edited = expected.Replace("description: Team notes", "description: Maintainer description # keep this comment", StringComparison.Ordinal);
        journey.Write(Note, edited);
        await journey.TextAsync(0, false, "index");
        journey.AssertBytes(Note, edited);
        Assert.Contains("[Maintainer description](my-note.md)", File.ReadAllText(journey.Combine(PublishedMaintenanceJourney.Guidance)), StringComparison.Ordinal);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    [Theory(DisplayName = "F03 completes partial headers in their existing form and derives headingless descriptions")]
    [InlineData(JourneyFrontmatterForm.Root, false), InlineData(JourneyFrontmatterForm.Scoped, false)]
    [InlineData(JourneyFrontmatterForm.Root, true), InlineData(JourneyFrontmatterForm.Scoped, true)]
    [Trait("Scenarios", "X03,X32,X33,C05-13")]
    public async Task PartialAndHeadingless(JourneyFrontmatterForm form, bool otherForm)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f03-partial", form);
        var authoredForm = otherForm ? (form == JourneyFrontmatterForm.Root ? JourneyFrontmatterForm.Scoped : JourneyFrontmatterForm.Root) : form;
        var scope = authoredForm == JourneyFrontmatterForm.Scoped ? "open-forge:\n" : "";
        var indent = authoredForm == JourneyFrontmatterForm.Scoped ? "  " : "";
        const string plain = "Keep the headingless body exactly.\n";
        const string partialPath = ".agents/guidance/partial.md";
        const string tagsPath = ".agents/guidance/tags-only.md";
        const string fallbackPath = ".agents/guidance/moneyRounding.md";
        var partial = $"---\n{scope}{indent}description: 'Authored description' # keep\ncustom: preserve-me\n---\n\n# Partial\n\nKeep body.\n";
        var tags = $"---\n{scope}{indent}tags: [Guidance, Team] # keep\ncustom: preserve-me\n---\n\n# Tags only\n\nKeep body.\n";
        journey.Write(partialPath, partial);
        journey.Write(tagsPath, tags);
        journey.Write(fallbackPath, plain);
        await journey.TextAsync(0, true, "index", "--dry-run");
        using (var preview = await journey.JsonAsync(0, true, "index", "--dry-run"))
        {
            Assert.Equal(3, preview.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
            var rows = preview.RootElement.GetProperty("data").GetProperty("metadata").EnumerateArray().ToArray();
            var descriptionRow = Assert.Single(rows, row => row.GetProperty("path").GetString() == partialPath);
            Assert.Equal(["tags"], descriptionRow.GetProperty("fields").EnumerateArray().Select(field => field.GetString()));
            Assert.Equal(JsonValueKind.Null, descriptionRow.GetProperty("description").ValueKind);
            var tagsRow = Assert.Single(rows, row => row.GetProperty("path").GetString() == tagsPath);
            Assert.Equal(["description"], tagsRow.GetProperty("fields").EnumerateArray().Select(field => field.GetString()));
            Assert.Equal("Tags only", tagsRow.GetProperty("description").GetString());
            Assert.Equal(JsonValueKind.Null, tagsRow.GetProperty("tags").ValueKind);
        }
        var applied = await journey.TextAsync(0, false, "index");
        Assert.Contains("Completed metadata in 3 files.", applied.StandardOutput, StringComparison.Ordinal);
        journey.AssertBytes(partialPath, partial.Replace($"{indent}description:", $"{indent}tags: [Guidance]\n{indent}description:", StringComparison.Ordinal));
        journey.AssertBytes(tagsPath, tags.Replace($"{indent}tags:", $"{indent}description: Tags only\n{indent}tags:", StringComparison.Ordinal));
        journey.AssertBytes(fallbackPath, journey.Document("Money rounding", ["Guidance"], plain));
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    [Fact(DisplayName = "F03 skips malformed metadata while completing its sibling and follows the named edit")]
    [Trait("Scenarios", "X05,X44")]
    public async Task MalformedSibling()
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f03-malformed");
        const string badPath = ".agents/guidance/bad.md";
        const string bad = "---\nopen-forge:\n  description: [unclosed\n";
        journey.Write(badPath, bad);
        journey.Write(Note, Body);
        await journey.TextAsync(3, true, "index", "--dry-run");
        using (var apply = await journey.JsonAsync(3, false, "index"))
        {
            var finding = Assert.Single(apply.RootElement.GetProperty("findings").EnumerateArray());
            Assert.Equal("index.metadata-skipped", finding.GetProperty("code").GetString());
            Assert.Equal($"{badPath} has invalid frontmatter. It was left unchanged. Other files were indexed.", finding.GetProperty("message").GetString());
            Assert.Equal(1, apply.RootElement.GetProperty("counts").GetProperty("filesSkipped").GetInt32());
        }
        journey.AssertBytes(badPath, bad);
        journey.AssertBytes(Note, PublishedMaintenanceJourney.Header(journey.Form, "Team notes", ["Guidance"], "\r\n") + Body);
        using (var diagnosis = await journey.JsonAsync(2, true, "doctor"))
            Assert.Equal($"Fix the frontmatter in {badPath}, then run open-forge index {journey.WorkspaceFlag}.", PublishedMaintenanceJourney.Next(diagnosis));
        journey.Write(badPath, "# Corrected\n\nThe author closed the malformed header by replacing it.\n");
        await journey.TextAsync(0, false, "index");
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    private static void AssertCompletion(JsonDocument json, string outcome, string[] fields, string? description, string[]? tags)
    {
        var row = Assert.Single(json.RootElement.GetProperty("data").GetProperty("metadata").EnumerateArray());
        Assert.Equal(Note, row.GetProperty("path").GetString());
        Assert.Equal(fields, row.GetProperty("fields").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(description, row.GetProperty("description").GetString());
        if (tags is null) Assert.Equal(JsonValueKind.Null, row.GetProperty("tags").ValueKind);
        else Assert.Equal(tags, row.GetProperty("tags").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(outcome, row.GetProperty("outcome").GetString());
        Assert.Empty(json.RootElement.GetProperty("data").GetProperty("entrypoints").EnumerateArray());
        Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("filesSkipped").GetInt32());
    }

    [Fact(DisplayName = "F03 distinguishes real read denial and finishes after the file becomes readable")]
    [Trait("Scenarios", "X05")]
    public async Task ReadDenial()
    {
        if (!OperatingSystem.IsWindows()) Assert.Skip("F03 X05 read-denial evidence requires Windows file-sharing semantics.");
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f03-read-denial");
        journey.Write(Note, Body);
        using (var locked = new FileStream(journey.Combine(Note), FileMode.Open, FileAccess.Read, FileShare.None))
        {
            var index = await journey.TextAsync(3, false, "index");
            Assert.Contains("read", index.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("index.metadata-skipped", index.StandardOutput, StringComparison.Ordinal);
            var doctor = await journey.TextAsync(3, false, "doctor");
            Assert.Contains("read", doctor.StandardOutput, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("has invalid frontmatter", doctor.StandardOutput, StringComparison.Ordinal);
        }
        journey.AssertBytes(Note, Body);
        await journey.TextAsync(0, false, "index");
        journey.AssertBytes(Note, PublishedMaintenanceJourney.Header(journey.Form, "Team notes", ["Guidance"], "\r\n") + Body);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    private static void AssertCompletionText(string text, bool preview)
    {
        Assert.Contains(preview ? "Would complete metadata in 1 file." : "Completed metadata in 1 file.", text, StringComparison.Ordinal);
        Assert.Contains(preview ? "metadata would be completed, your content would be kept" : "metadata completed, your content was kept", text, StringComparison.Ordinal);
        if (preview) Assert.Contains("No files were changed.", text, StringComparison.Ordinal);
    }

    [Theory(DisplayName = "F03 completes a flow mapping without rewriting its supplied bytes")]
    [InlineData(JourneyFrontmatterForm.Root), InlineData(JourneyFrontmatterForm.Scoped)]
    [Trait("Scenarios", "X03,X32")]
    public async Task FlowMapping(JourneyFrontmatterForm form)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f03-flow", form);
        const string body = "# Flow note\n\nKeep the flow-mapping body exactly.\n";
        var authored = form == JourneyFrontmatterForm.Root
            ? "---\n{description: 'Authored flow', custom: kept} # keep\n---\n\n" + body
            : "---\nopen-forge: {tags: [Guidance, Team]} # keep\ncustom: kept\n---\n\n" + body;
        var expected = form == JourneyFrontmatterForm.Root
            ? "---\n{tags: [Guidance], description: 'Authored flow', custom: kept} # keep\n---\n\n" + body
            : "---\nopen-forge: {description: \"Flow note\", tags: [Guidance, Team]} # keep\ncustom: kept\n---\n\n" + body;
        journey.Write(Note, authored);
        await journey.TextAsync(0, true, "index", "--dry-run");
        using (var preview = await journey.JsonAsync(0, true, "index", "--dry-run"))
            AssertCompletion(preview, "planned", form == JourneyFrontmatterForm.Root ? ["tags"] : ["description"], form == JourneyFrontmatterForm.Root ? null : "Flow note", form == JourneyFrontmatterForm.Root ? ["Guidance"] : null);
        var applied = await journey.TextAsync(0, false, "index");
        AssertCompletionText(applied.StandardOutput, false);
        journey.AssertBytes(Note, expected);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }
}
