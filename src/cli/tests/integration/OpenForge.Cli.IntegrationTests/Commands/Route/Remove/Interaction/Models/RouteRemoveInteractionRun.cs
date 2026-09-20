using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Remove.Interaction.Models;

internal sealed record RouteRemoveInteractionRun(
    CliProcessCompletion Completion,
    string StandardOutput,
    string StandardError,
    string TerminalOutput,
    ScriptedCliTerminal Terminal);
