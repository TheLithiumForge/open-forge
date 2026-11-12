using OpenForge.Cli.Composition.Shared.InstallOutsideRoute;
using OpenForge.Cli.Core.Commands.Install;
using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Commands.Install.Models.Request;
using OpenForge.Cli.Core.Commands.Install.Models.Result;
using OpenForge.Cli.Core.Commands.Install.Shared.Configuration;
using OpenForge.Cli.Core.Framework.Filesystem.PhysicalPaths;
using OpenForge.Cli.Core.Framework.Settings;
using OpenForge.Cli.Core.Framework.Settings.Shared.Serialization;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Framework.Workspace.Shared.Paths;
using OpenForge.Cli.Core.Presentation.Install;
using OpenForge.Cli.Core.Presentation.Install.Shared.Selection;
using OpenForge.Cli.Core.Presentation.Shared.Rendering;
using OpenForge.Cli.Core.Shell.Definitions;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.Core.Shell.Pipeline;
using OpenForge.Cli.IntegrationTests.Commands.Install.Shared.Interaction;
using OpenForge.Cli.IntegrationTests.Commands.Shared.Snapshots;

namespace OpenForge.Cli.IntegrationTests.Commands.Install;

[Trait("Feature", "install-outside-route"), Trait("Evidence", "Integration"), Trait("Boundary", "OS")]
public sealed class InstallOutsideRouteSetupIntegrationTests
{
    private const string DocsFolder = "docs/team";
    private enum Question { MainRoot, OutsideRoute, Preset, Frontmatter }

