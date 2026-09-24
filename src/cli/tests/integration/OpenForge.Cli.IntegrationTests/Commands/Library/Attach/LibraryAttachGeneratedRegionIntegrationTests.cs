using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachGeneratedRegionIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Theory, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData(true), InlineData(false)]
    public static async Task OnlyExistingUnambiguousConsumerRegionCanBeProjected(bool validRegion)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries
            """;
        if (!validRegion)
        {
            parent = parent.Replace("## Entries", "## Entries\n\n## Entries", StringComparison.Ordinal);
        }

        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(workspace.Attach(), TestContext.Current.CancellationToken);
        Assert.Equal(validRegion ? CliSemanticStatus.Complete : CliSemanticStatus.Blocked, result.Status);
        if (validRegion)
        {
            Assert.Equal(".agents/directives/_directives.md", Assert.Single(result.Result.Plan.GeneratedRegions).Path);
        }

        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public async Task FollowingAuthoredSectionRemainsOutsidePlannedBody()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        var parentPath = workspace.Absolute(".agents/directives/_directives.md");
        var parent = """
            ---
            open-forge:
              description: Directives
              tags: [Directive]
            ---
            # Authored prefix
            ## Entries

            ## Notes

            Authored suffix.
            """;
        File.WriteAllText(parentPath, parent);
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(),
            TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Single(result.Result.Plan.GeneratedRegions);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(parent, File.ReadAllText(parentPath));
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task ExcludedGeneratedNavigationHostBlocksAttachWithoutEffects()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        workspace.Write(".agents/open-forge.json", "{\"removedFiles\":[\".agents/directives/_directives.md\"]}");
        var hostPath = workspace.Absolute(".agents/directives/_directives.md");
        var hostBytes = File.ReadAllBytes(hostPath);
        var settingsBytes = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));
        var before = workspace.Snapshot();

        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding => finding.Code == LibraryAttachFindingCode.GeneratedNavigationBlocked);
        Assert.Empty(result.Result.Plan.Links);
        Assert.Empty(result.Result.Plan.GeneratedRegions);
        Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Equal(hostBytes, File.ReadAllBytes(hostPath));
        Assert.Equal(settingsBytes, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
        Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
        Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
    }

    [Trait("Boundary", "OS")]
    [Fact, Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task MissingExcludedRequiredNavigationHostBlocksAttachWithoutRecreatingItsAncestors()
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.Write("AGENTS.md", "# Workspace\n\nRead `.agents/loader.md`.\n");
        workspace.Write(".agents/loader.md", "# Loader\n\n## Entries\n- [Directives](directives/_directives.md) - #Directive\n");
        workspace.RoutedSource();
        workspace.Write(".agents/open-forge.json", "{\"removedFiles\":[\".agents/directives/_directives.md\"]}");
        var settingsBytes = File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json"));
        var loaderBytes = File.ReadAllBytes(workspace.Absolute(".agents/loader.md"));
        var before = workspace.Snapshot();

        try
        {
            var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply),
                TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Blocked, result.Status);
            Assert.Empty(result.Result.Plan.Links);
            Assert.Empty(result.Result.Plan.GeneratedRegions);
            Assert.Equal(LibraryApplicationState.NotStarted, result.Result.Application.State);
            Assert.Equal(before, workspace.Snapshot());
            Assert.False(System.IO.Directory.Exists(workspace.Absolute(".agents/directives")));
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.Leaf)));
            Assert.False(File.Exists(workspace.Absolute(LibraryMutationWorkspace.RecordPath)));
            Assert.Equal(settingsBytes, File.ReadAllBytes(workspace.Absolute(".agents/open-forge.json")));
            Assert.Equal(loaderBytes, File.ReadAllBytes(workspace.Absolute(".agents/loader.md")));
        }
        finally
        {
            var linkPath = workspace.Absolute(LibraryMutationWorkspace.Leaf);
            if (new FileInfo(linkPath).LinkTarget is not null || File.Exists(linkPath))
            {
                File.Delete(linkPath);
            }

            var hostPath = workspace.Absolute(".agents/directives/_directives.md");
            if (File.Exists(hostPath))
            {
                File.Delete(hostPath);
            }

            foreach (var relativeDirectory in new[] { ".agents/directives" })
            {
                var directoryPath = workspace.Absolute(relativeDirectory);
                if (System.IO.Directory.Exists(directoryPath)
                    && !System.IO.Directory.EnumerateFileSystemEntries(directoryPath).Any())
                {
                    System.IO.Directory.Delete(directoryPath);
                }
            }

            var ownershipPath = workspace.Absolute(LibraryMutationWorkspace.RecordPath);
            if (File.Exists(ownershipPath))
            {
                File.Delete(ownershipPath);
            }
        }
    }
}
