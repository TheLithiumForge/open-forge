using System.Text;
using System.Text.Json.Nodes;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F28SwitchFrontmatterFormJourneyTests
{
    private const string NotePath = ".agents/guidance/team-note.md";
    private const string GuidancePath = ".agents/guidance/_guidance.md";
    private const string EditedPath = ".agents/patterns/_patterns.md";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string NoteEntry = "- [Team notes for reviews](team-note.md) - #Guidance #Team";

    [Fact(DisplayName = "F28 C03-14 C01-13 C03-16 C03-17 C03-18 X31 C05-12 C04-12 switches forms while preserving edits and user notes"),
     Trait("Feature", "frontmatter-form"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F28")]
    public async Task SwitchFormsPreservesEditsAndUserNotesUntilExplicitUpdate()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f28-switch-frontmatter");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(NotePath);
        var sources = JourneyPayloadAssertions.CanonicalSources("collaboration");
        workspace.ExpectFiles(sources.Keys.ToArray());

        var install = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Scoped, "--automatic"));
        JourneyPayloadAssertions.AssertCompleted(install);
        Assert.Contains("Frontmatter: scoped", install.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Scoped);
        JourneyPayloadAssertions.AssertDelivered(workspace, JourneyPayloadAssertions.CanonicalSources(), JourneyFrontmatterForm.Scoped);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, JourneyPayloadAssertions.CanonicalSources());

        var settings = Assert.IsType<JsonObject>(JsonNode.Parse(File.ReadAllBytes(workspace.Combine(PublishedInstallWorkspace.SettingsPath))));
        Assert.True(settings.Remove("frontmatter"));
        workspace.WriteText(PublishedInstallWorkspace.SettingsPath, settings.ToJsonString());
        var legacySettings = File.ReadAllBytes(workspace.Combine(PublishedInstallWorkspace.SettingsPath));

        var extension = await workspace.RunAsync("extension", "install", "collaboration", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(extension);
        Assert.Contains("collaboration", extension.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Scoped);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, sources, GuidancePath, ".agents/templates/_templates.md");
        string[] guidanceWithExtension =
        [
            "- [Explore ideas, match the depth to the decision, integrate accepted outcomes, and offer useful independent review](adaptive-collaboration.md) - #Extension #Guidance #Collaboration #Ideation #Decision #Convergence #Review #Experience",
        ];
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, guidanceWithExtension);
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/templates/_templates.md",
            "- [Explore a choice before committing to a direction](collaboration/_collaboration.md) - #Extension #Template #Collaboration");

        var note = await workspace.RunAsync("route", "create", NotePath,
            "--description", "Team notes for reviews", "--tag", "Guidance", "--tag", "Team");
        JourneyPayloadAssertions.AssertCompleted(note);
        Assert.Contains(NotePath, note.StandardOutput, StringComparison.Ordinal);
        Assert.StartsWith("---\nopen-forge:\n", File.ReadAllText(workspace.Combine(NotePath)), StringComparison.Ordinal);
        var noteBytes = File.ReadAllBytes(workspace.Combine(NotePath));
        var expectedEntries = guidanceWithExtension.Append(NoteEntry).Order(StringComparer.Ordinal).ToArray();
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, expectedEntries);

        workspace.WriteText(EditedPath, File.ReadAllText(workspace.Combine(EditedPath)) + "\nKeep this authored paragraph through Configure.\n");
        var editedBytes = File.ReadAllBytes(workspace.Combine(EditedPath));
        var ownershipBytes = File.ReadAllBytes(workspace.Combine(OwnershipPath));

        var status = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "status");
        JourneyPayloadAssertions.AssertCompleted(status, exitCode: 2);
        Assert.Contains("changed", status.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("open-forge update", status.StandardOutput, StringComparison.Ordinal);

        var invalid = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace,
            JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--automatic"));
        Assert.Equal(4, invalid.ExitCode);
        Assert.Empty(invalid.StandardOutput);
        Assert.Contains("--configure", invalid.StandardError, StringComparison.Ordinal);

        var preview = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace,
            JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--configure", "--dry-run", "--detail", "standard"));
        JourneyPayloadAssertions.AssertCompleted(preview);
        Assert.Contains("Frontmatter: scoped -> root", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Kept 1 file", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(EditedPath, preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("edited", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("metadata would move to root keys", preview.StandardOutput, StringComparison.Ordinal);
        Assert.DoesNotContain("would be replaced", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", preview.StandardOutput, StringComparison.Ordinal);

        var configured = await workspace.RunAsync(
            JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, "--configure", "--automatic"));
        JourneyPayloadAssertions.AssertCompleted(configured);
        Assert.Contains("Frontmatter: scoped -> root", configured.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Kept 1 file", configured.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Root);
        Assert.Equal(editedBytes, File.ReadAllBytes(workspace.Combine(EditedPath)));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        Assert.Equal(ownershipBytes, File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Root, EditedPath, NotePath);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, sources, GuidancePath, ".agents/templates/_templates.md");
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, expectedEntries);

        var beforeIndex = workspace.SnapshotState();
        var index = await workspace.RunAsync("index");
        JourneyPayloadAssertions.AssertCompleted(index);
        PublishedJourneyAssertions.AssertOnlyFileMutations(beforeIndex, workspace.SnapshotState(), GuidancePath);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, expectedEntries);
        Assert.Equal(editedBytes, File.ReadAllBytes(workspace.Combine(EditedPath)));
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));

        var changed = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "status");
        JourneyPayloadAssertions.AssertCompleted(changed, exitCode: 2);
        Assert.Contains("changed", changed.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("open-forge update", changed.StandardOutput, StringComparison.Ordinal);

        var update = await workspace.RunAsync("update", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(update);
        Assert.Contains("Updated", update.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Root, NotePath);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, expectedEntries);
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Root);

        var scoped = await workspace.RunAsync(
            JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Scoped, "--configure", "--automatic"));
        JourneyPayloadAssertions.AssertCompleted(scoped);
        Assert.Contains("Frontmatter: root -> scoped", scoped.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Scoped);
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Scoped, NotePath);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, sources, GuidancePath, ".agents/templates/_templates.md");
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, expectedEntries);
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));

        var repeat = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace,
            JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Scoped, "--configure", "--automatic"));
        JourneyPayloadAssertions.AssertCompleted(repeat);
        Assert.Contains("Nothing to do", repeat.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Scoped, NotePath);
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        Assert.NotEqual(legacySettings, File.ReadAllBytes(workspace.Combine(PublishedInstallWorkspace.SettingsPath)));
    }
}
