using System.Text;
using OpenForge.Cli.Core.Commands.Update.Models.Request;
using OpenForge.Cli.Core.Commands.Update.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Settings.Models.Document;

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
}
