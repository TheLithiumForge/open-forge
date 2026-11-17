using System.Text.Json;
using OpenForge.Cli.Core.Shell.Pipeline.Models.Output;

namespace OpenForge.Cli.IntegrationTests.Commands.Tool.Shared.Composition.Models;

internal sealed record ToolCompositionRun(CliProcessCompletion Completion, string Output, string Error, string Prompts)
{
    internal int ExitCode => Completion.ExitCode;
    internal JsonDocument Json() => JsonDocument.Parse(Output);
}
