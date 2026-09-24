using System.IO.Compression;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed class F26BundledExtensionsJourneyTests
{
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string LifecyclePath = ".agents/open-forge.lifecycle.json";
    private const string UserNotePath = ".agents/guidance/user-note.md";
    private const string AdaptiveCollaborationPath = ".agents/guidance/adaptive-collaboration.md";
    private const string CollaborationEntrypointPath = ".agents/templates/collaboration/_collaboration.md";
    private const string TemplatesEntrypointPath = ".agents/templates/_templates.md";
    private const string PlanningEntrypointPath = ".agents/templates/planning/_planning.md";
    private const string PlanningPlanPath = ".agents/templates/planning/plan.md";
    private const string PlanningTaskPath = ".agents/templates/planning/task.md";
    private const string WorkflowReferencesEntrypointPath = ".agents/skills/use-workflow/references/_references.md";
    private const string WorkflowEntrypointPath = ".agents/templates/workflows/_workflows.md";

    private const string UserNoteBody =
        "\n# User Note\n\nF26 user-authored bytes remain after package removal.\n";

    private static readonly string[] CollaborationPayloadPaths =
    [
        ".agents/guidance/adaptive-collaboration.md",
        ".agents/templates/collaboration/_collaboration.md",
        ".agents/templates/collaboration/brainstorming.md",
    ];

    private static readonly string[] PlanningPayloadPaths =
    [
        ".agents/memory/crystallized/decisions/_decisions.md",
        ".agents/memory/emerging/analysis/_analysis.md",
        ".agents/memory/emerging/ideas/_ideas.md",
        ".agents/memory/working/checkpoints/_checkpoints.md",
        ".agents/patterns/work-records.md",
        ".agents/skills/use-workflow/references/planning/_planning.md",
        ".agents/skills/use-workflow/references/planning/planning.md",
        ".agents/templates/planning/_planning.md",
        ".agents/templates/planning/analysis.md",
        ".agents/templates/planning/backlog.md",
        ".agents/templates/planning/checkpoint.md",
        ".agents/templates/planning/decision.md",
        ".agents/templates/planning/idea.md",
        ".agents/templates/planning/plan.md",
        ".agents/templates/planning/task.md",
    ];

    private static readonly string[] WorkflowsPayloadPaths =
    [
        ".agents/skills/use-workflow/references/_references.md",
        ".agents/skills/use-workflow/SKILL.md",
        ".agents/templates/workflows/_workflows.md",
        ".agents/templates/workflows/workflow.md",
    ];

    private static readonly string[] PlanningTemplateEntryDestinations =
    [
        "analysis.md",
        "backlog.md",
        "checkpoint.md",
        "decision.md",
        "idea.md",
        "plan.md",
        "task.md",
    ];

    [Fact(DisplayName = "F26 selects bundled Collaboration and Planning content and removes only Collaboration")]
    [Trait("Feature", "f26-bundled-extensions-journey")]
    [Trait("Evidence", "EndToEnd")]
    [Trait("Journey", "F26")]
    public async Task SelectOnlyNeededExtensionsPreservesPlanningAndUserContent()
    {
        using var workspace = PublishedJourneyWorkspace.Create("e2e-f26-bundled-extensions-journey");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(
            CollaborationPayloadPaths
                .Concat(PlanningPayloadPaths)
                .Concat(WorkflowsPayloadPaths)
                .ToArray());

        var install = await workspace.RunAsync("install", "--automatic");
        AssertSuccessfulHumanResult(install);
        var baselineLockProperties = CaptureNonExtensionRootProperties(workspace);

        var userNote = OpenForgeDocumentSeed.Metadata(
            description: "F26 user note",
            tags: ["Guidance", "UserNote"],
            body: UserNoteBody);
        workspace.WriteText(UserNotePath, userNote);
        var indexed = await workspace.RunAsync("index");
        AssertSuccessfulHumanResult(indexed);
        var userNoteBytes = File.ReadAllBytes(workspace.Combine(UserNotePath));
        AssertRuntimeFiles(workspace, [UserNotePath]);
        AssertGeneratedEntries(workspace, containsCollaboration: false, containsPlanning: false, containsWorkflows: false);

        var collaborationInstall = await workspace.RunAsync(
            "extension", "install", "collaboration", "--automatic");
        AssertSuccessfulHumanResult(collaborationInstall);
        AssertRuntimeFiles(workspace, [UserNotePath, .. CollaborationPayloadPaths]);
        AssertExtensionClaims(
            workspace,
            new ExtensionClaimExpectation(
                "collaboration",
                "0.4.0",
                [],
                CollaborationPayloadPaths));
        AssertGeneratedEntries(workspace, containsCollaboration: true, containsPlanning: false, containsWorkflows: false);
        AssertNonExtensionRootPropertiesUnchanged(workspace, baselineLockProperties);
        Assert.Equal(userNoteBytes, File.ReadAllBytes(workspace.Combine(UserNotePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        workspace.LockStore.AssertNoRecoveryArtifacts(workspace.Path);

        await AssertContextRetrievalAsync(
            workspace,
            AdaptiveCollaborationPath,
            "## Preferred Approach");

        var planningInstall = await workspace.RunAsync(
            "extension", "install", "planning", "--automatic");
        AssertSuccessfulHumanResult(planningInstall);
        AssertRuntimeFiles(
            workspace,
            new[] { UserNotePath }
                .Concat(CollaborationPayloadPaths)
                .Concat(PlanningPayloadPaths)
                .Concat(WorkflowsPayloadPaths));
        AssertExtensionClaims(
            workspace,
            new ExtensionClaimExpectation("collaboration", "0.4.0", [], CollaborationPayloadPaths),
            new ExtensionClaimExpectation("planning", "0.4.0", ["workflows"], PlanningPayloadPaths),
            new ExtensionClaimExpectation("workflows", "0.4.0", [], WorkflowsPayloadPaths));
        AssertGeneratedEntries(workspace, containsCollaboration: true, containsPlanning: true, containsWorkflows: true);
        AssertNonExtensionRootPropertiesUnchanged(workspace, baselineLockProperties);
        Assert.Equal(userNoteBytes, File.ReadAllBytes(workspace.Combine(UserNotePath)));

        var beforeInstalledList = workspace.SnapshotState();
        var installedList = await workspace.RunAsync("extension", "list", "--installed");
        AssertSuccessfulHumanResult(installedList);
        Assert.Contains("collaboration", installedList.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("planning", installedList.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("workflows", installedList.StandardOutput, StringComparison.OrdinalIgnoreCase);
        Assert.Equal(beforeInstalledList, workspace.SnapshotState());
        var beforeListFacts = workspace.SnapshotState();
        var installedFacts = await workspace.RunAsync(
            "extension", "list", "--installed", "--format=json", "--detail=full");
        AssertSuccessfulJsonResult(installedFacts, "extension list");
        AssertInstalledFacts(
            installedFacts.StandardOutput,
            new ExtensionClaimExpectation("collaboration", "0.4.0", [], CollaborationPayloadPaths),
            new ExtensionClaimExpectation("planning", "0.4.0", ["workflows"], PlanningPayloadPaths),
            new ExtensionClaimExpectation("workflows", "0.4.0", [], WorkflowsPayloadPaths));
        Assert.Equal(beforeListFacts, workspace.SnapshotState());

        var availableFacts = await workspace.RunAsync(
            "extension", "list", "--available", "--format=json", "--detail=standard");
        AssertSuccessfulJsonResult(availableFacts, "extension list");
        AssertAvailableCatalogueFacts(availableFacts.StandardOutput);
        Assert.Equal(beforeListFacts, workspace.SnapshotState());

        await AssertContextRetrievalAsync(
            workspace,
            PlanningPlanPath,
            "## Steps And Dependencies");
        await AssertContextRetrievalAsync(
            workspace,
            PlanningTaskPath,
            "## Outcome");

        var collaborationPreimages = CollaborationPayloadPaths.ToDictionary(
            path => path,
            path => File.ReadAllBytes(workspace.Combine(path)),
            StringComparer.Ordinal);
        var remove = await workspace.RunAsync(
            "extension", "remove", "collaboration", "--automatic");
        AssertSuccessfulHumanResult(remove);
        Assert.Contains("collaboration", remove.StandardOutput, StringComparison.OrdinalIgnoreCase);
        using var removalSettings = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(".agents/open-forge.json")));
        Assert.Equal("collaboration", Assert.Single(removalSettings.RootElement
            .GetProperty("removedExtensions").EnumerateArray()).GetString());
        AssertRuntimeFiles(
            workspace,
            new[] { UserNotePath, ".agents/open-forge.json" }
                .Concat(PlanningPayloadPaths)
                .Concat(WorkflowsPayloadPaths));
        AssertExtensionClaims(
            workspace,
            new ExtensionClaimExpectation("planning", "0.4.0", ["workflows"], PlanningPayloadPaths),
            new ExtensionClaimExpectation("workflows", "0.4.0", [], WorkflowsPayloadPaths));
        AssertGeneratedEntriesAfterCollaborationRemoval(workspace);
        AssertNonExtensionRootPropertiesUnchanged(workspace, baselineLockProperties);
        Assert.Equal(userNoteBytes, File.ReadAllBytes(workspace.Combine(UserNotePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        AssertRetainedExtensionRemoveRecoveryEvidence(workspace, collaborationPreimages);

        var beforeDoctor = workspace.SnapshotState();
        var doctor = await workspace.RunAsync("doctor");
        AssertSuccessfulHumanResult(doctor);
        Assert.Equal(beforeDoctor, workspace.SnapshotState());

        var doctorFacts = await workspace.RunAsync(
            "doctor", "--format=json", "--detail=full");
        AssertSuccessfulJsonResult(doctorFacts, "doctor");
        AssertCompleteDoctorGraph(doctorFacts.StandardOutput);
        Assert.Equal(beforeDoctor, workspace.SnapshotState());
        Assert.Equal(userNoteBytes, File.ReadAllBytes(workspace.Combine(UserNotePath)));
        workspace.LockStore.AssertPersistentZeroByteLock(workspace.Path);
        AssertRetainedExtensionRemoveRecoveryEvidence(workspace, collaborationPreimages);
    }

    private static void AssertRetainedExtensionRemoveRecoveryEvidence(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, byte[]> expectedPreimages)
    {
        Assert.Equal(
            CollaborationPayloadPaths.Order(StringComparer.Ordinal),
            expectedPreimages.Keys.Order(StringComparer.Ordinal));

        var recoveryDirectory = workspace.LockStore.RecoveryWorkspaceDirectory(workspace.Path);
        Assert.True(Directory.Exists(recoveryDirectory));
        var artifacts = Directory.EnumerateFileSystemEntries(
                recoveryDirectory,
                "*",
                SearchOption.TopDirectoryOnly)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var artifact = Assert.Single(artifacts);
        var attributes = File.GetAttributes(artifact);
        Assert.True(
            (attributes & (FileAttributes.Directory | FileAttributes.Device | FileAttributes.ReparsePoint)) == 0,
            $"Recovery artifact '{artifact}' must be an ordinary file.");
        Assert.True(
            string.Equals(Path.GetExtension(artifact), ".zip", StringComparison.OrdinalIgnoreCase),
            $"Recovery artifact '{artifact}' must be a ZIP file without a draft suffix.");

        var workspaceKey = PublishedWorkspaceLockStore.RecoveryWorkspaceKey(workspace.Path);
        using var stream = new FileStream(artifact, FileMode.Open, FileAccess.Read, FileShare.Read);
        using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: false);
        var manifestEntry = archive.GetEntry("manifest.json");
        Assert.NotNull(manifestEntry);
        using var manifestStream = manifestEntry!.Open();
        using var manifest = JsonDocument.Parse(manifestStream);
        var root = manifest.RootElement;
        Assert.Equal(1, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("extension remove", root.GetProperty("command").GetString());
        Assert.True(Guid.TryParseExact(root.GetProperty("operationId").GetString(), "N", out _));
        Assert.Equal(workspace.Path, root.GetProperty("workspacePath").GetString());
        Assert.Equal(workspaceKey, root.GetProperty("workspaceKey").GetString());

        var attribution = root.GetProperty("attribution");
        Assert.Equal("extension", attribution.GetProperty("producer").GetString());
        Assert.Equal("remove", attribution.GetProperty("operation").GetString());
        var subject = attribution.GetProperty("subject");
        Assert.Equal("workspace", subject.GetProperty("kind").GetString());
        Assert.Equal(workspaceKey, subject.GetProperty("identity").GetString());

        var entries = root.GetProperty("entries").EnumerateArray().ToArray();
        Assert.NotEmpty(entries);
        foreach (var expectedPath in CollaborationPayloadPaths)
        {
            var entry = Assert.Single(
                entries,
                candidate => candidate.GetProperty("logicalPath").GetString() == expectedPath);
            Assert.Equal("ordinary-delete", entry.GetProperty("kind").GetString());
            Assert.Equal("ordinary-file", entry.GetProperty("prior").GetProperty("kind").GetString());
            Assert.Equal("missing", entry.GetProperty("intended").GetProperty("kind").GetString());

            var priorPayload = entry.GetProperty("priorPayload").GetString();
            Assert.False(string.IsNullOrWhiteSpace(priorPayload));
            var payloadEntry = archive.GetEntry(priorPayload!);
            Assert.NotNull(payloadEntry);
            using var payloadStream = payloadEntry!.Open();
            using var payload = new MemoryStream();
            payloadStream.CopyTo(payload);
            Assert.True(
                expectedPreimages[expectedPath].SequenceEqual(payload.ToArray()),
                $"Recovery payload for '{expectedPath}' did not match its captured pre-remove bytes.");
        }
    }

    private static void AssertRuntimeFiles(
        PublishedJourneyWorkspace workspace,
        IEnumerable<string> additionalPaths)
    {
        var expected = PublishedInstallWorkspace.EmbeddedPayloadPaths
            .Concat(additionalPaths)
            .Where(path => path is not OwnershipPath and not LifecyclePath)
            .Order(StringComparer.Ordinal)
            .ToArray();
        var actual = Directory
            .EnumerateFiles(workspace.Combine(".agents"), "*", SearchOption.AllDirectories)
            .Select(path => System.IO.Path.GetRelativePath(workspace.Path, path).Replace('\\', '/'))
            .Where(path => path is not OwnershipPath and not LifecyclePath)
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(expected, actual);
        foreach (var relativePath in expected)
        {
            AssertOrdinaryFile(workspace.Combine(relativePath));
        }
    }

    private static void AssertExtensionClaims(
        PublishedJourneyWorkspace workspace,
        params ExtensionClaimExpectation[] expected)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(workspace.Combine(OwnershipPath)));
        var claims = document.RootElement
            .GetProperty("extensions")
            .EnumerateArray()
            .ToArray();

        Assert.Equal(
            expected.Select(item => item.Id).Order(StringComparer.Ordinal).ToArray(),
            claims.Select(item => item.GetProperty("id").GetString()).Order(StringComparer.Ordinal).ToArray());

        foreach (var expectation in expected)
        {
            var claim = Assert.Single(
                claims,
                item => item.GetProperty("id").GetString() == expectation.Id);
            Assert.Equal(expectation.Version, claim.GetProperty("version").GetString());
            Assert.Equal("embedded catalogue", claim.GetProperty("source").GetString());
            Assert.Equal(
                expectation.Dependencies.Order(StringComparer.Ordinal).ToArray(),
                claim.GetProperty("dependencies")
                    .EnumerateArray()
                    .Select(item => item.GetString())
                    .Order(StringComparer.Ordinal)
                    .ToArray());
            Assert.Equal(
                expectation.Paths.Order(StringComparer.Ordinal).ToArray(),
                claim.GetProperty("paths")
                    .EnumerateArray()
                    .Select(item => NormalizeRelativePath(item.GetString()!))
                    .Order(StringComparer.Ordinal)
                    .ToArray());
            Assert.Empty(claim.GetProperty("regions").EnumerateArray());
        }
    }

    private static void AssertGeneratedEntries(
        PublishedJourneyWorkspace workspace,
        bool containsCollaboration,
        bool containsPlanning,
        bool containsWorkflows)
    {
        var guidanceEntryDestinations = containsCollaboration
            ? new[] { "adaptive-collaboration.md", "user-note.md" }
            : new[] { "user-note.md" };
        AssertEntryDestinations(
            workspace,
            ".agents/guidance/_guidance.md",
            guidanceEntryDestinations);
        var templateEntryDestinations = new List<string>();
        if (containsCollaboration)
        {
            templateEntryDestinations.Add("collaboration/_collaboration.md");
        }

        if (containsPlanning)
        {
            templateEntryDestinations.Add("planning/_planning.md");
        }

        if (containsWorkflows)
        {
            templateEntryDestinations.Add("workflows/_workflows.md");
        }

        AssertEntryDestinations(
            workspace,
            TemplatesEntrypointPath,
            templateEntryDestinations);

        if (containsCollaboration)
        {
            AssertEntryDestinations(
                workspace,
                CollaborationEntrypointPath,
                ["brainstorming.md"]);
        }

        if (containsPlanning)
        {
            AssertEntryDestinations(
                workspace,
                PlanningEntrypointPath,
                PlanningTemplateEntryDestinations);
        }

        if (containsWorkflows)
        {
            AssertEntryDestinations(
                workspace,
                WorkflowReferencesEntrypointPath,
                containsPlanning ? ["planning/_planning.md"] : []);
            AssertEntryDestinations(
                workspace,
                WorkflowEntrypointPath,
                ["workflow.md"]);
        }
    }

    private static void AssertGeneratedEntriesAfterCollaborationRemoval(
        PublishedJourneyWorkspace workspace)
    {
        AssertGeneratedEntries(workspace, containsCollaboration: false, containsPlanning: true, containsWorkflows: true);
    }

    private static void AssertEntryDestinations(
        PublishedJourneyWorkspace workspace,
        string relativePath,
        IEnumerable<string> expectedEntries)
    {
        Assert.Equal(
            expectedEntries.Order(StringComparer.Ordinal).ToArray(),
            ReadEntryDestinations(workspace, relativePath));
    }

    private static string[] ReadEntryDestinations(
        PublishedJourneyWorkspace workspace,
        string relativePath)
    {
        var lines = File.ReadAllText(workspace.Combine(relativePath)).Replace("\r\n", "\n")
            .Split('\n');
        var headings = lines
            .Select((line, index) => (line, index))
            .Where(value => value.line.Trim() == "## Entries")
            .Select(value => value.index)
            .ToArray();
        Assert.Single(headings);

        var destinations = new List<string>();
        for (var index = headings[0] + 1; index < lines.Length; index++)
        {
            var line = lines[index];
            if (line.StartsWith("#", StringComparison.Ordinal))
            {
                break;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith("- none -", StringComparison.Ordinal))
            {
                continue;
            }

            if (!trimmed.StartsWith("- [", StringComparison.Ordinal))
            {
                Assert.False(
                    trimmed.StartsWith("- ", StringComparison.Ordinal),
                    $"Unexpected generated Entries row: {line}");
                continue;
            }

            var linkStart = line.IndexOf("](", StringComparison.Ordinal);
            Assert.True(linkStart >= 0, $"Generated Entries line has no link: {line}");
            var linkEnd = line.IndexOf(')', linkStart + 2);
            Assert.True(linkEnd > linkStart + 2, $"Generated Entries link is incomplete: {line}");
            destinations.Add(line[(linkStart + 2)..linkEnd]);
        }

        Assert.Equal(destinations.Count, destinations.Distinct(StringComparer.Ordinal).Count());
        return destinations.Order(StringComparer.Ordinal).ToArray();
    }

    private static async Task AssertContextRetrievalAsync(
        PublishedJourneyWorkspace workspace,
        string relativePath,
        string expectedBodyMarker)
    {
        var before = workspace.SnapshotState();
        var human = await workspace.RunAsync("context", relativePath);
        AssertSuccessfulHumanResult(human);
        Assert.Contains(relativePath, human.StandardOutput, StringComparison.Ordinal);
        Assert.Contains(expectedBodyMarker, human.StandardOutput, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());

        var structured = await workspace.RunAsync(
            "context",
            relativePath,
            "--content=body",
            "--detail=full",
            "--format=json");
        AssertSuccessfulJsonResult(structured, "context");
        using var document = JsonDocument.Parse(structured.StandardOutput);
        var source = Assert.Single(
            document.RootElement
                .GetProperty("data")
                .GetProperty("sources")
                .EnumerateArray(),
            item => item.GetProperty("path").GetString() == relativePath);
        var body = Assert.Single(source.GetProperty("parts").EnumerateArray());
        Assert.Equal("body", body.GetProperty("part").GetString());
        Assert.Contains(expectedBodyMarker, body.GetProperty("text").GetString(), StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotState());
    }

    private static void AssertInstalledFacts(
        string json,
        params ExtensionClaimExpectation[] expected)
    {
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("data");
        var source = data.GetProperty("source");
        Assert.Equal("embedded-catalogue", source.GetProperty("kind").GetString());
        Assert.Equal("embedded catalogue", source.GetProperty("path").GetString());

        var installed = data
            .GetProperty("installed")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(
            expected.Select(item => item.Id).Order(StringComparer.Ordinal).ToArray(),
            installed.Select(item => item.GetProperty("id").GetString()).Order(StringComparer.Ordinal).ToArray());

        foreach (var expectation in expected)
        {
            var row = Assert.Single(
                installed,
                item => item.GetProperty("id").GetString() == expectation.Id);
            Assert.Equal(expectation.Version, row.GetProperty("version").GetString());
            Assert.Equal(expectation.Paths.Length, row.GetProperty("files").GetInt32());
            Assert.Equal("embedded catalogue", row.GetProperty("recordedSource").GetString());
            Assert.Equal("complete", row.GetProperty("coverage").GetString());
        }
    }

    private static void AssertAvailableCatalogueFacts(string json)
    {
        using var document = JsonDocument.Parse(json);
        var data = document.RootElement.GetProperty("data");
        var source = data.GetProperty("source");
        Assert.Equal("embedded-catalogue", source.GetProperty("kind").GetString());
        Assert.Equal("embedded catalogue", source.GetProperty("path").GetString());

        var available = data
            .GetProperty("available")
            .EnumerateArray()
            .ToArray();
        var planning = Assert.Single(
            available,
            item => item.GetProperty("id").GetString() == "planning");
        Assert.Equal("0.4.0", planning.GetProperty("version").GetString());
        Assert.Equal(
            ["workflows"],
            planning.GetProperty("dependencies")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray());

        var workflows = Assert.Single(
            available,
            item => item.GetProperty("id").GetString() == "workflows");
        Assert.Equal("0.4.0", workflows.GetProperty("version").GetString());
        Assert.Equal("Workflow Support", workflows.GetProperty("name").GetString());
        Assert.Empty(workflows.GetProperty("dependencies").EnumerateArray());
    }

    private static IReadOnlyDictionary<string, JsonElement> CaptureNonExtensionRootProperties(
        PublishedJourneyWorkspace workspace)
    {
        using var document = JsonDocument.Parse(
            File.ReadAllText(workspace.Combine(OwnershipPath)));
        return document.RootElement
            .EnumerateObject()
            .Where(property => !property.NameEquals("extensions"))
            .ToDictionary(
                property => property.Name,
                property => property.Value.Clone(),
                StringComparer.Ordinal);
    }

    private static void AssertNonExtensionRootPropertiesUnchanged(
        PublishedJourneyWorkspace workspace,
        IReadOnlyDictionary<string, JsonElement> expected)
    {
        var actual = CaptureNonExtensionRootProperties(workspace);
        Assert.Equal(
            expected.Keys.Order(StringComparer.Ordinal),
            actual.Keys.Order(StringComparer.Ordinal));
        foreach (var property in expected.Keys.Order(StringComparer.Ordinal))
        {
            AssertSemanticallyEqual(expected[property], actual[property], property);
        }
    }

    private static void AssertSemanticallyEqual(
        JsonElement expected,
        JsonElement actual,
        string location)
    {
        Assert.Equal(expected.ValueKind, actual.ValueKind);
        switch (expected.ValueKind)
        {
            case JsonValueKind.Object:
                {
                    var expectedProperties = expected.EnumerateObject()
                        .ToDictionary(property => property.Name, property => property.Value, StringComparer.Ordinal);
                    var actualProperties = actual.EnumerateObject()
                        .ToDictionary(property => property.Name, property => property.Value, StringComparer.Ordinal);
                    Assert.Equal(
                        expectedProperties.Keys.Order(StringComparer.Ordinal),
                        actualProperties.Keys.Order(StringComparer.Ordinal));
                    foreach (var property in expectedProperties.Keys.Order(StringComparer.Ordinal))
                    {
                        AssertSemanticallyEqual(
                            expectedProperties[property],
                            actualProperties[property],
                            $"{location}.{property}");
                    }

                    break;
                }
            case JsonValueKind.Array:
                {
                    var expectedItems = expected.EnumerateArray().ToArray();
                    var actualItems = actual.EnumerateArray().ToArray();
                    Assert.Equal(expectedItems.Length, actualItems.Length);
                    for (var index = 0; index < expectedItems.Length; index++)
                    {
                        AssertSemanticallyEqual(
                            expectedItems[index],
                            actualItems[index],
                            $"{location}[{index}]");
                    }

                    break;
                }
            case JsonValueKind.String:
                Assert.Equal(expected.GetString(), actual.GetString());
                break;
            case JsonValueKind.Number:
                Assert.Equal(expected.GetDecimal(), actual.GetDecimal());
                break;
            case JsonValueKind.True:
            case JsonValueKind.False:
                Assert.Equal(expected.GetBoolean(), actual.GetBoolean());
                break;
            case JsonValueKind.Null:
                break;
            default:
                throw new Xunit.Sdk.XunitException(
                    $"Unsupported JSON value kind at {location}: {expected.ValueKind}.");
        }
    }

    private static void AssertCompleteDoctorGraph(string json)
    {
        using var document = JsonDocument.Parse(json);
        var categories = document.RootElement
            .GetProperty("data")
            .GetProperty("categories")
            .EnumerateArray()
            .ToArray();
        Assert.Equal(6, categories.Length);
        foreach (var category in categories)
        {
            Assert.Equal("complete", category.GetProperty("coverage").GetString());
            Assert.Empty(category.GetProperty("limitations").EnumerateArray());
        }
    }

    private static void AssertOrdinaryFile(string path)
    {
        var attributes = File.GetAttributes(path);
        Assert.Equal(
            (FileAttributes)0,
            attributes & (FileAttributes.Directory | FileAttributes.ReparsePoint | FileAttributes.Device));
    }

    private static string NormalizeRelativePath(string path)
        => path.Replace('\\', '/');

    private static void AssertSuccessfulHumanResult(ProcessRunResult result)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Equal(string.Empty, result.StandardError);
    }

    private static void AssertSuccessfulJsonResult(ProcessRunResult result, string command)
    {
        AssertSuccessfulHumanResult(result);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
    }

    private sealed record ExtensionClaimExpectation(
        string Id,
        string Version,
        string[] Dependencies,
        string[] Paths);
}
