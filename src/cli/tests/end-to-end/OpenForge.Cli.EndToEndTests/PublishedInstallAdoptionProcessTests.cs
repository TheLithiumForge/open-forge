using System.Text;
using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;

namespace OpenForge.Cli.EndToEndTests;

public sealed class PublishedInstallAdoptionProcessTests
{
    private const string Feature = "install-adoption";
    private const string OwnershipPath = ".agents/open-forge.lock.json";
    private const string SkillRoot = ".agents/skills/local-skill";
    private const string SkillPath = SkillRoot + "/SKILL.md";
    private const string ReferencePath = SkillRoot + "/references/guide.md";
    private const string ReferenceCataloguePath = SkillRoot + "/references/_references.md";
    private const string SkillBody = "# Local Skill\n\nRead [Reference](references/guide.md).\n";
    private const string ReferenceBody = "# Reference\n\nPreserve reference text.\n";

    private static readonly UTF8Encoding StrictUtf8 = new(
        encoderShouldEmitUTF8Identifier: false,
        throwOnInvalidBytes: true);

    [Theory]
    [InlineData("missing-frontmatter")]
    [InlineData("name-license-without-description")]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task MinimalNativeSkillAdoptionCompletesOnlyMissingMetadataAndRoutesReference(string fixtureCase)
    {
        using var workspace = PublishedJourneyWorkspace.Create($"task70-minimal-{fixtureCase}");
        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(ReferenceCataloguePath);
        var inputSkill = MinimalSkillBytes(fixtureCase);
        var expectedBody = fixtureCase == "missing-frontmatter"
            ? StrictUtf8.GetString(inputSkill)
            : BodyAfterFrontmatter(StrictUtf8.GetString(inputSkill));
        workspace.WriteBytes(SkillPath, inputSkill);
        workspace.WriteText(ReferencePath, ReferenceBody);

        var beforeDryRunWorkspace = workspace.SnapshotState();
        var beforeDryRunDataHome = SnapshotDataHome(workspace);
        var dryRun = await workspace.RunAsync(
            "install", "--automatic", "--dry-run", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(dryRun, "install"))
        {
            var skillMigration = ReadMigration(document.RootElement, SkillPath, "planned");
            var actions = ReadStrings(skillMigration.GetProperty("actions"));
            var fields = ReadStrings(skillMigration.GetProperty("fields"));
            Assert.Contains("metadata-completed", actions);
            Assert.Contains("description", fields);
            if (fixtureCase == "missing-frontmatter")
            {
                Assert.Contains("name", fields);
            }
            else
            {
                Assert.DoesNotContain("name", fields);
            }

            var catalogueMigration = ReadMigration(document.RootElement, ReferenceCataloguePath, "planned");
            Assert.Contains("entrypoint-created", ReadStrings(catalogueMigration.GetProperty("actions")));
        }

        Assert.Equal(beforeDryRunWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeDryRunDataHome, SnapshotDataHome(workspace));

        var apply = await workspace.RunAsync(
            "install", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(apply, "install"))
        {
            var skillMigration = ReadMigration(document.RootElement, SkillPath, "applied");
            Assert.Contains("metadata-completed", ReadStrings(skillMigration.GetProperty("actions")));
            Assert.Contains("description", ReadStrings(skillMigration.GetProperty("fields")));
            _ = ReadMigration(document.RootElement, ReferenceCataloguePath, "applied");
        }

        var completedText = StrictUtf8.GetString(File.ReadAllBytes(workspace.Combine(SkillPath)));
        if (fixtureCase == "missing-frontmatter")
        {
            Assert.Contains("name: \"local-skill\"\n", completedText, StringComparison.Ordinal);
        }
        else
        {
            Assert.Contains("name: local-skill\nlicense: MIT\n", completedText, StringComparison.Ordinal);
        }

        Assert.Contains("description:", completedText, StringComparison.Ordinal);
        Assert.EndsWith(expectedBody, completedText, StringComparison.Ordinal);
        if (fixtureCase == "name-license-without-description")
        {
            Assert.Contains("license: MIT", completedText, StringComparison.Ordinal);
        }
        else
        {
            Assert.DoesNotContain("license:", completedText, StringComparison.Ordinal);
        }

        Assert.Equal(StrictUtf8.GetBytes(ReferenceBody), File.ReadAllBytes(workspace.Combine(ReferencePath)));
        Assert.True(File.Exists(workspace.Combine(ReferenceCataloguePath)));
        Assert.Contains("guide.md", File.ReadAllText(workspace.Combine(ReferenceCataloguePath)), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(".agents/skills/_local-skill.md")));
        Assert.False(File.Exists(workspace.Combine(SkillRoot + "/_local-skill.md")));
        AssertFrameworkDoesNotOwn(workspace, SkillPath, ReferenceCataloguePath);

