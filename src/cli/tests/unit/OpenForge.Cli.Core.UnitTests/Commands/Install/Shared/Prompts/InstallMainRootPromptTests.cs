using OpenForge.Cli.Core.Framework.Workspace.Models.Layout;
using OpenForge.Cli.Core.Presentation.Install.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Commands.Install.Shared.Prompts;

[Trait("Feature", "install-main-root"), Trait("Evidence", "Unit"), Trait("Boundary", "Input")]
public sealed class InstallMainRootPromptTests
{
    private const string CustomRoot = "docs/context";
    private const string QuestionSnapshot = "context-folder-question";

    [Fact(DisplayName = "The context folder question explains the default and fixed settings and Skills")]
    public async Task DefaultAnswer()
    {
        var script = ScriptedCliTerminal.Lines([string.Empty]);
        var reply = await new CliPrompts(script.Terminal).TextAsync(InstallSetupQuestions.MainRoot(new(WorkspaceLayout.DefaultMainRoot)), new(true), CancellationToken.None);
        Assert.Equal(CliPromptState.Answered, reply.State);
        Assert.Equal(WorkspaceLayout.DefaultMainRoot, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(script.Output.ToString(), QuestionSnapshot);
    }

    [Fact(DisplayName = "The context folder prompt uses canonical layout validation and accepts nested folders")]
    public void ValidatesFolder()
    {
        var question = InstallSetupQuestions.MainRoot(new(WorkspaceLayout.DefaultMainRoot));
        Assert.Equal(CustomRoot, question.Validate(CustomRoot).Value);
        Assert.False(question.Validate("../outside").IsValid);
        Assert.False(question.Validate(WorkspaceLayout.DefaultMainRoot + "/context").IsValid);
    }

    [Fact(DisplayName = "A disallowed context folder question writes nothing and reads no answer")]
    public async Task SuppressedPrompt()
    {
        var script = ScriptedCliTerminal.Lines([CustomRoot]);
        var reply = await new CliPrompts(script.Terminal).TextAsync(InstallSetupQuestions.MainRoot(new(WorkspaceLayout.DefaultMainRoot)), new(false), CancellationToken.None);
        Assert.Equal(CliPromptState.Unavailable, reply.State);
        Assert.Empty(script.Frames);
        Assert.Empty(script.Output.ToString());
        Assert.Equal(0, script.LineReadCalls);
    }
}
