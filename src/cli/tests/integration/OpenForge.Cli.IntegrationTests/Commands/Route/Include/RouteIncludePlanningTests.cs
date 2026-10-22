using System.Text.Json;
using OpenForge.Cli.Core.Commands.Route.Include;
using OpenForge.Cli.Core.Commands.Route.Include.Models.Request;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Models;
using OpenForge.Cli.Core.Commands.Route.Shared.Inclusion.Planning;
using OpenForge.Cli.Core.Framework.Documents.Shared.Applicability;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.Inclusion;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Include;

public sealed class RouteIncludePlanningTests
{
    [Theory(DisplayName = "Include previews leaf rows in text and JSON without changing bytes"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData("text", false)]
    [InlineData("text", true)]
    [InlineData("json", false)]
    [InlineData("json", true)]
    public async Task PreviewLeaf(string format, bool metadata)
    {
        using var workspace = new RouteInclusionWorkspace();
        var before = workspace.Files.SnapshotHashes();
        string[] arguments = ["route", "include", "./guide.md", "--description", "Guide", "--dry-run", "--format", format];
        if (metadata) arguments = [.. arguments, "--tag", "#LoadNow", "--tag", "Docs", "--apply-to", "src/**"];
        var result = await CliHostCapture.RunAsync(arguments, workspace.Files.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        if (format == "json")
        {
            using var json = JsonDocument.Parse(result.Output);
            Assert.Equal("completed", json.RootElement.GetProperty("status").GetString());
            Assert.Equal(metadata ? "- [Guide](../guide.md) - #LoadNow #Docs - applies to \u0060src/**\u0060" : "- [Guide](../guide.md)",
                json.RootElement.GetProperty("data").GetProperty("after").GetString());
            Assert.Equal(2, json.RootElement.GetProperty("effects").GetArrayLength());
        }
        else Assert.Contains("- [Guide](../guide.md)", result.Output, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Include rejects duplicate rows instead of discarding an omitted policy"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task DuplicateRows()
    {
        using var workspace = new RouteInclusionWorkspace("- [One](../guide.md) - #Docs\n- [Two](../guide.md) - #LoadNow");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = "guide.md", Description = "Guide", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains("guide.md", result.Outcome.Cause, StringComparison.Ordinal);
        Assert.Contains("Exclude", result.Outcome.Cause, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Fact(DisplayName = "Include accepts an external Markdown catalogue and encodes its path"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task Catalogue()
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.Files.WriteText("docs/team notes/_team notes.md", "# Team\n\n## Entries\n\n- [Guide](../guide.md)\n");
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = @"./docs\team notes\_team notes.md", Description = "Team", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("- [Team](../docs/team%20notes/_team%20notes.md)", result.Outcome.After);
    }

    [Fact(DisplayName = "Include merges one field and registers hand-written rows without rewriting"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MergeFields()
    {
        const string row = "- [Old guide](../guide.md) - #Docs #LoadNow - applies to \u0060src/**\u0060";
        using var workspace = new RouteInclusionWorkspace(row);
        var request = new RouteInclusionRequest { Workspace = workspace.Workspace, File = "guide.md", Action = RouteInclusionAction.Include, DryRun = true };
        var registered = await new RouteInclusionPlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, registered.Outcome.Status);
        Assert.Equal(row, registered.Outcome.After);
        Assert.Equal(".agents/open-forge.json", Assert.Single(registered.Outcome.Effects).Path);
        var changed = await new RouteInclusionPlanBuilder().BuildAsync(request with { Description = "New guide" }, TestContext.Current.CancellationToken);
        Assert.Equal("- [New guide](../guide.md) - #Docs #LoadNow - applies to \u0060src/**\u0060", changed.Outcome.After);
        var tags = await new RouteInclusionPlanBuilder().BuildAsync(request with { Tags = ["Review"] }, TestContext.Current.CancellationToken);
        Assert.Equal("- [Old guide](../guide.md) - #Review - applies to \u0060src/**\u0060", tags.Outcome.After);
        var conditions = await new RouteInclusionPlanBuilder().BuildAsync(request with { ApplyTo = ApplyToPatternExpressionParser.Parse("docs/**").Patterns }, TestContext.Current.CancellationToken);
        Assert.Equal("- [Old guide](../guide.md) - #Docs #LoadNow - applies to \u0060docs/**\u0060", conditions.Outcome.After);
    }

    [Fact(DisplayName = "Identical Include is a verified no-op"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task IdenticalRequest()
    {
        using var workspace = new RouteInclusionWorkspace("- [Guide](../guide.md) - #Docs");
        workspace.Files.ReplaceText(".agents/open-forge.json", "{\"roots\":[\"guide.md\"]}\n");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = "guide.md", Description = "Guide", Tags = ["Docs"] }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.NoOp);
        Assert.True(result.Outcome.Verified);
        Assert.Empty(result.Outcome.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Include rejects unsupported operands with a named cause and no writes"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData("docs", "docs/_docs.md")]
    [InlineData("empty", "empty/_empty.md")]
    [InlineData("missing.md", "Create the file")]
    [InlineData("*.md", "workspace-relative")]
    [InlineData("https://example.invalid/a.md", "URL")]
    [InlineData("guide.md#part", "fragment")]
    [InlineData("../outside.md", "inside this workspace")]
    [InlineData(".git/config", "Git metadata")]
    [InlineData(".agents/loader.md", "control file")]
    [InlineData(".agents/open-forge.json", "control file")]
    [InlineData(".agents/open-forge.lock.json", "control file")]
    [InlineData("binary.txt", "UTF-8")]
    [InlineData(".agents/patterns/deep/note.md", ".agents/patterns/_patterns.md")]
    [InlineData(".agents/direct.md", "not routed")]
    public async Task Rejections(string file, string cause)
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.Files.WriteText("docs/_docs.md", "# Docs\n\n## Entries\n\n- none - No entries - #Empty\n");
        workspace.Files.CreateDirectory("empty");
        workspace.Files.WriteText(".git/config", "git metadata\n");
        workspace.Files.WriteText(".agents/open-forge.lock.json", "{}\n");
        workspace.Files.WriteBytes("binary.txt", [0xff, 0xfe, 0xfd]);
        workspace.AddRoot();
        workspace.Files.WriteText(".agents/patterns/deep/note.md", "# Note\n");
        workspace.Files.WriteText(".agents/direct.md", "# Direct\n");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = file, Description = "Guide", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains(file, result.Outcome.Cause, StringComparison.Ordinal);
        Assert.Contains(cause, result.Outcome.Cause, StringComparison.Ordinal);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
    }

    [Theory(DisplayName = "Include points main-root metadata options to catalogue frontmatter"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData("description")]
    [InlineData("tag")]
    [InlineData("apply-to")]
    public async Task MainRootMetadata(string option)
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.AddRoot();
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = ".agents/patterns/_patterns.md", DryRun = true };
        request = option switch
        {
            "description" => request with { Description = "Patterns" },
            "tag" => request with { Tags = ["Pattern"] },
            _ => request with { ApplyTo = ApplyToPatternExpressionParser.Parse("src/**").Patterns },
        };
        var result = await new RouteIncludeOperation().ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains("frontmatter", result.Outcome.Cause, StringComparison.Ordinal);
        Assert.Contains(request.File, result.Outcome.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Include requires a description for a new external row"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MissingDescription()
    {
        using var workspace = new RouteInclusionWorkspace();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = "guide.md", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains("--description", result.Outcome.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Include rejects a target that escapes through a link"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task LinkedEscape()
    {
        using var workspace = new RouteInclusionWorkspace();
        using var outside = TemporaryWorkspace.Create("route-include-external");
        outside.WriteText("note.md", "outside\n");
        try { workspace.Files.CreateDirectorySymbolicLink("escape", outside.Path); }
        catch (UnauthorizedAccessException) { Assert.Skip("Directory symbolic links are unavailable."); }
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = "escape/note.md", Description = "Escape", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        Assert.Contains("links stay inside", result.Outcome.Cause, StringComparison.Ordinal);
    }

    [Fact(DisplayName = "Include turns on a main-root catalogue without a roots registration"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MainRootMembership()
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.AddRoot();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = ".agents/patterns/_patterns.md", DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal("- [Patterns](patterns/_patterns.md) - #Pattern", result.Outcome.After);
        Assert.False(result.Outcome.RegisteredAfter);
        Assert.Single(result.Outcome.Effects);
    }
}
