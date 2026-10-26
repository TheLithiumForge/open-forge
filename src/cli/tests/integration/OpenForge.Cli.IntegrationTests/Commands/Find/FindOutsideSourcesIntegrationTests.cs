using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Presentation.Find.Models;
using OpenForge.Cli.Core.Commands.Find;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.TestSupport.Contracts;
using System.Text.Json;
using OpenForge.Cli.Core.Framework.Sources.Identity;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.IntegrationTests.Hosting;

namespace OpenForge.Cli.IntegrationTests.Commands.Find;

[Trait("Boundary", "Host"), Trait("Feature", "find-query"), Trait("Evidence", "Integration")]
public sealed class FindOutsideSourcesIntegrationTests
{
    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(FindOutsideWorkspace.CustomRoot)]
    public async Task RegistrationRetainsRowMetadataConditionsAndAliasIdentity(string mainRoot)
    {
        using var workspace = new FindOutsideWorkspace(mainRoot, alias: true);
        var arguments = new[] { FindDefinitions.Include.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Leaf,
            FindDefinitions.Tag.Name, FindOutsideWorkspace.RowTag, FindDefinitions.For.Name, "src/App.cs" };
        var initial = await Read(workspace, arguments);
        AssertRule(initial);
        workspace.Register();
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var bytes = File.ReadAllBytes(workspace.Files.Combine(FindOutsideWorkspace.Anchor));
        var registered = await Read(workspace, arguments);
        AssertRule(registered);
        Assert.Equal(initial.GetRawText(), registered.GetRawText());
        var alias = await Read(workspace, FindDefinitions.Include.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Alias,
            FindDefinitions.For.Name, "src/App.cs");
        AssertRule(alias);
        var mismatch = await Read(workspace, FindDefinitions.Include.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Leaf,
            FindDefinitions.For.Name, "tests/App.cs");
        Assert.Empty(mismatch.GetProperty(PublishedTokens.FindData.Matches).EnumerateArray());
        Assert.Equal(bytes, File.ReadAllBytes(workspace.Files.Combine(FindOutsideWorkspace.Anchor)));
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    [Theory, InlineData(WorkspaceLayout.DefaultMainRoot), InlineData(FindOutsideWorkspace.CustomRoot)]
    public async Task RegistrationDiscoversUnroutedFilesWithoutLoadingMembership(string mainRoot)
    {
        using var workspace = new FindOutsideWorkspace(mainRoot, listed: false);
        var initial = await Read(workspace);
        Assert.DoesNotContain(FindOutsideWorkspace.Unlisted, Paths(initial));
        workspace.Register();
        var before = workspace.Files.SnapshotHashes(includeRuntime: true);
        var registered = await Read(workspace);
        Assert.Equal(new[] { FindOutsideWorkspace.Anchor, FindOutsideWorkspace.Leaf, FindOutsideWorkspace.Unlisted, workspace.Layout.LoaderPath }
            .Order(StringComparer.Ordinal), Paths(registered).Order(StringComparer.Ordinal));
        var exact = await Read(workspace, FindDefinitions.Include.Name, SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Unlisted);
        Assert.Equal([FindOutsideWorkspace.Unlisted], Paths(exact));
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Unlisted,
            Assert.Single(exact.GetProperty(PublishedTokens.FindData.Matches).EnumerateArray()).GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(before, workspace.Files.SnapshotHashes(includeRuntime: true));
    }

    private static void AssertRule(JsonElement data)
    {
        var rule = Assert.Single(data.GetProperty(PublishedTokens.FindData.Matches).EnumerateArray());
        Assert.Equal(FindOutsideWorkspace.Leaf, rule.GetProperty(PublishedTokens.Json.Path).GetString());
        Assert.Equal(SourceWorkspaceRelativePath.LiteralReferencePrefix + FindOutsideWorkspace.Leaf, rule.GetProperty(PublishedTokens.Json.Id).GetString());
        Assert.Equal(FindOutsideWorkspace.RowDescription, Property(rule, nameof(FindDataMatch.Description)).GetString());
        var applicability = Property(rule, nameof(FindDataMatch.Applicability));
        var conditions = Property(applicability, nameof(FindDataApplicability.Conditions)).EnumerateArray().ToArray();
        Assert.Equal(["src/App.cs"], Property(applicability, nameof(FindDataApplicability.MatchingPaths)).EnumerateArray().Select(path => path.GetString()));
        Assert.Equal(new[] { FindOutsideWorkspace.Anchor, FindOutsideWorkspace.Leaf },
            conditions.Select(condition => Property(condition, nameof(FindDataApplicabilityCondition.Source)).GetString()));
        Assert.Equal(new[] { FindOutsideWorkspace.Condition, FindOutsideWorkspace.ChildCondition },
            conditions.SelectMany(condition => Property(condition, nameof(FindDataApplicabilityCondition.Patterns)).EnumerateArray()).Select(pattern => pattern.GetString()));
    }

    private static JsonElement Property(JsonElement element, string property)
        => element.GetProperty(JsonNamingPolicy.CamelCase.ConvertName(property));

    private static async Task<JsonElement> Read(FindOutsideWorkspace workspace, params string[] arguments)
    {
        var preserved = workspace.Files.SnapshotHashes(includeRuntime: true);
        var result = await CliHostCapture.RunAsync([FindDefinitions.CommandIdentity, .. arguments,
            CliSyntaxDefinitions.Format.Name, CliPresentationDefinitions.JsonFormat,
            CliSyntaxDefinitions.Detail.Name, CliPresentationDefinitions.Full], workspace.Files.Path);
        Assert.True(result.ExitCode == 0, result.Output + result.Error);
        Assert.Empty(result.Error);
        Assert.Equal(preserved, workspace.Files.SnapshotHashes(includeRuntime: true));
        Assert.False(System.IO.Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Layout.Workspace)));
        using var json = JsonDocument.Parse(result.Output);
        return json.RootElement.GetProperty(PublishedTokens.Json.Data).Clone();
    }

    private static string?[] Paths(JsonElement data)
        => data.GetProperty(PublishedTokens.FindData.Matches).EnumerateArray().Select(match => match.GetProperty(PublishedTokens.Json.Path).GetString()).ToArray();
}
