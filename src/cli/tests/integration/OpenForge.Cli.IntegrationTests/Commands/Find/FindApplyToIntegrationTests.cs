using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindApplyToIntegrationTests
{
    private const string RulesPath = ".agents/rules/_rules.md";
    private const string CSharpRulesPath = ".agents/rules/csharp.md";

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Fact(DisplayName = "Find applies inherited and local conditions to the same path without expanding its filtered universe")]
    public async Task ConditionsUseOneWorkingPathAndKeepTheFilteredSourceUniverse()
    {
        using var workspace = CreateWorkspace();
        var matching = await Run(
            workspace,
            "--format", "json",
            "--detail", "full",
            "--include", CSharpRulesPath,
            "--for", "src/Order.cs");

        Assert.Equal(0, matching.ExitCode);
        Assert.Empty(matching.Error);
        using var matchingDocument = JsonDocument.Parse(matching.Output);
        var matchingData = matchingDocument.RootElement.GetProperty("data");
        var selected = Assert.Single(Matches(matchingDocument).EnumerateArray());
        Assert.Equal(CSharpRulesPath, selected.GetProperty("path").GetString());
        AssertApplicability(
            selected,
            "matched",
            [
                (RulesPath, "src/**"),
                (CSharpRulesPath, "**/*.cs"),
            ],
            ["src/Order.cs"]);

        var sourceSet = matchingData.GetProperty("sourceSet");
        Assert.Equal(1, sourceSet.GetProperty("candidates").GetInt32());
        Assert.Single(sourceSet.GetProperty("include").EnumerateArray());
        Assert.Empty(sourceSet.GetProperty("exclude").EnumerateArray());
        Assert.DoesNotContain(
            Matches(matchingDocument).EnumerateArray(),
            match => match.GetProperty("path").GetString() == RulesPath);

        var splitAcrossFiles = await Run(
            workspace,
            "--format", "json",
            "--detail", "full",
            "--include", CSharpRulesPath,
            "--for", "src/readme.md",
            "--for", "tests/Order.cs");

        Assert.Equal(0, splitAcrossFiles.ExitCode);
        Assert.Empty(splitAcrossFiles.Error);
        using var splitDocument = JsonDocument.Parse(splitAcrossFiles.Output);
        Assert.Empty(Matches(splitDocument).EnumerateArray());
        var splitSourceSet = splitDocument.RootElement.GetProperty("data").GetProperty("sourceSet");
        Assert.Equal(1, splitSourceSet.GetProperty("candidates").GetInt32());
        Assert.Single(splitSourceSet.GetProperty("include").EnumerateArray());
        Assert.Empty(splitSourceSet.GetProperty("exclude").EnumerateArray());
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Fact(DisplayName = "Find combines file compatibility with tag and heading predicates and keeps unconditioned sources eligible")]
    public async Task FileCompatibilityIsAnIndependentFilterAtStandardAndFullDetail()
    {
        using var workspace = CreateWorkspace();
        var standard = await Run(
            workspace,
            "--format", "json",
            "--detail", "standard",
            "--tag", "Selected",
            "--heading", "Needle",
            "--require", "all",
            "--for", "src/Order.cs");
        var full = await Run(
            workspace,
            "--format", "json",
            "--detail", "full",
            "--tag", "Selected",
            "--heading", "Needle",
            "--require", "all",
            "--for", "src/Order.cs");

        Assert.Equal(0, standard.ExitCode);
        Assert.Equal(0, full.ExitCode);
        Assert.Empty(standard.Error);
        Assert.Empty(full.Error);
        using var standardDocument = JsonDocument.Parse(standard.Output);
        using var fullDocument = JsonDocument.Parse(full.Output);

        AssertEligibleResults(standardDocument);
        AssertEligibleResults(fullDocument);
        AssertApplicability(
            MatchAtPath(standardDocument, CSharpRulesPath),
            "matched",
            [
                (RulesPath, "src/**"),
                (CSharpRulesPath, "**/*.cs"),
            ],
            ["src/Order.cs"]);
        AssertApplicability(
            MatchAtPath(fullDocument, CSharpRulesPath),
            "matched",
            [
                (RulesPath, "src/**"),
                (CSharpRulesPath, "**/*.cs"),
            ],
            ["src/Order.cs"]);
        AssertApplicability(MatchAtPath(standardDocument, ".agents/plain.md"), "unconditioned", [], []);
        AssertApplicability(MatchAtPath(fullDocument, ".agents/plain.md"), "unconditioned", [], []);

        var anyPredicate = await Run(
            workspace,
            "--format", "json",
            "--detail", "standard",
            "--tag", "Selected",
            "--heading", "Absent",
            "--require", "any",
            "--for", "src/Order.cs");

        Assert.Equal(0, anyPredicate.ExitCode);
        using var anyPredicateDocument = JsonDocument.Parse(anyPredicate.Output);
        Assert.Equal(
            new[] { ".agents/plain.md", CSharpRulesPath },
            Paths(Matches(anyPredicateDocument)).OrderBy(path => path, StringComparer.Ordinal));
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Fact(DisplayName = "Find without file paths preserves discovery and reports conditioned matches as pending")]
    public async Task OmittedForPreservesLegacyMatchesAndLeavesConditionsPending()
    {
        using var workspace = CreateWorkspace();
        var result = await Run(
            workspace,
            "--format", "json",
            "--detail", "full",
            "--tag", "Selected",
            "--heading", "Needle",
            "--require", "all");

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(
            new[] { ".agents/plain.md", CSharpRulesPath, ".agents/rules/other-path.md" },
            Paths(Matches(document)).OrderBy(path => path, StringComparer.Ordinal));

        foreach (var path in new[] { CSharpRulesPath, ".agents/rules/other-path.md" })
        {
            var applicability = MatchAtPath(document, path).GetProperty("applicability");
            Assert.Equal("pending", applicability.GetProperty("state").GetString());
            Assert.Empty(applicability.GetProperty("matchingPaths").EnumerateArray());
        }

        Assert.False(MatchAtPath(document, ".agents/plain.md").TryGetProperty("applicability", out _));
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Fact(DisplayName = "Find normalizes repeated absolute and relative paths for a planned file that does not exist")]
    public async Task RepeatedAbsoluteAndRelativePathsNormalizeWithoutExistenceChecks()
    {
        using var workspace = CreateWorkspace();
        var plannedPath = workspace.Combine("src/Order.cs");
        Assert.False(File.Exists(plannedPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(plannedPath)));

        var result = await Run(
            workspace,
            "--format", "json",
            "--detail", "standard",
            "--tag", "Selected",
            "--heading", "Needle",
            "--require", "all",
            "--for", "src/Order.cs",
            "--for", plannedPath,
            "--for", "src/../src/Order.cs");

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        AssertApplicability(
            MatchAtPath(document, CSharpRulesPath),
            "matched",
            [
                (RulesPath, "src/**"),
                (CSharpRulesPath, "**/*.cs"),
            ],
            ["src/Order.cs"]);
        AssertApplicability(MatchAtPath(document, ".agents/plain.md"), "unconditioned", [], []);
        Assert.False(File.Exists(plannedPath));
        Assert.False(Directory.Exists(Path.GetDirectoryName(plannedPath)));
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Fact(DisplayName = "Find applies file patterns case-sensitively")]
    public async Task FilePatternCaseDoesNotDependOnTheHostFilesystem()
    {
        using var workspace = CreateWorkspace();
        var result = await Run(
            workspace,
            "--format", "json",
            "--detail", "full",
            "--include", CSharpRulesPath,
            "--for", "src/Order.CS");

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Empty(Matches(document).EnumerateArray());
        Assert.Equal(1, document.RootElement.GetProperty("data").GetProperty("sourceSet").GetProperty("candidates").GetInt32());
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Theory(DisplayName = "Find rejects invalid, outside, and workspace-root file paths")]
    [InlineData("invalid"), InlineData("relative-outside"), InlineData("absolute-outside"), InlineData("workspace-root")]
    public async Task InvalidForPathsUseTheExistingInvalidInputResult(string scenario)
    {
        using var workspace = CreateWorkspace();
        var path = scenario switch
        {
            "invalid" => "\u0001",
            "relative-outside" => "../Order.cs",
            "absolute-outside" => Path.GetFullPath(Path.Combine(workspace.Path, "..", "outside.cs")),
            "workspace-root" => workspace.Path,
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The invalid path scenario is not defined."),
        };

        var result = await Run(workspace, "--format", "json", "--for", path);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "find.invalid-input");
    }

    [Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
    [Theory(DisplayName = "Find keeps require and within dependent on tag or heading predicates")]
    [InlineData("--require", "all"), InlineData("--within", "frontmatter")]
    public async Task ForDoesNotMakeRequireOrWithinValidWithoutTagOrHeading(string option, string value)
    {
        using var workspace = CreateWorkspace();
        var result = await Run(workspace, "--format", "json", "--for", "src/Order.cs", option, value);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "find.invalid-input");
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("find-apply-to");
        try
        {
            workspace.WriteText(".agents/loader.md", """
                ---
                open-forge:
                  description: Loader
                  tags: [LoadNow]
                ---
                # Loader
                """);
            workspace.WriteText(RulesPath, """
                ---
                applyTo: ["src/**"]
                open-forge:
                  description: Rules
                  tags: [Directive]
                ---
                # Rules

                ## Entries

                - [C# rules](csharp.md) - #Directive
                - [Other tag](other-tag.md) - #Directive
                - [Other path](other-path.md) - #Directive
                """);
            workspace.WriteText(CSharpRulesPath, """
                ---
                open-forge:
                  description: C sharp rules
                  tags: [Selected]
                  applyTo: ["**/*.cs"]
                ---
                # C sharp rules

                ## Needle
                """);
            workspace.WriteText(".agents/rules/other-tag.md", """
                ---
                open-forge:
                  description: Other tag
                  tags: [OtherTag]
                  applyTo: ["**/*.cs"]
                ---
                # Other tag

                ## Needle
                """);
            workspace.WriteText(".agents/rules/other-path.md", """
                ---
                open-forge:
                  description: Other path
                  tags: [Selected]
                  applyTo: ["docs/**/*.md"]
                ---
                # Other path

                ## Needle
                """);
            workspace.WriteText(".agents/plain.md", """
                ---
                open-forge:
                  description: Plain guidance
                  tags: [Selected]
                ---
                # Plain guidance

                ## Needle
                """);
            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static async Task<CliHostCaptureResult> Run(TemporaryWorkspace workspace, params string[] options)
    {
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(
            ["find", "--workspace", workspace.Path, .. options],
            workspace.Path);
        Assert.Equal(before, workspace.SnapshotHashes());
        return result;
    }

    private static JsonElement Matches(JsonDocument document)
        => document.RootElement.GetProperty("data").GetProperty("matches");

    private static JsonElement MatchAtPath(JsonDocument document, string path)
        => Assert.Single(Matches(document).EnumerateArray(), match => match.GetProperty("path").GetString() == path);

    private static string[] Paths(JsonElement matches)
        => matches.EnumerateArray()
            .Select(match => match.GetProperty("path").GetString())
            .OfType<string>()
            .ToArray();

    private static void AssertEligibleResults(JsonDocument document)
    {
        Assert.Equal(
            new[] { ".agents/plain.md", CSharpRulesPath },
            Paths(Matches(document)).OrderBy(path => path, StringComparer.Ordinal));
    }

    private static void AssertApplicability(
        JsonElement match,
        string expectedState,
        (string Source, string Pattern)[] expectedConditions,
        string[] expectedMatchingPaths)
    {
        var applicability = match.GetProperty("applicability");
        Assert.Equal(expectedState, applicability.GetProperty("state").GetString());
        var conditions = applicability.GetProperty("conditions").EnumerateArray().ToArray();
        Assert.Equal(expectedConditions.Length, conditions.Length);
        for (var index = 0; index < expectedConditions.Length; index++)
        {
            Assert.Equal(expectedConditions[index].Source, conditions[index].GetProperty("source").GetString());
            Assert.Equal(
                new[] { expectedConditions[index].Pattern },
                conditions[index].GetProperty("patterns").EnumerateArray()
                    .Select(pattern => pattern.GetString())
                    .OfType<string>());
        }

        Assert.Equal(
            expectedMatchingPaths,
            applicability.GetProperty("matchingPaths").EnumerateArray()
                .Select(path => path.GetString())
                .OfType<string>());
    }
}
