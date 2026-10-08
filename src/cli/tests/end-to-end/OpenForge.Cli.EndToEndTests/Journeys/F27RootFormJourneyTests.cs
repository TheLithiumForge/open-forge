using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F27RootFormJourneyTests
{
    private const string NotePath = ".agents/guidance/review-checklist.md";
    private const string GuidancePath = ".agents/guidance/_guidance.md";
    private const string NoteDescription = "Review checklist for pull requests";
    private const string NoteEntry = "- [Review checklist for pull requests](review-checklist.md) - #Guidance #Review";
    private const string UpdatedNoteEntry = "- [Review checklist for pull requests](review-checklist.md) - #Checklist";

    [Fact(DisplayName = "F27 C03-02 C03-13 X31 C01-02 C14-15 C09-13 C08-15 C15-14 C21-19 C02-12 C03-03 C05-12 starts in root form and keeps working in it"),
     Trait("Feature", "frontmatter-form"), Trait("Evidence", "EndToEnd"), Trait("Journey", "F27")]
    public async Task DefaultRootInstallCarriesStateThroughRoutingAndExtensionDelivery()
    {
        using var workspace = PublishedJourneyWorkspace.Create("f27-root-form");
        workspace.WriteText("README.md", "# Root-form workspace\n\nKeep this authored README exactly.\n");
        var readmeBytes = File.ReadAllBytes(workspace.Combine("README.md"));
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(NotePath);
        var sources = JourneyPayloadAssertions.CanonicalSources();

        var preview = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "install", "--dry-run");
        JourneyPayloadAssertions.AssertCompleted(preview);
        Assert.Contains("Would install the Open Forge Framework", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Frontmatter: root", preview.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("No files were changed.", preview.StandardOutput, StringComparison.Ordinal);
        workspace.LockStore.AssertNoInfrastructure();

        var install = await workspace.RunAsync("install", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(install);
        Assert.Contains("Installed the Open Forge Framework", install.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("Frontmatter: root", install.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Root);
        JourneyPayloadAssertions.AssertDelivered(workspace, sources, JourneyFrontmatterForm.Root);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, sources);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, "- none - No entries - #Empty");
        Assert.Equal(readmeBytes, File.ReadAllBytes(workspace.Combine("README.md")));

        var status = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "status");
        JourneyPayloadAssertions.AssertCompleted(status);
        Assert.Contains("Open Forge is installed and current.", status.StandardOutput, StringComparison.Ordinal);

        var beforeCreate = workspace.SnapshotState();
        var create = await workspace.RunAsync("route", "create", NotePath,
            "--description", NoteDescription, "--tag", "Guidance", "--tag", "Review");
        JourneyPayloadAssertions.AssertCompleted(create);
        Assert.Contains(NotePath, create.StandardOutput, StringComparison.Ordinal);
        PublishedJourneyAssertions.AssertOnlyFileMutations(beforeCreate, workspace.SnapshotState(), NotePath, GuidancePath);
        var createdText = File.ReadAllText(workspace.Combine(NotePath), Encoding.UTF8);
        Assert.StartsWith("---\ndescription: Review checklist for pull requests\n", createdText, StringComparison.Ordinal);
        Assert.Contains("tags: [Guidance, Review]\n", createdText, StringComparison.Ordinal);
        Assert.DoesNotContain("open-forge:", createdText, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, NoteEntry);

        var sharedText = createdText.Insert("---\n".Length, "sidebar_position: 3\n");
        workspace.WriteText(NotePath, sharedText);
        Assert.Equal(Encoding.UTF8.GetBytes(sharedText), File.ReadAllBytes(workspace.Combine(NotePath)));

        var find = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "find", "--tag", "Review");
        JourneyPayloadAssertions.AssertCompleted(find);
        Assert.Contains("guidance/review-checklist", find.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(NotePath, find.StandardOutput, StringComparison.Ordinal);

        var context = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "context", "guidance/review-checklist");
        JourneyPayloadAssertions.AssertCompleted(context);
        Assert.Contains(NotePath, context.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(sharedText.TrimEnd('\n'), context.StandardOutput, StringComparison.Ordinal);

        var beforeUpdate = workspace.SnapshotState();
        var routeUpdate = await workspace.RunAsync("route", "update", "guidance/review-checklist",
            "--responsibility", "Define the pull request review checklist", "--tag", "Checklist");
        JourneyPayloadAssertions.AssertCompleted(routeUpdate);
        Assert.Contains("Updated guidance/review-checklist", routeUpdate.StandardOutput, StringComparison.Ordinal);
        PublishedJourneyAssertions.AssertOnlyFileMutations(beforeUpdate, workspace.SnapshotState(), NotePath, GuidancePath);
        var updatedText = File.ReadAllText(workspace.Combine(NotePath), Encoding.UTF8);
        Assert.StartsWith("---\nsidebar_position: 3\n", updatedText, StringComparison.Ordinal);
        Assert.Contains("description: Review checklist for pull requests\n", updatedText, StringComparison.Ordinal);
        Assert.Contains("responsibility: Define the pull request review checklist\n", updatedText, StringComparison.Ordinal);
        Assert.Contains("tags: [Checklist]\n", updatedText, StringComparison.Ordinal);
        Assert.DoesNotContain("open-forge:", updatedText, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, UpdatedNoteEntry);
        var noteBytes = File.ReadAllBytes(workspace.Combine(NotePath));

        var extensionSources = JourneyPayloadAssertions.CanonicalSources("planning", "workflows");
        workspace.ExpectFiles(extensionSources.Keys.ToArray());
        var extension = await workspace.RunAsync("extension", "install", "planning", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(extension);
        Assert.Contains("planning", extension.StandardOutput, StringComparison.Ordinal);
        Assert.Contains("workflows", extension.StandardOutput, StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertDelivered(workspace, extensionSources, JourneyFrontmatterForm.Root, NotePath);
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, UpdatedNoteEntry);
        JourneyPayloadAssertions.AssertCanonicalEntries(workspace, extensionSources,
            GuidancePath, ".agents/templates/_templates.md", ".agents/patterns/_patterns.md", ".agents/skills/_skills.md",
            ".agents/memory/crystallized/_crystallized.md", ".agents/memory/emerging/_emerging.md", ".agents/memory/working/_working.md",
            ".agents/skills/use-workflow/references/_references.md");
        var loaderEntries = JourneyPayloadAssertions.Entries(File.ReadAllText(workspace.Combine(".agents/loader.md")));
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/patterns/_patterns.md",
            "- [Keep task outcomes, planned steps, and current state predictable without duplicating project knowledge](work-records.md) - #Extension #Pattern #Planning #Task #Memory");
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/skills/use-workflow/references/_references.md",
            "- [Turn an accepted outcome into ordered work with clear dependencies and evidence](planning/_planning.md) - #Extension #Workflow #Planning");
        var skillEntries = JourneyPayloadAssertions.Entries(File.ReadAllText(workspace.Combine(".agents/skills/_skills.md")));
        Assert.Equal(2, skillEntries.Length);
        Assert.Equal(JourneyPayloadAssertions.Entries(File.ReadAllText(sources[".agents/skills/_skills.md"]))[0], skillEntries[0]);
        Assert.EndsWith("](use-workflow/SKILL.md) - #Skill", skillEntries[1], StringComparison.Ordinal);
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/templates/_templates.md",
            "- [Choose a starter for a possibility, investigation, accepted choice, or active work](planning/_planning.md) - #Extension #Template #Planning #Memory",
            "- [Create a method for the Use Workflow Skill without defining a new harness capability](workflows/_workflows.md) - #Extension #Template #Workflow");
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/memory/crystallized/_crystallized.md",
            "- [What was chosen, why, and what follows from the choice](decisions/_decisions.md) - #Extension #Memory #Decision #CurrentTruth");
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/memory/emerging/_emerging.md",
            "- [Structured reasoning, investigation, or comparison that is useful but not accepted truth](analysis/_analysis.md) - #Extension #Memory #Analysis #Contextual #Candidate",
            "- [Future possibilities, experiments, open questions, and options to explore later](ideas/_ideas.md) - #Extension #Memory #Idea #Exploration #Contextual #Candidate");
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/memory/working/_working.md",
            "- [Current state, current step, and next steps for one active workstream](checkpoints/_checkpoints.md) - #Extension #Memory #Working #Checkpoint #Contextual");

        var doctor = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "doctor");
        JourneyPayloadAssertions.AssertCompleted(doctor);
        Assert.Contains("No problems found.", doctor.StandardOutput, StringComparison.Ordinal);

        var repeat = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "install", "--automatic");
        JourneyPayloadAssertions.AssertCompleted(repeat);
        Assert.Contains("Open Forge is already installed and current. Nothing to do.", repeat.StandardOutput, StringComparison.Ordinal);

        var index = await JourneyPayloadAssertions.RunWithoutWritesAsync(workspace, "index");
        JourneyPayloadAssertions.AssertCompleted(index);
        Assert.Contains("current", index.StandardOutput, StringComparison.OrdinalIgnoreCase);
        JourneyPayloadAssertions.AssertEntries(workspace, GuidancePath, UpdatedNoteEntry);
        JourneyPayloadAssertions.AssertEntries(workspace, ".agents/loader.md", loaderEntries);
        JourneyPayloadAssertions.AssertDelivered(workspace, extensionSources, JourneyFrontmatterForm.Root, NotePath);
        JourneyPayloadAssertions.AssertForm(workspace, JourneyFrontmatterForm.Root);
        Assert.Equal(noteBytes, File.ReadAllBytes(workspace.Combine(NotePath)));
        Assert.Equal(readmeBytes, File.ReadAllBytes(workspace.Combine("README.md")));
    }
}
