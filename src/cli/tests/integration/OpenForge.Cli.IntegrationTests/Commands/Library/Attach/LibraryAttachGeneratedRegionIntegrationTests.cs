using OpenForge.Cli.Core.Commands.Library.Attach.Models.Result;
using OpenForge.Cli.Core.Commands.Library.Attach;
using OpenForge.Cli.Core.Commands.Library.Detach;
using OpenForge.Cli.Core.Commands.Library.Models.Application;
using OpenForge.Cli.Core.Commands.Library.Models.Request;
using OpenForge.Cli.Core.Commands.Library.Sync;
using OpenForge.Cli.Core.Framework.Ownership;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Ownership.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Libraries.Operational.Models;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Commands.Library.Shared.Mutation;

namespace OpenForge.Cli.IntegrationTests.Commands.Library.Attach;

public sealed class LibraryAttachGeneratedRegionIntegrationTests
{
    private const string HostPath = ".agents/directives/_directives.md";
    private const string AdditionalLeaf = ".agents/directives/guide.md";

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Framework-owned generated host reconciles across Library attach, sync, and detach"), Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    public static async Task FrameworkOwnedGeneratedHostReconcilesAcrossLibraryLifecycle()
    {
        using var workspace = new LibraryMutationWorkspace();
        try
        {
            workspace.ConsumerRoute();
            workspace.RoutedSource();
            workspace.Replace(HostPath, """
                ---
                open-forge:
                  description: Directives
                  tags: [Directive]
                ---
                # Authored prefix

                ## Entries
                - [Stale](stale.md) - #Directive

                ## Notes

                Authored suffix.
                """);
            WriteOwnership(workspace, new FrameworkOwnership(
                new OwnedSource("framework", null),
                [HostPath],
                [new OwnedRegion(HostPath, "entries")]));

            var attach = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, attach.Status);
            Assert.Equal(HostPath, Assert.Single(attach.Result.Plan.GeneratedRegions).Path);
            AssertHostAuthorship(workspace);
            Assert.Contains("review.md", File.ReadAllText(workspace.Absolute(HostPath)), StringComparison.Ordinal);
            Assert.DoesNotContain("stale.md", File.ReadAllText(workspace.Absolute(HostPath)), StringComparison.Ordinal);
            AssertLibraryPaths(workspace, LibraryMutationWorkspace.Leaf);
            AssertFrameworkHostOwnership(ReadOwnership(workspace));
            Assert.NotNull(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);

            workspace.Write($"{LibraryMutationWorkspace.SourceRoot}/{AdditionalLeaf}", """
                ---
                open-forge:
                  description: Guide
                  tags: [Directive]
                ---
                # Guide

                Guide source bytes.
                """);
            var sync = await new LibrarySyncOperation(workspace.Permissions).ExecuteAsync(
                workspace.Sync(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, sync.Status);
            Assert.Equal(HostPath, Assert.Single(sync.Result.Plan.GeneratedRegions).Path);
            AssertHostAuthorship(workspace);
            Assert.Contains("guide.md", File.ReadAllText(workspace.Absolute(HostPath)), StringComparison.Ordinal);
            AssertLibraryPaths(workspace, AdditionalLeaf, LibraryMutationWorkspace.Leaf);
            AssertFrameworkHostOwnership(ReadOwnership(workspace));
            Assert.NotNull(new FileInfo(workspace.Absolute(AdditionalLeaf)).LinkTarget);

            var detach = await new LibraryDetachOperation(workspace.Permissions).ExecuteAsync(
                workspace.Detach(LibraryMode.Apply), TestContext.Current.CancellationToken);

            Assert.Equal(CliSemanticStatus.Complete, detach.Status);
            Assert.Equal(HostPath, Assert.Single(detach.Result.Plan.GeneratedRegions).Path);
            var detachedHost = File.ReadAllText(workspace.Absolute(HostPath));
            AssertHostAuthorship(workspace);
            Assert.DoesNotContain("review.md", detachedHost, StringComparison.Ordinal);
            Assert.DoesNotContain("guide.md", detachedHost, StringComparison.Ordinal);
            AssertLibraryPaths(workspace);
            AssertFrameworkHostOwnership(ReadOwnership(workspace));
            Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
            Assert.Null(new FileInfo(workspace.Absolute(AdditionalLeaf)).LinkTarget);
        }
        finally
        {
            var reviewLinkPath = workspace.Absolute(LibraryMutationWorkspace.Leaf);
            if (new FileInfo(reviewLinkPath).LinkTarget is not null)
            {
                File.Delete(reviewLinkPath);
            }

            var guideLinkPath = workspace.Absolute(AdditionalLeaf);
            if (new FileInfo(guideLinkPath).LinkTarget is not null)
            {
                File.Delete(guideLinkPath);
            }
        }
    }

