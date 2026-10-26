using OpenForge.Cli.TestSupport.Contracts;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.TestSupport.Snapshots;
using OpenForge.Cli.Core.Framework.Settings;
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
    private const string GitConfig = WorkspaceSettingsDefinitions.GitMetadataDirectoryName + "/config";
    private const string DeepNote = SourceLogicalPath.AgentsRoot + "/" + Category + "/deep/note" + SourceFormClassifier.MarkdownExtension;
    private const string DirectNote = SourceLogicalPath.AgentsRoot + "/direct" + SourceFormClassifier.MarkdownExtension;
    private const string GuidePath = "guide.md";
    private const string GuideDescription = "Guide";
    private const string NewGuideDescription = "New guide";
    private const string MissingPath = "missing.md";
    private const string SourcePattern = "src/**";
    private const string DocumentationPattern = "docs/**";
    private const string Category = "patterns";
    private const string CataloguePath = SourceLogicalPath.AgentsRoot + "/" + Category + "/_" + Category + SourceFormClassifier.MarkdownExtension;

    [Theory(DisplayName = "Include previews leaf rows in text and JSON without changing bytes"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData(PublishedTokens.Syntax.TextFormat, false)]
    [InlineData(PublishedTokens.Syntax.TextFormat, true)]
    [InlineData(PublishedTokens.Syntax.JsonFormat, false)]
    [InlineData(PublishedTokens.Syntax.JsonFormat, true)]
    public async Task PreviewLeaf(string format, bool metadata)
    {
        using var workspace = new RouteInclusionWorkspace();
        var before = workspace.Files.SnapshotHashes();
        string[] arguments = [PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, SourceWorkspaceRelativePath.LiteralReferencePrefix + GuidePath, PublishedTokens.RouteInclude.Description, GuideDescription, PublishedTokens.RouteInclude.DryRun, PublishedTokens.Syntax.FormatOption, format];
        if (metadata) arguments = [.. arguments, PublishedTokens.RouteInclude.Tag, "#LoadNow", PublishedTokens.RouteInclude.Tag, "Docs", PublishedTokens.RouteInclude.ApplyTo, SourcePattern];
        var result = await CliHostCapture.RunAsync(arguments, workspace.Files.Path);
        Assert.Equal(0, result.ExitCode);
        Assert.Empty(result.Error);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        CommandOutputSnapshot.MatchSnapshot(result.Output.Replace(workspace.Files.Path, "<workspace>", StringComparison.Ordinal)
            .Replace(JsonEncodedText.Encode(workspace.Files.Path).ToString(), "<workspace>", StringComparison.Ordinal), $"{nameof(PreviewLeaf)}-{$"{format}-{metadata}"}.content", testName: nameof(RouteIncludePlanningTests));
    }

    [Fact(DisplayName = "Include rejects duplicate rows instead of discarding an omitted policy"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task DuplicateRows()
    {
        using var workspace = new RouteInclusionWorkspace("- [One](../guide.md) - #Docs\n- [Two](../guide.md) - #LoadNow");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = GuidePath, Description = GuideDescription, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.Cause), $"{nameof(DuplicateRows)}-{"cause"}.content", testName: nameof(RouteIncludePlanningTests));
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
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.After), $"{nameof(Catalogue)}-{"row"}.content", testName: nameof(RouteIncludePlanningTests));
    }

    [Fact(DisplayName = "Include merges one field and registers hand-written rows without rewriting"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MergeFields()
    {
        const string row = "- [Old guide](../guide.md) - #Docs #LoadNow - applies to \u0060src/**\u0060";
        using var workspace = new RouteInclusionWorkspace(row);
        var request = new RouteInclusionRequest { Workspace = workspace.Workspace, File = GuidePath, Action = RouteInclusionAction.Include, DryRun = true };
        var registered = await new RouteInclusionPlanBuilder().BuildAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, registered.Outcome.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(registered.Outcome.After), $"{nameof(MergeFields)}-{"registered"}.content", testName: nameof(RouteIncludePlanningTests));
        Assert.Equal(WorkspaceSettingsDefinitions.RelativePath, Assert.Single(registered.Outcome.Effects).Path);
        var changed = await new RouteInclusionPlanBuilder().BuildAsync(request with { Description = NewGuideDescription }, TestContext.Current.CancellationToken);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(changed.Outcome.After), $"{nameof(MergeFields)}-{"changed"}.content", testName: nameof(RouteIncludePlanningTests));
        var tags = await new RouteInclusionPlanBuilder().BuildAsync(request with { Tags = ["Review"] }, TestContext.Current.CancellationToken);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(tags.Outcome.After), $"{nameof(MergeFields)}-{"tags"}.content", testName: nameof(RouteIncludePlanningTests));
        var conditions = await new RouteInclusionPlanBuilder().BuildAsync(request with { ApplyTo = ApplyToPatternExpressionParser.Parse(DocumentationPattern).Patterns }, TestContext.Current.CancellationToken);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(conditions.Outcome.After), $"{nameof(MergeFields)}-{"conditions"}.content", testName: nameof(RouteIncludePlanningTests));
    }

    [Fact(DisplayName = "Identical Include is a verified no-op"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task IdenticalRequest()
    {
        using var workspace = new RouteInclusionWorkspace("- [Guide](../guide.md) - #Docs");
        workspace.Files.ReplaceText(WorkspaceSettingsDefinitions.RelativePath, "{\"roots\":[\"guide.md\"]}\n");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = GuidePath, Description = GuideDescription, Tags = ["Docs"] }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.True(result.Outcome.NoOp);
        Assert.True(result.Outcome.Verified);
        Assert.Empty(result.Outcome.Effects);
        Assert.Equal(before, workspace.Files.SnapshotHashes());
        Assert.False(workspace.Locks.InfrastructureExists);
    }

    [Theory(DisplayName = "Include rejects unsupported operands with a named cause and no writes"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    [InlineData("docs")]
    [InlineData("empty")]
    [InlineData(MissingPath)]
    [InlineData("*.md")]
    [InlineData("https://example.invalid/a.md")]
    [InlineData("guide.md#part")]
    [InlineData("../outside.md")]
    [InlineData(GitConfig)]
    [InlineData(SourceLogicalPath.LoaderPath)]
    [InlineData(WorkspaceSettingsDefinitions.RelativePath)]
    [InlineData(WorkspaceOwnershipDefinitions.RelativePath)]
    [InlineData("binary.txt")]
    [InlineData(DeepNote)]
    [InlineData(DirectNote)]
    public async Task Rejections(string file)
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.Files.WriteText("docs/_docs.md", "# Docs\n\n## Entries\n\n- none - No entries - #Empty\n");
        workspace.Files.CreateDirectory("empty");
        workspace.Files.WriteText(GitConfig, "git metadata\n");
        workspace.Files.WriteBytes("binary.txt", [0xff, 0xfe, 0xfd]);
        workspace.AddRoot();
        workspace.Files.WriteText(DeepNote, "# Note\n");
        workspace.Files.WriteText(DirectNote, "# Direct\n");
        var before = workspace.Files.SnapshotHashes();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = file, Description = GuideDescription, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.Cause), $"{nameof(Rejections)}-{new string(file.Select(c => char.IsLetterOrDigit(c) ? c : '-').ToArray())}.content", testName: nameof(RouteIncludePlanningTests));
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
        var request = new RouteIncludeRequest { Workspace = workspace.Workspace, File = CataloguePath, DryRun = true };
        request = option switch
        {
            "description" => request with { Description = "Patterns" },
            "tag" => request with { Tags = ["Pattern"] },
            _ => request with { ApplyTo = ApplyToPatternExpressionParser.Parse(SourcePattern).Patterns },
        };
        var result = await new RouteIncludeOperation().ExecuteAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.Cause), $"{nameof(MainRootMetadata)}-{option}.content", testName: nameof(RouteIncludePlanningTests));
    }

    [Fact(DisplayName = "Include requires a description for a new external row"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MissingDescription()
    {
        using var workspace = new RouteInclusionWorkspace();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = GuidePath, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Invalid, result.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.Cause), $"{nameof(MissingDescription)}-{"cause"}.content", testName: nameof(RouteIncludePlanningTests));
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
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.Cause), $"{nameof(LinkedEscape)}-{"cause"}.content", testName: nameof(RouteIncludePlanningTests));
    }

    [Fact(DisplayName = "Include turns on a main-root catalogue without a roots registration"), Trait("Feature", "route-include"), Trait("Evidence", "Integration")]
    public async Task MainRootMembership()
    {
        using var workspace = new RouteInclusionWorkspace();
        workspace.AddRoot();
        var result = await new RouteIncludeOperation().ExecuteAsync(new RouteIncludeRequest
        { Workspace = workspace.Workspace, File = CataloguePath, DryRun = true }, TestContext.Current.CancellationToken);
        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        CommandOutputSnapshot.MatchSnapshot(Assert.IsType<string>(result.Outcome.After), $"{nameof(MainRootMembership)}-{"row"}.content", testName: nameof(RouteIncludePlanningTests));
        Assert.False(result.Outcome.RegisteredAfter);
        Assert.Single(result.Outcome.Effects);
    }
}
