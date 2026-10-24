using OpenForge.Cli.EndToEndTests.Shared.Journeys;
using OpenForge.Cli.EndToEndTests.Shared.Journeys.Models;
using OpenForge.Cli.TestSupport;
using OpenForge.Cli.TestSupport.Isolation;

using Tokens = OpenForge.Cli.TestSupport.Contracts.PublishedTokens;

namespace OpenForge.Cli.EndToEndTests.Journeys;

public sealed partial class WorkspaceRuntimeIsolationJourneyTests
{
    private const string Feature = "workspace-runtime-isolation";
    private const string RemovalPath = "remove-me.md";
    private const string InvalidRoute = "guidance/bad";
    private const string GuidanceTag = "Guidance";

    [Fact(DisplayName = "First Install previews without state then applies and repeats as a verified no-op")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task FirstInstallPreviewApplyAndVerifiedNoOp()
    {
        using var workspace = PublishedJourneyWorkspace.Create("runtime-first-install");
        workspace.ExpectCoreInstall();
        var before = workspace.SnapshotState(includeRuntime: true);
        var preview = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic, Tokens.Install.DryRun));
        Assert.Equal(0, preview.ExitCode);
        Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(TestDataHome.LocalRuntimeDirectory(workspace.Path)));

        var runtime = Directory.CreateDirectory(TestDataHome.LocalRuntimeDirectory(workspace.Path));
        var emptyRuntimePreview = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic, Tokens.Install.DryRun));
        Assert.Equal(0, emptyRuntimePreview.ExitCode);
        Assert.Equal(preview.StandardOutput, emptyRuntimePreview.StandardOutput);
        Assert.Equal(preview.StandardError, emptyRuntimePreview.StandardError);
        Assert.Empty(Directory.EnumerateFileSystemEntries(runtime.FullName));

        var installed = await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic));
        Assert.Equal(0, installed.ExitCode);
        Assert.Equal(Tokens.Runtime.IgnoreContent, File.ReadAllText(TestDataHome.LocalRuntimeIgnorePath(workspace.Path)));
        Assert.Equal(new[] { TestDataHome.LocalRuntimeIgnorePath(workspace.Path), TestDataHome.LocalWorkspaceLockPath(workspace.Path) }.Order(StringComparer.Ordinal),
            Directory.EnumerateFiles(TestDataHome.LocalRuntimeDirectory(workspace.Path)).Order(StringComparer.Ordinal));
        Assert.True(File.Exists(workspace.Combine(Tokens.Sources.LoaderPath)));
        var installedState = workspace.SnapshotState(includeRuntime: true);
        Assert.Equal(0, (await workspace.RunAsync(Tokens.Install.Command, Tokens.Install.Automatic)).ExitCode);
        Assert.Equal(installedState, workspace.SnapshotState(includeRuntime: true));
    }

    [Fact(DisplayName = "Dry runs, prompt-required refusals, verified no-ops and observers create no runtime state")]
    [Trait("Feature", Feature), Trait("Evidence", "EndToEnd")]
    public async Task NonWritingCommandsCreateNothing()
    {
        using var workspace = PublishedJourneyWorkspace.Create("runtime-read-only");
        workspace.ExpectCoreInstall();
        Assert.Equal(0, (await workspace.RunAsync(JourneyFrontmatter.InstallArguments(JourneyFrontmatterForm.Root, Tokens.Install.Automatic))).ExitCode);
        var runtime = TestDataHome.LocalRuntimeDirectory(workspace.Path);
        File.Delete(TestDataHome.LocalWorkspaceLockPath(workspace.Path));
        File.Delete(TestDataHome.LocalRuntimeIgnorePath(workspace.Path));
        Directory.Delete(runtime, recursive: false);
        var before = workspace.SnapshotState(includeRuntime: true);
        string[][] readOnlyCommands =
        [
            [Tokens.Status.Command],
            [Tokens.Doctor.Command],
            [Tokens.Cleanup.Command],
            [Tokens.Cleanup.Command, Tokens.Cleanup.DryRun],
            [Tokens.Index.Command, Tokens.Index.DryRun],
            [Tokens.Index.Command],
            [Tokens.Install.Command, Tokens.Install.Automatic],
        ];
        foreach (var command in readOnlyCommands)
        {
            var result = await workspace.RunAsync(command);
            Assert.Equal(0, result.ExitCode);
            Assert.Equal(before, workspace.SnapshotState(includeRuntime: true));
            Assert.False(Directory.Exists(runtime));
        }

        workspace.WriteText(RemovalPath, OpenForgeDocumentSeed.Metadata(description: "Removal choice", tags: [GuidanceTag], body: "\n# Removal choice\n"));
        var pending = workspace.SnapshotState(includeRuntime: true);
        var refusal = await workspace.RunAsync(Tokens.Remove.Command, RemovalPath);
        Assert.Equal(4, refusal.ExitCode);
        Assert.Equal(pending, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
        var invalid = await workspace.RunAsync(Tokens.Route.Command, Tokens.RouteCreate.Command, InvalidRoute, Tokens.RouteCreate.Tag, GuidanceTag, Tokens.Install.Automatic);
        Assert.NotEqual(0, invalid.ExitCode);
        Assert.Equal(pending, workspace.SnapshotState(includeRuntime: true));
        Assert.False(Directory.Exists(runtime));
    }
}