        var beforeRepeatWorkspace = workspace.SnapshotState();
        var beforeRepeatDataHome = SnapshotDataHome(workspace);
        var repeat = await workspace.RunAsync(
            "install", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(repeat, "install"))
        {
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            AssertNoMigrations(document.RootElement);
        }

        Assert.Equal(beforeRepeatWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeRepeatDataHome, SnapshotDataHome(workspace));
        Assert.Equal(StrictUtf8.GetBytes(ReferenceBody), File.ReadAllBytes(workspace.Combine(ReferencePath)));

        var beforeIndexWorkspace = workspace.SnapshotState();
        var beforeIndexDataHome = SnapshotDataHome(workspace);
        var index = await workspace.RunAsync(
            "index", "--dry-run", "--format", "json", "--detail", "full");
        using (var document = AssertIndexWarnings(
            index,
            [ReferencePath],
            [ReferenceCataloguePath],
            requireNoChanges: true))
        {
            Assert.Equal("completed-with-warnings", document.RootElement.GetProperty("status").GetString());
        }

        Assert.Equal(beforeIndexWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeIndexDataHome, SnapshotDataHome(workspace));
    }

    [Fact]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task ExistingNativeSkillAndNestedMarkdownKeepAuthoredBytesAndReceiveLocalRoutes()
    {
        using var workspace = PublishedJourneyWorkspace.Create("task70-valid-nested-skill");
        const string skillPath = ".agents/skills/team-notes/SKILL.md";
        const string directReferencePath = ".agents/skills/team-notes/references/guide.md";
        const string nestedReferencePath = ".agents/skills/team-notes/references/nested/format.md";
        const string referenceCatalogue = ".agents/skills/team-notes/references/_references.md";
        const string nestedCatalogue = ".agents/skills/team-notes/references/nested/_nested.md";
        const string skillText = "---\r\nname: team-notes\r\ndescription: Keep team decisions and next actions clear.\r\ncustom-field: preserve exactly\r\n---\r\n\r\n# Team notes\r\n\r\nRead the nested reference.\r\n";
        const string directReferenceText = "# Guide\r\n\r\nKeep the original line endings.\r\n";
        const string nestedReferenceText = "# Format\r\n\r\nPreserve nested authored content.\r\n";
        var skillBytes = StrictUtf8.GetBytes(skillText);
        var directReferenceBytes = StrictUtf8.GetBytes(directReferenceText);
        var nestedReferenceBytes = StrictUtf8.GetBytes(nestedReferenceText);

        workspace.ExpectCoreInstall();
        workspace.ExpectFiles(referenceCatalogue, nestedCatalogue);
        workspace.WriteBytes(skillPath, skillBytes);
        workspace.WriteBytes(directReferencePath, directReferenceBytes);
        workspace.WriteBytes(nestedReferencePath, nestedReferenceBytes);

        var install = await workspace.RunAsync(
            "install", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(install, "install"))
        {
            Assert.NotEmpty(document.RootElement.GetProperty("effects").EnumerateArray());
        }

        Assert.Equal(skillBytes, File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.Equal(directReferenceBytes, File.ReadAllBytes(workspace.Combine(directReferencePath)));
        Assert.Equal(nestedReferenceBytes, File.ReadAllBytes(workspace.Combine(nestedReferencePath)));
        Assert.DoesNotContain("license:", StrictUtf8.GetString(File.ReadAllBytes(workspace.Combine(skillPath))), StringComparison.Ordinal);
        Assert.True(File.Exists(workspace.Combine(referenceCatalogue)));
        Assert.True(File.Exists(workspace.Combine(nestedCatalogue)));
        Assert.Contains("guide.md", File.ReadAllText(workspace.Combine(referenceCatalogue)), StringComparison.Ordinal);
        Assert.Contains("format.md", File.ReadAllText(workspace.Combine(nestedCatalogue)), StringComparison.Ordinal);
        Assert.False(File.Exists(workspace.Combine(".agents/skills/team-notes/_team-notes.md")));
        AssertFrameworkDoesNotOwn(workspace, skillPath, referenceCatalogue, nestedCatalogue);

        var skillList = await workspace.RunAsync(
            "route", "list", skillPath, "--depth=all", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(skillList, "route list"))
        {
            var rows = document.RootElement.GetProperty("data").GetProperty("rows").EnumerateArray().ToArray();
            var skillRow = Assert.Single(rows);
            Assert.Equal(skillPath, skillRow.GetProperty("path").GetString());
            Assert.Contains("team-notes", skillRow.GetProperty("id").GetString(), StringComparison.Ordinal);
            Assert.Equal("file", skillRow.GetProperty("kind").GetString());
            Assert.Equal("explicit-root", skillRow.GetProperty("selectedAs").GetString());
        }

        var referencesList = await workspace.RunAsync(
            "route", "list", referenceCatalogue, "--depth=all", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedWithExactWarnings(
            referencesList,
            "route list",
            ("route-list.authored-form", referenceCatalogue),
            ("route-list.metadata-missing", directReferencePath),
            ("route-list.metadata-missing", nestedReferencePath)))
        {
            var rows = document.RootElement.GetProperty("data").GetProperty("rows").EnumerateArray().ToArray();
            var catalogueRow = Assert.Single(rows, row => row.GetProperty("path").GetString() == referenceCatalogue);
            Assert.Equal("entrypoint", catalogueRow.GetProperty("kind").GetString());
            Assert.Contains(rows, row => row.GetProperty("path").GetString() == directReferencePath);
            Assert.Contains(rows, row => row.GetProperty("path").GetString() == nestedCatalogue);
            Assert.Contains(rows, row => row.GetProperty("path").GetString() == nestedReferencePath);
        }

        var referenceContext = await workspace.RunAsync(
            "context", nestedReferencePath, "--content", "body", "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(referenceContext, "context"))
        {
            var source = Assert.Single(
                document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray(),
                item => item.GetProperty("path").GetString() == nestedReferencePath);
            var body = Assert.Single(source.GetProperty("parts").EnumerateArray());
            Assert.Equal("body", body.GetProperty("part").GetString());
            Assert.Equal(nestedReferenceText, body.GetProperty("text").GetString());
        }

        var inspected = await workspace.RunAsync(
            "route", "inspect", skillPath, "--format", "json", "--detail", "full");
        using (var document = AssertCompletedJson(inspected, "route inspect"))
        {
            Assert.Equal(skillPath, document.RootElement.GetProperty("data").GetProperty("path").GetString());
            Assert.Equal("native", document.RootElement.GetProperty("data").GetProperty("kind").GetString());
        }
    }

    [Fact]
    [Trait("Feature", Feature)]
    [Trait("Evidence", "EndToEnd")]
    public async Task MalformedSkillDescriptionBlocksAtExactPathWithoutEffects()
    {
        using var workspace = PublishedJourneyWorkspace.Create("task70-malformed-skill");
        const string skillPath = ".agents/skills/bad-native/SKILL.md";
        const string malformedSkill = "---\nname: bad-native\ndescription: [unterminated\n---\n\n# Bad native skill\n";
        workspace.ExpectCoreInstall();
        workspace.WriteText(skillPath, malformedSkill);

        var beforeDryRunWorkspace = workspace.SnapshotState();
        var beforeDryRunDataHome = SnapshotDataHome(workspace);
        var dryRun = await workspace.RunAsync(
            "install", "--automatic", "--dry-run", "--format", "json", "--detail", "full");
        using (var document = AssertBlockedJson(dryRun, "install", skillPath))
        {
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        }

        Assert.Equal(beforeDryRunWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeDryRunDataHome, SnapshotDataHome(workspace));

        var beforeApplyWorkspace = workspace.SnapshotState();
        var beforeApplyDataHome = SnapshotDataHome(workspace);
        var apply = await workspace.RunAsync(
            "install", "--automatic", "--format", "json", "--detail", "full");
        using (var document = AssertBlockedJson(apply, "install", skillPath))
        {
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
        }

        Assert.Equal(beforeApplyWorkspace, workspace.SnapshotState());
        Assert.Equal(beforeApplyDataHome, SnapshotDataHome(workspace));
        Assert.Equal(StrictUtf8.GetBytes(malformedSkill), File.ReadAllBytes(workspace.Combine(skillPath)));
        Assert.False(File.Exists(workspace.Combine(OwnershipPath)));
    }

    private static byte[] MinimalSkillBytes(string fixtureCase)
    {
        var content = fixtureCase switch
        {
            "missing-frontmatter" => SkillBody,
            "name-license-without-description" =>
                $"---\nname: local-skill\nlicense: MIT\n---\n\n{SkillBody}",
            _ => throw new ArgumentOutOfRangeException(nameof(fixtureCase), fixtureCase, "Unknown minimal fixture case."),
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
        Assert.True(closingIndex >= 0, "The minimal native Skill fixture has a closed frontmatter block.");
        return source[(closingIndex + "\n---\n".Length)..];
    }

    private static JsonElement ReadMigration(JsonElement resultRoot, string path, string outcome)
    {
        var migrations = resultRoot.GetProperty("data").GetProperty("migrations");
        Assert.Equal(JsonValueKind.Array, migrations.ValueKind);
        var row = Assert.Single(
            migrations.EnumerateArray(),
            item => string.Equals(item.GetProperty("path").GetString(), path, StringComparison.Ordinal));
        Assert.Equal(outcome, row.GetProperty("outcome").GetString());
        Assert.Equal(JsonValueKind.Array, row.GetProperty("actions").ValueKind);
        Assert.Equal(JsonValueKind.Array, row.GetProperty("fields").ValueKind);
        Assert.Equal(JsonValueKind.Array, row.GetProperty("derivation").ValueKind);
        return row.Clone();
    }

    private static string?[] ReadStrings(JsonElement array)
        => array.EnumerateArray().Select(value => value.GetString()).ToArray();

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

    private static JsonDocument AssertIndexWarnings(
        ProcessRunResult result,
        IReadOnlyCollection<string> expectedMetadataPaths,
        IReadOnlyCollection<string> expectedCurrentCatalogues,
        bool requireNoChanges)
    {
        var expectedFindings = expectedMetadataPaths
            .Select(path => ("index.metadata-optional", path))
            .ToArray();
        var document = AssertCompletedWithExactWarnings(result, "index", expectedFindings);
        var data = document.RootElement.GetProperty("data");
        if (requireNoChanges)
        {
            Assert.Empty(document.RootElement.GetProperty("effects").EnumerateArray());
            Assert.Equal(0, document.RootElement.GetProperty("counts").GetProperty("filesUpdated").GetInt32());
            Assert.Empty(data.GetProperty("changes").EnumerateArray());
            var unchanged = data.GetProperty("unchanged").EnumerateArray()
                .Select(item => item.GetProperty("path").GetString())
                .ToArray();
            foreach (var catalogue in expectedCurrentCatalogues)
            {
                Assert.Contains(catalogue, unchanged);
            }
        }

        return document;
    }

    private static JsonDocument AssertCompletedWithExactWarnings(
        ProcessRunResult result,
        string command,
        params (string Code, string Path)[] expectedFindings)
    {
        Assert.Equal(2, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(3, document.RootElement.GetProperty("schemaVersion").GetInt32());
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("completed-with-warnings", document.RootElement.GetProperty("status").GetString());

        var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();
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
        return document;
    }

    private static JsonDocument AssertBlockedJson(ProcessRunResult result, string command, string affectedPath)
    {
        Assert.Equal(5, result.ExitCode);
        Assert.NotEmpty(result.StandardOutput);
        Assert.Empty(result.StandardError);
        Assert.Contains(affectedPath, result.StandardOutput, StringComparison.Ordinal);
        var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal(command, document.RootElement.GetProperty("command").GetString());
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        return document;
    }

    private static void AssertNoMigrations(JsonElement resultRoot)
    {
        var data = resultRoot.GetProperty("data");
        if (data.TryGetProperty("migrations", out var migrations))
        {
            Assert.Equal(JsonValueKind.Array, migrations.ValueKind);
            Assert.Empty(migrations.EnumerateArray());
        }
    }

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

    private static IReadOnlyDictionary<string, string> SnapshotDataHome(PublishedJourneyWorkspace workspace)
    {
        var dataHome = workspace.ProcessEnvironment["OPENFORGE_DATA_HOME"];
        return Directory.Exists(dataHome)
            ? PublishedWorkspaceTreeSnapshot.Capture(dataHome)
            : new Dictionary<string, string>(StringComparer.Ordinal);
    }
}
