using System.Collections.ObjectModel;
using System.Security.Cryptography;
using System.Text;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F04IndependentTemplateCopyJourneyTests
{
    private const string TemplatePath = ".agents/templates/example.md";
    private const string TemplatesEntrypointPath = ".agents/templates/_templates.md";
    private const string TargetId = "memory/working/new-note";
    private const string TargetPath = ".agents/memory/working/new-note.md";
    private const string WorkingEntrypointPath = ".agents/memory/working/_working.md";
    private const string TemplateV1 =
        "---\n"
        + "open-forge:\n"
        + "  description: Reusable team notes\n"
        + "  tags: [Template]\n"
        + "---\n\n"
        + "# Team note\n\n"
        + "Replace {team-name} before review.\n\n"
        + "## Optional context\n\n"
        + "Remove this section after copying.\n";
    private const string TemplateV2 =
        "---\n"
        + "open-forge:\n"
        + "  description: Reusable team notes\n"
        + "  tags: [Template]\n"
        + "---\n\n"
        + "# Team note\n\n"
        + "Template version 2 is intentionally different.\n\n"
        + "## Optional context\n\n"
        + "This changed only in the Template.\n";
    private const string AuthoredCopyBody =
        "# Team note\n\n"
        + "Replace Team North with a real owner.\n";
    private const string ExistingDifferentTarget =
        "---\n"
        + "open-forge:\n"
        + "  description: Existing authored note\n"
        + "  tags: [Existing]\n"
        + "---\n\n"
        + "# Existing note\n\n"
        + "Keep this ordinary target exactly.\n";

    [Fact(
        DisplayName = "F04 copies a Template, keeps the authored document independent, and protects its body on update"),
        Trait("Feature", "independent-template-copy-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F04")]
    public async Task TemplateCopyJourneyRemainsIndependentAndPreservesAuthoredBody()
    {
        using var workspace = CreateInstalledWorkspace("e2e-f04-main", TemplatePath, TargetPath);
        await InstallW1Async(workspace);
        workspace.WriteText(TemplatePath, TemplateV1);
        var templateV1Bytes = Encoding.UTF8.GetBytes(TemplateV1);

        var beforeTemplateIndex = workspace.SnapshotState();
        var templateIndex = await workspace.RunAsync("index");

        Assert.Equal(0, templateIndex.ExitCode);
        Assert.Equal(string.Empty, templateIndex.StandardError);
        Assert.Equal(templateV1Bytes, File.ReadAllBytes(workspace.Combine(TemplatePath)));
        var afterTemplateIndex = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeTemplateIndex, afterTemplateIndex, TemplatesEntrypointPath);
        var templatesEntrypoint = File.ReadAllText(workspace.Combine(TemplatesEntrypointPath), Encoding.UTF8);
        Assert.Contains("example", templatesEntrypoint, StringComparison.Ordinal);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        var beforeCreate = workspace.SnapshotState();
        var create = await workspace.RunAsync(
            "route", "create", TargetId,
            "--description", "Scenario for adding team notes",
            "--tag=Scenario",
            "--template", "templates/example");

        Assert.Equal(0, create.ExitCode);
        Assert.Equal(string.Empty, create.StandardError);
        Assert.NotEmpty(create.StandardOutput);
        AssertOrdinaryFile(workspace.Combine(TargetPath));
        var createdDocument = File.ReadAllText(workspace.Combine(TargetPath), Encoding.UTF8);
        Assert.Contains("description: Scenario for adding team notes", createdDocument, StringComparison.Ordinal);
        Assert.Contains("tags: [Scenario]", createdDocument, StringComparison.Ordinal);
        Assert.DoesNotContain("tags: [Template]", createdDocument, StringComparison.Ordinal);
        Assert.DoesNotContain("description: Reusable team notes", createdDocument, StringComparison.Ordinal);
        Assert.DoesNotContain("templates/example", createdDocument, StringComparison.Ordinal);
        Assert.Contains("Replace {team-name} before review.", createdDocument, StringComparison.Ordinal);
        Assert.Equal(TemplateV1[(TemplateV1.IndexOf("\n---\n", 4, StringComparison.Ordinal) + 5)..], ExtractBody(createdDocument));
        var afterCreate = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeCreate, afterCreate, TargetPath, WorkingEntrypointPath);
        Assert.NotEqual(beforeCreate[WorkingEntrypointPath], afterCreate[WorkingEntrypointPath]);
        Assert.Contains("new-note", File.ReadAllText(workspace.Combine(WorkingEntrypointPath), Encoding.UTF8), StringComparison.Ordinal);
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);

        var authoredCopy = ReplaceBody(createdDocument, AuthoredCopyBody);
        workspace.WriteText(TargetPath, authoredCopy);
        var authoredCopyBytes = Encoding.UTF8.GetBytes(authoredCopy);
        Assert.Equal(AuthoredCopyBody, ExtractBody(File.ReadAllText(workspace.Combine(TargetPath), Encoding.UTF8)));
        Assert.DoesNotContain("Optional context", authoredCopy, StringComparison.Ordinal);

        var beforeTemplateEdit = workspace.SnapshotState();
        workspace.WriteText(TemplatePath, TemplateV2);
        Assert.Equal(Encoding.UTF8.GetBytes(TemplateV2), File.ReadAllBytes(workspace.Combine(TemplatePath)));
        Assert.Equal(authoredCopyBytes, File.ReadAllBytes(workspace.Combine(TargetPath)));
        var afterTemplateEdit = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeTemplateEdit, afterTemplateEdit, TemplatePath);

        var beforeUpdate = workspace.SnapshotState();
        var update = await workspace.RunAsync(
            "route", "update", TargetId,
            "--description", "Accepted review scenario",
            "--template", "templates/example");

        Assert.Equal(2, update.ExitCode);
        Assert.Equal(string.Empty, update.StandardError);
        Assert.Contains("template", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("body", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("not copied", update.StandardOutput, StringComparison.OrdinalIgnoreCase);
        var updatedDocument = File.ReadAllText(workspace.Combine(TargetPath), Encoding.UTF8);
        Assert.Contains("description: Accepted review scenario", updatedDocument, StringComparison.Ordinal);
        Assert.Contains("tags: [Scenario]", updatedDocument, StringComparison.Ordinal);
        Assert.DoesNotContain("tags: [Template]", updatedDocument, StringComparison.Ordinal);
        Assert.Equal(AuthoredCopyBody, ExtractBody(updatedDocument));
        Assert.Equal(Encoding.UTF8.GetBytes(TemplateV2), File.ReadAllBytes(workspace.Combine(TemplatePath)));
        var afterUpdate = workspace.SnapshotState();
        AssertOnlyPathsChanged(beforeUpdate, afterUpdate, TargetPath, WorkingEntrypointPath);
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(
        DisplayName = "F04 unknown Template is invalid input and leaves the destination and navigation untouched"),
        Trait("Feature", "independent-template-copy-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F04")]
    public async Task UnknownTemplateRefusesCreationWithoutEffects()
    {
        using var workspace = CreateInstalledWorkspace("e2e-f04-unknown-template", TargetPath);
        await InstallW1Async(workspace);
        var before = SnapshotState(workspace);

        var result = await RunWithoutWritesAsync(
            workspace,
            "route", "create", TargetId,
            "--description", "Scenario for adding team notes",
            "--tag=Scenario",
            "--template", "templates/does-not-exist");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("does-not-exist", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("template", result.StandardError, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("Created ", result.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, SnapshotState(workspace));
        Assert.False(File.Exists(workspace.Combine(TargetPath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    [Fact(
        DisplayName = "F04 create refuses an existing different target and preserves its authored bytes"),
        Trait("Feature", "independent-template-copy-journey"),
        Trait("Evidence", "EndToEnd"),
        Trait("Journey", "F04")]
    public async Task ExistingDifferentTargetBlocksReplacementWithoutEffects()
    {
        using var workspace = CreateInstalledWorkspace("e2e-f04-existing-target", TargetPath);
        await InstallW1Async(workspace);
        workspace.WriteText(TargetPath, ExistingDifferentTarget);
        var before = SnapshotState(workspace);

        var result = await RunWithoutWritesAsync(
            workspace,
            "route", "create", TargetId,
            "--description", "Scenario for adding team notes",
            "--tag=Scenario");

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.NotEmpty(result.StandardError);
        Assert.Contains("new-note", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Created ", result.StandardError, StringComparison.Ordinal);
        Assert.Equal(before, SnapshotState(workspace));
        Assert.Equal(
            Encoding.UTF8.GetBytes(ExistingDifferentTarget),
            File.ReadAllBytes(workspace.Combine(TargetPath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);
    }

    private static PublishedJourneyWorkspace CreateInstalledWorkspace(
        string purpose,
        params string[] reservedFiles)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(reservedFiles);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task InstallW1Async(PublishedJourneyWorkspace workspace)
    {
        var install = await workspace.RunAsync("install", "--automatic");

        Assert.Equal(0, install.ExitCode);
        Assert.Equal(string.Empty, install.StandardError);
        Assert.True(File.Exists(workspace.Combine("AGENTS.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/memory/working/_working.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/templates/_templates.md")));
        Assert.True(File.Exists(workspace.Combine(".agents/open-forge.lock.json")));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
    }

    private static Task<ProcessRunResult> RunWithoutWritesAsync(
        PublishedJourneyWorkspace workspace,
        params string[] arguments)
        => PublishedJourneyProcess.RunWithoutWritesAsync(
            workspace.Target,
            workspace.Path,
            () => SnapshotState(workspace),
            arguments,
            workspace.ProcessEnvironment);

    private static IReadOnlyDictionary<string, string> SnapshotState(PublishedJourneyWorkspace workspace)
    {
        var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var entry in workspace.SnapshotState())
        {
            state[$"workspace/{entry.Key}"] = entry.Value;
        }

        AddDirectorySnapshot(
            state,
            "data-home",
            workspace.LockStore.LocalApplicationDataDirectory);
        AddDirectorySnapshot(
            state,
            "recovery-workspace",
            workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path));
        return new ReadOnlyDictionary<string, string>(state);
    }

    private static void AddDirectorySnapshot(
        IDictionary<string, string> state,
        string prefix,
        string path)
    {
        if (!Directory.Exists(path))
        {
            state[prefix] = File.Exists(path)
                ? FileState(path)
                : "absent";
            return;
        }

        foreach (var entry in PublishedWorkspaceTreeSnapshot.Capture(path))
        {
            state[$"{prefix}/{entry.Key}"] = entry.Value;
        }
    }

    private static string FileState(string path)
        => $"file:{Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(path)))}";

    private static string ExtractBody(string document)
    {
        var closingMarker = document.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        Assert.True(closingMarker >= 0, "The copied destination did not retain a readable metadata boundary.");
        return document[(closingMarker + 5)..];
    }

    private static string ReplaceBody(string document, string body)
    {
        var closingMarker = document.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        Assert.True(closingMarker >= 0, "The created destination did not retain a readable metadata boundary.");
        return document[..(closingMarker + 5)] + body;
    }

    private static void AssertOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        Assert.Equal(
            (FileAttributes)0,
            attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
    }

    private static void AssertOnlyPathsChanged(
        IReadOnlyDictionary<string, string> before,
        IReadOnlyDictionary<string, string> after,
        params string[] allowedPaths)
        => PublishedJourneyAssertions.AssertOnlyFileMutations(before, after, allowedPaths);
}
