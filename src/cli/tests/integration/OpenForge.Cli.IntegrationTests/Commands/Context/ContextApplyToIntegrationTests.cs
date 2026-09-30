using System.Text.Json;
using OpenForge.Cli.Hosting;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Context;

public sealed class ContextApplyToIntegrationTests
{
    [Fact(DisplayName = "Context text renders pending startup conditions at every detail"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task PendingStartupConditionRendersAtEveryDetail()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Directives", "directives/_directives.md", ["LoadNow", "Core", "Directive"]),
            EntryPointFile(
                ".agents/directives/_directives.md",
                "Required instructions",
                ["LoadNow", "Core", "Directive"],
                null,
                Entry("C# style rules for every C# file", "csharp.md", ["Directive", "LoadNow"], ["**/*.cs"])),
            DocumentFile(
                ".agents/directives/csharp.md",
                "C# style rules for every C# file",
                ["Directive", "LoadNow"],
                ["**/*.cs"],
                "# CSharp\n\nFollow C# style rules.\n"));
        var before = workspace.SnapshotHashes();

        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
        {
            foreach (var pathsOnly in new[] { false, true })
            {
                string[] projection = pathsOnly ? ["--content", "paths"] : [];
                var result = await RunContextAsync(workspace, ["--detail", detail, .. projection]);

                Assert.Equal(3, result.ExitCode);
                if (detail == "debug")
                {
                    Assert.True(string.IsNullOrWhiteSpace(result.Error), result.Error);
                }
                else
                {
                    Assert.Empty(result.Error);
                }
                var situation = pathsOnly ? "pending-startup-paths" : "pending-startup";
                snapshots.Add($"{situation}.{detail}", CommandOutputNormalization.Normalize(
                    result.Output, workspace.Path, CliBuildVersion.InformationalVersion, normalizeTextPaths: true));

                var json = await RunContextAsync(workspace, ["--detail", detail, "--format", "json", .. projection]);
                Assert.Equal(3, json.ExitCode);
                using var document = JsonDocument.Parse(json.Output);
                Assert.Equal("incomplete", document.RootElement.GetProperty("status").GetString());
                Assert.Equal("open-forge context --for <path>", document.RootElement.GetProperty("next").GetProperty("command").GetString());
                snapshots.Add($"{situation}.json.{detail}", CommandOutputNormalization.Normalize(
                    json.Output, workspace.Path, CliBuildVersion.InformationalVersion));
            }
        }

        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);

        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context does not automatically include an exposed matching untagged applyTo entry"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task MatchingExposedConditionDoesNotLoadUntaggedChild()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("CSharp", "csharp.md", [], ["src/**/*.cs"]),
                Entry("Web", "web.md", [], ["web/**/*.ts"])),
            DocumentFile(
                ".agents/rules/csharp.md",
                "CSharp",
                ["Rule"],
                ["src/**/*.cs"],
                "# CSharp\n\nCSharp rule.\n"),
            DocumentFile(
                ".agents/rules/web.md",
                "Web",
                ["Rule"],
                ["web/**/*.ts"],
                "# Web\n\nWeb rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "rules",
            "--for", "src/Order.cs",
            "--format", "json",
            "--detail", "standard");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        var paths = SourcePaths(root);
        Assert.Contains(".agents/rules/_rules.md", paths);
        Assert.DoesNotContain(".agents/rules/csharp.md", paths);
        Assert.DoesNotContain(".agents/rules/web.md", paths);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context closure matches and rejects comma-separated applyTo expressions"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task CommaSeparatedExpressionControlsContextClosure()
    {
        using var workspace = CreatePatternClosureWorkspace(
            ["src/**/*.cs", "tests/**/*.cs"],
            "src/**/*.cs,tests/**/*.cs",
            "# CSharp\n\nComma-separated rule.\n");
        var before = workspace.SnapshotHashes();

        var matching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "tests/Order.cs",
            "--format", "json");
        var nonmatching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "docs/readme.md",
            "--format", "json");

        AssertSuccessfulCommand(matching);
        AssertSuccessfulCommand(nonmatching);
        using var matchingDocument = JsonDocument.Parse(matching.Output);
        using var nonmatchingDocument = JsonDocument.Parse(nonmatching.Output);
        Assert.Contains(".agents/rules/csharp.md", SourcePaths(matchingDocument.RootElement));
        Assert.DoesNotContain(".agents/rules/csharp.md", SourcePaths(nonmatchingDocument.RootElement));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context closure matches and rejects brace applyTo patterns"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task BraceExpressionControlsContextClosure()
    {
        using var workspace = CreatePatternClosureWorkspace(
            ["{src,test}/**/*.cs"],
            "{src,test}/**/*.cs",
            "# CSharp\n\nBrace rule.\n");
        var before = workspace.SnapshotHashes();

        var matching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "test/Order.cs",
            "--format", "json");
        var nonmatching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "docs/readme.md",
            "--format", "json");

        AssertSuccessfulCommand(matching);
        AssertSuccessfulCommand(nonmatching);
        using var matchingDocument = JsonDocument.Parse(matching.Output);
        using var nonmatchingDocument = JsonDocument.Parse(nonmatching.Output);
        Assert.Contains(".agents/rules/csharp.md", SourcePaths(matchingDocument.RootElement));
        Assert.DoesNotContain(".agents/rules/csharp.md", SourcePaths(nonmatchingDocument.RootElement));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context closure matches and rejects class applyTo patterns"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ClassExpressionControlsContextClosure()
    {
        using var workspace = CreatePatternClosureWorkspace(
            ["src/[OT]*.cs"],
            "src/[OT]*.cs",
            "# CSharp\n\nClass rule.\n");
        var before = workspace.SnapshotHashes();

        var matching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "src/Order.cs",
            "--format", "json");
        var nonmatching = await RunContextAsync(
            workspace,
            "rules",
            "--for", "src/Json.cs",
            "--format", "json");

        AssertSuccessfulCommand(matching);
        AssertSuccessfulCommand(nonmatching);
        using var matchingDocument = JsonDocument.Parse(matching.Output);
        using var nonmatchingDocument = JsonDocument.Parse(nonmatching.Output);
        Assert.Contains(".agents/rules/csharp.md", SourcePaths(matchingDocument.RootElement));
        Assert.DoesNotContain(".agents/rules/csharp.md", SourcePaths(nonmatchingDocument.RootElement));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context pending action preserves selected sources and resolves their conditions"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnknownWorkingPathsReportPendingConditionWithContentProjection()
    {
        var snapshots = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var multipleSources in new[] { false, true })
        {
            using var workspace = CreateWorkspace(
                LoaderEntry("Rules", "rules/_rules.md"),
                EntryPointFile(
                    ".agents/rules/_rules.md",
                    "Rules",
                    ["Project"],
                    null,
                    Entry("CSharp", "csharp.md", ["LoadNow"], ["src/**/*.cs"])),
                DocumentFile(
                    ".agents/rules/csharp.md",
                    "CSharp",
                    ["LoadNow", "Rule"],
                    ["src/**/*.cs"],
                    "# CSharp\n\nCSharp rule.\n"),
                DocumentFile(
                    ".agents/extra guide.md",
                    "Extra guide",
                    ["Guide"],
                    null,
                    "# Extra guide\n\nAdditional guidance.\n"));
            var before = workspace.SnapshotHashes();

            string[] references = multipleSources ? ["rules", ".agents/extra guide.md"] : ["rules"];
            var result = await RunContextAsync(
                workspace,
                [.. references, "--content=metadata,paths", "--format", "json"]);

            Assert.Equal(3, result.ExitCode);
            Assert.Equal(string.Empty, result.Error);
            using var document = JsonDocument.Parse(result.Output);
            var root = document.RootElement;
            Assert.Equal("incomplete", root.GetProperty("status").GetString());
            var finding = Assert.Single(root.GetProperty("findings").EnumerateArray());
            Assert.Equal("context.applicability-pending", finding.GetProperty("code").GetString());
            var expectedCommand = multipleSources
                ? "open-forge context \"rules\" \".agents/extra guide.md\" --for <path>"
                : "open-forge context \"rules\" --for <path>";
            var nextCommand = root.GetProperty("next").GetProperty("command").GetString();
            Assert.Equal(expectedCommand, nextCommand);
            Assert.NotNull(nextCommand);
            Assert.Equal("workspace", finding.GetProperty("subject").GetProperty("kind").GetString());
            Assert.Equal(workspace.Path, finding.GetProperty("subject").GetProperty("path").GetString());
            var pending = root.GetProperty("data").GetProperty("pendingConditions").EnumerateArray().ToArray();
            var condition = Assert.Single(pending);
            Assert.Equal(".agents/rules/csharp.md", condition.GetProperty("source").GetString());
            Assert.Equal(["src/**/*.cs"], condition.GetProperty("patterns").EnumerateArray().Select(pattern => pattern.GetString()));
            Assert.DoesNotContain(".agents/rules/csharp.md", SourcePaths(root));
            Assert.All(
                root.GetProperty("data").GetProperty("sources").EnumerateArray(),
                source => Assert.Contains(
                    source.GetProperty("parts").EnumerateArray(),
                    part => part.GetProperty("part").GetString() == "paths"));

            var situation = multipleSources ? "pending-multiple-sources" : "pending-selected-source";
            foreach (var detail in new[] { "minimal", "standard", "full", "debug" })
            {
                var json = detail == "minimal"
                    ? result
                    : await RunContextAsync(workspace, [.. references, "--content=metadata,paths", "--format", "json", "--detail", detail]);
                var text = await RunContextAsync(workspace, [.. references, "--content=metadata,paths", "--detail", detail]);
                Assert.Equal(3, json.ExitCode);
                Assert.Equal(3, text.ExitCode);
                Assert.True(string.IsNullOrWhiteSpace(json.Error), json.Error);
                Assert.True(string.IsNullOrWhiteSpace(text.Error), text.Error);
                snapshots.Add($"{situation}.json.{detail}", CommandOutputNormalization.Normalize(
                    json.Output, workspace.Path, CliBuildVersion.InformationalVersion));
                snapshots.Add($"{situation}.{detail}", CommandOutputNormalization.Normalize(
                    text.Output, workspace.Path, CliBuildVersion.InformationalVersion, normalizeTextPaths: true));
            }
            var next = await CliHostCapture.RunAsync(
                System.CommandLine.Parsing.CommandLineParser.SplitCommandLine(
                    nextCommand.Replace("open-forge ", string.Empty, StringComparison.Ordinal)
                        .Replace("<path>", "src/Order.cs", StringComparison.Ordinal)).ToArray(),
                workspace.Path);
            AssertSuccessfulCommand(next);
            Assert.Contains("CSharp rule.", next.Output, StringComparison.Ordinal);
            if (multipleSources)
            {
                Assert.Contains("Additional guidance.", next.Output, StringComparison.Ordinal);
            }
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        CommandOutputSnapshot.MatchDetailSnapshot(snapshots);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context ANDs ancestor conditions on one path and leaves hidden ancestors unselected"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task AncestorConditionsUseSamePathAndHiddenAncestorStaysUnselected()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Products", "products/_products.md"),
            EntryPointFile(
                ".agents/products/_products.md",
                "Products",
                ["Project"],
                ["src/**"],
                Entry("Managed", "managed/_managed.md", ["LoadNow"], ["**/*.cs"])),
            EntryPointFile(
                ".agents/products/managed/_managed.md",
                "Managed",
                ["LoadNow", "Project"],
                ["**/*.cs"],
                Entry("CSharp", "csharp.md", ["LoadNow"])),
            DocumentFile(
                ".agents/products/managed/csharp.md",
                "CSharp",
                ["LoadNow", "Rule"],
                null,
                "# CSharp\n\nScoped rule.\n"),
            EntryPointFile(
                ".agents/hidden/_hidden.md",
                "Hidden",
                ["Project"],
                ["src/**"],
                Entry("Hidden CSharp", "csharp.md", [], ["**/*.cs"])),
            DocumentFile(
                ".agents/hidden/csharp.md",
                "Hidden CSharp",
                ["Rule"],
                ["**/*.cs"],
                "# Hidden\n\nHidden rule.\n"));
        var before = workspace.SnapshotHashes();

        var splitPaths = await RunContextAsync(
            workspace,
            "products",
            "--for", "src/readme.md",
            "--for", "tests/A.cs",
            "--format", "json");
        var matchingPath = await RunContextAsync(
            workspace,
            "products",
            "--for", "src/A.cs",
            "--format", "json");
        var hiddenRoute = await RunContextAsync(
            workspace,
            "--for", "src/A.cs",
            "--format", "json");

        AssertSuccessfulCommand(splitPaths);
        AssertSuccessfulCommand(matchingPath);
        AssertSuccessfulCommand(hiddenRoute);
        using var splitDocument = JsonDocument.Parse(splitPaths.Output);
        using var matchingDocument = JsonDocument.Parse(matchingPath.Output);
        using var hiddenDocument = JsonDocument.Parse(hiddenRoute.Output);
        Assert.DoesNotContain(".agents/products/managed/_managed.md", SourcePaths(splitDocument.RootElement));
        Assert.Contains(".agents/products/managed/_managed.md", SourcePaths(matchingDocument.RootElement));
        Assert.Contains(".agents/products/managed/csharp.md", SourcePaths(matchingDocument.RootElement));
        Assert.DoesNotContain(".agents/hidden/_hidden.md", SourcePaths(hiddenDocument.RootElement));
        Assert.DoesNotContain(".agents/hidden/csharp.md", SourcePaths(hiddenDocument.RootElement));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context explicitly inspects a nonmatching source with ancestors and overwrite but no automatic child"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ExplicitNonmatchIncludesAncestorAndOverwriteWithoutAutomaticChild()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("Guide", "guide/_guide.md", [], ["src/**/*.cs"])),
            EntryPointFile(
                ".agents/rules/guide/_guide.md",
                "Guide",
                ["Project"],
                ["src/**/*.cs"],
                Entry("Automatic", "automatic.md", [], ["docs/**/*.md"])),
            DocumentFile(
                ".agents/rules/guide/_guide.overwrite.md",
                "Guide overwrite",
                ["Project"],
                null,
                "# Guide overwrite\n\nOverwrite.\n"),
            DocumentFile(
                ".agents/rules/guide/automatic.md",
                "Automatic",
                ["Rule"],
                ["docs/**/*.md"],
                "# Automatic\n\nAutomatic rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "rules/guide",
            "--for", "docs/readme.md",
            "--format", "json",
            "--detail", "standard");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        var paths = SourcePaths(root);
        Assert.Contains(".agents/rules/_rules.md", paths);
        Assert.Contains(".agents/rules/guide/_guide.md", paths);
        Assert.Contains(".agents/rules/guide/_guide.overwrite.md", paths);
        Assert.DoesNotContain(".agents/rules/guide/automatic.md", paths);
        Assert.Equal("unmatched", Source(root, ".agents/rules/guide/_guide.md")
            .GetProperty("applicability").GetProperty("state").GetString());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context follows a nonmatching reference with ancestors and overwrite without its automatic child"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task ReferencedNonmatchIncludesAncestorsAndOverwriteWithoutAutomaticChild()
    {
        using var workspace = CreateWorkspace(
            $"{LoaderEntry("Docs", "docs/_docs.md")}\n{LoaderEntry("Projects", "projects/_projects.md")}",
            EntryPointFile(
                ".agents/docs/_docs.md",
                "Docs",
                ["Project"],
                null,
                Entry("Start", "start.md", ["Guide"])),
            DocumentFile(
                ".agents/docs/start.md",
                "Start",
                ["Guide"],
                null,
                "# Start\n\nRead [Reference](../projects/reference/_reference.md).\n"),
            EntryPointFile(
                ".agents/projects/_projects.md",
                "Projects",
                ["Project"],
                null,
                Entry("Reference", "reference/_reference.md", [], ["src/**/*.cs"])),
            EntryPointFile(
                ".agents/projects/reference/_reference.md",
                "Reference",
                ["Project"],
                ["src/**/*.cs"],
                Entry("Automatic", "automatic.md", [], ["docs/**/*.md"])),
            DocumentFile(
                ".agents/projects/reference/_reference.overwrite.md",
                "Reference overwrite",
                ["Project"],
                null,
                "# Reference overwrite\n\nOverwrite.\n"),
            DocumentFile(
                ".agents/projects/reference/automatic.md",
                "Automatic",
                ["Rule"],
                ["docs/**/*.md"],
                "# Automatic\n\nAutomatic rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "docs/start",
            "--for", "docs/task.md",
            "--follow-links", "all",
            "--format", "json",
            "--detail", "full");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        var paths = SourcePaths(root);
        Assert.Contains(".agents/projects/_projects.md", paths);
        Assert.Contains(".agents/projects/reference/_reference.md", paths);
        Assert.Contains(".agents/projects/reference/_reference.overwrite.md", paths);
        Assert.DoesNotContain(".agents/projects/reference/automatic.md", paths);
        Assert.Contains(
            root.GetProperty("data").GetProperty("links").EnumerateArray(),
            link => link.GetProperty("resolvedPath").GetString() == ".agents/projects/reference/_reference.md"
                && link.GetProperty("followed").GetBoolean());
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context additions-only subtracts startup under the same working path set"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task AdditionsOnlyUsesSameWorkingPathsForStartupAndCombinedSelection()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Startup", "startup/_startup.md", ["LoadNow", "Core"]),
            EntryPointFile(
                ".agents/startup/_startup.md",
                "Startup",
                ["LoadNow", "Core"],
                null,
                Entry("Matching", "matching.md", ["LoadNow"], ["src/**/*.cs"]),
                Entry("Other", "other.md", ["LoadNow"], ["docs/**/*.md"])),
            DocumentFile(
                ".agents/startup/matching.md",
                "Matching",
                ["LoadNow"],
                ["src/**/*.cs"],
                "# Matching\n\nStartup rule.\n"),
            DocumentFile(
                ".agents/startup/other.md",
                "Other",
                ["LoadNow"],
                ["docs/**/*.md"],
                "# Other\n\nNonmatching rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "startup",
            "--additions-only",
            "--for", "src/App.cs",
            "--format", "json");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Empty(SourcePaths(root));
        Assert.Equal(["src/App.cs"], root.GetProperty("data").GetProperty("workingPaths")
            .EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context without applyTo and without for preserves unconditioned source output"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task UnconditionedContextWithoutForRetainsLegacyProjection()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("Note", "note.md", ["Guide"])),
            DocumentFile(
                ".agents/rules/note.md",
                "Note",
                ["Guide"],
                null,
                "# Note\n\nUnconditioned content.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(workspace, "rules/note", "--format", "json");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Equal(
            ["AGENTS.md", ".agents/loader.md", ".agents/rules/_rules.md", ".agents/rules/note.md"],
            SourcePaths(root));
        Assert.False(root.GetProperty("data").TryGetProperty("workingPaths", out _));
        Assert.False(root.GetProperty("data").TryGetProperty("pendingConditions", out _));
        Assert.All(
            root.GetProperty("data").GetProperty("sources").EnumerateArray(),
            source => Assert.False(source.TryGetProperty("applicability", out _)));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context matches separate related CSharp and TypeScript sources across the working set"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task RelatedLanguageSourcesMatchDifferentWorkingPaths()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("CSharp", "csharp.md", ["LoadNow"], ["src/**/*.cs"]),
                Entry("TypeScript", "typescript.md", ["LoadNow"], ["web/**/*.ts"])),
            DocumentFile(
                ".agents/rules/csharp.md",
                "CSharp",
                ["LoadNow", "Rule"],
                ["src/**/*.cs"],
                "# CSharp\n\nCSharp rule.\n"),
            DocumentFile(
                ".agents/rules/typescript.md",
                "TypeScript",
                ["LoadNow", "Rule"],
                ["web/**/*.ts"],
                "# TypeScript\n\nTypeScript rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "rules",
            "--for", "src/Order.cs",
            "--for", "web/order.ts",
            "--format", "json",
            "--detail", "standard");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        var paths = SourcePaths(root);
        Assert.Contains(".agents/rules/csharp.md", paths);
        Assert.Contains(".agents/rules/typescript.md", paths);
        Assert.Equal(["src/Order.cs"], Source(root, ".agents/rules/csharp.md")
            .GetProperty("applicability").GetProperty("matchingPaths")
            .EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(["web/order.ts"], Source(root, ".agents/rules/typescript.md")
            .GetProperty("applicability").GetProperty("matchingPaths")
            .EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context matches a planned path that does not exist yet"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task PlannedMissingPathCanMatchApplyTo()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("CSharp", "csharp.md", ["LoadNow"], ["src/**/*.cs"])),
            DocumentFile(
                ".agents/rules/csharp.md",
                "CSharp",
                ["LoadNow", "Rule"],
                ["src/**/*.cs"],
                "# CSharp\n\nPlanned rule.\n"));
        const string plannedPath = "src/new-feature.cs";
        Assert.False(File.Exists(workspace.Combine(plannedPath)));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "rules",
            "--for", plannedPath,
            "--format", "json",
            "--detail", "standard");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Contains(".agents/rules/csharp.md", SourcePaths(root));
        Assert.Equal([plannedPath], Source(root, ".agents/rules/csharp.md")
            .GetProperty("applicability").GetProperty("matchingPaths")
            .EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Context discards a LoadNow entry with a nonmatching applyTo condition"), Trait("Feature", "context"), Trait("Evidence", "Integration")]
    public async Task LoadNowNonmatchingConditionIsDiscarded()
    {
        using var workspace = CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("CSharp", "csharp.md", ["LoadNow"], ["src/**/*.cs"])),
            DocumentFile(
                ".agents/rules/csharp.md",
                "CSharp",
                ["LoadNow", "Rule"],
                ["src/**/*.cs"],
                "# CSharp\n\nCSharp rule.\n"));
        var before = workspace.SnapshotHashes();

        var result = await RunContextAsync(
            workspace,
            "rules",
            "--for", "docs/readme.md",
            "--format", "json",
            "--detail", "standard");

        AssertSuccessfulCommand(result);
        using var document = JsonDocument.Parse(result.Output);
        var root = document.RootElement;
        Assert.Equal("completed", root.GetProperty("status").GetString());
        Assert.Contains(".agents/rules/_rules.md", SourcePaths(root));
        Assert.DoesNotContain(".agents/rules/csharp.md", SourcePaths(root));
        Assert.False(root.GetProperty("data").TryGetProperty("pendingConditions", out _));
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static TemporaryWorkspace CreateWorkspace(string loaderEntries, params SeedFile[] files)
    {
        var workspace = TemporaryWorkspace.Create("context-apply-to");
        try
        {
            workspace.WriteText("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
            workspace.WriteText(
                ".agents/loader.md",
                $"# Loader\n\n## Entries\n\n{loaderEntries}\n");
            foreach (var file in files)
            {
                workspace.WriteText(file.Path, file.Content);
            }

            return workspace;
        }
        catch
        {
            workspace.Dispose();
            throw;
        }
    }

    private static SeedFile EntryPointFile(
        string path,
        string description,
        IReadOnlyList<string> tags,
        IReadOnlyList<string>? applyTo,
        params string[] entries)
    {
        var generatedEntries = OpenForgeDocumentSeed.GeneratedEntries(new GeneratedEntriesSeed
        {
            Entries = string.Join('\n', entries),
            Prefix = $"# {description}",
        });
        return DocumentFile(path, description, tags, applyTo, generatedEntries);
    }

    private static SeedFile DocumentFile(
        string path,
        string description,
        IReadOnlyList<string> tags,
        IReadOnlyList<string>? applyTo,
        string body)
    {
        var tagsText = string.Join(", ", tags);
        var applyToLine = applyTo is { Count: > 0 }
            ? $"  applyTo: [{string.Join(", ", applyTo.Select(pattern => $"\"{pattern}\""))}]\n"
            : string.Empty;
        var content = $"---\nopen-forge:\n  description: {description}\n  tags: [{tagsText}]\n{applyToLine}---\n{body}";
        return new SeedFile(path, content);
    }

    private static TemporaryWorkspace CreatePatternClosureWorkspace(
        IReadOnlyList<string> entryPatterns,
        string applyToExpression,
        string body)
        => CreateWorkspace(
            LoaderEntry("Rules", "rules/_rules.md"),
            EntryPointFile(
                ".agents/rules/_rules.md",
                "Rules",
                ["Project"],
                null,
                Entry("CSharp", "csharp.md", ["LoadNow"], entryPatterns)),
            DocumentFileWithExpression(
                ".agents/rules/csharp.md",
                "CSharp",
                ["LoadNow", "Rule"],
                applyToExpression,
                body));

    private static SeedFile DocumentFileWithExpression(
        string path,
        string description,
        IReadOnlyList<string> tags,
        string applyToExpression,
        string body)
    {
        var tagsText = string.Join(", ", tags);
        var content = $"---\nopen-forge:\n  description: {description}\n  tags: [{tagsText}]\n  applyTo: \"{applyToExpression}\"\n---\n{body}";
        return new SeedFile(path, content);
    }

    private static string Entry(
        string label,
        string target,
        IReadOnlyList<string> tags,
        IReadOnlyList<string>? applyTo = null)
    {
        var tagSuffix = tags.Count > 0 ? $" - {string.Join(' ', tags.Select(tag => $"#{tag}"))}" : string.Empty;
        var applyToSuffix = applyTo is { Count: > 0 }
            ? $" - applies to {string.Join(", ", applyTo.Select(pattern => $"`{pattern}`"))}"
            : string.Empty;
        return $"- [{label}]({target}){tagSuffix}{applyToSuffix}";
    }

    private static string LoaderEntry(string label, string target, IReadOnlyList<string>? tags = null)
        => Entry(label, target, tags ?? ["Project"]);

    private static Task<CliHostCaptureResult> RunContextAsync(
        TemporaryWorkspace workspace,
        params string[] arguments)
        => CliHostCapture.RunAsync(["context", .. arguments], workspace.Path);

    private static string[] SourcePaths(JsonElement root)
        => root.GetProperty("data").GetProperty("sources").EnumerateArray()
            .Select(source => source.GetProperty("path").GetString()!)
            .ToArray();

    private static void AssertSuccessfulCommand(CliHostCaptureResult result)
    {
        Assert.True(
            result.ExitCode == 0,
            $"Expected exit code 0, got {result.ExitCode}.\nStderr:\n{result.Error}\nStdout:\n{result.Output}");
        Assert.Equal(string.Empty, result.Error);
    }

    private static JsonElement Source(JsonElement root, string path)
        => Assert.Single(
            root.GetProperty("data").GetProperty("sources").EnumerateArray(),
            source => source.GetProperty("path").GetString() == path);

    private sealed record SeedFile(string Path, string Content);
}
