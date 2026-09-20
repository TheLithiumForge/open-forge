using System.Text;
using OpenForge.Cli.Core.Commands.Install.Models.Planning;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Shared.Planning;
using OpenForge.Cli.Core.Framework.Distribution;
using OpenForge.Cli.Core.Framework.Distribution.Models;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Mutation.Models.Filesystem.Files;
using OpenForge.Cli.Core.Framework.Sources.Models.Identity;
using OpenForge.Cli.Core.Framework.Sources.Models.Inventory;

namespace OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Planning;

public sealed class InstallIntendedStateBuilderIntegrationTests
{
    [Trait("Boundary", "OS")]
    [Fact(DisplayName = "Install planning retains unselected invalid UTF-8 overwrite observations"), Trait("Feature", "install-command"), Trait("Evidence", "Integration")]
    public async Task RetainsExactOverwriteExpectationWithoutDecodingUnselectedBytes()
    {
        using var workspace = InstallOperationWorkspace.Create("install-unselected-overwrite");
        const string basePath = ".agents/memory/local-note.md";
        const string overwritePath = ".agents/memory/local-note.overwrite.md";
        const string baseText = """
            ---
            open-forge:
              description: Local projection input
              tags: [Memory]
            ---

            # Local projection input
            """;
        byte[] overwriteBytes = [0xFF, 0xFE, 0x00, 0xC3, 0x28];
        workspace.WriteText(basePath, baseText);
        workspace.WriteText(overwritePath, string.Empty);
        File.WriteAllBytes(workspace.Combine(overwritePath), overwriteBytes);
        var before = workspace.SnapshotHashes();
        var payload = Assert.IsType<FrameworkPayload>(EmbeddedFrameworkPayloadReader.Read().Payload);

        var build = await new InstallIntendedStateBuilder(new PhysicalPathResolver()).BuildAsync(
            workspace.Request(InstallMode.DryRun),
            payload,
            TestContext.Current.CancellationToken);

        Assert.Equal(InstallIntendedStateBuildState.Complete, build.State);
        Assert.Null(build.Cause);
        var intended = Assert.IsType<InstallIntendedState>(build.IntendedState);
        Assert.Contains(
            "- [Local projection input](local-note.md) - #Memory",
            Encoding.UTF8.GetString(intended.TargetBytes[".agents/memory/_memory.md"]),
            StringComparison.Ordinal);
        Assert.Equal(
            [basePath, overwritePath],
            intended.ProjectionInputs.Select(input => input.CanonicalLayerPath));
        var overwrite = Assert.Single(
            intended.ProjectionInputs,
            input => input.CanonicalLayerPath == overwritePath);
        Assert.Equal("memory/local-note", overwrite.AutomaticId);
        Assert.Equal(basePath, overwrite.CanonicalBasePath);
        Assert.Equal(SourceDocumentForm.OverwriteCompanion, overwrite.Form);
        Assert.Equal(SourceLayerKind.Overwrite, overwrite.Kind);
        Assert.Equal(FileExpectationKind.File, overwrite.Expectation.Kind);
        Assert.Equal(workspace.Combine(overwritePath), overwrite.Expectation.LogicalPath);
        Assert.Equal(workspace.Combine(overwritePath), overwrite.Expectation.PhysicalPath);
        Assert.Equal(
            "d2b4465a410ab73d19480230dd87c2f5caa50197a14be0d0d726937d6ac6d76d",
            overwrite.Expectation.ContentHash);
        Assert.Equal(overwriteBytes, File.ReadAllBytes(workspace.Combine(overwritePath)));
        Assert.Equal(before, workspace.SnapshotHashes());
    }
}
