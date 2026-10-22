using System.Text.Json;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Sources.Reading;
using OpenForge.Cli.Core.Framework.Sources.Shared.Includes;
using OpenForge.Cli.Core.Framework.Workspace.Models;
using OpenForge.Cli.Core.Commands.Route.List;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Application;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Filesystem;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Selection;
using OpenForge.Cli.Core.Commands.Route.List.Shared.Topology;
using OpenForge.Cli.Core.Framework.Sources.Routing;
using OpenForge.Cli.Core.Shell.Definitions;

namespace OpenForge.Cli.IntegrationTests.Framework.Sources.Shared.Includes;

public sealed class IncludedSourcesIntegrationTests
{
    [Fact(DisplayName = "Included aliases resolve the first file without a missing required read or duplicate source")]
    [Trait("Boundary", "OS"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task IncludedAliasesRetainOneSource()
    {
        using var workspace = CreateWorkspace();
        if (!workspace.TryCreateFileSymbolicLink("alias.md", workspace.Combine("CONTRIBUTING.md"), out _))
        {
            return;
        }

        workspace.ReplaceText(".agents/loader.md", File.ReadAllText(workspace.Combine(".agents/loader.md"))
            + "\n- [Alias](../alias.md) - #LoadNow\n");
        workspace.ReplaceText("docs/team/conditioned.md", File.ReadAllText(workspace.Combine("docs/team/conditioned.md"))
            + "\n[Alias reference](../../alias.md)\n");
        var context = await CliHostCapture.RunAsync(["context", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, context.ExitCode);
        using var document = JsonDocument.Parse(context.Output);
        Assert.Single(document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray(),
            source => source.GetProperty("path").GetString() == "CONTRIBUTING.md");
        foreach (var arguments in new[]
        {
            new[] { "context", "./alias.md", "--for", "src/Feature.cs" },
            new[] { "find", "--include", "./alias.md" },
            new[] { "references", "./alias.md", "--direction", "out" },
            new[] { "route", "list", "./alias.md" },
        })
        {
            var result = await CliHostCapture.RunAsync([.. arguments, "--format", "json"], workspace.Path);
            Assert.True(result.ExitCode == 0, result.Output + result.Error);
        }

        var incoming = await CliHostCapture.RunAsync(["references", "./CONTRIBUTING.md", "--direction", "in", "--format", "json"], workspace.Path);
        Assert.Equal(0, incoming.ExitCode);
        using var incomingDocument = JsonDocument.Parse(incoming.Output);
        Assert.Equal(2, incomingDocument.RootElement.GetProperty("data").GetProperty("incoming").GetArrayLength());
    }

    [Fact(DisplayName = "An included route selection honors cancellation after source reading")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task IncludedRouteSelectionHonorsCancellation()
    {
        using var workspace = CreateWorkspace();
        var request = new RouteListRequest(new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory),
            "./docs/team/_team.md", RouteListDepth.All);
        var resolver = new RouteListSourceResolver(new RouteListInventoryReader(), new RouteListSelectionResolver(new PhysicalPathResolver()), new SourceRouteFactsResolver());
        var source = await resolver.ResolveAsync(request, CancellationToken.None);
        var composer = new RouteListResultComposer(new RouteListTopologySelector(), new RouteListCoverageBuilder(), new RouteListResultBuilder());
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();
        Assert.Equal(CliSemanticStatus.Interrupted, composer.Build(request, source, null, cancellation.Token).Status);
    }

    [Fact(DisplayName = "Included reader keeps the first physical file and refuses lexical and physical escapes")]
    [Trait("Boundary", "OS"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task IncludedReaderKeepsFirstParentAndBoundary()
    {
        using var workspace = CreateWorkspace();
        using var outside = TemporaryWorkspace.Create("included-outside");
        outside.WriteText("outside.md", "# Outside\n");
        var rows = File.ReadAllText(workspace.Combine(".agents/loader.md"))
            + "\n- [Escape](../../outside.md) - #LoadNow\n";
        if (workspace.TryCreateFileSymbolicLink("escape.md", outside.Combine("outside.md"), out _))
        {
            rows += "- [Physical escape](../escape.md) - #LoadNow\n";
        }

        if (workspace.TryCreateFileSymbolicLink("alias.md", workspace.Combine("CONTRIBUTING.md"), out _))
        {
            rows += "- [Alias](../alias.md) - #LoadNow\n";
        }

        workspace.ReplaceText(".agents/loader.md", rows);
        var session = await new SourceReadSessionReader(new PhysicalPathResolver()).ReadAsync(
            new CliWorkspace(workspace.Path, workspace.Path, CliWorkspaceSelectionMethod.CurrentDirectory), CancellationToken.None);
        var included = await new SourceIncludedEntryReader().ReadAsync(session.Catalogue, session.DocumentReader, CancellationToken.None);
        Assert.Equal([".cursor/rules/testing.mdc", "docs/team/_team.md", "CONTRIBUTING.md", "docs/team/conditioned.md"],
            included.Select(source => source.Base.CanonicalPath));
        Assert.Equal("docs/team/_team.md", included[^1].IncludedParentPath);
    }

    [Theory(DisplayName = "Read commands share included files without scanning adjacent folders"), InlineData("text"), InlineData("json")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task ReadCommandsShareIncludes(string format)
    {
        using var workspace = CreateWorkspace();
        var before = workspace.SnapshotHashes();
        var context = await Run("context", "--for", "src/Feature.cs", "--content", "body");
        Assert.Contains("Testing instructions", context, StringComparison.Ordinal);
        Assert.Contains("Conditioned instructions", context, StringComparison.Ordinal);
        Assert.DoesNotContain("Unlisted instructions", context, StringComparison.Ordinal);

        var find = await Run("find", "--tag", "Testing");
        Assert.Contains("./.cursor/rules/testing.mdc", find, StringComparison.Ordinal);
        Assert.Contains("./docs/team/conditioned.md", find, StringComparison.Ordinal);
        Assert.DoesNotContain("unlisted.md", find, StringComparison.Ordinal);

        var references = await Run("references", "./CONTRIBUTING.md", "--direction", "in");
        Assert.Contains("./docs/team/conditioned.md", references, StringComparison.Ordinal);
        Assert.DoesNotContain("testing.mdc", references, StringComparison.Ordinal);
        Assert.DoesNotContain("loader.md", references, StringComparison.Ordinal);

        var routes = await Run("route", "list", "--depth", "all");
        Assert.Contains("./.cursor/rules/testing.mdc", routes, StringComparison.Ordinal);
        Assert.Contains("./docs/team/_team.md", routes, StringComparison.Ordinal);
        Assert.Contains("./off.md", routes, StringComparison.Ordinal);
        Assert.Contains("not in the loader", routes, StringComparison.Ordinal);
        Assert.Contains("missing-target.md", routes, StringComparison.Ordinal);
        Assert.DoesNotContain("unlisted.md", routes, StringComparison.Ordinal);
        Assert.Equal(before, workspace.SnapshotHashes());

        async Task<string> Run(params string[] arguments)
        {
            var result = await CliHostCapture.RunAsync([.. arguments, "--format", format], workspace.Path);
            Assert.True(result.ExitCode is 0 or 3, result.Output + result.Error);
            Assert.Empty(result.Error);
            if (format == "json")
            {
                using var document = JsonDocument.Parse(result.Output);
                Assert.NotEqual("failed", document.RootElement.GetProperty("status").GetString());
            }

            return result.Output;
        }
    }

    [Fact(DisplayName = "Included catalogue filters follow listed children and inherited conditions")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task IncludedCatalogueFiltersUseListedDescendants()
    {
        using var workspace = CreateWorkspace();
        var match = await CliHostCapture.RunAsync(["find", "--include", "./docs/team/_team.md", "--tag", "Testing", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.True(match.ExitCode == 0, match.Output + match.Error);
        Assert.Contains("./docs/team/conditioned.md", match.Output, StringComparison.Ordinal);
        Assert.DoesNotContain("unlisted.md", match.Output, StringComparison.Ordinal);
        var noMatch = await CliHostCapture.RunAsync(["find", "--include", "./docs/team/_team.md", "--tag", "Testing", "--for", "tests/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, noMatch.ExitCode);
        using var document = JsonDocument.Parse(noMatch.Output);
        Assert.Empty(document.RootElement.GetProperty("data").GetProperty("matches").EnumerateArray());
        var context = await CliHostCapture.RunAsync(["context", "./docs/team/_team.md", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, context.ExitCode);
        Assert.Contains("Conditioned instructions", context.Output, StringComparison.Ordinal);
        var routes = await CliHostCapture.RunAsync(["route", "list", "./docs/team/_team.md", "--depth", "all", "--format", "json"], workspace.Path);
        Assert.Equal(0, routes.ExitCode);
        Assert.Contains("./docs/team/conditioned.md", routes.Output, StringComparison.Ordinal);
        var leaf = await CliHostCapture.RunAsync(["route", "list", "./docs/team/conditioned.md", "--detail", "full", "--format", "json"], workspace.Path);
        Assert.Equal(0, leaf.ExitCode);
        using var leafDocument = JsonDocument.Parse(leaf.Output);
        var leafRow = Assert.Single(leafDocument.RootElement.GetProperty("data").GetProperty("rows").EnumerateArray());
        Assert.Equal("./docs/team/_team.md", leafRow.GetProperty("parentId").GetString());
        Assert.Equal(1, leafRow.GetProperty("absoluteDepth").GetInt32());
        Assert.Equal(0, leafRow.GetProperty("relativeDepth").GetInt32());

        workspace.ReplaceText("docs/team/_team.md", File.ReadAllText(workspace.Combine("docs/team/_team.md"))
            + "\n- [Missing child](missing-child.md) - #LoadNow\n");
        var incomplete = await CliHostCapture.RunAsync(["route", "list", "./docs/team/_team.md", "--depth", "all", "--detail", "full", "--format", "json"], workspace.Path);
        Assert.Equal(3, incomplete.ExitCode);
        Assert.Contains("docs/team/missing-child.md", incomplete.Output, StringComparison.Ordinal);
        using var incompleteDocument = JsonDocument.Parse(incomplete.Output);
        Assert.Equal(JsonValueKind.Null, incompleteDocument.RootElement.GetProperty("data").GetProperty("rows")[0].GetProperty("directChildren").ValueKind);
    }

    [Fact(DisplayName = "Explicit main-root catalogue selections retain their included children")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task ExplicitMainCatalogueIncludesChildren()
    {
        using var workspace = CreateWorkspace();
        workspace.ReplaceText(".agents/directives/_directives.md", File.ReadAllText(workspace.Combine(".agents/directives/_directives.md"))
            + "\n- [Extra](../../CONTRIBUTING.md) - #External\n");
        var result = await CliHostCapture.RunAsync(["route", "list", "directives", "--depth", "all", "--format", "json"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Contains("./CONTRIBUTING.md", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "An off root registration does not hide the same file active under another catalogue")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task OffRegistrationAndActiveDescendantStayDistinct()
    {
        using var workspace = CreateWorkspace();
        workspace.ReplaceText(".agents/loader.md", File.ReadAllText(workspace.Combine(".agents/loader.md"))
            .Replace("[Team](../docs/team/_team.md)", "[Docs](../docs/_docs.md)", StringComparison.Ordinal));
        workspace.WriteText("docs/_docs.md", "# Docs\n\n## Entries\n\n- [Team](team/_team.md) - #LoadNow\n");
        var before = workspace.SnapshotHashes();
        var result = await CliHostCapture.RunAsync(["route", "list", "--depth", "all", "--detail", "full", "--format", "json"], workspace.Path);
        Assert.Equal(3, result.ExitCode);
        using var document = JsonDocument.Parse(result.Output);
        var rows = document.RootElement.GetProperty("data").GetProperty("rows").EnumerateArray().ToArray();
        var team = rows.Where(row => row.GetProperty("path").GetString() == "docs/team/_team.md").ToArray();
        Assert.Equal(2, team.Length);
        Assert.Single(team, row => row.TryGetProperty("unlisted", out var unlisted) && unlisted.GetBoolean());
        Assert.Single(team, row => row.GetProperty("parentId").GetString() == "./docs/_docs.md");
        Assert.Single(rows, row => row.GetProperty("path").GetString() == "docs/team/conditioned.md");
        Assert.Equal(before, workspace.SnapshotHashes());
    }

    [Fact(DisplayName = "Context required reads agree with a hand-written loader-only ordered list")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task RequiredReadsFollowLoaderOrder()
    {
        using var workspace = CreateWorkspace();
        var result = await CliHostCapture.RunAsync(["context", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        using var document = JsonDocument.Parse(result.Output);
        var paths = document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray()
            .Select(source => source.GetProperty("path").GetString()).ToArray();
        Assert.Equal([
            "AGENTS.md",
            ".agents/loader.md",
            ".agents/directives/_directives.md",
            ".agents/directives/rule.md",
            ".agents/directives/rule.overwrite.md",
            ".cursor/rules/testing.mdc",
            "docs/team/_team.md",
            "docs/team/conditioned.md",
        ], paths);
        Assert.DoesNotContain(".agents/README.md", paths);
    }

    [Fact(DisplayName = "Including the workspace entry still returns that physical file once")]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task IncludedWorkspaceEntryIsReadOnce()
    {
        using var workspace = CreateWorkspace();
        workspace.ReplaceText(".agents/loader.md", File.ReadAllText(workspace.Combine(".agents/loader.md"))
            + "\n- [Workspace entry](../AGENTS.md) - #LoadNow\n");
        var result = await CliHostCapture.RunAsync(["context", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, result.ExitCode);
        using var document = JsonDocument.Parse(result.Output);
        Assert.Single(document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray(),
            source => source.GetProperty("path").GetString() == "AGENTS.md");
    }

    [Theory(DisplayName = "Context selects exact includes and an unrouted guide file"), InlineData(false), InlineData(true)]
    [Trait("Boundary", "Host"), Trait("Feature", "included-sources"), Trait("Evidence", "Integration")]
    public async Task ExactOperandsAndUnroutedOverwrite(bool overwrite)
    {
        using var workspace = CreateWorkspace();
        if (overwrite)
        {
            workspace.WriteText(".agents/README.overwrite.md", "# Local guide\n");
        }

        var selected = await CliHostCapture.RunAsync(["context", "./CONTRIBUTING.md", "--additions-only", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, selected.ExitCode);
        Assert.Contains("CONTRIBUTING.md", selected.Output, StringComparison.Ordinal);
        Assert.Contains("./CONTRIBUTING.md", selected.Output, StringComparison.Ordinal);
        var unrouted = await CliHostCapture.RunAsync(["context", ".agents/README.md", "--additions-only", "--for", "src/Feature.cs", "--format", "json"], workspace.Path);
        Assert.Equal(0, unrouted.ExitCode);
        using var document = JsonDocument.Parse(unrouted.Output);
        var paths = document.RootElement.GetProperty("data").GetProperty("sources").EnumerateArray()
            .Select(source => source.GetProperty("path").GetString()).ToArray();
        Assert.Equal(overwrite ? [".agents/README.md", ".agents/README.overwrite.md"] : [".agents/README.md"], paths);
        var missing = await CliHostCapture.RunAsync(["context", "./missing.md", "--format", "json"], workspace.Path);
        Assert.Equal(4, missing.ExitCode);
        Assert.Contains("missing.md", missing.Output, StringComparison.Ordinal);
    }

    private static TemporaryWorkspace CreateWorkspace()
    {
        var workspace = TemporaryWorkspace.Create("included-sources");
        workspace.WriteText("AGENTS.md", "# Workspace\n\nRead .agents/loader.md.\n");
        workspace.WriteText(".agents/loader.md", """
            # Loader

            ## Entries

            - [Directives](directives/_directives.md) - #LoadNow
            - [Testing](../.cursor/rules/testing.mdc) - #LoadNow #Testing
            - [Team](../docs/team/_team.md) - #LoadNow - applies to `src/**`
            - [Contributing](../CONTRIBUTING.md) - #Contribution
            - [Missing](../missing-target.md) - #Missing
            """);
        workspace.WriteText(".agents/directives/_directives.md", Metadata("Directives", "LoadNow") + "# Directives\n\n## Entries\n\n- [Rule](rule.md) - #LoadNow\n");
        workspace.WriteText(".agents/directives/rule.md", Metadata("Rule", "LoadNow") + "# Rule\n\n## Instructions\n\nUse tests.\n");
        workspace.WriteText(".agents/directives/rule.overwrite.md", "# Local rule\n\nUse focused tests.\n");
        workspace.WriteText(".agents/README.md", "# Authoring\n\nWrite valid Markdown.\n");
        workspace.WriteText(".agents/open-forge.json", "{\"schemaVersion\":3,\"roots\":[\".cursor/rules/testing.mdc\",\"docs/team/_team.md\",\"off.md\"]}\n");
        workspace.WriteText(".cursor/rules/testing.mdc", "# Testing instructions\n\n[Opaque](../../CONTRIBUTING.md)\n");
        workspace.WriteText(".cursor/rules/unlisted.md", "# Unlisted instructions\n\n#Testing\n");
        workspace.WriteText("docs/team/_team.md", "# Team\n\n## Entries\n\n- [Conditioned](conditioned.md) - #LoadNow #Testing - applies to `**/*.cs`\n");
        workspace.WriteText("docs/team/conditioned.md", "# Conditioned instructions\n\n[Contributing](../../CONTRIBUTING.md)\n");
        workspace.WriteText("docs/team/unlisted.md", "# Unlisted instructions\n\n#Testing\n");
        workspace.WriteText("CONTRIBUTING.md", "# Contributing\n\nUse the test suite.\n");
        workspace.WriteText("off.md", "# Off instructions\n");
        return workspace;
    }

    private static string Metadata(string description, string tags)
        => $"---\nopen-forge:\n  description: {description}\n  tags: [{tags}]\n---\n";
}
