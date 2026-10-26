using System.Text.Json;
using OpenForge.Cli.EndToEndTests.Shared.PublishedProcess;
using OpenForge.Cli.TestSupport.Contracts;

namespace OpenForge.Cli.EndToEndTests;

public sealed partial class PublishedRouteIncludeProcessTests
{
    private const string OutsideFolder = "project/team";
    private const string OutsideCatalogue = OutsideFolder + "/_team" + PublishedTokens.Layout.MarkdownExtension;

    [Fact(DisplayName = "Published folder Include previews, creates, converges and Exclude keeps the catalogue"), Trait("Feature", "route-include"), Trait("Evidence", "EndToEnd")]
    public async Task FolderRegistrationJourney()
    {
        var target = PublishedExecutableTarget.Discover();
        using var workspace = PublishedExtensionInstallWorkspace.Create();
        async Task<ProcessRunResult> Run(string[] arguments)
            => await PublishedProcessTestSupport.RunAsync(target, workspace.WorkspacePath, arguments, workspace.EnvironmentVariables);
        Assert.Equal(0, (await Run([PublishedTokens.Install.Command, PublishedTokens.Install.Automatic])).ExitCode);
        var folder = Path.Combine(workspace.WorkspacePath, OutsideFolder);
        var catalogue = Path.Combine(workspace.WorkspacePath, OutsideCatalogue);
        Directory.CreateDirectory(folder);
        try
        {
            string[] include = [PublishedTokens.Route.Command, PublishedTokens.RouteInclude.Command, OutsideFolder,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat];
            var preview = await PublishedProcessTestSupport.RunWithoutWritesAsync(target, workspace.WorkspacePath,
                workspace.SnapshotWorkspace, [.. include, PublishedTokens.RouteInclude.DryRun], workspace.EnvironmentVariables);
            Assert.Equal(0, preview.ExitCode);
            using (var json = JsonDocument.Parse(preview.StandardOutput)) CheckFolder(json.RootElement, registeredBefore: false, registeredAfter: true, verified: false);
            Assert.False(File.Exists(Path.Combine(workspace.WorkspacePath, OutsideCatalogue)));
            var applied = await Run(include);
            Assert.Equal(0, applied.ExitCode);
            using (var json = JsonDocument.Parse(applied.StandardOutput)) CheckFolder(json.RootElement, registeredBefore: false, registeredAfter: true, verified: true);
            var bytes = File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, OutsideCatalogue));
            var noOp = await Run(include);
            Assert.Equal(0, noOp.ExitCode);
            using (var json = JsonDocument.Parse(noOp.StandardOutput))
            {
                CheckFolder(json.RootElement, registeredBefore: true, registeredAfter: true, verified: true);
                Assert.Empty(json.RootElement.GetProperty(PublishedTokens.Json.Effects).EnumerateArray());
            }
            var excluded = await Run([PublishedTokens.Route.Command, PublishedTokens.RouteExclude.Command, OutsideFolder,
                PublishedTokens.Syntax.FormatOption, PublishedTokens.Syntax.JsonFormat]);
            Assert.Equal(0, excluded.ExitCode);
            using (var json = JsonDocument.Parse(excluded.StandardOutput)) CheckFolder(json.RootElement, registeredBefore: true, registeredAfter: false, verified: true);
            Assert.Equal(bytes, File.ReadAllBytes(Path.Combine(workspace.WorkspacePath, OutsideCatalogue)));
        }
        finally
        {
            File.Delete(catalogue);
            Directory.Delete(folder);
            Directory.Delete(Path.GetDirectoryName(folder)!);
        }
    }

    private static void CheckFolder(JsonElement root, bool registeredBefore, bool registeredAfter, bool verified)
    {
        Assert.Equal(PublishedTokens.Statuses.Complete, root.GetProperty(PublishedTokens.Json.Status).GetString());
        var data = root.GetProperty(PublishedTokens.Json.Data);
        Assert.Equal(OutsideFolder, data.GetProperty(PublishedTokens.RouteInclusionData.Target).GetString());
        Assert.Equal(PublishedTokens.RouteInclusionData.Folder, data.GetProperty(PublishedTokens.RouteInclusionData.TargetKind).GetString());
        Assert.Equal(OutsideCatalogue, data.GetProperty(PublishedTokens.RouteInclusionData.Catalogue).GetString());
        Assert.Equal(verified, data.GetProperty(PublishedTokens.RouteInclusionData.Verified).GetBoolean());
        Assert.Equal(registeredBefore, data.GetProperty(PublishedTokens.RouteInclusionData.RegisteredBefore).GetBoolean());
        Assert.Equal(registeredAfter, data.GetProperty(PublishedTokens.RouteInclusionData.RegisteredAfter).GetBoolean());
    }
}
