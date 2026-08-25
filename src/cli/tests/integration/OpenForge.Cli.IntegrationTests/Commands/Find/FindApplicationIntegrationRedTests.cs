using System.Text.Json;
using System.Runtime.CompilerServices;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Rendering;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace;
using OpenForge.Cli.Core.Shell.Composition;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Composition;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindApplicationIntegrationRedTests
{
    [Fact(DisplayName = "Composed CliCompositionRoot registers Find as a direct root leaf with one exact binding"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersFindAsDirectRootLeaf()
    {
        var application = CliCompositionRoot.Create(
            new CliProcessIdentity("open-forge", "test"));
        var parser = CliCoreApplicationAccess.Parser(application);
        var tree = CliParserAccess.Tree(parser);
        var parse = tree.Parse(["find"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Equal("find", selection.Command.Name);
        Assert.False(tree.IsGroup(selection.Command));
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        var binding = tree.FindBinding(selection.Command);
        Assert.NotNull(binding);
        Assert.Same(binding, selection.Binding);
        Assert.Same(selection.Command, binding!.Command);
    }

    [Fact(DisplayName = "Composed root help exposes the direct Find leaf exactly once in Discovery"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRootHelpExposesFindOnceInDiscovery()
    {
        using var workspace = FindWorkspace.CreateBare();
        var missing = workspace.Combine("missing-root-help-workspace");
        AssertPathAbsent(missing);

        var result = await RunWithoutWrites(
            workspace,
            ["--help", "--workspace", missing]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        var lines = result.Output.Split(Environment.NewLine, StringSplitOptions.None);
        var discovery = Array.FindIndex(
            lines,
            line => line.Equals("Discovery:", StringComparison.Ordinal));
        Assert.True(discovery >= 0, "Root help must contain the Discovery section.");
        var findLines = lines[(discovery + 1)..]
            .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal))
            .Where(line =>
            {
                var trimmed = line.Trim();
                return trimmed.Equals("find", StringComparison.Ordinal)
                    || trimmed.StartsWith("find ", StringComparison.Ordinal);
            })
            .ToArray();
        Assert.Single(findLines);
        Assert.Contains("find", findLines[0], StringComparison.Ordinal);
        AssertPathAbsent(missing);
    }

    [Fact(DisplayName = "Composed CliHost exposes the registered direct Find leaf and its binding-owned help"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedHostExposesFindHelpAndRegistration()
    {
        using var workspace = FindWorkspace.CreateBare();
        var missing = workspace.Combine("missing-find-help-workspace");
        AssertPathAbsent(missing);
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--help", "--workspace", missing]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("open-forge find", result.Output, StringComparison.Ordinal);
        Assert.Contains("Source references", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.Output, StringComparison.Ordinal);
        AssertPathAbsent(missing);
    }

    [Fact(DisplayName = "Composed Find bare invocation uses the current directory and default expanded view"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task BareInvocationUsesCurrentDirectoryAndDefaultExpandedView()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains($"Workspace: {workspace.Path}", result.Output, StringComparison.Ordinal);
        Assert.Contains("Selected by: current directory", result.Output, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", result.Output, StringComparison.Ordinal);
        Assert.Contains("Matches: 3", result.Output, StringComparison.Ordinal);
        Assert.Contains("docs", result.Output, StringComparison.Ordinal);
        Assert.Contains("guide", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("result=complete", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Find explicit --workspace uses the default expanded view without duplicate compact coverage"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ExplicitWorkspaceUsesDefaultExpandedView()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains($"Workspace: {workspace.Path}", result.Output, StringComparison.Ordinal);
        Assert.Contains("Selected by: --workspace", result.Output, StringComparison.Ordinal);
        Assert.Contains("Coverage: complete", result.Output, StringComparison.Ordinal);
        Assert.Contains("Matches: 3", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("result=complete", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Find compact tag filtering returns exactly the ordinary matching source"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task CompactTagFilteringReturnsExactlyOneSource()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--view=compact", "--tag=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Equal(
            [
                "result=complete\tcoverage=complete\tuniverse=default\tmatches=1",
                "docs\t.agents/docs.md",
            ],
            NonEmptyLines(result.Output));
        Assert.DoesNotContain("guide\t.agents/guide.md", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Find expanded heading filtering returns exactly one source and excludes the distinct ordinary source"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ExpandedHeadingFilteringReturnsExactlyOneSource()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--view=expanded", "--heading=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        Assert.Contains("Coverage: complete", result.Output, StringComparison.Ordinal);
        Assert.Contains("Matches: 1", result.Output, StringComparison.Ordinal);
        Assert.Contains("docs", result.Output, StringComparison.Ordinal);
        Assert.Contains("Path: .agents/docs.md", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/guide.md", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Composed Find JSON is one complete document and --view is a no-op while content remains typed"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task JsonViewAndContentRemainOneTypedDocument()
    {
        using var workspace = FindWorkspace.CreateBare();
        var compact = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--tag=Architecture", "--json", "--view=compact", "--content=metadata,frontmatter,headings,body,section:Target"]);
        var expanded = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--tag=Architecture", "--json", "--view=expanded", "--content=metadata,frontmatter,headings,body,section:Target"]);

        Assert.Equal(0, compact.ExitCode);
        Assert.Equal(0, expanded.ExitCode);
        Assert.Equal(string.Empty, compact.Error);
        Assert.Equal(string.Empty, expanded.Error);
        using var compactDocument = JsonDocument.Parse(compact.Output);
        using var expandedDocument = JsonDocument.Parse(expanded.Output);
        AssertJsonViewChangesOnlyEchoedSelection(compactDocument, expandedDocument);
        AssertCompleteDocsProjection(compactDocument);
        AssertCompleteDocsProjection(expandedDocument);
    }

    [Fact(DisplayName = "Composed Find typed invalid input uses the Find envelope for malformed content"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task TypedInvalidInputUsesFindEnvelope()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--json", "--content=not-a-part"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal("not-started", document.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString());
        Assert.Equal([], document.RootElement.GetProperty("result").GetProperty("presentation").GetProperty("content").GetProperty("effective").EnumerateArray().Select(value => value.GetString()));
    }

    [Fact(DisplayName = "Composed Find blocked workspace selection emits a typed blocked result with workspace null"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task BlockedWorkspaceUsesNullWorkspaceInEnvelope()
    {
        using var workspace = FindWorkspace.CreateBare();
        var missing = workspace.Combine("missing-workspace");
        AssertPathAbsent(missing);
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", missing, "--json"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        AssertPathAbsent(missing);
    }

    [Theory(DisplayName = "Composed Find exposes deterministic attention, missing-projection, lock, and invalid-encoding findings"),
        InlineData("attention", "attention", 2, "find.identity-collision"),
        InlineData("missing-projection", "attention", 2, "find.projection-missing"),
        InlineData("unreadable", "incomplete", 3, "find.inspection-unavailable"),
        InlineData("invalid-encoding", "incomplete", 3, "find.invalid-encoding"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task RealSourceFindingsUseStableTypedStatuses(
        string scenario,
        string expectedStatus,
        int expectedExitCode,
        string expectedFindingCode)
    {
        using var workspace = scenario switch
        {
            "attention" => FindWorkspace.CreateCollision(),
            "missing-projection" => FindWorkspace.CreateBare(),
            "unreadable" => FindWorkspace.CreateIncomplete(false),
            "invalid-encoding" => FindWorkspace.CreateIncomplete(true),
            _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, "The Find source finding scenario is not defined."),
        };
        var content = scenario switch
        {
            "attention" => "body",
            "missing-projection" => "section:Missing",
            _ => "metadata,body",
        };
        var result = await RunWithoutWrites(
            workspace,
            ["find", "--workspace", workspace.Path, "--json", $"--content={content}"]);

        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Equal(string.Empty, result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        var findings = document.RootElement.GetProperty("result").GetProperty("findings").EnumerateArray().ToArray();
        Assert.Contains(findings, finding => finding.GetProperty("code").GetString() == expectedFindingCode);
        var coverage = document.RootElement.GetProperty("result").GetProperty("coverage");
        Assert.Equal(expectedStatus == "incomplete" ? "incomplete" : "complete", coverage.GetProperty("state").GetString());
        if (scenario is "unreadable" or "invalid-encoding")
        {
            var expectedPath = scenario == "unreadable" ? ".agents/unreadable.md" : ".agents/bad.md";
            Assert.Contains(findings, finding =>
                finding.GetProperty("code").GetString() == expectedFindingCode
                && finding.GetProperty("path").GetString() == expectedPath);
        }
        if (expectedStatus == "attention")
        {
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("next").ValueKind);
        }
        else
        {
            Assert.Equal("open-forge doctor", document.RootElement.GetProperty("next").GetProperty("command").GetString());
        }
    }

    [Theory(DisplayName = "Composed Find verbose mode keeps primary output and diagnostics on separate streams for every terminal status"),
        InlineData("complete", 0),
        InlineData("invalid", 4),
        InlineData("blocked", 5),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task VerboseStreamsRemainSeparated(string expectedStatus, int expectedExitCode)
    {
        using var workspace = FindWorkspace.CreateBare();
        string? missing = null;
        string[] arguments;
        if (expectedStatus == "complete")
        {
            arguments = ["find", "--workspace", workspace.Path];
        }
        else if (expectedStatus == "invalid")
        {
            arguments = ["find", "--workspace", workspace.Path, "--content=bad"];
        }
        else if (expectedStatus == "blocked")
        {
            missing = workspace.Combine("missing");
            AssertPathAbsent(missing);
            arguments = ["find", "--workspace", missing];
        }
        else
        {
            throw new ArgumentOutOfRangeException(nameof(expectedStatus), expectedStatus, "The verbose status is not defined.");
        }

        var plain = await RunWithoutWrites(workspace, arguments);
        if (missing is not null)
        {
            AssertPathAbsent(missing);
        }

        var verbose = await RunWithoutWrites(workspace, [.. arguments, "--verbose"]);
        if (missing is not null)
        {
            AssertPathAbsent(missing);
        }

        Assert.Equal(expectedExitCode, plain.ExitCode);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Contains("status", verbose.Error, StringComparison.OrdinalIgnoreCase);
        Assert.InRange(verbose.Error.Length, 1, 4096);
        if (expectedStatus == "complete")
        {
            Assert.Equal(plain.Output, verbose.Output);
            Assert.Contains("Coverage: complete", verbose.Output, StringComparison.Ordinal);
            Assert.Contains("Matches: 3", verbose.Output, StringComparison.Ordinal);
        }
        else
        {
            Assert.Equal(string.Empty, plain.Output);
            Assert.Equal(string.Empty, verbose.Output);
        }
    }

    [Fact(DisplayName = "Composed Find invalid JSON keeps primary output, status, and exit invariant under verbose diagnostics"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task InvalidJsonVerboseModePreservesPrimaryDocument()
    {
        using var workspace = FindWorkspace.CreateBare();
        string[] arguments = ["find", "--workspace", workspace.Path, "--json", "--content=bad"];
        var plain = await RunWithoutWrites(workspace, arguments);
        var verbose = await RunWithoutWrites(workspace, arguments.Append("--verbose").ToArray());

        Assert.Equal(4, plain.ExitCode);
        Assert.Equal(plain.ExitCode, verbose.ExitCode);
        Assert.Equal(string.Empty, plain.Error);
        Assert.Equal(plain.Output, verbose.Output);
        Assert.InRange(verbose.Error.Length, 1, 4096);

        using var plainDocument = JsonDocument.Parse(plain.Output);
        using var verboseDocument = JsonDocument.Parse(verbose.Output);
        Assert.Equal("invalid", plainDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("status").GetString(),
            verboseDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString(),
            verboseDocument.RootElement.GetProperty("result").GetProperty("coverage").GetProperty("projection").GetString());
    }

    [Theory(DisplayName = "Direct typed Find terminal results preserve exact compact summaries and next actions"),
        InlineData("failed", "result=failed\tcoverage=failed\tuniverse=default\tmatches=1", "Next: report the failure and retry with bounded diagnostics."),
        InlineData("interrupted", "result=interrupted\tcoverage=interrupted\tuniverse=default\tmatches=1", "Next: rerun the same request."),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void DirectTypedTerminalResultsPreserveSummaryAndNext(
        string statusValue,
        string expectedSummary,
        string expectedNext)
    {
        var result = CreateTerminalResult(statusValue);

        Assert.Equal(
            statusValue == "failed" ? CliSemanticStatus.Failed : CliSemanticStatus.Interrupted,
            result.Status);
        Assert.Equal(
            statusValue == "failed" ? FindFindingCode.OperationFailed : FindFindingCode.Interrupted,
            Assert.Single(result.Findings).Code);
        Assert.NotNull(result.Next);
        Assert.Equal(
            statusValue == "failed" ? "open-forge find --verbose" : "open-forge find",
            result.Next!.Command);

        var rendered = FindCompactRenderer.Render(result);

        Assert.Equal(expectedSummary, NonEmptyLines(rendered)[0]);
        Assert.Equal(
            [expectedNext],
            NonEmptyLines(rendered)
                .Where(line => line.StartsWith("Next:", StringComparison.Ordinal)));
    }

    [Fact(DisplayName = "Parser-native global failures remain Shell diagnostics and do not invent a typed Find envelope"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ParserNativeFailuresRemainShellDiagnostics()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["--unknown-global-option"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.Output);
        Assert.InRange(result.Error.Length, 1, 4096);
        Assert.DoesNotContain("schemaVersion", result.Error, StringComparison.Ordinal);
    }

    private static Task<CliHostCaptureResult> Run(FindWorkspace workspace, string[] arguments)
        => CliHostCapture.RunAsync(arguments, workspace.Path);

    private static Task<CliHostCaptureResult> RunWithoutWrites(FindWorkspace workspace, string[] arguments)
        => RunWithoutWritesCore(workspace, arguments);

    private static async Task<CliHostCaptureResult> RunWithoutWritesCore(
        FindWorkspace workspace,
        string[] arguments)
    {
        var before = workspace.SnapshotState();
        var result = await Run(workspace, arguments);
        Assert.Equal(before, workspace.SnapshotState());
        return result;
    }

    private static string[] NonEmptyLines(string output)
        => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

    private static void AssertCompleteDocsProjection(JsonDocument document)
    {
        var root = document.RootElement;
        Assert.Equal("find", root.GetProperty("command").GetString());
        Assert.Equal("complete", root.GetProperty("status").GetString());
        Assert.Equal(3, root.GetProperty("result").GetProperty("universe").GetProperty("candidateCount").GetInt32());
        Assert.Equal(3, root.GetProperty("result").GetProperty("universe").GetProperty("inspectedCount").GetInt32());
        Assert.Equal(1, root.GetProperty("result").GetProperty("universe").GetProperty("matchedCount").GetInt32());

        var presentation = root.GetProperty("result").GetProperty("presentation");
        string[] expectedContent = ["metadata", "frontmatter", "headings", "body", "section:Target"];
        Assert.Equal(expectedContent, presentation.GetProperty("content").GetProperty("supplied").EnumerateArray().Select(value => value.GetString()));
        Assert.Equal(expectedContent, presentation.GetProperty("content").GetProperty("effective").EnumerateArray().Select(value => value.GetString()));

        var match = Assert.Single(root.GetProperty("result").GetProperty("matches").EnumerateArray());
        Assert.Equal("docs", match.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", match.GetProperty("path").GetString());
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            match.GetProperty("projections").EnumerateArray().Select(projection => projection.GetProperty("part").GetString()));
        Assert.Equal(
            ["available", "available", "available", "available", "available", "available", "available", "available", "available"],
            match.GetProperty("projections").EnumerateArray().Select(projection => projection.GetProperty("state").GetString()));

        var projections = match.GetProperty("projections").EnumerateArray().ToArray();
        Assert.Equal(
            ["metadata", "frontmatter", "headings", "body", "section", "frontmatter", "headings", "body", "section"],
            projections.Select(projection => projection.GetProperty("part").GetString()));
        Assert.Equal(
            new string?[] { null, "base", "base", "base", "base", "overwrite", "overwrite", "overwrite", "overwrite" },
            projections.Select(projection => projection.GetProperty("layer").GetString()));
        Assert.Equal(
            new string?[] { null, ".agents/docs.md", ".agents/docs.md", ".agents/docs.md", ".agents/docs.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md", ".agents/docs.overwrite.md" },
            projections.Select(projection => projection.GetProperty("path").GetString()));

        var metadata = projections[0].GetProperty("metadata");
        Assert.Equal("docs", metadata.GetProperty("id").GetString());
        Assert.Equal(".agents/docs.md", metadata.GetProperty("path").GetString());
        Assert.Equal("unrouted", metadata.GetProperty("routeState").GetString());
        Assert.Null(metadata.GetProperty("route").GetString());
        var metadataLayers = metadata.GetProperty("layers").EnumerateArray().ToArray();
        Assert.Equal(["base", "overwrite"], metadataLayers.Select(layer => layer.GetProperty("kind").GetString()));
        Assert.Equal(
            [".agents/docs.md", ".agents/docs.overwrite.md"],
            metadataLayers.Select(layer => layer.GetProperty("path").GetString()));

        AssertHeadingProjection(
            projections[2],
            ["Architecture", "Target"],
            [6, 8]);
        AssertHeadingProjection(
            projections[6],
            ["Architecture", "Target"],
            [5, 7]);

        AssertTextProjection(
            projections[1],
            ".agents/docs.md",
            "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---",
            1);
        AssertTextProjection(
            projections[3],
            ".agents/docs.md",
            "# Architecture\n\n## Target\n\nTarget body\n",
            6);
        AssertTextProjection(
            projections[4],
            ".agents/docs.md",
            "## Target\n\nTarget body\n",
            8);
        AssertTextProjection(
            projections[5],
            ".agents/docs.overwrite.md",
            "---\nopen-forge:\n  tags: [Architecture]\n---",
            1);
        AssertTextProjection(
            projections[7],
            ".agents/docs.overwrite.md",
            "# Architecture\n\n## Target\n\nOverwrite body\n",
            5);
        AssertTextProjection(
            projections[8],
            ".agents/docs.overwrite.md",
            "## Target\n\nOverwrite body\n",
            7);
    }

    private static void AssertJsonViewChangesOnlyEchoedSelection(
        JsonDocument compactDocument,
        JsonDocument expandedDocument)
    {
        var compactRoot = compactDocument.RootElement;
        var expandedRoot = expandedDocument.RootElement;
        var compactView = compactRoot.GetProperty("result").GetProperty("presentation").GetProperty("view");
        var expandedView = expandedRoot.GetProperty("result").GetProperty("presentation").GetProperty("view");
        Assert.Equal("compact", compactView.GetProperty("supplied").GetString());
        Assert.Equal("compact", compactView.GetProperty("effective").GetString());
        Assert.Equal("expanded", expandedView.GetProperty("supplied").GetString());
        Assert.Equal("expanded", expandedView.GetProperty("effective").GetString());
        Assert.Equal(compactRoot.GetProperty("schemaVersion").GetRawText(), expandedRoot.GetProperty("schemaVersion").GetRawText());
        Assert.Equal(compactRoot.GetProperty("command").GetRawText(), expandedRoot.GetProperty("command").GetRawText());
        Assert.Equal(compactRoot.GetProperty("status").GetRawText(), expandedRoot.GetProperty("status").GetRawText());
        Assert.Equal(compactRoot.GetProperty("workspace").GetRawText(), expandedRoot.GetProperty("workspace").GetRawText());
        Assert.Equal(compactRoot.GetProperty("next").GetRawText(), expandedRoot.GetProperty("next").GetRawText());

        var compactResult = compactRoot.GetProperty("result");
        var expandedResult = expandedRoot.GetProperty("result");
        foreach (var property in new[] { "universe", "query", "coverage", "findings", "matches" })
        {
            Assert.Equal(compactResult.GetProperty(property).GetRawText(), expandedResult.GetProperty(property).GetRawText());
        }

        Assert.Equal(
            compactResult.GetProperty("presentation").GetProperty("content").GetRawText(),
            expandedResult.GetProperty("presentation").GetProperty("content").GetRawText());
    }

    private static void AssertHeadingProjection(
        JsonElement projection,
        string[] expectedTexts,
        int[] expectedLines)
    {
        Assert.Equal("headings", projection.GetProperty("part").GetString());
        var headings = projection.GetProperty("headings").EnumerateArray().ToArray();
        Assert.Equal(expectedTexts, headings.Select(heading => heading.GetProperty("text").GetString()));
        Assert.Equal(expectedLines, headings.Select(heading => heading.GetProperty("location").GetProperty("line").GetInt32()));
        Assert.All(
            headings,
            heading => Assert.NotEqual(JsonValueKind.Null, heading.GetProperty("location").ValueKind));
    }

    private static void AssertTextProjection(
        JsonElement projection,
        string expectedPath,
        string expectedText,
        int expectedLine)
    {
        Assert.Equal("available", projection.GetProperty("state").GetString());
        Assert.Equal(expectedPath, projection.GetProperty("path").GetString());
        Assert.Equal(expectedText, projection.GetProperty("text").GetString());
        var location = projection.GetProperty("location");
        Assert.NotEqual(JsonValueKind.Null, location.ValueKind);
        Assert.Equal(expectedLine, location.GetProperty("line").GetInt32());
        Assert.Equal(1, location.GetProperty("column").GetInt32());
    }

    private static FindResult CreateTerminalResult(string statusValue)
    {
        var root = Path.GetFullPath(Path.Combine(Path.GetTempPath(), "open-forge-find-terminal-result"));
        var workspace = new CliWorkspace(root, root, CliWorkspaceSelectionMethod.ExplicitWorkspace);
        var query = new FindQuery(
            [],
            [],
            FindRequirement.All,
            new FindRegionSelection(
                [],
                [new FindRegion(FindRegionKind.Frontmatter, null, FindDefinitions.Frontmatter)],
                [new FindRegion(FindRegionKind.Body, null, FindDefinitions.Body)]));
        var presentation = new FindPresentationSelection(
            null,
            CliView.Expanded,
            new FindContentSelection([], []));
        var request = new FindRequestEcho(
            workspace,
            new FindUniverseFilter([], []),
            query,
            presentation);
        var terminal = statusValue switch
        {
            "failed" => new FindTerminalEvent(
                FindTerminalEventKind.Failed,
                "The Find operation failed at its bounded operating boundary."),
            "interrupted" => new FindTerminalEvent(
                FindTerminalEventKind.Interrupted,
                "The Find operation was interrupted at its bounded operating boundary."),
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The terminal status is not defined."),
        };

        return new FindResultBuilder().Build(new FindResultInput(
            request,
            null,
            [],
            [new FindMatch(1, "docs", ".agents/docs.md", null, [], [])],
            [],
            [],
            new FindStageCompletion(
                FindCoverageState.Complete,
                FindProjectionCoverageState.NotRequested),
            terminal));
    }

    private static void AssertPathAbsent(string path)
    {
        Assert.False(File.Exists(path));
        Assert.False(Directory.Exists(path));
    }

    private sealed class FindWorkspace : IDisposable
    {
        private readonly TemporaryWorkspace _workspace;
        private readonly FileStream? _lockedFile;
        private readonly IReadOnlyDictionary<string, string>? _lockedSnapshot;
        private bool _lockedSnapshotRead;

        private FindWorkspace(
            TemporaryWorkspace workspace,
            FileStream? lockedFile = null,
            IReadOnlyDictionary<string, string>? lockedSnapshot = null)
        {
            _workspace = workspace;
            _lockedFile = lockedFile;
            _lockedSnapshot = lockedSnapshot;
        }

        internal string Path => _workspace.Path;

        internal string Combine(string relativePath) => _workspace.Combine(relativePath);

        internal IReadOnlyDictionary<string, string> SnapshotState()
        {
            if (_lockedSnapshot is null)
            {
                return SnapshotState(_workspace);
            }

            if (!_lockedSnapshotRead)
            {
                _lockedSnapshotRead = true;
                return _lockedSnapshot;
            }

            _lockedFile?.Dispose();
            return SnapshotState(_workspace);
        }

        internal static FindWorkspace CreateBare()
        {
            var workspace = TemporaryWorkspace.Create("integration-find-bare");
            try
            {
                WriteBaseDocuments(workspace);
                return new FindWorkspace(workspace);
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }

        internal static FindWorkspace CreateCollision()
        {
            var workspace = TemporaryWorkspace.Create("integration-find-collision");
            try
            {
                WriteBaseDocuments(workspace);
                workspace.WriteText(
                    ".agents/docs/_docs.md",
                    "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\nCollision entrypoint\n");
                return new FindWorkspace(workspace);
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }

        internal static FindWorkspace CreateIncomplete(bool invalidEncoding)
        {
            var workspace = TemporaryWorkspace.Create(invalidEncoding
                ? "integration-find-invalid-encoding"
                : "integration-find-unreadable");
            FileStream? lockedFile = null;
            try
            {
                WriteBaseDocuments(workspace);
                if (invalidEncoding)
                {
                    workspace.WriteBytes(".agents/bad.md", [0xFF, 0xFE, 0x00, 0x01]);
                }
                else
                {
                    workspace.WriteText(".agents/unreadable.md", "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n");
                    var snapshot = SnapshotState(workspace);
                    lockedFile = new FileStream(
                        workspace.Combine(".agents/unreadable.md"),
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                    return new FindWorkspace(workspace, lockedFile, snapshot);
                }

                return new FindWorkspace(workspace);
            }
            catch
            {
                lockedFile?.Dispose();
                workspace.Dispose();
                throw;
            }
        }

        public void Dispose()
        {
            _lockedFile?.Dispose();
            _workspace.Dispose();
        }

        private static void WriteBaseDocuments(TemporaryWorkspace workspace)
        {
            workspace.WriteText(
                ".agents/loader.md",
                "---\nopen-forge:\n  description: Loader\n  tags: [LoadNow]\n---\n# Loader\n\n"
                + "## Entries\n\n"
                + "<!-- open-forge:generated-index:start -->\n"
                + "- none - No entries - #Empty\n"
                + "<!-- open-forge:generated-index:end -->\n");
            workspace.WriteText(
                ".agents/docs.md",
                "---\nopen-forge:\n  description: Docs\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nTarget body\n");
            workspace.WriteText(
                ".agents/docs.overwrite.md",
                "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n\n## Target\n\nOverwrite body\n");
            workspace.WriteText(
                ".agents/guide.md",
                "---\nopen-forge:\n  description: Guide\n  tags: [Reference]\n---\n# Guide\n\n## Details\n\nGuide body\n");
        }

        private static IReadOnlyDictionary<string, string> SnapshotState(TemporaryWorkspace workspace)
        {
            var state = new SortedDictionary<string, string>(StringComparer.Ordinal);
            var root = new DirectoryInfo(workspace.Path);
            SnapshotEntry(root, workspace.Path, ".", state);
            return new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(state);
        }

        private static void SnapshotEntry(
            FileSystemInfo entry,
            string rootPath,
            string relativePath,
            IDictionary<string, string> state)
        {
            entry.Refresh();
            var attributes = entry.Attributes;
            var isReparsePoint = (attributes & FileAttributes.ReparsePoint) != 0;
            state[relativePath] = DescribeEntry(entry, relativePath, attributes, isReparsePoint);
            if (isReparsePoint || (attributes & FileAttributes.Directory) == 0)
            {
                return;
            }

            foreach (var child in ((DirectoryInfo)entry)
                .EnumerateFileSystemInfos("*", SearchOption.TopDirectoryOnly)
                .OrderBy(child => GetRelativePath(rootPath, child.FullName), StringComparer.Ordinal))
            {
                SnapshotEntry(
                    child,
                    rootPath,
                    GetRelativePath(rootPath, child.FullName),
                    state);
            }
        }

        private static string DescribeEntry(
            FileSystemInfo entry,
            string relativePath,
            FileAttributes attributes,
            bool isReparsePoint)
        {
            var type = isReparsePoint
                ? (attributes & FileAttributes.Directory) != 0 ? "directory-reparse" : "file-reparse"
                : (attributes & FileAttributes.Directory) != 0 ? "directory" : "file";
            var description = $"type={type};attributes={(int)attributes};creationUtcTicks={entry.CreationTimeUtc.Ticks};lastWriteUtcTicks={entry.LastWriteTimeUtc.Ticks}";
            if (isReparsePoint)
            {
                return $"{description};reparseIdentity={relativePath};linkTarget={entry.LinkTarget ?? "<null>"}";
            }

            if (entry is not FileInfo file)
            {
                return description;
            }

            var bytes = File.ReadAllBytes(file.FullName);
            return $"{description};length={file.Length};sha256={Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(bytes))}";
        }

        private static string GetRelativePath(string rootPath, string path)
            => System.IO.Path.GetRelativePath(rootPath, path).Replace('\\', '/');
    }

    private static class CliCoreApplicationAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_parser")]
        internal static extern ref CliParser Parser(CliCoreApplication application);
    }

    private static class CliParserAccess
    {
        [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_tree")]
        internal static extern ref CliCommandTree Tree(CliParser parser);
    }
}
