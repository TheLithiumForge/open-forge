using System.Text.Json;
using OpenForge.Cli.Composition;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Commands.Find.Models.Operation;
using OpenForge.Cli.Core.Commands.Find.Models.Presentation;
using OpenForge.Cli.Core.Commands.Find.Models.Query;
using OpenForge.Cli.Core.Commands.Find.Models.Request;
using OpenForge.Cli.Core.Commands.Find.Models.Result;
using OpenForge.Cli.Core.Commands.Find.Shared.Result;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Presentation.Find;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Invocation.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Presentation;
using OpenForge.Cli.Core.Shell.Parsing;
using OpenForge.Cli.Core.Shell.Parsing.Models.Results;
using OpenForge.Cli.Core.Shell.Presentation.Models;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Composition;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

public sealed class FindApplicationIntegrationTests
{
    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed CliCompositionRoot registers Find as a direct root leaf with one exact binding"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void ComposedRootRegistersFindAsDirectRootLeaf()
    {
        var application = CliCompositionRoot.Create(new CliProcessIdentity("open-forge", "test"));
        var tree = CliCoreApplicationAccess.Tree(application);
        var parse = tree.Parse(["find"]);
        var selection = CliBindingSelector.Select(parse);

        Assert.Equal("find", selection.Command.Name);
        Assert.False(tree.IsGroup(selection.Command));
        Assert.Equal(CliBindingSelectionState.Leaf, selection.State);
        Assert.Same(selection.Command, tree.FindBinding(selection.Command)!.Command);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed root help exposes the direct Find leaf exactly once in Commands"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedRootHelpExposesFindOnceInCommands()
    {
        using var workspace = FindWorkspace.CreateBare();
        var missing = workspace.Combine("missing-root-help-workspace");
        var result = await RunWithoutWrites(workspace, ["--help", "--workspace", missing]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        var lines = result.Output.Split(Environment.NewLine, StringSplitOptions.None);
        var discovery = Array.FindIndex(lines, line => line.Equals("Commands:", StringComparison.Ordinal));
        Assert.True(discovery >= 0);
        Assert.Single(
            lines[(discovery + 1)..]
                .TakeWhile(line => line.StartsWith("  ", StringComparison.Ordinal)),
            line => line.Trim().Equals("find", StringComparison.Ordinal)
                || line.Trim().StartsWith("find ", StringComparison.Ordinal));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed host exposes Find help and registration"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ComposedHostExposesFindHelp()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--help", "--workspace", workspace.Path]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Contains("open-forge find", result.Output, StringComparison.Ordinal);
        Assert.Contains("Source references", result.Output, StringComparison.Ordinal);
        Assert.Contains("Results and streams", result.Output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find bare invocation uses the current directory and native minimal rows"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task BareInvocationUsesNativeMinimalRows()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find"]);
        var json = await RunWithoutWrites(workspace, ["find", "--format", "json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(
            [
                "  docs    .agents/docs.md",
                "  guide   .agents/guide.md",
                "  loader  .agents/loader.md",
            ],
            NonEmptyLines(result.Output));
        Assert.DoesNotContain("result=", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("Workspace:", result.Output, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(json.Output);
        Assert.Equal("minimal", document.RootElement.GetProperty("detail").GetString());
        Assert.Equal("current-directory", document.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
        Assert.Equal(3, document.RootElement.GetProperty("data").GetProperty("matches").GetArrayLength());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find explicit workspace retains native minimal rows and workspace selection"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ExplicitWorkspaceUsesNativeMinimalRows()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path]);
        var json = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--format", "json"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(4, NonEmptyLines(result.Output).Length);
        Assert.DoesNotContain("result=", result.Output, StringComparison.Ordinal);
        Assert.Contains("Workspace: ", result.Output, StringComparison.Ordinal);

        using var document = JsonDocument.Parse(json.Output);
        Assert.Equal("explicit-workspace", document.RootElement.GetProperty("workspace").GetProperty("selectedBy").GetString());
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find minimal tag filtering returns exactly the ordinary matching source"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task MinimalTagFilteringReturnsExactlyOneSource()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--detail=minimal", "--tag=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(2, NonEmptyLines(result.Output).Length);
        Assert.Contains("  docs  .agents/docs.md", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("guide", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("loader", result.Output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find standard heading filtering returns one source with its description"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task StandardHeadingFilteringReturnsOneSource()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--detail=standard", "--heading=Architecture"]);

        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal("1 source matches --heading Architecture.", NonEmptyLines(result.Output)[0]);
        Assert.Contains("  docs  .agents/docs.md", result.Output, StringComparison.Ordinal);
        Assert.Contains("Docs", result.Output, StringComparison.Ordinal);
        Assert.DoesNotContain(".agents/guide.md", result.Output, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find JSON keeps selected matches, evidence, parts, and source-set detail"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task JsonUsesNativeDetailShape()
    {
        using var workspace = FindWorkspace.CreateBare();
        var minimal = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--tag=Architecture", "--format", "json", "--detail=minimal", "--content=metadata,frontmatter,headings,body,section:Target"]);
        var standard = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--tag=Architecture", "--format", "json", "--detail=standard", "--content=metadata,frontmatter,headings,body,section:Target"]);
        var full = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--tag=Architecture", "--format", "json", "--detail=full", "--content=metadata,frontmatter,headings,body,section:Target"]);

        Assert.Equal(0, minimal.ExitCode);
        Assert.Equal(0, standard.ExitCode);
        Assert.Equal(0, full.ExitCode);
        Assert.Empty(minimal.Error);
        Assert.Empty(standard.Error);
        Assert.Empty(full.Error);
        using var minimalDocument = JsonDocument.Parse(minimal.Output);
        using var standardDocument = JsonDocument.Parse(standard.Output);
        using var fullDocument = JsonDocument.Parse(full.Output);

        var minimalData = minimalDocument.RootElement.GetProperty("data");
        Assert.False(minimalData.TryGetProperty("query", out _));
        Assert.False(minimalData.TryGetProperty("sourceSet", out _));
        Assert.True(minimalData.GetProperty("matches")[0].TryGetProperty("parts", out _));
        var standardData = standardDocument.RootElement.GetProperty("data");
        Assert.True(standardData.TryGetProperty("query", out _));
        Assert.True(standardData.GetProperty("matches")[0].TryGetProperty("evidence", out _));
        Assert.False(standardData.TryGetProperty("sourceSet", out _));
        var fullData = fullDocument.RootElement.GetProperty("data");
        Assert.True(fullData.TryGetProperty("sourceSet", out _));
        Assert.Equal(3, fullData.GetProperty("sourceSet").GetProperty("candidates").GetInt32());
        Assert.Contains(
            fullData.GetProperty("matches")[0].GetProperty("parts").EnumerateArray(),
            part => part.GetProperty("part").GetString() == "body");
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find invalid input uses the native error envelope"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task InvalidInputUsesNativeEnvelope()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", workspace.Path, "--format", "json", "--content=not-a-part"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("invalid-input", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "find.invalid-input");
        Assert.False(document.RootElement.GetProperty("data").TryGetProperty("presentation", out _));
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find blocked workspace selection emits a native blocked result with workspace null"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task BlockedWorkspaceUsesNullWorkspaceInEnvelope()
    {
        using var workspace = FindWorkspace.CreateBare();
        var missing = workspace.Combine("missing-workspace");
        var result = await RunWithoutWrites(workspace, ["find", "--workspace", missing, "--format", "json"]);

        Assert.Equal(5, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal("blocked", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
        Assert.Contains(
            document.RootElement.GetProperty("findings").EnumerateArray(),
            finding => finding.GetProperty("code").GetString() == "find.workspace-unavailable");
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Composed Find exposes stable native findings and exits"),
        InlineData("attention", "completed-with-warnings", 2, "find.identity-collision"),
        InlineData("missing-projection", "completed-with-warnings", 2, "find.projection-missing"),
        InlineData("unreadable", "incomplete", 3, "find.inspection-unavailable"),
        InlineData("invalid-encoding", "incomplete", 3, "find.invalid-encoding"),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task RealSourceFindingsUseNativeEnvelope(
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
        var result = await Run(workspace, ["find", "--workspace", workspace.Path, "--format", "json", $"--content={content}"]);

        Assert.Equal(expectedExitCode, result.ExitCode);
        Assert.Empty(result.Error);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Equal(expectedStatus, document.RootElement.GetProperty("status").GetString());
        var findings = document.RootElement.GetProperty("findings").EnumerateArray().ToArray();
        Assert.Contains(findings, value => value.GetProperty("code").GetString() == expectedFindingCode);
        var finding = findings.First(value => value.GetProperty("code").GetString() == expectedFindingCode);
        if (scenario is "unreadable" or "invalid-encoding")
        {
            var expectedPath = scenario == "unreadable" ? ".agents/unreadable.md" : ".agents/bad.md";
            Assert.Equal(expectedPath, finding.GetProperty("subject").GetProperty("path").GetString());
        }

        if (expectedStatus == "incomplete")
        {
            Assert.Equal("open-forge doctor", document.RootElement.GetProperty("next").GetProperty("command").GetString());
        }
        else
        {
            Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("next").ValueKind);
        }
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Composed Find keeps primary output and debug diagnostics on their assigned streams"),
        InlineData("complete", 0),
        InlineData("invalid", 4),
        InlineData("blocked", 5),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task VerboseStreamsRemainSeparated(string expectedStatus, int expectedExitCode)
    {
        using var workspace = FindWorkspace.CreateBare();
        string[] arguments;
        if (expectedStatus == "complete")
        {
            arguments = ["find", "--workspace", workspace.Path];
        }
        else if (expectedStatus == "invalid")
        {
            arguments = ["find", "--workspace", workspace.Path, "--content=bad"];
        }
        else
        {
            var missing = workspace.Combine("missing");
            arguments = ["find", "--workspace", missing];
        }

        var full = await RunWithoutWrites(workspace, [.. arguments, "--detail", "full"]);
        var debug = await RunWithoutWrites(workspace, [.. arguments, "--detail", "debug"]);

        Assert.Equal(expectedExitCode, full.ExitCode);
        Assert.Equal(full.ExitCode, debug.ExitCode);
        Assert.InRange(debug.Error.Length, 1, 4096);
        if (expectedStatus == "complete")
        {
            Assert.Equal(full.Output, debug.Output);
            Assert.Contains("Search details:", debug.Output, StringComparison.Ordinal);
            Assert.Contains("status=completed", debug.Error, StringComparison.Ordinal);
        }
        else
        {
            Assert.Empty(full.Output);
            Assert.Empty(debug.Output);
            Assert.Contains("Cannot search:", full.Error, StringComparison.Ordinal);
        }
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Composed Find invalid JSON debug mode preserves the primary native document"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task InvalidJsonVerboseModePreservesPrimaryDocument()
    {
        using var workspace = FindWorkspace.CreateBare();
        string[] arguments = ["find", "--workspace", workspace.Path, "--format", "json", "--content=bad"];
        var plain = await RunWithoutWrites(workspace, arguments);
        var debug = await RunWithoutWrites(workspace, [.. arguments, "--detail", "debug"]);

        Assert.Equal(4, plain.ExitCode);
        Assert.Equal(plain.ExitCode, debug.ExitCode);
        Assert.Empty(plain.Error);
        Assert.InRange(debug.Error.Length, 1, 4096);

        using var plainDocument = JsonDocument.Parse(plain.Output);
        using var debugDocument = JsonDocument.Parse(debug.Output);
        Assert.Equal("invalid-input", plainDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("status").GetString(),
            debugDocument.RootElement.GetProperty("status").GetString());
        Assert.Equal(
            plainDocument.RootElement.GetProperty("data").GetProperty("matches").GetArrayLength(),
            debugDocument.RootElement.GetProperty("data").GetProperty("matches").GetArrayLength());
        Assert.False(plainDocument.RootElement.GetProperty("data").TryGetProperty("presentation", out _));
        Assert.False(debugDocument.RootElement.GetProperty("data").TryGetProperty("presentation", out _));
    }

    [Trait("Boundary", "Host")]
    [Theory(DisplayName = "Direct typed Find terminal results preserve catalogue headlines and streams"),
        InlineData("failed", "Find stopped because of an unexpected error:"),
        InlineData("interrupted", "Find was cancelled."),
        Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public void DirectTypedTerminalResultsPreserveHeadlines(string statusValue, string expectedHeadline)
    {
        var result = CreateTerminalResult(statusValue);

        Assert.Equal(
            statusValue == "failed" ? FindFindingCode.OperationFailed : FindFindingCode.Interrupted,
            Assert.Single(result.Findings).Code);
        Assert.NotNull(result.Next);
        Assert.Equal(
            statusValue == "failed" ? "open-forge find --detail debug" : "open-forge find",
            result.Next!.Command);

        var rendered = CliRenderingStage.Render(
            new CliPresentationRequest<FindResult>(result, new CliPresentation(CliFormat.Text, CliDetail.Standard, null)),
            FindPresentation.Rendering);

        Assert.Equal(statusValue == "failed" ? CliSemanticStatus.Failed : CliSemanticStatus.Interrupted, rendered.Status);
        Assert.Equal(CliOutputTarget.StandardError, rendered.PrimaryTarget);
        Assert.StartsWith(expectedHeadline, rendered.PrimaryContent, StringComparison.Ordinal);
        Assert.DoesNotContain("Next:", rendered.PrimaryContent, StringComparison.Ordinal);
    }

    [Trait("Boundary", "Host")]
    [Fact(DisplayName = "Parser-native global failures remain Shell diagnostics without a Find envelope"), Trait("Feature", "find-presentation"), Trait("Evidence", "Integration")]
    public async Task ParserNativeFailuresRemainShellDiagnostics()
    {
        using var workspace = FindWorkspace.CreateBare();
        var result = await RunWithoutWrites(workspace, ["--unknown-global-option"]);

        Assert.Equal(4, result.ExitCode);
        Assert.Empty(result.Output);
        Assert.InRange(result.Error.Length, 1, 4096);
        Assert.DoesNotContain("schemaVersion", result.Error, StringComparison.Ordinal);
    }

    private static Task<CliHostCaptureResult> Run(FindWorkspace workspace, string[] arguments)
        => CliHostCapture.RunAsync(arguments, workspace.Path);

    private static async Task<CliHostCaptureResult> RunWithoutWrites(FindWorkspace workspace, string[] arguments)
    {
        var before = workspace.SnapshotState();
        var result = await Run(workspace, arguments);
        Assert.Equal(before, workspace.SnapshotState());
        return result;
    }

    private static string[] NonEmptyLines(string output)
        => output.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries);

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
        var presentation = new FindPresentationSelection(null, CliDetail.Standard, new FindContentSelection([], []));
        var request = new FindRequestEcho(workspace, new FindUniverseFilter([], []), query, presentation);
        var terminal = statusValue switch
        {
            "failed" => new FindTerminalEvent(FindTerminalEventKind.Failed, "The Find operation failed at its bounded operating boundary."),
            "interrupted" => new FindTerminalEvent(FindTerminalEventKind.Interrupted, "The Find operation was interrupted at its bounded operating boundary."),
            _ => throw new ArgumentOutOfRangeException(nameof(statusValue), statusValue, "The terminal status is not defined."),
        };
        return new FindResultBuilder().Build(new FindResultInput(
            request,
            null,
            [],
            [new FindMatch(1, "docs", ".agents/docs.md", null, [], [])],
            [],
            [],
            new FindStageCompletion(FindCoverageState.Complete, FindProjectionCoverageState.NotRequested),
            terminal));
    }

    private sealed class FindWorkspace : IDisposable
    {
        private readonly TemporaryWorkspace _workspace;
        private readonly FileStream? _lockedFile;

        private FindWorkspace(TemporaryWorkspace workspace, FileStream? lockedFile = null)
        {
            _workspace = workspace;
            _lockedFile = lockedFile;
        }

        internal string Path => _workspace.Path;

        internal string Combine(string relativePath) => _workspace.Combine(relativePath);

        internal IReadOnlyDictionary<string, string> SnapshotState() => _workspace.SnapshotHashes();

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
            var workspace = CreateBare();
            try
            {
                workspace._workspace.WriteText(
                    ".agents/docs/_docs.md",
                    "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\nCollision entrypoint\n");
                return workspace;
            }
            catch
            {
                workspace.Dispose();
                throw;
            }
        }

        internal static FindWorkspace CreateIncomplete(bool invalidEncoding)
        {
            var workspace = TemporaryWorkspace.Create(invalidEncoding ? "integration-find-invalid-encoding" : "integration-find-unreadable");
            FileStream? lockedFile = null;
            try
            {
                WriteBaseDocuments(workspace);
                if (invalidEncoding)
                {
                    workspace.WriteBytes(".agents/bad.md", [0xff, 0xfe, 0x00, 0x01]);
                }
                else
                {
                    workspace.WriteText(".agents/unreadable.md", "---\nopen-forge:\n  tags: [Architecture]\n---\n# Architecture\n");
                    lockedFile = new FileStream(
                        workspace.Combine(".agents/unreadable.md"),
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.None);
                }

                return new FindWorkspace(workspace, lockedFile);
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
                "---\nopen-forge:\n  description: Loader\n  tags: [LoadNow]\n---\n# Loader\n\n## Entries\n\n- none - No entries - #Empty\n");
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
    }
}
