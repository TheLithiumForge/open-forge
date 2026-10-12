using OpenForge.Cli.IntegrationTests.Commands.Route.Shared.References;
using OpenForge.Cli.IntegrationTests.Hosting;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove;

public sealed class RouteRemoveGitIgnoreIntegrationTests
{
    [Theory(DisplayName = "Route and root Remove exclude ignored folders while detaching ignored agents references"),
     InlineData(false, ".agents"), InlineData(true, ".agents"), InlineData(false, ".AGENTS"), InlineData(false, ".AgEnTs"),
     Trait("Feature", "route-remove"), Trait("Evidence", "Integration"), Trait("Boundary", "Host")]
    public static async Task IgnoredFoldersAreExcludedThroughBothCommands(bool rootRemove, string physicalRoot)
    {
        if (physicalRoot != ".agents" && !OperatingSystem.IsWindows())
            Assert.Skip("Mixed-case agents identity requires a case-insensitive Windows filesystem.");
        using var workspace = RouteRemoveIntegrationWorkspace.Create("remove-git-ignored");
        using var repository = new RouteReferenceGitWorkspace(workspace.Path);
        using var target = TemporaryWorkspace.Create("remove-ignored-link-target");
        await repository.InitializeAsync();
        if (physicalRoot != ".agents") repository.RenameAgentsRoot(physicalRoot);
        workspace.WriteText(".gitignore", $"node_modules/\n{physicalRoot}/guidance/topics/\n");
        const string ignored = "[Old guide](../.agents/guidance/old%20guide.md)\n";
        workspace.WriteText("node_modules/ignored.md", ignored);
        workspace.WriteText(RouteRemoveIntegrationWorkspace.CategoryChildPath, "Before [Old guide](../old%20guide.md), after.\n");
        const string overwritePath = ".agents/guidance/topics/child.overwrite.md";
        workspace.WriteText(overwritePath, "Overwrite [Old guide](../old%20guide.md).\n");
        var link = workspace.Combine("node_modules/dependency");
        await repository.CreateDirectoryLinkAsync(link, target.Path);
        try
        {
            string[] arguments = rootRemove
                ? ["remove", RouteRemoveIntegrationWorkspace.LeafId, "--kind", "route", "--automatic", "--detail", "debug"]
                : ["route", "remove", RouteRemoveIntegrationWorkspace.LeafId, "--automatic", "--detail", "debug"];
            var result = await CliHostCapture.RunAsync(arguments, workspace.Path);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(ignored, workspace.ReadText("node_modules/ignored.md"));
            Assert.Equal("Before Old guide, after.\n", workspace.ReadText(RouteRemoveIntegrationWorkspace.CategoryChildPath));
            Assert.Equal("Overwrite Old guide.\n", workspace.ReadText(overwritePath));
            Assert.False(File.Exists(workspace.Combine(RouteRemoveIntegrationWorkspace.LeafPath)));
            if (!rootRemove) Assert.Contains("gitIgnoredPaths=1", result.Error, StringComparison.Ordinal);

            var repeat = await CliHostCapture.RunAsync(arguments, workspace.Path);
            Assert.Equal(0, repeat.ExitCode);
            Assert.Equal(ignored, workspace.ReadText("node_modules/ignored.md"));
        }
        finally
        {
            Directory.Delete(link);
        }
    }
}
