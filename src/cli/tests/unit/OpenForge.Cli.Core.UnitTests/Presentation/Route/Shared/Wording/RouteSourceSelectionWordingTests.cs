using OpenForge.Cli.Core.Presentation.Route.Inspect.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Route.Move.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Route.Remove.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Route.Update.Shared.Interaction;
using OpenForge.Cli.Core.Presentation.Shared.Prompts;
using OpenForge.Cli.Core.Shell.Interaction.Models;
using OpenForge.Cli.TestSupport.Interaction;
using OpenForge.Cli.TestSupport.Snapshots;

namespace OpenForge.Cli.Core.UnitTests.Presentation.Route.Shared.Wording;

public sealed class RouteSourceSelectionWordingTests
{
    [Theory(DisplayName = "Route disambiguation names the requested action while preserving path order in both input modes")]
    [InlineData("inspect"), InlineData("move"), InlineData("remove"), InlineData("update")]
    [Trait("Feature", "route-interaction"), Trait("Evidence", "Unit"), Trait("Boundary", "Output")]
    public async Task SourceSelectionScreens(string action)
    {
        var keys = ScriptedCliTerminal.Keys([new(CliKey.Enter)]);
        var lines = ScriptedCliTerminal.Lines(["1"]);
        foreach (var script in new[] { keys, lines })
        {
            var reply = await SelectAsync(new(script.Terminal), action);
            Assert.Equal(".agents/docs/guide.md", reply.Value);
        }
        CommandOutputSnapshot.MatchSnapshot(Assert.Single(keys.Frames), action + ".key.80x24");
        CommandOutputSnapshot.MatchSnapshot(lines.Output.ToString(), action + ".line");
    }

    private static ValueTask<CliPromptReply<string>> SelectAsync(CliPrompts prompts, string action)
    {
        string[] paths = [".agents/docs/guide.md", ".agents/docs/guide.overwrite.md"];
        return action switch
        {
            "inspect" => RouteInspectSourceSelectionPrompt.Create(prompts)(new("docs/guide", paths), new(true), CancellationToken.None),
            "move" => RouteMoveSourceSelectionPrompt.Create(prompts)(new("docs/guide", paths), new(true), CancellationToken.None),
            "remove" => RouteRemoveSourceSelectionPrompt.Create(prompts)(new("docs/guide", paths), new(true), CancellationToken.None),
            "update" => RouteUpdateSourceSelectionPrompt.Create(prompts)(new("docs/guide", paths), new(true), CancellationToken.None),
            _ => throw new ArgumentOutOfRangeException(nameof(action)),
        };
    }
}
