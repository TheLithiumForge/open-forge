using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

[Trait("Feature", "deep-plain-notes-journey"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F30")]
public sealed class F30DeepPlainNotesJourneyTests
{
    private const string Id = "memory/crystallized/platform/payments/stripe/node";
    private const string Node = ".agents/memory/crystallized/platform/payments/stripe/node.md";
    private static readonly string[] Entrypoints =
    [
        ".agents/memory/crystallized/platform/_platform.md",
        ".agents/memory/crystallized/platform/payments/_payments.md",
        ".agents/memory/crystallized/platform/payments/stripe/_stripe.md",
    ];

    [Theory(DisplayName = "F30 completes all three missing scopes with exact workspace-form bytes and repeats without writes")]
    [InlineData(JourneyFrontmatterForm.Root, "heading"), InlineData(JourneyFrontmatterForm.Scoped, "heading")]
    [InlineData(JourneyFrontmatterForm.Root, "filename"), InlineData(JourneyFrontmatterForm.Scoped, "filename")]
    [InlineData(JourneyFrontmatterForm.Root, "partial"), InlineData(JourneyFrontmatterForm.Scoped, "partial")]
    [Trait("Scenarios", "X34,X32,X33,C02-14,C05-14")]
    public async Task DeepChain(JourneyFrontmatterForm form, string noteKind)
    {
        using var journey = await PublishedMaintenanceJourney.CreateAsync("f30-deep", form);
        journey.Expect(Entrypoints);
        var body = noteKind == "filename" ? "Keep the headingless authored body.\n" : "# Stripe notes\n\nKeep café, indentation and the final newline.\n";
        var description = noteKind == "filename" ? "Node" : "Stripe notes";
        var authored = body;
        var expected = journey.Document(description, ["Memory", "Crystallized"], body);
        if (noteKind == "partial")
        {
            var scope = form == JourneyFrontmatterForm.Scoped ? "open-forge:\n  " : "";
            authored = $"---\n{scope}description: Authored stripe notes # keep\ncustom: kept\n---\n\n{body}";
            description = "Authored stripe notes";
            var indent = form == JourneyFrontmatterForm.Scoped ? "  " : "";
            expected = authored.Replace($"{indent}description:", $"{indent}tags: [Memory, Crystallized]\n{indent}description:", StringComparison.Ordinal);
        }
        journey.Write(Node, authored);
        journey.Write(".agents/memory/crystallized/empty/deep/resource.txt", "A resource alone does not need a route.\n");
        var diagnosis = await journey.TextAsync(2, true, "doctor");
        AssertDeepText(diagnosis.StandardOutput);
        using (var doctor = await journey.JsonAsync(2, true, "doctor"))
        {
            Assert.Equal($"open-forge index {journey.WorkspaceFlag}", PublishedMaintenanceJourney.Next(doctor));
            Assert.Contains(doctor.RootElement.GetProperty("findings").EnumerateArray(), finding => finding.GetProperty("code").GetString() == "route.unreachable");
        }
        var preview = await journey.TextAsync(0, true, "index", "--dry-run");
        Assert.Contains("Would create 3 entrypoints and complete metadata in 1 file.", preview.StandardOutput, StringComparison.Ordinal);
        using (var json = await journey.JsonAsync(0, true, "index", "--dry-run")) AssertCompletion(json, noteKind, "planned", description);
        Assert.All(Entrypoints, path => Assert.False(File.Exists(journey.Combine(path))));
        journey.AssertBytes(Node, authored);
        using (var applied = await journey.JsonAsync(0, false, "index")) AssertCompletion(applied, noteKind, "done", description);
        journey.AssertBytes(Node, expected);
        journey.AssertBytes(Entrypoints[0], journey.Scaffold("memory/crystallized/platform", "Platform", ["Memory", "Crystallized"], "- [Notes in memory/crystallized/platform/payments](payments/_payments.md) - #Memory #Crystallized"));
        journey.AssertBytes(Entrypoints[1], journey.Scaffold("memory/crystallized/platform/payments", "Payments", ["Memory", "Crystallized"], "- [Notes in memory/crystallized/platform/payments/stripe](stripe/_stripe.md) - #Memory #Crystallized"));
        journey.AssertBytes(Entrypoints[2], journey.Scaffold("memory/crystallized/platform/payments/stripe", "Stripe", ["Memory", "Crystallized"], $"- [{description}](node.md) - #Memory #Crystallized"));
        Assert.Contains("- [Notes in memory/crystallized/platform](platform/_platform.md) - #Memory #Crystallized\n", File.ReadAllText(journey.Combine(PublishedMaintenanceJourney.Crystallized)), StringComparison.Ordinal);
        Assert.False(File.Exists(journey.Combine(".agents/memory/crystallized/empty/_empty.md")));
        Assert.False(File.Exists(journey.Combine(".agents/memory/crystallized/empty/deep/_deep.md")));
        var context = await journey.TextAsync(0, true, "context", Id);
        Assert.Contains(body, context.StandardOutput, StringComparison.Ordinal);
        await journey.CleanDoctorAsync();
        await journey.NoOpAsync("index");
    }

    private static void AssertCompletion(JsonDocument json, string kind, string outcome, string description)
    {
        var data = json.RootElement.GetProperty("data");
        Assert.Equal(Entrypoints.Order(StringComparer.Ordinal), data.GetProperty("entrypoints").EnumerateArray().Select(row => row.GetProperty("path").GetString()).Order(StringComparer.Ordinal));
        Assert.All(data.GetProperty("entrypoints").EnumerateArray(), row => Assert.Equal(outcome, row.GetProperty("outcome").GetString()));
        var metadata = Assert.Single(data.GetProperty("metadata").EnumerateArray());
        Assert.Equal(Node, metadata.GetProperty("path").GetString());
        Assert.Equal(kind == "partial" ? ["tags"] : new[] { "description", "tags" }, metadata.GetProperty("fields").EnumerateArray().Select(field => field.GetString()));
        Assert.Equal(kind == "partial" ? null : description, metadata.GetProperty("description").GetString());
        Assert.Equal(["Memory", "Crystallized"], metadata.GetProperty("tags").EnumerateArray().Select(tag => tag.GetString()));
        Assert.Equal(outcome, metadata.GetProperty("outcome").GetString());
        Assert.Equal(3, json.RootElement.GetProperty("counts").GetProperty("entrypointsCreated").GetInt32());
        Assert.Equal(1, json.RootElement.GetProperty("counts").GetProperty("metadataCompleted").GetInt32());
        Assert.Equal(0, json.RootElement.GetProperty("counts").GetProperty("filesSkipped").GetInt32());
        var change = Assert.Single(data.GetProperty("changes").EnumerateArray());
        Assert.Equal(PublishedMaintenanceJourney.Crystallized, change.GetProperty("path").GetString());
        Assert.Equal(0, change.GetProperty("before").GetInt32());
        Assert.Equal(1, change.GetProperty("after").GetInt32());
    }

    private static void AssertDeepText(string text)
        => Assert.Contains("is inside folders without navigation files. Index can create the missing chain.", text, StringComparison.Ordinal);
}