    [Theory(DisplayName = "First setup asks for a docs folder after the context folder and before route choices")]
    [InlineData(true), InlineData(false)]
    public async Task FirstSetupOrder(bool answered)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(FirstSetupOrder));
        workspace.CreateDirectory(DocsFolder);
        var questions = new List<Question>();
        var answer = answered ? DocsFolder : string.Empty;
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), Interaction(questions, answer)).ResolveAsync(
            InteractiveRequest(workspace), TestContext.Current.CancellationToken);
        Assert.Null(result.Boundary);
        Assert.Equal([Question.MainRoot, Question.OutsideRoute, Question.Preset, Question.Frontmatter], questions);
        var expected = answered ? DocsFolder : null;
        Assert.Equal(expected, result.OutsideRoute);
        Assert.Equal(expected, result.Recap?.OutsideRoute);
        Assert.False(File.Exists(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
    }

    [Theory(DisplayName = "A missing overlapping or traversing docs folder stops setup before any plan or effect")]
    [InlineData("docs/missing"), InlineData(WorkspaceLayout.DefaultMainRoot + "/notes"), InlineData("../outside")]
    public async Task RefusedFolder(string answer)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(RefusedFolder));
        var questions = new List<Question>();
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), Interaction(questions, answer)).ResolveAsync(
            InteractiveRequest(workspace), TestContext.Current.CancellationToken);
        Assert.Equal(InstallFindingCode.InvalidInput, result.Boundary?.Code);
        Assert.Equal([Question.MainRoot, Question.OutsideRoute], questions);
        Assert.False(File.Exists(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath)));
        Assert.False(workspace.AgentsDirectoryExists());
    }

    [Theory(DisplayName = "Preview automatic and installed setups never ask for a docs folder")]
    [InlineData(true, false, false), InlineData(false, true, false), InlineData(false, false, true)]
    public async Task NotAsked(bool preview, bool automatic, bool installed)
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(NotAsked));
        if (installed) workspace.WriteText(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName, "# Authored loader\n");
        var questions = new List<Question>();
        var interaction = Interaction(questions, DocsFolder) with
        {
            OutsideRoute = (_, _, _) => throw new InvalidOperationException("The docs folder question is only for a fresh interactive Install."),
        };
        var request = workspace.Request(preview ? InstallMode.DryRun : InstallMode.Apply, automatic: automatic, allowsInteractiveConfirmation: !preview && !automatic);
        var result = await new InstallSetupResolver(new PhysicalPathResolver(), interaction).ResolveAsync(request, TestContext.Current.CancellationToken);
        Assert.Null(result.OutsideRoute);
        Assert.DoesNotContain(Question.OutsideRoute, questions);
    }

    [Fact(DisplayName = "An answered docs folder appears in the plan and is registered after the Framework files")]
    public async Task RegistersAfterInstall()
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(RegistersAfterInstall));
        workspace.CreateDirectory(DocsFolder);
        IReadOnlyList<string> reviewed = [];
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(observe: (preview, _) => reviewed = InstallSetupRecapSelector.Select(preview)),
            workspace.LockStoreRoot,
            Interaction([], DocsFolder),
            InstallOutsideRouteComposer.Create(workspace.LockStoreRoot));
        var result = await operation.ExecuteAsync(InteractiveRequest(workspace), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Contains(reviewed, line => line.Contains(DocsFolder, StringComparison.Ordinal));
        var outcome = Assert.IsType<InstallOutsideRouteOutcome>(result.OutsideRoute);
        Assert.Equal(InstallOutsideRouteState.Registered, outcome.State);
        var catalogue = WorkspaceRoutePath.CanonicalEntrypointPath(DocsFolder);
        Assert.Equal(catalogue, outcome.Catalogue);
        Assert.True(File.Exists(workspace.Combine(catalogue)));
        Assert.Equal([DocsFolder], Settings(workspace).OutsideRoutes);
        Receipt().MatchDetails(result, "docs-folder-registered", testName: nameof(RegistersAfterInstall));
        // The temporary workspace removes only entries it created, and the Install payload cleanup does not know
        // about the registered folder's catalogue.
        File.Delete(workspace.Combine(catalogue));
    }

    [Fact(DisplayName = "A failed docs folder registration keeps the completed Install and reports the retry")]
    public async Task FailedRegistration()
    {
        using var workspace = InstallOperationWorkspace.Create(nameof(FailedRegistration));
        workspace.CreateDirectory(DocsFolder);
        var operation = InstallOperationFactory.Create(
            InstallInteractionTestSupport.Confirmation(),
            workspace.LockStoreRoot,
            Interaction([], DocsFolder),
            (_, _, _) => throw new IOException("The folder changed after setup."));
        var result = await operation.ExecuteAsync(InteractiveRequest(workspace), TestContext.Current.CancellationToken);

        Assert.Equal(CliSemanticStatus.Complete, result.Status);
        Assert.Equal(InstallOutsideRouteState.Failed, result.OutsideRoute?.State);
        Assert.True(File.Exists(workspace.Combine(WorkspaceLayout.DefaultMainRoot + "/" + WorkspaceLayoutNames.LoaderFileName)));
        Assert.Empty(Settings(workspace).OutsideRoutes);
        Receipt().MatchDetails(result, "docs-folder-failed", testName: nameof(FailedRegistration));
    }

    private static InstallRequest InteractiveRequest(InstallOperationWorkspace workspace)
        => workspace.Request(automatic: false, allowsInteractiveConfirmation: true);

    private static CommandOutputRenderers<InstallResult> Receipt()
        => new(request => CliRenderingStage.Render(request, InstallPresentation.Rendering));

    private static Core.Framework.Settings.Models.Document.WorkspaceSettingsDocument Settings(InstallOperationWorkspace workspace)
    {
        var document = WorkspaceSettingsCodec.Read(File.ReadAllBytes(workspace.Combine(WorkspaceSettingsDefinitions.RelativePath))).Document;
        Assert.NotNull(document);
        return document;
    }

    private static InstallSetupInteraction Interaction(List<Question> questions, string answer) => new(
        (_, _, _) =>
        {
            questions.Add(Question.Preset);
            return ValueTask.FromResult(CliPromptReply<InstallPreset>.Answered(InstallPreset.Minimal));
        },
        (_, _, _) => throw new InvalidOperationException("Minimal does not ask for Custom routes."),
        (question, _, _) =>
        {
            questions.Add(Question.Frontmatter);
            return ValueTask.FromResult(CliPromptReply<string>.Answered(question.InitialForm));
        },
        (question, _, _) =>
        {
            questions.Add(Question.MainRoot);
            return ValueTask.FromResult(CliPromptReply<string>.Answered(question.Initial));
        })
    {
        OutsideRoute = (_, _, _) =>
        {
            questions.Add(Question.OutsideRoute);
            return ValueTask.FromResult(CliPromptReply<string>.Answered(answer));
        },
    };
}
