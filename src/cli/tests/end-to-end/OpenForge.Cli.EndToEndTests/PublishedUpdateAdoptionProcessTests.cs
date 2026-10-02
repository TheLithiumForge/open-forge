using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedUpdateAdoptionProcessTests
{
    private const string Feature = "update-workspace-adoption";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string LoaderPath = ".agents/loader.md";
    private const string InstalledLoaderSentence =
        "It defines how to select context, follow applicable rules, and maintain the workspace.";
    private const string SyntheticEarlierLoaderSentence =
        "It defines how to select context, follow applicable rules, and maintain this synthetic earlier payload.";
    private const string SkillRoot = ".agents/skills/local-skill";
    private const string SkillPath = SkillRoot + "/SKILL.md";
    private const string ReferencePath = SkillRoot + "/references/guide.md";
    private const string ReferenceCataloguePath = SkillRoot + "/references/_references.md";
    private const string SkillBody = "# Local Skill\n\nRead [Guide](references/guide.md).\n";
    private const string ReferenceBody = "# Guide\n\nPreserve this authored reference byte-for-byte.\n";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    [Theory]
    [InlineData("missing-frontmatter")]
    [InlineData("partial-frontmatter")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task AutomaticUpdateAdoptsNativeSkillAndReferenceWithoutReplacingUserBytes(
        string fixtureCase)
    {
        using var workspace = await CreateFixtureAsync($"task70-update-{fixtureCase}", fixtureCase);

        var beforePreviewWorkspace = workspace.SnapshotState();
        var beforePreviewDataHome = SnapshotDataHome(workspace);
        var preview = await workspace.RunAsync(
            "update", "--automatic", "--dry-run", "--format", "json", "--detail", "full");
        using (var document = AssertUpdateResult(preview, "dry-run"))
        {
            AssertNativeSkillMigration(document.RootElement, fixtureCase, "planned");
            AssertMigration(document.RootElement, ReferenceCataloguePath, "planned", "entrypoint-created");
            AssertEffectPaths(
                document.RootElement,
                LoaderPath,
                SkillPath,
                ReferenceCataloguePath);
        }

        Assert.Equal(beforePreviewWorkspace, workspace.SnapshotState());
        Assert.Equal(beforePreviewDataHome, SnapshotDataHome(workspace));

        var applied = await workspace.RunAsync(
            "update", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertUpdateResult(applied, "apply"))
        {
            AssertNativeSkillMigration(document.RootElement, fixtureCase, "applied");
            AssertMigration(document.RootElement, ReferenceCataloguePath, "applied", "entrypoint-created");
            AssertEffectPaths(
                document.RootElement,
                LoaderPath,
                SkillPath,
                ReferenceCataloguePath);
        }

        AssertAdoptedFiles(workspace, fixtureCase);
        Assert.DoesNotContain(
            SyntheticEarlierLoaderSentence,
            StrictUtf8.GetString(File.ReadAllBytes(workspace.Combine(LoaderPath))),
            StringComparison.Ordinal);
        Assert.Contains(
            InstalledLoaderSentence,
            StrictUtf8.GetString(File.ReadAllBytes(workspace.Combine(LoaderPath))),
            StringComparison.Ordinal);

        var skillRoutes = await workspace.RunAsync(
            "route", "list", SkillPath, "--depth=all", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(skillRoutes, "route list"))
        {
            var rows = document.RootElement.GetProperty("data").GetProperty("rows")
                .EnumerateArray()
                .ToArray();
            var nativeSkill = Assert.Single(rows);
            Assert.Equal(SkillPath, nativeSkill.GetProperty("path").GetString());
            Assert.Contains("local-skill", nativeSkill.GetProperty("id").GetString(), StringComparison.Ordinal);
            Assert.Equal("file", nativeSkill.GetProperty("kind").GetString());
            Assert.Equal("explicit-root", nativeSkill.GetProperty("selectedAs").GetString());
        }

        var inspectedSkill = await workspace.RunAsync(
            "route", "inspect", SkillPath, "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(inspectedSkill, "route inspect"))
        {
            Assert.Equal(SkillPath, document.RootElement.GetProperty("data").GetProperty("path").GetString());
            Assert.Equal("native", document.RootElement.GetProperty("data").GetProperty("kind").GetString());
        }

        var referenceContext = await workspace.RunAsync(
            "context", ReferencePath, "--content", "body", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(referenceContext, "context"))
        {
            var source = Assert.Single(
                document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray(),
                item => item.GetProperty("path").GetString() == ReferencePath);
            var body = Assert.Single(source.GetProperty("parts").EnumerateArray());
            Assert.Equal("body", body.GetProperty("part").GetString());
            Assert.Equal(ReferenceBody, body.GetProperty("text").GetString());
        }

        var beforeRepeatWorkspace = workspace.SnapshotState();
        var beforeRepeatDataHome = SnapshotDataHome(workspace);
        var repeat = await workspace.RunAsync(
            "update", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertUpdateResult(repeat, "apply"))
        {
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            AssertNoMigrations(document.RootElement);
        }

        Assert.Equal(beforeRepeatWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeRepeatDataHome, SnapshotDataHome(workspace));

        var beforeIndexWorkspace = workspace.SnapshotState();
        var beforeIndexDataHome = SnapshotDataHome(workspace);
        var index = await workspace.RunAsync("index", "--format", "json", "--detail", "full");
        using (var document = AssertIndexWarnings(
            index,
            [ReferencePath],
            [ReferenceCataloguePath]))
        {
            Assert.Equal("completed-with-warnings", document.RootElement.GetProperty("status").GetString());
        }

        Assert.Equal(beforeIndexWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeIndexDataHome, SnapshotDataHome(workspace));
    }

    [Fact]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task PlainUpdateConfirmationAppliesAndReportsMigratedPaths()
    {
        SkipUnlessWindowsConPty();
        using var workspace = await CreateFixtureAsync("task70-update-confirmed", "partial-frontmatter");

        var terminal = await PublishedWindowsTerminal.RunAsync(
            workspace.Target,
            workspace.Path,
            ["update"],
            workspace.ProcessEnvironment,
            "y\r");

        Assert.Equal(0, terminal.ExitCode);
        var planned = terminal.Transcript.IndexOf("Planned migration", StringComparison.OrdinalIgnoreCase);
        var migrated = terminal.Transcript.IndexOf("Migrated", StringComparison.OrdinalIgnoreCase);
        Assert.True(planned >= 0, terminal.Transcript);
        Assert.True(migrated > planned, terminal.Transcript);
        Assert.Contains(SkillPath, terminal.Transcript, StringComparison.Ordinal);
        Assert.Contains(ReferenceCataloguePath, terminal.Transcript, StringComparison.Ordinal);
        AssertAdoptedFiles(workspace, "partial-frontmatter");
    }

    [Fact]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task PlainUpdateDeclineCancelsBeforeWorkspaceOrRecoveryWrites()
    {
        SkipUnlessWindowsConPty();
        using var workspace = await CreateFixtureAsync("task70-update-cancelled", "missing-frontmatter");
        var beforeWorkspace = workspace.SnapshotState();
        var beforeDataHome = SnapshotDataHome(workspace);
        AssertNoRecoveryArtifacts(beforeDataHome);

        var terminal = await PublishedWindowsTerminal.RunAsync(
            workspace.Target,
            workspace.Path,
            ["update"],
            workspace.ProcessEnvironment,
            "n\r");

        Assert.Equal(130, terminal.ExitCode);
        Assert.Contains("cancelled", terminal.Transcript, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain(
            $"Migrated {SkillPath}",
            terminal.Transcript,
            StringComparison.Ordinal);
        Assert.Equal(beforeWorkspace, workspace.SnapshotState());
        var afterDataHome = SnapshotDataHome(workspace);
        Assert.Equal(beforeDataHome, afterDataHome);
        AssertNoRecoveryArtifacts(afterDataHome);
    }

    private static async Task<PublishedJourneyWorkspace> CreateFixtureAsync(
        string purpose,
        string fixtureCase)
    {
        var workspace = PublishedJourneyWorkspace.Create(purpose);
        try
        {
            workspace.ExpectCoreInstall();
            workspace.ExpectFiles(SkillPath, ReferencePath, ReferenceCataloguePath);

            var install = await workspace.RunAsync(
                "install", "--automatic", "--format", "json", "--detail", "full");
            using (AssertCompletedJson(install, "install"))
            {
            }

            AddSyntheticEarlierLoader(workspace);
            var skillBytes = MinimalSkillBytes(fixtureCase);
            workspace.WriteBytes(SkillPath, skillBytes);
            workspace.WriteBytes(ReferencePath, StrictUtf8.GetBytes(ReferenceBody));

            var unchangedInstall = await workspace.RunAsync(
                "install", "--automatic", "--format", "json", "--detail", "full");
            using (var document = AssertManagedDivergence(unchangedInstall))
            {
                Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            }

            Assert.True(File.Exists(workspace.Combine(SkillPath)));
            Assert.False(File.Exists(workspace.Combine(ReferenceCataloguePath)));
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static void AddSyntheticEarlierLoader(PublishedJourneyWorkspace workspace)
    {
        var loaderBytes = File.ReadAllBytes(workspace.Combine(LoaderPath));
        var loaderText = StrictUtf8.GetString(loaderBytes);
        var sentenceIndex = loaderText.IndexOf(InstalledLoaderSentence, StringComparison.Ordinal);
        Assert.True(sentenceIndex >= 0, "The current embedded loader includes the authored sentence fixture.");
        Assert.Equal(
            sentenceIndex,
            loaderText.LastIndexOf(InstalledLoaderSentence, StringComparison.Ordinal));
        var entriesIndex = loaderText.IndexOf("\n## Entries\n", StringComparison.Ordinal);
        Assert.True(
            entriesIndex > sentenceIndex,
            "The synthetic earlier-payload sentence is before the generated Entries section.");

        var earlierLoaderText = loaderText[..sentenceIndex]
            + SyntheticEarlierLoaderSentence
            + loaderText[(sentenceIndex + InstalledLoaderSentence.Length)..];
        workspace.WriteBytes(LoaderPath, StrictUtf8.GetBytes(earlierLoaderText));
    }

    private static byte[] MinimalSkillBytes(string fixtureCase)
    {
        var content = fixtureCase switch
        {
            "missing-frontmatter" => SkillBody,
            "partial-frontmatter" =>
                $"---\nname: local-skill\nlicense: MIT\ncustom-field: preserve-me\n---\n\n{SkillBody}",
            _ => throw new ArgumentOutOfRangeException(
                nameof(fixtureCase),
                fixtureCase,
                "Unknown native Skill fixture case."),
        };
        return StrictUtf8.GetBytes(content);
    }

    private static string BodyAfterFrontmatter(string source)
    {
        if (!source.StartsWith("---\n", StringComparison.Ordinal))
        {
            return source;
        }

        var closingIndex = source.IndexOf("\n---\n", 4, StringComparison.Ordinal);
        Assert.True(closingIndex >= 0, "The partial native Skill fixture has closed frontmatter.");
        return source[(closingIndex + "\n---\n".Length)..];
    }

    private static void AssertAdoptedFiles(
        PublishedJourneyWorkspace workspace,
        string fixtureCase)
    {
        var completedBytes = File.ReadAllBytes(workspace.Combine(SkillPath));
        var completedText = StrictUtf8.GetString(completedBytes);
        var initialText = StrictUtf8.GetString(MinimalSkillBytes(fixtureCase));
        Assert.StartsWith("---\n", completedText, StringComparison.Ordinal);
        Assert.Contains("description:", completedText, StringComparison.Ordinal);
        Assert.EndsWith(BodyAfterFrontmatter(initialText), completedText, StringComparison.Ordinal);
        Assert.DoesNotContain("open-forge:", completedText, StringComparison.Ordinal);
        Assert.DoesNotContain("## Entries", completedText, StringComparison.Ordinal);

        if (fixtureCase == "missing-frontmatter")
        {
            Assert.Contains("name: \"local-skill\"\n", completedText, StringComparison.Ordinal);
            Assert.DoesNotContain("license:", completedText, StringComparison.Ordinal);
        }
        else
        {
            var nameIndex = completedText.IndexOf("name: local-skill\n", StringComparison.Ordinal);
            var licenseIndex = completedText.IndexOf("license: MIT\n", StringComparison.Ordinal);
            var customFieldIndex = completedText.IndexOf("custom-field: preserve-me\n", StringComparison.Ordinal);
            Assert.True(nameIndex >= 0 && nameIndex < licenseIndex && licenseIndex < customFieldIndex);
        }

        Assert.Equal(
            StrictUtf8.GetBytes(ReferenceBody),
            File.ReadAllBytes(workspace.Combine(ReferencePath)));
        Assert.True(File.Exists(workspace.Combine(ReferenceCataloguePath)));
        Assert.Contains(
            "guide.md",
            StrictUtf8.GetString(File.ReadAllBytes(workspace.Combine(ReferenceCataloguePath))),
            StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(".agents/skills/_local-skill.md")));
        Assert.False(File.Exists(workspace.Combine(SkillRoot + "/_local-skill.md")));
        AssertFrameworkDoesNotOwn(workspace, SkillPath, ReferencePath, ReferenceCataloguePath);
    }

    private static JsonDocument AssertCompletedJson(ProcessRunResult result, string command)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("findings").EnumerateArray());
        return document;
    }

    private static JsonDocument AssertManagedDivergence(ProcessRunResult result)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("install", root.GetProperty("command").GetString());
        Assert.Equal("blocked", root.GetProperty("status").GetString());
        Assert.Equal(
            "managed-divergence",
            root.GetProperty("data").GetProperty("classification").GetString());
        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
        Assert.Equal("install.managed-divergence", finding.GetProperty("code").GetString());
        Assert.Equal("error", finding.GetProperty("severity").GetString());
        return document;
    }

    private static JsonDocument AssertUpdateResult(ProcessRunResult result, string mode)
    {
        Assert.Equal(0, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("update", root.GetProperty("command").GetString());
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(mode, root.GetProperty("data").GetProperty("mode").GetString());
        Assert.Empty(root.GetProperty("findings").EnumerateArray());
        return document;
    }

    private static JsonElement AssertMigration(
        JsonElement root,
        string path,
        string outcome,
        string expectedAction)
    {
        var migrations = root.GetProperty("data").GetProperty("migrations");
        Assert.Equal(JsonValueKind.Array, migrations.ValueKind);
        var rows = migrations.EnumerateArray().ToArray();
        var paths = rows.Select(row => row.GetProperty("path").GetString()).ToArray();
        Assert.Equal(paths.OrderBy(value => value, StringComparer.Ordinal), paths);
        var row = Assert.Single(rows, item => item.GetProperty("path").GetString() == path);
        Assert.Equal(outcome, row.GetProperty("outcome").GetString());
        Assert.Contains(expectedAction, ReadStrings(row.GetProperty("actions")));
        Assert.Equal(JsonValueKind.Array, row.GetProperty("fields").ValueKind);
        Assert.Equal(JsonValueKind.Array, row.GetProperty("derivation").ValueKind);
        return row.Clone();
    }

    private static void AssertNativeSkillMigration(
        JsonElement root,
        string fixtureCase,
        string outcome)
    {
        var migration = AssertMigration(root, SkillPath, outcome, "metadata-completed");
        var expectedFields = fixtureCase == "missing-frontmatter"
            ? new[] { "description", "name" }
            : new[] { "description" };
        var actualFields = ReadStrings(migration.GetProperty("fields"));
        Assert.Equal(
            expectedFields.OrderBy(value => value, StringComparer.Ordinal),
            actualFields.OrderBy(value => value, StringComparer.Ordinal));
    }

    private static void AssertEffectPaths(JsonElement root, params string[] expectedPaths)
    {
        var effects = root.GetProperty("effects").EnumerateArray().ToArray();
        foreach (var path in expectedPaths)
        {
            Assert.Contains(
                effects,
                effect => effect.GetProperty("path").GetString() == path);
        }
    }

    private static void AssertNoMigrations(JsonElement root)
    {
        var data = root.GetProperty("data");
        if (data.TryGetProperty("migrations", out var migrations))
        {
            Assert.Equal(JsonValueKind.Array, migrations.ValueKind);
            Assert.Empty(migrations.EnumerateArray());
        }
    }

    private static JsonDocument AssertIndexWarnings(
        ProcessRunResult result,
        IReadOnlyCollection<string> expectedMetadataPaths,
        IReadOnlyCollection<string> expectedCurrentCatalogues)
    {
        var expectedFindings = expectedMetadataPaths
            .Select(path => (Code: "index.metadata-optional", Path: path))
            .ToArray();
        Assert.Equal(2, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        var root = document.RootElement;
        Assert.Equal(3, root.GetProperty("schemaVersion").GetInt32());
        Assert.Equal("index", root.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", root.GetProperty("status").GetString());

        var findings = root.GetProperty("findings").EnumerateArray().ToArray();
        Assert.All(findings, finding => Assert.Equal("warning", finding.GetProperty("severity").GetString()));
        var actualFindings = findings
            .Select(finding => (
                Code: finding.GetProperty("code").GetString()!,
                Path: finding.GetProperty("subject").GetProperty("path").GetString()!))
            .OrderBy(finding => finding.Code, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToArray();
        var orderedExpected = expectedFindings
            .OrderBy(finding => finding.Code, StringComparer.Ordinal)
            .ThenBy(finding => finding.Path, StringComparer.Ordinal)
            .ToArray();
        Assert.Equal(orderedExpected, actualFindings);

        Assert.Empty(root.GetProperty("effects").EnumerateArray());
        Assert.Equal(0, root.GetProperty("counts").GetProperty("filesUpdated").GetInt32());
        var data = root.GetProperty("data");
        Assert.Empty(data.GetProperty("changes").EnumerateArray());
        var unchanged = data.GetProperty("unchanged").EnumerateArray()
            .Select(item => item.GetProperty("path").GetString())
            .ToArray();
        foreach (var catalogue in expectedCurrentCatalogues)
        {
            Assert.Contains(catalogue, unchanged);
        }

        return document;
    }

    private static string?[] ReadStrings(JsonElement array)
        => array.EnumerateArray().Select(value => value.GetString()).ToArray();

    private static void AssertFrameworkDoesNotOwn(
        PublishedJourneyWorkspace workspace,
        params string[] userOwnedPaths)
    {
        using var document = JsonDocument.Parse(File.ReadAllBytes(workspace.Combine(OwnershipPath)));
        var frameworkPaths = document.RootElement.GetProperty("framework").GetProperty("paths")
            .EnumerateArray()
            .Select(value => value.GetString())
            .ToArray();
        foreach (var path in userOwnedPaths)
        {
            Assert.DoesNotContain(path, frameworkPaths);
        }
    }

    private static IReadOnlyDictionary<string, string> SnapshotDataHome(
        PublishedJourneyWorkspace workspace)
    {
        var dataHome = workspace.ProcessEnvironment["OPENFORGE_DATA_HOME"];
        return Directory.Exists(dataHome)
            ? PublishedWorkspaceTreeSnapshot.Capture(dataHome)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }

    private static void AssertNoRecoveryArtifacts(
        IReadOnlyDictionary<string, string> dataHomeSnapshot)
    {
        Assert.DoesNotContain(
            dataHomeSnapshot.Keys,
            path => path.Contains("recovery", StringComparison.OrdinalIgnoreCase));
    }

    private static void SkipUnlessWindowsConPty()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Skip("This confirmation journey requires the repository's Windows ConPTY harness.");
        }
    }
}
