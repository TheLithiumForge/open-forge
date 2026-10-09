using OpenForge.Cli.Core.Presentation.Library.Attach.Shared.Prompts;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Library.Attach.Shared.Prompts;

public sealed class LibraryAttachPromptsTests
{
    [Theory(DisplayName = "Library attach explains both Git-ignore choices and starts with No")]
    [InlineData(false), InlineData(true)]
    [Trait("Feature", "library-git-ignore"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task IgnoreScreens(bool ignore)
    {
        var script = ScriptedCliTerminal.Keys(ignore ? [new(CliKey.Down), new(CliKey.Enter)] : [new(CliKey.Enter)]);
        var reply = await new CliPrompts(script.Terminal).SelectAsync(LibraryAttachIgnoreQuestion.Create(), new(true), CancellationToken.None);
        Assert.Equal(ignore, reply.Value);
        CommandOutputSnapshot.MatchSnapshot(script.Frames[^1], $"ignore.{ignore}.key.80x24");
        var lines = ScriptedCliTerminal.Lines([ignore ? "2" : "1"]);
        var lineReply = await new CliPrompts(lines.Terminal).SelectAsync(LibraryAttachIgnoreQuestion.Create(), new(true), CancellationToken.None);
        Assert.Equal(ignore, lineReply.Value);
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), $"ignore.{ignore}.line");
    }
}