    [Trait("Boundary", "OS")]
    [Theory(DisplayName = "A mapped leaf owned by another manager blocks Library attach"), Trait("Feature", "library-mutation"), Trait("Evidence", "Integration")]
    [InlineData("framework")]
    [InlineData("extension")]
    public static async Task ActualMappedLeafOwnershipConflictStillBlocksAttach(string owner)
    {
        using var workspace = new LibraryMutationWorkspace();
        workspace.ConsumerRoute();
        workspace.RoutedSource();
        switch (owner)
        {
            case "framework":
                WriteOwnership(workspace, new FrameworkOwnership(
                    new OwnedSource("framework", null),
                    [LibraryMutationWorkspace.Leaf],
                    []));
                break;
            case "extension":
                WriteOwnership(workspace, null, new ExtensionOwnership(
                    "extension",
                    null,
                    "fixture",
                    [],
                    [LibraryMutationWorkspace.Leaf],
                    []));
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(owner), owner, "The ownership fixture is not defined.");
        }

        var before = workspace.Snapshot();
        var result = await new LibraryAttachOperation(workspace.Permissions).ExecuteAsync(
            workspace.Attach(LibraryMode.Apply), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Blocked, result.Status);
        Assert.Contains(result.Result.Findings, finding =>
            finding.Code == LibraryAttachFindingCode.OwnershipConflict
            && finding.Path == LibraryMutationWorkspace.Leaf);
        Assert.Equal(before, workspace.Snapshot());
        Assert.Null(new FileInfo(workspace.Absolute(LibraryMutationWorkspace.Leaf)).LinkTarget);
    }

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
        WriteOwnership(workspace, new FrameworkOwnership(
            new OwnedSource("framework", null),
            [HostPath],
            [new OwnedRegion(HostPath, "entries")]));
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

    private static void WriteOwnership(
        LibraryMutationWorkspace workspace,
        FrameworkOwnership? framework,
        params ExtensionOwnership[] extensions)
    {
        var document = new WorkspaceOwnershipDocument(
            WorkspaceOwnershipDefinitions.SchemaVersion,
            framework,
            [.. extensions],
            []);
        File.WriteAllBytes(
            workspace.Absolute(LibraryMutationWorkspace.RecordPath),
            WorkspaceOwnershipCodec.Write(document));
    }

    private static WorkspaceOwnershipDocument ReadOwnership(LibraryMutationWorkspace workspace)
        => WorkspaceOwnershipCodec.Read(
            File.ReadAllBytes(workspace.Absolute(LibraryMutationWorkspace.RecordPath))).Document
            ?? throw new InvalidOperationException("The ownership fixture did not produce a readable document.");

    private static void AssertLibraryPaths(LibraryMutationWorkspace workspace, params string[] expected)
    {
        var libraries = ReadOwnership(workspace).Libraries;
        if (expected.Length == 0)
        {
            Assert.Empty(libraries);
            return;
        }

        var library = Assert.Single(libraries);
        Assert.Equal(expected, library.Paths);
    }

    private static void AssertFrameworkHostOwnership(WorkspaceOwnershipDocument document)
    {
        var framework = document.Framework
            ?? throw new InvalidOperationException("The Framework ownership fixture was not retained.");
        Assert.Equal("framework", framework.Source.Id);
        Assert.Contains(HostPath, framework.Paths);
        Assert.Contains(framework.Regions, region =>
            region.Path == HostPath && region.Region == "entries");
    }

    private static void AssertHostAuthorship(LibraryMutationWorkspace workspace)
    {
        var host = File.ReadAllText(workspace.Absolute(HostPath));
        Assert.Contains("# Authored prefix\n\n## Entries\n", host, StringComparison.Ordinal);
        Assert.Contains("## Notes\n\nAuthored suffix.", host, StringComparison.Ordinal);
    }
}
