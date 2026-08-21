using System.Text.Json;
using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Commands.Route.List.Filesystem;
using OpenForge.Cli.Commands.Route.List.Parsing;
using OpenForge.Cli.Commands.Route.List.Rendering;
using OpenForge.Cli.Commands.Route.List.Topology;
using OpenForge.Cli.Definitions;
using OpenForge.Cli.Invocation;
using OpenForge.Cli.Parsing;
using OpenForge.Cli.Pipeline;

namespace OpenForge.Cli.UnitTests.Commands.Route.List;

public sealed class RouteListContractTests
{
    [Fact(DisplayName = "Route list depth defaults to one and accepts zero bounded and all"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DepthGrammarAcceptsDefaultZeroBoundedAndAll()
    {
        Assert.Equal("1", RouteListDepth.Default.ToString());
        Assert.True(RouteListDepth.TryParse("0", out var zero));
        Assert.Equal("0", zero.ToString());
        Assert.True(RouteListDepth.TryParse("12", out var bounded));
        Assert.Equal("12", bounded.ToString());
        Assert.True(RouteListDepth.TryParse("all", out var all));
        Assert.True(all.IsAll);
        Assert.Equal("all", all.ToString());
    }

    [Theory(DisplayName = "Route list depth rejects empty negative non-integer and unknown values"),
     InlineData(""),
     InlineData(" "),
     InlineData("-1"),
     InlineData("+1"),
     InlineData("1.5"),
     InlineData("ALL"),
     InlineData("unknown"),
     Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DepthGrammarRejectsInvalidValues(string value)
    {
        Assert.False(RouteListDepth.TryParse(value, out _));
    }

    [Fact(DisplayName = "Route list parser rejects repeated scalar values"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void ParserRejectsRepetition()
    {
        var plugin = RouteListCommandPlugin.Create();
        var parse = CliRootTree.Create(plugin.Branch).Parse(
        [
            "route", "list", "root", "second", "--workspace", "one", "--workspace", "two",
            "--depth=1", "--depth=2", "--view=compact", "--view=expanded",
        ]);

        Assert.NotEmpty(parse.Result.Errors);
    }

    [Fact(DisplayName = "Route list resolution accepts help and version with JSON as terminal no-ops"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TerminalResolutionAcceptsJsonNoOp()
    {
        var helpPlugin = RouteListCommandPlugin.Create();
        var helpParse = CliRootTree.Create(helpPlugin.Branch).Parse(["route", "list", "--json", "--help"]);
        var help = CliGlobalInputResolver.Resolve(helpParse);
        var versionPlugin = RouteListCommandPlugin.Create();
        var versionParse = CliRootTree.Create(versionPlugin.Branch).Parse(
            ["route", "list", "--json", "--view=compact", "--version"]);
        var version = CliGlobalInputResolver.Resolve(versionParse);

        Assert.Equal(CliTerminalMode.Help, help.TerminalMode);
        Assert.Equal(CliOutputFormat.Json, help.Input?.Presentation.Format);
        Assert.Equal(CliTerminalMode.Version, version.TerminalMode);
        Assert.Equal(CliOutputFormat.Json, version.Input?.Presentation.Format);
    }

    [Fact(DisplayName = "Route list terminal resolution rejects help and version together"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TerminalResolutionRejectsConflictingModes()
    {
        var plugin = RouteListCommandPlugin.Create();
        var parse = CliRootTree.Create(plugin.Branch).Parse(["route", "list", "--help", "--version"]);

        var resolution = CliGlobalInputResolver.Resolve(parse);

        Assert.Null(resolution.Input);
        Assert.NotNull(resolution.InvalidInput);
    }

    [Fact(DisplayName = "Route list terminal composition conflict emits one structured invalid result without help or operation"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public async Task TerminalCompositionConflictUsesStructuredInvalidResult()
    {
        var result = await RunApplicationAsync("route", "list", "--help", "--depth=1", "--json");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("Usage:", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Empty(document.RootElement.GetProperty("result").GetProperty("rows").EnumerateArray());
    }

    [Fact(DisplayName = "Route list terminal composition conflict emits one human invalid result without help or operation"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public async Task TerminalCompositionConflictUsesHumanInvalidResult()
    {
        var result = await RunApplicationAsync("route", "list", "--help", "--depth=1");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.Contains("Open Forge route list", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Result: invalid", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Rows: 0", result.StandardError, StringComparison.Ordinal);
        Assert.DoesNotContain("Usage:", result.StandardError, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list physical directory tracker rejects repeated normalized identity with OS path semantics"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void PhysicalDirectoryTrackerUsesOsPathIdentity()
    {
        var identity = Path.Combine(Path.GetTempPath(), "OpenForgePhysicalIdentity");
        var tracker = new RouteListPhysicalDirectoryTracker();

        Assert.True(tracker.TryVisitResolved(identity + Path.DirectorySeparatorChar));
        Assert.False(tracker.TryVisitResolved(identity));

        var caseTracker = new RouteListPhysicalDirectoryTracker();
        Assert.True(caseTracker.TryVisitResolved(identity.ToLowerInvariant()));
        Assert.Equal(!OperatingSystem.IsWindows(), caseTracker.TryVisitResolved(identity.ToUpperInvariant()));
    }

    [Theory(DisplayName = "Route list filesystem finding policy distinguishes safety from read failure"),
     InlineData(RouteListFindingCodes.PhysicalEscape, (int)RouteListFilesystemFindingKind.Safety),
     InlineData(RouteListFindingCodes.PhysicalCycle, (int)RouteListFilesystemFindingKind.Safety),
     InlineData(RouteListFindingCodes.FilesystemReadFailed, (int)RouteListFilesystemFindingKind.ReadFailure),
     Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void FilesystemFindingPolicyClassifiesFiniteCodes(string code, int expectedKind)
    {
        var finding = new RouteListFinding(code, "Boundary finding.", ".agents/root/boundary");

        Assert.Equal((RouteListFilesystemFindingKind)expectedKind, RouteListFilesystemFindingPolicy.Classify(finding));
    }

    [Theory(DisplayName = "Route list topology maps relevant filesystem safety to blocked and read failure to incomplete"),
     InlineData(RouteListFindingCodes.PhysicalEscape, (int)CliSemanticStatus.Blocked),
     InlineData(RouteListFindingCodes.PhysicalCycle, (int)CliSemanticStatus.Blocked),
     InlineData(RouteListFindingCodes.FilesystemReadFailed, (int)CliSemanticStatus.Incomplete),
     Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TopologyClassifiesRelevantFilesystemFindings(string code, int expectedStatus)
    {
        var result = ResolveWithFilesystemFinding(code, ".agents/root/boundary");

        Assert.Equal((CliSemanticStatus)expectedStatus, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Payload.Coverage.State);
        Assert.Contains(result.Payload.Findings, finding => finding.Code == code);
    }

    [Fact(DisplayName = "Route list topology ignores a physical-cycle finding outside the selected closure"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void TopologyIgnoresUnrelatedPhysicalCycle()
    {
        var result = ResolveWithFilesystemFinding(
            RouteListFindingCodes.PhysicalCycle,
            ".agents/unrelated/cycle");

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteListCoverageState.Complete, result.Payload.Coverage.State);
        Assert.DoesNotContain(result.Payload.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalCycle);
    }

    [Theory(DisplayName = "Route list derives IDs from loader canonical compatibility ordinary skill Unicode spaces and overwrite paths"),
     InlineData(".agents/loader.md", "loader"),
     InlineData(".agents/memory/_memory.md", "memory"),
     InlineData(".agents/memory/index.md", "memory"),
     InlineData(".agents/memory/_index.md", "memory"),
     InlineData(".agents/memory/references.md", "memory"),
     InlineData(".agents/memory/_references.md", "memory"),
     InlineData(".agents/memory/ordinary note.md", "memory/ordinary note"),
     InlineData(".agents/skills/experience-design/SKILL.md", "skills/experience-design"),
     InlineData(".agents/工作 alpha/_工作 alpha.md", "工作 alpha"),
     InlineData(".agents/memory/ordinary note.overwrite.md", "memory/ordinary note"),
     Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void DerivesExpectedSourceIds(string path, string expectedId)
    {
        Assert.Equal(expectedId, RouteListSourceIdentity.DeriveId(path));
    }

    [Fact(DisplayName = "Route list rejects identity paths outside the exact agents boundary"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void RejectsIdentityOutsideAgents()
    {
        Assert.Null(RouteListSourceIdentity.DeriveId("src/file.md"));
        Assert.Null(RouteListSourceIdentity.DeriveId(".agents/../outside.md"));
    }

    [Fact(DisplayName = "Route list ordering is parent-before-child and ordinal by canonical path"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void OrdersRowsDeterministically()
    {
        var rows = new[]
        {
            Row(".agents/z/child.md", "z/child", "z", ".agents/z/_z.md", 1),
            Row(".agents/a/_a.md", "a", null, null, 0),
            Row(".agents/z/_z.md", "z", null, null, 0),
            Row(".agents/a/child.md", "a/child", "a", ".agents/a/_a.md", 1),
        };

        var ordered = RouteListOrdering.ParentBeforeChild(rows);

        Assert.Equal(
            [".agents/a/_a.md", ".agents/a/child.md", ".agents/z/_z.md", ".agents/z/child.md"],
            ordered.Select(row => row.Path));
    }

    [Fact(DisplayName = "Route list uses every shared semantic status exit and human stream"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void UsesSharedStatusExitAndStream()
    {
        Assert.Equal(0, CliStatusDefinitions.Read(CliSemanticStatus.Complete).Disposition.ExitCode);
        Assert.Equal(1, CliStatusDefinitions.Read(CliSemanticStatus.Failed).Disposition.ExitCode);
        Assert.Equal(2, CliStatusDefinitions.Read(CliSemanticStatus.Attention).Disposition.ExitCode);
        Assert.Equal(3, CliStatusDefinitions.Read(CliSemanticStatus.Incomplete).Disposition.ExitCode);
        Assert.Equal(4, CliStatusDefinitions.Read(CliSemanticStatus.Invalid).Disposition.ExitCode);
        Assert.Equal(5, CliStatusDefinitions.Read(CliSemanticStatus.Blocked).Disposition.ExitCode);
        Assert.Equal(130, CliStatusDefinitions.Read(CliSemanticStatus.Interrupted).Disposition.ExitCode);
        Assert.Equal(CliOutputTarget.StandardOutput, CliStatusDefinitions.Read(CliSemanticStatus.Complete).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardOutput, CliStatusDefinitions.Read(CliSemanticStatus.Attention).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardOutput, CliStatusDefinitions.Read(CliSemanticStatus.Incomplete).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardError, CliStatusDefinitions.Read(CliSemanticStatus.Invalid).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardError, CliStatusDefinitions.Read(CliSemanticStatus.Blocked).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardError, CliStatusDefinitions.Read(CliSemanticStatus.Failed).Disposition.HumanOutputTarget);
        Assert.Equal(CliOutputTarget.StandardError, CliStatusDefinitions.Read(CliSemanticStatus.Interrupted).Disposition.HumanOutputTarget);
    }

    [Fact(DisplayName = "Route list ordinary finding precedence is blocked then incomplete then attention then complete"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void AppliesStatusPrecedence()
    {
        Assert.Equal(CliSemanticStatus.Blocked, RouteListStatusPolicy.FromOrdinaryFindings(blocked: true, incomplete: true, attention: true));
        Assert.Equal(CliSemanticStatus.Incomplete, RouteListStatusPolicy.FromOrdinaryFindings(blocked: false, incomplete: true, attention: true));
        Assert.Equal(CliSemanticStatus.Attention, RouteListStatusPolicy.FromOrdinaryFindings(blocked: false, incomplete: false, attention: true));
        Assert.Equal(CliSemanticStatus.Complete, RouteListStatusPolicy.FromOrdinaryFindings(blocked: false, incomplete: false, attention: false));
    }

    [Fact(DisplayName = "Route list JSON uses concrete camel-case fields string status and one workspace location"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void JsonUsesConcreteEnvelope()
    {
        var result = CompleteResult();
        var presentation = Presentation(result, CliView.Expanded, CliOutputFormat.Json);
        using var document = JsonDocument.Parse(RouteListJsonRenderer.Render(presentation));
        var root = document.RootElement;

        Assert.Equal(
            ["schemaVersion", "command", "status", "workspace", "result", "next"],
            root.EnumerateObject().Select(property => property.Name));
        Assert.Equal("attention", root.GetProperty("status").GetString());
        Assert.Equal("C:/workspace", root.GetProperty("workspace").GetProperty("path").GetString());
        Assert.DoesNotContain("workspace", root.GetProperty("result").EnumerateObject().Select(property => property.Name));
        Assert.Equal(1, root.GetProperty("result").GetProperty("requestedDepth").GetInt32());
        Assert.Equal("route-list", root.GetProperty("result").GetProperty("rows")[0].GetProperty("provenance")[0].GetString());
        Assert.Equal(
            [
                "id", "path", "parentId", "parentPath", "absoluteDepth", "relativeDepth", "kind",
                "description", "tags", "directChildCount", "provenance",
            ],
            root.GetProperty("result").GetProperty("rows")[0].EnumerateObject().Select(property => property.Name));
    }

    [Fact(DisplayName = "Route list compact and expanded views retain identical typed row order"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void HumanViewsRetainRowOrder()
    {
        var result = CompleteResult();

        var compact = RouteListHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = RouteListHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.True(compact.IndexOf("root", StringComparison.Ordinal) < compact.IndexOf("root/child", StringComparison.Ordinal));
        Assert.True(expanded.IndexOf("root", StringComparison.Ordinal) < expanded.IndexOf("root/child", StringComparison.Ordinal));
        Assert.Contains("description=\"Root\"", compact, StringComparison.Ordinal);
        Assert.Contains("Description: Root", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list human views escape NUL and all control characters deterministically"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void HumanViewsEscapeControlCharacters()
    {
        var result = CompleteResult();
        result = result with
        {
            Result = result.Result with
            {
                Findings = [new RouteListFinding("control", "NUL=\0 BEL=\a", null)],
            },
        };

        var compact = RouteListHumanRenderer.Render(Presentation(result, CliView.Compact));
        var expanded = RouteListHumanRenderer.Render(Presentation(result, CliView.Expanded));

        Assert.DoesNotContain("\0", compact, StringComparison.Ordinal);
        Assert.DoesNotContain("\0", expanded, StringComparison.Ordinal);
        Assert.Contains("NUL=\\u0000 BEL=\\u0007", compact, StringComparison.Ordinal);
        Assert.Contains("NUL=\\u0000 BEL=\\u0007", expanded, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Route list cancellation before and during resolution retains only confirmed safe rows"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CancellationBeforeAndDuringFormationIsInterrupted()
    {
        var formed = CompleteResult() with { Status = CliSemanticStatus.Failed };
        RouteListRow[] confirmed = [formed.Result.Rows[0]];

        var interrupted = RouteListCancellation.RetainOrInterrupt(formed, confirmed, resultFormationCompleted: false);

        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, interrupted.Result.Coverage.State);
        Assert.Equal(confirmed, interrupted.Result.Rows);
    }

    [Fact(DisplayName = "Route list cancellation after complete result formation preserves complete result"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CancellationAfterFormationPreservesResult()
    {
        var formed = CompleteResult() with { Status = CliSemanticStatus.Complete };

        var retained = RouteListCancellation.RetainOrInterrupt(formed, formed.Result.Rows, resultFormationCompleted: true);

        Assert.Same(formed, retained);
        Assert.Equal(CliSemanticStatus.Complete, retained.Status);
        Assert.Equal(RouteListCoverageState.Complete, retained.Result.Coverage.State);
    }

    [Fact(DisplayName = "Route list cancellation retains confirmed rows in deterministic parent-before-child order"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void CancellationOrdersConfirmedRows()
    {
        var formed = CompleteResult() with { Status = CliSemanticStatus.Failed };
        var confirmed = formed.Result.Rows.Reverse().ToArray();

        var interrupted = RouteListCancellation.RetainOrInterrupt(formed, confirmed, resultFormationCompleted: false);

        Assert.Equal(["root", "root/child"], interrupted.Result.Rows.Select(row => row.Id));
        Assert.Equal(CliSemanticStatus.Interrupted, interrupted.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, interrupted.Result.Coverage.State);
    }

    [Fact(DisplayName = "Route list invalid workspace paths form typed invalid resolution instead of throwing"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public void InvalidWorkspacePathIsTyped()
    {
        var plugin = RouteListCommandPlugin.Create();
        var parse = CliRootTree.Create(plugin.Branch).Parse(
            ["route", "list", "--workspace", "\0invalid", "--depth=0", "--json"]);
        var global = CliGlobalInputResolver.Resolve(parse);

        var resolution = RouteListInputResolver.Resolve(
            parse,
            plugin.Symbols,
            Assert.IsType<CliGlobalInput>(global.Input),
            Environment.CurrentDirectory);

        Assert.Null(resolution.Request);
        Assert.Contains(
            resolution.InvalidInput?.Diagnostics ?? [],
            diagnostic => diagnostic.Contains("workspace", StringComparison.OrdinalIgnoreCase));
    }

    [Fact(DisplayName = "Route list invalid NUL workspace emits JSON with null workspace and no raw control byte"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public async Task InvalidNulWorkspaceJsonIsSafe()
    {
        var result = await RunApplicationAsync(
            "route",
            "list",
            "--workspace",
            "\0invalid-workspace",
            "--json");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardError);
        Assert.DoesNotContain("\0", result.StandardOutput, StringComparison.Ordinal);
        using var document = JsonDocument.Parse(result.StandardOutput);
        Assert.Equal("invalid", document.RootElement.GetProperty("status").GetString());
        Assert.Equal(JsonValueKind.Null, document.RootElement.GetProperty("workspace").ValueKind);
    }

    [Fact(DisplayName = "Route list invalid NUL workspace emits escaped human invalid output without a resolved workspace"), Trait("Feature", "route-list"), Trait("Evidence", "Unit")]
    public async Task InvalidNulWorkspaceHumanIsSafe()
    {
        var result = await RunApplicationAsync(
            "route",
            "list",
            "--workspace",
            "\0invalid-workspace");

        Assert.Equal(4, result.ExitCode);
        Assert.Equal(string.Empty, result.StandardOutput);
        Assert.DoesNotContain("\0", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Workspace: none", result.StandardError, StringComparison.Ordinal);
        Assert.Contains("Result: invalid", result.StandardError, StringComparison.Ordinal);
    }

    private static RouteListResult CompleteResult()
    {
        var workspace = new CliWorkspace("C:/workspace", CliWorkspaceSelection.ExplicitWorkspace);
        var row = Row(".agents/root/_root.md", "root", null, null, 0) with
        {
            Description = "Root",
            Tags = ["Root"],
            DirectChildCount = 1,
            Provenance = ["route-list"],
        };
        var child = Row(".agents/root/child.md", "root/child", "root", ".agents/root/_root.md", 1) with
        {
            Description = "Child",
            Tags = ["Child"],
            DirectChildCount = null,
            Provenance = ["route-list-child"],
        };
        var payload = new RouteListPayload(
            new RouteListSelection(RouteListSelectionKind.LoaderRoots, null, null),
            RouteListDepth.Default,
            RouteListDepth.Bounded(1),
            new RouteListCoverage(RouteListCoverageState.Complete, null),
            [],
            [row, child]);
        return new RouteListResult(
            RouteListDefinitions.SchemaVersion,
            RouteListDefinitions.ResultCommand,
            CliSemanticStatus.Attention,
            workspace,
            payload,
            null);
    }

    private static RouteTopologyResult ResolveWithFilesystemFinding(string code, string findingPath)
    {
        const string workspacePath = "C:/workspace";
        const string rootPath = ".agents/root/_root.md";
        var root = new RouteListFileFact
        {
            Path = rootPath,
            Id = "root",
            Kind = RouteListRowKind.Entrypoint,
            IsCategoryEntrypoint = true,
            IsCompatibilityEntrypoint = false,
            IsSkill = false,
            IsLoader = false,
            Metadata = new RouteListMetadataFacts("Root", ["Root"], true, []),
        };
        var filesByPath = new Dictionary<string, RouteListFileFact>(StringComparer.Ordinal)
        {
            [rootPath] = root,
        };
        var filesById = new Dictionary<string, IReadOnlyList<RouteListFileFact>>(StringComparer.Ordinal)
        {
            [root.Id] = [root],
        };
        var folders = new Dictionary<string, RouteListFolderFact>(StringComparer.Ordinal)
        {
            [".agents/root"] = new RouteListFolderFact
            {
                Path = ".agents/root",
                RecognizedEntrypointPaths = [rootPath],
            },
        };
        var inventory = new RouteListSourceInventory(
            workspacePath,
            filesByPath,
            filesById,
            folders,
            new Dictionary<string, string>(StringComparer.Ordinal),
            null,
            new Dictionary<string, RouteListFileFact>(StringComparer.Ordinal),
            [new RouteListFinding(code, "Filesystem boundary.", findingPath)],
            true);
        var request = new RouteListRequest(
            new CliWorkspace(workspacePath, CliWorkspaceSelection.ExplicitWorkspace),
            RouteListSourceReference.Parse("root"),
            RouteListDepth.All);
        var graph = RouteListSourceGraph.Create(inventory);
        return RouteListTopologyResolver.Resolve(
            new RouteListTopologyInput(request, inventory, graph),
            CancellationToken.None);
    }

    private static async Task<ApplicationResult> RunApplicationAsync(params string[] arguments)
    {
        using var standardOutput = new StringWriter();
        using var standardError = new StringWriter();
        var writers = new CliOutputWriters(standardOutput, standardError);

        var exitCode = await CliApplication.RunAsync(
            arguments,
            writers,
            TestContext.Current.CancellationToken);
        return new ApplicationResult(exitCode, standardOutput.ToString(), standardError.ToString());
    }

    private static CliPresentationMessage<RouteListResult> Presentation(
        RouteListResult result,
        CliView view,
        CliOutputFormat format = CliOutputFormat.Human)
    {
        return new CliPresentationMessage<RouteListResult>(
            result.Status,
            result,
            new CliPresentation(format, view, CliVerbosity.Normal));
    }

    private static RouteListRow Row(
        string path,
        string id,
        string? parentId,
        string? parentPath,
        int relativeDepth)
    {
        return new RouteListRow(
            id,
            path,
            parentId,
            parentPath,
            relativeDepth,
            relativeDepth,
            RouteListRowKind.RoutedLeaf,
            null,
            [],
            null,
            []);
    }

    private sealed record ApplicationResult(int ExitCode, string StandardOutput, string StandardError);
}
