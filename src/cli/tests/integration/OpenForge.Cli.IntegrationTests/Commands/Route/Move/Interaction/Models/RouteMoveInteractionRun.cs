using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;
using OpenForge.Cli.TestSupport.Interaction;

namespace OpenForge.Cli.IntegrationTests.Commands.Route.Move.Interaction.Models;

internal sealed record RouteMoveInteractionRun(
    CliProcessCompletion Completion,
    string StandardOutput,
    string StandardError,
    string TerminalOutput,
    ScriptedCliTerminal Terminal);
