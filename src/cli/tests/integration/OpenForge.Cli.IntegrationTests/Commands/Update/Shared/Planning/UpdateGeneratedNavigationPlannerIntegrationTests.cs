using System.Text;
using OpenForge.Cli.Core.Commands.Update;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Ownership.Models.Document;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.IntegrationTests.Framework.Ownership;
using OpenForge.Cli.TestSupport;

namespace OpenForge.Cli.IntegrationTests.Commands.Update.Shared.Planning;

public sealed class UpdateGeneratedNavigationPlannerIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update generated planning observes invalid UTF-8 overwrite bytes without selecting them for projection"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task RetainsExactOverwriteSnapshotWithoutDecodingUnselectedBytes()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-unselected-overwrite");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        var basePath = Path.Combine(workspace.PhysicalPath, ".agents", "memory", "local-note.md");
        var overwritePath = Path.Combine(workspace.PhysicalPath, ".agents", "memory", "local-note.overwrite.md");
        const string baseText = """
            ---
            open-forge:
              description: Local projection input
              tags: [Memory]
            ---

            # Local projection input
            """;
        byte[] overwriteBytes = [0xFF, 0xFE, 0x00, 0xC3, 0x28];
        try
        {
            File.WriteAllBytes(basePath, Encoding.UTF8.GetBytes(baseText));
            File.WriteAllBytes(overwritePath, overwriteBytes);
            var before = workspace.SnapshotHashes();
            var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);

            var build = await new UpdateGeneratedNavigationPlanner(new PhysicalPathResolver()).BuildAsync(
                workspace.Request(UpdateMode.DryRun),
                payload,
                WorkspaceOwnershipDocument.Empty,
                WorkspaceSettingsDocument.Empty,
                payload.Assets,
                new HashSet<string>(StringComparer.Ordinal),
                new HashSet<string>(StringComparer.Ordinal),
                TestContext.Current.CancellationToken);

            Assert.Null(build.Finding);
            Assert.Contains(
                "- [Local projection input](local-note.md) - #Memory",
                Encoding.UTF8.GetString(build.TargetBytes[".agents/memory/_memory.md"]),
                StringComparison.Ordinal);
            Assert.Equal(
                [basePath, overwritePath],
                build.ProjectionInputs.Select(input => input.LogicalPath).ToArray());
            var overwrite = Assert.Single(
                build.ProjectionInputs,
                input => input.LogicalPath == overwritePath);
            Assert.Equal(FileExpectationKind.File, overwrite.Kind);
            Assert.Equal(overwritePath, overwrite.PhysicalPath);
            Assert.True(overwrite.HasBytes);
            Assert.Equal(overwriteBytes, overwrite.Bytes.ToArray());
            Assert.Equal(
                "d2b4465a410ab73d19480230dd87c2f5caa50197a14be0d0d726937d6ac6d76d",
                overwrite.ContentHash);
            Assert.Equal(overwriteBytes, File.ReadAllBytes(overwritePath));
            Assert.Equal(before, workspace.SnapshotHashes());
        }
        finally
        {
            workspace.RemoveFile(".agents/memory/local-note.overwrite.md");
            workspace.RemoveFile(".agents/memory/local-note.md");
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update identifies the linked Library projection source"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task LibraryProjectionRefusalIdentifiesTheLinkedSource()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-library-link");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        const string projectedPath = ".agents/directives/library-note.md";
        const string sourcePath = "shared/team/.agents/directives/library-note.md";

        workspace.WriteText(
            sourcePath,
            OpenForgeDocumentSeed.Metadata(
                "Library note",
                ["Directive"],
                "# Library note\n"));

        var linkPath = Path.Combine(
            workspace.PhysicalPath, ".agents", "directives", "library-note.md");
        var sourcePhysicalPath = Path.Combine(
            workspace.PhysicalPath, "shared", "team", ".agents", "directives", "library-note.md");
        var linkDirectory = Path.GetDirectoryName(linkPath)
            ?? throw new InvalidOperationException("The linked projection target requires a parent directory.");

        File.CreateSymbolicLink(
            linkPath,
            Path.GetRelativePath(linkDirectory, sourcePhysicalPath));

        try
        {
            var before = workspace.SnapshotHashes();
            var sourceBefore = File.ReadAllBytes(sourcePhysicalPath);
            var linkBefore = new FileInfo(linkPath).LinkTarget;

            var build = await workspace.BuildAsync(workspace.Request(UpdateMode.DryRun));

            Assert.Null(build.Plan);
            Assert.Equal(CliSemanticStatus.Blocked, build.Preview.Status);
            Assert.Empty(build.Preview.Effects);
            var finding = Assert.Single(build.Preview.Findings);
            Assert.Equal(UpdateFindingCode.TargetUnsafe, finding.Code);
            Assert.Equal(projectedPath, finding.Target);

            Assert.Equal(before, workspace.SnapshotHashes());
            Assert.Equal(sourceBefore, File.ReadAllBytes(sourcePhysicalPath));
            Assert.Equal(linkBefore, new FileInfo(linkPath).LinkTarget);
        }
        finally
        {
            File.Delete(linkPath);
            workspace.RemoveFile(sourcePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update accepts a healthy registered Library projection"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task AcceptsHealthyRegisteredLibraryProjectionForPreviewAndApply()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-library-registered-link");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        const string projectedPath = ".agents/directives/library-note.md";
        const string sourcePath = "shared/team/.agents/directives/library-note.md";
        workspace.WriteText(
            sourcePath,
            OpenForgeDocumentSeed.Metadata(
                "Library note",
                ["Directive"],
                "# Library note\n"));
        OwnershipFixture.Libraries(
            workspace.PhysicalPath,
            new("team", "shared/team", ".", [projectedPath]));

        var linkPath = Path.Combine(
            workspace.PhysicalPath, ".agents", "directives", "library-note.md");
        var sourcePhysicalPath = Path.Combine(
            workspace.PhysicalPath, "shared", "team", ".agents", "directives", "library-note.md");
        var linkDirectory = Path.GetDirectoryName(linkPath)
            ?? throw new InvalidOperationException("The linked projection target requires a parent directory.");
        File.CreateSymbolicLink(
            linkPath,
            RelativeLinkTarget(linkDirectory, sourcePhysicalPath));

        try
        {
            var sourceBefore = File.ReadAllBytes(sourcePhysicalPath);
            var linkBefore = new FileInfo(linkPath).LinkTarget;

            var preview = await workspace.ExecuteAsync(workspace.Request(UpdateMode.DryRun));

            Assert.Equal(CliSemanticStatus.Complete, preview.Status);
            Assert.Empty(preview.Findings);
            Assert.Contains(
                preview.Effects,
                effect => effect.Path == ".agents/directives/_directives.md");
            Assert.Equal(sourceBefore, File.ReadAllBytes(sourcePhysicalPath));
            Assert.Equal(linkBefore, new FileInfo(linkPath).LinkTarget);

            var applied = await workspace.ExecuteAsync(workspace.Request());

            Assert.Equal(CliSemanticStatus.Complete, applied.Status);
            Assert.Empty(applied.Findings);
            Assert.Contains(
                applied.Effects,
                effect => effect.Path == ".agents/directives/_directives.md");
            Assert.Contains(
                "- [Library note](library-note.md) - #Directive",
                workspace.ReadText(".agents/directives/_directives.md"),
                StringComparison.Ordinal);
            Assert.Equal(sourceBefore, File.ReadAllBytes(sourcePhysicalPath));
            Assert.Equal(linkBefore, new FileInfo(linkPath).LinkTarget);
        }
        finally
        {
            File.Delete(linkPath);
            workspace.RemoveFile(sourcePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update rejects a retargeted registered Library projection"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task RetargetedRegisteredLibraryProjectionIsBlockedAtDestination()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-library-retargeted-link");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        const string projectedPath = ".agents/directives/library-note.md";
        const string sourcePath = "shared/team/.agents/directives/library-note.md";
        const string wrongSourcePath = "shared/team/.agents/directives/other-note.md";
        workspace.WriteText(
            sourcePath,
            OpenForgeDocumentSeed.Metadata("Library note", ["Directive"], "# Library note\n"));
        workspace.WriteText(
            wrongSourcePath,
            OpenForgeDocumentSeed.Metadata("Other note", ["Directive"], "# Other note\n"));
        OwnershipFixture.Libraries(
            workspace.PhysicalPath,
            new("team", "shared/team", ".", [projectedPath]));

        var linkPath = Path.Combine(
            workspace.PhysicalPath, ".agents", "directives", "library-note.md");
        var wrongSourcePhysicalPath = Path.Combine(
            workspace.PhysicalPath, "shared", "team", ".agents", "directives", "other-note.md");
        var linkDirectory = Path.GetDirectoryName(linkPath)
            ?? throw new InvalidOperationException("The linked projection target requires a parent directory.");
        File.CreateSymbolicLink(
            linkPath,
            RelativeLinkTarget(linkDirectory, wrongSourcePhysicalPath));

        try
        {
            var build = await workspace.BuildAsync(workspace.Request(UpdateMode.DryRun));

            Assert.Null(build.Plan);
            Assert.Equal(CliSemanticStatus.Blocked, build.Preview.Status);
            var finding = Assert.Single(build.Preview.Findings);
            Assert.Equal(UpdateFindingCode.TargetUnsafe, finding.Code);
            Assert.Equal(projectedPath, finding.Target);
        }
        finally
        {
            File.Delete(linkPath);
            workspace.RemoveFile(sourcePath);
            workspace.RemoveFile(wrongSourcePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update rejects a registered Library projection with a linked source ancestor"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task LinkedLibrarySourceAncestorIsBlockedAtDestination()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-library-linked-ancestor");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);

        const string projectedPath = ".agents/directives/library-note.md";
        const string sourcePath = "shared/team/.agents/directives/library-note.md";
        const string linkedSourcePath = "shared/team-target/.agents/directives/library-note.md";
        workspace.WriteText(
            sourcePath,
            OpenForgeDocumentSeed.Metadata("Library note", ["Directive"], "# Library note\n"));
        workspace.WriteText(
            linkedSourcePath,
            OpenForgeDocumentSeed.Metadata("Library note", ["Directive"], "# Library note\n"));
        OwnershipFixture.Libraries(
            workspace.PhysicalPath,
            new("team", "shared/team", ".", [projectedPath]));

        var sourceAncestor = Path.Combine(workspace.PhysicalPath, "shared", "team", ".agents");
        var sourceAncestorTarget = Path.Combine(workspace.PhysicalPath, "shared", "team-target", ".agents");
        Directory.Delete(sourceAncestor, recursive: true);
        Directory.CreateSymbolicLink(sourceAncestor, sourceAncestorTarget);

        var linkPath = Path.Combine(
            workspace.PhysicalPath, ".agents", "directives", "library-note.md");
        var sourceLexicalPath = Path.Combine(
            workspace.PhysicalPath, "shared", "team", ".agents", "directives", "library-note.md");
        var linkDirectory = Path.GetDirectoryName(linkPath)
            ?? throw new InvalidOperationException("The linked projection target requires a parent directory.");
        File.CreateSymbolicLink(
            linkPath,
            RelativeLinkTarget(linkDirectory, sourceLexicalPath));

        try
        {
            var build = await workspace.BuildAsync(workspace.Request(UpdateMode.DryRun));

            Assert.Null(build.Plan);
            Assert.Equal(CliSemanticStatus.Blocked, build.Preview.Status);
            var finding = Assert.Single(build.Preview.Findings);
            Assert.Equal(UpdateFindingCode.TargetUnsafe, finding.Code);
            Assert.Equal(projectedPath, finding.Target);
        }
        finally
        {
            File.Delete(linkPath);
            workspace.RemoveFile(sourcePath);
            Directory.Delete(sourceAncestor);
            workspace.RemoveFile(linkedSourcePath);
        }
    }

    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Update preserves the Library ownership conflict guard"), Trait("Feature", "update"), Trait("Evidence", "Integration")]
    public async Task RegisteredLibraryDestinationCollidingWithFrameworkTargetIsBlocked()
    {
        using var workspace = UpdateIntegrationWorkspace.Create("update-library-target-conflict");
        await workspace.EstablishTrustedFrameworkAsync(TestContext.Current.CancellationToken);
        const string conflictingPath = ".agents/loader.md";
        OwnershipFixture.Libraries(
            workspace.PhysicalPath,
            new("team", "shared/team", ".", [conflictingPath]));

        var build = await workspace.BuildAsync(workspace.Request(UpdateMode.DryRun));

        Assert.Null(build.Plan);
        Assert.Equal(CliSemanticStatus.Blocked, build.Preview.Status);
        var finding = Assert.Single(build.Preview.Findings);
        Assert.Equal(UpdateFindingCode.OwnershipConflict, finding.Code);
        Assert.Equal(conflictingPath, finding.Target);
    }

    private static string RelativeLinkTarget(string linkDirectory, string targetPath)
        => Path.GetRelativePath(linkDirectory, targetPath)
            .Replace(Path.DirectorySeparatorChar, '/')
            .Replace(Path.AltDirectorySeparatorChar, '/');
}
