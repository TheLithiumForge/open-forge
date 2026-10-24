using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Runtime;
using OpenForge.Cli.Core.Shell.Interaction.Models;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "install-main-root"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallMainRootSetupIntegrationTests
{
    private const string CustomRoot = "docs/context";
    private const string LoaderBody = "# Authored loader\r\n";
    private enum Question { MainRoot, Preset, Frontmatter }

    [Theory(DisplayName = "First setup asks for the context folder before route selection and keeps interactive Minimal")]
    [InlineData(false), InlineData(true)]
    public async Task FirstSetupOrder(bool explicitRoot)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(FirstSetupOrder), CustomRoot);
        var questions = new List<Question>();
        var interaction = new InstallSetupInteraction(
            (initial, _, _) =>
            {
                questions.Add(Question.Preset);
                Assert.Equal(InstallPreset.Minimal, initial);
                return ValueTask.FromResult(CliPromptReply<InstallPreset>.Answered(initial));
            },
            (_, _, _) => throw new InvalidOperationException(),
            (question, _, _) =>
            {
                questions.Add(Question.Frontmatter);
                return ValueTask.FromResult(CliPromptReply<string>.Answered(question.InitialForm));
            },
            (initial, _, _) =>
            {
                Assert.False(explicitRoot);
                questions.Add(Question.MainRoot);
                Assert.Equal(WorkspaceLayout.DefaultMainRoot, initial.Initial);
                return ValueTask.FromResult(CliPromptReply<string>.Answered(CustomRoot));
            });
        var request = workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with
        {
            RequestedMainRoot = explicitRoot ? CustomRoot : null,
        };
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(result.Boundary);
        Assert.Equal(CustomRoot, result.RequestedMainRoot);
        Assert.Equal(InstallPreset.Minimal, result.Configuration?.Preset);
        Assert.Equal(explicitRoot ? [Question.Preset, Question.Frontmatter] : new[] { Question.MainRoot, Question.Preset, Question.Frontmatter }, questions);
        Assert.False(File.Exists(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.False(Directory.Exists(workspace.Combine(CustomRoot)));
    }

    [Theory(DisplayName = "Preview automatic and noninteractive setup never asks and keeps unattended Full Core")]
    [InlineData(true, false), InlineData(false, true), InlineData(false, false)]
    public async Task UnattendedSetup(bool preview, bool automatic)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(UnattendedSetup), CustomRoot);
        var interaction = new InstallSetupInteraction(
            (_, _, _) => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException());
        var request = workspace.Request(preview ? InstallMode.DryRun : InstallMode.Apply, automatic: automatic) with { RequestedMainRoot = CustomRoot };
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(result.Boundary);
        Assert.Equal(CustomRoot, result.RequestedMainRoot);
        Assert.Equal(InstallPreset.FullCore, result.Recap?.Preset);
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
    }

    [Theory(DisplayName = "An established context folder refuses a different root before setup and any effect")]
    [InlineData(false), InlineData(true)]
    public async Task EstablishedRefusal(bool configure)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(EstablishedRefusal));
        var loader = WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName;
        workspace.WriteText(loader, LoaderBody);
        var before = File.ReadAllBytes(workspace.Combine(loader));
        var interaction = new InstallSetupInteraction(
            (_, _, _) => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(), (_, _, _) => throw new InvalidOperationException());
        var request = workspace.Request(automatic: false, allowsInteractiveConfirmation: true) with
        {
            RequestedMainRoot = CustomRoot,
            Setup = new(configure, null, []),
        };
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.Equal(InstallFindingCode.InvalidInput, result.Boundary?.Code);
        Assert.Equal(before, File.ReadAllBytes(workspace.Combine(loader)));
        Assert.False(Directory.Exists(WorkspaceRuntimePaths.DirectoryPath(workspace.Workspace)));
        Assert.False(Directory.Exists(workspace.Combine(CustomRoot)));
    }

    [Theory(DisplayName = "A cancelled root answer stops setup and an unavailable terminal keeps unattended defaults")]
    [InlineData(true), InlineData(false)]
    public async Task NonAnswer(bool cancelled)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(NonAnswer));
        var interaction = new InstallSetupInteraction(
            (_, _, _) => ValueTask.FromResult(CliPromptReply<InstallPreset>.Unavailable()), (_, _, _) => throw new InvalidOperationException(),
            (_, _, _) => throw new InvalidOperationException(),
            (_, _, _) => ValueTask.FromResult(cancelled ? CliPromptReply<string>.Cancelled() : CliPromptReply<string>.Unavailable()));
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(
            workspace.Request(automatic: false, allowsInteractiveConfirmation: true), TestContext.Current.CancellationToken);
        Assert.Equal(cancelled, result.Cancelled);
        Assert.Null(result.RequestedMainRoot);
        if (!cancelled) Assert.Equal(InstallPreset.FullCore, result.Recap?.Preset);
        Assert.False(File.Exists(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
    }
}
