using OpenForge.Cli.Commands.Route.List;
using OpenForge.Cli.Definitions;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.List;

public sealed class RouteListIntegrationTests
{
    [Fact(DisplayName = "Route list resolves the exact current directory and explicit workspace"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ResolvesCurrentDirectoryAndExplicitWorkspace()
    {
        using var workspace = CreateRootedWorkspace();

        var currentDirectoryResult = await RunAsync(workspace.Request(selection: CliWorkspaceSelection.CurrentDirectory));
        var explicitWorkspaceResult = await RunAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, currentDirectoryResult.Status);
        Assert.Equal(CliWorkspaceSelection.CurrentDirectory, currentDirectoryResult.Workspace?.SelectedBy);
        Assert.Equal(CliSemanticStatus.Complete, explicitWorkspaceResult.Status);
        Assert.Equal(CliWorkspaceSelection.ExplicitWorkspace, explicitWorkspaceResult.Workspace?.SelectedBy);
    }

    [Fact(DisplayName = "Route list selects Loader roots including a workspace-defined root without emitting Loader"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task SelectsLoaderRootsMechanically()
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Result.Rows, row => row.Id == "root");
        Assert.Contains(result.Result.Rows, row => row.Id == "workspace-defined");
        Assert.DoesNotContain(result.Result.Rows, row => row.Id == "loader");
    }

    [Theory(DisplayName = "Route list honors default zero bounded and all structural depth"),
     InlineData("default", 1),
     InlineData("0", 0),
     InlineData("2", 2),
     InlineData("all", -1),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task HonorsStructuralDepth(string depthText, int expectedDepth)
    {
        using var workspace = CreateRootedWorkspace();
        var depth = ReadDepth(depthText);

        var result = await RunAsync(workspace.Request(depth: depth));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        if (expectedDepth >= 0)
        {
            Assert.Equal((ulong?)expectedDepth, result.Result.EffectiveDepth.BoundedValue);
        }
        else
        {
            Assert.True(result.Result.EffectiveDepth.IsAll);
        }
    }

    [Fact(DisplayName = "Route list includes a valid direct sibling even when generated Entries omits it"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task IncludesDirectSiblingOutsideEntries()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/omitted.md", "Omitted from generated navigation", "Leaf");

        var result = await RunAsync(workspace.Request(depth: RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Result.Rows, row => row.Id == "root/omitted");
    }

    [Fact(DisplayName = "Route list follows nested entrypoints as authored structural descendants"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task FollowsNestedEntrypoints()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/child/_child.md", "Nested child", "Nested");
        workspace.WriteRoute(".agents/root/child/grandchild/_grandchild.md", "Nested grandchild", "Nested");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Result.Rows, row => row.Id == "root/child");
        Assert.Contains(result.Result.Rows, row => row.Id == "root/child/grandchild");
        Assert.True(IndexOf(result.Result.Rows, "root/child") < IndexOf(result.Result.Rows, "root/child/grandchild"));
    }

    [Fact(DisplayName = "Route list excludes a direct child folder that has no recognized entrypoint"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExcludesUnrepresentedChildFolder()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/not-routed/leaf.md", "Not routed", "Leaf");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Result.Rows, row => row.Path.Contains("not-routed", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route list explicitly enumerates a detached entrypoint without fabricating Loader ancestry"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EnumeratesDetachedEntrypoint()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/detached/_detached.md", "Detached", "Detached");
        workspace.WriteRoute(".agents/detached/leaf.md", "Detached leaf", "Leaf");

        var result = await RunAsync(workspace.Request("detached", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(RouteListSelectionKind.ExplicitSource, result.Result.Selection.Kind);
        Assert.Contains(result.Result.Rows, row => row.Id == "detached");
        Assert.Null(result.Result.Rows.Single(row => row.Id == "detached").AbsoluteDepth);
    }

    [Fact(DisplayName = "Route list accepts an explicit routed leaf and does not invent descendants"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task EnumeratesExplicitLeaf()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/leaf.md", "A leaf", "Leaf");

        var result = await RunAsync(workspace.Request("root/leaf", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(result.Result.Rows);
        Assert.Equal("root/leaf", result.Result.Rows[0].Id);
        Assert.Equal(0, result.Result.Rows[0].RelativeDepth);
    }

    [Fact(DisplayName = "Route list resolves a base and overwrite reference to one logical row"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ResolvesBaseAndOverwrite()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/base.md", "Base", "Base");
        workspace.Write(".agents/root/base.overwrite.md", "Workspace adjustment\n");

        var baseResult = await RunAsync(workspace.Request("root/base"));
        var overwriteResult = await RunAsync(workspace.Request(".agents/root/base.overwrite.md"));

        Assert.Equal(CliSemanticStatus.Complete, baseResult.Status);
        Assert.Equal(CliSemanticStatus.Complete, overwriteResult.Status);
        Assert.Single(baseResult.Result.Rows);
        Assert.Single(overwriteResult.Result.Rows);
        Assert.Equal(baseResult.Result.Rows[0].Id, overwriteResult.Result.Rows[0].Id);
        Assert.DoesNotContain(overwriteResult.Result.Rows, row => row.Path.EndsWith(".overwrite.md", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route list does not promote an orphan overwrite into a route row"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ExcludesOrphanOverwrite()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write(".agents/root/orphan.overwrite.md", "Orphan adjustment\n");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Result.Rows, row => row.Path.EndsWith("orphan.overwrite.md", StringComparison.Ordinal));
    }

    [Fact(DisplayName = "Route list reports accepted compatibility entrypoint form as attention without incomplete coverage"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ReportsCompatibilityEntrypointAttention()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/compat/index.md", "Compatibility child", "Compat");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Attention, result.Status);
        Assert.Equal(RouteListCoverageState.Complete, result.Result.Coverage.State);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.CompatibilityEntrypoint);
    }

    [Fact(DisplayName = "Route list includes native SKILL metadata as a routed leaf with empty authored tags"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task IncludesNativeSkill()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteSkill(".agents/root/native-skill/SKILL.md", "native-skill", "Native skill description");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        var skill = Assert.Single(result.Result.Rows, row => row.Id == "root/native-skill");
        Assert.Equal(RouteListRowKind.RoutedLeaf, skill.Kind);
        Assert.Equal("native-skill", skill.Id.Split('/').Last());
        Assert.NotNull(skill.Tags);
        Assert.Empty(skill.Tags);
    }

    [Fact(DisplayName = "Route list preserves spaces and Unicode in source IDs paths and authored metadata"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PreservesSpacesAndUnicode()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/工作 alpha/_工作 alpha.md", "工作 alpha route", "Unicode,Space");
        workspace.WriteRoute(".agents/工作 alpha/leaf note.md", "Leaf note", "Leaf");

        var result = await RunAsync(workspace.Request("工作 alpha", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(result.Result.Rows, row => row.Id == "工作 alpha");
        Assert.Contains(result.Result.Rows, row => row.Path == ".agents/工作 alpha/leaf note.md");
    }

    [Fact(DisplayName = "Route list marks missing or malformed metadata incomplete with nullable safe facts"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ReportsIncompleteMetadata()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write(".agents/root/malformed.md", "---\nopen-forge: [\n---\n\n# Malformed\n");
        workspace.Write(".agents/root/missing.md", "# Missing metadata\n");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        var row = Assert.Single(result.Result.Rows, candidate => candidate.Id == "root/malformed");
        Assert.Null(row.Description);
        Assert.Null(row.Tags);
        Assert.NotEmpty(result.Result.Findings);
    }

    [Fact(DisplayName = "Route list marks a real strict-UTF8 source read failure incomplete rather than blocked"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task SourceReadFailureIsIncomplete()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteBytes(".agents/root/unreadable.md", [0xFF, 0xFE]);

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.SourceReadFailed);
        Assert.DoesNotContain(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalEscape);
        Assert.DoesNotContain(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalCycle);
    }

    [Fact(DisplayName = "Route list ignores stale and reordered generated Entries for membership and order"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task IgnoresGeneratedEntryProjection()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(
            ".agents/root/_root.md",
            "Root route",
            "Root",
            """
            ## Entries

            <!-- open-forge:generated-index:start -->
            - [Fake](fake.md) - #Fake
            - [Zeta](zeta.md) - #Zeta
            - [Alpha](alpha.md) - #Alpha
            <!-- open-forge:generated-index:end -->
            """);
        workspace.WriteRoute(".agents/root/zeta.md", "Zeta authored sibling", "Zeta");
        workspace.WriteRoute(".agents/root/alpha.md", "Alpha authored sibling", "Alpha");

        var result = await RunAsync(workspace.Request(depth: RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.DoesNotContain(result.Result.Rows, row => row.Id == "root/fake");
        Assert.Contains(result.Result.Rows, row => row.Id == "root/alpha");
        Assert.Contains(result.Result.Rows, row => row.Id == "root/zeta");
        Assert.True(IndexOf(result.Result.Rows, "root/alpha") < IndexOf(result.Result.Rows, "root/zeta"));
        Assert.True(IsParentBeforeChild(result.Result.Rows));
    }

    [Theory(DisplayName = "Route list reports missing or malformed Loader roots as incomplete"),
     InlineData(false),
     InlineData(true),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ReportsLoaderRootBoundaryFailure(bool malformed)
    {
        using var workspace = RouteListTestWorkspace.Create();
        var contents = "# Loader\n\n## No route declarations here.\n";
        if (malformed)
        {
            contents = "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [broken](\n<!-- open-forge:generated-index:end -->\n";
        }

        workspace.Write(RouteListDefinitions.LoaderPath, contents);

        var result = await RunAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        var expectedCode = malformed
            ? RouteListFindingCodes.LoaderEntryMalformed
            : RouteListFindingCodes.LoaderMarkers;
        var unexpectedCode = malformed
            ? RouteListFindingCodes.LoaderMarkers
            : RouteListFindingCodes.LoaderEntryMalformed;
        Assert.Contains(result.Result.Findings, finding => finding.Code == expectedCode);
        Assert.DoesNotContain(result.Result.Findings, finding => finding.Code == unexpectedCode);
    }

    [Fact(DisplayName = "Route list treats an empty Loader marker body as incomplete rather than an empty root set"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ReportsEmptyLoaderBodyIncomplete()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write(
            RouteListDefinitions.LoaderPath,
            "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n\n<!-- open-forge:generated-index:end -->\n");

        var result = await RunAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.LoaderEntriesEmpty);
    }

    [Fact(DisplayName = "Route list rejects a Loader link without a useful bare tag"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ReportsLoaderEntryWithoutBareTagIncomplete()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write(
            RouteListDefinitions.LoaderPath,
            "# Loader\n\n## Entries\n\n<!-- open-forge:generated-index:start -->\n- [Root](root/_root.md) - generated description\n<!-- open-forge:generated-index:end -->\n");

        var result = await RunAsync(workspace.Request());

        Assert.Equal(CliSemanticStatus.Incomplete, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.LoaderEntryMalformed);
    }

    [Theory(DisplayName = "Route list rejects unknown unrouted and Loader source references as invalid"),
     InlineData("unknown"),
     InlineData("root/unrepresented/unrouted"),
     InlineData("loader"),
     Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task RejectsInvalidSubjects(string sourceReference)
    {
        using var workspace = CreateRootedWorkspace();
        workspace.Write(".agents/root/unrepresented/unrouted.md", "# Unrouted\n");

        var result = await RunAsync(workspace.Request(sourceReference));

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.NotEmpty(result.Result.Findings);
    }

    [Fact(DisplayName = "Route list blocks ID collisions and duplicate recognized entrypoints"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task BlocksAmbiguousRouteTopology()
    {
        using var workspace = CreateRootedWorkspace();
        workspace.WriteRoute(".agents/root/collision.md", "Collision leaf", "Collision");
        workspace.WriteRoute(".agents/root/collision/_collision.md", "Collision entrypoint", "Collision");
        workspace.WriteRoute(".agents/root/ambiguous/_ambiguous.md", "Canonical", "Ambiguous");
        workspace.WriteRoute(".agents/root/ambiguous/index.md", "Compatibility", "Ambiguous");

        var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.IdCollision);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.DuplicateEntrypoint);
    }

    [Fact(DisplayName = "Route list blocks lexical traversal in exact source paths"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task BlocksLexicalTraversal()
    {
        using var workspace = CreateRootedWorkspace();

        var result = await RunAsync(workspace.Request("./.agents/../outside.md"));

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalEscape);
    }

    [Fact(DisplayName = "Route list blocks a real physical link escape when the platform can create one"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task BlocksPhysicalLinkEscape()
    {
        using var workspace = CreateRootedWorkspace();
        var outside = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"open-forge-route-outside-{Guid.NewGuid():N}");
        Directory.CreateDirectory(outside);
        var link = System.IO.Path.Combine(workspace.Path, RouteListDefinitions.AgentsDirectoryName, "escape");
        try
        {
            if (!TryCreateDirectoryLink(link, outside))
            {
                return;
            }

            var result = await RunAsync(workspace.Request("escape"));

            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalEscape);
        }
        finally
        {
            if (Directory.Exists(outside))
            {
                Directory.Delete(outside, recursive: true);
            }
        }
    }

    [Fact(DisplayName = "Route list stops a physical ancestor symlink cycle with one relevant blocked finding"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task BlocksPhysicalDirectoryCycle()
    {
        using var workspace = CreateRootedWorkspace();
        var target = System.IO.Path.Combine(workspace.Path, RouteListDefinitions.AgentsDirectoryName, "root");
        var link = System.IO.Path.Combine(target, "cycle");
        try
        {
            if (!TryCreateDirectoryLink(link, target))
            {
                return;
            }

            var result = await RunAsync(workspace.Request("root", RouteListDepth.All));

            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Single(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.PhysicalCycle);
        }
        finally
        {
            if (Directory.Exists(link))
            {
                Directory.Delete(link);
            }
        }
    }

    [Fact(DisplayName = "Route list ignores a physical escape under an unrelated unexposed Loader folder"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task IgnoresUnrelatedPhysicalLinkEscape()
    {
        using var workspace = CreateRootedWorkspace();
        var outside = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"open-forge-route-unrelated-outside-{Guid.NewGuid():N}");
        var link = System.IO.Path.Combine(workspace.Path, RouteListDefinitions.AgentsDirectoryName, "unrelated");
        Directory.CreateDirectory(outside);
        try
        {
            if (!TryCreateDirectoryLink(link, outside))
            {
                return;
            }

            var result = await RunAsync(workspace.Request());

            Assert.Equal(CliSemanticStatus.Complete, result.Status);
            Assert.Equal(RouteListCoverageState.Complete, result.Result.Coverage.State);
            Assert.Contains(result.Result.Rows, row => row.Id == "root");
        }
        finally
        {
            if (Directory.Exists(outside))
            {
                Directory.Delete(outside, recursive: true);
            }
        }
    }

    [Fact(DisplayName = "Route list repeats deterministically with identical typed rows and findings"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task RepeatsDeterministically()
    {
        using var workspace = CreateRootedWorkspace();

        var first = await RunAsync(workspace.Request(depth: RouteListDepth.All));
        var second = await RunAsync(workspace.Request(depth: RouteListDepth.All));

        Assert.Equal(first.Status, second.Status);
        Assert.Equal(first.Workspace, second.Workspace);
        Assert.Equal(first.Result.Selection, second.Result.Selection);
        Assert.Equal(first.Result.RequestedDepth, second.Result.RequestedDepth);
        Assert.Equal(first.Result.EffectiveDepth, second.Result.EffectiveDepth);
        Assert.Equal(first.Result.Coverage, second.Result.Coverage);
        Assert.Equal(
            first.Result.Findings.Select(finding => $"{finding.Code}|{finding.Path}|{finding.Message}"),
            second.Result.Findings.Select(finding => $"{finding.Code}|{finding.Path}|{finding.Message}"));
        Assert.Equal(first.Result.Rows.Select(RowFacts), second.Result.Rows.Select(RowFacts));
        Assert.Equal(CliSemanticStatus.Complete, first.Status);
        Assert.True(IsParentBeforeChild(first.Result.Rows));
    }

    [Fact(DisplayName = "Route list leaves workspace bytes unchanged during read-only enumeration"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task DoesNotMutateWorkspace()
    {
        using var workspace = CreateRootedWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await RunAsync(workspace.Request(depth: RouteListDepth.All));

        var after = workspace.SnapshotHashes();
        Assert.Equal(before, after);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
    }

    [Fact(DisplayName = "Route list operation maps a pre-cancelled token to interrupted incomplete no-write result"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task PreCancelledOperationIsInterruptedWithoutWrites()
    {
        using var workspace = CreateRootedWorkspace();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        var before = workspace.SnapshotHashes();

        var result = await RouteListOperation.RunAsync(workspace.Request(), cancellation.Token);

        Assert.Equal(CliSemanticStatus.Interrupted, result.Status);
        Assert.Equal(RouteListCoverageState.Incomplete, result.Result.Coverage.State);
        Assert.Equal(RouteListCoverageBoundaries.CallerCancellation, result.Result.Coverage.Boundary);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Route list operation returns source-path-invalid for a control-containing exact path without writes"), Trait("Feature", "route-list"), Trait("Evidence", "Integration")]
    public async Task ControlContainingExactPathIsInvalidWithoutWrites()
    {
        using var workspace = CreateRootedWorkspace();
        var before = workspace.SnapshotHashes();

        var result = await RouteListOperation.RunAsync(
            workspace.Request(".agents/\0invalid.md"),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == RouteListFindingCodes.SourcePathInvalid);
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    private static RouteListTestWorkspace CreateRootedWorkspace(params string[] loaderEntries)
    {
        var workspace = RouteListTestWorkspace.Create();
        var entries = loaderEntries;
        if (entries.Length == 0)
        {
            entries =
            [
                "- [Root](root/_root.md) - #Root",
                "- [Workspace](workspace-defined/_workspace-defined.md) - #Workspace",
            ];
        }

        workspace.WriteLoader(entries);
        workspace.WriteRoute(".agents/root/_root.md", "Root route", "Root");
        workspace.WriteRoute(".agents/workspace-defined/_workspace-defined.md", "Workspace-defined route", "Workspace");
        return workspace;
    }

    private static RouteListDepth ReadDepth(string depthText)
    {
        if (depthText == "default")
        {
            return RouteListDepth.Default;
        }

        if (depthText == RouteListDefinitions.DepthAll)
        {
            return RouteListDepth.All;
        }

        return RouteListDepth.Bounded(ulong.Parse(depthText, System.Globalization.CultureInfo.InvariantCulture));
    }

    private static Task<RouteListResult> RunAsync(RouteListRequest request)
    {
        return RouteListOperation.RunAsync(request, TestContext.Current.CancellationToken);
    }

    private static bool TryCreateDirectoryLink(string link, string target)
    {
        try
        {
            Directory.CreateSymbolicLink(link, target);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (PlatformNotSupportedException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private static int IndexOf(IEnumerable<RouteListRow> rows, string id)
    {
        return rows.Select((row, index) => (row, index)).Single(item => item.row.Id == id).index;
    }

    private static bool IsParentBeforeChild(IEnumerable<RouteListRow> rows)
    {
        var positions = rows
            .Select((row, index) => (row.Path, index))
            .ToDictionary(item => item.Path, item => item.index, StringComparer.Ordinal);
        return rows.All(row => row.ParentPath is null
            || !positions.TryGetValue(row.ParentPath, out var parentPosition)
            || parentPosition < positions[row.Path]);
    }

    private static string RowFacts(RouteListRow row)
    {
        return string.Join(
            "|",
            row.Id,
            row.Path,
            row.ParentId,
            row.ParentPath,
            row.AbsoluteDepth,
            row.RelativeDepth,
            row.Kind,
            row.Description,
            row.Tags is null ? "<null>" : string.Join(',', row.Tags),
            row.DirectChildCount,
            string.Join(',', row.Provenance));
    }
}
