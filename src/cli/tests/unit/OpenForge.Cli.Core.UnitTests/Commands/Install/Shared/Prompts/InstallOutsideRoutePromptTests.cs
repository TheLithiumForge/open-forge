using OpenForge.Cli.Core.Commands.Install.Models.Configuration;
using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Prompts;

[Trait("Feature", "install-outside-route"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class InstallOutsideRoutePromptTests
{
    private const string DocsFolder = "docs/team";
    private const string CustomRoot = "docs/context";
    private const string QuestionSnapshot = "docs-folder-question";

    [Fact(DisplayName = "The docs folder question explains the example and that Enter skips it")]
    public async Task SkipAnswer()
    {
        var script = ScriptedCliTerminal.Lines([string.Empty]);
        var reply = await new CliPrompts(script.Terminal).TextAsync(
            InstallSetupQuestions.OutsideRoute(new(WorkspaceLayout.DefaultMainRoot)), new(true), CancellationToken.None);
        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.True(InstallOutsideRouteQuestion.IsSkip(reply.Value));
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), QuestionSnapshot);
    }

    [Fact(DisplayName = "The docs folder prompt uses the outside-route path rules for the chosen context folder")]
    public void ValidatesFolder()
    {
        var question = InstallSetupQuestions.OutsideRoute(new(WorkspaceLayout.DefaultMainRoot));
        Assert.Equal(DocsFolder, question.Validate(DocsFolder).Value);
        Assert.True(question.Validate(string.Empty).IsValid);
        Assert.False(question.Validate("../outside").IsValid);
        Assert.False(question.Validate(WorkspaceLayout.DefaultMainRoot + "/notes").IsValid);
        Assert.False(question.Validate(DocsFolder + "/").IsValid);
        Assert.False(InstallSetupQuestions.OutsideRoute(new(CustomRoot)).Validate(CustomRoot + "/team").IsValid);
    }

    [Fact(DisplayName = "A disallowed docs folder question writes nothing and reads no answer")]
    public async Task SuppressedPrompt()
    {
        var script = ScriptedCliTerminal.Lines([DocsFolder]);
        var reply = await new CliPrompts(script.Terminal).TextAsync(
            InstallSetupQuestions.OutsideRoute(new(WorkspaceLayout.DefaultMainRoot)), new(false), CancellationToken.None);
        Assert.Equal(CliPromptState.Unavailable, reply.State);
        Assert.Empty(script.Output.ToString());
        Assert.Equal(0, script.LineReadCalls);
    }
}
